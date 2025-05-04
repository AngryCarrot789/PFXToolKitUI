// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of FramePFX.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Text;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins.Exceptions;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Plugins;

public sealed class PluginLoader {
    private readonly List<Plugin> plugins;
    private List<Type>? corePlugins;
    private HashSet<Type> loadedPluginTypes; // is this a bad idea?

    public ReadOnlyCollection<Plugin> Plugins { get; }

    public PluginLoader() {
        this.plugins = new List<Plugin>();
        this.Plugins = this.plugins.AsReadOnly();
        this.loadedPluginTypes = new HashSet<Type>();
    }

    /// <summary>
    /// Adds a core plugin descriptor to the list of plugins to load once the application begins
    /// loading plugins. A core plugin is a plugin directly referenced by FramePFX, rather than
    /// being completely dynamically loaded
    /// </summary>
    /// <param name="descriptor">the descriptor</param>
    public void AddCorePlugin(Type pluginType) {
        Validate.NotNull(pluginType);
        if (!typeof(Plugin).IsAssignableFrom(pluginType))
            throw new ArgumentException($"Plugin type does not derive from {nameof(Plugin)}: {pluginType}");

        (this.corePlugins ??= new List<Type>()).Add(pluginType);
    }

    /// <summary>
    /// Loads all registered core plugins
    /// </summary>
    /// <param name="exceptions">A list of exceptions encountered</param>
    public void LoadCorePlugins(List<PluginLoadException> exceptions) {
        if (this.corePlugins == null) {
            return;
        }

        foreach (Type pluginType in this.corePlugins) {
            if (this.loadedPluginTypes.Contains(pluginType)) {
                exceptions.Add(new PluginLoadException("Plugin type already in use: " + pluginType));
            }
            else {
                Plugin? instance;
                try {
                    instance = (Plugin?) Activator.CreateInstance(pluginType) ?? throw new InvalidOperationException($"Failed to create plugin instance of type {pluginType}");
                }
                catch (Exception e) {
                    exceptions.Add(new PluginLoadException("Failed to create instance of plugin", e));
                    continue;
                }

                this.OnPluginCreated(null, instance);
            }
        }

        this.corePlugins.Clear();
        this.corePlugins = null;
    }

    /// <summary>
    /// Loads all dynamic plugins. This can be called multiple times with different target folders. Don't call twice with the same folder!
    /// </summary>
    /// <param name="pluginsFolder">The plugins folder to load from</param>
    /// <param name="exceptions">A list of exceptions encountered</param>
    public async Task LoadPlugins(string pluginsFolder, List<PluginLoadException> exceptions) {
        string[] dirs;
        try {
            // Plugins use relative paths, so we need to give them the full path so they can do their thing.
            // By using the full path here, it ensures GetDirectories returns full paths... hopefully
            string fullPath = Path.GetFullPath(pluginsFolder);
            dirs = Directory.Exists(fullPath) ? Directory.GetDirectories(fullPath) : [];
        }
        catch {
            // Plugins dir doesn't exist maybe
            dirs = [];
        }

        foreach (string folder in dirs) {
            Plugin? plugin = null;
            try {
                plugin = await this.ReadDescriptorAndCreatePluginInstance(folder);
            }
            catch (PluginLoadException e) {
                exceptions.Add(e);
            }
            catch (Exception e) {
                exceptions.Add(new PluginLoadException($"Unknown error loading plugin in folder {folder}", e));
            }

            if (plugin != null) {
                this.OnPluginCreated(folder, plugin);
            }
        }
    }

    public void RegisterCommands(CommandManager manager) {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterCommands(manager);
        }
    }

    public void RegisterServices() {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterServices();
        }
    }

    /// <summary>
    /// Gets all of the xaml files that plugins require to be injected into the application resources.
    /// This gives absolute paths
    /// </summary>
    /// <returns></returns>
    public List<(Plugin, string)> GetInjectableXamlResources() {
        List<(Plugin, string)> fullPaths = new List<(Plugin, string)>();
        foreach (Plugin plugin in this.plugins) {
            List<string> pluginPaths = new List<string>();
            plugin.GetXamlResources(pluginPaths);

            if (pluginPaths.Count > 0) {
                string? asmFullName;
                Assembly asm = plugin.GetType().Assembly;
                if ((asmFullName = asm.GetName().Name) == null) {
                    asmFullName = plugin.GetType().Namespace;
                }

                if (string.IsNullOrWhiteSpace(asmFullName)) {
                    AppLogger.Instance.WriteLine($"Could not identify {plugin.Name}'s assembly name");
                    continue;
                }

                foreach (string path in pluginPaths) {
                    if (!string.IsNullOrWhiteSpace(path)) {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("avares://").Append(asmFullName);
                        if (path[0] != '/') {
                            sb.Append('/');
                        }

                        sb.Append(path);
                        fullPaths.Add((plugin, sb.ToString()));
                    }
                }
            }
        }

        return fullPaths;
    }

    private void OnPluginCreated(string? pluginFolder, Plugin plugin) {
        this.plugins.Add(plugin);
        plugin.PluginLoader = this;
        plugin.PluginFolder = pluginFolder;
        plugin.OnCreated();
    }

    private async Task<Plugin> ReadDescriptorAndCreatePluginInstance(string folder) {
        string dllFile = Path.Combine(folder, $"{Path.GetFileName(folder)}.dll");
        Assembly assembly;
        try {
            assembly = await Task.Run(() => Assembly.LoadFrom(dllFile));
        }
        catch (Exception e) {
            throw new Exception($"Invalid or missing DLL file '{dllFile}'", e);
        }

        Type? entryType = null;
        foreach (Type type in assembly.ExportedTypes) {
            if (typeof(Plugin).IsAssignableFrom(type.BaseType)) {
                entryType = type;
                break;
            }
        }

        if (entryType == null)
            throw new PluginLoadException($"Could not find a class extending {nameof(Plugin)} in {dllFile}");

        if (entryType.IsInterface || entryType.IsAbstract)
            throw new Exception($"Plugin entry class is abstract or an interface: {entryType.Name} in {dllFile}");

        if (this.loadedPluginTypes.Contains(entryType))
            throw new PluginLoadException($"Plugin type already in use: {entryType}");

        return (Plugin) Activator.CreateInstance(entryType)! ?? throw new Exception("Failed to instantiate plugin");
    }

    public void RegisterConfigurations(PersistentStorageManager manager) {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterConfigurations(manager);
        }
    }

    public async Task OnApplicationFullyLoaded() {
        foreach (Plugin plugin in this.plugins) {
            await plugin.OnApplicationFullyLoaded();
        }
    }

    public void OnApplicationExiting() {
        foreach (Plugin plugin in this.plugins) {
            plugin.OnApplicationExiting();
        }
    }
}