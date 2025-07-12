using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace LibrespotTrayApp
{
    public partial class ConfigForm : Form
    {
        private Config config;

        public ConfigForm(Config config, List<string> audioDevices)
        {
            InitializeComponent();
            this.config = config;
            
            // Populate ProcessPriority ComboBox
            processPriorityComboBox.DataSource = Enum.GetValues(typeof(ProcessPriorityClass));

            // Populate AudioDevices ComboBox
            audioDeviceComboBox.Items.AddRange(audioDevices.ToArray());

            LoadConfigIntoForm();
        }

        private void LoadConfigIntoForm()
        {
            deviceNameTextBox.Text = config.DeviceName;
            bitrateComboBox.SelectedItem = config.Bitrate;
            cacheTextBox.Text = config.Cache;
            deviceTypeComboBox.SelectedItem = config.DeviceType;
            initialVolumeTrackBar.Value = config.InitialVolume;
            enableVolumeNormalizationCheckBox.Checked = config.EnableVolumeNormalization;
            enableAutoplayCheckBox.Checked = config.EnableAutoplay;
            processPriorityComboBox.SelectedItem = config.ProcessPriority;
            if (string.IsNullOrEmpty(config.AudioDevice))
            {
                audioDeviceComboBox.SelectedIndex = 0;
            }
            else
            {
                audioDeviceComboBox.SelectedItem = config.AudioDevice;
            }
        }

        

        private void SaveConfigFromForm()
        {
            config.DeviceName = deviceNameTextBox.Text;
            config.Bitrate = bitrateComboBox.SelectedItem?.ToString() ?? "320";
            config.Cache = cacheTextBox.Text;
            config.DeviceType = deviceTypeComboBox.SelectedItem?.ToString() ?? "speaker";
            config.InitialVolume = initialVolumeTrackBar.Value;
            config.EnableVolumeNormalization = enableVolumeNormalizationCheckBox.Checked;
            config.EnableAutoplay = enableAutoplayCheckBox.Checked;
            if (processPriorityComboBox.SelectedItem != null)
            {
                config.ProcessPriority = (ProcessPriorityClass)processPriorityComboBox.SelectedItem;
            }
            if (audioDeviceComboBox.SelectedIndex == 0)
            {
                config.AudioDevice = null;
            }
            else
            {
                config.AudioDevice = audioDeviceComboBox.SelectedItem?.ToString();
            }
        }

        private void InitialVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            initialVolumeLabel.Text = $"Volume Initial: {initialVolumeTrackBar.Value}%";
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveConfigFromForm();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    cacheTextBox.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }
    }
}