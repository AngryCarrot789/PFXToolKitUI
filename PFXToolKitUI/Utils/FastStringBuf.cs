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

public class FastStringBuf {
    private char[] buffer;
    public int count;

    public FastStringBuf(int initialCapacity) {
        this.buffer = new char[initialCapacity];
    }

    public void append(char ch) {
        this.EnsureCapacityForAddition(1);
        this.buffer[this.count++] = ch;
    }

    public void append(char[] data) {
        int len = data.Length;
        if (len > 0) {
            this.EnsureCapacityForAddition(len);
            Array.Copy(data, 0, this.buffer, this.count, len);
            this.count += len;
        }
    }

    // Using reflection to access the string's `char[] value` field is about 8% slower
    // (1 / (1400ms)) * (1520ms) = 1.085
    // private static Field CHARS_FIELD = Reflect.getField(() -> String.class.getDeclaredField("value"));

    public void append(string str) {
        this.append(str, 0, str.Length);
    }

    public void append(object value) {
        this.append(value != null ? value.ToString() : "null");
    }

    public void append(string str, int startIndex, int endIndex) {
        int len = endIndex - startIndex;
        if (len > 0) {
            this.EnsureCapacityForAddition(len);
            str.CopyTo(startIndex, this.buffer, this.count, len);
            this.count += len;
        }
    }

    public void append(string str, int startIndex) {
        this.append(str, startIndex, str.Length);
    }

    public void append(char[] array, int startIndex, int endIndex) {
        int len = endIndex - startIndex;
        if (len > 0) {
            this.EnsureCapacityForAddition(len);
            Array.Copy(array, startIndex, this.buffer, this.count, len);
            this.count += len;
        }
    }

    public void append(int i) {
        this.append(i.ToString());
    }

    public void setCharAt(int index, char ch) {
        this.buffer[index] = ch;
    }

    private void EnsureCapacityForAddition(int additional) {
        if (((this.count + additional) - this.buffer.Length) > 0) {
            this.grow(this.count + additional);
        }
    }

    private void grow(int minCapacity) {
        int oldCapacity = this.buffer.Length;
        int newCapacity = oldCapacity + (oldCapacity >> 1);
        if (newCapacity - minCapacity < 0)
            newCapacity = minCapacity;

        char[] buff = new char[newCapacity];
        for (int i = 0, end = Math.Min(this.count, newCapacity); i < end; i++) {
            buff[i] = this.buffer[i];
        }

        this.buffer = buff;
    }

    public void getChars(int startIndex, int endIndex, char[] dest, int destStart) {
        Array.Copy(this.buffer, startIndex, dest, destStart, endIndex - startIndex);
    }

    public void putChars(int startIndex, int endIndex, char[] src, int srcStart) {
        Array.Copy(src, srcStart, this.buffer, startIndex, endIndex - startIndex);
    }

    public override string ToString() {
        return new String(this.buffer, 0, this.count);
    }
}