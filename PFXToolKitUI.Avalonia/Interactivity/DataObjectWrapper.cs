﻿// 
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

using Avalonia.Input;
using Avalonia.Platform.Storage;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Interactivity;

public class DataObjectWrapper : IDataObjekt {
    private readonly IDataObject mObject;

    public IDataObject RawDataObject => this.mObject;
    
    public DataObjectWrapper(IDataObject mObject) {
        this.mObject = mObject;
    }

    public object? GetData(string format) {
        object? value = this.mObject.Get(format);

        switch (format) {
            //case "Text":
            //case "UnicodeText":
            //case "Dib":
            //case "Bitmap":
            //case "EnhancedMetafile":
            //case "MetafilePicture":
            //case "SymbolicLink":
            //case "Dif":
            //case "Tiff":
            //case "OemText":
            //case "Palette":
            //case "PenData":
            //case "Riff":
            //case "WaveAudio":
            case "Files":
                if (value is IEnumerable<IStorageItem> items)
                    return items.Select(x => x.Path.LocalPath).ToArray();
            break;
            //case "Locale":
            //case "Html":
            //case "Rtf":
            //case "CommaSeparatedValue":
            //case "StringFormat":
            //case "Serializable":
            //case "Xaml":
            //case "XamlPackage":
            default: break;
        }

        return value;
    }

    public bool Contains(string format) {
        return this.mObject.Contains(format);
    }

    public IEnumerable<string> GetFormats() {
        return this.mObject.GetDataFormats();
    }

    public void SetData(string format, object data) {
        (this.mObject as DataObject)?.Set(format, data);
    }
}