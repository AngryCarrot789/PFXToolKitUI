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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate void DynamicGenerateContextFunction(DynamicContextGroup group, IContextData ctx, List<IContextObject> items);

/// <summary>
/// A dynamic group. The docs for <see cref="IContextGroup"/> explain this better, but this class
/// contains a generator which generates the context objects based on the current state of the
/// application and also the <see cref="IContextData"/> provided to the generator
/// </summary>
public class DynamicContextGroup : IContextGroup {
    private readonly DynamicGenerateContextFunction generate;

    public DynamicContextGroup(DynamicGenerateContextFunction generate) {
        ArgumentNullException.ThrowIfNull(generate);
        this.generate = generate;
    }

    public List<IContextObject> GenerateItems(IContextData context) {
        ArgumentNullException.ThrowIfNull(context);
        
        List<IContextObject> list = new List<IContextObject>();
        this.generate(this, context, list);
        return list;
    }
}