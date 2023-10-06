using System;
using System.IO;
using System.Text.Json;

namespace Flow.Launcher.Plugin.RDP {
    /// <summary>
    /// template for settings. right now, there are no settings.
    /// </summary>
    public class Settings {
        internal string SettingsFileLocation;
        internal Action<Settings> OnSettingsChanged { get; set; }

        internal void Save() {
            File.WriteAllText(SettingsFileLocation, JsonSerializer.Serialize(this));
        }
    }
}
