using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Command_s
{
    [Group("Help")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task SendHelpModule()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Help menu")
                .WithColor(Color.DarkTeal)
                .WithFooter(".help <category>")

                .AddField(":scales: Moderation", "Server managment commands", true)
                .AddField(":partly_sunny: Weather", "Weather info commands", true)
                .AddField(":file_folder: Info", "Information commands", true)

                .AddField(":alarm_clock: Time", "setting timers", true)
                .AddField(":page_facing_up: Settings", "Server settings", true)
                .AddField(":loud_sound: Voice", "In proggress", true);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Moderation")]
        public async Task SendModerationCategoryInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":scales: Moderation Info menu")
                .WithDescription("Some commands have extra info for moderators/admins")
                .WithColor(Color.DarkTeal)

                .AddField("warn <user> <reason>", "adds a warning to the users Server moderation record")
                .AddField("timeout <user> <minutes> <reason>", "removes the users ability to send messages in certain Text channels for the specified amount of time")
                .AddField("purge <amount>", "Deletes the specified amount of messages")
                .AddField("Kick <user> <reason>", "Kicks the user from the server")
                .AddField("Ban <user> <hours> <reason>", "Bans the user from the server for speicifed amount of time, <hours> can be set to -1 to never unban");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Weather")]
        public async Task SendWeatherCommandInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("weather <location>")
                .WithDescription("Overload: w <location> \nGets the weather information for the location using OpenWeatherMap's API")
                .WithColor(Color.DarkTeal)
                .WithFooter("Used as: .weather London");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Info")]
        public async Task SendInfoCategoryInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":file_folder: Info menu")
                .WithDescription("Some commands have extra info for moderators/admins")
                .WithColor(Color.DarkTeal)

                .AddField("user <user>", "information about the user")
                .AddField("server ", "information about the server")
                .AddField("role <role>", "infromation about the role")
                .AddField("channel <channel>", "information about the channel")
                .AddField("bot", "information about the bot");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Voice")]
        public async Task SendVoiceCategoryInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":loud_sound: Voice menu")
                .WithColor(Color.DarkTeal)

                .AddField("join <channel>", "channel can be left empty to join the channel you're currently in")
                .AddField("leave", "leaves its current voice channel")
                .AddField("play", "Not working yet");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Time")]
        public async Task SendTimeCategoryInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":alarm_clock: Time menu")
                .WithColor(Color.DarkTeal)

                .AddField("timer <minutes> <message>", "sets a timer that sends the message in the channel when the timer expires\n<minutes> any whole number\n<message> can be left empyt");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Settings")]
        public async Task SendSetCategoryInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":alarm_clock: Time menu")
                .WithColor(Color.DarkTeal)

                .AddField("set Log <channel>", "sets the channel for bot to log server/user changes (User banned, user joined info...)")
                .AddField("set Notif <channel>", "sets the channel for bot to send notification (User joined, left...)")
                .AddField("set preifx", "sets the prefix for the bot inside the guild. 3 characters maximum.")
                .AddField("set Logging <severity>", "**0** (no notifications & logs [default]), **1** (User joined, left + Some Moderation logging), **2** (User joined, left + All Moderation logging ) **3** Everything")
                .AddField("set TimeoutRole <role>", "sets the role given to timedout users");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ff")]
        public async Task DoX()
        {
            await Context.Channel.SendMessageAsync(Context.Message.GetJumpUrl().Substring(32).Remove(18));
        }
    }
}
