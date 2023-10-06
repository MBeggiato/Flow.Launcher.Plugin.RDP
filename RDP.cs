using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.RDP {
    /// <summary>
    /// Flow.Launcher.Plugin.RDP 
    /// </summary>
    public class RDP : IPlugin, ISettingProvider {

        internal PluginInitContext Context;
        private Settings settings;


        /// <summary>
        /// initialize the plugin.
        /// </summary>
        /// <param name="context"></param>
        public void Init(PluginInitContext context) {
            Context = context;

            // initialize template settings. right now there are no settings.
            #region init settings
            var settingsFolderLocation =
                Path.Combine(
                    Directory.GetParent(
                        Directory.GetParent(context.CurrentPluginMetadata.PluginDirectory).FullName)
                    .FullName,
                    "Settings", "Plugins", "Flow.Launcher.Plugin.RDP");

            var settingsFileLocation = Path.Combine(settingsFolderLocation, "Settings.json");

            if (!Directory.Exists(settingsFolderLocation)) {
                Directory.CreateDirectory(settingsFolderLocation);

                settings = new Settings {
                    SettingsFileLocation = settingsFileLocation
                };

                settings.Save();
            }
            #endregion
        }


        /// <summary>
        /// return results for the given query, starting with rpd
        /// </summary>
        /// <param name="query">search query provided by flow-launcher</param>
        /// <returns></returns>
        public List<Result> Query(Query query) {
            // initialize a empty result list
            var results = new List<Result>();
            // read the registry to get historical rdp sessions of the current user
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Servers");
            // create the default result for a new rdp connection with no given host
            results.Add(CreateDefaultResult());
            // check if the user is searching for a connection
            // if not, show him all historic rdp connections found in the registry
            if (string.IsNullOrEmpty(query.Search)) {
                foreach (string item in key.GetSubKeyNames()) {
                    results.Add(new Result { Title = item, SubTitle = "Connect to this host via RDP", IcoPath = "screen-mirroring.png" });
                }
            }
            // if the user has typed a search query, show him matching results from the registry
            else {
                List<string> subKeys = key.GetSubKeyNames().ToList<string>().Where(x => x.Contains(query.Search)).ToList<string>();
                // add a result for each matching result
                foreach (var item in subKeys) {
                    results.Add(new Result {
                        Title = item,
                        SubTitle = "Connect to this host via RDP",
                        IcoPath = "screen-mirroring.png",
                        Action = c => {
                            Process.Start("mstsc", "/v:" + item);
                            return true;
                        }
                    });
                }
            }
            // return results to flow-launcher.
            return results;
        }


        /// <summary>
        /// create the settings pane inside flow-launchers plugins page.
        /// </summary>
        /// <returns></returns>
        public Control CreateSettingPanel() {
            return new SettingsUserControl(settings);
        }


        /// <summary>
        /// create the default result for a new rdp connection with no given host
        /// </summary>
        /// <returns>default result</returns>
        private Result CreateDefaultResult() {
            return new Result {
                Title = "RDP",
                SubTitle = "Establish a new RDP connection",
                IcoPath = "screen-mirroring.png",
                Score = 50,
                Action = c => {
                    Process.Start("mstsc");
                    return true;
                }
            };
        }
    }
}