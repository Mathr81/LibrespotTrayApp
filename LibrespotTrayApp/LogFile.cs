using System.Text;

namespace LibrespotTrayApp;

/// <summary>
/// Thread-safe log of the tray app and librespot output. The previous session is kept in
/// librespot.previous.log and the file is rotated when it gets too big.
/// </summary>
internal sealed class LogFile : IDisposable
{
    private const long MaxSize = 4 * 1024 * 1024;

    private readonly Lock gate = new();
    private StreamWriter? writer;

    public LogFile()
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        Rotate();
    }

    public void Info(string message) => Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [tray] {message}");

    public void Write(string line)
    {
        lock (gate)
        {
            try
            {
                writer ??= Open();
                writer.WriteLine(line);
                writer.Flush();
                if (writer.BaseStream.Length > MaxSize)
                {
                    Rotate();
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ObjectDisposedException)
            {
                // Logging must never take the app down.
                writer?.Dispose();
                writer = null;
            }
        }
    }

    private void Rotate()
    {
        lock (gate)
        {
            writer?.Dispose();
            writer = null;
            try
            {
                if (File.Exists(AppPaths.LogFile) && new FileInfo(AppPaths.LogFile).Length > 0)
                {
                    File.Move(AppPaths.LogFile, AppPaths.PreviousLogFile, overwrite: true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    private static StreamWriter Open()
    {
        // FileShare.ReadWrite lets the user open the log in an editor while librespot runs.
        var stream = new FileStream(AppPaths.LogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        return new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024);
    }

    public void Dispose()
    {
        lock (gate)
        {
            writer?.Dispose();
            writer = null;
        }
    }
}
