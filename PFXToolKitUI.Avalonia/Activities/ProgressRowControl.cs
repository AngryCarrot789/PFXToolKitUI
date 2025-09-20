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
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Avalonia.Activities;

public class ProgressRowControl : TemplatedControl {
    public static readonly StyledProperty<IActivityProgress?> ActivityProgressProperty = AvaloniaProperty.Register<ProgressRowControl, IActivityProgress?>(nameof(ActivityProgress));
    public static readonly StyledProperty<bool> ShowCaptionProperty = AvaloniaProperty.Register<ProgressRowControl, bool>(nameof(ShowCaption), true);

    public IActivityProgress? ActivityProgress {
        get => this.GetValue(ActivityProgressProperty);
        set => this.SetValue(ActivityProgressProperty, value);
    }

    public bool ShowCaption {
        get => this.GetValue(ShowCaptionProperty);
        set => this.SetValue(ShowCaptionProperty, value);
    }

    private TextBlock? PART_Header;
    private ProgressBar? PART_ProgressBar;
    private TextBlock? PART_Footer;

    private readonly IBinder<IActivityProgress> binderCaption = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.CaptionChanged), (b) => ((ProgressRowControl) b.Control).PART_Header!.Text = b.Model.Caption);
    private readonly IBinder<IActivityProgress> binderText = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.TextChanged), (b) => ((ProgressRowControl) b.Control).PART_Footer!.Text = b.Model.Text);
    private readonly IBinder<IActivityProgress> binderIsIndeterminate = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.IsIndeterminateChanged), (b) => ((ProgressRowControl) b.Control).PART_ProgressBar!.IsIndeterminate = b.Model.IsIndeterminate);
    private readonly IBinder<CompletionState> binderCompletionValue = new EventUpdateBinder<CompletionState>(nameof(CompletionState.CompletionValueChanged), (b) => ((ProgressRowControl) b.Control).PART_ProgressBar!.Value = b.Model.TotalCompletion);

    public ProgressRowControl() {
    }

    static ProgressRowControl() {
        ActivityProgressProperty.Changed.AddClassHandler<ProgressRowControl, IActivityProgress?>((s, e) => s.OnActivityProgressChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ShowCaptionProperty.Changed.AddClassHandler<ProgressRowControl, bool>((s, e) => {
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

        this.binderCaption.AttachControl(this);
        this.binderText.AttachControl(this);
        this.binderIsIndeterminate.AttachControl(this);
        this.binderCompletionValue.AttachControl(this);
    }

    private void OnActivityProgressChanged(IActivityProgress? oldTask, IActivityProgress? newTask) {
        this.binderCaption.SwitchModel(newTask);
        this.binderText.SwitchModel(newTask);
        this.binderIsIndeterminate.SwitchModel(newTask);
        this.binderCompletionValue.SwitchModel(newTask?.CompletionState);
    }
}