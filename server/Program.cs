using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(Options =>
{
    Options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()
             .SetIsOriginAllowed(_ => true);
    });
});
builder.Services.AddSignalR();
var app = builder.Build();
app.UseCors();

app.MapHub<ChatHub>("/chatHub");


app.MapGet("/", () => "Hello World!");

app.Run();

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine("Received from client: " + message);
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
