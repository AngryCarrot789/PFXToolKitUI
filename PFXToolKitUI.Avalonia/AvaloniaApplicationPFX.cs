using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Configurations;
using PFXToolKitUI.Avalonia.Icons;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Files;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Themes;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Avalonia.Toolbars.Toolbars;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Toolbars;

namespace PFXToolKitUI.Avalonia;

public abstract class AvaloniaApplicationPFX : ApplicationPFX {
    public Application Application { get; }
    
    public override IDispatcher Dispatcher { get; }

    protected AvaloniaApplicationPFX(Application application) {
        this.Application = application ?? throw new ArgumentNullException(nameof(application));
        Dispatcher avd = global::Avalonia.Threading.Dispatcher.UIThread;
        this.Dispatcher = new AvaloniaDispatcherDelegate(avd);
        
        if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime e) {
            e.Exit += this.OnApplicationExit;
        }
        
        avd.ShutdownFinished += this.OnDispatcherShutDown;
    }

    private void OnDispatcherShutDown(object? sender, EventArgs e) {
        this.StartupPhase = ApplicationStartupPhase.Stopped;
    }
    
    private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        this.OnExiting(e.ApplicationExitCode);
    }

    protected override void RegisterServices(ServiceManager manager) {
        // We always want to make sure message dialogs are registered, just in case of errors
        manager.RegisterConstant<IMessageDialogService>(new MessageDialogServiceImpl());
        
        // we have to register these before base class
        manager.RegisterConstant<ShortcutManager>(new AvaloniaShortcutManager());
        manager.RegisterConstant<ThemeManager>(new ThemeManagerImpl(this.Application));
        manager.RegisterConstant<IconManager>(new IconManagerImpl());
        
        base.RegisterServices(manager);
        
        manager.RegisterConstant<IUserInputDialogService>(new InputDialogServiceImpl());
        manager.RegisterConstant<IColourPickerDialogService>(new ColourPickerDialogServiceImpl());
        manager.RegisterConstant<IFilePickDialogService>(new FilePickDialogServiceImpl());
        manager.RegisterConstant<IConfigurationDialogService>(new ConfigurationDialogServiceImpl());
        manager.RegisterConstant<IInputStrokeQueryDialogService>(new InputStrokeDialogsImpl());
        manager.RegisterConstant<BrushManager>(new BrushManagerImpl());
        manager.RegisterConstant<ToolbarButtonFactory>(new ToolbarButtonFactoryImpl());
    }

    protected override async Task OnSetupApplication(IApplicationStartupProgress progress) {
        await base.OnSetupApplication(progress);
        
        await progress.ProgressAndSynchroniseAsync("Loading themes...");
        ((ThemeManagerImpl) this.ServiceManager.GetService<ThemeManager>()).SetupBuiltInThemes();
    }

    protected override async Task OnPluginsLoaded() {
        List<(Plugin, string)> injectable = this.PluginLoader.GetInjectableXamlResources();
        if (injectable.Count > 0) {
            IList<IResourceProvider> resources = this.Application.Resources.MergedDictionaries;
            
            List<string> errorLines = new List<string>();
            foreach ((Plugin plugin, string path) in injectable) {
                int idx = resources.Count;
                try {
                    // adding resource here is the only way to actually get an exception e.g. when file does not exist or is invalid or whatever
                    resources.Add(new ResourceInclude((Uri?) null) { Source = new Uri(path) });
                }
                catch (Exception e) {
                    // remove invalid resource include
                    try {
                        resources.RemoveAt(idx);
                    }
                    catch { /* will always throw but it does remove the item */ }

                    errorLines.Add(plugin.Name + ": " + path + "\n" + e);
                }
            }

            if (errorLines.Count > 0) {
                string dblNewLine = Environment.NewLine + Environment.NewLine;
                await IMessageDialogService.Instance.ShowMessage(
                    "Error loading plugin XAML", 
                    "One or more plugins' XAML files are invalid. Issues may occur later on.", 
                    string.Join(dblNewLine, errorLines));
            }
        }
    }
}