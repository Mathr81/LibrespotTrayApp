using System.Diagnostics;

namespace LibrespotTrayApp;

/// <summary>
/// Settings window (modeless). Works on a copy of the settings, exposed by <see cref="Settings"/>;
/// <see cref="Form.DialogResult"/> is <see cref="DialogResult.OK"/> once they are saved.
/// </summary>
internal sealed class SettingsForm : Form
{
    private sealed record Choice<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }

    private const string DefaultDeviceLabel = "Périphérique par défaut de Windows";

    private readonly CancellationTokenSource closing = new();
    private readonly ToolTip toolTip = new() { AutoPopDelay = 15000 };

    private readonly TextBox deviceName = new() { Width = 280, MaxLength = 64 };
    private readonly ComboBox deviceType = NewComboBox();
    private readonly ComboBox bitrate = NewComboBox();
    private readonly ComboBox audioDevice = NewComboBox();
    private readonly CheckBox normalization = NewCheckBox("Normaliser le volume (même niveau sonore pour tous les titres)");
    private readonly CheckBox rememberVolume = NewCheckBox("Reprendre le volume de la dernière session");
    private readonly TrackBar initialVolume = new() { Minimum = 0, Maximum = 100, TickFrequency = 10, SmallChange = 1, LargeChange = 10, Width = 220, AutoSize = false, Height = 32 };
    private readonly Label initialVolumeValue = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly CheckBox autoplay = NewCheckBox("Lecture automatique de titres similaires (autoplay)");
    private readonly CheckBox mediaControls = NewCheckBox("Touches multimédias (casque, clavier) et titre en cours dans Windows");
    private readonly ComboBox priority = NewComboBox();
    private readonly NumericUpDown cacheSize = new() { Minimum = 0, Maximum = 100, Width = 70 };
    private readonly TextBox cacheDir = new() { Width = 240 };
    private readonly Button browseCacheDir = new() { Text = "…", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
    private readonly CheckBox startWithWindows = NewCheckBox("Lancer au démarrage de Windows");
    private readonly CheckBox discovery = NewCheckBox("Visible par les appareils du réseau local (Spotify Connect)");
    private readonly CheckBox restartAfterResume = NewCheckBox("Relancer librespot après une mise en veille");
    private readonly CheckBox verboseLogging = NewCheckBox("Journal détaillé (diagnostic)");
    private readonly Label accountStatus = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Button logOut = new() { Text = "Se déconnecter", AutoSize = true };

    public SettingsForm(AppSettings settings, bool startsWithWindows, Icon icon)
    {
        Settings = settings;

        Text = "Paramètres — Librespot";
        Icon = icon;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var save = new Button { Text = "Enregistrer", AutoSize = true };
        var cancel = new Button { Text = "Annuler", AutoSize = true };
        AcceptButton = save;
        CancelButton = cancel;
        save.Click += OnSave;
        cancel.Click += (_, _) => Close();

        SuspendLayout();
        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        Section(grid, "Appareil Spotify Connect", first: true);
        Row(grid, "Nom :", deviceName);
        Row(grid, "Type :", deviceType);

        Section(grid, "Audio");
        Row(grid, "Qualité :", bitrate);
        Row(grid, "Sortie :", audioDevice);
        Row(grid, null, normalization);
        Row(grid, null, rememberVolume);
        Row(grid, "Volume initial :", Flow(initialVolume, initialVolumeValue));
        Row(grid, null, autoplay);
        Row(grid, null, mediaControls);

        Section(grid, "Performances");
        Row(grid, "Priorité :", priority);
        Row(grid, "Cache audio :", Flow(cacheSize, new Label { Text = "Go maximum (0 = désactivé)", AutoSize = true, Anchor = AnchorStyles.Left }));
        Row(grid, "Dossier du cache :", Flow(cacheDir, browseCacheDir));

        Section(grid, "Système");
        Row(grid, null, startWithWindows);
        Row(grid, null, discovery);
        Row(grid, null, restartAfterResume);
        Row(grid, null, verboseLogging);

        Section(grid, "Compte Spotify");
        Row(grid, null, Flow(accountStatus, logOut));

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 16, 0, 0),
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(save);
        Row(grid, null, buttons);

        Controls.Add(grid);
        ResumeLayout(performLayout: true);

        toolTip.SetToolTip(deviceName, "Nom affiché dans la liste des appareils de Spotify.");
        toolTip.SetToolTip(bitrate, "320 kbit/s est la meilleure qualité proposée par Spotify à librespot.");
        toolTip.SetToolTip(audioDevice, "Avec le périphérique par défaut, la lecture suit le périphérique choisi dans Windows.");
        toolTip.SetToolTip(priority, "« Supérieure à la normale » évite les coupures quand un jeu occupe le processeur, sans le ralentir : librespot consomme très peu.");
        toolTip.SetToolTip(cacheSize, "Les titres déjà écoutés sont relus depuis le disque au lieu d'être retéléchargés.");
        toolTip.SetToolTip(mediaControls, "Lecture/pause, suivant et précédent depuis le casque ou le clavier, et titre/pochette dans l'affichage du volume de Windows.");
        toolTip.SetToolTip(discovery, "Désactivé : seul votre compte voit l'appareil (un peu moins de trafic réseau).");
        toolTip.SetToolTip(restartAfterResume, "Rétablit la connexion à Spotify après la mise en veille du PC.");

        LoadValues(startsWithWindows);

        rememberVolume.CheckedChanged += (_, _) => UpdateVolumeControls();
        initialVolume.ValueChanged += (_, _) => UpdateVolumeControls();
        cacheSize.ValueChanged += (_, _) => UpdateCacheControls();
        browseCacheDir.Click += OnBrowseCacheDir;
        logOut.Click += (_, _) =>
        {
            if (MessageBox.Show(
                    this,
                    "Oublier le compte Spotify enregistré ?\n\nLa page de connexion s'ouvrira pour vous reconnecter (avec ce compte ou un autre).",
                    Text,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) == DialogResult.OK)
            {
                LogOutRequested?.Invoke();
                UpdateAccountStatus(loggedIn: false);
            }
        };
    }

    public AppSettings Settings { get; }

    public bool StartWithWindows => startWithWindows.Checked;

    public event Action? LogOutRequested;

    public void UpdateAccountStatus(bool loggedIn)
    {
        accountStatus.Text = loggedIn ? "Connecté (identifiants enregistrés)" : "Non connecté";
        logOut.Enabled = loggedIn;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadAudioDevicesAsync();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Windows opened from the tray tend to appear behind the active window.
        Activate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        closing.Cancel();
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            closing.Dispose();
            toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadValues(bool startsWithWindows)
    {
        deviceName.Text = Settings.DeviceName;
        Fill(deviceType, AppSettings.DeviceTypes.Select(t => new Choice<string>(t.Value, t.Label)), Settings.DeviceType);
        Fill(bitrate, AppSettings.Bitrates.Select(b => new Choice<string>(b.Value, b.Label)), Settings.Bitrate);
        Fill(priority, AppSettings.Priorities.Select(p => new Choice<ProcessPriorityClass>(p.Value, p.Label)), Settings.ProcessPriority);
        FillAudioDevices([]);
        audioDevice.Enabled = false;

        normalization.Checked = Settings.EnableVolumeNormalization;
        rememberVolume.Checked = Settings.RememberVolume;
        initialVolume.Value = Settings.InitialVolume;
        autoplay.Checked = Settings.EnableAutoplay;
        mediaControls.Checked = Settings.EnableMediaControls;
        cacheSize.Value = Settings.AudioCacheSizeGb;
        cacheDir.Text = Settings.AudioCacheDir;
        startWithWindows.Checked = startsWithWindows;
        discovery.Checked = Settings.EnableDiscovery;
        restartAfterResume.Checked = Settings.RestartAfterResume;
        verboseLogging.Checked = Settings.VerboseLogging;

        UpdateVolumeControls();
        UpdateCacheControls();
        UpdateAccountStatus(LibrespotService.HasCachedCredentials);
    }

    private async Task LoadAudioDevicesAsync()
    {
        try
        {
            IReadOnlyList<string> devices = await AudioDevices.ListAsync(closing.Token);
            if (!IsDisposed)
            {
                FillAudioDevices(devices);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            toolTip.SetToolTip(audioDevice, $"Liste des périphériques indisponible : {ex.Message}");
        }

        if (!IsDisposed)
        {
            audioDevice.Enabled = true;
        }
    }

    private void FillAudioDevices(IReadOnlyList<string> devices)
    {
        string? selected = audioDevice.SelectedItem is Choice<string?> current ? current.Value : Settings.AudioDevice;

        var choices = new List<Choice<string?>>
        {
            new(null, devices.Count > 0 ? $"{DefaultDeviceLabel} ({devices[0]})" : DefaultDeviceLabel),
        };
        choices.AddRange(devices.Select(d => new Choice<string?>(d, d)));
        if (selected != null && !devices.Contains(selected))
        {
            choices.Add(new Choice<string?>(selected, devices.Count > 0 ? $"{selected} (non disponible)" : selected));
        }

        Fill(audioDevice, choices, selected);
    }

    private static void Fill<T>(ComboBox comboBox, IEnumerable<Choice<T>> choices, T selected)
    {
        comboBox.BeginUpdate();
        comboBox.Items.Clear();
        foreach (Choice<T> choice in choices)
        {
            comboBox.Items.Add(choice);
            if (EqualityComparer<T>.Default.Equals(choice.Value, selected))
            {
                comboBox.SelectedItem = choice;
            }
        }

        if (comboBox.SelectedIndex < 0 && comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }

        comboBox.EndUpdate();
    }

    private void UpdateVolumeControls()
    {
        initialVolume.Enabled = !rememberVolume.Checked;
        initialVolumeValue.Enabled = !rememberVolume.Checked;
        initialVolumeValue.Text = $"{initialVolume.Value} %";
    }

    private void UpdateCacheControls()
    {
        cacheDir.Enabled = cacheSize.Value > 0;
        browseCacheDir.Enabled = cacheSize.Value > 0;
    }

    private void OnBrowseCacheDir(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Dossier du cache audio",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(cacheDir.Text) ? cacheDir.Text : AppPaths.DataDir,
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            cacheDir.Text = dialog.SelectedPath;
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        string name = deviceName.Text.Trim();
        if (name.Length == 0)
        {
            Reject(deviceName, "Le nom de l'appareil ne peut pas être vide.");
            return;
        }

        string dir = cacheDir.Text.Trim();
        if (cacheSize.Value > 0)
        {
            try
            {
                dir = Path.GetFullPath(dir);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                Reject(cacheDir, "Le dossier du cache n'est pas valide.");
                return;
            }
        }

        Settings.DeviceName = name;
        Settings.DeviceType = ((Choice<string>)deviceType.SelectedItem!).Value;
        Settings.Bitrate = ((Choice<string>)bitrate.SelectedItem!).Value;
        Settings.AudioDevice = (audioDevice.SelectedItem as Choice<string?>)?.Value;
        Settings.EnableVolumeNormalization = normalization.Checked;
        Settings.RememberVolume = rememberVolume.Checked;
        Settings.InitialVolume = initialVolume.Value;
        Settings.EnableAutoplay = autoplay.Checked;
        Settings.EnableMediaControls = mediaControls.Checked;
        Settings.ProcessPriority = ((Choice<ProcessPriorityClass>)priority.SelectedItem!).Value;
        Settings.AudioCacheSizeGb = (int)cacheSize.Value;
        if (dir.Length > 0)
        {
            Settings.AudioCacheDir = dir;
        }

        Settings.EnableDiscovery = discovery.Checked;
        Settings.RestartAfterResume = restartAfterResume.Checked;
        Settings.VerboseLogging = verboseLogging.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void Reject(Control control, string message)
    {
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
    }

    private static ComboBox NewComboBox() => new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };

    private static CheckBox NewCheckBox(string text) => new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 3, 3, 3) };

    private static void Section(TableLayoutPanel grid, string title, bool first = false)
    {
        var header = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font(Control.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, first ? 0 : 14, 0, 4),
        };
        Row(grid, null, header);
    }

    private static void Row(TableLayoutPanel grid, string? label, Control field)
    {
        int row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (label == null)
        {
            grid.Controls.Add(field, 0, row);
            grid.SetColumnSpan(field, 2);
        }
        else
        {
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 3, 8, 3) }, 0, row);
            grid.Controls.Add(field, 1, row);
        }
    }

    private static FlowLayoutPanel Flow(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = Padding.Empty };
        panel.Controls.AddRange(controls);
        return panel;
    }
}
