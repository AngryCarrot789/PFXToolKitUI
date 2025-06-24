// 
// Copyright (c) 2025 REghZy
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

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Themes;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Services;
using PFXToolKitUI.Tests.Configs;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Tests;

public class TestApplication : AvaloniaApplicationPFX {
    public TestApplication(Application application) : base(application) {
    }

    protected override void RegisterServices(ServiceManager manager) {
        base.RegisterServices(manager);
        manager.RegisterConstant<IStartupManager>(new TestAppStartupManager());
        manager.RegisterConstant<IIconPreferences>(new IconPreferencesImpl());
        manager.RegisterConstant<IDesktopService>(new DesktopServiceImpl(this.Application));
    }

    private class IconPreferencesImpl : IIconPreferences {
        public bool UseAntiAliasing {
            get => EditorConfigurationOptions.Instance.UseIconAntiAliasing;
            set => EditorConfigurationOptions.Instance.UseIconAntiAliasing = value;
        }
    }
    
    protected override void RegisterCommands(CommandManager manager) {
        base.RegisterCommands(manager);
    }

    protected override async Task OnSetupApplication(IApplicationStartupProgress progress) {
        await base.OnSetupApplication(progress);
        
        ApplicationConfigurationManager appConfig = ApplicationConfigurationManager.Instance;
        appConfig.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Startup", Id = "config.startup", Page = new StartupPropEditorConfigurationPage()
        });

        // register core plugins here, as they will be loaded once we return
    }

    protected override void RegisterConfigurations() {
        PersistentStorageManager psm = this.PersistentStorageManager;
        
        psm.Register(new EditorConfigurationOptions(), "editor", "window");
        psm.Register(new StartupConfigurationOptions(), null, "startup");
        psm.Register<ThemeConfigurationOptions>(new ThemeConfigurationOptionsImpl(), "themes", "themes");
    }

    protected override Task OnApplicationFullyLoaded() {
        StartupConfigurationOptions.Instance.ApplyTheme();
        return Task.CompletedTask;
    }
    
    protected override async Task OnApplicationRunning(IApplicationStartupProgress progress, string[] envArgs) {
        if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            await progress.ProgressAndSynchroniseAsync("Startup completed", 1.0);
            await base.OnApplicationRunning(progress, envArgs);
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        else {
            await base.OnApplicationRunning(progress, envArgs);
        }
    }
    
    protected override void OnExiting(int exitCode) {
        base.OnExiting(exitCode);
    }
    
    protected override string? GetSolutionFileName() {
        return "PFXToolKitUI.Tests.sln";
    }

    public override string GetApplicationName() {
        return "PFXToolKitUI.Tests";
    }
}