using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LibrespotTrayApp
{
    public class LibrespotApplicationContext : ApplicationContext
    {
        private const string AppName = "LibrespotTrayApp";
        private NotifyIcon? trayIcon;
        private Process? librespotProcess;
        private Config config = null!;
        private List<string> audioDevices = new List<string>();
        private readonly string appPath;
        private string librespotLogFilePath;
        private StreamWriter? librespotLogWriter;

        public LibrespotApplicationContext()
        {
            appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            librespotLogFilePath = Path.Combine(appPath, "librespot.log");

            try
            {
                LoadConfig();

                trayIcon = new NotifyIcon()
                {
                    Icon = new System.Drawing.Icon(Path.Combine(appPath, "app.ico")),
                    ContextMenuStrip = new ContextMenuStrip(),
                    Visible = true
                };

                var startupMenuItem = new ToolStripMenuItem("Démarrer au démarrage", null, ToggleStartup);
                startupMenuItem.Checked = IsStartupEnabled();

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Configuration", null, ShowConfig));
                trayIcon.ContextMenuStrip.Items.Add(startupMenuItem);
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Redémarrer Librespot", null, RestartLibrespot));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Ouvrir le fichier de log", null, OpenLogFile));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Quitter", null, Exit));

                Task.Run(UpdateAudioDevices);
                StartLibrespot();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(appPath, "error.log"), ex.ToString());
                MessageBox.Show($"Une erreur est survenue : {ex.Message}\nConsultez error.log pour plus de détails.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(appPath, "config.json");
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
                catch (JsonException ex)
                {
                    File.WriteAllText(Path.Combine(appPath, "error.log"), ex.ToString());
                    MessageBox.Show($"Le fichier config.json est corrompu. Il sera recréé avec les paramètres par défaut.\nErreur: {ex.Message}", "Erreur de Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    config = new Config(); // Reset to default config
                    SaveConfig(); // Save the default config
                }
            }
            else
            {
                config = new Config();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            string configPath = Path.Combine(appPath, "config.json");
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }

        private async Task UpdateAudioDevices()
        {
            var devices = new List<string>();
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(appPath, "librespot.exe"),
                        Arguments = "--device ?",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var defaultDeviceMatch = Regex.Match(output, @"Default Audio Device:\n  (.+)");
                if (defaultDeviceMatch.Success)
                {
                    devices.Add($"Défaut ({defaultDeviceMatch.Groups[1].Value.Trim()})");
                }

                var otherDevicesMatch = Regex.Match(output, @"Other Available Audio Devices:((\n  .+)+)");
                if (otherDevicesMatch.Success)
                {
                    var otherDevices = Regex.Matches(otherDevicesMatch.Groups[1].Value, @"^  (.+)", RegexOptions.Multiline);
                    foreach (Match match in otherDevices)
                    {
                        devices.Add(match.Groups[1].Value.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting audio devices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            audioDevices = devices;
        }


        void ShowConfig(object? sender, EventArgs e)
        {
            using (var configForm = new ConfigForm(config, audioDevices))
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    SaveConfig();
                    RestartLibrespot(null, EventArgs.Empty);
                }
            }
        }

        void ToggleStartup(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem startupMenuItem)
            {
                startupMenuItem.Checked = !startupMenuItem.Checked;
                SetStartup(startupMenuItem.Checked);
            }
        }

        private bool IsStartupEnabled()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key?.GetValue(AppName) != null;
            }
        }

        private void SetStartup(bool enable)
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (enable)
                {
                    key?.SetValue(AppName, Application.ExecutablePath);
                }
                else
                {
                    key?.DeleteValue(AppName, false);
                }
            }
        }

        void RestartLibrespot(object? sender, EventArgs e)
        {
            if (librespotProcess != null && !librespotProcess.HasExited)
            {
                librespotProcess.Kill();
            }
            librespotLogWriter?.Close();
            librespotLogWriter?.Dispose();
            Task.Run(UpdateAudioDevices);
            StartLibrespot();
        }

        void StartLibrespot()
        {
            var argumentsList = new System.Collections.Generic.List<string>
			{
				$"--name \"{config.DeviceName}\"",
				$"--bitrate {config.Bitrate}",
				$"--cache \"{config.Cache}\"",
				$"--device-type {config.DeviceType}",
				$"--initial-volume {config.InitialVolume}",
				"--enable-oauth",
				"--oauth-port 8067"
			};

            if (config.EnableVolumeNormalization)
            {
                argumentsList.Add("--enable-volume-normalisation");
            }

            if (config.EnableAutoplay)
            {
                argumentsList.Add("--autoplay on");
            }

            if (!string.IsNullOrEmpty(config.AudioDevice))
            {
                argumentsList.Add($"--device \"{config.AudioDevice}\"");
            }

            

            

            string arguments = string.Join(" ", argumentsList);
            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(appPath, "librespot.exe"),
                Arguments = arguments,
                WorkingDirectory = appPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            librespotProcess = new Process();
            librespotProcess.StartInfo = startInfo;
            librespotProcess.OutputDataReceived += (sender, args) => LogLibrespotOutput(args.Data);
            librespotProcess.ErrorDataReceived += (sender, args) => LogLibrespotOutput(args.Data);

            // Clear the log file before starting a new process
            File.WriteAllText(librespotLogFilePath, string.Empty);
            librespotLogWriter = new StreamWriter(librespotLogFilePath, append: true);

            librespotProcess.Start();
            librespotProcess.PriorityClass = config.ProcessPriority;
            librespotProcess.BeginOutputReadLine();
            librespotProcess.BeginErrorReadLine();

            if (librespotProcess == null)
            {
                MessageBox.Show("Impossible de démarrer librespot.exe.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void LogLibrespotOutput(string? data)
        {
            if (data != null)
            {
                librespotLogWriter?.WriteLine(data);
                librespotLogWriter?.Flush();

                // Check if the output contains the Spotify authorization URL
                if (data.Contains("Browse to:") && data.Contains("accounts.spotify.com/authorize"))
                {
                    int startIndex = data.IndexOf("http");
                    if (startIndex != -1)
                    {
                        string authUrl = data.Substring(startIndex).Trim();
                        try
                        {
                            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Impossible d'ouvrir l'URL d'authentification Spotify : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        void Exit(object? sender, EventArgs e)
        {
            if (librespotProcess != null && !librespotProcess.HasExited)
            {
                librespotProcess.Kill();
            }
            librespotLogWriter?.Close();
            librespotLogWriter?.Dispose();
            if (trayIcon != null) trayIcon.Visible = false;
            Application.Exit();
        }

        void OpenLogFile(object? sender, EventArgs e)
        {
            if (File.Exists(librespotLogFilePath))
            {
                Process.Start(new ProcessStartInfo(librespotLogFilePath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Le fichier de log de Librespot n'existe pas encore.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
