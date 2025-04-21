using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Avalonia.Themes.Controls;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Tests;

public partial class MainWindow : WindowEx {
    private ContextEntryGroup? themesSubList;
    private ActivityTask? primaryActivity;

    public TopLevelMenuRegistry ToolBarRegistry { get; }

    public MainWindow() {
        this.InitializeComponent();

        // TemplateUtils.ApplyRecursive(this);
        // this.Measure(default);

        // average 5 samples. Will take a second to catch up when playing at 5 fps but meh
        ActivityManager activityManager = ActivityManager.Instance;
        activityManager.TaskStarted += this.OnTaskStarted;
        activityManager.TaskCompleted += this.OnTaskCompleted;

        this.ToolBarRegistry = new TopLevelMenuRegistry();
        ContextEntryGroup fileEntry = new ContextEntryGroup("File");
        fileEntry.Items.Add(new CommandContextEntry("commands.mainWindow.OpenEditorSettings", "Preferences"));
#if DEBUG
        fileEntry.Items.Add(new SeparatorEntry());
        fileEntry.Items.Add(new TestDynamicInsertionContextEntry(this, fileEntry));
#endif

        this.ToolBarRegistry.Items.Add(fileEntry);

        this.themesSubList = new ContextEntryGroup("Themes");
        this.ToolBarRegistry.Items.Add(this.themesSubList);

        this.PART_TopLevelMenu.TopLevelMenuRegistry = this.ToolBarRegistry;

        ActivityManager.Instance.RunTask(async () => {
            IActivityProgress prog = ActivityManager.Instance.GetCurrentProgressOrEmpty();
            prog.Text = "Test activity!";
            for (int i = 0; i < 100; i++) {
                await Task.Delay(10);
                prog.CompletionState.TotalCompletion = i / 100.0;
            }
        });
    }

    #region Task Manager and Activity System

    private void OnTaskStarted(ActivityManager manager, ActivityTask task, int index) {
        if (this.primaryActivity == null || this.primaryActivity.IsCompleted) {
            this.SetActivityTask(task);
        }
    }

    private void OnTaskCompleted(ActivityManager manager, ActivityTask task, int index) {
        if (task == this.primaryActivity) {
            // try to access next task
            this.SetActivityTask(manager.ActiveTasks.Count > 0 ? manager.ActiveTasks[0] : null);
        }
    }

    private void SetActivityTask(ActivityTask? task) {
        IActivityProgress? prog = null;
        if (this.primaryActivity != null) {
            prog = this.primaryActivity.Progress;
            prog.TextChanged -= this.OnPrimaryActivityTextChanged;
            prog.CompletionState.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged -= this.OnPrimaryActivityIndeterminateChanged;
            prog = null;
        }

        this.primaryActivity = task;
        if (task != null) {
            prog = task.Progress;
            prog.TextChanged += this.OnPrimaryActivityTextChanged;
            prog.CompletionState.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged += this.OnPrimaryActivityIndeterminateChanged;
            this.PART_ActiveBackgroundTaskGrid.IsVisible = true;
        }
        else {
            this.PART_ActiveBackgroundTaskGrid.IsVisible = false;
        }

        this.OnPrimaryActivityTextChanged(prog);
        this.OnPrimaryActionCompletionValueChanged(prog?.CompletionState);
        this.OnPrimaryActivityIndeterminateChanged(prog);
    }

    private void OnPrimaryActivityTextChanged(IActivityProgress? tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_TaskCaption.Text = tracker?.Text ?? "", DispatchPriority.Loaded);
    }

    private void OnPrimaryActionCompletionValueChanged(CompletionState? state) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.Value = state?.TotalCompletion ?? 0.0, DispatchPriority.Loaded);
    }

    private void OnPrimaryActivityIndeterminateChanged(IActivityProgress? tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false, DispatchPriority.Loaded);
    }

    #endregion

#if DEBUG
    private class TestDynamicInsertionContextEntry : CustomContextEntry {
        private readonly ContextEntryGroup entry;
        private readonly MainWindow editorWindow;

        public TestDynamicInsertionContextEntry(MainWindow editorWindow, ContextEntryGroup entry, Icon? icon = null) : base("Test Dynamic Items", null, icon) {
            this.entry = entry;
            this.editorWindow = editorWindow;
        }

        public override Task OnExecute(IContextData context) {
            return ActivityManager.Instance.RunTask(async () => {
                IActivityProgress prog = ActivityManager.Instance.GetCurrentProgressOrEmpty();
                prog.Text = "Waiting a few secs; Open 'file'";

                await Task.Delay(2000);

                List<int> indices = new List<int>();
                prog.Text = "Inserting 10 items";
                for (int i = 0; i <= 10; i += 2) {
                    await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                        int idx = Math.Min(i, this.entry.Items.Count);
                        if (i % 5 == 0) {
                            this.entry.Items.Insert(idx, new ContextEntryGroup($"Before SubList at {i}", null));
                            this.entry.Items.Insert(idx + 1, new DynamicGroupPlaceholderContextObject(new DynamicContextGroup(this.Generate)));
                            indices.Add(idx);
                            indices.Add(idx + 1);
                        }
                        else {
                            this.entry.Items.Insert(idx, new ContextEntryGroup($"Another Test at {i}", null));
                            indices.Add(idx); // 2
                        }
                    });

                    prog.CompletionState.OnProgress(0.1);
                    await Task.Delay(500);
                }

                prog.Text = "Waiting 1 sec...";
                prog.CompletionState.TotalCompletion = 0.0;
                await Task.Delay(3000);
                await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                    prog.Text = "Removing those 10 items";
                });

                for (int i = indices.Count - 1; i >= 0; i--) {
                    await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                        this.entry.Items.RemoveAt(indices[i]);
                        prog.CompletionState.OnProgress(0.1);
                    });

                    await Task.Delay(500);
                }
            }).Task;
        }

        private void Generate(DynamicContextGroup group, IContextData ctx, List<IContextObject> items) {
            items.Add(new ContextEntryGroup("Sub Item 0", null));
            items.Add(new ContextEntryGroup("Sub Item 1", null));
            items.Add(new ContextEntryGroup("Sub Item 2", null));
        }
    }
#endif
}