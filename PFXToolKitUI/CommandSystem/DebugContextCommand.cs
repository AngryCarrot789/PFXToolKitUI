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
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.CommandSystem;

public class DebugContextCommand : Command {
    private static readonly Dictionary<Type, Action<StringBuilder, int, object>> dataAppenders = new Dictionary<Type, Action<StringBuilder, int, object>>();

    static DebugContextCommand() {
        RegisterExtendedDataHandler<BaseContextEntry>((sb, indent, entry) => {
            if (entry is CommandContextEntry cmdEntry) {
                sb.Append(new string(' ', indent)).Append("Command ID: ").AppendLine(cmdEntry.CommandId);
            }
        });
    }

    /// <summary>
    /// Registers a handler that will be invoked when the specific type is within the context data when the command executes
    /// </summary>
    /// <param name="handler">The handler. Receives the string buffer, the indentation to be added (as whitespaces) and the object in question</param>
    /// <typeparam name="T">The type of context data</typeparam>
    public static void RegisterExtendedDataHandler<T>(Action<StringBuilder, int, T> handler) where T : class {
        dataAppenders[typeof(T)] = (sb, indent, obj) => handler(sb, indent, (T) obj);
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (ApplicationPFX.TryGetService(out IMessageDialogService? service)) {
            List<KeyValuePair<string, object>> entries = e.ContextData.Entries.ToList();
            StringBuilder sb = new StringBuilder(256);

            if (entries.Count < 1) {
                sb.Append("No context is available here");
            }
            else {
                foreach (KeyValuePair<string, object> entry in entries) {
                    sb.AppendLine($"--  Key: {entry.Key}");
                    sb.AppendLine($"    Value: {entry.Value}");

                    List<Action<StringBuilder, int, object>>? handlers = null;
                    for (Type? type = entry.Value.GetType(); type != null; type = type.BaseType) {
                        if (dataAppenders.TryGetValue(type, out Action<StringBuilder, int, object>? handler)) {
                            (handlers ??= new List<Action<StringBuilder, int, object>>()).Add(handler);
                        }
                    }

                    if (handlers != null) {
                        for (int i = handlers.Count - 1; i >= 0; i--) {
                            handlers[i](sb, 4, entry.Value);
                        }
                    }
                }
            }

            await service.ShowMessage("Context Available", "Debugging Context", sb.ToString());
        }
    }
}