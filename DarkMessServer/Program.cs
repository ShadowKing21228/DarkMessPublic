using System.Text;
using DarkMessServer.Core.Config;
using DarkMessServer.Core.Security;
using DarkMessServer.Infrastructure.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

JwtConfig.Init(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = JwtHandler.JwtParameters;
    });
builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001); // ws
    //options.ListenAnyIP(25565, listenOptions =>
    //{
    //    listenOptions.UseHttps(); // wss
    //});
});

var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
};
app.UseWebSockets(webSocketOptions);

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws") {
        await WebSocketHandler.HandleWebSocketAsync(context);
    }
    else {
        await next();
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();