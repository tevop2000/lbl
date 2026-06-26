using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentManagement.Avalonia.ViewModels
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private string _statusMessage = "就绪";
    }
}
