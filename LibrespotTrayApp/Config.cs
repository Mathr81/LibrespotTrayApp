using System;
using System.IO;

namespace LibrespotTrayApp
{
    public class Config
    {
        public string DeviceName { get; set; } = "Librespot";
        public string DeviceType { get; set; } = "speaker";
        public string Bitrate { get; set; } = "320";
        public string Cache { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LibrespotTrayApp", "cache").Replace('\\', '/');
        public int InitialVolume { get; set; } = 50;
        public bool EnableVolumeNormalization { get; set; } = false;
        
    }
}