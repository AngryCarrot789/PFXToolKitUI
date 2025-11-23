namespace PFXToolKitUI.Utils.Reactive;

/// <summary>
/// Represents an observable object
/// </summary>
public interface IEventObservable<T> {
    /// <summary>
    /// Adds a subscriber to the event
    /// </summary>
    /// <param name="state">The state to pass to the callback</param>
    /// <param name="callback">The callback</param>
    /// <param name="initialCallback">Immediately calls the callback before this method returns</param>
    /// <returns>The subscription to the event. Dispose to unsubscribe</returns>
    IDisposable Subscribe(T owner, Action<T> callback, bool initialCallback = true);
}