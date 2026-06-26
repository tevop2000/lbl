using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.VisualTree;
using AgentManagement.Avalonia.ViewModels;

namespace AgentManagement.Avalonia.Views;

public partial class MainWindowContentView : UserControl
{
    public MainWindowContentView()
    {
        InitializeComponent();
        
        // 查找 TreeView 并添加选择事件
        var treeView = this.FindControl<TreeView>("MenuTreeView");
        if (treeView != null)
        {
            treeView.SelectionChanged += OnTreeViewSelectionChanged;
            // 使用 AddHandler 确保事件能够被捕获
            treeView.AddHandler(PointerPressedEvent, MenuTreeView_PointerPressed, handledEventsToo: true);
        }
    }

    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is MenuItemViewModel menuItem)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.NavigateToMenu(menuItem);
            }
        }
    }

    private void MenuTreeView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[MainWindowContentView] MenuTreeView_PointerPressed triggered");
        
        if (e.Source is Control control)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindowContentView] e.Source type: {control.GetType().Name}");
            
            var treeViewItem = control.FindAncestorOfType<TreeViewItem>();
            if (treeViewItem != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindowContentView] Found TreeViewItem, IsExpanded: {treeViewItem.IsExpanded}");
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                System.Diagnostics.Debug.WriteLine($"[MainWindowContentView] TreeViewItem IsExpanded set to: {treeViewItem.IsExpanded}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[MainWindowContentView] No TreeViewItem found");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[MainWindowContentView] e.Source is not a Control");
        }
    }
}
