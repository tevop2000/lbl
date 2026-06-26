using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using AgentManagement.Avalonia.ViewModels.Controls;

namespace AgentManagement.Avalonia.Controls
{
    /// <summary>
    /// 产品选择器对话框（可复用组件）
    /// </summary>
    public partial class ProductSelectorDialog : Window
    {
        private readonly ProductSelectorDialogViewModel _viewModel;

        public ProductSelectorDialog()
        {
            InitializeComponent();
            
            _viewModel = new ProductSelectorDialogViewModel();
            DataContext = _viewModel;

            // 监听选择完成事件
            _viewModel.ProductSelected += (result) =>
            {
                // 设置对话框结果
                this.Tag = result;
            };

            // 监听关闭请求
            _viewModel.RequestClose += () =>
            {
                this.Close();
            };
        }

        /// <summary>
        /// 显示对话框并返回选择结果
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <returns>产品选择结果，如果取消则返回 null</returns>
        public new async Task<ProductSelectorResult?> ShowDialog(Window owner)
        {
            await base.ShowDialog(owner);
            return this.Tag as ProductSelectorResult;
        }
    }
}
