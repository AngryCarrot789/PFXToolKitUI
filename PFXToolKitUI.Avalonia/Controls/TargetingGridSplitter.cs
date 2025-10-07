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
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace PFXToolKitUI.Avalonia.Controls;

/// <summary>
/// A clone of <see cref="GridSplitter"/> that supports targeting a specific <see cref="Grid"/> and row/column, without being parented to it at all
/// </summary>
public class TargetingGridSplitter : Thumb {
    /// <summary>
    /// Defines the <see cref="ResizeDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<GridResizeDirection> ResizeDirectionProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, GridResizeDirection>(nameof(ResizeDirection));

    /// <summary>
    /// Defines the <see cref="ResizeBehavior"/> property.
    /// </summary>
    public static readonly StyledProperty<GridResizeBehavior> ResizeBehaviorProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, GridResizeBehavior>(nameof(ResizeBehavior));

    /// <summary>
    /// Defines the <see cref="ShowsPreview"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowsPreviewProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, bool>(nameof(ShowsPreview));

    /// <summary>
    /// Defines the <see cref="KeyboardIncrement"/> property.
    /// </summary>
    public static readonly StyledProperty<double> KeyboardIncrementProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, double>(nameof(KeyboardIncrement), 10d);

    /// <summary>
    /// Defines the <see cref="DragIncrement"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DragIncrementProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, double>(nameof(DragIncrement), 1d);

    /// <summary>
    /// Defines the <see cref="PreviewContent"/> property.
    /// </summary>
    public static readonly StyledProperty<ITemplate<Control>> PreviewContentProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, ITemplate<Control>>(nameof(PreviewContent));

    public static readonly StyledProperty<Grid?> TargetGridProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, Grid?>(nameof(TargetGrid));

    public static readonly StyledProperty<int> TargetColumnProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, int>(nameof(TargetColumn), validate: (Func<int, bool>) (v => v >= 0));

    public static readonly StyledProperty<int> TargetRowProperty =
        AvaloniaProperty.Register<TargetingGridSplitter, int>(nameof(TargetRow), validate: (Func<int, bool>) (v => v >= 0));

    private static readonly Cursor s_columnSplitterCursor = new Cursor(StandardCursorType.SizeWestEast);
    private static readonly Cursor s_rowSplitterCursor = new Cursor(StandardCursorType.SizeNorthSouth);

    private ResizeData? myResizeData;
    private bool _isFocusEngaged;

    /// <summary>
    /// Indicates whether the Splitter resizes the Columns, Rows, or Both.
    /// </summary>
    public GridResizeDirection ResizeDirection {
        get => this.GetValue(ResizeDirectionProperty);
        set => this.SetValue(ResizeDirectionProperty, value);
    }

    /// <summary>
    /// Indicates which Columns or Rows the Splitter resizes.
    /// </summary>
    public GridResizeBehavior ResizeBehavior {
        get => this.GetValue(ResizeBehaviorProperty);
        set => this.SetValue(ResizeBehaviorProperty, value);
    }

    /// <summary>
    /// Indicates whether to Preview the column resizing without updating layout.
    /// </summary>
    public bool ShowsPreview {
        get => this.GetValue(ShowsPreviewProperty);
        set => this.SetValue(ShowsPreviewProperty, value);
    }

    /// <summary>
    /// The Distance to move the splitter when pressing the keyboard arrow keys.
    /// </summary>
    public double KeyboardIncrement {
        get => this.GetValue(KeyboardIncrementProperty);
        set => this.SetValue(KeyboardIncrementProperty, value);
    }

    /// <summary>
    /// Restricts splitter to move a multiple of the specified units.
    /// </summary>
    public double DragIncrement {
        get => this.GetValue(DragIncrementProperty);
        set => this.SetValue(DragIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets content that will be shown when <see cref="ShowsPreview"/> is enabled and user starts resize operation.
    /// </summary>
    public ITemplate<Control> PreviewContent {
        get => this.GetValue(PreviewContentProperty);
        set => this.SetValue(PreviewContentProperty, value);
    }

    public Grid? TargetGrid {
        get => this.GetValue(TargetGridProperty);
        set => this.SetValue(TargetGridProperty, value);
    }

    public int TargetColumn {
        get => this.GetValue(TargetColumnProperty);
        set => this.SetValue(TargetColumnProperty, value);
    }

    public int TargetRow {
        get => this.GetValue(TargetRowProperty);
        set => this.SetValue(TargetRowProperty, value);
    }

    /// <summary>
    /// Converts BasedOnAlignment direction to Rows, Columns, or Both depending on its width/height.
    /// </summary>
    internal GridResizeDirection GetEffectiveResizeDirection() {
        GridResizeDirection direction = this.ResizeDirection;

        if (direction != GridResizeDirection.Auto) {
            return direction;
        }

        // When HorizontalAlignment is Left, Right or Center, resize Columns.
        if (this.HorizontalAlignment != HorizontalAlignment.Stretch) {
            direction = GridResizeDirection.Columns;
        }
        else if (this.VerticalAlignment != VerticalAlignment.Stretch) {
            direction = GridResizeDirection.Rows;
        }
        else if (this.Bounds.Width <= this.Bounds.Height) // Fall back to Width vs Height.
        {
            direction = GridResizeDirection.Columns;
        }
        else {
            direction = GridResizeDirection.Rows;
        }

        return direction;
    }

    /// <summary>
    /// Convert BasedOnAlignment to Next/Prev/Both depending on alignment and Direction.
    /// </summary>
    private GridResizeBehavior GetEffectiveResizeBehavior(GridResizeDirection direction) {
        GridResizeBehavior resizeBehavior = this.ResizeBehavior;

        if (resizeBehavior == GridResizeBehavior.BasedOnAlignment) {
            if (direction == GridResizeDirection.Columns) {
                switch (this.HorizontalAlignment) {
                    case HorizontalAlignment.Left:  resizeBehavior = GridResizeBehavior.PreviousAndCurrent; break;
                    case HorizontalAlignment.Right: resizeBehavior = GridResizeBehavior.CurrentAndNext; break;
                    default:                        resizeBehavior = GridResizeBehavior.PreviousAndNext; break;
                }
            }
            else {
                switch (this.VerticalAlignment) {
                    case VerticalAlignment.Top:    resizeBehavior = GridResizeBehavior.PreviousAndCurrent; break;
                    case VerticalAlignment.Bottom: resizeBehavior = GridResizeBehavior.CurrentAndNext; break;
                    default:                       resizeBehavior = GridResizeBehavior.PreviousAndNext; break;
                }
            }
        }

        return resizeBehavior;
    }

    /// <summary>
    /// Removes preview adorner from the grid.
    /// </summary>
    private void RemovePreviewAdorner() {
        if (this.myResizeData?.Adorner != null) {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this)!;
            layer.Children.Remove(this.myResizeData.Adorner);
        }
    }

    /// <summary>
    /// Initialize the data needed for resizing.
    /// </summary>
    private void InitializeData(bool showsPreview) {
        // If not in a grid or can't resize, do nothing.
        Grid? grid = this.TargetGrid;
        if (grid == null) {
            return;
        }

        GridResizeDirection resizeDirection = this.GetEffectiveResizeDirection();

        // Setup data used for resizing.
        this.myResizeData = new ResizeData {
            Grid = grid,
            ShowsPreview = showsPreview,
            ResizeDirection = resizeDirection,
            SplitterLength = Math.Min(this.Bounds.Width, this.Bounds.Height),
            ResizeBehavior = this.GetEffectiveResizeBehavior(resizeDirection),
            Scaling = (this.VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1,
        };

        // Store the rows and columns to resize on drag events.
        if (!this.SetupDefinitionsToResize()) {
            // Unable to resize, clear data.
            this.myResizeData = null;
            return;
        }

        // Setup the preview in the adorner if ShowsPreview is true.
        this.SetupPreviewAdorner();
    }

    /// <summary>
    /// Returns true if TargetingGridSplitter can resize rows/columns.
    /// </summary>
    private bool SetupDefinitionsToResize() {
        bool horizontal = this.myResizeData!.ResizeDirection == GridResizeDirection.Columns;
        int splitterIndex = horizontal ? this.TargetColumn : this.TargetRow;

        // Select the columns based on behavior.
        int index1, index2;

        switch (this.myResizeData.ResizeBehavior) {
            case GridResizeBehavior.PreviousAndCurrent:
                // Get current and previous.
                index1 = splitterIndex - 1;
                index2 = splitterIndex;
                break;
            case GridResizeBehavior.CurrentAndNext:
                // Get current and next.
                index1 = splitterIndex;
                index2 = splitterIndex + 1;
                break;
            default: // GridResizeBehavior.PreviousAndNext.
                // Get previous and next.
                index1 = splitterIndex - 1;
                index2 = splitterIndex + 1;
                break;
        }

        // Get count of rows/columns in the resize direction.
        int count = this.myResizeData.ResizeDirection == GridResizeDirection.Columns ? this.myResizeData.Grid!.ColumnDefinitions.Count : this.myResizeData.Grid!.RowDefinitions.Count;

        if (index1 >= 0 && index2 < count) {
            this.myResizeData.SplitterIndex = splitterIndex;

            this.myResizeData.Definition1Index = index1;
            this.myResizeData.Definition1 = GetGridDefinition(this.myResizeData.Grid, index1, this.myResizeData.ResizeDirection);
            this.myResizeData.OriginalDefinition1Length = GetUserSizeValue(this.myResizeData.Definition1); // Save Size if user cancels.
            this.myResizeData.OriginalDefinition1ActualLength = GetActualLength(this.myResizeData.Definition1);

            this.myResizeData.Definition2Index = index2;
            this.myResizeData.Definition2 = GetGridDefinition(this.myResizeData.Grid, index2, this.myResizeData.ResizeDirection);
            this.myResizeData.OriginalDefinition2Length = GetUserSizeValue(this.myResizeData.Definition2); // Save Size if user cancels.
            this.myResizeData.OriginalDefinition2ActualLength = GetActualLength(this.myResizeData.Definition2);

            // Determine how to resize the columns.
            bool isStar1 = IsStar(this.myResizeData.Definition1);
            bool isStar2 = IsStar(this.myResizeData.Definition2);

            if (isStar1 && isStar2) {
                // If they are both stars, resize both.
                this.myResizeData.SplitBehavior = SplitBehavior.Split;
            }
            else {
                // One column is fixed width, resize the first one that is fixed.
                this.myResizeData.SplitBehavior = !isStar1 ? SplitBehavior.Resize1 : SplitBehavior.Resize2;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Create the preview adorner and add it to the adorner layer.
    /// </summary>
    private void SetupPreviewAdorner() {
        if (this.myResizeData!.ShowsPreview) {
            // Get the adorner layer and add an adorner to it.
            AdornerLayer? adornerLayer = AdornerLayer.GetAdornerLayer(this.myResizeData.Grid!);

            ITemplate<Control>? previewContent = this.PreviewContent;

            // Can't display preview.
            if (adornerLayer == null) {
                return;
            }

            Control? builtPreviewContent = previewContent?.Build();

            this.myResizeData.Adorner = new PreviewAdorner(builtPreviewContent);

            AdornerLayer.SetAdornedElement(this.myResizeData.Adorner, this);
            AdornerLayer.SetIsClipEnabled(this.myResizeData.Adorner, false);

            adornerLayer.Children.Add(this.myResizeData.Adorner);

            // Get constraints on preview's translation.
            this.GetDeltaConstraints(out this.myResizeData.MinChange, out this.myResizeData.MaxChange);
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e) {
        base.OnPointerEntered(e);

        GridResizeDirection direction = this.GetEffectiveResizeDirection();

        switch (direction) {
            case GridResizeDirection.Columns: this.Cursor = s_columnSplitterCursor; break;
            case GridResizeDirection.Rows:    this.Cursor = s_rowSplitterCursor; break;
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
        base.OnLostFocus(e);

        if (this.myResizeData != null) {
            this.CancelResize();
        }
    }

    protected override void OnDragStarted(VectorEventArgs e) {
        base.OnDragStarted(e);

        // TODO: Looks like that sometimes thumb will raise multiple drag started events.
        // Debug.Assert(_resizeData == null, "_resizeData is not null, DragCompleted was not called");

        if (this.myResizeData != null) {
            return;
        }

        this.InitializeData(this.ShowsPreview);
    }

    protected override void OnDragDelta(VectorEventArgs e) {
        base.OnDragDelta(e);

        if (this.myResizeData != null) {
            double horizontalChange = e.Vector.X;
            double verticalChange = e.Vector.Y;

            // Round change to nearest multiple of DragIncrement.
            double dragIncrement = this.DragIncrement;
            horizontalChange = Math.Round(horizontalChange / dragIncrement) * dragIncrement;
            verticalChange = Math.Round(verticalChange / dragIncrement) * dragIncrement;

            if (this.myResizeData.ShowsPreview) {
                // Set the Translation of the Adorner to the distance from the thumb.
                if (this.myResizeData.ResizeDirection == GridResizeDirection.Columns) {
                    this.myResizeData.Adorner!.OffsetX = Math.Min(
                        Math.Max(horizontalChange, this.myResizeData.MinChange),
                        this.myResizeData.MaxChange);
                }
                else {
                    this.myResizeData.Adorner!.OffsetY = Math.Min(
                        Math.Max(verticalChange, this.myResizeData.MinChange),
                        this.myResizeData.MaxChange);
                }
            }
            else {
                // Directly update the grid.
                this.MoveSplitter(horizontalChange, verticalChange);
            }
        }
    }

    protected override void OnDragCompleted(VectorEventArgs e) {
        base.OnDragCompleted(e);

        if (this.myResizeData != null) {
            if (this.myResizeData.ShowsPreview) {
                // Update the grid.
                this.MoveSplitter(this.myResizeData.Adorner!.OffsetX, this.myResizeData.Adorner.OffsetY);
                this.RemovePreviewAdorner();
            }

            this.myResizeData = null;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        bool usingXyNavigation = this.IsAllowedXYNavigationMode(e.KeyDeviceType);
        bool allowArrowKeys = this._isFocusEngaged || !usingXyNavigation;

        switch (e.Key) {
            case Key.Enter when usingXyNavigation:
                this._isFocusEngaged = !this._isFocusEngaged;
                e.Handled = true;
                break;
            case Key.Escape:
                this._isFocusEngaged = false;
                if (this.myResizeData != null) {
                    this.CancelResize();
                    e.Handled = true;
                }

                break;

            case Key.Left when allowArrowKeys:  e.Handled = this.KeyboardMoveSplitter(-this.KeyboardIncrement, 0); break;
            case Key.Right when allowArrowKeys: e.Handled = this.KeyboardMoveSplitter(this.KeyboardIncrement, 0); break;
            case Key.Up when allowArrowKeys:    e.Handled = this.KeyboardMoveSplitter(0, -this.KeyboardIncrement); break;
            case Key.Down when allowArrowKeys:  e.Handled = this.KeyboardMoveSplitter(0, this.KeyboardIncrement); break;
        }
    }

    /// <summary>
    /// Cancels the resize operation.
    /// </summary>
    private void CancelResize() {
        // Restore original column/row lengths.
        if (this.myResizeData!.ShowsPreview) {
            this.RemovePreviewAdorner();
        }
        else // Reset the columns/rows lengths to the saved values.
        {
            SetDefinitionLength(this.myResizeData.Definition1!, this.myResizeData.OriginalDefinition1Length);
            SetDefinitionLength(this.myResizeData.Definition2!, this.myResizeData.OriginalDefinition2Length);
        }

        this.myResizeData = null;
    }

    /// <summary>
    /// Returns true if the row/column has a star length.
    /// </summary>
    private static bool IsStar(DefinitionBase definition) {
        return (definition is ColumnDefinition cd ? cd.Width : ((RowDefinition) definition).Height).IsStar;
    }

    private static GridLength GetUserSizeValue(DefinitionBase d) => d is ColumnDefinition c ? c.Width : ((RowDefinition) d).Height;
    private static double GetUserMinSizeValue(DefinitionBase d) => d is ColumnDefinition c ? c.MinWidth : ((RowDefinition) d).MinHeight;
    private static double GetUserMaxSizeValue(DefinitionBase d) => d is ColumnDefinition c ? c.MaxWidth : ((RowDefinition) d).MaxHeight;

    /// <summary>
    /// Gets Column or Row definition at index from grid based on resize direction.
    /// </summary>
    private static DefinitionBase GetGridDefinition(Grid grid, int index, GridResizeDirection direction) {
        return direction == GridResizeDirection.Columns ? grid.ColumnDefinitions[index] : grid.RowDefinitions[index];
    }

    /// <summary>
    /// Retrieves the ActualWidth or ActualHeight of the definition depending on its type Column or Row.
    /// </summary>
    private static double GetActualLength(DefinitionBase definition) {
        ColumnDefinition? column = definition as ColumnDefinition;

        return column?.ActualWidth ?? ((RowDefinition) definition).ActualHeight;
    }

    /// <summary>
    /// Gets Column or Row definition at index from grid based on resize direction.
    /// </summary>
    private static void SetDefinitionLength(DefinitionBase definition, GridLength length) {
        definition.SetValue(
            definition is ColumnDefinition ? ColumnDefinition.WidthProperty : RowDefinition.HeightProperty, length);
    }

    /// <summary>
    /// Get the minimum and maximum Delta can be given definition constraints (MinWidth/MaxWidth).
    /// </summary>
    private void GetDeltaConstraints(out double minDelta, out double maxDelta) {
        double definition1Len = GetActualLength(this.myResizeData!.Definition1!);
        double definition1Min = GetUserMinSizeValue(this.myResizeData.Definition1!);
        double definition1Max = GetUserMaxSizeValue(this.myResizeData.Definition1!);

        double definition2Len = GetActualLength(this.myResizeData.Definition2!);
        double definition2Min = GetUserMinSizeValue(this.myResizeData.Definition2!);
        double definition2Max = GetUserMaxSizeValue(this.myResizeData.Definition2!);

        // Set MinWidths to be greater than width of splitter.
        if (this.myResizeData.SplitterIndex == this.myResizeData.Definition1Index) {
            definition1Min = Math.Max(definition1Min, this.myResizeData.SplitterLength);
        }
        else if (this.myResizeData.SplitterIndex == this.myResizeData.Definition2Index) {
            definition2Min = Math.Max(definition2Min, this.myResizeData.SplitterLength);
        }

        // Determine the minimum and maximum the columns can be resized.
        minDelta = -Math.Min(definition1Len - definition1Min, definition2Max - definition2Len);
        maxDelta = Math.Min(definition1Max - definition1Len, definition2Len - definition2Min);
    }

    /// <summary>
    /// Sets the length of definition1 and definition2.
    /// </summary>
    private void SetLengths(double definition1Pixels, double definition2Pixels) {
        // For the case where both definition1 and 2 are stars, update all star values to match their current pixel values.
        if (this.myResizeData!.SplitBehavior == SplitBehavior.Split) {
            IAvaloniaReadOnlyList<DefinitionBase> definitions = this.myResizeData.ResizeDirection == GridResizeDirection.Columns ? this.myResizeData.Grid!.ColumnDefinitions : (IAvaloniaReadOnlyList<DefinitionBase>) this.myResizeData.Grid!.RowDefinitions;

            int definitionsCount = definitions.Count;

            for (int i = 0; i < definitionsCount; i++) {
                DefinitionBase definition = definitions[i];

                // For each definition, if it is a star, set is value to ActualLength in stars
                // This makes 1 star == 1 pixel in length
                if (i == this.myResizeData.Definition1Index) {
                    SetDefinitionLength(definition, new GridLength(definition1Pixels, GridUnitType.Star));
                }
                else if (i == this.myResizeData.Definition2Index) {
                    SetDefinitionLength(definition, new GridLength(definition2Pixels, GridUnitType.Star));
                }
                else if (IsStar(definition)) {
                    SetDefinitionLength(definition, new GridLength(GetActualLength(definition), GridUnitType.Star));
                }
            }
        }
        else if (this.myResizeData.SplitBehavior == SplitBehavior.Resize1) {
            SetDefinitionLength(this.myResizeData.Definition1!, new GridLength(definition1Pixels));
        }
        else {
            SetDefinitionLength(this.myResizeData.Definition2!, new GridLength(definition2Pixels));
        }
    }

    /// <summary>
    /// Move the splitter by the given Delta's in the horizontal and vertical directions.
    /// </summary>
    private void MoveSplitter(double horizontalChange, double verticalChange) {
        Debug.Assert(this.myResizeData != null, "_resizeData should not be null when calling MoveSplitter");

        // Calculate the offset to adjust the splitter.  If layout rounding is enabled, we
        // need to round to an integer physical pixel value to avoid round-ups of children that
        // expand the bounds of the Grid.  In practice this only happens in high dpi because
        // horizontal/vertical offsets here are never fractional (they correspond to mouse movement
        // across logical pixels).  Rounding error only creeps in when converting to a physical
        // display with something other than the logical 96 dpi.
        double delta = this.myResizeData.ResizeDirection == GridResizeDirection.Columns ? horizontalChange : verticalChange;

        if (this.UseLayoutRounding) {
            delta = LayoutHelper.RoundLayoutValue(delta, LayoutHelper.GetLayoutScale(this));
        }

        DefinitionBase? definition1 = this.myResizeData.Definition1;
        DefinitionBase? definition2 = this.myResizeData.Definition2;

        if (definition1 != null && definition2 != null) {
            double actualLength1 = GetActualLength(definition1);
            double actualLength2 = GetActualLength(definition2);
            double pixelLength = 1 / this.myResizeData.Scaling;
            double epsilon = pixelLength + LayoutHelper.LayoutEpsilon;

            // When splitting, Check to see if the total pixels spanned by the definitions 
            // is the same as before starting resize. If not cancel the drag. We need to account for
            // layout rounding here, so ignore differences of less than a device pixel to avoid problems
            // that WPF has, such as https://stackoverflow.com/questions/28464843.
            if (this.myResizeData.SplitBehavior == SplitBehavior.Split &&
                !MathUtilities.AreClose(
                    actualLength1 + actualLength2,
                    this.myResizeData.OriginalDefinition1ActualLength + this.myResizeData.OriginalDefinition2ActualLength, epsilon)) {
                this.CancelResize();

                return;
            }

            this.GetDeltaConstraints(out double min, out double max);

            // Constrain Delta to Min/MaxWidth of columns
            delta = Math.Min(Math.Max(delta, min), max);

            double definition1LengthNew = actualLength1 + delta;
            double definition2LengthNew = actualLength1 + actualLength2 - definition1LengthNew;

            this.SetLengths(definition1LengthNew, definition2LengthNew);
        }
    }

    /// <summary>
    /// Move the splitter using the Keyboard (Don't show preview).
    /// </summary>
    private bool KeyboardMoveSplitter(double horizontalChange, double verticalChange) {
        // If moving with the mouse, ignore keyboard motion.
        if (this.myResizeData != null) {
            return false; // Don't handle the event.
        }

        // Don't show preview.
        this.InitializeData(false);

        // Check that we are actually able to resize.
        if (this.myResizeData == null) {
            return false; // Don't handle the event.
        }

        this.MoveSplitter(horizontalChange, verticalChange);

        this.myResizeData = null;

        return true;
    }

    /// <summary>
    /// This adorner draws the preview for the <see cref="TargetingGridSplitter"/>.
    /// It also positions the adorner.
    /// </summary>
    private sealed class PreviewAdorner : Decorator {
        private readonly TranslateTransform _translation;
        private readonly Decorator _decorator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1012", Justification = "Private object")]
        public PreviewAdorner(Control? previewControl) {
            // Add a decorator to perform translations.
            this._translation = new TranslateTransform();

            this._decorator = new Decorator {
                Child = previewControl,
                RenderTransform = this._translation
            };

            this.Child = this._decorator;
        }

        /// <summary>
        /// The Preview's Offset in the X direction from the TargetingGridSplitter.
        /// </summary>
        public double OffsetX {
            get => this._translation.X;
            set => this._translation.X = value;
        }

        /// <summary>
        /// The Preview's Offset in the Y direction from the TargetingGridSplitter.
        /// </summary>
        public double OffsetY {
            get => this._translation.Y;
            set => this._translation.Y = value;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            // Adorners always get clipped to the owner control. In this case we want
            // to constrain size to the splitter size but draw on top of the parent grid.
            this.Clip = null;

            return base.ArrangeOverride(finalSize);
        }
    }

    /// <summary>
    /// <see cref="TargetingGridSplitter"/> has special Behavior when columns are fixed.
    /// If the left column is fixed, splitter will only resize that column.
    /// Else if the right column is fixed, splitter will only resize the right column.
    /// </summary>
    private enum SplitBehavior {
        /// <summary>
        /// Both columns/rows are star lengths.
        /// </summary>
        Split,

        /// <summary>
        /// Resize 1 only.
        /// </summary>
        Resize1,

        /// <summary>
        /// Resize 2 only.
        /// </summary>
        Resize2
    }

    /// <summary>
    /// Stores data during the resizing operation.
    /// </summary>
    private class ResizeData {
        public bool ShowsPreview;
        public PreviewAdorner? Adorner;

        // The constraints to keep the Preview within valid ranges.
        public double MinChange;
        public double MaxChange;

        // The grid to Resize.
        public Grid? Grid;

        // Cache of Resize Direction and Behavior.
        public GridResizeDirection ResizeDirection;
        public GridResizeBehavior ResizeBehavior;

        // The columns/rows to resize.
        public DefinitionBase? Definition1;
        public DefinitionBase? Definition2;

        // Are the columns/rows star lengths.
        public SplitBehavior SplitBehavior;

        // The index of the splitter.
        public int SplitterIndex;

        // The indices of the columns/rows.
        public int Definition1Index;
        public int Definition2Index;

        // The original lengths of Definition1 and Definition2 (to restore lengths if user cancels resize).
        public GridLength OriginalDefinition1Length;
        public GridLength OriginalDefinition2Length;
        public double OriginalDefinition1ActualLength;
        public double OriginalDefinition2ActualLength;

        // The minimum of Width/Height of Splitter.  Used to ensure splitter 
        // isn't hidden by resizing a row/column smaller than the splitter.
        public double SplitterLength;

        // The current layout scaling factor.
        public double Scaling;
    }
}

internal static class XYFocusHelpers {
    internal static bool IsAllowedXYNavigationMode(this InputElement visual, KeyDeviceType? keyDeviceType) {
        return IsAllowedXYNavigationMode(XYFocus.GetNavigationModes(visual), keyDeviceType);
    }

    private static bool IsAllowedXYNavigationMode(XYFocusNavigationModes modes, KeyDeviceType? keyDeviceType) {
        if (!keyDeviceType.HasValue)
            return !modes.Equals(XYFocusNavigationModes.Disabled);

        switch (keyDeviceType.GetValueOrDefault()) {
            case KeyDeviceType.Keyboard: return modes.HasFlag(XYFocusNavigationModes.Keyboard);
            case KeyDeviceType.Gamepad:  return modes.HasFlag(XYFocusNavigationModes.Gamepad);
            case KeyDeviceType.Remote:   return modes.HasFlag(XYFocusNavigationModes.Remote);
            default:                     throw new ArgumentOutOfRangeException(nameof(keyDeviceType), keyDeviceType, null);
        }
    }

    internal static InputElement? FindXYSearchRoot(this InputElement visual, KeyDeviceType? keyDeviceType) {
        InputElement visual1 = visual;
        for (InputElement? ancestorOfType = visual.FindAncestorOfType<InputElement>(); ancestorOfType != null && ancestorOfType.IsAllowedXYNavigationMode(keyDeviceType); ancestorOfType = visual1.FindAncestorOfType<InputElement>())
            visual1 = ancestorOfType;
        return visual1;
    }
}