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

using Avalonia;
using Avalonia.Media;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Icons;

public static class StretchModeExtensions {
    /// <summary>
    /// Calculates scaling based on a <see cref="Stretch"/> value.
    /// </summary>
    /// <param name="s">The stretch mode.</param>
    /// <param name="dstSize">The size of the destination viewport.</param>
    /// <param name="srcSize">The size of the source.</param>
    /// <param name="stretchDirection">The stretch direction.</param>
    /// <returns>A vector with the X and Y scaling factors.</returns>
    public static Vector CalculateScaling(this StretchMode s, Size dstSize, Size srcSize, StretchDirection stretchDirection = StretchDirection.Both) {
        double scaleX = 1.0;
        double scaleY = 1.0;

        bool isConstrainedWidth = !double.IsPositiveInfinity(dstSize.Width);
        bool isConstrainedHeight = !double.IsPositiveInfinity(dstSize.Height);

        if (IsNotNone(s) && (isConstrainedWidth || isConstrainedHeight)) {
            // Compute scaling factors for both axes
            scaleX = DoubleUtils.IsZero(srcSize.Width) ? 0.0 : dstSize.Width / srcSize.Width;
            scaleY = DoubleUtils.IsZero(srcSize.Height) ? 0.0 : dstSize.Height / srcSize.Height;

            if (!isConstrainedWidth) {
                scaleX = scaleY;
            }
            else if (!isConstrainedHeight) {
                scaleY = scaleX;
            }
            else {
                // If not preserving aspect ratio, then just apply transform to fit
                switch (s) {
                    case StretchMode.Fill:
                        // We already computed the fill scale factors above, so just use them
                        break;
                    
                    case StretchMode.Uniform:
                    case StretchMode.UniformNoUpscale:
                        // Find minimum scale that we use for both axes
                        double minScale = scaleX < scaleY ? scaleX : scaleY;
                        if (s == StretchMode.UniformNoUpscale && minScale > 1.0)
                            minScale = 1.0;
                        
                        scaleX = scaleY = minScale;
                        break;

                    case StretchMode.UniformToFill:
                        // Find maximum scale that we use for both axes
                        double maxScale = scaleX > scaleY ? scaleX : scaleY;
                        scaleX = scaleY = maxScale;
                        break;
                }
            }

            // Apply stretch direction by bounding scales.
            // In the uniform case, scaleX=scaleY, so this sort of clamping will maintain aspect ratio
            // In the uniform fill case, we have the same result too.
            // In the fill case, note that we change aspect ratio, but that is okay
            switch (stretchDirection) {
                case StretchDirection.UpOnly:
                    if (scaleX < 1.0)
                        scaleX = 1.0;
                    if (scaleY < 1.0)
                        scaleY = 1.0;
                    break;

                case StretchDirection.DownOnly:
                    if (scaleX > 1.0)
                        scaleX = 1.0;
                    if (scaleY > 1.0)
                        scaleY = 1.0;
                    break;

                case StretchDirection.Both: break;
            }
        }

        return new Vector(scaleX, scaleY);
    }

    private static bool IsNotNone(StretchMode s) {
        return s >= StretchMode.Fill && s <= StretchMode.UniformNoUpscale;
    }

    /// <summary>
    /// Calculates a scaled size based on a <see cref="StretchMode"/> value.
    /// </summary>
    /// <param name="s">The stretch mode.</param>
    /// <param name="dstSize">The size of the destination viewport.</param>
    /// <param name="srcSize">The size of the source.</param>
    /// <param name="stretchDirection">The stretch direction.</param>
    /// <returns>The size of the stretched source.</returns>
    public static Size CalculateSize(this StretchMode s, Size dstSize, Size srcSize, StretchDirection stretchDirection = StretchDirection.Both) {
        return srcSize * s.CalculateScaling(dstSize, srcSize, stretchDirection);
    }
}