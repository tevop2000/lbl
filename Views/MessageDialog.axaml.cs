using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace AgentManagement.Avalonia.Views;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 显示信息提示框
    /// </summary>
    public static async Task ShowInfoAsync(Window owner, string title, string message)
    {
        var dialog = new MessageDialog();
        var viewModel = new MessageDialogViewModel
        {
            Title = title,
            Message = message,
            Icon = "ℹ️"
        };
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(owner);
    }

    /// <summary>
    /// 显示成功提示框
    /// </summary>
    public static async Task ShowSuccessAsync(Window owner, string title, string message)
    {
        var dialog = new MessageDialog();
        var viewModel = new MessageDialogViewModel
        {
            Title = title,
            Message = message,
            Icon = "✅"
        };
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(owner);
    }

    /// <summary>
    /// 显示错误提示框
    /// </summary>
    public static async Task ShowErrorAsync(Window owner, string title, string message)
    {
        var dialog = new MessageDialog();
        var viewModel = new MessageDialogViewModel
        {
            Title = title,
            Message = message,
            Icon = "❌"
        };
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(owner);
    }
}

public partial class MessageDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private string _icon = "ℹ️";
}
