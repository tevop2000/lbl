using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Views;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class ImportOptionsViewModel : ViewModelBase
    {
        private readonly ImportOptionsDialog _dialog;

        [ObservableProperty]
        private ObservableCollection<int> _yearList = new();

        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private bool _isOverwrite = true;

        [ObservableProperty]
        private bool _isSkip;

        public ImportOptionsViewModel(ImportOptionsDialog dialog)
        {
            _dialog = dialog;
            
            // 生成近5年的年份列表
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 4; i--)
            {
                YearList.Add(i);
            }
            
            SelectedYear = currentYear;
        }

        [RelayCommand]
        private void Confirm()
        {
            _dialog.Result = new ImportOptionsResult
            {
                Year = SelectedYear,
                Overwrite = IsOverwrite
            };
            _dialog.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            _dialog.Result = null;
            _dialog.Close();
        }
    }
}
