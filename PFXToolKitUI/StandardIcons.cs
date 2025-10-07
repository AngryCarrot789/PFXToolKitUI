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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI;

public static class StandardIcons {
    public static readonly IConstantColourBrush TransparentBrush = BrushManager.Instance.CreateConstant(SKColors.Transparent);
    public static readonly IDynamicColourBrush GlyphBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static");
    public static readonly IDynamicColourBrush ForegroundBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Foreground.Static");
    public static readonly IDynamicColourBrush DisabledGlyphBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Disabled");
    
    // An X shape
    public static readonly Icon CancelActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(CancelActivityIcon),
            [new GeometryEntry("m.0005 1.9789q-0-.3411.2388-.5798l1.1599-1.1598q.2388-.2388.5799-.2388.3411-0 .5799.2388l2.5077 2.507 2.5077-2.507q.2388-.2388.5799-.2388.3411-0 .5799.2388l1.1599 1.1598Q10.1336 1.6378 10.1336 1.9789t-.2388.5799l-2.5077 2.507L9.8949 7.5726Q10.1337 7.8114 10.1337 8.1524t-.2388.5799l-1.1599 1.1598q-.2388.2386-.58.2387-.3411 0-.58-.2387l-2.5077-2.507-2.5077 2.507q-.2388.2386-.58.2387-.3411 0-.58-.2387l-1.1599-1.1598q-.2388-.2388-.2388-.5799t.2388-.5798L2.7469 5.0658.2392 2.5588q-.2388-.2388-.2388-.5799z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"))]);

    // Arrow
    private const string SmallerRunActivityArrowSvg = "M7.8496 3.676 2.144.2032C1.1888-.378 0 .3572 0 1.527V8.4727c0 1.1714 1.1888 1.905 2.144 1.324L7.8496 6.3253c.9624-.5857.9624-2.0635 0-2.6493";
    private const string LargerRunActivityArrowSvg = "M9.4195 4.4112 2.5728.2438C1.4265-.4536 0 .4286 0 1.8324v8.3349c0 1.4057 1.4265 2.286 2.5728 1.5888L9.4195 7.5903c1.1549-.7028 1.1549-2.4762 0-3.1791";
    
    public static readonly Icon SmallContinueActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(SmallContinueActivityIcon),
            [new GeometryEntry(SmallerRunActivityArrowSvg, BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"))]);

    public static readonly Icon SmallContinueActivityIconColourful =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(SmallContinueActivityIconColourful),
            [new GeometryEntry(SmallerRunActivityArrowSvg, BrushManager.Instance.CreateConstant(SKColors.MediumSeaGreen))]);

    public static readonly Icon SmallContinueActivityIconDisabled =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(SmallContinueActivityIconDisabled),
            [new GeometryEntry(SmallerRunActivityArrowSvg, DisabledGlyphBrush)]);
    
    public static readonly Icon LargeContinueActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(LargeContinueActivityIcon),
            [new GeometryEntry(LargerRunActivityArrowSvg, BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"))]);

    public static readonly Icon LargeContinueActivityIconColourful =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(LargeContinueActivityIconColourful),
            [new GeometryEntry(LargerRunActivityArrowSvg, BrushManager.Instance.CreateConstant(SKColors.MediumSeaGreen))]);

    public static readonly Icon LargeContinueActivityIconDisabled =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(LargeContinueActivityIconDisabled),
            [new GeometryEntry(LargerRunActivityArrowSvg, DisabledGlyphBrush)]);

    // "||" shape
    public static readonly Icon PauseActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(PauseActivityIcon),
            [new GeometryEntry("M0 8 2 8 2 0 0 0 0 8ZM4 8 6 8 6 0 4 0 4 8Z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"))]);

    // https://www.svgrepo.com/svg/501539/drag-handle -- Copyright MIT licenced
    public static readonly Icon DragGripIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(DragGripIcon),
            [new GeometryEntry("M6.8572 6.8572v-.0069l3.4286.0069H6.8571Zm0 75.4286c3.7783 0 6.8572 3.0789 6.8572 6.8572S10.6355 96 6.8572 96C3.0721 96 .0001 92.9212.0001 89.1429s3.072-6.8572 6.8571-6.8572Zm27.4286 0c3.7783 0 6.8572 3.0789 6.8572 6.8572S38.0641 96 34.2858 96c-3.7851 0-6.8572-3.0789-6.8572-6.8572s3.072-6.8572 6.8572-6.8572ZM6.8571 54.8572c3.7783 0 6.8572 3.0789 6.8572 6.8572 0 3.7783-3.0789 6.8572-6.8572 6.8572C3.072 68.5715 0 65.4926 0 61.7143c0-3.7783 3.072-6.8572 6.8571-6.8572Zm27.4286 0c3.7783 0 6.8572 3.0789 6.8572 6.8572 0 3.7783-3.0789 6.8572-6.8572 6.8572-3.7851 0-6.8572-3.0789-6.8572-6.8572 0-3.7783 3.072-6.8572 6.8572-6.8572ZM6.8571 27.4285c3.7783 0 6.8572 3.0789 6.8572 6.8572 0 3.7783-3.0789 6.8572-6.8572 6.8572C3.072 41.1428 0 38.064 0 34.2857 0 30.5074 3.072 27.4285 6.8571 27.4285Zm27.4286 0c3.7783 0 6.8572 3.0789 6.8572 6.8572 0 3.7783-3.0789 6.8572-6.8572 6.8572-3.7851 0-6.8572-3.0789-6.8572-6.8572 0-3.7782 3.072-6.8572 6.8572-6.8572ZM6.8571 0c3.7783 0 6.8572 3.0789 6.8572 6.8572S10.6354 13.7143 6.8571 13.7143C3.072 13.7143 0 10.6355 0 6.8572S3.0721 0 6.8571 0Zm27.4252 0c3.7783 0 6.8572 3.0789 6.8572 6.8572s-3.0789 6.8572-6.8572 6.8572c-3.7782 0-6.8572-3.0789-6.8572-6.8572S30.5041 0 34.2823 0Z", GlyphBrush)]);

    public static readonly Icon StopIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(StopIcon),
            [new GeometryEntry("M0 10V0H10V10H0", GlyphBrush)]);

    public static readonly Icon StopIconColourful =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(StopIconColourful),
            [new GeometryEntry("M0 10V0H10V10H0", BrushManager.Instance.CreateConstant(SKColors.Red))]);

    public static readonly Icon StopIconDisabled =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(StopIconDisabled),
            [new GeometryEntry("M0 10V0H10V10H0", DisabledGlyphBrush)]);
    
    // https://www.svgrepo.com/svg/486816/rename
    public static readonly Icon ABCTextIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(ABCTextIcon),
            [new GeometryEntry("M34.5745 27.5184c-2.1173.315-3.9546 1.6275-4.9393 3.5279v7.7617c.9302 2.1029 2.993 3.4782 5.292 3.5279 2.8225 0 4.5863-2.8225 4.5863-7.4088 0-4.5863-2.1168-7.4088-4.9391-7.4088ZM16.2288 35.2799H14.112c-4.2336.3529-5.9976 1.7641-5.9976 4.2336-.064.8601.2499 1.7055.8598 2.3154.6099.6099 1.4552.9238 2.3154.8598 2.0786-.1513 3.9386-1.347 4.9392-3.1752V35.2799Zm3.5281-8.4672c1.2449 2.1285 1.7417 4.6125 1.4111 7.0561v6.3504c-.0698 1.8886.0483 3.7795.3527 5.6447H16.9344V43.3944c-1.6836 1.9185-4.1543 2.9589-6.7032 2.8225-1.8052.2133-3.6148-.3615-4.966-1.5775-1.3511-1.216-2.1128-2.9553-2.09-4.7729-.2562-2.5544 1.171-4.9806 3.528-5.9977 2.013-.8161 4.1815-1.1776 6.3504-1.0584h3.5281v-.3527c.1469-1.3498-.3261-2.6933-1.2861-3.6533-.96-.96-2.3035-1.433-3.6532-1.286-2.0962.1856-4.1347.7852-5.9976 1.7641L4.2336 25.7543c2.516-1.2537 5.305-1.86 8.1144-1.7639 2.7726-.2514 5.5062.79 7.4088 2.8223Zm9.8783-11.9951V26.8127c1.5064-1.9901 3.8544-3.1641 6.3504-3.1752 5.292 0 8.8199 4.5865 8.8199 11.2897-.1821 3.1855-1.2851 6.2495-3.1752 8.8199-1.4571 1.8152-3.6702 2.8565-5.9975 2.8225-2.6061.0878-5.0482-1.2689-6.3504-3.5279v3.1752H24.3432c0-.789.0361-1.4699.085-2.1462l.0131-.1765.0136-.1766c.1034-1.3265.2411-2.7045.2411-4.9095V14.8176h4.9393ZM45.8639 10.584H0V52.92H45.8639V10.584Zm24.6961 0H56.4479v7.0561H63.504V45.8639H56.4479V52.92H70.5601V10.584ZM59.9761 0H42.336V3.528l7.0559-0V59.9759l-7.0559.0002V63.504l7.0559-.0002.0002.0002H52.92v-.0002l7.0561.0002V59.9761L52.92 59.9759V3.5279l7.0561 0V0Z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"))]);

    public static readonly Icon ResetIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(ResetIcon),
            [
                new GeometryEntry("M 8 1.4615 C 10.3018 1.4615 12.3774 2.4304 13.842 3.9828 L 13.842 0 L 16.0327 2.1923 L 16.0328 8.0385 L 10.1907 8.0385 L 8 5.8461 L 12.0823 5.8461 C 11.0794 4.7249 9.622 4.0192 8 4.0192 C 5.2229 4.0192 2.9284 6.0878 2.5714 8.7693 L 0 8.7693 C 0.369 4.6721 3.8098 1.4615 8 1.4615 Z M 8 14.9808 C 10.7772 14.9808 13.0716 12.9122 13.4285 10.2308 L 16 10.2308 C 15.6311 14.3279 12.1902 17.5385 8 17.5385 C 5.6982 17.5385 3.6225 16.5697 2.158 15.0172 L 2.158 19 L -0.0328 16.8077 L -0.0328 10.9615 L 5.8092 10.9615 L 8 13.1539 L 3.9177 13.1539 C 4.9206 14.2752 6.378 14.9808 8 14.9808", StandardIcons.GlyphBrush)
            ]);

    public const string SvgSmallIcon = "M4 0 6 0 6 4 10 4 10 6 6 6 6 10 4 10 4 6 0 6 0 4 4 4 4 0";
    public const string SvgLargeIcon = "M5 0 7 0 7 5 12 5 12 7 7 7 7 12 5 12 5 7 0 7 0 5 5 5 5 0";
    public static readonly Icon SmallAddIcon = IconManager.Instance.RegisterGeometryIcon(nameof(SmallAddIcon), [new GeometryEntry(SvgSmallIcon, GlyphBrush)]);
    public static readonly Icon SmallGreenAddIcon = IconManager.Instance.RegisterGeometryIcon(nameof(SmallGreenAddIcon), [new GeometryEntry(SvgSmallIcon, BrushManager.Instance.CreateConstant(SKColors.LimeGreen))]);
    public static readonly Icon LargeAddIcon = IconManager.Instance.RegisterGeometryIcon(nameof(LargeAddIcon), [new GeometryEntry(SvgLargeIcon, GlyphBrush)]);
    public static readonly Icon LargeGreenAddIcon = IconManager.Instance.RegisterGeometryIcon(nameof(LargeGreenAddIcon), [new GeometryEntry(SvgLargeIcon, BrushManager.Instance.CreateConstant(SKColors.LimeGreen))]);
}