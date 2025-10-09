using System.Diagnostics;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI;

/// <summary>
/// Manages post-started actions
/// </summary>
public sealed class PostStartupManager {
    public static PostStartupManager Instance => ApplicationPFX.GetComponent<PostStartupManager>();
    
    private List<Action>? actions = new List<Action>();

    public PostStartupManager() {
    }

    internal void OnPostStartup() {
        List<Action>? list = Interlocked.Exchange(ref this.actions, null);
        Debug.Assert(list != null);
        foreach (Action action in list) {
            try {
                action();
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Exception while executing post-startup actions: " + e.GetToString());
            }
        }
    }
    
    public void Register(Action action) {
        if (this.actions == null)
            throw new InvalidOperationException("Post-startup actions already invoked");
        this.actions.Add(action);
    }
}