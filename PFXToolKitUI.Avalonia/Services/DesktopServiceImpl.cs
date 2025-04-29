// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PFXToolKitUI.Avalonia.Services;

// Current impl is shared across every platform. Could at some point use Win32DesktopServiceImpl, MacDesktopServiceImpl, etc.
public class DesktopServiceImpl : IDesktopService {
    public Application Application { get; }

    public IClassicDesktopStyleApplicationLifetime ApplicationLifetime { get; }
    
    public DesktopServiceImpl(Application application) {
        this.Application = application ?? throw new ArgumentNullException(nameof(application));
        this.ApplicationLifetime = (IClassicDesktopStyleApplicationLifetime?) application.ApplicationLifetime ?? throw new InvalidOperationException("Cannot create desktop service impl when not using classic desktop style");
    }

    public bool SetCursorPosition(int x, int y) {
        if (OperatingSystem.IsWindows()) {
            return Win32CursorUtils.SetCursorPos(x, y);
        }

        // TODO: other platforms
        return false;
    }

    public bool GetCursorPosition(out int x, out int y) {
        if (OperatingSystem.IsWindows()) {
            return Win32CursorUtils.GetCursorPos(out x, out y);
        }

        // TODO: other platforms
        x = y = 0;
        return false;
    }

    public static class Win32CursorUtils {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int x;
            public int y;
            public int w;
            public int h;

            public RECT(int x, int y, int w, int h) {
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll")]
        private static extern unsafe bool ClipCursor([In] RECT* rect);

        // rect will contain either the current clip or the screen clip
        [DllImport("user32.dll")]
        private static extern unsafe bool GetClipCursor([Out] RECT* rect);

        [DllImport("user32.dll")]
        private static extern unsafe bool SetCursorPos([In] POINT* p);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern unsafe bool GetCursorPos([Out] POINT* p);

        /// <summary>
        /// Whether this application has a logical clip on the cursor (set to true
        /// when <see cref="SetClip"/> is invoked, and set to false when <see cref="ClearClip"/> is invoked)
        /// </summary>
        public static bool IsCursorClipped { get; private set; }

        public static unsafe void SetClip(int x, int y, int width, int height) {
            RECT rect = new RECT(x, y, width, height);
            if (!ClipCursor(&rect))
                throw new Win32Exception();
            IsCursorClipped = true;
        }

        public static unsafe void ClearClip() {
            if (!ClipCursor(null))
                throw new Win32Exception();
            IsCursorClipped = false;
        }

        public static unsafe RECT GetClip() {
            RECT r;
            if (!GetClipCursor(&r))
                throw new Win32Exception();
            return r;
        }

        public static unsafe bool GetCursorPos(out int x, out int y) {
            POINT p;
            if (!GetCursorPos(&p)) {
                x = y = 0;
                return false;
            }

            x = p.x;
            y = p.y;
            return true;
        }
    }
}