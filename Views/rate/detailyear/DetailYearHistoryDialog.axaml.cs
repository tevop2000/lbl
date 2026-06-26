using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AgentManagement.Avalonia.Views.rate.detailyear
{
    public partial class DetailYearHistoryDialog : Window
    {
        public DetailYearHistoryDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}