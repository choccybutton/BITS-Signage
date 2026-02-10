namespace BITS.Signage.Api.Common;

/// <summary>
/// RFC 7807 Problem Details response format.
/// Used for all error responses to provide consistent, machine-readable error information.
/// </summary>
public class ProblemResponse
{
    public string Type { get; set; } = "about:blank";
    public string Title { get; set; } = null!;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, object>? Extensions { get; set; }

    /// <summary>
    /// Creates a problem response for the given status code and message.
    /// </summary>
    public static ProblemResponse Create(int statusCode, string title, string? detail = null)
    {
        return new ProblemResponse
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = GetTypeUri(statusCode)
        };
    }

    /// <summary>
    /// Maps HTTP status codes to RFC 7807 problem type URIs.
    /// </summary>
    private static string GetTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        412 => "https://tools.ietf.org/html/rfc9110#section-15.5.13",
        413 => "https://tools.ietf.org/html/rfc9110#section-15.5.14",
        428 => "https://tools.ietf.org/html/rfc6585#section-3",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        503 => "https://tools.ietf.org/html/rfc9110#section-15.6.3",
        _ => "about:blank"
    };
}
