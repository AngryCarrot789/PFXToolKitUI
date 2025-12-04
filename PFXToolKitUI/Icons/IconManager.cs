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

using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI.Icons;

/// <summary>
/// A class that manages a set of registered icons throughout the application. This is used to simply icon usage
/// </summary>
public abstract class IconManager {
    public static IconManager Instance => ApplicationPFX.GetComponent<IconManager>();

    private readonly Dictionary<string, Icon> nameToIcon;

    protected IconManager() {
        this.nameToIcon = new Dictionary<string, Icon>();
    }

    protected void ValidateName(string name) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (this.nameToIcon.ContainsKey(name))
            throw new InvalidOperationException("Icon name already in use: '" + name + "'");
    }

    protected Icon RegisterHelper(Icon icon) {
        this.AddIcon(icon.Name, icon);
        return icon;
    }

    public bool IconExists(string name) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return this.GetIconFromName(name) != null;
    }

    /// <summary>
    /// Gets an icon key from the name it was registered with
    /// </summary>
    /// <param name="name">The name of the icon</param>
    /// <returns>The icon key, if found</returns>
    public virtual Icon? GetIconFromName(string name) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return this.nameToIcon.GetValueOrDefault(name);
    }

    /// <summary>
    /// Registers an icon that is based on an image on the disk somewhere
    /// </summary>
    /// <param name="name">A globally identifiable key for the icon</param>
    /// <param name="filePath">The file path</param>
    /// <param name="lazilyLoad">True to load only on demand, False to load during the execution of this method (blocking)</param>
    /// <returns></returns>
    public abstract Icon RegisterIconByFilePath(string name, string filePath, bool lazilyLoad = true);
    
    /// <summary>
    /// Registers an icon that is based on an image on the disk somewhere
    /// </summary>
    /// <param name="name">A globally identifiable key for the icon</param>
    /// <param name="uri">The uri</param>
    /// <param name="lazilyLoad">True to load only on demand, False to load during the execution of this method (blocking)</param>
    /// <returns></returns>
    public abstract Icon RegisterIconByUri(string name, Uri uri, bool lazilyLoad = true);

    /// <summary>
    /// Registers an icon that draws the given bitmap data
    /// </summary>
    public abstract Icon RegisterIconUsingBitmap(string name, SKBitmap bitmap);

    /// <summary>
    /// Registers an icon that uses one or more SVG path elements to compose a final shape
    /// </summary>
    public abstract Icon RegisterGeometryIcon(string name, GeometryEntry[] geometry);
    
    /// <summary>
    /// Registers an icon that draws an ellipse shape
    /// </summary>
    public abstract Icon RegisterEllipseIcon(string name, IColourBrush? fill, IColourBrush? stroke, double radiusX, double radiusY, double strokeThickness = 0.0);
    
    /// <summary>
    /// Adds the icon key, with the given name. Throws if the name is
    /// invalid (null, empty or whitespaces) or an icon exists with the name already
    /// </summary>
    /// <param name="name">The icon name</param>
    /// <param name="key">The icon key</param>
    protected void AddIcon(string name, Icon key) {
        this.ValidateName(name);
        ArgumentNullException.ThrowIfNull(key);

        this.nameToIcon.Add(name, key);
    }
}