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
        AffectsIsModified(StartupThemeParameter);
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