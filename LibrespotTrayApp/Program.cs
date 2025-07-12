using System;
using System.IO;
using System.Windows.Forms;

namespace LibrespotTrayApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Init configuration (DPI etc.)
                ApplicationConfiguration.Initialize();

                // Lancer l'application dans un bloc protégé
                Application.Run(new LibrespotApplicationContext());
            }
            catch (Exception ex)
            {
                // Chemin de secours dans %AppData%
                string fallbackLogDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LibrespotTrayApp");

                Directory.CreateDirectory(fallbackLogDir);

                string logPath = Path.Combine(fallbackLogDir, "fatal_error.log");
                File.WriteAllText(logPath, ex.ToString());

                MessageBox.Show(
                    $"Une erreur fatale est survenue au démarrage.\nDétails dans :\n{logPath}\n\nMessage : {ex.Message}",
                    "Erreur critique",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
