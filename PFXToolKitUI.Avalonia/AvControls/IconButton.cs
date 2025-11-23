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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.AvControls;

/// <summary>
/// A button that uses a <see cref="PFXToolKitUI.Icons.Icon"/> to present an icon for the button contents
/// </summary>
public class IconButton : Button, IIconButton {
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconButton, Icon?>(nameof(Icon));
    public static readonly StyledProperty<StretchMode> StretchProperty = AvaloniaProperty.Register<IconButton, StretchMode>(nameof(Stretch), StretchMode.UniformNoUpscale);
    public static readonly StyledProperty<Dock> IconPlacementProperty = AvaloniaProperty.Register<IconButton, Dock>(nameof(IconPlacement));
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<IconButton, double>(nameof(Spacing), 5.0);
    public static readonly StyledProperty<Thickness> IconPaddingProperty = AvaloniaProperty.Register<IconButton, Thickness>(nameof(IconPadding), new Thickness(3.0));
    public static readonly StyledProperty<Thickness> ContentPaddingProperty = AvaloniaProperty.Register<IconButton, Thickness>(nameof(ContentPadding), new Thickness(2.0));

    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    public StretchMode Stretch {
        get => this.GetValue(StretchProperty);
        set => this.SetValue(StretchProperty, value);
    }

    public double? IconMaxWidth {
        get => field;
        set {
            field = value;
            IconButtonHelper.SetMaxWidth(this.PART_IconControl, value);
        }
    } = 64;

    public double? IconMaxHeight {
        get => field;
        set {
            field = value;
            IconButtonHelper.SetMaxHeight(this.PART_IconControl, value);
        }
    } = 64;

    public Dock IconPlacement {
        get => this.GetValue(IconPlacementProperty);
        set => this.SetValue(IconPlacementProperty, value);
    }

    public double Spacing {
        get => this.GetValue(SpacingProperty);
        set => this.SetValue(SpacingProperty, value);
    }

    public Thickness IconPadding {
        get => this.GetValue(IconPaddingProperty);
        set => this.SetValue(IconPaddingProperty, value);
    }

    public Thickness ContentPadding {
        get => this.GetValue(ContentPaddingProperty);
        set => this.SetValue(ContentPaddingProperty, value);
    }

    private IconControl? PART_IconControl;
    private ContentPresenter? PART_ContentPresenter;
    private Grid? PART_Grid;
    private RowDefinition? SpacingRowDefinition;
    private ColumnDefinition? SpacingColumnDefinition;

    public IconButton() {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty
            || change.Property == IconProperty
            || change.Property == SpacingProperty
            || change.Property == IconPlacementProperty
            || this.ShouldUpdateForPaddingChange(change)) {
            this.UpdateGridArrangement();
        }
    }

    private bool ShouldUpdateForPaddingChange(AvaloniaPropertyChangedEventArgs change) {
        return this.PART_IconControl != null && (change.Property == IconPaddingProperty || change.Property == ContentPaddingProperty) && (this.PART_IconControl!.IsVisible && this.PART_IconControl!.IsVisible);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        IconButtonHelper.ApplyTemplate(this, e, ref this.PART_IconControl);
        this.PART_ContentPresenter = e.NameScope.GetTemplateChild<ContentPresenter>("PART_ContentPresenter");
        this.PART_Grid = e.NameScope.GetTemplateChild<Grid>("PART_Grid");
        this.SpacingRowDefinition = this.PART_Grid.RowDefinitions[1];
        this.SpacingColumnDefinition = this.PART_Grid.ColumnDefinitions[1];
        this.UpdateGridArrangement();
    }

    private void UpdateGridArrangement() {
        if (this.PART_Grid == null) {
            return;
        }

        Dock placement = this.IconPlacement;
        bool hasIcon = this.PART_IconControl!.IsVisible = this.Icon != null;
        bool hasText = this.PART_ContentPresenter!.IsVisible = this.Content != null && (!(this.Content is string str) || !string.IsNullOrEmpty(str));
        bool hasBoth = hasIcon && hasText;
        bool isHorizontal = placement == Dock.Left || placement == Dock.Right;
        if (hasBoth) {
            double existingPadding;
            Grid.SetColumnSpan(this.PART_IconControl, isHorizontal ? 1 : 3);
            Grid.SetColumnSpan(this.PART_ContentPresenter, isHorizontal ? 1 : 3);
            Grid.SetRowSpan(this.PART_IconControl, isHorizontal ? 3 : 1);
            Grid.SetRowSpan(this.PART_ContentPresenter, isHorizontal ? 3 : 1);
            this.PART_IconControl.SetValue(isHorizontal ? Grid.RowProperty : Grid.ColumnProperty, 0);
            this.PART_ContentPresenter.SetValue(isHorizontal ? Grid.RowProperty : Grid.ColumnProperty, 0);

            switch (this.IconPlacement) {
                case Dock.Left:
                    Grid.SetColumn(this.PART_IconControl, 0);
                    Grid.SetColumn(this.PART_ContentPresenter, 2);
                    existingPadding = this.IconPadding.Right + this.ContentPadding.Left;
                    break;
                case Dock.Bottom:
                    Grid.SetRow(this.PART_IconControl, 2);
                    Grid.SetRow(this.PART_ContentPresenter, 0);
                    existingPadding = this.IconPadding.Top + this.ContentPadding.Bottom;
                    break;
                case Dock.Right:
                    Grid.SetColumn(this.PART_IconControl, 2);
                    Grid.SetColumn(this.PART_ContentPresenter, 0);
                    existingPadding = this.IconPadding.Left + this.ContentPadding.Right;
                    break;
                case Dock.Top:
                    Grid.SetRow(this.PART_IconControl, 0);
                    Grid.SetRow(this.PART_ContentPresenter, 2);
                    existingPadding = this.IconPadding.Bottom + this.ContentPadding.Top;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            GridLength spacing = new GridLength(Math.Max(this.Spacing - existingPadding, 0.0));
            this.SpacingRowDefinition!.Height = spacing;
            this.SpacingColumnDefinition!.Width = spacing;
        }
        else {
            this.SpacingRowDefinition!.Height = default;
            this.SpacingColumnDefinition!.Width = default;
            if (hasIcon) {
                Grid.SetColumn(this.PART_ContentPresenter, 1);
                Grid.SetRow(this.PART_ContentPresenter, 1);
                Grid.SetColumnSpan(this.PART_ContentPresenter, 1);
                Grid.SetRowSpan(this.PART_ContentPresenter, 1);

                Grid.SetColumn(this.PART_IconControl, 0);
                Grid.SetRow(this.PART_IconControl, 0);
                Grid.SetColumnSpan(this.PART_IconControl, 3);
                Grid.SetRowSpan(this.PART_IconControl, 3);
            }
            else /* hasText */ {
                Grid.SetColumn(this.PART_IconControl, 1);
                Grid.SetRow(this.PART_IconControl, 1);
                Grid.SetColumnSpan(this.PART_IconControl, 1);
                Grid.SetRowSpan(this.PART_IconControl, 1);

                Grid.SetColumn(this.PART_ContentPresenter, 0);
                Grid.SetRow(this.PART_ContentPresenter, 0);
                Grid.SetColumnSpan(this.PART_ContentPresenter, 3);
                Grid.SetRowSpan(this.PART_ContentPresenter, 3);
            }
        }
    }
}