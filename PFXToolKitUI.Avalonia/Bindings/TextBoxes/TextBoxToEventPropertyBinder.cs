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

using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Bindings.TextBoxes;

public class TextBoxToEventPropertyBinder<TModel> : BaseTextBoxBinder<TModel>, IRelayEventHandler where TModel : class {
    private readonly Func<IBinder<TModel>, string> getText;
    private readonly SenderEventRelay[] eventRelay;

    /// <summary>
    /// Initialises the <see cref="TextBoxToEventPropertyBinder{TModel}"/> object
    /// </summary>
    /// <param name="eventName">The name of the model's event</param>
    /// <param name="getText">A getter to fetch the model's value as raw text for the text box</param>
    /// <param name="parseAndUpdate">
    /// A function which tries to update the model's value from the text box's text. Returns true on success,
    /// returns false when the text box contains invalid data (and it's assumed it shows a dialog too).
    /// When this returns false, the text box's text is re-selected (if <see cref="FocusTextBoxOnError"/> is true)
    /// </param>
    public TextBoxToEventPropertyBinder(string eventName, Func<IBinder<TModel>, string> getText, Func<IBinder<TModel>, string, Task<bool>> parseAndUpdate) : base(parseAndUpdate) {
        this.getText = getText;
        this.eventRelay = [EventRelayStorage.UIStorage.GetEventRelay(typeof(TModel), eventName)];
    }
    
    /// <summary>
    /// Initialises the <see cref="TextBoxToEventPropertyBinder{TModel}"/> object
    /// </summary>
    /// <param name="eventNames">The name of the events that trigger <see cref="BaseBinder{TModel}.UpdateControl"/></param>
    /// <param name="getText">A getter to fetch the model's value as raw text for the text box</param>
    /// <param name="parseAndUpdate">
    /// A function which tries to update the model's value from the text box's text. Returns true on success,
    /// returns false when the text box contains invalid data (and it's assumed it shows a dialog too).
    /// When this returns false, the text box's text is re-selected (if <see cref="FocusTextBoxOnError"/> is true)
    /// </param>
    public TextBoxToEventPropertyBinder(string[] eventNames, Func<IBinder<TModel>, string> getText, Func<IBinder<TModel>, string, Task<bool>> parseAndUpdate) : base(parseAndUpdate) {
        this.getText = getText;
        this.eventRelay = eventNames.Select(name => EventRelayStorage.UIStorage.GetEventRelay(typeof(TModel), name)).ToArray();
    }
    
    void IRelayEventHandler.OnEvent(object sender) => this.UpdateControl();

    protected override string GetTextCore() => this.getText(this);
    
    protected override void OnAttached() {
        base.OnAttached();
        foreach (SenderEventRelay relay in this.eventRelay)
            EventRelayStorage.UIStorage.AddHandler(this.myModel!, this, relay);
    }

    protected override void OnDetached() {
        base.OnDetached();
        foreach (SenderEventRelay relay in this.eventRelay)
            EventRelayStorage.UIStorage.RemoveHandler(this.myModel!, this, relay);
    }
}