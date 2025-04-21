using System.Collections.Generic;
using System.Threading.Tasks;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Tests.Configs;

public class StartupPropEditorConfigurationPage : PropertyEditorConfigurationPage {
    public static readonly DataParameterString StartupThemeParameter = DataParameter.Register(new DataParameterString(typeof(StartupPropEditorConfigurationPage), nameof(StartupTheme), "Dark", ValueAccessors.Reflective<string?>(typeof(StartupPropEditorConfigurationPage), nameof(startupTheme))));

    private string? startupTheme;

    public string? StartupTheme {
        get => this.startupTheme;
        set => DataParameter.SetValueHelper(this, StartupThemeParameter, ref this.startupTheme, value);
    }

    public StartupPropEditorConfigurationPage() {
        this.startupTheme = StartupThemeParameter.GetDefaultValue(this);

        this.PropertyEditor.Root.AddItem(new DataParameterStringPropertyEditorSlot(StartupThemeParameter, typeof(StartupPropEditorConfigurationPage), "Startup Theme"));
    }

    static StartupPropEditorConfigurationPage() {
        AffectsModifiedState(StartupThemeParameter);
    }

    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
        this.startupTheme = options.StartupTheme;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
        options.StartupTheme = this.startupTheme ?? "";
        options.ApplyTheme();
        return ValueTask.CompletedTask;
    }
}