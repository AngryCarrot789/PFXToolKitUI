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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.AdvancedMenuService;


/// <summary>
/// A context entry that has a <see cref="CanExecute"/> and <see cref="OnExecute"/> method
/// </summary>
public class CustomLambdaContextEntry : CustomContextEntry {
    private readonly Func<IContextData, Task> execute;
    private readonly Predicate<IContextData>? canExecute;
    
    public CustomLambdaContextEntry(string displayName, Func<IContextData, Task> execute, Predicate<IContextData>? canExecute) : base(displayName, null) {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public override bool CanExecute(IContextData context) {
        return this.canExecute == null || this.canExecute(context);
    }

    public override Task OnExecute(IContextData context) {
        return this.execute(context);
    }
}