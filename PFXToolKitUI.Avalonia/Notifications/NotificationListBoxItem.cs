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
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Avalonia.Activities;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Notifications;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Commands;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Notifications;

public class NotificationListBoxItem : ModelBasedListBoxItem<Notification> {
    public static readonly IDynamicColourBrush DynamicForegroundBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Foreground.Static");
    
    public static readonly ModelControlRegistry<Notification, Control> ModelControlRegistry;

    public static readonly StyledProperty<string?> CaptionProperty = AvaloniaProperty.Register<NotificationListBoxItem, string?>(nameof(Caption));

    public string? Caption {
        get => this.GetValue(CaptionProperty);
        set => this.SetValue(CaptionProperty, value);
    }

    private Button? PART_Close;
    private Panel? PART_ActionPanel;
    private Border? PART_HeaderBorder;
    private ObservableItemProcessorIndexing<NotificationAction>? processor;
    private readonly LazyHelper2<MultiBrushFlipFlopTimer, bool> alertFlipFlop;
    private Animation? animation;

    public NotificationListBoxItem() {
        this.alertFlipFlop = new LazyHelper2<MultiBrushFlipFlopTimer, bool>((a, b, enabled) => a.IsEnabled = enabled && b);
    }

    static NotificationListBoxItem() {
        ModelControlRegistry = new ModelControlRegistry<Notification, Control>();
        ModelControlRegistry.RegisterType<TextNotification>((b) => new NotificationContent_TextBlock(b));
        ModelControlRegistry.RegisterType<ActivityNotification>((b) => new NotificationContent_Activity(b));
    }

    protected override void OnPointerEntered(PointerEventArgs e) {
        base.OnPointerEntered(e);
        if (this.Model != null) {
            if (this.Model.AlertMode == NotificationAlertMode.UntilUserInteraction) {
                this.Model.AlertMode = NotificationAlertMode.None;
            }
            
            this.Model.CancelAutoHide();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e) {
        base.OnPointerExited(e);
        // this.Model?.StartAutoHide();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) {
            this.Model?.Hide();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.alertFlipFlop.Value1.Value.EnableTargets();
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.alertFlipFlop.Value1.Value.ClearTarget();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Close = e.NameScope.GetTemplateChild<Button>("PART_Close");
        this.PART_Close.Click += (sender, args) => this.Model?.Hide();

        this.PART_ActionPanel = e.NameScope.GetTemplateChild<Panel>("PART_ActionPanel");
        this.PART_HeaderBorder = e.NameScope.GetTemplateChild<Border>("PART_HeaderBorder");
        
        this.alertFlipFlop.Value1 = new MultiBrushFlipFlopTimer(TimeSpan.FromMilliseconds(500), [
            new BrushExchange(this.PART_HeaderBorder, BackgroundProperty, ConstantAvaloniaColourBrush.Transparent, new ConstantAvaloniaColourBrush(Brushes.Yellow)),
            new BrushExchange(this.PART_HeaderBorder, ForegroundProperty, DynamicForegroundBrush, new ConstantAvaloniaColourBrush(Brushes.Black)),
        ]) { StartHigh = true };
        
        this.processor?.AddExistingItems();
    }

    protected override void OnAddingToList() {
        this.Opacity = 1.0;
    }

    protected override void OnAddedToList() {
        this.Caption = this.Model!.Caption ?? "";
        this.Model.CaptionChanged += this.ModelOnCaptionChanged;
        this.Model.IsAutoHideActiveChanged += this.OnIsAutoHideActiveChanged;
        this.Model.AlertModeChanged += this.OnAlertModeChanged;
        if (this.Model.IsAutoHideActive) {
            this.OnIsAutoHideActiveChanged(this.Model);
        }

        this.processor = ObservableItemProcessor.MakeIndexable(this.Model!.Actions, this.OnCommandInserted, this.OnCommandRemoved, this.OnCommandMoved);
        if (this.PART_ActionPanel != null)
            this.processor.AddExistingItems();

        if (ModelControlRegistry.TryGetNewInstance(this.Model!, out Control? control)) {
            this.Content = control;
            (control as INotificationContent)?.OnShown();
        }
        
        this.alertFlipFlop.Value2 = this.Model.AlertMode != NotificationAlertMode.None;
    }

    protected override void OnRemovingFromList() {
        (this.Content as INotificationContent)?.OnHidden();
        this.Content = null;

        this.processor!.RemoveExistingItems();
        this.processor!.Dispose();

        this.Model!.CaptionChanged -= this.ModelOnCaptionChanged;
        this.Model!.IsAutoHideActiveChanged -= this.OnIsAutoHideActiveChanged;
        this.Model!.AlertModeChanged -= this.OnAlertModeChanged;
        
        this.alertFlipFlop.Value2 = default;
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
    
    private void OnAlertModeChanged(Notification sender) {
        this.alertFlipFlop.Value2 = sender.AlertMode != NotificationAlertMode.None;
    }

    // Note: the buttons are added/removed when the actual notification is added to/removed from the notification list box.
    // This is only done so that we can cache hyperlinks. Even though the IBinders aren't that expensive, it's still good to do
    private void OnCommandInserted(object sender, int index, NotificationAction item) {
        if (this.PART_ActionPanel != null)
            this.PART_ActionPanel!.Children.Insert(index, NotificationHyperlinkButton.GetCachedOrCreate(item));
    }

    private void OnCommandRemoved(object sender, int index, NotificationAction item) {
        if (this.PART_ActionPanel != null) {
            NotificationHyperlinkButton button = (NotificationHyperlinkButton) this.PART_ActionPanel!.Children[index];
            this.PART_ActionPanel!.Children.RemoveAt(index);
            NotificationHyperlinkButton.PushCachedButton(button);
        }
    }

    private void OnCommandMoved(object sender, int oldindex, int newindex, NotificationAction item) {
        if (this.PART_ActionPanel != null)
            this.PART_ActionPanel!.Children.Move(oldindex, newindex);
    }
    
    private class NotificationContent_TextBlock : TextBlock, INotificationContent {
        private readonly TextNotification notification;
        protected override Type StyleKeyOverride => typeof(TextBlock);

        public NotificationContent_TextBlock(TextNotification textNotification) {
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

    private class NotificationContent_Activity : ActivityListItem, INotificationContent {
        protected override Type StyleKeyOverride => typeof(ActivityListItem);

        private readonly ActivityNotification notification;

        public NotificationContent_Activity(ActivityNotification notification) {
            this.ShowCaption = false;

            this.notification = notification;
            if (!this.notification.ActivityTask.IsCompleted) {
                this.notification.ActivityTask.IsCompletedChanged += this.OnIsCompletedChanged;
            }
        }

        private void OnIsCompletedChanged(ActivityTask sender) {
            this.notification.ActivityTask.IsCompletedChanged -= this.OnIsCompletedChanged;
            this.notification.Hide();
        }

        public void OnShown() {
            this.ActivityTask = this.notification.ActivityTask;
        }

        public void OnHidden() {
            this.ActivityTask = null;
        }
    }

    private class NotificationHyperlinkButton : HyperlinkButton {
        private static readonly Stack<NotificationHyperlinkButton> buttonCache = new Stack<NotificationHyperlinkButton>(8);

        protected override Type StyleKeyOverride => typeof(HyperlinkButton);

        private readonly IBinder<NotificationAction> textBinder = new EventUpdateBinder<NotificationAction>(nameof(NotificationAction.TextChanged), (b) => ((NotificationHyperlinkButton) b.Control).Content = b.Model.Text);

        private readonly IBinder<NotificationAction> toolTipBinder = new EventUpdateBinder<NotificationAction>(nameof(NotificationAction.ToolTipChanged), (b) => {
            if (!string.IsNullOrEmpty(b.Model.ToolTip)) {
                TextBlock tb = new TextBlock {
                    Text = b.Model.ToolTip,
                    TextDecorations = []
                };

                ToolTipEx.SetTip((NotificationHyperlinkButton) b.Control, tb);
            }
            else {
                b.Control.ClearValue(ToolTipEx.TipProperty);
            }
        });

        private NotificationAction? myCurrentCommand;

        public NotificationHyperlinkButton() {
            this.Command = new NotificationCommandDelegate(this);
            Binders.AttachControls(this, this.toolTipBinder, this.textBinder);
        }

        public static NotificationHyperlinkButton GetCachedOrCreate(NotificationAction item) {
            if (!buttonCache.TryPop(out NotificationHyperlinkButton? button)) {
                button = new NotificationHyperlinkButton();
            }

            button.myCurrentCommand = item;
            return button;
        }

        public static bool PushCachedButton(NotificationHyperlinkButton button) {
            Debug.Assert(button.myCurrentCommand != null);
            button.myCurrentCommand = null;
            bool pushed = buttonCache.Count < 8;
            if (pushed)
                buttonCache.Push(button);
            return pushed;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if (this.myCurrentCommand == null) {
                throw new Exception(nameof(this.myCurrentCommand) + " not set");
            }

            Binders.AttachModels(this.myCurrentCommand, this.toolTipBinder, this.textBinder);
            ((NotificationCommandDelegate) this.Command!).OnButtonAttachedToVT();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            Binders.DetachModels(this.toolTipBinder, this.textBinder);
            ((NotificationCommandDelegate) this.Command!).OnButtonDetachedFromVT();
        }

        // We delegate to ICommand just because it's simpler to do that over managing
        // the clicking and also effectively enabled state ourself :P
        private class NotificationCommandDelegate(NotificationHyperlinkButton button) : BaseAsyncRelayCommand {
            private NotificationAction? cmd;

            protected override bool CanExecuteCore(object? parameter) => button.myCurrentCommand?.CanExecute() ?? false;

            protected override async Task ExecuteCoreAsync(object? parameter) {
                if (button.myCurrentCommand != null) {
                    try {
                        await CommandManager.Instance.RunActionAsync(_ => button.myCurrentCommand.Execute(), DataManager.GetFullContextData(button));
                    }
                    catch (Exception exception) when (!Debugger.IsAttached) {
                        await LogExceptionHelper.ShowMessageAndPrintToLogs("Notification Action Error", exception);
                    }
                }
            }

            // Delegate NotificationAction's CanExecuteChange to the ICommand version
            private void OnCanExecuteChanged(NotificationAction sender) => this.RaiseCanExecuteChanged();

            public void OnButtonAttachedToVT() {
                this.cmd = button.myCurrentCommand!;
                Debug.Assert(this.cmd != null);

                this.cmd.CanExecuteChanged += this.OnCanExecuteChanged;
                
                // When another notification is shown in the same call frame as a notification action being executed,
                // if the pressed notification's button is cached then re-used in the newly shown notification,
                // it will be stuck disabled because it's still technically running when CanExecute is queried.
                
                // So, schedule update at some point in the future.
                // Using BeforeRender so that the user won't see it flicker as disabled then enabled next frame
                ApplicationPFX.Instance.Dispatcher.Post(this.RaiseCanExecuteChanged, DispatchPriority.BeforeRender);
            }

            public void OnButtonDetachedFromVT() {
                Debug.Assert(this.cmd != null);

                this.cmd!.CanExecuteChanged -= this.OnCanExecuteChanged;
            }
        }
    }
}