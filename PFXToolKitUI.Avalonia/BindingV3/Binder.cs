using System.Diagnostics;
using Avalonia.Controls;
using PFXToolKitUI.Avalonia.Bindings.Events;

namespace PFXToolKitUI.Avalonia.BindingV3;

public static class Binder {
    public static EventPropertyBinderEx<T> EventOneWay<T>(string eventName, Action<Control, T> updateControl) where T : class {
        return new EventPropertyBinderEx<T>(eventName, updateControl);
    }
}

public class EventPropertyBinderEx<TModel> where TModel : class {
    private readonly Dictionary<Control, TModel> c2m = new Dictionary<Control, TModel>();
    private readonly Dictionary<TModel, Control> m2c = new Dictionary<TModel, Control>();
    
    private readonly SenderEventRelay eventRelay;
    private readonly Action<Control, TModel> updateControl;

    public EventPropertyBinderEx(string eventName, Action<Control, TModel> updateControl) {
        this.updateControl = updateControl;
        this.eventRelay = EventRelayBinderUtils.GetEventRelay(typeof(TModel), eventName);
    }

    public void Attach(Control control, TModel model) {
        if (this.c2m.ContainsKey(control))
            throw new InvalidOperationException("Control already attached");
        if (this.m2c.ContainsKey(model))
            throw new InvalidOperationException("Model already attached");
        
        this.c2m.Add(control, model);
        this.m2c.Add(model, control);
    }

    public void Detach(Control control) {
        if (!this.c2m.TryGetValue(control, out TModel? model))
            throw new InvalidOperationException("Control not attached");
        if (!this.m2c.Remove(model))
            throw new InvalidOperationException("Model not attached... error");
        bool removed = this.c2m.Remove(control);
        Debug.Assert(removed);
    }
}