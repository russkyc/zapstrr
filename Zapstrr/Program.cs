using System.Security.Cryptography;
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
        
        // Accounts

        app.MapPost("/api/register", ([FromBody] Account account) =>
        {
            if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
            {
                return Results.BadRequest();
            }

            var existing = db.GetCollection<Account>()
                .Count(entry => entry.Username.Equals(account.Username));

            if (existing > 0)
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

        // Quizzes
        
        app.MapGet("/api/quizzes/{id}", ([FromRoute] int id) =>
        {
            var quiz = db.GetCollection<Quiz>()
                .FindById(id);

            if (quiz is null)
            {
                return Results.NotFound(id);
            }
            
            return Results.Ok(quiz);
        });
        
        app.MapGet("/api/quizzes", () =>
        {
            var quizzes = db.GetCollection<Quiz>()
                .Query()
                .OrderByDescending(quiz => quiz.Id)
                .ToList();
            return Results.Ok(quizzes);
        });
        
        app.MapPost("/api/quizzes", ([FromBody] Quiz quiz) =>
        {
            var entry = db.GetCollection<Quiz>()
                .Insert(quiz);
            
            quiz.Id = entry.AsInt32;
            return Results.Ok(quiz);
        });
        
        // Game
        app.MapGet("/api/play/{id}", ([FromRoute] int id) =>
        {
            var quiz = db.GetCollection<Quiz>()
                .FindById(id);

            if (quiz is null)
            {
                return Results.NotFound(id);
            }

            var response = new
            {
                code = GenerateCode(),
                quiz = quiz
            };
            
            return Results.Ok(response);
            
            string GenerateCode()
            {
                const string digits = "0123456789";
                var code = new StringBuilder(7);
                using (var rng = new RNGCryptoServiceProvider())
                {
                    byte[] randomNumber = new byte[1];
                    for (int i = 0; i < 7; i++)
                    {
                        rng.GetBytes(randomNumber);
                        int index = randomNumber[0] % digits.Length;
                        code.Append(digits[index]);
                    }
                }
                return code.ToString();
            }
        });
        

        app.UseCors(
            config => config.AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true));

        app.Run();
    }
}