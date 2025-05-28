// 
// Copyright (c) 2024-2025 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.AvControls;

/// <summary>
/// A button that uses a <see cref="PFXToolKitUI.Icons.Icon"/> to present an icon for the button contents
/// </summary>
public class IconButton : Button, IIconButton {
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconButton, Icon?>(nameof(Icon));
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<IconButton, Stretch>(nameof(Stretch), Stretch.Uniform);
    public static readonly StyledProperty<Dock> IconPlacementProperty = AvaloniaProperty.Register<IconButton, Dock>(nameof(IconPlacement), Dock.Left);
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<IconButton, double>(nameof(Spacing), 5.0);

    private double? iconW = 64, iconH = 64; // prevent crashing in designer due to infinitely or giant sized icons

    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    public Stretch Stretch {
        get => this.GetValue(StretchProperty);
        set => this.SetValue(StretchProperty, value);
    }

    public double? IconMaxWidth {
        get => this.iconW;
        set {
            this.iconW = value;
            IconButtonHelper.SetMaxWidth(this.PART_IconControl, value);
        }
    }

    public double? IconMaxHeight {
        get => this.iconH;
        set {
            this.iconH = value;
            IconButtonHelper.SetMaxHeight(this.PART_IconControl, value);
        }
    }

    public Dock IconPlacement {
        get => this.GetValue(IconPlacementProperty);
        set => this.SetValue(IconPlacementProperty, value);
    }

    public double Spacing {
        get => this.GetValue(SpacingProperty);
        set => this.SetValue(SpacingProperty, value);
    }

    private IconControl? PART_IconControl;
    private ContentPresenter? PART_ContentPresenter;
    private DockPanel? PART_DockPanel;

    public IconButton() {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty || change.Property == IconProperty || change.Property == SpacingProperty) {
            this.UpdateControlsVisibility();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        IconButtonHelper.ApplyTemplate(this, e, ref this.PART_IconControl);
        this.PART_ContentPresenter = e.NameScope.GetTemplateChild<ContentPresenter>("PART_ContentPresenter");
        this.PART_DockPanel = e.NameScope.GetTemplateChild<DockPanel>("PART_DockPanel");
        this.UpdateControlsVisibility();
    }

    private void UpdateControlsVisibility() {
        if (this.PART_IconControl != null) {
            this.PART_IconControl.IsVisible = this.Icon != null;
        }

        if (this.PART_ContentPresenter != null) {
            this.PART_ContentPresenter.IsVisible = this.Content != null && (!(this.Content is string str) || !string.IsNullOrEmpty(str));
        }

        if (this.PART_DockPanel != null) {
            double newSpacing = this.PART_IconControl?.IsVisible == true && this.PART_ContentPresenter?.IsVisible == true ? this.Spacing : 0.0;
            this.PART_DockPanel.VerticalSpacing = this.PART_DockPanel.HorizontalSpacing = newSpacing;
        }
    }
}