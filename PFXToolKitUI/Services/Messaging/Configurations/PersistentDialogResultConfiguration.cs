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

using System.Diagnostics;
using System.Xml;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Persistence.Serialisation;

namespace PFXToolKitUI.Services.Messaging.Configurations;

public class PersistentDialogResultConfiguration : PersistentConfiguration {
    public static readonly PersistentProperty<Dictionary<string, MessageBoxResult>?> PersistentButtonResultsProperty = PersistentProperty.RegisterCustom<Dictionary<string, MessageBoxResult>?, PersistentDialogResultConfiguration>("PersistentButtonResults", null, x => x.myEntries, (x, y) => x.myEntries = y, new DialogEntryListSerializer());

    private Dictionary<string, MessageBoxResult>? myEntries;

    public Dictionary<string, MessageBoxResult>? DialogEntries {
        get => PersistentButtonResultsProperty.GetValue(this);
        set => PersistentButtonResultsProperty.SetValue(this, value);
    }
    
    public static PersistentDialogResultConfiguration Instance => ApplicationPFX.Instance.PersistentStorageManager.GetConfiguration<PersistentDialogResultConfiguration>();

    public PersistentDialogResultConfiguration() {
    }

    static PersistentDialogResultConfiguration() {
        PersistentButtonResultsProperty.DescriptionLines.Add("'preferredbutton' is the result of a message box without it even being shown.");
        PersistentButtonResultsProperty.DescriptionLines.Add("It can be 'OK', 'Cancel', 'Yes' or 'No' (case insensitive)");
    }

    protected internal override void OnLoaded() {
        if (this.myEntries != null) {
            foreach (KeyValuePair<string,MessageBoxResult> entry in this.myEntries) {
                PersistentDialogResult.InternalSetButton(entry.Key, entry.Value);
            }
        }
    }

    private class DialogEntryListSerializer : IValueSerializer<Dictionary<string, MessageBoxResult>?> {
        public bool Serialize(Dictionary<string, MessageBoxResult>? value, XmlDocument document, XmlElement parent) {
            if (value == null) {
                return false;
            }

            foreach (KeyValuePair<string, MessageBoxResult> option in value) {
                XmlElement entry = (XmlElement) parent.AppendChild(document.CreateElement("Dialog"))!;
                entry.SetAttribute("name", option.Key);
                entry.SetAttribute("preferredbutton", option.Value.ToString());
            }

            return true;
        }

        public Dictionary<string, MessageBoxResult> Deserialize(XmlElement element) {
            Dictionary<string, MessageBoxResult> list = new Dictionary<string, MessageBoxResult>();
            foreach (XmlElement dialog in element.GetElementsByTagName("Dialog").OfType<XmlElement>()) {
                string name = dialog.GetAttribute("name");
                string btn = dialog.GetAttribute("preferredbutton");
                if (Enum.TryParse(btn, ignoreCase: true, out MessageBoxResult result) && result != MessageBoxResult.None) {
                    list[name] = result;
                }
            }

            return list;
        }
    }

    public void EnsureAddedOrRemoved(PersistentDialogResult result, bool save) {
        if (!result.IsPersistentOnlyUntilAppCloses && result.Button.HasValue) {
            Dictionary<string, MessageBoxResult> dict = this.DialogEntries ??= new Dictionary<string, MessageBoxResult>();
            if (!dict.TryAdd(result.DialogName, result.Button.Value)) {
                Debug.Assert(dict[result.DialogName] == result.Button.Value);
                return; // do not save when already added
            }
        }
        else if (this.myEntries == null || !this.myEntries.Remove(result.DialogName)) {
            return; // do not save when already removed
        }

        this.MarkModified();
        if (save)
            this.StorageManager.SaveArea(this);
    }
}