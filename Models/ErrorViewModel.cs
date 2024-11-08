namespace iCareWebApplication.Models
{
    // Represents a model used for displaying error information in the iCare web application.
    public class ErrorViewModel
    {
        // Unique identifier for the current request, which helps in tracing errors.
        // This property can be null if there is no request ID available.
        public string? RequestId { get; set; }

        // Boolean property that indicates whether the RequestId should be shown in the view.
        // Returns true if RequestId is not null or empty, making it useful for conditional display in error views.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
