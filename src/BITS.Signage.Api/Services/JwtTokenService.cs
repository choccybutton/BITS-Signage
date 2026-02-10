using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BITS.Signage.Api.Services;

/// <summary>
/// Service for issuing and validating JWT tokens.
/// Supports both user and device tokens with appropriate claims.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a user JWT token with tenant and venue roles.
    /// </summary>
    string CreateUserToken(string userId, string tenantId, List<string> tenantRoles, Dictionary<string, List<string>> venueRoles);

    /// <summary>
    /// Creates a device JWT token for player authentication.
    /// </summary>
    string CreateDeviceToken(string deviceId, string venueId, string tenantId);

    /// <summary>
    /// Creates a refresh token (opaque string stored in database).
    /// </summary>
    string CreateRefreshToken();
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string CreateUserToken(string userId, string tenantId, List<string> tenantRoles, Dictionary<string, List<string>> venueRoles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", userId),
            new("tid", tenantId),
            new("typ", "user"),
        };

        // Add tenant roles
        if (tenantRoles.Any())
        {
            claims.Add(new Claim("t_roles", string.Join(",", tenantRoles)));
        }

        // Add venue roles as JSON
        if (venueRoles.Any())
        {
            var venueRolesJson = System.Text.Json.JsonSerializer.Serialize(
                venueRoles.ToDictionary(x => x.Key, x => string.Join(",", x.Value)));
            claims.Add(new Claim("v_roles", venueRolesJson));
        }

        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: GetAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateDeviceToken(string deviceId, string venueId, string tenantId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("did", deviceId),
            new("vid", venueId),
            new("tid", tenantId),
            new("typ", "device"),
        };

        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: GetAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(365), // Long-lived device token
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        // Generate a cryptographically secure random token
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }

    private string GetSigningKey()
    {
        var key = _config["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("JWT secret key not configured; using default (insecure)");
            return "default-insecure-key-change-in-production-12345678";
        }
        return key;
    }

    private string GetIssuer()
    {
        return _config["Jwt:Issuer"] ?? "https://bits-signage.local";
    }

    private string GetAudience()
    {
        return _config["Jwt:Audience"] ?? "https://bits-signage.local";
    }
}
