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
using Avalonia.Platform.Storage;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Services.FilePicking;
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
        if (!WindowingSystem.TryGetInstance(out WindowingSystem? service) || !service.TryGetActiveWindow(out DesktopWindow? window)) {
            return null;
        }

        await Task.Delay(WaitTimeMillisForWM_ENABLE);
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        IReadOnlyList<IStorageFile> list = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = message ?? "Pick a file",
            AllowMultiple = false,
            SuggestedFileName = fileName,
            FileTypeFilter = ConvertFilters(filters)
        });

        return list.Count != 1 ? null : list[0].Path.LocalPath;
    }

    public async Task<string[]?> OpenMultipleFiles(string? message, IEnumerable<FileFilter>? filters = null, string? initialPath = null) {
        if (!WindowingSystem.TryGetInstance(out WindowingSystem? service) || !service.TryGetActiveWindow(out DesktopWindow? window)) {
            return null;
        }

        await Task.Delay(WaitTimeMillisForWM_ENABLE);
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        IReadOnlyList<IStorageFile> list = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = message ?? "Pick some files",
            AllowMultiple = true,
            SuggestedFileName = fileName,
            FileTypeFilter = ConvertFilters(filters)
        });

        return list.Count == 0 ? null : list.Select(x => x.Path.LocalPath).ToArray();
    }

    public async Task<string?> SaveFile(string? message, IEnumerable<FileFilter>? filters = null, string? initialPath = null, bool warnOverwrite = true) {
        if (!WindowingSystem.TryGetInstance(out WindowingSystem? service) || !service.TryGetActiveWindow(out DesktopWindow? window)) {
            return null;
        }

        await Task.Delay(WaitTimeMillisForWM_ENABLE);
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        string? extension = fileName != null ? Path.GetExtension(fileName) : null;
        IStorageFile? item = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
            Title = message ?? "Save a file",
            SuggestedFileName = fileName,
            DefaultExtension = extension,
            ShowOverwritePrompt = warnOverwrite,
            FileTypeChoices = ConvertFilters(filters)
        });

        return item?.Path.LocalPath;
    }
}