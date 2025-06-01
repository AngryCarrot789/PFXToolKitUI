// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of MemEngine360.
// 
// MemEngine360 is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// MemEngine360 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MemEngine360. If not, see <https://www.gnu.org/licenses/>.
// 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.Messaging.Configurations;

namespace PFXToolKitUI.Configurations.Dialogs;

public class PersistentDialogResultViewModel : INotifyPropertyChanged {
    private bool fuckYouAvaloniaJustFuckingWorkProperly;
    
    public PersistentDialogResult PersistentDialogResult { get; }

    public string DialogName => this.PersistentDialogResult.DialogName;

    public MessageBoxResult? Button { get; private set; }

    public bool FuckYouAvaloniaJustFuckingWorkProperly {
        get => this.fuckYouAvaloniaJustFuckingWorkProperly;
        set {
            this.fuckYouAvaloniaJustFuckingWorkProperly = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FuckYouAvaloniaJustFuckingWorkProperly)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PersistentDialogResultViewModel(PersistentDialogResult persistentDialogResult) {
        this.PersistentDialogResult = persistentDialogResult;
        this.PersistentDialogResult.IsPersistentOnlyUntilAppClosesChanged += this.OnIsPersistentOnlyUntilAppClosesChanged;
        this.PersistentDialogResult.ButtonChanged += this.OnButtonChanged;
        this.fuckYouAvaloniaJustFuckingWorkProperly = persistentDialogResult.IsPersistentOnlyUntilAppCloses;
        this.Button = persistentDialogResult.Button;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnIsPersistentOnlyUntilAppClosesChanged(PersistentDialogResult sender) {
        this.FuckYouAvaloniaJustFuckingWorkProperly = sender.IsPersistentOnlyUntilAppCloses;
    }
    
    private void OnButtonChanged(PersistentDialogResult sender) {
        this.Button = sender.Button;
        this.OnPropertyChanged(nameof(this.Button));
    }

    public void Dispose() {
        this.PersistentDialogResult.IsPersistentOnlyUntilAppClosesChanged -= this.OnIsPersistentOnlyUntilAppClosesChanged;
        this.PersistentDialogResult.ButtonChanged -= this.OnButtonChanged;
    }

    public void SetButtonToNull() {
        this.Button = null;
        this.OnPropertyChanged(nameof(this.Button));
    }
}