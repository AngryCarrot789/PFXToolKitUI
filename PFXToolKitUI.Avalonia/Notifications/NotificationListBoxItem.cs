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

using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using PFXToolKitUI.Avalonia.Activities;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Notifications;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Notifications;

public class NotificationListBoxItem : ModelBasedListBoxItem<Notification> {
    public static readonly ModelControlRegistry<Notification, Control> ModelControlRegistry;

    public static readonly StyledProperty<string?> CaptionProperty = AvaloniaProperty.Register<NotificationListBoxItem, string?>(nameof(Caption));

    public string? Caption {
        get => this.GetValue(CaptionProperty);
        set => this.SetValue(CaptionProperty, value);
    }

    private Button? PART_Close;
    private Panel? PART_ActionPanel;
    private ObservableItemProcessorIndexing<NotificationCommand>? processor;

    private Animation? animation;

    public NotificationListBoxItem() {
    }

    static NotificationListBoxItem() {
        ModelControlRegistry = new ModelControlRegistry<Notification, Control>();
        ModelControlRegistry.RegisterType<TextNotification>((b) => new NotificationTextBlock(b));
        ModelControlRegistry.RegisterType<ActivityNotification>((b) => new NotificationActivityListItem(b));
    }

    protected override void OnPointerEntered(PointerEventArgs e) {
        base.OnPointerEntered(e);
        this.Model?.CancelAutoHide();
    }

    protected override void OnPointerExited(PointerEventArgs e) {
        base.OnPointerExited(e);
        // this.Model?.StartAutoHide();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) {
            this.Model?.Close();
        }
    }

    private class NotificationTextBlock : TextBlock, INotificationContent {
        private readonly TextNotification notification;
        protected override Type StyleKeyOverride => typeof(TextBlock);

        public NotificationTextBlock(TextNotification textNotification) {
            this.notification = textNotification;
            this.Text = textNotification.Text;
        }

        private void OnTextChanged(Notification sender) {
            Debug.Assert(this.notification == sender);
            
            this.Text = this.notification.Text;
        }

        public void OnShown() {
            this.Text = this.notification.Text;
            this.notification.TextChanged += this.OnTextChanged;
        }

        public void OnHidden() {
            this.notification.TextChanged -= this.OnTextChanged;
        }
    }

    private class NotificationActivityListItem : ActivityListItem, INotificationContent {
        protected override Type StyleKeyOverride => typeof(ActivityListItem);

        private readonly ActivityNotification notification;

        public NotificationActivityListItem(ActivityNotification notification) {
            this.ShowCaption = false;

            this.notification = notification;
            if (!this.notification.ActivityTask.IsCompleted) {
                this.notification.ActivityTask.IsCompletedChanged += this.OnIsCompletedChanged;
            }
        }

        private void OnIsCompletedChanged(ActivityTask sender) {
            this.notification.ActivityTask.IsCompletedChanged -= this.OnIsCompletedChanged;
            this.notification.Close();
        }

        public void OnShown() {
            this.ActivityTask = this.notification.ActivityTask;
        }

        public void OnHidden() {
            this.ActivityTask = null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Close = e.NameScope.GetTemplateChild<Button>("PART_Close");
        this.PART_Close.Click += (sender, args) => this.Model?.Close();

        this.PART_ActionPanel = e.NameScope.GetTemplateChild<Panel>("PART_ActionPanel");
        this.processor?.AddExistingItems();
    }

    protected override void OnAddingToList() {
        this.Opacity = 1.0;
    }

    protected override void OnAddedToList() {
        this.Caption = this.Model!.Caption ?? "";
        this.Model.CaptionChanged += this.ModelOnCaptionChanged;
        this.Model.IsAutoHideActiveChanged += this.OnIsAutoHideActiveChanged;
        if (this.Model.IsAutoHideActive) {
            this.OnIsAutoHideActiveChanged(this.Model);
        }

        this.processor = ObservableItemProcessor.MakeIndexable(this.Model!.Commands, this.OnCommandInserted, this.OnCommandRemoved, this.OnCommandMoved);
        if (this.PART_ActionPanel != null)
            this.processor.AddExistingItems();

        if (ModelControlRegistry.TryGetNewInstance(this.Model!, out Control? control)) {
            this.Content = control;
            (control as INotificationContent)?.OnShown();
        }
    }

    protected override void OnRemovingFromList() {
        (this.Content as INotificationContent)?.OnHidden();
        this.Content = null;

        this.processor!.RemoveExistingItems();
        this.processor!.Dispose();

        this.Model!.CaptionChanged -= this.ModelOnCaptionChanged;
    }

    protected override void OnRemovedFromList() {
    }

    private void ModelOnCaptionChanged(Notification sender) {
        this.Caption = sender.Caption ?? "";
    }

    private void OnIsAutoHideActiveChanged(Notification sender) {
        if (!sender.IsAutoHideActive) {
            this.Opacity = sender.NotificationManager != null ? 1.0 : 0.0;
            return;
        }

        TimeSpan preExistingTime = DateTime.Now - sender.AutoHideStartTime;
        TimeSpan delay = sender.AutoHideDelay - preExistingTime;
        if (delay >= TimeSpan.FromMilliseconds(50)) {
            this.animation = new Animation {
                Duration = delay,
                Easing = new QuarticEaseIn(), FillMode = FillMode.Forward,
                Children = {
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 1.0) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 0.0) } }
                }
            };

            this.animation.RunAsync(this, sender.CancellationToken);
        }
    }

    private void OnCommandInserted(object sender, int index, NotificationCommand item) {
        if (this.PART_ActionPanel != null) {
            this.PART_ActionPanel!.Children.Insert(index, new HyperlinkButton() {
                Content = item.Text, Command = new CommandWrapper(item)
            });
        }
    }

    private void OnCommandRemoved(object sender, int index, NotificationCommand item) {
        if (this.PART_ActionPanel != null)
            this.PART_ActionPanel!.Children.RemoveAt(index);
    }

    private void OnCommandMoved(object sender, int oldindex, int newindex, NotificationCommand item) {
        if (this.PART_ActionPanel != null)
            this.PART_ActionPanel!.Children.Move(oldindex, newindex);
    }

    private class CommandWrapper : BaseAsyncRelayCommand {
        private readonly NotificationCommand command;

        public CommandWrapper(NotificationCommand command) {
            this.command = command;
        }

        protected override Task ExecuteCoreAsync(object? parameter) {
            return this.command.Execute();
        }
    }
}