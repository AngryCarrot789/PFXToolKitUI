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

using System.Text;

namespace PFXToolKitUI.Utils;

public class StringJoiner {
    private readonly StringBuilder sb;
    private readonly string delimiter;
    private bool hasFirst;

    public StringJoiner(string delimiter) {
        this.sb = new StringBuilder();
        this.delimiter = delimiter;
    }

    public void Append(string? value) {
        if (this.hasFirst) {
            this.sb.Append(this.delimiter);
        }
        else {
            this.hasFirst = true;
        }

        this.sb.Append(value);
    }

    public override string ToString() {
        return this.sb.ToString();
    }
}