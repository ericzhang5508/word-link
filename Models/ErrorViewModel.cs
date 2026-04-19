namespace WordLink.Models;

/// <summary>
/// View model used to display error information to the user.
/// </summary>
public class ErrorViewModel
{
    /// <summary>The unique identifier for the current request, used for tracing.</summary>
    public string? RequestId { get; set; }

    /// <summary>Indicates whether the RequestId should be shown (true if it has a value).</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
