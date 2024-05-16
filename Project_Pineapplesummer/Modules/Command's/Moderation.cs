using System;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Project_Pineapplesummer.Modules.Services;
using Project_Pineapplesummer.Modules.Services.SqlServices;
using System.Data.SqlClient;
using Discord.Rest;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        readonly SqlServices SqlServices = new SqlServices();
        readonly ErrorServices ErrorServices = new ErrorServices();

        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("warn")]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string msg = null)
        {
            if (msg == null || msg.Length >= 256)
                msg = "hurting my feelings";

            if (SqlServices.InsertData("Warnings", Context.Guild.Id, user.Id, msg, Context.User.Id, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"**{user}** has been warned for `{msg}`");
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("kick")]
        public async Task KickUser(SocketGuildUser user, [Remainder] string msg)
        {
            if (user == null || msg == null)
            {
                await ErrorServices.SendErrorMessage("Invalid arguments", "PPSc0xModKickUsrArg", Context.Channel, ErrorServices.severity.Warning);
                return;
            }

            SqlServices.InsertData("Kicks", Context.Guild.Id, user.Id, msg, Context.User.Id, Context.Channel);

            try
            {
                await user.KickAsync(msg + " by " + Context.User);
                await Context.Channel.SendMessageAsync($"**{user}** has been kicked for `{msg}`");
            }
            catch (Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSc0xModKikUsr", Context.Channel, ErrorServices.severity.Error);
            }
        }

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(SocketGuildUser user, double hours, [Remainder] string msg)
        {
            if (user == null || msg == null)
            {
                await ErrorServices.SendErrorMessage("Invalid arguments", "PPSc0xModBanUsrArg", Context.Channel, ErrorServices.severity.Warning);
                return;
            }

            try
            {
                await user.BanAsync(0, msg);
            }
            catch (ArgumentException ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSc0xModBnUsr", ErrorServices.severity.Error);
            }

            DateTime time = DateTime.Now;

            if (SqlServices.InsertData(Context.Guild.Id, user.Id, Context.User.Id, msg, time, hours, Context.Channel) > 0)
            {
                int caseid = SqlServices.FindData(Context.Guild.Id, user.Id, time, "Id", "Bans", Context.Channel);
                await Context.Channel.SendMessageAsync($"**{user}** has been banned `{msg}`\n`CaseID: {caseid}`");
            }

        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task BulkDeleteMessagesAsync(int amount, [Remainder] string reason = null)
        {
            if (amount < 1)
            {
                await ErrorServices.SendErrorMessage("Invalid amount, must be greater then 0", "PPSs0xModMsDeMgArg", Context.Channel, ErrorServices.severity.Error);
                return;
            }
            if (reason == null || reason.Length > 256)
                reason = "Against TOS I guess? ¯\\_(ツ)_/¯ - Bot";
            try
            {
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                int i = 0;
                foreach (var msg in messages)
                {
                    string content = msg.Content;
                    if (msg.Content == null)
                        content = "Null";

                    if (msg.Content != $".purge {amount}" && msg.Content != $".purge {amount} {reason}")
                        await SqlServices.LogMessage(msg.Id, Context.Guild.Id, msg.Channel.Id, Context.User.Id, msg.Author.Id, content, reason, amount + 1, i, DateTime.Now, Context.Channel);
                    i++;
                }

                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            }
            catch (Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSs0xBlkDlMsgExQ", Context.Channel, ErrorServices.severity.Error);
            }
        }

        [Command("Timeout")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task TimeoutUser(SocketGuildUser user, int minutes, [Remainder] string reason)
        {
            if (reason == null || reason.Length > 256)
            {
                await ErrorServices.SendErrorMessage("Arg reason is too long", "PPSm0xTUsrArg", Context.Channel, ErrorServices.severity.Error);
                return;
            }

            ulong roleId = SqlServices.FindData(Context.Guild.Id, "TimeoutRole", "ServerSettings");

            if (roleId == 0)
            {
                GuildPermissions guildPermissions = new GuildPermissions().Modify(false, false, false, false, false, false, false, false ,true ,false,false ,false, false ,false ,true, false, false
                                                                                , false, false, false, false ,false, false, false, false, false, false, false, false, false);

                RestRole role= await Context.Guild.CreateRoleAsync("Erik-mute", guildPermissions, new Color(0, 0, 0), false, false);

                ServerSetting setting = new ServerSetting();
                await setting.SetServerSettings(role, Context.Guild);
            }

            try
            {
                SocketRole role = Context.Guild.GetRole(roleId);
                await user.AddRoleAsync(role);
            }
            catch (Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSm0xTUsrRlExc", Context.Channel, ErrorServices.severity.Error);
                return;
            }

            if (SqlServices.InsertData(Context.Guild.Id, user.Id, Context.User.Id, reason, DateTime.Now.AddMinutes(minutes), Context.Channel) > 0)
            {
                await Context.Channel.SendMessageAsync($"**{user}** has been timedout");
            }

        }

    }


    [Group("Set")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class ServerSetting : ModuleBase<SocketCommandContext>
    {
        readonly SqlServices SqlServices = new SqlServices();
        readonly ErrorServices errorServices = new ErrorServices();

        [Command("logging")]
        public async Task SetServerLoggingSeverity(int severity)
        {
            if(severity > 3 || severity < 0)
            {
                errorServices.SendErrorMessage("Invalid severity value", "PPSc0xSSLSargOF", Context.Channel, ErrorServices.severity.Error);
                return;
            }
            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", Context.Guild.Id, "ServerId"))
            {
                sqlCommand.CommandText = "UPDATE ServerSettings SET Severity = @rID WHERE ServerId = @sID";
                sqlCommand.Parameters.AddWithValue("@rID", severity);
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
            }
            else
            {
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, Severity) VALUES (@sID, @rID)";
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
                sqlCommand.Parameters.AddWithValue("@rID", severity);
            }

            if (SqlServices.ExQComm(sqlCommand, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"Server settings updated: `Severity`:`{severity}`");
            else
                await Context.Channel.SendMessageAsync($"Failed updating Server settings");

            sqlCommand.Dispose();
        }

        [Command("TimeoutRole")]
        [Alias("TRole")]
        public async Task SetTimeoutRoleCommand([Remainder] SocketRole role)
        {
            await SetServerSettings(role, Context.Guild, Context.Channel);
        }

        public async Task SetServerSettings(IRole role, SocketGuild guild = null, ISocketMessageChannel channel = null)
        {
            if (guild == null)
                guild = Context.Guild;

            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", guild.Id, "ServerId"))
            {
                sqlCommand.CommandText = "UPDATE ServerSettings SET TimeoutRole = @rID WHERE ServerId = @sID";
                sqlCommand.Parameters.AddWithValue("@rID", (long)role.Id);
                sqlCommand.Parameters.AddWithValue("@sID", (long)guild.Id);
            }
            else
            {
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, TimeoutRole) VALUES (@sID, @rID)";
                sqlCommand.Parameters.AddWithValue("@sID", (long)guild.Id);
                sqlCommand.Parameters.AddWithValue("@rID", (long)role.Id);
            }

            if (channel != null)
            {
                if (SqlServices.ExQComm(sqlCommand, channel) > 0)
                    await channel.SendMessageAsync($"Server settings updated: `Timeout-Role`:`{role.Name}`");
                else
                    await channel.SendMessageAsync("Failed updating Server settings");
            }
            else
                SqlServices.ExQComm(sqlCommand);

            sqlCommand.Dispose();
        }

        [Command("id")]
        [Alias("ulong")]
        [RequireOwner]
        public async Task SetServerSettings(string field, ulong value)
        {
            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", Context.Guild.Id, "ServerId"))
            {
                sqlCommand.CommandText = "UPDATE ServerSettings SET @fld = @rID WHERE ServerId = @sID";
                sqlCommand.Parameters.AddWithValue("@fld", field);
                sqlCommand.Parameters.AddWithValue("@rID", (long)value);
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
            }
            else
            {
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, @fld) VALUES (@sID, @rID)";
                sqlCommand.Parameters.AddWithValue("@fld", field);
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
                sqlCommand.Parameters.AddWithValue("@rID", (long)value);
            }

            if (SqlServices.ExQComm(sqlCommand, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"Server settings updated: `{field}`:`{value}`");
            else
                await Context.Channel.SendMessageAsync($"Failed updating Server settings");

            sqlCommand.Dispose();
        }

        [Command("Prefix")]
        [Alias("Pref")]
        public async Task SetServerSettings(string value)
        {
            if(value.Length > 3)
            {
                await errorServices.SendErrorMessage("Prefix value is too long, must be 3 or less characters", "PPSc0xSSSpfxOf", Context.Channel, ErrorServices.severity.Warning);
                return;
            }

            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", Context.Guild.Id, "ServerId"))
            {
                sqlCommand.CommandText = "UPDATE ServerSettings SET Prefix = @chr WHERE ServerId = @sID";
                sqlCommand.Parameters.AddWithValue("@chr", value);
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
            }
            else
            {
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, Prefix) VALUES (@sID, @chr)";
                sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);
                sqlCommand.Parameters.AddWithValue("@chr", value);
            }

            if (SqlServices.ExQComm(sqlCommand, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"Server settings updated: `Prefix`:`{value}`");
            else
                await Context.Channel.SendMessageAsync("Failed updating Server settings");

            sqlCommand.Dispose();
        }

        [Command("LogChannel")]
        [Alias("Log")]
        public async Task SetLogChannel([Remainder] SocketTextChannel channel)
        {
            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", Context.Guild.Id, "ServerId"))
                sqlCommand.CommandText = "UPDATE ServerSettings SET LogChannel = @cID WHERE ServerId = @sID";
            else
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, LogChannel) VALUES (@sID, @cID)";

            sqlCommand.Parameters.AddWithValue("@cID", (long)channel.Id);
            sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);

            if (SqlServices.ExQComm(sqlCommand, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"Server settings updated: `Log-Channel`:`{channel.Name}`");
            else
                await Context.Channel.SendMessageAsync($"Failed updating Server settings");

            sqlCommand.Dispose();
        }

        [Command("Notificationchannel")]
        [Alias("Notif")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetNotificationChannel([Remainder] SocketTextChannel channel)
        {
            SqlCommand sqlCommand = new SqlCommand();

            if (SqlServices.Contains("ServerSettings", Context.Guild.Id, "ServerId"))
                sqlCommand.CommandText = "UPDATE ServerSettings SET NotificationChannel = @cID WHERE ServerId = @sID";
            else
                sqlCommand.CommandText = "INSERT INTO ServerSettings (ServerId, NotificationChannel) VALUES (@sID, @cID)";

            sqlCommand.Parameters.AddWithValue("@cID", (long)channel.Id);
            sqlCommand.Parameters.AddWithValue("@sID", (long)Context.Guild.Id);

            if (SqlServices.ExQComm(sqlCommand, Context.Channel) > 0)
                await Context.Channel.SendMessageAsync($"Server settings updated: `Notification-Channel`:`{channel.Name}`");
            else
                await Context.Channel.SendMessageAsync($"Failed updating Server settings");

            sqlCommand.Dispose();
        }

    }
}
