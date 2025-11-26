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

using System.Diagnostics;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Interactivity.Selections;

public static class SelectionUtils {
    public static int GetIndexOfNthSelectedItem<T>(int index, HashSet<T> selection, IObservableList<T> sourceList) {
        if (index == 0)
            return GetIndexOfFirstSelectedItem(selection, sourceList);
        if (index == selection.Count - 1)
            return GetIndexOfLastSelectedItem(selection, sourceList);
        return InternalGetIndexOfNthSelectedItem(index, selection, sourceList);
    }

    public static int GetIndexOfFirstSelectedItem<T>(HashSet<T> selection, IObservableList<T> sourceList) {
        using (RentHelper.RentSpan(selection.Count, out Span<int> span)) {
            int i = 0;
            foreach (T item in selection) {
                int idx = sourceList.IndexOf(item);
                Debug.Assert(idx != -1, "Corrupted selection model");
                if (idx == 0)
                    return 0; // short path; first item is selected

                span[i++] = idx;
            }

            return span[0]; // should not throw
        }
    }

    public static int GetIndexOfLastSelectedItem<T>(HashSet<T> selection, IObservableList<T> sourceList) {
        int count = selection.Count;
        int endIndex = sourceList.Count - 1;

        using (RentHelper.RentSpan(count, out Span<int> span)) {
            int i = 0;
            foreach (T item in selection) {
                int idx = sourceList.IndexOf(item);
                Debug.Assert(idx != -1, "Corrupted selection model");
                if (idx == endIndex)
                    return endIndex; // short path; last item is selected

                span[i++] = idx;
            }

            return span[count - 1]; // should not throw
        }
    }
    
    private static int InternalGetIndexOfNthSelectedItem<T>(int index, HashSet<T> selection, IObservableList<T> sourceList) {
        using (RentHelper.RentSpan(selection.Count, out Span<int> span)) {
            int i = 0;
            foreach (T item in selection) {
                int idx = sourceList.IndexOf(item);
                Debug.Assert(idx != -1, "Corrupted selection model");
                span[i++] = idx;
            }

            return span[index]; // should not throw
        }
    }
}