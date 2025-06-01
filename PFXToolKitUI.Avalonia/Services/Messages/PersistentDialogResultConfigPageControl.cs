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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity.Selecting;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Configurations.Dialogs;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Services.Messages;

public class PersistentDialogResultConfigPageControl : BaseConfigurationPageControl, IPersistentDialogResultConfigurationPageUI {
    private PersistentDialogResultConfigurationPage? myPage;
    
    private DataGrid? myDataGrid;

    PersistentDialogResultConfigurationPage IPersistentDialogResultConfigurationPageUI.Page => this.myPage ?? throw new InvalidOperationException("Not connected to a page");
    
    public IListSelectionManager<PersistentDialogResultViewModel> SelectionManager { get; private set; }

    public PersistentDialogResultConfigPageControl() {
        this.SelectionManager = new DataGridSelectionManager<PersistentDialogResultViewModel>();
        DataManager.GetContextData(this).Set(IPersistentDialogResultConfigurationPageUI.DataKey, this);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.myDataGrid = e.NameScope.GetTemplateChild<DataGrid>("PART_DataGrid");
        ((DataGridSelectionManager<PersistentDialogResultViewModel>) this.SelectionManager).DataGrid = this.myDataGrid;
        
        if (this.myPage != null) {
            this.myDataGrid.ItemsSource = this.myPage.PersistentDialogResults;
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.myPage = (PersistentDialogResultConfigurationPage) this.Page!;
        if (this.myDataGrid != null) {
            this.myDataGrid.ItemsSource = this.myPage.PersistentDialogResults;
        }
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.myPage = null;
        if (this.myDataGrid != null) {
            this.myDataGrid.ItemsSource = null;
        }
    }
}