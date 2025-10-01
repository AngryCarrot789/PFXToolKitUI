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

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Utils.Events;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils.Events;

[TestSubject(typeof(SenderEventRelay))]
public class SenderEventRelayTest {
    private const string TestTextAsCustomParameter = "mr sexy!";
    
    private int handleCount;
    private CommandContextEntry entry;

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known")]
    public void TestOrder() {
        SenderEventRelay relay1 = SenderEventRelay.Create<CommandContextEntry>(nameof(CommandContextEntry.DescriptionChanged), this.Handler1);
        SenderEventRelay relay2 = SenderEventRelay.Create(nameof(CommandContextEntry.DescriptionChanged), typeof(CommandContextEntry), this.Handler2);
        SenderEventRelay relay3 = SenderEventRelay.Create(nameof(CommandContextEntry.DescriptionChanged), typeof(CommandContextEntry), this.Handler3, TestTextAsCustomParameter);

        this.entry = new CommandContextEntry("entry");
        relay1.AddEventHandler(this.entry);
        relay2.AddEventHandler(this.entry);
        relay3.AddEventHandler(this.entry);

        this.entry.Description = "some new text";
        
        Assert.Equal(3, this.handleCount);
    }

    private void Handler1(CommandContextEntry obj) {
        Assert.Equal(this.entry, obj);
        this.handleCount++;
    }

    private void Handler2(object obj) {
        Assert.Equal(this.entry, obj);
        this.handleCount++;
    }
    
    private void Handler3(object arg1, object arg2) {
        Assert.Equal(this.entry, arg1);
        Assert.Equal(TestTextAsCustomParameter, arg2);
        this.handleCount++;
    }
}