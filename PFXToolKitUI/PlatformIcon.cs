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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI;

/// <summary>
/// Contains icons for the different supported platforms
/// </summary>
public static class PlatformIcon {
    // Windows Icon by Pictonic - https://www.svgrepo.com/svg/508993/windows
    // Licenced under 'CC BY' - https://creativecommons.org/share-your-work/cclicenses/
    // This is a modified version, which splits each sector into 4 to allow colouring
    public static readonly Icon WindowsIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(WindowsIcon),
            [
                new GeometryEntry("M106.169 21.6196c0 0 102.123-55.525 189.491 11.392l-48.896 167.995c0 0-54.756-37.67-95.361-33.602-40.605 4.079-94.13 23.508-94.13 23.508L106.169 21.6196z", BrushManager.Instance.CreateConstant(SKColor.Parse("#F24020"))),
                new GeometryEntry("M0 389.7476c22.54-11.653 56.372-17.559 95.35-20.823 39.011-3.255 93.91 32.468 93.91 32.468l49.401-169.776c-7.817-5.618-44.168-30.094-89.666-33.348-45.52-3.255-100.354 22.243-100.354 22.243L0 389.7476z", BrushManager.Instance.CreateConstant(SKColor.Parse("#00A4FF"))),
                new GeometryEntry("M215.604 416.3466c8.928 3.519 38.22 31.38 86.972 33.854 48.763 2.419 102.541-22.749 102.541-22.749s47.894-167.6 48.861-168.249c.33-.176-.604.484-.968.649-45.596 17.845-68.872 20.506-102.496 21.309-34.141.824-86.269-33.095-86.269-33.095L215.604 416.3466z", BrushManager.Instance.CreateConstant(SKColor.Parse("#FFB800"))),
                new GeometryEntry("M321.51 50.7566l-48.995 167.017c0 0 39.803 39 95.867 35.768 56.098-3.267 86.532-20.945 93.976-23.442L511 61.5976c0 0-64.606 26.982-106.851 22.09C361.884 78.7946 335.594 60.8386 321.51 50.7566z", BrushManager.Instance.CreateConstant(SKColor.Parse("#7FBA00"))),
            ]);
    
    public static readonly Icon BlankIcon = IconManager.Instance.RegisterGeometryIcon(nameof(BlankIcon), []);
}