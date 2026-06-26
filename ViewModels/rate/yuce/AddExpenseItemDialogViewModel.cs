using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.ViewModels.rate.yuce
{
    public partial class AddExpenseItemDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _expenseType = string.Empty;

        [ObservableProperty]
        private decimal _amount = 0;

        [ObservableProperty]
        private int _isIncome = 1;

        [ObservableProperty]
        private bool _isLoading = false;

        public event Action<string, decimal, int>? OnConfirm;

        public AddExpenseItemDialogViewModel()
        {
        }

        [RelayCommand]
        private void Confirm()
        {
            if (string.IsNullOrWhiteSpace(ExpenseType))
            {
                Logger.Warning("请输入项目名称");
                return;
            }

            if (Amount <= 0)
            {
                Logger.Warning("请输入有效的项目费用");
                return;
            }

            Logger.Info($"确认添加项目: {ExpenseType}, 金额: {Amount}, 类型: {(IsIncome == 1 ? "收益" : "固定成本")}");
            OnConfirm?.Invoke(ExpenseType, Amount, IsIncome);
        }
    }
}
