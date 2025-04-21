using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.Progressing;

public delegate void ProgressionInfoEventHandler(ProgressionInfo sender);

/// <summary>
/// The base class for information used in a progression window
/// </summary>
public abstract class ProgressionInfo : ITransferableData {
    public static readonly DataParameterString CaptionParameter = DataParameter.Register(new DataParameterString(typeof(ProgressionInfo), nameof(Caption), "Progress", ValueAccessors.Reflective<string?>(typeof(ProgressionInfo), nameof(caption))));
    public static readonly DataParameterString MessageParameter = DataParameter.Register(new DataParameterString(typeof(ProgressionInfo), nameof(Message), null, ValueAccessors.Reflective<string?>(typeof(ProgressionInfo), nameof(message))));
    public static readonly DataParameterString CancelTextParameter = DataParameter.Register(new DataParameterString(typeof(ProgressionInfo), nameof(CancelText), "Cancel", ValueAccessors.Reflective<string?>(typeof(ProgressionInfo), nameof(cancelText))));

    private string? caption = CaptionParameter.DefaultValue;
    private string? message = MessageParameter.DefaultValue;
    private string? cancelText = CancelTextParameter.DefaultValue;
    private bool isCancellable;

    /// <summary>
    /// Gets or sets the dialog's caption, displayed usually in the titlebar
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => DataParameter.SetValueHelper(this, CaptionParameter, ref this.caption, value);
    }

    /// <summary>
    /// Gets or sets the dialog's message, displayed above the input field(s) at the top of the dialog's content,
    /// typically in bolder text. This could be some general information about what the fields do or maybe some rules.
    /// See derived classes for properties such as labels or field descriptions, which may be more specific
    /// </summary>
    public string? Message {
        get => this.message;
        set => DataParameter.SetValueHelper(this, MessageParameter, ref this.message, value);
    }

    /// <summary>
    /// Gets or sets the text in the confirm button
    /// </summary>
    public string? CancelText {
        get => this.cancelText;
        set => DataParameter.SetValueHelper(this, CancelTextParameter, ref this.cancelText, value);
    }

    public bool IsCancellable {
        get => this.isCancellable;
        set {
            if (this.isCancellable == value)
                return;

            this.isCancellable = value;
            this.IsCancellableChanged?.Invoke(this);
        }
    }

    public TransferableData TransferableData { get; }

    public event ProgressionInfoEventHandler? IsCancellableChanged;

    protected ProgressionInfo() {
        this.TransferableData = new TransferableData(this);
    }
}