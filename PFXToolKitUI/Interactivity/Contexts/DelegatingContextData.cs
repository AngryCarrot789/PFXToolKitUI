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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

public class DelegatingContextData : IContextData {
    private readonly IContextData[] context;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.context.SelectMany(x => x.Entries);

    public DelegatingContextData(IContextData data1) : this([data1]) { }

    public DelegatingContextData(IContextData data1, IContextData data2) : this([data1, data2]) { }

    public DelegatingContextData(params IContextData[] context) {
        this.context = context;
    }

    public bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        // Scan from bottom to top, which is behaviour that the DataManager uses for the logical tree
        for (int i = this.context.Length - 1; i >= 0; i--) {
            if (this.context[i].TryGetContext(key, out value)) {
                return true;
            }
        }

        value = null;
        return false;
    }

    public bool ContainsKey(string key) => this.context.Any(x => x.ContainsKey(key));
}