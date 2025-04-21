using System.ComponentModel;
using System.Reflection;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.Core;

public delegate void INPCPropertyEditorSlotDisplayNameChangedEventHandler(INPCPropertyEditorSlot sender);

/// <summary>
/// A property editor slot that supports any kind of value type object
/// </summary>
public class INPCPropertyEditorSlot : PropertyEditorSlot {
    private bool isProcessingValueChange;
    private bool hasMultipleValues;
    private bool hasProcessedMultipleValuesSinceSetup;
    protected bool lastQueryHasMultipleValues;
    private string displayName;
    
    public override bool IsSelectable => true;

    /// <summary>
    /// Gets the property info used to get/set the property
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets or sets the display name. Defaults to the property name
    /// </summary>
    public string DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;

            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Gets whether the slot has multiple handlers and they all have different underlying values.
    /// This is used to present some sort of signal in the UI warning the user before they try to modify it.
    /// This state must be updated manually by derived classes when the values are no longer different
    /// </summary>
    public bool HasMultipleValues {
        get => this.hasMultipleValues;
        protected set {
            if (this.hasMultipleValues == value)
                return;

            if (value)
                this.HasProcessedMultipleValuesSinceSetup = false;
            this.hasMultipleValues = value;
            this.HasMultipleValuesChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets whether the <see cref="HasMultipleValues"/> has been
    /// updated since <see cref="BasePropertyEditorItem.IsCurrentlyApplicable"/> became true
    /// </summary>
    public bool HasProcessedMultipleValuesSinceSetup {
        get => this.hasProcessedMultipleValuesSinceSetup;
        set {
            if (this.hasProcessedMultipleValuesSinceSetup == value)
                return;

            this.hasProcessedMultipleValuesSinceSetup = value;
            this.HasProcessedMultipleValuesChanged?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Gets the current value of the selected object(s)
    /// </summary>
    public object? CurrentValue { get; private set; }

    public event PropertyEditorSlotEventHandler? CurrentValueChanged;
    public event PropertyEditorSlotEventHandler? HasMultipleValuesChanged;
    public event PropertyEditorSlotEventHandler? HasProcessedMultipleValuesChanged;
    public event PropertyEditorSlotEventHandler? DisplayNameChanged;
    
    public INPCPropertyEditorSlot(Type type, string propertyName) : base(type) {
        this.PropertyInfo = type.GetProperty(propertyName) ?? throw new Exception("No such property: " + type.Name + "." + propertyName);
        this.displayName = propertyName;
    }

    protected override void OnHandlersLoaded() {
        base.OnHandlersLoaded();
        if (this.Handlers.Count == 1) {
            ((INotifyPropertyChanged) this.Handlers[0]).PropertyChanged += this.OnPropertyChanged;
        }

        this.RequeryFromHandlers();
        this.lastQueryHasMultipleValues = this.HasMultipleValues;
        this.OnValueChanged();
    }
    
    protected override void OnClearingHandlers() {
        base.OnClearingHandlers();
        if (this.Handlers.Count == 1) {
            ((INotifyPropertyChanged) this.Handlers[0]).PropertyChanged -= this.OnPropertyChanged;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        object? value;
        if (!this.isProcessingValueChange && this.CurrentValue != (value = this.PropertyInfo.GetValue(sender))) {
            this.lastQueryHasMultipleValues = this.HasMultipleValues;
            this.CurrentValue = value;
            this.OnValueChanged();
        }
    }
    
    public void RequeryFromHandlers() {
        this.CurrentValue = CollectionUtils.GetEqualValue(this.Handlers, x => this.PropertyInfo.GetValue(x), out object? d) ? d : null;
    }
    
    protected void OnValueChanged(bool? hasMultipleValues = null, bool? hasProcessedMultiValueSinceSetup = null) {
        this.CurrentValueChanged?.Invoke(this);
        if (hasMultipleValues.HasValue)
            this.HasMultipleValues = hasMultipleValues.Value;
        if (hasProcessedMultiValueSinceSetup.HasValue)
            this.HasProcessedMultipleValuesSinceSetup = hasProcessedMultiValueSinceSetup.Value;
    }
}