using System.Threading.Tasks;

namespace PFXToolKitUI.Tests;

public class TestAppStartupManager : IStartupManager {
    public Task OnApplicationStartupWithArgs(string[] args) {
        new MainWindow().Show();
        return Task.CompletedTask;
    }
}