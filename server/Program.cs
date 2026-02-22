// Program.cs
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Server
{
  class Program
  {
    // Simple GameRoom class to track 2 players
    class GameRoom
    {
      public WebSocket? Player1 { get; set; }
      public WebSocket? Player2 { get; set; }

      public string? RoomId { get; set; }

      public GameRoom(string roomId)
      {
        RoomId = roomId;
      }

      public WebSocket? GetOther(WebSocket sender)
      {
        if (Player1 == sender) return Player2;
        if (Player2 == sender) return Player1;
        return null;
      }

      public bool IsFull() => Player1 != null && Player2 != null;
    }

    // Rooms dictionary
    static ConcurrentDictionary<string, GameRoom> rooms = new();

    static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var app = builder.Build();

      _ = app.UseWebSockets();

      _ = app.MapGet("/", () => "Hello World!");
      _ = app.MapGet("/greet/{name}", (string name) => $"Hello, {name}!");

      _ = app.Map("/ws", static async context =>
      {
        if (!context.WebSockets.IsWebSocketRequest)
        {
          context.Response.StatusCode = 400;
          return;
        }

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("Player connected!");

        var buffer = new byte[1024 * 4];

        // First message must be join: { "type": "join", "room": "room1" }
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var firstMsg = Encoding.UTF8.GetString(buffer, 0, result.Count);

        string? roomId = null;
        try
        {
          var json = System.Text.Json.JsonDocument.Parse(firstMsg);
          if (json.RootElement.GetProperty("type").GetString() == "join")
          {
            roomId = json.RootElement.GetProperty("room").GetString();
            var room = rooms.GetOrAdd(roomId, id => new GameRoom(id));

            if (room.Player1 == null)
              room.Player1 = ws;
            else if (room.Player2 == null)
              room.Player2 = ws;
            else
            {
              // Room full
              await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Room full", CancellationToken.None);
              return;
            }

            Console.WriteLine($"Player joined room {roomId}");
            await ws.SendAsync(Encoding.UTF8.GetBytes($"Joined room {roomId}"), WebSocketMessageType.Text, true, CancellationToken.None);
          }
        }
        catch
        {
          await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid join message", CancellationToken.None);
          return;
        }

        // Main loop
        while (!result.CloseStatus.HasValue)
        {
          result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
          var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
          Console.WriteLine($"Received: {message}");

          if (roomId != null && rooms.TryGetValue(roomId, out var room))
          {
            // Broadcast to other player
            var other = room.GetOther(ws);
            if (other != null && other.State == WebSocketState.Open)
            {
              await other.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
          }
        }

        // Cleanup on disconnect
        if (roomId != null && rooms.TryGetValue(roomId, out var cleanupRoom))
        {
          if (cleanupRoom.Player1 == ws) cleanupRoom.Player1 = null;
          if (cleanupRoom.Player2 == ws) cleanupRoom.Player2 = null;

          if (cleanupRoom.Player1 == null && cleanupRoom.Player2 == null)
          {
            _ = rooms.TryRemove(roomId, out _);
            Console.WriteLine($"Room {roomId} removed");
          }
        }

        Console.WriteLine("Player disconnected");
      });

      ChessBoard board = new ChessBoard();

      app.Run("http://0.0.0.0:5062");
    }
  }
}