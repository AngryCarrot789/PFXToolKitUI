using System.Runtime.InteropServices;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder that does not listen to property changes or listen to any kind of event.
/// Useful for binding immutable models to a UI.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class ManualBinder<TModel> : BaseBinder<TModel> where TModel : class {
    private readonly Action<IBinder<TModel>> onAttached;
    private readonly Action<IBinder<TModel>>? onDetached;

    public ManualBinder(Action<IBinder<TModel>> onAttached, Action<IBinder<TModel>>? onDetached = null) {
        this.onAttached = onAttached;
        this.onDetached = onDetached;
    }

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
    }

    protected override void OnAttached() {
        this.onAttached.Invoke(this);
    }

    protected override void OnDetached() {
        this.onDetached?.Invoke(this);
    }
}