using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.RDP;

/// <summary>
/// Flow.Launcher.Plugin.RDP 
/// </summary>
public class RDP : IPlugin, ISettingProvider
{
    private PluginInitContext _context;
    private Settings _settings;
    private RDPConnections _rdpConnections;
    private RDPConnectionsStore _store;
    private SearchPhraseProvider _searchPhraseProvider;


    /// <summary>
    /// initialize the plugin.
    /// </summary>
    /// <param name="context"></param>
    public void Init(PluginInitContext context)
    {
        _context = context;
        _store = new RDPConnectionsStore(Path.Combine(
            context.CurrentPluginMetadata.PluginDirectory,
            "data",
            "connections.txt"));
        _rdpConnections = _store.Load();
        _searchPhraseProvider = new SearchPhraseProvider();

        // initialize template settings. right now there are no settings.

        #region init settings

        var settingsFolderLocation =
            Path.Combine(
                Directory.GetParent(
                        Directory.GetParent(context.CurrentPluginMetadata.PluginDirectory).FullName)
                    .FullName,
                "Settings", "Plugins", "Flow.Launcher.Plugin.RDP");

        var settingsFileLocation = Path.Combine(settingsFolderLocation, "Settings.json");

        if (!Directory.Exists(settingsFolderLocation))
        {
            Directory.CreateDirectory(settingsFolderLocation);

            _settings = new Settings
            {
                SettingsFileLocation = settingsFileLocation
            };

            _settings.Save();
        }

        #endregion
    }


    /// <summary>
    /// return results for the given query, starting with rpd
    /// </summary>
    /// <param name="query">search query provided by flow-launcher</param>
    /// <returns></returns>
    public List<Result> Query(Query query)
    {
        _context.API.LogInfo("RDP", $"Query: {query}");

        _searchPhraseProvider.Search = query.Search;
        _rdpConnections.Reload(GetRdpConnectionsFromRegistry());

        var connections = _rdpConnections.FindConnections(query.Search);

        var matchedHost = connections.FirstOrDefault(c => c.Connection.Equals(query.Search, StringComparison.InvariantCultureIgnoreCase));

        var results = new List<Result>();

        if (matchedHost.Connection != null)
        {
            results.Add(MapToResult(matchedHost));
            connections = connections.Where(c => !c.Connection.Equals(matchedHost.Connection)).ToList();
        }

        if (query.FirstSearch == "" || query.FirstSearch == null)
        {
            results.Add(CreateDefaultResult());
        }
        else
        {
            results.Remove(CreateDefaultResult());
        }

            results.AddRange(connections.Select(MapToResult));
        
        LogResults(results);

        return results;
    }


    /// <summary>
    /// create the settings pane inside flow-launchers plugins page.
    /// </summary>
    /// <returns></returns>
    public Control CreateSettingPanel()
    {
        return new SettingsUserControl(_settings);
    }

    private static IReadOnlyCollection<string> GetRdpConnectionsFromRegistry()
    {
        var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Terminal Server Client\Servers");
        if (key is null)
        {
            return Array.Empty<string>();
        }

        return key.GetSubKeyNames();
    }

    private Result MapToResult((string connection, int score) item) =>
        new()
        {
            // For some reason SubTitle must be unique otherwise score is not respected
            Title = $"{item.connection}",
            SubTitle = $"Connect to {item.connection} via RDP",
            IcoPath = "screen-mirroring.png",
            Score = item.score,
            Action = c =>
            {
                _rdpConnections.ConnectionWasSelected(item.connection);
                _store.Save(_rdpConnections);

                StartMstsc(item.connection);
                return true;
            }
        };


    private void LogResults(IReadOnlyCollection<Result> results)
    {
        _context.API.LogInfo("RDP", "Results: ");
        foreach (var result in results)
        {
            _context.API.LogInfo("RDP", $"{result.Title} - {result.Score}");
        }
    }

    private Result CreateDefaultResult() =>
        new()
        {
            Title = "RDP",
            SubTitle = "Establish a new RDP connection",
            IcoPath = "screen-mirroring.png",
            Score = 100,
            Action = c =>
            {
                StartMstsc(_searchPhraseProvider.Search);
                return true;
            }
        };

    private static void StartMstsc(string connection)
    {
        if (string.IsNullOrWhiteSpace(connection))
        {
            Process.Start("mstsc");
        }
        else
        {
            Process.Start("mstsc", "/v:" + connection);
        }
    }

    /// <summary>
    /// In order to have a newest search phrase in Result.Action lambda.
    /// Passing unwrapped Search string to Action for some reason doesn't work,
    /// since it keeps the value of first search which is empty string 
    /// </summary>
    private class SearchPhraseProvider
    {
        public string Search { get; set; }
    }
}