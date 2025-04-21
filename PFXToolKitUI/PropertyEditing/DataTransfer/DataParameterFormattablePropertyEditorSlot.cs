using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Interactivity.Formatting;

namespace PFXToolKitUI.PropertyEditing.DataTransfer;

public delegate void DataParameterValueFormatterChangedEventHandler(DataParameterFormattablePropertyEditorSlot sender, IValueFormatter? oldValueFormatter, IValueFormatter? newValueFormatter);

public abstract class DataParameterFormattablePropertyEditorSlot : DataParameterPropertyEditorSlot {
    private IValueFormatter? valueFormatter;
    
    /// <summary>
    /// Gets or sets the value formatter used to format our numeric value in the UI
    /// </summary>
    public IValueFormatter? ValueFormatter {
        get => this.valueFormatter;
        set {
            IValueFormatter? oldValueFormatter = this.valueFormatter;
            if (oldValueFormatter == value)
                return;

            this.valueFormatter = value;
            this.ValueFormatterChanged?.Invoke(this, oldValueFormatter, value);
        }
    }
    
    public event DataParameterValueFormatterChangedEventHandler? ValueFormatterChanged;
    
    public DataParameterFormattablePropertyEditorSlot(DataParameter parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
    }
}