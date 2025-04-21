using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

namespace PFXToolKitUI.Avalonia.Bindings.ComboBoxes;

public class ComboBoxToEventPropertyEnumBinder<TEnum> where TEnum : struct, Enum {
    private static readonly ReadOnlyCollection<TEnum> ENUM_VALUES = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList().AsReadOnly();
    
    private readonly Action<object, TEnum> setter;
    private readonly Func<object, TEnum> getter;
    private readonly AutoEventHelper eventHelper;
    private readonly DataParameterEnumInfo<TEnum>? enumInfo;
    private bool isUpdatingControl;

    /// <summary>
    /// Gets or sets the connected control
    /// </summary>
    public ComboBox? Control { get; private set; }
    
    /// <summary>
    /// Gets or sets the connected model
    /// </summary>
    public object? Model { get; private set; }

    public ComboBoxToEventPropertyEnumBinder(Type modelType, string eventName, Func<object, TEnum> getter, Action<object, TEnum> setter, DataParameterEnumInfo<TEnum>? info = null) {
        this.eventHelper = new AutoEventHelper(eventName, modelType, this.OnModelEnumChanged);
        this.setter = setter;
        this.getter = getter;
        this.enumInfo = info;
    }

    private void OnModelEnumChanged() {
        if (this.Model == null)
            throw new Exception("Fatal application bug");
        
        this.UpdateControl(this.getter(this.Model!));
    }
    
    private void OnControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (!this.isUpdatingControl && e.Property == SelectingItemsControl.SelectedIndexProperty) {
            int idx = ((AvaloniaPropertyChangedEventArgs<int>) e).NewValue.GetValueOrDefault();
            this.setter(this.Model!, idx == -1 ? default : (this.enumInfo?.EnumList ?? ENUM_VALUES)[idx]);
        }
    }
    
    public void Attach(ComboBox comboBox, object model) {
        ArgumentNullException.ThrowIfNull(comboBox);
        ArgumentNullException.ThrowIfNull(model);
        if (this.Control != null)
            throw new InvalidOperationException("Already attached");
        
        this.Control = comboBox;
        this.Model = model;
        this.Control.PropertyChanged += this.OnControlPropertyChanged;
        this.eventHelper.AddEventHandler(model);
        this.UpdateControl(this.getter(model));
    }

    public void Detach() {
        if (this.Control == null)
            throw new InvalidOperationException("Not attached");

        this.Control.PropertyChanged -= this.OnControlPropertyChanged;
        this.eventHelper.RemoveEventHandler(this.Model!);
        this.Control = null;
        this.Model = null;
    }
    
    private void UpdateControl(TEnum currentValue) {
        try {
            this.isUpdatingControl = true;
            if (this.Control!.Items.Count == 0 && (this.enumInfo != null ? this.enumInfo.AllowedEnumList.Count : ENUM_VALUES.Count) > 0) {
                foreach (TEnum enumValue in this.enumInfo?.EnumList ?? ENUM_VALUES) {
                    string displayText;
                    if (this.enumInfo != null && this.enumInfo.EnumToText.TryGetValue(enumValue, out string? text)) {
                        displayText = text;
                    }
                    else {
                        displayText = enumValue.ToString();
                    }
                
                    this.Control.Items.Add(new ComboBoxItem() { Content = displayText, Tag = enumValue });
                }
            }

            this.Control.SelectedIndex = (this.enumInfo?.EnumList ?? ENUM_VALUES).IndexOf(currentValue);
        }
        finally {
            this.isUpdatingControl = false;
        }
    }
}