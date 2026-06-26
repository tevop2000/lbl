using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentManagement.Avalonia.ViewModels.rate.yuce;

namespace AgentManagement.Avalonia.Views.rate.yuce
{
    public partial class AddExpenseItemDialog : Window
    {
        public AddExpenseItemDialog()
        {
            InitializeComponent();
        }

        public AddExpenseItemDialogViewModel ViewModel => (AddExpenseItemDialogViewModel)DataContext!;

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
