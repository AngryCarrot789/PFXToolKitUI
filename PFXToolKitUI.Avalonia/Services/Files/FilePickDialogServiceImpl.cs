// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of PFXToolKitUI.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using Path = System.IO.Path;

namespace PFXToolKitUI.Avalonia.Services.Files;

public class FilePickDialogServiceImpl : IFilePickDialogService {
    // We wait for this much time in order to get around an issue where opening a file
    // picker right after another dialog closes can cause WM_ENABLE not to be sent to
    // the parent window, basically freezing the whole application
    private const int WaitTimeMillisForWM_ENABLE = 100;

    public static IReadOnlyList<FilePickerFileType>? ConvertFilters(IEnumerable<FileFilter>? filters) {
        if (filters == null)
            return null;

        return filters.Select(x => new FilePickerFileType(x.Name) {
            Patterns = x.Patterns,
            AppleUniformTypeIdentifiers = x.AppleUniformTypeIdentifiers,
            MimeTypes = x.MimeTypes,
        }).ToImmutableList();
    }

    public async Task<string?> OpenFile(string? message, IEnumerable<FileFilter>? filters = null, string? initialPath = null) {
        if (!IWindowManager.TryGetInstance(out IWindowManager? service) || !service.TryGetActiveOrMainWindow(out IWindow? window)) {
            return null;
        }

        if (window.TryGetTopLevel(out TopLevel? topLevel)) {
            await Task.Delay(WaitTimeMillisForWM_ENABLE);
            IStorageProvider provider = topLevel.StorageProvider;
            if (!provider.CanOpen) {
                await IMessageDialogService.Instance.ShowMessage("Error", "This platform does not support picking files");
                return null;
            }

            string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
            IReadOnlyList<IStorageFile> list = await provider.OpenFilePickerAsync(new FilePickerOpenOptions() {
                Title = message ?? "Pick a file",
                AllowMultiple = false,
                SuggestedFileName = fileName,
                FileTypeFilter = ConvertFilters(filters)
            });

            return list.Count != 1 ? null : list[0].Path.LocalPath;
        }

        return null;
    }

    public async Task<string[]?> OpenMultipleFiles(string? message, IEnumerable<FileFilter>? filters = null, string? initialPath = null) {
        if (!IWindowManager.TryGetInstance(out IWindowManager? service) || !service.TryGetActiveOrMainWindow(out IWindow? window)) {
            return null;
        }

        if (window.TryGetTopLevel(out TopLevel? topLevel)) {
            await Task.Delay(WaitTimeMillisForWM_ENABLE);
            IStorageProvider provider = topLevel.StorageProvider;
            if (!provider.CanOpen) {
                await IMessageDialogService.Instance.ShowMessage("Error", "This platform does not support picking files");
                return null;
            }

            string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
            IReadOnlyList<IStorageFile> list = await provider.OpenFilePickerAsync(new FilePickerOpenOptions() {
                Title = message ?? "Pick some files",
                AllowMultiple = true,
                SuggestedFileName = fileName,
                FileTypeFilter = ConvertFilters(filters)
            });

            return list.Count == 0 ? null : list.Select(x => x.Path.LocalPath).ToArray();
        }

        return null;
    }

    public async Task<string?> OpenFolders(string? message, string? initialPath = null, bool allowMultiple = false) {
        if (!IWindowManager.TryGetInstance(out IWindowManager? service) || !service.TryGetActiveOrMainWindow(out IWindow? window)) {
            return null;
        }

        if (window.TryGetTopLevel(out TopLevel? topLevel)) {
            await Task.Delay(WaitTimeMillisForWM_ENABLE);
            IStorageProvider provider = topLevel.StorageProvider;
            if (!provider.CanPickFolder) {
                await IMessageDialogService.Instance.ShowMessage("Error", "This platform does not support picking folders");
                return null;
            }

            string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
            IReadOnlyList<IStorageFolder> list = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions() {
                Title = message ?? ("Pick " + (allowMultiple ? " folders" : " a folder")),
                AllowMultiple = allowMultiple,
                SuggestedFileName = fileName
            });

            return list.Count != 1 ? null : list[0].Path.LocalPath;
        }

        return null;
    }

    public async Task<string?> SaveFile(string? message, IEnumerable<FileFilter>? filters = null, string? initialPath = null, bool warnOverwrite = true) {
        if (!IWindowManager.TryGetInstance(out IWindowManager? service) || !service.TryGetActiveOrMainWindow(out IWindow? window)) {
            return null;
        }

        if (window.TryGetTopLevel(out TopLevel? topLevel)) {
            await Task.Delay(WaitTimeMillisForWM_ENABLE);
            IStorageProvider provider = topLevel.StorageProvider;
            if (!provider.CanSave) {
                await IMessageDialogService.Instance.ShowMessage("Error", "This platform does not support picking files");
                return null;
            }

            string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
            string? extension = fileName != null ? Path.GetExtension(fileName) : null;
            IStorageFile? item = await provider.SaveFilePickerAsync(new FilePickerSaveOptions() {
                Title = message ?? "Save a file",
                SuggestedFileName = fileName,
                DefaultExtension = extension,
                ShowOverwritePrompt = warnOverwrite,
                FileTypeChoices = ConvertFilters(filters)
            });

            return item?.Path.LocalPath;
        }

        return null;
    }
}