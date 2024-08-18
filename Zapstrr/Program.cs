using Jose;
using Microsoft.AspNetCore.Mvc;
using Zapstrr.Models;

namespace Zapstrr;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddCors();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseFileServer();
        app.UseHttpsRedirection();
        app.MapFallbackToFile("index.html");

        app.MapPost("/api/login", ([FromBody] Credential credential) =>
        {
            if (string.IsNullOrWhiteSpace(credential.Username) || string.IsNullOrWhiteSpace(credential.Password))
            {
                return Results.Unauthorized();
            }
            
            if (!string.Equals(credential.Username, credential.Password))
            {
                return Results.Unauthorized();
            }
            
            var payload = new Dictionary<string, object>()
            {
                { "username", credential.Username },
                { "sub", "localhost:5169" },
                { "exp", 1300819380 }
            };

            var secretKey = new byte[]
            {
                164, 60, 194, 0, 161, 189, 41, 38, 130, 89, 141, 164, 45, 170, 159, 209, 69, 137, 243, 216, 191, 131,
                47, 250, 32, 107, 231, 117, 37, 158, 225, 234
            };

            string token = Jose.JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
            return Results.Ok(token);
        });

        app.UseCors(
            config => config.AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true));

        app.Run();
    }
}