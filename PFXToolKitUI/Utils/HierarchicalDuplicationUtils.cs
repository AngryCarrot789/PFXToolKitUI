// 
// Copyright (c) 2025-2025 REghZy
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

public static class HierarchicalDuplicationUtils {
    public static Dictionary<TParent, List<(T, int)>> GetEffectiveOrderedDuplication<T, TParent>(
        List<T> itemsToDuplicate, 
        Func<T, TParent?> getParentProc, 
        Func<T, int> getIdxInParentProc) 
        where T : class 
        where TParent : T {
        List<T> roots = [];
        foreach (T item in itemsToDuplicate) {
            for (int i = roots.Count - 1; i >= 0; i--) {
                if (IsParent(roots[i], item, getParentProc) || IsParent(item, roots[i], getParentProc)) {
                    roots.RemoveAt(i);
                }
            }

            roots.Add(item);
        }

        Dictionary<TParent, List<(T, int)>> items = [];
        foreach (T item in roots) {
            if (!items.TryGetValue(getParentProc(item)!, out List<(T, int)>? entry))
                items[getParentProc(item)!] = entry = [];
            entry.Add((item, getIdxInParentProc(item)));
        }

        foreach (KeyValuePair<TParent, List<(T, int)>> entry in items) {
            entry.Value.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        }

        return items;
    }

    private static bool IsParent<T, TParent>(T @this, T check, Func<T, TParent?> getParentProcedure, bool self = true) where T : class where TParent : T {
        for (T? entry = self ? @this : getParentProcedure(@this); entry != null; entry = getParentProcedure(entry)) {
            if (ReferenceEquals(entry, check)) {
                return true;
            }
        }

        return false;
    }
}