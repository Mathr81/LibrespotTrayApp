namespace LibrespotTrayApp
{
    partial class ConfigForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.deviceNameLabel = new System.Windows.Forms.Label();
            this.deviceNameTextBox = new System.Windows.Forms.TextBox();
            this.bitrateLabel = new System.Windows.Forms.Label();
            this.bitrateComboBox = new System.Windows.Forms.ComboBox();
            this.cacheLabel = new System.Windows.Forms.Label();
            this.cacheTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // deviceNameLabel
            // 
            this.deviceNameLabel.AutoSize = true;
            this.deviceNameLabel.Location = new System.Drawing.Point(20, 20);
            this.deviceNameLabel.Name = "deviceNameLabel";
            this.deviceNameLabel.Size = new System.Drawing.Size(87, 13);
            this.deviceNameLabel.TabIndex = 0;
            this.deviceNameLabel.Text = "Nom de l'appareil";
            // 
            // deviceNameTextBox
            // 
            this.deviceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceNameTextBox.Location = new System.Drawing.Point(120, 17);
            this.deviceNameTextBox.Name = "deviceNameTextBox";
            this.deviceNameTextBox.Size = new System.Drawing.Size(250, 20);
            this.deviceNameTextBox.TabIndex = 1;
            // 
            // bitrateLabel
            // 
            this.bitrateLabel.AutoSize = true;
            this.bitrateLabel.Location = new System.Drawing.Point(20, 50);
            this.bitrateLabel.Name = "bitrateLabel";
            this.bitrateLabel.Size = new System.Drawing.Size(37, 13);
            this.bitrateLabel.TabIndex = 2;
            this.bitrateLabel.Text = "Bitrate";
            // 
            // bitrateComboBox
            // 
            this.bitrateComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateComboBox.FormattingEnabled = true;
            this.bitrateComboBox.Items.AddRange(new object[] {
            "96",
            "160",
            "320"});
            this.bitrateComboBox.Location = new System.Drawing.Point(120, 47);
            this.bitrateComboBox.Name = "bitrateComboBox";
            this.bitrateComboBox.Size = new System.Drawing.Size(250, 21);
            this.bitrateComboBox.TabIndex = 3;
            // 
            // cacheLabel
            // 
            this.cacheLabel.AutoSize = true;
            this.cacheLabel.Location = new System.Drawing.Point(20, 80);
            this.cacheLabel.Name = "cacheLabel";
            this.cacheLabel.Size = new System.Drawing.Size(37, 13);
            this.cacheLabel.TabIndex = 4;
            this.cacheLabel.Text = "Cache";
            // 
            // cacheTextBox
            // 
            this.cacheTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cacheTextBox.Location = new System.Drawing.Point(120, 77);
            this.cacheTextBox.Name = "cacheTextBox";
            this.cacheTextBox.Size = new System.Drawing.Size(210, 20);
            this.cacheTextBox.TabIndex = 5;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(336, 75);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(34, 23);
            this.browseButton.TabIndex = 6;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // deviceTypeLabel
            // 
            this.deviceTypeLabel = new System.Windows.Forms.Label();
            this.deviceTypeComboBox = new System.Windows.Forms.ComboBox();
            this.deviceTypeLabel.AutoSize = true;
            this.deviceTypeLabel.Location = new System.Drawing.Point(20, 110);
            this.deviceTypeLabel.Name = "deviceTypeLabel";
            this.deviceTypeLabel.Size = new System.Drawing.Size(70, 13);
            this.deviceTypeLabel.TabIndex = 9;
            this.deviceTypeLabel.Text = "Type d'appareil";
            // 
            // deviceTypeComboBox
            // 
            this.deviceTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceTypeComboBox.FormattingEnabled = true;
            this.deviceTypeComboBox.Items.AddRange(new object[] {
            "computer",
            "tablet",
            "smartphone",
            "speaker",
            "tv",
            "avr",
            "stereo",
            "gameconsole",
            "castaudio",
            "audioplayer",
            "automobile",
            "unknown"});
            this.deviceTypeComboBox.Location = new System.Drawing.Point(120, 107);
            this.deviceTypeComboBox.Name = "deviceTypeComboBox";
            this.deviceTypeComboBox.Size = new System.Drawing.Size(250, 21);
            this.deviceTypeComboBox.TabIndex = 10;
            // 
            // backendLabel
            // 
            this.backendLabel = new System.Windows.Forms.Label();
            this.backendComboBox = new System.Windows.Forms.ComboBox();
            this.backendLabel.AutoSize = true;
            this.backendLabel.Location = new System.Drawing.Point(20, 140);
            this.backendLabel.Name = "backendLabel";
            this.backendLabel.Size = new System.Drawing.Size(47, 13);
            this.backendLabel.TabIndex = 11;
            this.backendLabel.Text = "Backend";
            // 
            // backendComboBox
            // 
            this.backendComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.backendComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backendComboBox.FormattingEnabled = true;
            this.backendComboBox.Items.AddRange(new object[] {
            "pulseaudio",
            "alsa",
            "portaudio",
            "winmm",
            "wasapi",
            "pipe",
            "null"});
            this.backendComboBox.Location = new System.Drawing.Point(120, 137);
            this.backendComboBox.Name = "backendComboBox";
            this.backendComboBox.Size = new System.Drawing.Size(250, 21);
            this.backendComboBox.TabIndex = 12;
            // 
            // initialVolumeLabel
            // 
            this.initialVolumeLabel = new System.Windows.Forms.Label();
            this.initialVolumeTrackBar = new System.Windows.Forms.TrackBar();
            this.initialVolumeLabel.AutoSize = true;
            this.initialVolumeLabel.Location = new System.Drawing.Point(20, 170);
            this.initialVolumeLabel.Name = "initialVolumeLabel";
            this.initialVolumeLabel.Size = new System.Drawing.Size(79, 13);
            this.initialVolumeLabel.TabIndex = 13;
            this.initialVolumeLabel.Text = "Volume Initial";
            // 
            // initialVolumeTrackBar
            // 
            this.initialVolumeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.initialVolumeTrackBar.Location = new System.Drawing.Point(120, 167);
            this.initialVolumeTrackBar.Maximum = 100;
            this.initialVolumeTrackBar.Name = "initialVolumeTrackBar";
            this.initialVolumeTrackBar.Size = new System.Drawing.Size(250, 45);
            this.initialVolumeTrackBar.TabIndex = 14;
            this.initialVolumeTrackBar.TickFrequency = 10;
            this.initialVolumeTrackBar.Scroll += new System.EventHandler(this.InitialVolumeTrackBar_Scroll);
            // 
            // enableVolumeNormalizationCheckBox
            // 
            this.enableVolumeNormalizationCheckBox = new System.Windows.Forms.CheckBox();
            this.enableVolumeNormalizationCheckBox.AutoSize = true;
            this.enableVolumeNormalizationCheckBox.Location = new System.Drawing.Point(20, 210);
            this.enableVolumeNormalizationCheckBox.Name = "enableVolumeNormalizationCheckBox";
            this.enableVolumeNormalizationCheckBox.Size = new System.Drawing.Size(160, 17);
            this.enableVolumeNormalizationCheckBox.TabIndex = 15;
            this.enableVolumeNormalizationCheckBox.Text = "Activer la normalisation du volume";
            this.enableVolumeNormalizationCheckBox.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(214, 260);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 17;
            this.saveButton.Text = "Enregistrer";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(295, 260);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 18;
            this.cancelButton.Text = "Annuler";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.enableVolumeNormalizationCheckBox);
            this.Controls.Add(this.initialVolumeTrackBar);
            this.Controls.Add(this.initialVolumeLabel);
            this.Controls.Add(this.deviceTypeComboBox);
            this.Controls.Add(this.deviceTypeLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.cacheTextBox);
            this.Controls.Add(this.cacheLabel);
            this.Controls.Add(this.bitrateComboBox);
            this.Controls.Add(this.bitrateLabel);
            this.Controls.Add(this.deviceNameTextBox);
            this.Controls.Add(this.deviceNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "ConfigForm";
            this.Text = "Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.initialVolumeTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label deviceNameLabel;
        private System.Windows.Forms.TextBox deviceNameTextBox;
        private System.Windows.Forms.Label bitrateLabel;
        private System.Windows.Forms.ComboBox bitrateComboBox;
        private System.Windows.Forms.Label cacheLabel;
        private System.Windows.Forms.TextBox cacheTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label deviceTypeLabel;
        private System.Windows.Forms.ComboBox deviceTypeComboBox;
        private System.Windows.Forms.Label initialVolumeLabel;
        private System.Windows.Forms.TrackBar initialVolumeTrackBar;
        private System.Windows.Forms.CheckBox enableVolumeNormalizationCheckBox;
        private System.Windows.Forms.Label backendLabel;
        private System.Windows.Forms.ComboBox backendComboBox;
    }
}