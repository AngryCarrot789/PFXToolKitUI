// 
// Copyright (c) 2023-2025 REghZy
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

using System.Collections.ObjectModel;
using System.ComponentModel;
using PFXToolKitUI.Services.Messaging.Configurations;

namespace PFXToolKitUI.Configurations.Dialogs;

public class PersistentDialogResultConfigurationPage : ConfigurationPage {
    private readonly ObservableCollection<PersistentDialogResultViewModel> myList;
    
    public ReadOnlyObservableCollection<PersistentDialogResultViewModel> PersistentDialogResults { get; }

    public PersistentDialogResultConfigurationPage() {
        this.myList = new ObservableCollection<PersistentDialogResultViewModel>();
        this.PersistentDialogResults = new ReadOnlyObservableCollection<PersistentDialogResultViewModel>(this.myList);
    }

    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        foreach (PersistentDialogResult result in PersistentDialogResult.GetAllInstances()) {
            PersistentDialogResultViewModel vm = new PersistentDialogResultViewModel(result);
            vm.PropertyChanged += this.OnVMPropertyChanged;
            this.myList.Add(vm);
        }

        PersistentDialogResult.InstanceCreated += this.OnPersistentDialogResultCreated;
        return base.OnContextCreated(context);
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        PersistentDialogResult.InstanceCreated -= this.OnPersistentDialogResultCreated;
        this.ClearItems();
        return base.OnContextDestroyed(context);
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        foreach (PersistentDialogResultViewModel vm in this.myList) {
            vm.PersistentDialogResult.SetButton(vm.Button, vm.IsPersistentOnlyUntilAppCloses, false);
        }

        PersistentDialogResultConfiguration.Instance.StorageManager.SaveArea(PersistentDialogResultConfiguration.Instance);
        return ValueTask.CompletedTask;
    }

    private void OnVMPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        this.IsModified = true;
    }

    // Hook onto creation event, just in case a dialog is opened when the settings are open... somehow
    private void OnPersistentDialogResultCreated(PersistentDialogResult sender) {
        this.myList.Add(new PersistentDialogResultViewModel(sender));
    }

    public void RemoveItems(IEnumerable<PersistentDialogResultViewModel> items) {
        foreach (PersistentDialogResultViewModel vm in items) {
            if (this.myList.Remove(vm)) {
                vm.PropertyChanged -= this.OnVMPropertyChanged;
                vm.Dispose();
            }
        }
    }

    public void ClearItems() {
        foreach (PersistentDialogResultViewModel vm in this.myList) {
            vm.PropertyChanged -= this.OnVMPropertyChanged;
            vm.Dispose(); // unhook event handler
        }

        this.myList.Clear();
    }
}