using PFXToolKitUI.Services;

namespace PFXToolKitUI.CommandSystem;

public class AboutApplicationCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return ApplicationPFX.Instance.ServiceManager.HasService<IAboutService>() ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!ApplicationPFX.Instance.ServiceManager.TryGetService(out IAboutService? service)) {
            return Task.CompletedTask;
        }

        return service.ShowDialog();
    }
}