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

using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using PFXToolKitUI.Utils.Ranges;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils;

[TestSubject(typeof(IntegerSet<>))]
public class IntegerSetTest {
    [Fact] public void int_TestEnclosingRange() => new TestImpl<int>().TestEnclosingRange();
    [Fact] public void int_TestGrandTotal() => new TestImpl<int>().TestGrandTotal();
    [Fact] public void int_TestGeneralUsage() => new TestImpl<int>().TestGeneralUsage();
    [Fact] public void int_Test_Not_Present_Zero_To_Max() => new TestImpl<int>().Test_Not_Present_Zero_To_Max();
    [Fact] public void int_Test_Is_Present_Zero_To_Max() => new TestImpl<int>().Test_Is_Present_Zero_To_Max();
    [Fact] public void int_Test_Is_Present_Ranged() => new TestImpl<int>().Test_Is_Present_Ranged();
    [Fact] public void int_Test_Not_Present_Ranged() => new TestImpl<int>().Test_Not_Present_Ranged();
    
    [Fact] public void uint_TestEnclosingRange() => new TestImpl<uint>().TestEnclosingRange();
    [Fact] public void uint_TestGrandTotal() => new TestImpl<uint>().TestGrandTotal();
    [Fact] public void uint_TestGeneralUsage() => new TestImpl<uint>().TestGeneralUsage();
    [Fact] public void uint_Test_Not_Present_Zero_To_Max() => new TestImpl<uint>().Test_Not_Present_Zero_To_Max();
    [Fact] public void uint_Test_Is_Present_Zero_To_Max() => new TestImpl<uint>().Test_Is_Present_Zero_To_Max();
    [Fact] public void uint_Test_Is_Present_Ranged() => new TestImpl<uint>().Test_Is_Present_Ranged();
    [Fact] public void uint_Test_Not_Present_Ranged() => new TestImpl<uint>().Test_Not_Present_Ranged();
    
    [Fact] public void long_TestEnclosingRange() => new TestImpl<long>().TestEnclosingRange();
    [Fact] public void long_TestGrandTotal() => new TestImpl<long>().TestGrandTotal();
    [Fact] public void long_TestGeneralUsage() => new TestImpl<long>().TestGeneralUsage();
    [Fact] public void long_Test_Not_Present_Zero_To_Max() => new TestImpl<long>().Test_Not_Present_Zero_To_Max();
    [Fact] public void long_Test_Is_Present_Zero_To_Max() => new TestImpl<long>().Test_Is_Present_Zero_To_Max();
    [Fact] public void long_Test_Is_Present_Ranged() => new TestImpl<long>().Test_Is_Present_Ranged();
    [Fact] public void long_Test_Not_Present_Ranged() => new TestImpl<long>().Test_Not_Present_Ranged();
    
    [Fact] public void ulong_TestEnclosingRange() => new TestImpl<ulong>().TestEnclosingRange();
    [Fact] public void ulong_TestGrandTotal() => new TestImpl<ulong>().TestGrandTotal();
    [Fact] public void ulong_TestGeneralUsage() => new TestImpl<ulong>().TestGeneralUsage();
    [Fact] public void ulong_Test_Not_Present_Zero_To_Max() => new TestImpl<ulong>().Test_Not_Present_Zero_To_Max();
    [Fact] public void ulong_Test_Is_Present_Zero_To_Max() => new TestImpl<ulong>().Test_Is_Present_Zero_To_Max();
    [Fact] public void ulong_Test_Is_Present_Ranged() => new TestImpl<ulong>().Test_Is_Present_Ranged();
    [Fact] public void ulong_Test_Not_Present_Ranged() => new TestImpl<ulong>().Test_Not_Present_Ranged();
    
    [Fact] public void int_SignedOnly_TestEnclosingRange() => new TestImpl_SignedOnly<int>().TestEnclosingRange();
    [Fact] public void int_SignedOnly_TestGrandTotal() => new TestImpl_SignedOnly<int>().TestGrandTotal();
    [Fact] public void int_SignedOnly_Test_Not_Present_Min_To_Max() => new TestImpl_SignedOnly<int>().Test_Not_Present_Min_To_Max();
    [Fact] public void int_SignedOnly_Test_Is_Present_Zero_To_Max() => new TestImpl_SignedOnly<int>().Test_Is_Present_Zero_To_Max();
    
    [Fact] public void long_SignedOnly_TestEnclosingRange() => new TestImpl_SignedOnly<long>().TestEnclosingRange();
    [Fact] public void long_SignedOnly_TestGrandTotal() => new TestImpl_SignedOnly<long>().TestGrandTotal();
    [Fact] public void long_SignedOnly_Test_Not_Present_Min_To_Max() => new TestImpl_SignedOnly<long>().Test_Not_Present_Min_To_Max();
    [Fact] public void long_SignedOnly_Test_Is_Present_Zero_To_Max() => new TestImpl_SignedOnly<long>().Test_Is_Present_Zero_To_Max();
    
    public sealed class TestImpl<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
        public void TestEnclosingRange() {
            IntegerSet<T> list = new IntegerSet<T>();
            list.Add(T.CreateChecked(4));
            list.Add(T.CreateChecked(9));
            list.Add(T.CreateChecked(7));
            list.Add(T.CreateChecked(6));
            list.Add(T.CreateChecked(8));
            list.Add(T.CreateChecked(42));
            list.Add(T.CreateChecked(41));
            
            Assert.Equal(list.EnclosingRange, IntegerRange.FromStartAndEnd(T.CreateChecked(4), T.CreateChecked(43) /* exclusive */));
        }
        
        public void TestGrandTotal() {
            IntegerSet<T> list = new IntegerSet<T>();
            list.Add(T.CreateChecked(4));
            list.Add(T.CreateChecked(9));
            list.Add(T.CreateChecked(10));
            list.Add(IntegerRange.FromStartAndLength(T.CreateChecked(9), T.CreateChecked(4)));
            list.Add(T.CreateChecked(6));
            
            Assert.Equal(list.GrandTotal, T.CreateChecked(6));
        }
        
        public void TestGeneralUsage() {
            IntegerSet<T> list = new IntegerSet<T>();
            list.Add(T.CreateChecked(1));
            list.Add(T.CreateChecked(4));
            list.Add(IntegerRange.FromStartAndLength(T.CreateChecked(2), T.CreateChecked(2)));
            list.Add(T.CreateChecked(40));
            list.Add(T.CreateChecked(5));
            list.Add(T.CreateChecked(9));
            list.Add(T.CreateChecked(7));
            list.Add(T.CreateChecked(6));
            list.Add(T.CreateChecked(8));
            list.Add(T.CreateChecked(42));
            list.Add(T.CreateChecked(41));

            List<IntegerRange<T>> tmpList = list.ToList();
            Assert.Equal(2, tmpList.Count);
            Assert.True(tmpList[0].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(1), T.CreateChecked(10))));
            Assert.True(tmpList[1].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(40), T.CreateChecked(43))));

            list.Remove(T.CreateChecked(2));
            tmpList = list.ToList();
            Assert.Equal(3, tmpList.Count);
            Assert.True(tmpList[0].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(1), T.CreateChecked(2))));
            Assert.True(tmpList[1].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(3), T.CreateChecked(10))));
            Assert.True(tmpList[2].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(40), T.CreateChecked(43))));

            list.Remove(IntegerRange.FromStartAndEnd(T.CreateChecked(3), T.CreateChecked(5)));
            tmpList = list.ToList();
            Assert.Equal(3, tmpList.Count);
            Assert.True(tmpList[0].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(1), T.CreateChecked(2))));
            Assert.True(tmpList[1].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(5), T.CreateChecked(10))));
            Assert.True(tmpList[2].Equals(IntegerRange.FromStartAndEnd(T.CreateChecked(40), T.CreateChecked(43))));
        }

        public void Test_Not_Present_Zero_To_Max() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndLength(T.CreateChecked(0), T.MaxValue), false);
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(0), T.CreateChecked(5))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(21), T.MaxValue)));

            Assert.False(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))));
        }

        public void Test_Is_Present_Zero_To_Max() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndLength(T.CreateChecked(0), T.MaxValue), true);
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(0), T.CreateChecked(5))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(21), T.MaxValue)));

            Assert.True(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))));
        }

        public void Test_Is_Present_Ranged() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndEnd(T.CreateChecked(8), T.CreateChecked(19)), true); // 8 -> 18
            Assert.True(!missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(0), T.CreateChecked(8))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(8), T.CreateChecked(16))));
            Assert.True(!missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(17), T.CreateChecked(19))));
            Assert.True(!missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(19), T.MaxValue)));
        }

        public void Test_Not_Present_Ranged() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndEnd(T.CreateChecked(8), T.CreateChecked(19)), false); // 8 -> 18
            Assert.True(!missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(0), T.CreateChecked(8))));
            Assert.True(!missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(8), T.CreateChecked(16))));
            Assert.True(missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.True(!missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(17), T.CreateChecked(19))));
            Assert.True(!missing.Overlaps(IntegerRange.FromStartAndEnd(T.CreateChecked(19), T.MaxValue)));
        }
    }
    
    public sealed class TestImpl_SignedOnly<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>, ISignedNumber<T> {
        public void TestEnclosingRange() {
            IntegerSet<T> list = new IntegerSet<T>();
            list.Add(T.CreateChecked(-20));
            list.Add(T.CreateChecked(42));
            list.Add(T.CreateChecked(0));
            list.Add(T.CreateChecked(5));
            list.Add(T.CreateChecked(1));
            list.Add(T.CreateChecked(41));
            
            Assert.Equal(list.EnclosingRange, IntegerRange.FromStartAndEnd(T.CreateChecked(-20), T.CreateChecked(43) /* exclusive */));
        }
        
        public void TestGrandTotal() {
            IntegerSet<T> list = new IntegerSet<T>();
            list.Add(T.CreateChecked(-20));
            list.Add(T.CreateChecked(0));
            list.Add(T.CreateChecked(1));
            list.Add(T.CreateChecked(5));
            list.Add(IntegerRange.FromStartAndLength(T.CreateChecked(9), T.CreateChecked(4)));
            list.Add(T.CreateChecked(6));
            
            Assert.Equal(list.GrandTotal, T.CreateChecked(9));
        }

        public void Test_Not_Present_Min_To_Max() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndEnd(T.MinValue, T.MaxValue), false);
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.MinValue, T.CreateChecked(5))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(21), T.MaxValue)));

            Assert.False(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))));
        }

        public void Test_Is_Present_Zero_To_Max() {
            IntegerSet<T> union = new IntegerSet<T>();
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))); // 5 to 15, both inclusive 
            union.Add(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))); // 17 to 20, both inclusive

            IntegerSet<T> missing = union.GetPresenceUnion(IntegerRange.FromStartAndEnd(T.MinValue, T.MaxValue), true);
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.MinValue, T.CreateChecked(5))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(16), T.CreateChecked(17))));
            Assert.False(missing.Contains(IntegerRange.FromStartAndEnd(T.CreateChecked(21), T.MaxValue)));

            Assert.True(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(5), T.CreateChecked(11))));
            Assert.True(missing.Contains(IntegerRange.FromStartAndLength(T.CreateChecked(17), T.CreateChecked(4))));
        }
    }
}