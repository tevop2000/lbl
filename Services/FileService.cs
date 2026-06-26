using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Services
{
    /// <summary>
    /// 文件操作服务（跨平台）
    /// </summary>
    public class FileService
    {
        private static TopLevel? _topLevel;

        /// <summary>
        /// 设置顶层窗口（用于文件对话框）
        /// </summary>
        public static void SetTopLevel(TopLevel topLevel)
        {
            _topLevel = topLevel;
        }

        /// <summary>
        /// 显示保存文件对话框
        /// </summary>
        public static async Task<string?> ShowSaveFileDialogAsync(string title, string defaultFileName, params string[] fileTypes)
        {
            if (_topLevel == null)
            {
                Logger.Error("TopLevel未设置，无法显示文件对话框");
                return null;
            }

            var storageProvider = _topLevel.StorageProvider;
            
            if (!storageProvider.CanSave)
            {
                Logger.Error("当前平台不支持保存文件对话框");
                return null;
            }

            var result = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultFileName,
                FileTypeChoices = GetFileTypeChoices(fileTypes)
            });

            return result?.Path.LocalPath;
        }

        /// <summary>
        /// 显示打开文件对话框
        /// </summary>
        public static async Task<string?> ShowOpenFileDialogAsync(string title, params string[] fileTypes)
        {
            if (_topLevel == null)
            {
                Logger.Error("TopLevel未设置，无法显示文件对话框");
                return null;
            }

            var storageProvider = _topLevel.StorageProvider;
            
            if (!storageProvider.CanOpen)
            {
                Logger.Error("当前平台不支持打开文件对话框");
                return null;
            }

            var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = GetFileTypeChoices(fileTypes)
            });

            return result.Count > 0 ? result[0].Path.LocalPath : null;
        }

        private static FilePickerFileType[] GetFileTypeChoices(string[] fileTypes)
        {
            var choices = new System.Collections.Generic.List<FilePickerFileType>();
            
            foreach (var type in fileTypes)
            {
                if (type.ToLower().Contains("excel") || type.Contains(".xlsx") || type.Contains(".xls"))
                {
                    choices.Add(new FilePickerFileType("Excel文件")
                    {
                        Patterns = new[] { "*.xlsx", "*.xls" }
                    });
                }
                else if (type.ToLower().Contains("all") || type == "*.*")
                {
                    choices.Add(new FilePickerFileType("所有文件")
                    {
                        Patterns = new[] { "*.*" }
                    });
                }
            }

            return choices.ToArray();
        }

        /// <summary>
        /// 保存文件到指定路径
        /// </summary>
        public static async Task<bool> SaveFileAsync(string filePath, byte[] data)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, data);
                Logger.Success($"文件保存成功: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"文件保存失败: {ex.Message}", ex);
                return false;
            }
        }
    }
}