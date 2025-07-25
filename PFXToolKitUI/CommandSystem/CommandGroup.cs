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

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// A command that stores a collection of child commands
/// </summary>
public class CommandGroup : Command {
    private readonly List<string> commands;

    public IReadOnlyList<string> Commands => this.commands;

    public CommandGroup() : this(new List<string>()) { }

    public CommandGroup(IEnumerable<string> commands) : this(new List<string>(commands)) { }

    private CommandGroup(List<string> commands) => this.commands = commands;

    public CommandGroup AddCommand(string commandId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        if (this.commands.Contains(commandId))
            return this;

        this.commands.Add(commandId);
        return this;
    }

    public CommandGroup AddCommands(params string[] cmds) {
        ArgumentNullException.ThrowIfNull(cmds, nameof(cmds));
        foreach (string cmdId in cmds)
            if (string.IsNullOrWhiteSpace(cmdId))
                throw new ArgumentException("One of the command ids was null, empty or whitespaces");

        if (cmds.Length == 0)
            return this;

        if (cmds.Length == 1)
            return this.AddCommand(cmds[0]);

        List<string> list = new List<string>();
        HashSet<string> existing = new HashSet<string>(this.commands);
        foreach (string cmdId in cmds)
            if (!existing.Contains(cmdId))
                list.Add(cmdId);

        foreach (string cmdId in list)
            this.commands.Add(cmdId);

        return this;
    }

    public bool RemoveCommand(string commandId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        return this.commands.Remove(commandId);
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return this.commands.Count > 0 ? base.CanExecuteCore(e) : Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        return Task.CompletedTask;
    }
}