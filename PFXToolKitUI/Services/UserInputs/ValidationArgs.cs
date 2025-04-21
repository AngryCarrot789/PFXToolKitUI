namespace PFXToolKitUI.Services.UserInputs;

public readonly struct ValidationArgs {
    /// <summary>
    /// The value in the text box
    /// </summary>
    public readonly string Input;
    
    /// <summary>
    /// A list of errors to present to the user
    /// </summary>
    public readonly List<string> Errors;
    
    /// <summary>
    /// Whether there was an error last time the validation was invoked
    /// </summary>
    public readonly bool HadErrorPreviously;

    public ValidationArgs(string input, List<string> errors, bool hadErrorPreviously) {
        this.Input = input;
        this.Errors = errors;
        this.HadErrorPreviously = hadErrorPreviously;
    }
}