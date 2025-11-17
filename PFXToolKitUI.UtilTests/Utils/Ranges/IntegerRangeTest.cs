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

using System;
using System.Numerics;
using JetBrains.Annotations;
using PFXToolKitUI.Utils.Ranges;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils.Ranges;

[TestSubject(typeof(IntegerRange<>))]
public class IntegerRangeTest {

    [Fact]
    public void TestSuccess() {
        TestImpl<int>.TestSuccess();
        TestImpl<uint>.TestSuccess();
        TestImpl<long>.TestSuccess();
        TestImpl<ulong>.TestSuccess();
    }
    
    [Fact]
    public void TestFail() {
        TestImpl<int>.TestFail();
        TestImpl<uint>.TestFail();
        TestImpl<long>.TestFail();
        TestImpl<ulong>.TestFail();
    }

    public sealed class TestImpl<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
        public static void TestSuccess() {
            try {
                IntegerRange.FromStartAndEnd(T.MinValue, T.MaxValue);
                IntegerRange.FromStartAndLength(T.MinValue, T.Zero);
                
                IntegerRange.FromStartAndEnd(T.Zero, T.MaxValue);
                IntegerRange.FromStartAndLength(T.Zero, T.Zero);
                
                IntegerRange.FromStartAndEnd(T.MaxValue, T.MaxValue);
                IntegerRange.FromStartAndLength(T.MaxValue, T.Zero);

                T constant = T.CreateChecked(9);
                IntegerRange.FromStartAndLength(T.MaxValue - constant, constant);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
    
        public static void TestFail() {
            try {
                IntegerRange.FromStartAndEnd(T.MaxValue, T.MinValue);
                Assert.Fail("Did not fail");
            }
            catch {
                // ignored
            }
            
            try {
                IntegerRange.FromStartAndLength(T.Zero, T.MinValue);
                Assert.Fail("Did not fail");
            }
            catch {
                // ignored
            }
            
            try {
                T constant = T.CreateChecked(5);
                T constantPlus1 = T.CreateChecked(6);
                IntegerRange.FromStartAndLength(T.MaxValue - constant, constantPlus1);
                Assert.Fail("Did not fail");
            }
            catch {
                // ignored
            }
        }
    }
}