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
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.Interactivity;

public static class ButtonClickContextMenu {
    public static readonly AttachedProperty<bool> ShowOnLeftClickProperty = AvaloniaProperty.RegisterAttached<Button, bool>("ShowOnLeftClick", typeof(ButtonClickContextMenu));
    private static readonly AttachedProperty<bool> IsProcessingClickProperty = AvaloniaProperty.RegisterAttached<Button, bool>("IsProcessingClick", typeof(ButtonClickContextMenu));

    public static void SetShowOnLeftClick(Button obj, bool value) => obj.SetValue(ShowOnLeftClickProperty, value);
    public static bool GetShowOnLeftClick(Button obj) => obj.GetValue(ShowOnLeftClickProperty);

    static ButtonClickContextMenu() {
        // Need to handle it as class handlers because the instance versions are called after static handlers,
        // and custom code might only fire the event after other custom code
        Button.ClickEvent.AddClassHandler<Button>(ButtonOnClick, RoutingStrategies.Bubble);
        Control.ContextRequestedEvent.AddClassHandler<Button>(ButtonContextRequested, RoutingStrategies.Bubble);
    }

    private static void ButtonOnClick(object? sender, RoutedEventArgs e) {
        Button button = (Button) sender!;
        if (!GetShowOnLeftClick(button) || button.GetValue(IsProcessingClickProperty)) {
            return;
        }

        button.SetValue(IsProcessingClickProperty, true);

        try {
            ContextRequestedEventArgs args = new ContextRequestedEventArgs();
            button.RaiseEvent(args);
            e.Handled = args.Handled;
        }
        finally {
            button.SetValue(IsProcessingClickProperty, false);
        }
    }

    private static void ButtonContextRequested(object? sender, ContextRequestedEventArgs e) {
        Button button = (Button) sender!;
        if (GetShowOnLeftClick(button) && !button.GetValue(IsProcessingClickProperty)) {
            e.Handled = true;
        }
    }
}