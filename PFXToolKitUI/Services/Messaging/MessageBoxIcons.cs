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

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// Contains standard message box icons
/// </summary>
public static class MessageBoxIcons {
    // These 4 icons are licenced under MIT https://opensource.org/license/mit
    
    // https://www.svgrepo.com/svg/501232/info
    private const string InfoIconSvgPath = "M48 0c26.5097 0 48 21.4904 48 48s-21.4904 48-48 48S0 74.5097 0 48 21.4904 0 48 0Zm11.1898 35.3574c-1.4265-1.4781-3.3913-1.9972-5.4614-1.9728-2.7613.0329-5.7099 1.0332-7.819 2.0158-5.0471 2.3512-8.9198 6.5147-12.1452 10.9656-.5808.8013-.8839 1.7473.138 2.4849.8714.629 1.4989.0662 2.0245-.4949l.0345-.037c.0401-.0431.0796-.086.1185-.1283.5897-.6386 1.1597-1.2999 1.7297-1.9619l.1425-.1655.1426-.1654c1.7116-1.9844 3.4528-3.9403 5.7572-5.2673 1.37-.7889 2.3571.4295 2.1456 1.7982-.1268.8207-.5583 1.5937-.86 2.3872-1.072 2.8182-2.1599 5.6303-3.2431 8.4444-1.187 3.085-2.3702 6.1713-3.5213 9.2699l-.1.269-.0999.2687c-1.0155 2.732-2.016 5.4436-2.6777 8.2948-.5287 2.2796-1.2406 5.0453-.2179 7.2849.5891 1.29 1.8385 2.1766 3.2283 2.3783 1.8956.2752 3.9453.3067 5.8002-.1154.9608-.2184 1.906-.5035 2.8285-.8502 2.8323-1.0649 5.4113-2.7073 7.7341-4.6377 2.363-1.9692 4.4459-4.2986 6.3453-6.7146.6105-.7765 1.3502-1.6352 1.5582-2.6298.1954-.9329-.6373-2.2651-1.7163-1.7237-.5697.2859-.9964.9934-1.4116 1.4635-.521.5899-1.0522 1.1711-1.5893 1.746-1.0744 1.1493-2.1757 2.2732-3.2817 3.3916-.677.6846-1.5185 1.2632-2.3831 1.6882-1.0795.5304-1.9392-.0579-1.8224-1.2532.1072-1.0977.3758-2.2073.7523-3.2463 1.5153-4.1837 3.0595-8.3567 4.5917-12.5343.9579-2.6107 1.9108-5.2231 2.85-7.8408.8777-2.4464 1.6257-4.8731 1.9417-7.465.2179-1.7855-.245-3.6324-1.5134-4.9469Zm3.186-20.0749c-4.5671-1.7769-10.0116 1.2556-10.9287 6.0879-.6625 3.4892.6668 6.5615 3.3999 7.8578 5.2883 2.508 11.6142-1.4977 11.6145-7.3542.0003-3.2499-1.4306-5.5583-4.0857-6.5914Z";
    
    // Modified version of https://www.svgrepo.com/svg/453443/warning
    private const string WarningIconSvgPath = "M0 19H22L11 0 0 19ZM12 17H10V15H12V17ZM12 12H10V6H12V10Z";
    
    // https://www.svgrepo.com/svg/486408/error-filled
    private const string ErrorIconSvgPath = "M436.896,74.869c-99.84-99.819-262.208-99.819-362.048,0c-99.797,99.819-99.797,262.229,0,362.048    c49.92,49.899,115.477,74.837,181.035,74.837s131.093-24.939,181.013-74.837C536.715,337.099,536.715,174.688,436.896,74.869z     M361.461,331.317c8.341,8.341,8.341,21.824,0,30.165c-4.16,4.16-9.621,6.251-15.083,6.251c-5.461,0-10.923-2.091-15.083-6.251    l-75.413-75.435l-75.392,75.413c-4.181,4.16-9.643,6.251-15.083,6.251c-5.461,0-10.923-2.091-15.083-6.251    c-8.341-8.341-8.341-21.845,0-30.165l75.392-75.413l-75.413-75.413c-8.341-8.341-8.341-21.845,0-30.165    c8.32-8.341,21.824-8.341,30.165,0l75.413,75.413l75.413-75.413c8.341-8.341,21.824-8.341,30.165,0    c8.341,8.32,8.341,21.824,0,30.165l-75.413,75.413L361.461,331.317z";
    
    // https://www.svgrepo.com/svg/486470/question-filled
    private const string QuestionIconSvgPath = "M48 0C74.5095 0 96 21.4901 96 48 96 74.5095 74.5095 96 48 96 21.4904 96 0 74.5095 0 48 0 21.4901 21.4904 0 48 0ZM48 63.6C44.6859 63.6 41.9997 66.2862 42 69.6 42 72.9137 44.6859 75.6 48 75.6 51.3134 75.6 54 72.9137 54 69.6 54 66.2862 51.3134 63.6 48 63.6ZM47.199 21.0003C42.5267 21.0003 38.5998 22.2732 35.4228 24.8118 31.6582 27.8528 29.7773 32.344 29.7773 38.2889L29.7773 38.2889 39.9837 38.2889 39.9837 38.2204C39.9837 35.9522 40.4616 34.0916 41.4146 32.6405 42.73 30.6896 44.8635 29.7135 47.8128 29.7135 49.6277 29.7135 51.1725 30.1898 52.4408 31.1411 54.0281 32.4592 54.8244 34.4543 54.8244 37.1314 54.8244 38.8107 54.4139 40.3072 53.5983 41.6238 52.9174 42.8032 51.8288 43.9593 50.3294 45.0946 47.1536 47.2722 45.0887 49.4279 44.1372 51.5616 43.3204 53.3311 42.9099 56.1442 42.9099 60L42.9099 60 52.5096 60C52.5096 57.4585 52.8474 55.5539 53.5312 54.283 54.0746 53.2392 55.2102 52.1285 56.9346 50.9478 59.9305 48.7236 62.0613 46.6585 63.3328 44.7527 64.8747 42.4849 65.6475 39.8294 65.6475 36.7884 65.6475 30.4863 63.0843 26.0159 57.9536 23.3827 54.8698 21.7954 51.285 21.0003 47.199 21.0003Z";

    public static readonly Icon InfoIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(InfoIcon),
            [new GeometryEntry(InfoIconSvgPath, BrushManager.Instance.CreateConstant(SKColors.DodgerBlue))]);

    public static readonly Icon WarningIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(WarningIcon),
            [new GeometryEntry(WarningIconSvgPath, BrushManager.Instance.CreateConstant(SKColors.Goldenrod))]);

    public static readonly Icon ErrorIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(ErrorIcon),
            [new GeometryEntry(ErrorIconSvgPath, BrushManager.Instance.CreateConstant(SKColors.IndianRed))]);

    public static readonly Icon QuestionIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(QuestionIcon),
            [new GeometryEntry(QuestionIconSvgPath, BrushManager.Instance.CreateConstant(SKColors.DeepSkyBlue))]);
}