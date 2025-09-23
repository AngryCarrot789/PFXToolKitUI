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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Avalonia.Activities;

public class SubActivityProgressRowControl : TemplatedControl {
    public static readonly StyledProperty<SubActivity?> SubActivityProperty = AvaloniaProperty.Register<SubActivityProgressRowControl, SubActivity?>(nameof(SubActivity));
    public static readonly StyledProperty<bool> ShowCaptionProperty = AvaloniaProperty.Register<SubActivityProgressRowControl, bool>(nameof(ShowCaption), true);

    public SubActivity? SubActivity {
        get => this.GetValue(SubActivityProperty);
        set => this.SetValue(SubActivityProperty, value);
    }

    public bool ShowCaption {
        get => this.GetValue(ShowCaptionProperty);
        set => this.SetValue(ShowCaptionProperty, value);
    }

    private TextBlock? PART_Header;
    private ProgressBar? PART_ProgressBar;
    private TextBlock? PART_Footer;
    private Button? PART_CancelActivityButton;

    private readonly IBinder<IActivityProgress> binderCaption = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.CaptionChanged), (b) => ((SubActivityProgressRowControl) b.Control).PART_Header!.Text = b.Model.Caption);
    private readonly IBinder<IActivityProgress> binderText = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.TextChanged), (b) => ((SubActivityProgressRowControl) b.Control).PART_Footer!.Text = b.Model.Text);
    private readonly IBinder<IActivityProgress> binderIsIndeterminate = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.IsIndeterminateChanged), (b) => ((SubActivityProgressRowControl) b.Control).PART_ProgressBar!.IsIndeterminate = b.Model.IsIndeterminate);
    private readonly IBinder<CompletionState> binderCompletionValue = new EventUpdateBinder<CompletionState>(nameof(CompletionState.CompletionValueChanged), (b) => ((SubActivityProgressRowControl) b.Control).PART_ProgressBar!.Value = b.Model.TotalCompletion);

    public SubActivityProgressRowControl() {
    }

    static SubActivityProgressRowControl() {
        SubActivityProperty.Changed.AddClassHandler<SubActivityProgressRowControl, SubActivity?>((s, e) => s.OnSubActivityChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ShowCaptionProperty.Changed.AddClassHandler<SubActivityProgressRowControl, bool>((s, e) => {
            if (s.PART_Header != null)
                s.PART_Header.IsVisible = e.NewValue.GetValueOrDefault();
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Header = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Header));
        this.PART_Header.IsVisible = this.ShowCaption;
        this.PART_ProgressBar = e.NameScope.GetTemplateChild<ProgressBar>(nameof(this.PART_ProgressBar));
        this.PART_Footer = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Footer));
        this.PART_CancelActivityButton = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_CancelActivityButton));
        this.PART_CancelActivityButton.Click += this.PART_CancelActivityButtonOnClick;

        this.binderCaption.AttachControl(this);
        this.binderText.AttachControl(this);
        this.binderIsIndeterminate.AttachControl(this);
        this.binderCompletionValue.AttachControl(this);
        this.UpdateCancelButton();
    }

    private void OnSubActivityChanged(SubActivity? oldActivity, SubActivity? newActivity) {
        this.binderCaption.SwitchModel(newActivity?.Progress);
        this.binderText.SwitchModel(newActivity?.Progress);
        this.binderIsIndeterminate.SwitchModel(newActivity?.Progress);
        this.binderCompletionValue.SwitchModel(newActivity?.Progress.CompletionState);
        if (newActivity.HasValue) {
            this.UpdateCancelButton();
        }
    }
    
    private void UpdateCancelButton() {
        if (this.PART_CancelActivityButton != null) {
            SubActivity? activity = this.SubActivity;
            this.PART_CancelActivityButton!.IsEnabled = activity?.Cancellation != null;
            this.PART_CancelActivityButton!.IsVisible = activity?.Cancellation != null;
        }
    }

    private void PART_CancelActivityButtonOnClick(object? sender, RoutedEventArgs e) {
        this.PART_CancelActivityButton!.IsEnabled = false;
        this.SubActivity?.Cancellation?.Cancel();
    }
}