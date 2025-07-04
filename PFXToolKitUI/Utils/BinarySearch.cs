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

namespace PFXToolKitUI.Utils;

public static class BinarySearch {
    public static int IndexOf(IList<int> list, int value) {
        int min = 0, max = list.Count - 1;
        while (min <= max) {
            int mid = min + (max - min) / 2;
            int val = list[mid];
            if (val == value)
                return mid;
            else if (value < val)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return ~min;
    }

    public static int IndexOf<T>(IReadOnlyList<T> list, int value, Func<T, int> func) {
        int min = 0, max = list.Count - 1;
        while (min <= max) {
            int mid = min + (max - min) / 2;
            int val = func(list[mid]);
            if (val == value)
                return mid;
            else if (value < val)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return ~min;
    }
}