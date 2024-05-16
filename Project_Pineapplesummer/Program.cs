using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Project_Pineapplesummer.Modules.Services;
using Project_Pineapplesummer.Modules.Services.SqlServices;
using Project_Pineapplesummer.Modules.Services.VoiceServices;

class Program
{
    //Discord API token
    private readonly string token = new TokenService().GetToken("Discord_api_key");

    static void Main()
    {
        Program program = new Program();
        program.MainAsync().GetAwaiter().GetResult();
    }

    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    
    public IServiceProvider Services { get; }

    internal Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Debug,

            ExclusiveBulkDelete = true,

            MessageCacheSize = 200,
        });

        _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,

            CaseSensitiveCommands = false,
        });

        _client.Log += Log;
        _commands.Log += Log;
    }

    private static Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();

        return Task.CompletedTask;
    }

    private async Task MainAsync()
    { 
        await InitCommands().ConfigureAwait(true);

        await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(true);
        await _client.StartAsync();

        //Aditional event subscriptions
        await Start();

        await Task.Delay(Timeout.Infinite);
    }

    private Task Start()
    {
        new TimeServices().EventHandler(_client);

        new VoiceServices().VoiceEventHandler(_client);

        new ModerationServices().StartModerationServices(_client);

        new LoggingServices().StartLogging(_client);

        new StatusServices().SetStatus(_client);

        return Task.CompletedTask;
    }

    private async Task InitCommands()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

        _client.MessageReceived += HandleCommandAsync;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (!(arg is SocketUserMessage msg)) return;

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

        int pos = 0;

        string prefix = new SqlServices().GetPrefix(arg.GetJumpUrl().Substring(32).Remove(18), arg.Channel).Replace(" ", string.Empty);
        
        if (msg.HasStringPrefix(prefix, ref pos) || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
        {
            var context = new SocketCommandContext(_client, msg);

            var result = await _commands.ExecuteAsync(context, pos, Services);
            if(!result.IsSuccess)
            {
                ErrorServices es = new ErrorServices();

                switch(result.Error)
                {
                    case CommandError.UnmetPrecondition:
                        await es.SendErrorMessage(result.ErrorReason, "Prog0xUnmetPrecondition", context.Channel, ErrorServices.severity.Error);
                        break;
                    case CommandError.MultipleMatches:
                        await es.SendErrorMessage(result.ErrorReason, "Prog0xMultipleMatches", context.Channel, ErrorServices.severity.Error);
                        break;
                    case CommandError.ParseFailed:
                        await es.SendErrorMessage("One or more arguments may have been inputed wrong", "Prog0xParseFailed", context.Channel, ErrorServices.severity.Error);
                        break;
                    case CommandError.Exception:
                        await es.SendErrorMessage(result.ErrorReason, "Prog0x?Exception", context.Channel, ErrorServices.severity.Error);
                        break;
                    case CommandError.UnknownCommand:
                        await es.SendErrorMessage(result.ErrorReason, "Prog0xUnknown", context.Channel, ErrorServices.severity.Error);
                        break;
                    case CommandError.Unsuccessful:
                        await es.SendErrorMessage(result.ErrorReason, "Prog0xFail", context.Channel, ErrorServices.severity.Error);
                        break;
                }
            }
        }

    }

}

