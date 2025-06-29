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

namespace PFXToolKitUI.Utils;

public class NumberAverager {
    private readonly double[] averages;

    public int NextIndex { get; private set; }

    public int Count => this.averages.Length;

    public NumberAverager(int count) {
        this.averages = new double[count];
    }

    public void PushValue(double number) {
        if (this.NextIndex >= this.averages.Length) {
            this.NextIndex = 0;
        }

        this.averages[this.NextIndex++] = number;
    }

    public double GetAverage() {
        double average = 0;
        foreach (double elem in this.averages) {
            average += elem;
        }

        return average / this.averages.Length;
    }
}