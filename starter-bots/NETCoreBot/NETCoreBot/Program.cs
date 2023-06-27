using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using NETCoreBot.Models;
using NETCoreBot.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NETCoreBot
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        private static async Task Main(string[] args)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder().AddJsonFile(
                $"appsettings.json",
                optional: false
            );

            Configuration = builder.Build();
            var environmentIp = Environment.GetEnvironmentVariable("RUNNER_IPV4");
            var ip = !string.IsNullOrWhiteSpace(environmentIp)
                ? environmentIp
                : Configuration.GetSection("RunnerIP").Value;
            ip = ip.StartsWith("http://") ? ip : "http://" + ip;

            var botNickname =
                Environment.GetEnvironmentVariable("BOT_NICKNAME")
                ?? Configuration.GetSection("BotNickname").Value;

            var token =
                Environment.GetEnvironmentVariable("Token") ??
                Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");

            var port = Configuration.GetSection("RunnerPort");

            var url = ip + ":" + port.Value + "/runnerhub";

            var connection = new HubConnectionBuilder()
                .WithUrl($"{url}")
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            var botService = new BotService();

            await connection.StartAsync();
            Console.WriteLine("Connected to Runner");

            connection.On<Guid>("Registered", (id) => botService.SetBotId(id));

            connection.On<String>(
                "Disconnect",
                async (reason) =>
                {
                    Console.WriteLine($"Server sent disconnect with reason: {reason}");
                    await connection.StopAsync();
                }
            );

            connection.On<BotStateDTO>(
                "ReceiveBotState",
                (botState) =>
                {
                    botService.SetBotState(botState);
                }
            );

            connection.Closed += (error) =>
            {
                Console.WriteLine($"Server closed with error: {error}");
                return Task.CompletedTask;
            };

            await connection.InvokeAsync("Register", token, botNickname);
            while (connection.State == HubConnectionState.Connected)
            {
                var state = botService.GetBotState();
                var botId = botService.GetBotId();
                if (state == null || botId == null)
                {
                    continue;
                }
                Console.WriteLine($"Bot ID: {botId}");
                Console.WriteLine(
                    $"Position: ({state.X}, {state.Y}), Collected: {state.Collected}, Level: {state.CurrentLevel}"
                );
                Console.WriteLine(state.PrintWindow());
            }
        }
    }
}
