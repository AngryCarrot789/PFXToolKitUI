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
using Avalonia.Controls;
using Avalonia.Interactivity;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService.ToolTips;

public partial class ContextMenuToolTip : UserControl, IToolTipControl {
    private static readonly Dictionary<Type, Type> hintInfoToControlType = new Dictionary<Type, Type>();
    private static readonly Dictionary<Type, Control> hintControlCache = new Dictionary<Type, Control>();

    private readonly EventUpdateBinder<BaseMenuEntry> normalToolTipBinder =
        new EventUpdateBinder<BaseMenuEntry>(
            nameof(BaseMenuEntry.DescriptionChanged),
            b => {
                TextBlock? tb = ((ContextMenuToolTip) b.Control).PART_ToolTipText;
                tb.IsVisible = !string.IsNullOrWhiteSpace(b.Model.Description);
                tb.Text = b.Model.Description;
                ((ContextMenuToolTip) b.Control).UpdateSeparator();
            });

    private BaseMenuEntry? currentEntry;
    private DisabledHintInfo? hintInfo;
    private Control? myCurrentDisabledHintControl;
    private readonly ColourBrushHandler normalTTForegroundHandler;

    public ContextMenuToolTip() {
        this.InitializeComponent();
        this.normalToolTipBinder.AttachControl(this);
        this.normalTTForegroundHandler = new ColourBrushHandler(TextBlock.ForegroundProperty);
        this.normalTTForegroundHandler.SetTarget(this.PART_ToolTipText);
    }

    static ContextMenuToolTip() {
        RegisterDisabledHintControl<SimpleDisabledHintInfo>(typeof(SimpleDisabledHintInfoControl));
    }

    public static void RegisterDisabledHintControl<T>(Type typeOfControl) {
        hintInfoToControlType[typeof(T)] = typeOfControl;
    }

    private static Control GetControlForDisabledHintType(Type typeOfHintInfo) {
        if (!hintControlCache.TryGetValue(typeOfHintInfo, out Control? tip)) {
            Type controlType = hintInfoToControlType[typeOfHintInfo];
            hintControlCache[typeOfHintInfo] = tip = (Control) (Activator.CreateInstance(controlType) ?? throw new InvalidOperationException("huh"));
        }

        return tip;
    }

    private static DisabledHintInfo? TryGetDisabledHintInfo(Control owner, BaseMenuEntry entry) {
        ContextRegistry? registry = (((AdvancedMenuItem) owner).OwnerMenu as AdvancedContextMenu)?.MyContextRegistry;
        DisabledHintInfo? hint = entry.ProvideDisabledHint?.Invoke(DataManager.GetFullContextData(owner), registry);
        if (hint != null) {
            return hint;
        }

        if (entry is CommandMenuEntry cmdEntry) {
            if (CommandManager.Instance.TryFindCommandById(cmdEntry.CommandId, out Command? command)) {
                if (command is IDisabledHintProvider hintProvider) {
                    hint = hintProvider.ProvideDisabledHint(DataManager.GetFullContextData(owner), registry);
                    if (hint != null) {
                        return hint;
                    }
                }
            }
        }

        return null;
    }

    public void OnOpening(Control owner, CancelRoutedEventArgs e) {
        BaseMenuEntry? entry = ((AdvancedMenuItem) owner).Entry;
        if (entry != null) {
            if (!string.IsNullOrWhiteSpace(entry.Description)) {
                return; // we at least have a description so we can show
            }

            this.hintInfo?.Dispose();
            if ((this.hintInfo = TryGetDisabledHintInfo(owner, entry)) != null) {
                return;
            }
        }

        e.Cancel = true;
    }

    public void OnOpened(Control owner, IContextData data) {
        this.currentEntry = ((AdvancedMenuItem) owner).Entry;
        if (this.currentEntry != null) {
            this.normalToolTipBinder.AttachModel(this.currentEntry);
        }

        if (this.currentEntry != null && (this.hintInfo ??= TryGetDisabledHintInfo(owner, this.currentEntry)) != null) {
            this.myCurrentDisabledHintControl = GetControlForDisabledHintType(this.hintInfo!.GetType());
            this.PART_DisabledTipInfoControl.Content = this.myCurrentDisabledHintControl;
            this.PART_DisabledTipInfoControl.IsVisible = true;
            if (this.myCurrentDisabledHintControl is IDisabledHintInfoControl control) {
                control.OnConnected(this.hintInfo);
            }

            this.normalTTForegroundHandler.Brush = StandardIcons.DisabledForegroundBrush;
        }
        else {
            this.PART_DisabledTipInfoControl.IsVisible = false;
            this.normalTTForegroundHandler.Brush = StandardIcons.ForegroundBrush;
        }

        this.UpdateSeparator();
    }

    public void OnClosed(Control owner) {
        this.normalToolTipBinder.SwitchModel(null);
        DisabledHintInfo? oldInfo = this.hintInfo;
        this.hintInfo = null;
        if (this.myCurrentDisabledHintControl != null) {
            Debug.Assert(ReferenceEquals(this.PART_DisabledTipInfoControl.Content, this.myCurrentDisabledHintControl));
            Debug.Assert(oldInfo != null);
            if (this.myCurrentDisabledHintControl is IDisabledHintInfoControl control) {
                control.OnDisconnected(oldInfo);
            }

            this.PART_DisabledTipInfoControl.Content = null;
            this.myCurrentDisabledHintControl = null;
        }

        oldInfo?.Dispose();
    }

    internal void OnCanExecuteChanged() {
    }

    private void UpdateSeparator() {
        this.PART_Separator.IsVisible = this.currentEntry != null
                                        && !string.IsNullOrWhiteSpace(this.currentEntry.Description)
                                        && this.PART_DisabledTipInfoControl.IsVisible;
    }
}