// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of PFXToolKitUI.
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
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Persistence;

namespace PFXToolKitUI.Plugins;

/// <summary>
/// The base class for a plugin's entry point class
/// </summary>
public abstract class Plugin {
    /// <summary>
    /// Gets the plugin loader that created this plugin instance
    /// </summary>
    public PluginLoader PluginLoader { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the folder that this plugin exists in (which contains plugin.xml).
    /// If this is a core plugin (and therefore not loaded from an assembly), this will be null,
    /// in which case you may be able to default to <see cref="Environment.CurrentDirectory"/>
    /// </summary>
    public string? PluginFolder { get; internal set; }

    /// <summary>
    /// Gets the display name of this plugin
    /// </summary>
    public virtual string Name => this.GetType().AssemblyQualifiedName ?? this.GetType().Name;

    protected Plugin() {
    }

    /// <summary>
    /// Invoked after the plugin is created and the descriptor is set.
    /// This can be used for initial state detection, e.g. detecting if the operating system is supported
    /// </summary>
    public virtual void OnCreated() {
    }

    /// <summary>
    /// Register this plugin's commands
    /// </summary>
    /// <param name="manager">Command manager</param>
    public virtual void RegisterCommands(CommandManager manager) {
    }

    /// <summary>
    /// Register this plugin's services
    /// </summary>
    public virtual void RegisterServices() {
    }

    /// <summary>
    /// Registers this plugin's configurations
    /// </summary>
    /// <returns></returns>
    public virtual void RegisterConfigurations(PersistentStorageManager manager) {
    }

    /// <summary>
    /// Invoked when the application has loaded. This is invoked before any editor window is created.
    /// Things like context menus, clip types, resource types, model->control mappings and so on should be registered here
    /// </summary>
    /// <returns></returns>
    public virtual Task OnApplicationFullyLoaded() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invoked when the application is about to exit. This is sort of pointless, but it exists just in case
    /// </summary>
    public virtual void OnApplicationExiting() {
    }

    public virtual void GetXamlResources(List<string> paths) {
    }
}