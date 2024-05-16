using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Project_Pineapplesummer.Modules.Services.SqlServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class Info : ModuleBase<SocketCommandContext>
    {
        readonly SqlServices sqlServices = new SqlServices();

        [Command("server")]
        [Alias("Server-info")]
        public async Task SendServerInfo()
        {
            string ad = sqlServices.GetRandomString("Ads");

            string owner = Context.Guild.Owner.Username;
            if (Context.Guild.Id == 371694158072512512)
                owner = "Augustin";

            int bots = 0;
            foreach (var user in Context.Guild.Users)
            {
                if (user.IsBot)
                    bots++;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{Context.Guild.Name}'s Info")
                .WithColor(new Color(0, 138, 230))
                .AddField("ID", Context.Channel.Id, true)
                .AddField("Created at", Context.Guild.CreatedAt.Date.ToShortDateString(), true)
                .AddField("Memebers", (Context.Guild.MemberCount - bots).ToString(), true)

                .AddField("Default channel", Context.Guild.DefaultChannel.Name, true)
                .AddField("Boosters", Context.Guild.PremiumSubscriptionCount.ToString(), true)
                .AddField("Timeout", (Context.Guild.AFKTimeout / 60).ToString() + " min", true)

                .AddField("Bans", (await Context.Guild.GetBansAsync()).Count.ToString(), true)
                .AddField("Warnings", (sqlServices.Count("Warnings", "ServerID", Context.Guild.Id, Context.Channel)).ToString(), true)
                .AddField("Owner", owner, true)

                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithFooter(ad, "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fpng.pngtree.com%2Fpng_detail%2F18%2F09%2F10%2Fpngtree-AD-Letter-Logo-png-clipart_3592378.jpg&f=1&nofb=1");

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("user")]
        [Alias("User-info")]
        public async Task SendUserInfo(SocketGuildUser user = null)
        {
            if (user is null)
                user = Context.Guild.GetUser(Context.User.Id);

            string ad = sqlServices.GetRandomString("Ads");
            string roles = "";
            EmbedBuilder embed = new EmbedBuilder();

            string perms;
            if (user.GuildPermissions.Administrator)
                perms = "Administrator";
            else if (user.GuildPermissions.ManageMessages || user.GuildPermissions.ManageChannels)
                perms = "Moderator";
            else
                perms = "Member";

            foreach (SocketRole role in user.Roles)
            {
                if (role.IsEveryone)
                    continue;
                roles += role.Mention + "\n";
            }
            if (string.IsNullOrEmpty(roles))
                roles = "None";

            embed.WithAuthor($"Requested by: {Context.User}", Context.User.GetAvatarUrl(), Context.User.GetAvatarUrl())
                .WithTitle(user.ToString() + "'s information")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(ad)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithCurrentTimestamp()

                .AddField("ID", user.Id, true)
                .AddField("Created", user.CreatedAt.DateTime.ToShortDateString(), true)
                .AddField("Joined", user.JoinedAt.Value.DateTime.ToShortDateString() + " " + user.JoinedAt.Value.DateTime.ToShortTimeString(), true)

                .AddField("Roles", roles, true)
                .AddField("Status", perms, true)
                .AddField("Bot?", user.IsBot, true);

            SocketGuildUser socketGuildUser = Context.Guild.GetUser(Context.User.Id);
            if(socketGuildUser.GuildPermissions.ManageMessages)
            {
                string warnings = "";
                if (sqlServices.Contains("Warnings", Context.Guild.Id, "ServerId", user.Id, "UserId", Context.Channel))
                {
                    System.Data.DataTable warningTBL = sqlServices.FindData($"SELECT * FROM Warnings WHERE ServerId = {Context.Guild.Id} AND UserId = {user.Id}", Context.Channel);
                    for (int i = 0; i < warningTBL.Rows.Count; i++)
                    {
                        SocketUser mod = Context.Client.GetUser(Convert.ToUInt64(warningTBL.Rows[i][4]));
                        warnings += $"\n[{i+1}] " + warningTBL.Rows[i][2].ToString() + $" by **{mod}**";
                    }
                }
                else
                    warnings = "None";

                string bans = "";
                if (sqlServices.Contains("Bans", Context.Guild.Id, "ServerId", user.Id, "UserId", Context.Channel))
                {
                    System.Data.DataTable banTBL = sqlServices.FindData($"SELECT * FROM Bans WHERE ServerId = {Context.Guild.Id} AND UserId = {user.Id}", Context.Channel);
                    for (int i = 0; i < banTBL.Rows.Count; i++)
                    {
                        SocketUser mod = Context.Client.GetUser(Convert.ToUInt64(banTBL.Rows[i][3]));
                        bans += $"\n\n[{i+1}] " + banTBL.Rows[i][4].ToString() + $" by **{mod}** [{Convert.ToDateTime(banTBL.Rows[i][5]).ToShortDateString()}]";

                        if (!banTBL.Rows[i][10].Equals(DBNull.Value) && Convert.ToBoolean(banTBL.Rows[i][10]))
                        {
                            SocketUser revokemod = Context.Client.GetUser(Convert.ToUInt64(banTBL.Rows[i][7]));
                            bans += $"\n  **↳** _revoked: {banTBL.Rows[i][9]} by __{revokemod}__ [{Convert.ToDateTime(banTBL.Rows[i][8]).ToShortDateString()}]_";
                        }
                    }

                    banTBL.Dispose();
                }
                else
                    bans = "None";

                embed.AddField("Warnings", warnings)
                    .AddField("Bans", bans);
            }

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("bot")]
        public async Task SendBotInfo()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Info (ALPHA)")
                .WithColor(new Color(0, 138, 230))
                .AddField("Name", "ErikAPI", true)
                .AddField("Language", "COBOL", true)
                .AddField("Database", "PršutDB", true)
                .AddField("Purpose", "Crypto mining", true)
                .AddField("Owner", "Dalmatinski Institut za integraciju pršuta u modernu tehnologiju", true)
                .AddField("Version", "0.0.0.5", true)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("role")]
        [Alias("role-info")]
        public async Task SendRoleInfo([Remainder] SocketRole role)
        {
            string ad = sqlServices.GetRandomString("Ads");
            bool isTimeoutRole = sqlServices.FindData(Context.Guild.Id, "TimeoutRole", "ServerSettings", Context.Channel) == role.Id;
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithAuthor($"Requested by {Context.User}", Context.User.GetAvatarUrl())
                .WithTitle($"{role.Name}'s information")
                .WithColor(role.Color)
                .WithFooter(ad)
                
                .AddField("ID", role.Id, true)
                .AddField("Is separated", role.IsHoisted, true)
                .AddField("Position", role.Position, true)

                .AddField("Members", role.Members.Count(), true)
                .AddField("IsTimeoutRole", isTimeoutRole, true)
                .AddField("IsBotRole", role.IsManaged, true)

                .AddField("Is Admin", role.Permissions.Administrator, true)
                .AddField("Can Manage messages", role.Permissions.ManageMessages, true)
                .AddField("Can Manage channels", role.Permissions.ManageChannels, true);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("channel")]
        [Alias("text-channel")]
        public async Task SendTextChannelInfo(SocketTextChannel channel)
        {
            string ad = sqlServices.GetRandomString("Ads");
            string topic = channel.Topic;
            if (string.IsNullOrEmpty(topic))
                topic = "N/A";
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle(channel.Name + " text channel info")
                .WithFooter(ad)
                .WithColor(Color.DarkerGrey)
                
                .AddField("ID", channel.Id, true)
                .AddField("Created at", channel.CreatedAt.DateTime.ToShortDateString(), true)
                .AddField("Description", topic, true)

                .AddField("Slow mode", channel.SlowModeInterval.ToString(), true)
                .AddField("NSFW?", channel.IsNsfw, true)
                .AddField("Users", channel.Users.Count, true);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("channel")]
        [Alias("voice-channel")]
        public async Task SendVoiceChannelInfo(SocketVoiceChannel channel)
        {
            string ad = sqlServices.GetRandomString("Ads");
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle(channel.Name + " voice channel info")
                .WithFooter(ad)
                .WithColor(Color.Blue)

                .AddField("Bitrate", channel.Bitrate, true)
                .AddField("Created at", channel.CreatedAt.DateTime.ToShortDateString(), true)
                .AddField("ID", channel.Id, true);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}
