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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// A class that manages a stack of context data local to an async call
/// </summary>
public sealed class AsyncLocalContextManager {
    /*
     * TEST CODE -- THIS MUST NOT FAIL
     * 
     * private static void Test() {
     *     DataKey<int> testKey = DataKey<int>.Create("cool test");
     *     _ = Task.Run(() => Instance.Dispatcher.Post(() => _ = RunFunThings(new ContextData().Set(testKey, 1))));
     *     await Task.Delay(100);
     *     _ = Task.Run(() => Instance.Dispatcher.Post(() => _ = RunFunThings(new ContextData().Set(testKey, 2)))); 
     * }
     * 
     * private static async Task RunFunThings(IContextData context) {
     *     using IDisposable usage = CommandManager.LocalContextManager.PushGlobalContext(context);
     *     await Task.Delay(1000);
     *
     *     bool found = CommandManager.LocalContextManager.TryGetGlobalContext(out IContextData? localContext);
     *     Debug.Assert(found && localContext == context);
     * }
     *
     * This code cannot fail because:
     *   The first Task.Run posts a call to RunFunThings, which effectively pushes "1" onto the global stack.
     *   Then, after 100ms it pushes "2" onto the global stack
     *   But, because of how AsyncLocal works, the two are separate, so they do
     *   not collide, which is further proven by the 1000ms delay in RunFunThings
     *
     *   However, if RunFunThings was recursive, then "1" and "2" would be on the stack, which is supported as well.
     *   But the important thing is that both calls have completely separate context, since they're called
     *   at completely different points in time and also in different locations (technically; task scheduler)
     * 
     */
    
    private sealed class LocalContext {
        public readonly AsyncLocalContextManager manager;
        public readonly List<IContextData> stack;
        public IContextData? fullContext;

        public LocalContext(AsyncLocalContextManager manager) {
            this.manager = manager;
            this.stack = new List<IContextData>();
        }
    }

    private readonly AsyncLocal<LocalContext> myAsyncLocals;

    public AsyncLocalContextManager() {
        this.myAsyncLocals = new AsyncLocal<LocalContext>();
    }

    private LocalContext GetContextStack() => this.myAsyncLocals.Value ??= new LocalContext(this);

    public IDisposable PushGlobalContext(IContextData context) {
        LocalContext ctx = this.GetContextStack();
        ctx.stack.Add(context);
        ctx.fullContext = null; // invalidate
        return new PopGlobalContextFlow(this, context);
    }

    private void PopGlobalContext(IContextData context) {
        LocalContext ctx = this.GetContextStack();
        bool popped = ctx.stack.Remove(context);
        Debug.Assert(popped);

        ctx.fullContext = null; // invalidate
    }

    /// <summary>
    /// Tries to get the current global context, which is the merged results of all context
    /// </summary>
    /// <param name="contextData"></param>
    /// <returns></returns>
    public bool TryGetGlobalContext([NotNullWhen(true)] out IContextData? contextData) {
        LocalContext? ctx = this.myAsyncLocals.Value;
        if (ctx == null || ctx.stack.Count < 1) {
            contextData = null;
            return false;
        }
        
        ctx.fullContext ??= ctx.stack.Count == 1 ? ctx.stack[0] : new DelegatingContextData(ctx.stack.ToArray());
        return (contextData = ctx.fullContext) != null;
    }

    /// <summary>
    /// Gets the global context, which may be empty
    /// </summary>
    public IContextData GetGlobalContext() => this.TryGetGlobalContext(out IContextData? context) ? context : EmptyContext.Instance;

    private sealed class PopGlobalContextFlow(AsyncLocalContextManager manager, IContextData context) : IDisposable {
        private AsyncLocalContextManager? myManager = manager;
        private IContextData? myContext = context;

        public void Dispose() {
            AsyncLocalContextManager? manager = Interlocked.Exchange(ref this.myManager, null);
            if (manager != null) {
                IContextData? context = this.myContext;
                Debug.Assert(context != null);
                this.myContext = null;

                manager.PopGlobalContext(context!);
            }
        }
    }
}