using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentManagement.Avalonia.ViewModels;

namespace AgentManagement.Avalonia.Views
{
    public partial class CVPAnalysisView : UserControl
    {
        public CVPAnalysisView()
        {
            InitializeComponent();
        }

        private void DeleteProduct_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductItem product)
            {
                if (DataContext is CVPAnalysisViewModel vm)
                {
                    vm.DeleteProductCommand.Execute(product);
                }
            }
        }
    }
}
