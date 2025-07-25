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

namespace PFXToolKitUI;

public static class BugTrack {
    public static EmptyToken BeginBug(object senderOrType, string bugName) {
        return new EmptyToken(senderOrType, bugName);
    }

    public static EmptyToken ReallyBadImplementation(object senderOrType, string badImplName) {
        return new EmptyToken(senderOrType, badImplName);
    }

    public readonly struct EmptyToken : IDisposable {
        public readonly string Text;
        public readonly object SenderOrType;

        public EmptyToken(object senderOrType, string text) {
            this.SenderOrType = senderOrType;
            this.Text = text;
        }

        public void Dispose() {
        }

        public override string ToString() {
            if (this.SenderOrType == null)
                return this.Text;

            return $"{(this.SenderOrType is Type t ? t.Name : this.SenderOrType.GetType().Name)}: {this.Text}";
        }
    }
}