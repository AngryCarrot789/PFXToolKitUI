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

using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Shortcuts.Usage;

public class MouseShortcutUsage : IMouseShortcutUsage {
    private LinkedListNode<MouseStroke>? currentStroke;
    // private int clickCounter;

    public IMouseShortcut MouseShortcut { get; }

    public LinkedList<MouseStroke> Strokes { get; }

    public MouseStroke CurrentMouseStroke => this.currentStroke?.Value ?? default;

    public IShortcut Shortcut {
        get => this.MouseShortcut;
    }

    public bool IsCompleted => this.currentStroke == null;

    public IInputStroke PreviousStroke { get; private set; }

    public IInputStroke CurrentStroke => this.currentStroke!.Value;

    public IEnumerable<IInputStroke> RemainingStrokes {
        get {
            LinkedListNode<MouseStroke>? stroke = this.currentStroke;
            while (stroke != null) {
                yield return stroke.Value;
                stroke = stroke.Next;
            }
        }
    }

    public bool IsCurrentStrokeMouseBased => true;

    public bool IsCurrentStrokeKeyBased => false;

    public MouseShortcutUsage(IMouseShortcut shortcut) {
        this.MouseShortcut = shortcut;
        this.Strokes = new LinkedList<MouseStroke>(shortcut.MouseStrokes);
        this.currentStroke = this.Strokes.First!.Next;
        this.PreviousStroke = this.Strokes.First!.Value;
    }

    public bool OnMouseStroke(in MouseStroke stroke) {
        if (this.currentStroke == null) {
            return true;
        }

        if (this.currentStroke.Value.Equals(stroke)) {
            this.PreviousStroke = stroke;
            this.currentStroke = this.currentStroke.Next;
            return true;
        }
        else if (this.currentStroke.Value.EqualsWithoutClickOrRelease(stroke)) {
            // this allows double or triple clicking
            // this.clickCounter++;
            return true;
        }
        else {
            return false;
        }
    }

    public bool OnInputStroke(IInputStroke stroke) {
        return stroke is MouseStroke keyStroke && this.OnMouseStroke(keyStroke);
    }
}