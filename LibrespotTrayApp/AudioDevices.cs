using System.Diagnostics;
using System.Text;

namespace LibrespotTrayApp;

internal static class AudioDevices
{
    /// <summary>
    /// Lists the output devices exactly as librespot names them (the default device first), by
    /// running "librespot --device ?". Only done when the settings window is open.
    /// </summary>
    public static async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(AppPaths.LibrespotExe)
        {
            ArgumentList = { "--device", "?" },
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = AppContext.BaseDirectory,
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("librespot n'a pas pu être lancé.");
        try
        {
            Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken);
            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await stderr.ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return Parse(output);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    /// <summary>
    /// Output format:
    /// <code>
    /// Default Audio Device:
    ///   Speakers (Realtek(R) Audio)
    /// Other Available Audio Devices:
    ///   Headphones (USB Audio)
    /// </code>
    /// </summary>
    internal static List<string> Parse(string output)
    {
        var devices = new List<string>();
        foreach (string rawLine in output.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            // Device names are indented by exactly two spaces; "[unknown name]" entries by three.
            if (line.Length > 2 && line.StartsWith("  ", StringComparison.Ordinal) && line[2] != ' ')
            {
                string name = line[2..];
                if (!devices.Contains(name))
                {
                    devices.Add(name);
                }
            }
        }

        return devices;
    }
}
