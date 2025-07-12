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
            this.deviceTypeLabel = new System.Windows.Forms.Label();
            this.deviceTypeComboBox = new System.Windows.Forms.ComboBox();
            this.initialVolumeLabel = new System.Windows.Forms.Label();
            this.initialVolumeTrackBar = new System.Windows.Forms.TrackBar();
            this.processPriorityLabel = new System.Windows.Forms.Label();
            this.processPriorityComboBox = new System.Windows.Forms.ComboBox();
            this.enableVolumeNormalizationCheckBox = new System.Windows.Forms.CheckBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.initialVolumeTrackBar)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.deviceNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.deviceNameTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.bitrateLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.bitrateComboBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cacheLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cacheTextBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.browseButton, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.deviceTypeLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.deviceTypeComboBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.initialVolumeLabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.initialVolumeTrackBar, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.processPriorityLabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.processPriorityComboBox, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.enableVolumeNormalizationCheckBox, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.buttonsPanel, 0, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 320);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // deviceNameLabel
            // 
            this.deviceNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deviceNameLabel.AutoSize = true;
            this.deviceNameLabel.Location = new System.Drawing.Point(13, 18);
            this.deviceNameLabel.Name = "deviceNameLabel";
            this.deviceNameLabel.Size = new System.Drawing.Size(101, 13);
            this.deviceNameLabel.TabIndex = 0;
            this.deviceNameLabel.Text = "Nom de l'appareil";
            // 
            // deviceNameTextBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.deviceNameTextBox, 2);
            this.deviceNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceNameTextBox.Location = new System.Drawing.Point(120, 13);
            this.deviceNameTextBox.Name = "deviceNameTextBox";
            this.deviceNameTextBox.Size = new System.Drawing.Size(317, 20);
            this.deviceNameTextBox.TabIndex = 1;
            // 
            // bitrateLabel
            // 
            this.bitrateLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bitrateLabel.AutoSize = true;
            this.bitrateLabel.Location = new System.Drawing.Point(13, 46);
            this.bitrateLabel.Name = "bitrateLabel";
            this.bitrateLabel.Size = new System.Drawing.Size(37, 13);
            this.bitrateLabel.TabIndex = 2;
            this.bitrateLabel.Text = "Bitrate";
            // 
            // bitrateComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.bitrateComboBox, 2);
            this.bitrateComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bitrateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bitrateComboBox.FormattingEnabled = true;
            this.bitrateComboBox.Items.AddRange(new object[] {
            "96",
            "160",
            "320"});
            this.bitrateComboBox.Location = new System.Drawing.Point(120, 42);
            this.bitrateComboBox.Name = "bitrateComboBox";
            this.bitrateComboBox.Size = new System.Drawing.Size(317, 21);
            this.bitrateComboBox.TabIndex = 3;
            // 
            // cacheLabel
            // 
            this.cacheLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cacheLabel.AutoSize = true;
            this.cacheLabel.Location = new System.Drawing.Point(13, 75);
            this.cacheLabel.Name = "cacheLabel";
            this.cacheLabel.Size = new System.Drawing.Size(37, 13);
            this.cacheLabel.TabIndex = 4;
            this.cacheLabel.Text = "Cache";
            // 
            // cacheTextBox
            // 
            this.cacheTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cacheTextBox.Location = new System.Drawing.Point(120, 71);
            this.cacheTextBox.Name = "cacheTextBox";
            this.cacheTextBox.Size = new System.Drawing.Size(236, 20);
            this.cacheTextBox.TabIndex = 5;
            // 
            // browseButton
            // 
            this.browseButton.AutoSize = true;
            this.browseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.browseButton.Location = new System.Drawing.Point(362, 71);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(26, 23);
            this.browseButton.TabIndex = 6;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // deviceTypeLabel
            // 
            this.deviceTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deviceTypeLabel.AutoSize = true;
            this.deviceTypeLabel.Location = new System.Drawing.Point(13, 104);
            this.deviceTypeLabel.Name = "deviceTypeLabel";
            this.deviceTypeLabel.Size = new System.Drawing.Size(90, 13);
            this.deviceTypeLabel.TabIndex = 7;
            this.deviceTypeLabel.Text = "Type d'appareil";
            // 
            // deviceTypeComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.deviceTypeComboBox, 2);
            this.deviceTypeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.deviceTypeComboBox.Location = new System.Drawing.Point(120, 100);
            this.deviceTypeComboBox.Name = "deviceTypeComboBox";
            this.deviceTypeComboBox.Size = new System.Drawing.Size(317, 21);
            this.deviceTypeComboBox.TabIndex = 8;
            // 
            // initialVolumeLabel
            // 
            this.initialVolumeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.initialVolumeLabel.AutoSize = true;
            this.initialVolumeLabel.Location = new System.Drawing.Point(13, 140);
            this.initialVolumeLabel.Name = "initialVolumeLabel";
            this.initialVolumeLabel.Size = new System.Drawing.Size(79, 13);
            this.initialVolumeLabel.TabIndex = 9;
            this.initialVolumeLabel.Text = "Volume Initial";
            // 
            // initialVolumeTrackBar
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.initialVolumeTrackBar, 2);
            this.initialVolumeTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initialVolumeTrackBar.Location = new System.Drawing.Point(120, 129);
            this.initialVolumeTrackBar.Maximum = 100;
            this.initialVolumeTrackBar.Name = "initialVolumeTrackBar";
            this.initialVolumeTrackBar.Size = new System.Drawing.Size(317, 45);
            this.initialVolumeTrackBar.TabIndex = 10;
            this.initialVolumeTrackBar.TickFrequency = 10;
            this.initialVolumeTrackBar.Scroll += new System.EventHandler(this.InitialVolumeTrackBar_Scroll);
            // 
            // processPriorityLabel
            // 
            this.processPriorityLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.processPriorityLabel.AutoSize = true;
            this.processPriorityLabel.Location = new System.Drawing.Point(13, 183);
            this.processPriorityLabel.Name = "processPriorityLabel";
            this.processPriorityLabel.Size = new System.Drawing.Size(99, 13);
            this.processPriorityLabel.TabIndex = 11;
            this.processPriorityLabel.Text = "Priorité du processus";
            // 
            // processPriorityComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.processPriorityComboBox, 2);
            this.processPriorityComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processPriorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processPriorityComboBox.FormattingEnabled = true;
            this.processPriorityComboBox.Location = new System.Drawing.Point(120, 179);
            this.processPriorityComboBox.Name = "processPriorityComboBox";
            this.processPriorityComboBox.Size = new System.Drawing.Size(317, 21);
            this.processPriorityComboBox.TabIndex = 12;
            // 
            // enableVolumeNormalizationCheckBox
            // 
            this.enableVolumeNormalizationCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.enableVolumeNormalizationCheckBox.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.enableVolumeNormalizationCheckBox, 3);
            this.enableVolumeNormalizationCheckBox.Location = new System.Drawing.Point(13, 210);
            this.enableVolumeNormalizationCheckBox.Name = "enableVolumeNormalizationCheckBox";
            this.enableVolumeNormalizationCheckBox.Size = new System.Drawing.Size(182, 17);
            this.enableVolumeNormalizationCheckBox.TabIndex = 13;
            this.enableVolumeNormalizationCheckBox.Text = "Activer la normalisation du volume";
            this.enableVolumeNormalizationCheckBox.UseVisualStyleBackColor = true;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonsPanel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.buttonsPanel, 3);
            this.buttonsPanel.Controls.Add(this.saveButton);
            this.buttonsPanel.Controls.Add(this.cancelButton);
            this.buttonsPanel.Location = new System.Drawing.Point(201, 236);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = new System.Drawing.Size(236, 29);
            this.buttonsPanel.TabIndex = 14;
            // 
            // saveButton
            // 
            this.saveButton.AutoSize = true;
            this.saveButton.Location = new System.Drawing.Point(3, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(112, 23);
            this.saveButton.TabIndex = 15;
            this.saveButton.Text = "Enregistrer";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(121, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(112, 23);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Annuler";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ConfigForm
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(450, 320);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ConfigForm";
            this.Text = "Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.initialVolumeTrackBar)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel buttonsPanel;
        private System.Windows.Forms.Label processPriorityLabel;
        private System.Windows.Forms.ComboBox processPriorityComboBox;
    }
}
        