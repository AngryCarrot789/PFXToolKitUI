# PFX ToolKit
This is a tool kit, originally developed for FramePFX (a non-linear video editor)

It acts as a "base" for an application and provides a multitude of helper libraries, including:
- Activity Tasks (which track progress with a caption, message and completion or with an indeterminate mode)
- Advanced Menu system, which use model objects to define menus and context menus, and supports on-demand generators for menu options (i.e. generate options based on available context, and the state of the objects/application)
- Command System, Uses registered commands (with string keys) to centralize actions that can be invoked from multiple places (e.g. rename an object via a keystroke (CTRL+R), or via a button, or a context menu)
- "Command Usages" help with allowing UI controls to be updated when the state of contextual objects change, where those UI controls would typically cause a command to run which depends on those states that can change (i.e. if a task is already running, it can't be run again, so disable the button. The command never needs to know about the button, only the Command Usage)
- Themes, which allow user customizable theme options, supporting theme and theme-key inheritance (which is something Avalonia does not natively implement, AFAIK)
- Brush and Icon systems, which are mainly used by the Advanced Menu System, to support model-defined icons and usage of icons without exposing Avalonia-specific details. The brush system also supports the equivalent of `DynamicResource`, so that icons such as geometry icons can change colour when the theme changes.
- Plugins, either "Core" (referenced by an app's entry point project) or "Assembly" (loaded dynamically), to add extra functionality to an app
- Notifications (Toasts), adds a notification management system, with support for "actions" that the user can click within the notification.
- Shortcut System, which supports user-customizable shortcuts to run commands via the Command System.
- Persistent Configurations, which simplify dealing with configs by loading on app startup and saving on app shutdown
- Toolbars, which are still WIP, but allow for code-defined toolbars rather than a declarative (AXAML) toolbar
- Property Editor, a completely custom control for modifying models. Usually used with the DataParameter system, but FramePFX extends this with its own automatable/animatable parameters
- DataManager manages the context associated with UI objects using via the interface `IContextData` — basically a `Dictionary<string, object>`. It also allows for accumulating the absolute "fully inherited" context from a UI element by traversing the visual tree upwards.
  
  This system is used by the Notification, Command, Shortcut, Advanced Menu and ToolTipEx systems
- Windowing and Overlay abstractions, which allow showing a control either as a window or an overlay over another control.
- History (undo/redo) system
- ToolTipEx service, allows for reusable tool tips. Controls that wish to be shown as a tool tip implement `IToolTipControl`, and there will only ever be once instance of that control. The tool tip control then observes the context of its adorned control (i.e. tooltip owner) to update itself.
- Configuration Dialogs, which support trees of config pages
- EventRelay system, which allows allocating event handlers for any delegate type
- Binders, which is a more customizable system than regular Avalonia binding. For example, parse text box text manually and show a dialog on parse failure. Or a text block whose text changes based on multiple properties, all you have to do if specify the event names that cause the text box text to be re-evaluated