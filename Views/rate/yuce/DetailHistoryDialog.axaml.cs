using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentManagement.Avalonia.ViewModels.rate.yuce;

namespace AgentManagement.Avalonia.Views.rate.yuce
{
    public partial class DetailHistoryDialog : Window
    {
        public DetailHistoryDialog()
        {
            InitializeComponent();
        }

        public DetailHistoryDialogViewModel ViewModel => (DetailHistoryDialogViewModel)DataContext!;

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}