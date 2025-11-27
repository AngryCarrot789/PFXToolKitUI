namespace PFXToolKitUI.Utils.Reactive;

/// <summary>
/// Represents an observable object
/// </summary>
public interface IEventObservable<T> {
    /// <summary>
    /// Adds a subscriber to the event
    /// </summary>
    /// <param name="owner">The instance to add the event handler to</param>
    /// <param name="callback">The callback</param>
    /// <param name="invokeImmediately">Immediately calls the callback before this method returns</param>
    /// <returns>The subscription to the event. Dispose to unsubscribe</returns>
    IDisposable Subscribe(T owner, Action<T> callback, bool invokeImmediately = true);
}