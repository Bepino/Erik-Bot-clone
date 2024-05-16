using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Services
{
    public class LoggingServices
    {
        readonly SqlServices.SqlServices SqlServices = new SqlServices.SqlServices();
        readonly ErrorServices ErrorServices = new ErrorServices();
        DiscordSocketClient socketClient = new DiscordSocketClient();

        internal async Task StartLogging(DiscordSocketClient client)
        {
            socketClient = client;

            //Severity level : 1
            client.UserJoined += UserJoined;
            client.UserLeft += UserLeft;

            //Severity level : 2 
            client.UserBanned += UserBanned;
            client.UserUnbanned += UserUnBanned;
            client.MessagesBulkDeleted += MessageBulkDeleted;

            //Severity level : 3
            client.ChannelCreated += ChannelCreated;
            client.ChannelDestroyed += ChannelDestroyed; //Doesn't fire for some reason

            client.RoleCreated += RoleCreated;
            client.RoleDeleted += RoleDeleted;
            client.RoleUpdated += RoleUpdated;
        }

        private async Task RoleUpdated(SocketRole role, SocketRole newrole)
        {
            if (SqlServices.GetSeverity(role.Guild.Id) < 3)
                return;

            var log = await role.Guild.GetAuditLogsAsync(1, null, null, null, ActionType.RoleDeleted).Flatten().ToListAsync();

            SocketTextChannel logChannel = role.Guild.GetTextChannel(SqlServices.FindData(role.Guild.Id, "LogChannel", "ServerSettings"));
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Role updated")
                .WithColor(role.Color)
                .AddField("Name", newrole.Name, true)
                .AddField("Updated by", log[0].User.ToString(), true);

            var roleList = role.Permissions.ToList();
            var newroleList = newrole.Permissions.ToList();

            string removed = "";
            foreach(var e in roleList)
            {
                if(!newroleList.Contains(e))
                {
                    removed += e + "\n";
                }
            }
            if (string.IsNullOrEmpty(removed))
                removed = "None";

            string added = "";
            foreach (var e in newroleList)
            {
                if (!roleList.Contains(e))
                {
                    added += e + "\n";
                }
            }
            if (string.IsNullOrEmpty(added))
                added = "None";

            embed.AddField("Added", added)
                .AddField("Removed", removed);

            await logChannel.SendMessageAsync(null, false, embed.Build());
        }

        private async Task RoleDeleted(SocketRole role)
        {
            if (SqlServices.GetSeverity(role.Guild.Id) < 3)
                return;

            var log = await role.Guild.GetAuditLogsAsync(1, null, null, null, ActionType.RoleDeleted).Flatten().ToListAsync();

            SocketTextChannel logChannel = role.Guild.GetTextChannel(SqlServices.FindData(role.Guild.Id, "LogChannel", "ServerSettings"));
            Embed embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Role deleted")

                .AddField("Name", role.Name, true)
                .AddField("Deleted by", log[0].User.ToString(), true)
                .AddField("Speareted?", role.IsHoisted, true)
                .AddField("Admin?", role.Permissions.Administrator, true)
                .AddField("Send messages?", role.Permissions.SendMessages, true)
                .AddField("Postition", role.Position, true)

                .Build();

            await logChannel.SendMessageAsync(null, false, embed);
        }

        private async Task RoleCreated(SocketRole role)
        {
            await Task.Delay(10000);

            role = role.Guild.GetRole(role.Id);

            if (SqlServices.GetSeverity(role.Guild.Id) < 3)
                return;

            var log = await role.Guild.GetAuditLogsAsync(1, null, null, null, ActionType.RoleCreated).Flatten().ToListAsync();

            SocketTextChannel logChannel = role.Guild.GetTextChannel(SqlServices.FindData(role.Guild.Id, "LogChannel", "ServerSettings"));
            Embed embed = new EmbedBuilder()
                .WithColor(role.Color)
                .WithTitle("Role created")

                .AddField("Name", role.Name, true)
                .AddField("Created by", log[0].User.ToString(), true)
                .AddField("Speareted?", role.IsHoisted, true)
                .AddField("Admin?", role.Permissions.Administrator, true)
                .AddField("Send messages?", role.Permissions.SendMessages, true)
                .AddField("Postition", role.Position, true)

                .Build();

            await logChannel.SendMessageAsync(null, false, embed);
        }

        private async Task ChannelDestroyed(SocketChannel channel)
        {
            SocketGuild socketGuild = null;

            foreach (var guild in socketClient.Guilds)
            {
                foreach (var e in guild.Channels)
                {
                    if (e.Id == channel.Id)
                        socketGuild = guild;
                }
            }

            if (socketGuild == null)
                return;

            if (SqlServices.GetSeverity(socketGuild.Id) < 3)
                return;

            ulong logChannelId = SqlServices.FindData(socketGuild.Id, "LogChannel", "ServerSettings");
            SocketTextChannel logChannel = socketGuild.GetTextChannel(logChannelId);
            var log = await socketGuild.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelDeleted).Flatten().ToListAsync();

            Embed embed = new EmbedBuilder()
                .WithTitle("Channel Deleted")
                .WithColor(Color.Red)

                .AddField("Name", channel.ToString(), true)
                .AddField("Deleted by", log[0].User.ToString(), true)
                .AddField("Accessible to", channel.Users.Count.ToString(), true)

                .Build();

            await logChannel.SendMessageAsync(null, false, embed);
        }

        private async Task ChannelCreated(SocketChannel channel)
        {
            SocketGuild socketGuild = null;

            foreach(var guild in socketClient.Guilds)
            {
                foreach(var e in guild.Channels)
                {
                    if (e.Id == channel.Id)
                        socketGuild = guild;
                }
            }

            if (socketGuild == null)
                return;

            if (SqlServices.GetSeverity(socketGuild.Id) < 3)
                return;

            ulong logChannelId = SqlServices.FindData(socketGuild.Id, "LogChannel", "ServerSettings");
            SocketTextChannel logChannel = socketGuild.GetTextChannel(logChannelId);
            var log = await socketGuild.GetAuditLogsAsync(1, null, null, null, ActionType.ChannelCreated).Flatten().ToListAsync();

            Embed embed = new EmbedBuilder()
                .WithTitle("Channel created")
                .WithColor(Color.Green)

                .AddField("Name", channel.ToString(), true)
                .AddField("Created by", log[0].User.ToString(), true)
                .AddField("Accessible to", channel.Users.Count.ToString(), true)

                .Build();

            await logChannel.SendMessageAsync(null, false, embed);
        }

        private async Task MessageBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cache, ISocketMessageChannel channel)
        {
            await Task.Delay(2000);

            IMessage FirstMessage = cache.ToList()[0].Value;
            ulong serverid = Convert.ToUInt64(FirstMessage.GetJumpUrl().Substring(32).Remove(18));

            if (SqlServices.GetSeverity(serverid) > 1)
            {
                ulong logChannelId = SqlServices.FindData(serverid, "LogChannel", "ServerSettings");
                SocketTextChannel logChannel = socketClient.GetGuild(serverid).GetTextChannel(logChannelId);

                //Because Discord.NET hasn't been updated yet,
                string mod = "Have you considerd using .purge?";

                if (SqlServices.Contains("MessageLog", FirstMessage.Id, "MessageID"))
                {
                    var tbl = SqlServices.FindData($"SELECT ModeratorID FROM MessageLog WHERE MessageID = {FirstMessage.Id}");
                    ulong modId = Convert.ToUInt64(tbl.Rows[0][0]);
                    mod = socketClient.GetUser(modId).ToString();
                }

                Embed embed = new EmbedBuilder()
                    .WithTitle("Messages bulk deleted")
                    .WithDescription(FirstMessage.Id.ToString())
                    .WithColor(Color.LightOrange)

                    .AddField("Count", cache.Count, true)
                    .AddField("Channel", channel.Name, true)
                    .AddField("Removed by", mod, true)

                    .Build();

                await logChannel.SendMessageAsync(null, false, embed);
            }

            int i = 0;
            foreach(var message in cache)
            {
                //if the message is an embed without a message it throws a hissie fit and doesn't want to give an message.Id
                //which throws an error and everything burns to the ground
                try
                {
                    if (!SqlServices.Contains("MessageLog", message.Value.Id, "MessageID"))
                    {
                        string content = message.Value.Content;
                        if (string.IsNullOrEmpty(content))
                            content = "Null";
                        await SqlServices.LogMessage(message.Value.Id, serverid, channel.Id, 0, message.Value.Author.Id, content, "Idk", cache.Count, i, DateTime.Now, channel);
                    }
                }
                catch(Exception ex)
                {
                    await ErrorServices.SendErrorMessage(ex.Message, "LGS0xMsgBDLgg", ErrorServices.severity.Error);
                }

                i++;
            }
        }

        private async Task UserUnBanned(SocketUser user, SocketGuild guild)
        {
            if (SqlServices.GetSeverity(guild.Id) < 2)
                return;

            var log = (await guild.GetAuditLogsAsync(1, null, null, null, ActionType.Unban).FlattenAsync()).ToList();

            int CaseID = SqlServices.FindBan(user.Id, guild.Id);
            string reason = log[0].Reason;
            if (string.IsNullOrEmpty(reason))
                reason = "Idk discord won't say";

            await SqlServices.AddRevoke(log[0].User.Id, reason, true, CaseID);

            EmbedBuilder embed = new EmbedBuilder();
            SocketUser mod = guild.GetUser(log[0].User.Id);
            var ban = SqlServices.GetBan(CaseID);

            embed.WithTitle("User unbanned")
                .WithColor(Color.Green)
                .WithThumbnailUrl(user.GetAvatarUrl())

                .AddField("User", user.ToString(), true)
                .AddField("ID", user.Id, true)
                .AddField("Moderator", mod.ToString(), true)

                .AddField("Reason", reason, true)
                .AddField("Ban reson", ban.Reason, true)
                .AddField("Banned by", guild.GetUser(ban.ModeratorId).ToString(), true);

            ulong channelId = SqlServices.FindData(guild.Id, "LogChannel", "ServerSettings");

            if (channelId != 0)
            {
                SocketTextChannel channel = guild.GetTextChannel(channelId);

                await channel.SendMessageAsync(null, false, embed.Build());
            }
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            if (SqlServices.GetSeverity(guild.Id) < 2)
                return ;

            await Task.Delay(1000);
            
            string warnings = "";
            DataTable tbl = SqlServices.FindData($"SELECT * FROM Warnings WHERE UserId = {user.Id} AND ServerId = {guild.Id}");

            for(int i = 0; i < tbl.Rows.Count; i++)
            {
                warnings += $"[{i}] {tbl.Rows[i][2]} \n";
            }
            if (string.IsNullOrEmpty(warnings))
                warnings = "None";

            var log = (await guild.GetAuditLogsAsync(1, null, null, null, ActionType.Ban).FlattenAsync()).ToList();
            IUser mod = log[0].User;
            int expire = -1;

            string reason = log[0].Reason;
            if (string.IsNullOrEmpty(reason))
                reason = "Something";

            if (!SqlServices.IsBanLogged(user, guild))
            {
                SqlServices.InsertData(guild.Id, user.Id, log[0].User.Id, reason, DateTime.Now, -1);
            }
            else
            {
                var ban = SqlServices.GetBan(SqlServices.FindBan(user.Id, guild.Id));

                expire = DateTime.Now.Subtract(ban.ExpirationTime).Hours;
                mod = guild.GetUser(ban.ModeratorId);
            }

            ulong channelId = SqlServices.FindData(guild.Id, "LogChannel", "ServerSettings");

            if (channelId != 0)
            {
                EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("User banned")
                .WithColor(Color.Red)
                .WithThumbnailUrl(user.GetAvatarUrl())

                .AddField("User", user.ToString(), true)
                .AddField("Id", user.Id, true)
                .AddField("Banned by", mod.ToString(), true)

                .AddField("Reason", reason, true)
                .AddField("Warnings", warnings, true)
                .AddField("Expires", "In " + expire + " hours", true); //has some personal issues displaying negative numbers

                SocketTextChannel channel = guild.GetTextChannel(channelId);

                await channel.SendMessageAsync(null, false, embed.Build());
            }

            channelId = SqlServices.FindData(guild.Id, "NotificationChannel", "ServerSettings");

            if (channelId != 0)
            {
                SocketTextChannel channel = guild.GetTextChannel(channelId);

                await channel.SendMessageAsync($"{user} has been banned");
            }
        }

        private Task UserLeft(SocketGuildUser user)
        {
            //Comment if you want the bot to send a leave message when a user gets banned (UserBanned sends its own message)
            if (SqlServices.IsBanLogged(user, user.Guild) ||                //if user has been banned
                SqlServices.GetSeverity(user.Guild.Id) < 1)                 //if server doesn't log/notify of this
                return Task.CompletedTask;

            if (SqlServices.Contains("ServerSettings", 0, "NotificationChannel", user.Guild.Id, "ServerId"))
            {
                ErrorServices.SendErrorMessage("NotificationChannel not set to a value", "LGGs0xULNCNull", ErrorServices.severity.Error);
                return Task.CompletedTask;
            }

            ulong channelId = SqlServices.FindData(user.Guild.Id, "NotificationChannel", "ServerSettings");

            SocketTextChannel channel = user.Guild.GetTextChannel(channelId);

            string message = SqlServices.GetRandomString("UserLeft").Replace("{user}", user.Mention);

            _ = channel.SendMessageAsync(message);

            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            if (SqlServices.GetSeverity(user.Guild.Id) < 1)
                return Task.CompletedTask;

            //Send notification to notification-channel
            if (SqlServices.Contains("ServerSettings", 0, "NotificationChannel", user.Guild.Id, "ServerId"))
                ErrorServices.SendErrorMessage("NotificationChannel not set to a value", "LGGs0xUJNCNull", ErrorServices.severity.Error);
            else
            {
                ulong channelId = SqlServices.FindData(user.Guild.Id, "NotificationChannel", "ServerSettings");

                SocketTextChannel channel = user.Guild.GetTextChannel(channelId);

                string message = SqlServices.GetRandomString("UserJoin").Replace("{user}", user.Mention);

                _ = channel.SendMessageAsync(message);
            }

            //Send message about user in log-channel
            if (SqlServices.Contains("ServerSettings", 0, "LogChannel", user.Guild.Id, "ServerId"))
                ErrorServices.SendErrorMessage("LogChannel  not set to a value", "LGGs0xUJLCNull", ErrorServices.severity.Error);
            else
            {
                ulong channelId = SqlServices.FindData(user.Guild.Id, "LogChannel", "ServerSettings");
                SocketTextChannel channel = user.Guild.GetTextChannel(channelId);

                string clients = "-";
                foreach(var client in user.ActiveClients)
                {
                    clients += client.ToString();
                }

                var offset = DateTime.Now - user.CreatedAt.DateTime;
                string CreatedAgo;
                if (offset.TotalMinutes < 1)
                    CreatedAgo = offset.TotalSeconds.ToString() + " s";
                else if (offset.TotalHours < 1)
                    CreatedAgo = offset.Minutes.ToString() + " min";
                else if (offset.TotalDays < 1)
                    CreatedAgo = offset.TotalHours.ToString() + " h";
                else if (offset.TotalDays < 5)
                    CreatedAgo = offset.TotalDays.ToString() + " days";
                else
                    CreatedAgo = user.CreatedAt.DateTime.ToShortDateString() + " " + user.CreatedAt.DateTime.ToShortTimeString();

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("User Joined")
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(user.GetAvatarUrl())

                    .AddField("User", user.ToString(), true)
                    .AddField("ID", user.Id, true)
                    .AddField("Bot?", user.IsBot, true)

                    .AddField("Created", CreatedAgo, true)
                    .AddField("Clients", clients, true)
                    .AddField("Mention", user.Mention, true);

                _ = channel.SendMessageAsync(null, false, embed.Build());
            }

            return Task.CompletedTask;
        }
    }
}
