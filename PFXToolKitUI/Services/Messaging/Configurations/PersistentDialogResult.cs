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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.Messaging.Configurations;

public delegate void PersistentDialogResultEventHandler(PersistentDialogResult sender);

public class PersistentDialogResult {
    private static readonly Dictionary<string, PersistentDialogResult> registry = new Dictionary<string, PersistentDialogResult>();

    private MessageBoxResult? button;
    private bool isPersistentOnlyUntilAppCloses = true;

    /// <summary>
    /// Gets the unique name for this persistent message box result
    /// </summary>
    public string DialogName { get; }

    /// <summary>
    /// Gets or sets the button 
    /// </summary>
    public MessageBoxResult? Button {
        get => this.button;
        private set => PropertyHelper.SetAndRaiseINE(ref this.button, value, this, static t => t.ButtonChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets if this persistent message box result is only persistent until the app closes, meaning the
    /// value resets next app startup. True by default, meaning the button is not saved to the config files
    /// </summary>
    public bool IsPersistentOnlyUntilAppCloses {
        get => this.isPersistentOnlyUntilAppCloses;
        set => PropertyHelper.SetAndRaiseINE(ref this.isPersistentOnlyUntilAppCloses, value, this, static t => t.IsPersistentOnlyUntilAppClosesChanged?.Invoke(t));
    }

    public event PersistentDialogResultEventHandler? ButtonChanged;
    public event PersistentDialogResultEventHandler? IsPersistentOnlyUntilAppClosesChanged;

    public static event PersistentDialogResultEventHandler? InstanceCreated;

    private PersistentDialogResult(string dialogName) {
        this.DialogName = dialogName;
    }

    public void SetButton(MessageBoxResult? newButton, bool isPersistentUntilAppCloses = true, bool saveConfig = true) {
        this.Button = newButton;
        this.IsPersistentOnlyUntilAppCloses = isPersistentUntilAppCloses;
        PersistentDialogResultConfiguration.Instance.EnsureAddedOrRemoved(this, saveConfig);
    }

    public static PersistentDialogResult GetInstance(string dialogName) {
        if (!registry.TryGetValue(dialogName, out PersistentDialogResult? instance)) {
            registry[dialogName] = instance = new PersistentDialogResult(dialogName);
            InstanceCreated?.Invoke(instance);
        }

        return instance;
    }

    public static IList<PersistentDialogResult> GetAllInstances() => registry.Values.ToList();

    internal static void InternalSetButton(string dialogName, MessageBoxResult button) {
        GetInstance(dialogName).Button = button;
    }
}