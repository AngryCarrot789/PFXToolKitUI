namespace PFXToolKitUI.Utils.Events;

/// <summary>
/// Helper methods for item add, remove and move events using <see cref="ItemIndexEventArgs{T}"/> and <see cref="ItemMovedEventArgs{T}"/>
/// </summary>
public static class ItemEventUtils {
    /// <summary>
    /// Invokes the event handler for all items within the list
    /// </summary>
    /// <param name="existingItems">The list to iterate</param>
    /// <param name="sender">The sender argument passed to the event handler</param>
    /// <param name="eventHandler">The event handler</param>
    /// <param name="isAdding">
    /// Whether items should be added forward or backward. When adding, we add
    /// from 0 to Count-1, whereas when removing, we can remove back to front.
    /// </param>
    /// <typeparam name="T">The type of item</typeparam>
    public static void InvokeItems<T>(IReadOnlyList<T> existingItems, object? sender, EventHandler<ItemIndexEventArgs<T>>? eventHandler, bool isAdding) {
        if (eventHandler != null) {
            if (isAdding) {
                for (int i = 0; i < existingItems.Count; i++) {
                    eventHandler(sender, new ItemIndexEventArgs<T>(existingItems[i], i));
                }
            }
            else {
                for (int i = existingItems.Count - 1; i >= 0; i--) {
                    eventHandler(sender, new ItemIndexEventArgs<T>(existingItems[i], i));
                }   
            }
        }
    }
}