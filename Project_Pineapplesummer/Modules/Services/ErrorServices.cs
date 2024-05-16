using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Project_Pineapplesummer.Modules.Services
{
    public class ErrorServices
    {
        public enum severity {Info, Warning, Error, DB_Error, Message, Success };

        public async Task SendErrorMessage(string message, string errorCode, ISocketMessageChannel channel, severity severity)
        {
            EmbedBuilder embed = new EmbedBuilder();

            switch(severity)
            {
                case severity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    embed.Color = new Color(68, 85, 90);
                    break;
                case severity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    embed.Color = Color.LightOrange;
                    break;
                case severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    embed.Color = Color.Red;
                    break;
                case severity.DB_Error:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    embed.Color = new Color(255, 153, 0);
                    break;
                case severity.Message:
                    break;
                case severity.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    embed.Color = Color.Green;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{severity,8}] ErrorCode:{errorCode} | Message:{message}");
            Console.ResetColor();

            embed.WithAuthor(severity.ToString())
                .WithDescription(message)
                .WithFooter($"Error code: {errorCode}");

            _ = await channel.SendMessageAsync("", false, embed.Build());
        }

        public Task SendErrorMessage(string message, string errorCode, severity severity)
        {
            switch (severity)
            {
                case severity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case severity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case severity.DB_Error:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case severity.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case severity.Message:
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{severity,8}] ErrorCode:{errorCode} | Message:{message}");
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}
