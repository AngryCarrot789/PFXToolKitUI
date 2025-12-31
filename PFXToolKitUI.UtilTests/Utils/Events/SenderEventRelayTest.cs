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

#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.Utils.Events;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils.Events;

[TestSubject(typeof(EventWrapper))]
public class EventWrapperTest {
    private const string TestTextAsCustomParameter = "mr sexy!";

    private int handleCount;
    private CommandMenuEntry? entry;
    private TestObject? testObj;

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known")]
    public void TestOrder() {
        EventWrapper relay1 = EventWrapper.CreateWithSender<CommandMenuEntry>(nameof(CommandMenuEntry.DescriptionChanged), obj => {
            Assert.Equal(this.entry, obj);
            Assert.Equal(0, this.handleCount);
            this.handleCount++;
        });
        
        EventWrapper relay2 = EventWrapper.CreateWithSender(nameof(CommandMenuEntry.DescriptionChanged), typeof(CommandMenuEntry), obj => {
            Assert.Equal(this.entry, obj);
            Assert.Equal(1, this.handleCount);
            this.handleCount++;
        });
        
        EventWrapper relay3 = EventWrapper.CreateWithSenderAndState(nameof(CommandMenuEntry.DescriptionChanged), typeof(CommandMenuEntry), (arg1, arg2) => {
            Assert.Equal(this.entry, arg1);
            Assert.Equal(TestTextAsCustomParameter, arg2);
            Assert.Equal(2, this.handleCount);
            this.handleCount++;
        }, TestTextAsCustomParameter);

        this.entry = new CommandMenuEntry("entry");
        relay1.AddEventHandler(this.entry);
        relay2.AddEventHandler(this.entry);
        relay3.AddEventHandler(this.entry);

        this.entry.Description = "some new text";

        Assert.Equal(3, this.handleCount);
    }

    private class TestObject {
        public string? Prop1 {
            get => field;
            set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.Prop1Changed);
        }
        
        public string? Prop2 {
            get => field;
            set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.Prop2Changed?.Invoke(t, new Prop2ChangedEventArgs(o, n)));
        }

        public event EventHandler? Prop1Changed;
        public event EventHandler<Prop2ChangedEventArgs>? Prop2Changed;
    }

    private readonly struct Prop2ChangedEventArgs(string? oldValue, string? newValue) {
        public string? OldValue { get; } = oldValue;
        public string? NewValue { get; } = newValue;
    }

    [Fact]
    public void TestGenericUsage() {
        EventWrapper relay1 = EventWrapper.CreateWithSender<TestObject>(nameof(TestObject.Prop1Changed), obj => {
            Assert.Equal(this.testObj, obj);
            this.handleCount++;
        });
        
        EventWrapper relay2 = EventWrapper.CreateWithSender<TestObject>(nameof(TestObject.Prop2Changed), obj => {
            Assert.Equal(this.testObj, obj);
            this.handleCount++;
        });

        this.testObj = new TestObject();
        relay1.AddEventHandler(this.testObj);
        relay2.AddEventHandler(this.testObj);

        Assert.Equal(0, this.handleCount);
        this.testObj.Prop1 = "some new text";
        Assert.Equal(1, this.handleCount);
        this.testObj.Prop2 = "some new text";
        Assert.Equal(2, this.handleCount);
    }
}