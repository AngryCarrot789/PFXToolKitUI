// 
// Copyright (c) 2024-2025 REghZy
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Configurations.Commands;
using PFXToolKitUI.Configurations.Dialogs;
using PFXToolKitUI.Configurations.Shortcuts.Commands;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.Plugins.Exceptions;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.Messaging.Configurations;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Themes.Commands;
using PFXToolKitUI.Themes.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI;

/// <summary>
/// The main application model class
/// </summary>
public abstract class ApplicationPFX : IServiceable, IComponentManager {
    private static ApplicationPFX? instance;
    private readonly ComponentStorage myComponentStorage;
    private ApplicationStartupPhase startupPhase;

    public static ApplicationPFX Instance {
        get {
            if (instance == null)
                throw new InvalidOperationException("Application instance has not been setup.");

            return instance;
        }
    }

    /// <summary>
    /// Gets the application service manager
    /// </summary>
    public ServiceManager ServiceManager { get; }

    /// <summary>
    /// Gets the application's persistent storage manager
    /// </summary>
    public PersistentStorageManager PersistentStorageManager => this.ServiceManager.GetService<PersistentStorageManager>();

    /// <summary>
    /// Gets the application main thread dispatcher
    /// </summary>
    public abstract IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the current version of the application. This value does not change during runtime.
    /// <para>The <see cref="Version.Major"/> property is used to represent a backwards-compatibility breaking change to the application (e.g. removal or a change in operating behaviour of a core feature)</para>
    /// <para>The <see cref="Version.Minor"/> property is used to represent a significant but non-breaking change (e.g. new feature that does not affect existing features, or a UI change)</para>
    /// <para>The <see cref="Version.Revision"/> property is used to represent any change to the code</para>
    /// <para>The <see cref="Version.Build"/> property is representing the current build, e.g. if a revision is made but then reverted, there are 2 builds in that</para>
    /// <para>
    /// 'for next update' meaning the number is incremented when there's a push to the github, as this is
    /// easiest to track. Many different changes can count as one update
    /// </para>
    /// </summary>
    // Even though we're version 2.0, I wouldn't consider this an official release yet, so we stay at 1.0
    public Version CurrentVersion { get; } = new Version(1, 0, 0, 0);

    /// <summary>
    /// Gets the current build version for this application. This accesses <see cref="CurrentVersion"/>, and changes whenever a new change is made to the application (regardless of how small)
    /// </summary>
    public int CurrentBuild => this.CurrentVersion.Build;

    /// <summary>
    /// Gets the application's plugin loader
    /// </summary>
    public PluginLoader PluginLoader { get; }

    /// <summary>
    /// Gets whether the application is in the process of shutting down
    /// </summary>
    public bool IsShuttingDown => this.StartupPhase == ApplicationStartupPhase.Stopping;

    /// <summary>
    /// Gets whether the application is actually running. False after exited
    /// </summary>
    public bool IsRunning => this.StartupPhase == ApplicationStartupPhase.Running;

    /// <summary>
    /// Gets the current application state
    /// </summary>
    public ApplicationStartupPhase StartupPhase {
        get => this.startupPhase;
        protected set {
            if (this.startupPhase == value)
                throw new InvalidOperationException("Already at phase: " + value);

            this.startupPhase = value;
            AppLogger.Instance.WriteLine($"Transitioned to startup phase: {value}");
        }
    }

    ComponentStorage IComponentManager.ComponentStorage => this.myComponentStorage;
    IComponentManager? IComponentManager.ParentComponentManager => null;

    protected ApplicationPFX() {
        this.myComponentStorage = new ComponentStorage(this);
        this.ServiceManager = new ServiceManager();
        this.PluginLoader = new PluginLoader();
    }

    /// <summary>
    /// Sets the global application instance. This can only be invoked once, with a non-null application instance
    /// </summary>
    /// <param name="application"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void InitializeInstance(ApplicationPFX application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
    }

    /// <summary>
    /// Initializes the application by calling <see cref="InitializeApplicationAsync"/> and blocks until completion using a dispatcher frame
    /// </summary>
    public static void InitializeApplication(IApplicationStartupProgress progress, string[] envArgs) {
        Instance.Dispatcher.AwaitForCompletion(InitializeApplicationAsync(progress, envArgs));
    }

    /// <summary>
    /// Actually initialize the application. This includes loading services, plugins, persistent configurations and more.
    /// </summary>
    public static async Task InitializeApplicationAsync(IApplicationStartupProgress progress, string[] envArgs) {
        ApplicationPFX app = Instance;

        await progress.ProgressAndWaitForRender("Setup", 0.01);

        // App initialisation takes a big chunk of the startup
        // phase, so it has a healthy dose of range available
        using (progress.CompletionState.PushCompletionRange(0.0, 0.7)) {
            try {
                app.StartupPhase = ApplicationStartupPhase.PreLoad;
                await progress.ProgressAndWaitForRender("Loading services");
                using (progress.CompletionState.PushCompletionRange(0.0, 0.2)) {
                    app.RegisterServices(app.ServiceManager);
                }

                string storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), app.GetApplicationName(), "Options");
                app.ServiceManager.RegisterConstant(new PersistentStorageManager(storageDir));

                await progress.ProgressAndWaitForRender("Loading commands");
                using (progress.CompletionState.PushCompletionRange(0.2, 0.4)) {
                    app.RegisterCommands(CommandManager.Instance);
                }
                
                await progress.ProgressAndWaitForRender("Loading keymap...");
                using (progress.CompletionState.PushCompletionRange(0.4, 0.6)) {
                    string keymapFilePath = Path.GetFullPath("Keymap.xml");
                    try {
                        await using FileStream stream = File.OpenRead(keymapFilePath);
                        ShortcutManager.Instance.ReloadFromStream(stream);
                    }
                    catch (FileNotFoundException) {
                        AppLogger.Instance.WriteLine("Keymap file does not exist at " + keymapFilePath + ". This error can be ignored, but shortcuts won't work");
                    }
                    catch (Exception ex) {
                        AppLogger.Instance.WriteLine("Failed to read keymap file" + keymapFilePath + ". This error can be ignored, but shortcuts won't work" + Environment.NewLine + ex.GetToString());
                    }
                }

                app.StartupPhase = ApplicationStartupPhase.Loading;
                await progress.ProgressAndWaitForRender("Loading application", 0.8);
                using (progress.CompletionState.PushCompletionRange(0.8, 1.0)) {
                    await app.OnSetupApplication(progress);
                }

                await progress.WaitForRender();
            }
            catch (Exception ex) {
                Console.WriteLine("Exception during setup:" + Environment.NewLine + ex.GetToString());
                Debugger.Break();
                await app.OnSetupFailed(ex);
                return;
            }
        }

        await progress.ProgressAndWaitForRender("Loading plugins");
        using (progress.CompletionState.PushCompletionRange(0.7, 0.8)) {
            List<PluginLoadException> exceptions = new List<PluginLoadException>();
            app.PluginLoader.LoadCorePlugins(exceptions);

#if DEBUG
            string? solutionFolder = null;
            string workingDirectory = Directory.GetCurrentDirectory();
            for (string? folder = workingDirectory; folder != null; folder = Path.GetDirectoryName(folder)) {
                foreach (string file in Directory.EnumerateFiles(folder)) {
                    if (file.EndsWith(".sln")) {
                        solutionFolder = folder;
                        break;
                    }
                }
            }

            if (solutionFolder != null) {
                // Load plugins in the solution folder
                string? solName = app.GetSolutionFileName();
                if (!string.IsNullOrWhiteSpace(solName) && File.Exists(Path.Combine(solutionFolder, solName))) {
                    await app.PluginLoader.LoadPlugins(Path.Combine(solutionFolder, "Plugins"), exceptions);
                }
            }
#endif
            await app.PluginLoader.LoadPlugins("Plugins", exceptions);

            if (exceptions.Count > 0) {
                string errorText = string.Join(Environment.NewLine + Environment.NewLine, exceptions);
                AppLogger.Instance.WriteLine(errorText);
                await IMessageDialogService.Instance.ShowMessage("Errors", "One or more exceptions occurred while loading plugins. See logs for more info");
            }

            await progress.ProgressAndWaitForRender("Initialising plugins...", 0.5);
            app.PluginLoader.InitializePlugins();
            await app.OnPluginsLoaded();
        }

        {
            await progress.ProgressAndWaitForRender("Loading configurations...");
            PersistentStorageManager psm = app.PersistentStorageManager;

            app.RegisterConfigurations();
            app.PluginLoader.RegisterConfigurations(psm);

            await psm.LoadAllAsync(null, false);
        }

        await progress.ProgressAndWaitForRender("Finalizing startup...", 0.99);
        {
            app.StartupPhase = ApplicationStartupPhase.FullyLoaded;
            await app.OnApplicationFullyLoaded();
            await app.PluginLoader.OnApplicationFullyLoaded();
        }

        await Task.Delay(500);

        app.StartupPhase = ApplicationStartupPhase.Running;
        await app.OnApplicationRunning(progress, envArgs);
    }

    // The methods from RegisterServices to OnExiting are ordered based
    // on the order they're invoked during application lifetime.

    protected virtual void RegisterServices(ServiceManager manager) {
        manager.RegisterConstant(ApplicationConfigurationManager.Instance);
        manager.RegisterConstant(new ActivityManager());
        manager.RegisterConstant(new CommandManager());
    }

    protected virtual void RegisterCommands(CommandManager manager) {
#if DEBUG
        manager.Register("commands.application.DebugContextCommand", new DebugContextCommand());
#endif

        manager.Register("commands.mainWindow.OpenEditorSettings", new OpenApplicationSettingsCommand());
        manager.Register("commands.application.AboutApplicationCommand", new AboutApplicationCommand());
        manager.Register("commands.application.ShowLogsCommand", new ShowLogsCommand());

        // Config managers
        manager.Register("commands.shortcuts.AddKeyStrokeToShortcut", new AddKeyStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.shortcuts.AddMouseStrokeToShortcut", new AddMouseStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.themes.ShowKeysInheritingFromThemeCommand", new ShowKeysInheritingFromThemeCommand());
        manager.Register("commands.config.keymap.ExpandShortcutTree", new ExpandShortcutTreeCommand());
        manager.Register("commands.config.keymap.CollapseShortcutTree", new CollapseShortcutTreeCommand());
        manager.Register("commands.config.themeconfig.ExpandThemeConfigTree", new ExpandThemeConfigTreeCommand());
        manager.Register("commands.config.themeconfig.CollapseThemeConfigTree", new CollapseThemeConfigTreeCommand());
        manager.Register("commands.config.themeconfig.CreateInheritedCopy", new CreateThemeCommand(false));
        manager.Register("commands.config.themeconfig.CreateCompleteCopy", new CreateThemeCommand(true));
        manager.Register("commands.config.dialogs.DeleteSelectedDialogResultEntriesCommand", new DeleteSelectedDialogResultEntriesCommand());
    }

    /// <summary>
    /// Invoked after services and commands are loaded but before any plugins are loaded
    /// </summary>
    protected virtual Task OnSetupApplication(IApplicationStartupProgress progress) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when setup fails. Maybe a command or service could not be created or threw an error, or maybe <see cref="OnSetupApplication"/> threw
    /// </summary>
    /// <param name="exception">The exception that occured</param>
    protected virtual async Task OnSetupFailed(Exception exception) {
        if (this.ServiceManager.TryGetService(out IMessageDialogService? service)) {
            await service.ShowMessage("App startup failed", "Failed to initialise application", exception.ToString());
        }

        this.Dispatcher.InvokeShutdown();
    }

    /// <summary>
    /// Invoked when all plugins are loaded
    /// </summary>
    protected virtual Task OnPluginsLoaded() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers the application configurations
    /// </summary>
    protected virtual void RegisterConfigurations() {
        this.PersistentStorageManager.Register(new PersistentDialogResultConfiguration(), "dialogs", "preferred-options");
    }

    /// <summary>
    /// Invoked once the application is fully loaded
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnApplicationFullyLoaded() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invoked once the application is in the running state. This delegates to <see cref="IStartupManager.OnApplicationStartupWithArgs"/>
    /// </summary>
    /// <param name="progress">Progress manager</param>
    /// <param name="envArgs">Command line arguments, typically passed to the startup manager</param>
    protected virtual Task OnApplicationRunning(IApplicationStartupProgress progress, string[] envArgs) {
        if (Instance.ServiceManager.TryGetService(out IStartupManager? service)) {
            return service.OnApplicationStartupWithArgs(progress, envArgs.Length > 1 ? envArgs.Skip(1).ToArray() : Array.Empty<string>());
        }
        else {
            return IMessageDialogService.Instance.ShowMessage("Information", "Hey! No IStartupManager service registered. Define and Register one to do stuff!");
        }
    }

    protected virtual async Task OnExiting(int exitCode) {
        this.StartupPhase = ApplicationStartupPhase.Stopping;
        this.PluginLoader.OnApplicationExiting();

        PersistentStorageManager manager = this.PersistentStorageManager;

        // Should be inactive at this point realistically, but just in case, clear it all since we're exiting
        AppLogger.Instance.WriteLine("Saving configs...");
        while (manager.IsSaveStackActive) {
            if (manager.EndSavingStack()) {
                break;
            }
        }

        manager.SaveAll();

        AppLogger.Instance.WriteLine("Waiting for configs to flush to disk...");
        // Task task = manager.FlushToDisk(true);
        await manager.FlushToDisk(true);
        // Debug.Assert(task.IsCompleted);
        AppLogger.Instance.WriteLine("Flushed!");
    }

    public void EnsureBeforePhase(ApplicationStartupPhase phase) {
        if (this.StartupPhase >= phase)
            throw new InvalidOperationException($"Application startup phase has passed '{phase}'");
    }

    public void EnsureAfterPhase(ApplicationStartupPhase phase) {
        if (this.StartupPhase <= phase)
            throw new InvalidOperationException($"Application has not reached the startup phase '{phase}' yet.");
    }

    public void EnsureAtPhase(ApplicationStartupPhase phase) {
        if (this.startupPhase == phase)
            return;

        if (this.StartupPhase < phase)
            throw new InvalidOperationException($"Application has not reached the startup phase '{phase}' yet.");

        if (this.StartupPhase > phase)
            throw new InvalidOperationException($"Application has already passed the startup phase '{phase}' yet.");
    }

    public bool IsBeforePhase(ApplicationStartupPhase phase) {
        return this.StartupPhase < phase;
    }

    public bool IsAfterPhase(ApplicationStartupPhase phase) {
        return this.StartupPhase > phase;
    }

    public bool IsAtPhase(ApplicationStartupPhase phase) {
        return this.StartupPhase == phase;
    }

    /// <summary>
    /// Shuts down the application. May not happen immediately.
    /// </summary>
    public virtual void Shutdown() => this.Dispatcher.InvokeShutdown();

    protected abstract string? GetSolutionFileName();

    public abstract string GetApplicationName();

    /// <summary>
    /// Gets an application service. Delegates to <see cref="Services.ServiceManager.GetService{T}"/> of <see cref="ServiceManager"/>
    /// </summary>
    public static T GetService<T>() where T : class {
        return Instance.ServiceManager.GetService<T>();
    }

    /// <summary>
    /// Tries to get an application service. Delegates to <see cref="Services.ServiceManager.TryGetService{T}"/> of <see cref="ServiceManager"/>
    /// </summary>
    public static bool TryGetService<T>([NotNullWhen(true)] out T? service, bool canCreate = true) where T : class {
        return Instance.ServiceManager.TryGetService(out service, canCreate);
    }
}