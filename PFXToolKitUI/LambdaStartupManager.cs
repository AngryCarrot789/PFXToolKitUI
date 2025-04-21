namespace PFXToolKitUI;

public class LambdaStartupManager : IStartupManager {
    private readonly object action;

    public LambdaStartupManager(Action action) {
        this.action = action;
    }
    
    public LambdaStartupManager(Action<string[]> actionWithArgs) {
        this.action = actionWithArgs;
    }
    
    public LambdaStartupManager(Func<Task> asyncAction) {
        this.action = asyncAction;
    }
    
    public LambdaStartupManager(Func<string[], Task> asyncActionWithArgs) {
        this.action = asyncActionWithArgs;
    }
    
    public Task OnApplicationStartupWithArgs(string[] args) {
        switch (this.action) {
            case Action<string[]> a:
                a(args);
                return Task.CompletedTask;
            case Action a:
                a();
                return Task.CompletedTask;
            case Func<string[], Task> a: return a(args);
            case Func<Task> a:           return a();
            default:                     return Task.CompletedTask;
        }
    }
}