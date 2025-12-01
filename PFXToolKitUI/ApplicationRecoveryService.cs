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

using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI;

/// <summary>
/// A service that implements application recovery support to allow the app to continue recovering
/// </summary>
public abstract class ApplicationRecoveryService {
    /// <summary>
    /// Gets the recover callback
    /// </summary>
    protected Action? Recover {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnRecoveryActionChanged(o, n));
    }

    protected ApplicationRecoveryService() {
    }

    public static bool TryGetInstance([NotNullWhen(true)] out ApplicationRecoveryService? service) {
        return ApplicationPFX.TryGetComponent(out service);
    }

    /// <summary>
    /// Installs the recover callback, which is invoked when the user clicks the "recover application" button
    /// </summary>
    /// <param name="newCallback"></param>
    public void Install(Action? newCallback) {
        this.Recover = newCallback;
    }

    /// <summary>
    /// Uninstalls the recover callback
    /// </summary>
    public void Uninstall() {
        this.Recover = null;
    }

    protected virtual void OnRecoveryActionChanged(Action? oldCallback, Action? newCallback) {
    }
}

// internal sealed class Win32ApplicationRecoveryServiceImpl : ApplicationRecoveryService {
//     public delegate int ApplicationRecoveryCallback(IntPtr parameter);
//
//     private ApplicationRecoveryCallback? myCallback;
//     private volatile bool hasCalledInFrame;
//
//     public Win32ApplicationRecoveryServiceImpl() {
//     }
//
//     protected override void OnRecoveryActionChanged(Action? oldCallback, Action? newCallback) {
//         if (newCallback != null) {
//             Action action = newCallback;
//             this.myCallback = _ => {
//                 if (!Interlocked.CompareExchange(ref this.hasCalledInFrame, true, false)) {
//                     try {
//                         action();
//                     }
//                     catch {
//                         // ignored
//                     }
//                     finally {
//                         this.hasCalledInFrame = false;
//                     }
//                 }
//
//                 return 0;
//             };
//
//             int result = RegisterApplicationRecoveryCallback(this.myCallback, IntPtr.Zero, dwPingInterval: 5000, 0);
//             Debug.Assert(result == 0, "Should be 0 as per windows docs");
//         }
//         else {
//             int result = UnregisterApplicationRecoveryCallback();
//             this.myCallback = null;
//         }
//     }
//
//     [DllImport("kernel32.dll")]
//     private static extern int RegisterApplicationRecoveryCallback(ApplicationRecoveryCallback recoveryCallback, IntPtr pvParameter, uint dwPingInterval, uint dwFlags);
//     
//     [DllImport("kernel32.dll")]
//     private static extern int UnregisterApplicationRecoveryCallback();
// }