using System.Text.Json.Serialization;

namespace sebkuw.ExceptionProcessor.Models;

/// <summary>
/// Standardized exception response model used in ExceptionProcessor.
/// 
/// This class represents a uniform error response structure that is sent to clients
/// when exceptions occur in the application. It provides comprehensive error information
/// including error codes, HTTP status codes, user-friendly messages, detailed descriptions,
/// timestamps for audit trails, and correlation IDs for distributed tracing across
/// microservices and multiple systems.
/// 
/// The ExceptionResponse is immutable after creation (uses init-only properties),
/// ensuring thread-safety and preventing accidental modifications.
/// 
/// Usage:
///     var response = new ExceptionResponse(
///         code: "ValidationError",
///         httpCode: 400,
///         mainText: "Invalid input",
///         description: "Email field is required and cannot be empty.",
///         traceId: "0HN1GH8P91FD0:00000001");
/// 
/// JSON Response Example:
///     {
///         "code": "ValidationError",
///         "httpCode": 400,
///         "mainText": "Invalid input",
///         "description": "Email field is required and cannot be empty.",
///         "timestamp": "2025-11-04T09:45:30.1234567Z",
///         "traceId": "0HN1GH8P91FD0:00000001"
///     }
/// </summary>
public class ExceptionResponse
{
    /// <summary>
    /// Unique error code identifying the type of exception.
    /// 
    /// This property provides a machine-readable error identifier that can be used
    /// by client applications for programmatic error handling and routing. Different
    /// error codes help distinguish between various error scenarios, allowing clients
    /// to handle specific errors differently (e.g., retry logic for timeout errors).
    /// 
    /// Common error codes:
    ///     - "ValidationError": Input validation failed (HTTP 400)
    ///     - "ObjectNotFound": Requested resource does not exist (HTTP 404)
    ///     - "ForbiddenAccess": User lacks permissions (HTTP 403)
    ///     - "UnhandledException": Unexpected server error (HTTP 500)
    ///     - "DatabaseError": Database operation failed (HTTP 500)
    ///     - "TimeoutError": Operation exceeded allowed execution time (HTTP 408)
    /// 
    /// Characteristics:
    ///     - PascalCase format for consistency
    ///     - Immutable after initialization (init-only property)
    ///     - Should be globalized (not localized)
    ///     - Useful for API clients to handle errors programmatically
    /// 
    /// JSON Serialization: "code"
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; init; }

    /// <summary>
    /// HTTP status code associated with the error.
    /// 
    /// This property contains the recommended HTTP status code that should be set
    /// in the response. It follows standard HTTP status code conventions to allow
    /// HTTP clients and proxies to understand the nature of the error.
    /// 
    /// HTTP Status Code Mapping:
    ///     - 400 (Bad Request): Input validation errors, malformed requests
    ///     - 401 (Unauthorized): Authentication required but not provided/invalid
    ///     - 403 (Forbidden): Authentication successful but permission denied
    ///     - 404 (Not Found): Requested resource does not exist
    ///     - 408 (Request Timeout): Operation exceeded allowed execution time
    ///     - 409 (Conflict): Request conflicts with current state (e.g., duplicate)
    ///     - 500 (Internal Server Error): Unexpected server errors, database failures
    /// 
    /// Characteristics:
    ///     - Integer value between 100 and 599
    ///     - Immutable after initialization (init-only property)
    ///     - Must be set in HTTP response headers automatically by middleware
    ///     - Enables proper HTTP client behavior (retries, caching, etc.)
    /// 
    /// JSON Serialization: "httpCode"
    /// Example: 400, 404, 500
    /// </summary>
    [JsonPropertyName("httpCode")]
    public int HttpCode { get; init; }

    /// <summary>
    /// Short main error message describing the issue in user-friendly terms.
    /// 
    /// This property provides a concise, human-readable error message intended for
    /// end-users and developers. It should be clear and actionable without being
    /// excessively detailed. This message is localization-friendly and may be
    /// displayed in UI error dialogs or notifications.
    /// 
    /// Characteristics:
    ///     - Concise (typically 2-5 words)
    ///     - User-friendly language
    ///     - Should be safe to display to end-users
    ///     - Immutable after initialization (init-only property)
    ///     - Not localized in this base class (consumers may localize)
    ///     - Should hint at the solution when possible
    /// 
    /// Examples:
    ///     - "Invalid input"
    ///     - "Object not found"
    ///     - "Access denied"
    ///     - "Operation failed"
    ///     - "Database error"
    ///     - "Request timeout"
    /// 
    /// JSON Serialization: "mainText"
    /// </summary>
    [JsonPropertyName("mainText")]
    public string MainText { get; init; }

    /// <summary>
    /// Detailed explanation providing additional context for the error.
    /// 
    /// This property contains a more comprehensive error description that provides
    /// context about what went wrong and why. It includes specific details that help
    /// developers diagnose issues while being informative enough for technical users.
    /// 
    /// Should include:
    ///     - Specific field or entity names that caused the error
    ///     - Actual values or constraints that were violated
    ///     - Actionable suggestions for resolution
    /// 
    /// Should NOT include:
    ///     - Stack traces or internal system details
    ///     - Sensitive information (passwords, tokens, personal data)
    ///     - Internal database structures or queries
    ///     - File system paths on the server
    /// 
    /// Characteristics:
    ///     - More detailed than MainText
    ///     - User-friendly but technical
    ///     - Safe to display to end-users (no sensitive data)
    ///     - Immutable after initialization (init-only property)
    ///     - Can be localized if needed
    /// 
    /// Examples:
    ///     - "Email field is required and cannot be empty."
    ///     - "The requested User with ID 123 was not found."
    ///     - "You do not have permission to access resource 'Reports'."
    ///     - "The operation timed out after 30 seconds of execution."
    ///     - "Database connection failed: connection string is invalid."
    /// 
    /// JSON Serialization: "description"
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; }

    /// <summary>
    /// Timestamp indicating when the error occurred (UTC timezone).
    /// 
    /// This property records the exact moment when the exception was caught and
    /// converted to an ExceptionResponse. It is automatically set to the current UTC time
    /// during object construction and cannot be modified afterward.
    /// 
    /// Uses ISO 8601 format with UTC timezone for consistency across different systems,
    /// time zones, and locales. This standardization is crucial for:
    ///     - Audit logging and compliance
    ///     - Request tracing across microservices
    ///     - Debugging and issue investigation
    ///     - Performance analysis and monitoring
    ///     - Log correlation with external systems
    /// 
    /// Characteristics:
    ///     - Always in UTC timezone (no local time)
    ///     - ISO 8601 format with microsecond precision
    ///     - Immutable after initialization (init-only property)
    ///     - Automatically set in constructor (cannot be manually set)
    ///     - Useful for time-based log analysis and monitoring
    /// 
    /// Format Examples:
    ///     - "2025-11-04T09:45:30.1234567Z"
    ///     - "2025-11-04T14:30:00.0000000Z"
    /// 
    /// Usage in Diagnostics:
    ///     - Match log entries with errors: grep "2025-11-04T09:45" logfile.log
    ///     - Calculate error duration: endTime - Timestamp
    ///     - Group errors by time period: WHERE Timestamp BETWEEN @start AND @end
    ///     - Create time-based dashboards: GROUP BY DATE(Timestamp)
    /// 
    /// JSON Serialization: "timestamp" (ISO 8601 format)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Optional trace identifier for request correlation in distributed systems.
    /// 
    /// This property contains a unique identifier assigned to each HTTP request that
    /// flows through the application. The same TraceId is propagated across all
    /// microservices, databases, and external API calls involved in processing that
    /// request. This enables end-to-end request tracing and distributed correlation.
    /// 
    /// Distributed Tracing Use Cases:
    ///     1. Multi-service Tracing: Track request: Client → API1 → API2 → Database
    ///     2. Request Correlation: Link all logs from same request together
    ///     3. Performance Analysis: Identify bottlenecks in request processing chain
    ///     4. Error Root-Cause Analysis: Find exactly where in pipeline error occurred
    ///     5. Debugging Microservices: Follow request flow without log searching
    /// 
    /// How TraceId Works:
    ///     1. Middleware creates or receives TraceId (from X-Correlation-ID header)
    ///     2. TraceId stored in HttpContext.Items
    ///     3. Every service, handler, and logger includes TraceId in logs
    ///     4. Response includes TraceId in X-Correlation-ID header
    ///     5. Client receives TraceId to include in support tickets or debug requests
    /// 
    /// Format:
    ///     - Generated by Activity class (W3C Trace Context format)
    ///     - Example: "0HN1GH8P91FD0:00000001"
    ///     - Also can be: GUID format "550e8400-e29b-41d4-a716-446655440000"
    ///     - Length varies based on tracing provider
    /// 
    /// Characteristics:
    ///     - Nullable (can be null if not set)
    ///     - Immutable after initialization (init-only property)
    ///     - Optional parameter in constructor (defaults to null)
    ///     - Should be included in HTTP response headers
    ///     - Should be added to all structured log entries
    /// 
    /// Request-Response Flow:
    ///     Request Header:  GET /api/users/123
    ///     Response Header: X-Correlation-ID: 0HN1GH8P91FD0:00000001
    ///     Response Body:   { "traceId": "0HN1GH8P91FD0:00000001", ... }
    /// 
    /// Log Entry Example:
    ///     [2025-11-04 09:45:30.123] [ERROR] [0HN1GH8P91FD0:00000001] User not found
    /// 
    /// Searching logs by TraceId:
    ///     grep "0HN1GH8P91FD0:00000001" application.log
    ///     - Shows all logs (info, warnings, errors) from this single request
    ///     - Helps debug request-specific issues
    /// 
    /// JSON Serialization: "traceId" (null if not provided)
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    /// <summary>
    /// Initializes a new instance of the ExceptionResponse class.
    /// 
    /// This constructor creates a standardized exception response with all required
    /// information. The Timestamp is automatically set to the current UTC time,
    /// ensuring consistent and accurate error timestamps across the system.
    /// 
    /// Parameters:
    /// - code (string): Unique machine-readable error code
    ///     Requirement: Non-null, PascalCase format
    ///     Examples: "ValidationError", "ObjectNotFound", "UnhandledException"
    /// 
    /// - httpCode (int): HTTP status code (100-599)
    ///     Requirement: Valid HTTP status code
    ///     Common: 400, 403, 404, 408, 409, 500
    /// 
    /// - mainText (string): Short user-friendly error message
    ///     Requirement: Non-null, 2-5 words
    ///     Examples: "Invalid input", "Object not found", "Access denied"
    /// 
    /// - description (string): Detailed error explanation
    ///     Requirement: Non-null, clear and actionable
    ///     Should not contain sensitive information
    /// 
    /// - traceId (string?): Optional correlation ID for request tracing
    ///     Requirement: Nullable, can be null
    ///     Default: null
    ///     When provided: Use format from W3C Trace Context or GUID
    /// 
    /// Return Value:
    ///     Fully initialized ExceptionResponse object with immutable properties.
    ///     Timestamp automatically set to DateTime.UtcNow.
    /// 
    /// Example Usage:
    ///     // Without TraceId (defaults to null)
    ///     var response = new ExceptionResponse(
    ///         "ValidationError",
    ///         400,
    ///         "Invalid input",
    ///         "Email field is required and cannot be empty.");
    /// 
    ///     // With TraceId for distributed tracing
    ///     var response = new ExceptionResponse(
    ///         "ObjectNotFound",
    ///         404,
    ///         "Object not found",
    ///         "The requested User with ID 123 was not found.",
    ///         "0HN1GH8P91FD0:00000001");
    /// 
    /// Constructor Behavior:
    ///     1. Assigns all parameters to corresponding init properties
    ///     2. Sets Timestamp to DateTime.UtcNow (current UTC time)
    ///     3. Returns immutable object (all properties are init-only)
    ///     4. Thread-safe due to immutability
    /// </summary>
    /// <param name="code">Unique error code identifying the exception type.</param>
    /// <param name="httpCode">Suggested HTTP status code for the response.</param>
    /// <param name="mainText">Short user-friendly error message.</param>
    /// <param name="description">Detailed explanation providing context for the error.</param>
    /// <param name="traceId">Optional correlation ID for distributed request tracing.</param>
    public ExceptionResponse(
        string code,
        int httpCode,
        string mainText,
        string description,
        string? traceId = null)
    {
        Code = code;
        HttpCode = httpCode;
        MainText = mainText;
        Description = description;
        Timestamp = DateTime.UtcNow;
        TraceId = traceId;
    }
}
