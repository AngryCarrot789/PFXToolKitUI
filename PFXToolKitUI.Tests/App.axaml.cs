using System;
using System.IO;
using Avalonia;
using Avalonia.Markup.Xaml;
using PFXToolKitUI.Avalonia;

namespace PFXToolKitUI.Tests;

public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
        AvUtils.OnApplicationInitialised();
        ApplicationPFX.InitializeInstance(new TestApplication(this));
    }

    public override async void OnFrameworkInitializationCompleted() {
        base.OnFrameworkInitializationCompleted();
        AvUtils.OnFrameworkInitialised();

        string[] envArgs = Environment.GetCommandLineArgs();
        if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
            Directory.SetCurrentDirectory(dir);
        }
        
        await ApplicationPFX.InitializeApplication(new EmptyApplicationStartupProgress(), envArgs);
    }
}