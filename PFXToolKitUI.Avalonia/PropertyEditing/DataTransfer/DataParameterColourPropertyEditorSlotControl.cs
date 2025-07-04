// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Utils.Commands;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;

public class DataParameterColourPropertyEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    public new DataParameterColourPropertyEditorSlot? SlotModel => (DataParameterColourPropertyEditorSlot?) base.SlotControl?.Model;

    private Rectangle? myRectangle;
    private SKColor myColour;
    private static readonly AsyncRelayCommand<DataParameterColourPropertyEditorSlotControl> choseColourCommand;

    public DataParameterColourPropertyEditorSlotControl() {
    }

    static DataParameterColourPropertyEditorSlotControl() {
        choseColourCommand = new AsyncRelayCommand<DataParameterColourPropertyEditorSlotControl>(async (x) => {
            SKColor? colour = await IColourPickerDialogService.Instance.PickColourAsync(x!.myColour);
            if (colour.HasValue) {
                x.myColour = colour.Value;
                x.OnControlValueChanged();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.myRectangle = e.NameScope.GetTemplateChild<Rectangle>("PART_Rectangle");
        this.myRectangle.PointerPressed += this.OnRectangleClicked;
    }

    private void OnRectangleClicked(object? sender, PointerPressedEventArgs e) => choseColourCommand.Execute(this);

    protected override void OnConnected() {
        base.OnConnected();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
    }

    protected override void UpdateControlValue() {
        this.myColour = this.SlotModel!.Value;
        SKColor c = this.myColour;
        this.myRectangle!.Fill = new ImmutableSolidColorBrush(new Color(c.Alpha, c.Red, c.Green, c.Blue));
    }

    protected override void UpdateModelValue() {
        this.SlotModel!.Value = this.myColour;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.myRectangle!.IsEnabled = canEdit;
    }
}