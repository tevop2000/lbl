using Avalonia.Controls;
using AgentManagement.Avalonia.ViewModels;
using System.Threading.Tasks;

namespace AgentManagement.Avalonia.Views;

public partial class ImportOptionsDialog : Window
{
    public ImportOptionsDialog()
    {
        InitializeComponent();
        DataContext = new ImportOptionsViewModel(this);
    }

    public ImportOptionsResult? Result { get; internal set; }

    /// <summary>
    /// 异步显示对话框并返回结果
    /// </summary>
    public async Task<bool?> ShowDialogAsync(Window owner)
    {
        await this.ShowDialog(owner);
        return Result != null;
    }
}

public class ImportOptionsResult
{
    public int Year { get; set; }
    public bool Overwrite { get; set; }
}
