// 
// Copyright (c) 2024-2026 REghZy
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

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Metadata;
using PFXToolKitUI.Avalonia.Controls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.ToolTips;

/// <summary>
/// A control to be placed within a tool tip that supports showing text inlines
/// and optionally a small block at the end with the shortcut(s) to trigger the command
/// </summary>
public class CommandToolTip : Control, IToolTipControl {
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<CommandToolTip, string?>(nameof(Text));
    public static readonly StyledProperty<string?> CommandIdProperty = AvaloniaProperty.Register<CommandToolTip, string?>(nameof(CommandId));
    public static readonly DirectProperty<CommandToolTip, InlineCollection?> InlinesProperty = AvaloniaProperty.RegisterDirect<CommandToolTip, InlineCollection?>(nameof(Inlines), t => t.Inlines, (t, v) => t.Inlines = v);
    public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty = TextElement.FontFeaturesProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<double> FontSizeProperty = TextElement.FontSizeProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<FontStyle> FontStyleProperty = TextElement.FontStyleProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<FontWeight> FontWeightProperty = TextElement.FontWeightProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<FontStretch> FontStretchProperty = TextElement.FontStretchProperty.AddOwner<CommandToolTip>();
    public static readonly StyledProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<CommandToolTip>();

    public string? Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public string? CommandId {
        get => this.GetValue(CommandIdProperty);
        set => this.SetValue(CommandIdProperty, value);
    }

    [Content]
    public InlineCollection? Inlines {
        get => field;
        set => this.SetAndRaise(InlinesProperty, ref field, value);
    }

    public FontFamily FontFamily {
        get => this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }

    public FontFeatureCollection? FontFeatures {
        get => this.GetValue(FontFeaturesProperty);
        set => this.SetValue(FontFeaturesProperty, value);
    }

    public double FontSize {
        get => this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle {
        get => this.GetValue(FontStyleProperty);
        set => this.SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight {
        get => this.GetValue(FontWeightProperty);
        set => this.SetValue(FontWeightProperty, value);
    }

    public FontStretch FontStretch {
        get => this.GetValue(FontStretchProperty);
        set => this.SetValue(FontStretchProperty, value);
    }

    public IBrush? Foreground {
        get => this.GetValue(ForegroundProperty);
        set => this.SetValue(ForegroundProperty, value);
    }

    private bool isToolTipOpen;
    private readonly TextBlock myTextBlock;
    private readonly InlineCollection myTextBlockInlines;

    public CommandToolTip() {
        this.myTextBlock = new TextBlock() {
            TextWrapping = TextWrapping.Wrap
        };

        this.myTextBlockInlines = this.myTextBlock.Inlines!;

        this.LogicalChildren.Add(this.myTextBlock);
        this.VisualChildren.Add(this.myTextBlock);

        this.Inlines = new InlineCollection();
    }

    static CommandToolTip() {
        TextProperty.Changed.AddClassHandler<CommandToolTip, string?>((s, e) => s.InvalidateInlines());
        CommandIdProperty.Changed.AddClassHandler<CommandToolTip, string?>((s, e) => s.InvalidateInlines());
        InlinesProperty.Changed.AddClassHandler<CommandToolTip, InlineCollection?>((s, e) => {
            if (e.TryGetOldValue(out InlineCollection? oldValue))
                oldValue.CollectionChanged -= s.OnInlinesCollectionChanged;
            if (e.TryGetNewValue(out InlineCollection? newValue))
                newValue.CollectionChanged += s.OnInlinesCollectionChanged;

            s.InvalidateInlines();
        });
    }

    private void OnInlinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        this.InvalidateInlines();
    }

    private void InvalidateInlines() {
        this.myTextBlockInlines.Clear();
        if (this.isToolTipOpen) {
            this.GenerateInlines();
        }
    }

    private void GenerateInlines() {
        this.myTextBlockInlines.Clear();
        if (this.Inlines != null) {
            this.myTextBlockInlines.AddRange(this.Inlines.Select(x => {
                x.BaselineAlignment = BaselineAlignment.Center;
                return x;
            }));
        }

        if (!string.IsNullOrEmpty(this.Text)) {
            this.myTextBlockInlines.Add(new Run(this.Text) { BaselineAlignment = BaselineAlignment.Center });
        }

        if (!string.IsNullOrWhiteSpace(this.CommandId)) {
            this.myTextBlockInlines.Add(new InlineUIContainer(new CommandShortcutLabel() {
                CommandId = this.CommandId, 
                Margin = new Thickness(5, 0, 0, 0)
            }) {
                BaselineAlignment = BaselineAlignment.Center
            });
        }
    }

    public void OnOpened(Control owner, IContextData data) {
        this.isToolTipOpen = true;
        this.GenerateInlines();
    }

    public void OnClosed(Control owner) {
        this.isToolTipOpen = false;
        this.InvalidateInlines();
    }
}