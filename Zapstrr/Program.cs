using System.Text;
using Jose;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Zapstrr.Models.Entities;

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

        var db = new LiteDatabase("data.db");

        app.MapPost("/api/register", ([FromBody] Account account) =>
        {
            if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
            {
                return Results.BadRequest();
            }
            
            var entry = db.GetCollection<Account>().Insert(account);
            account.Id = entry.AsInt32;
            return Results.Ok(account);
        });

        app.MapPost("/api/login", (HttpContext context, [FromBody] Account credential) =>
        {
            if (string.IsNullOrWhiteSpace(credential.Username) || string.IsNullOrWhiteSpace(credential.Password))
            {
                return Results.Unauthorized();
            }

            var account = db.GetCollection<Account>()
                .Query()
                .Where(account =>
                    account.Username.Equals(credential.Username)
                    && account.Password.Equals(credential.Password))
                .FirstOrDefault();

            if (account is null)
            {
                return Results.Unauthorized();
            }

            var payload = new Dictionary<string, object>()
            {
                { "username", account.Username },
                { "sub", context.Request.Host },
                { "exp", 1300819380 }
            };

            var secretKey = Encoding.ASCII.GetBytes("secretKey");

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