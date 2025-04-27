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

namespace PFXToolKitUI.Utils.Destroying;

/// <summary>
/// A helper class for disposable objects
/// </summary>
public static class DisposableUtils {
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2) {
        if (d1 != null)
            try {
                d1.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d2 != null)
            try {
                d2.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }
    }

    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3) {
        if (d1 != null)
            try {
                d1.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d2 != null)
            try {
                d2.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d3 != null)
            try {
                d3.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }
    }

    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4) {
        if (d1 != null)
            try {
                d1.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d2 != null)
            try {
                d2.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d3 != null)
            try {
                d3.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d4 != null)
            try {
                d4.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }
    }

    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4, IDisposable? d5) {
        if (d1 != null)
            try {
                d1.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d2 != null)
            try {
                d2.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d3 != null)
            try {
                d3.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d4 != null)
            try {
                d4.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d5 != null)
            try {
                d5.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }
    }

    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4, IDisposable? d5, IDisposable? d6) {
        if (d1 != null)
            try {
                d1.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d2 != null)
            try {
                d2.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d3 != null)
            try {
                d3.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d4 != null)
            try {
                d4.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d5 != null)
            try {
                d5.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }

        if (d6 != null)
            try {
                d6.Dispose();
            }
            catch (Exception e) {
                errorList?.Add(e);
            }
    }

    public static void DisposeMany(ErrorList? errorList, IEnumerable<IDisposable>? disposables) {
        if (disposables != null) {
            foreach (IDisposable disposable in disposables) {
                try {
                    disposable.Dispose();
                }
                catch (Exception e) {
                    errorList?.Add(e);
                }
            }
        }
    }

    /// <summary>
    /// Dispose a reference to a field and then set it to null
    /// </summary>
    /// <param name="disposable">The ref to disposable</param>
    /// <param name="canThrow">True to allow the disposable to throw, False to swallow the exception and never throw</param>
    public static void Dispose(ref IDisposable? disposable, bool canThrow = true) {
        try {
            if (canThrow) {
                disposable?.Dispose();
            }
            else {
                try {
                    disposable?.Dispose();
                }
                catch {
                    // ignored
                }
            }
        }
        finally {
            disposable = null;
        }
    }
    
    /// <summary>
    /// Dispose an array of disposables and set them to null
    /// </summary>
    /// <param name="array">The disposables</param>
    /// <param name="canThrow">True to allow the disposable to throw, False to swallow the exception and never throw</param>
    public static void DisposeArray(IDisposable?[] array, bool canThrow = true) {
        using ErrorList list = new ErrorList("Exception while disposing one or more objects", true, true);
        for (int i = 0; i < array.Length; i++) {
            try {
                array[i]?.Dispose();
            }
            catch (Exception e) {
                if (canThrow)
                    list.Add(e);
            }
            finally {
                array[i] = null;
            }
        }
    }
}