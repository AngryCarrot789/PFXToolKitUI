using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI;

public class ThreadedPumpDispatcher : IDispatcher, IDispatcherFrameManager {
    private readonly Thread thread;
    private readonly PriorityQueue<BaseOperation, DispatchPriority> queue;
    private ManualResetEvent? myMre;
    private readonly Lock queueLock = new Lock();
    private CancellationTokenSource? myShutdownCts;
    private readonly CancellationToken shutdownToken;

    public ThreadedPumpDispatcher() {
        this.thread = new Thread(this.ThreadMain);
        this.queue = new PriorityQueue<BaseOperation, DispatchPriority>();
        this.myMre = new ManualResetEvent(false);
        this.myShutdownCts = new CancellationTokenSource();
    }

    private abstract class BaseOperation {
        private int state; // 0 = waiting, 1 = running, 2 = completed, 3 = exception
    }

    public bool IsFramePushingSuspended => false;

    public void PushFrame(CancellationToken cancellationToken) => this.EnterRunLoop(cancellationToken);

    private void ProcessEvents() {
        throw new NotImplementedException();
    }

    private void ThreadMain() {
        ManualResetEvent mre = this.myMre!;
        CancellationTokenSource cts = this.myShutdownCts!;
        Debug.Assert(mre != null);
        Debug.Assert(cts != null);

        try {
            this.EnterRunLoop(CancellationToken.None);
        }
        finally {
            lock (this.queueLock) {
                this.myMre = null;
                this.myShutdownCts = null;

                cts.Cancel();
                mre.Dispose();
                this.queue.Clear();
            }
        }
    }

    public void EnterRunLoop(CancellationToken cancellation) {
        if (this.myMre == null)
            throw new InvalidOperationException("Dispatcher shutdown");

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(this.shutdownToken, cancellation);
        using CancellationTokenRegistration registration = cancellation.Register(e => ((ManualResetEvent?) e)?.Set(), this.myMre);

        while (cts.IsCancellationRequested) {
            this.WaitForEvent();

            if (!cts.IsCancellationRequested)
                this.ProcessEvents();
        }
    }

    private void WaitForEvent() => this.myMre?.WaitOne();

    #region Interface implementation

    public bool CheckAccess() => this.thread == Thread.CurrentThread;

    public void Invoke(Action action, DispatchPriority priority = DispatchPriority.Send) {
    }

    public T Invoke<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Send) {
        throw new NotImplementedException();
    }

    public Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.BeforeRender, CancellationToken token = default) {
        throw new NotImplementedException();
    }

    public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.BeforeRender, CancellationToken token = default) {
        throw new NotImplementedException();
    }

    public void Post(Action<object?> action, object? state, DispatchPriority priority = DispatchPriority.Default) {
        throw new NotImplementedException();
    }

    public void Shutdown() {
        this.myShutdownCts?.Cancel();
        this.myMre?.Set();
        this.thread.Join();
    }

    public IDispatcherTimer CreateTimer(DispatchPriority priority) {
        throw new NotImplementedException();
    }

    public bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager) {
        return (frameManager = this) != null;
    }

    #endregion
}