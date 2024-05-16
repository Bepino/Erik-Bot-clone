using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Services
{
    public class ModerationServices : ModuleBase<SocketCommandContext>
    {
        readonly ErrorServices es = new ErrorServices();
        readonly SqlServices.SqlServices SqlServices = new SqlServices.SqlServices();

        public Task StartModerationServices(DiscordSocketClient client)
        {
            //waits for it to connect 
            Task.Delay(5000).Wait();

            BanService(client);
            TimeoutService(client);
            es.SendErrorMessage("Moderation services started", "Smsm0x", ErrorServices.severity.Message);
            return Task.CompletedTask;
        }

        private async void TimeoutService(DiscordSocketClient client)
        {
            DataTable tbl = SqlServices.FindData("SELECT * FROM Timeouts");
            await es.SendErrorMessage($"Updated Timeouts table [{tbl.Rows.Count}]", "Smsm0xT", ErrorServices.severity.Info);
            ulong serverid;

            //iterates through the DataTable
            for (int j = 0; j < tbl.Rows.Count; j++)
            {
                TimeSpan offset = DateTime.Now - Convert.ToDateTime(tbl.Rows[j][5]);
                if (offset.TotalMinutes > 0 && offset.TotalMinutes < 1)
                {
                    try
                    {
                        serverid = Convert.ToUInt64(tbl.Rows[j][1]);
                        SocketGuild guild = client.GetGuild(serverid);
                        
                        SocketGuildUser user = guild.GetUser(Convert.ToUInt64(tbl.Rows[j][2]));
                        SocketRole role = guild.GetRole(SqlServices.FindData(serverid, "TimeoutRole", "ServerSettings"));

                        if (SqlServices.RemoveData("Timeouts", "ID", ((int)tbl.Rows[j][0]).ToString()) > 0)
                        {
                            //Removes the timeout role
                            user.RemoveRoleAsync(role).Wait();
                        }
                        else
                            await es.SendErrorMessage($"Something went wrong in removing {tbl.Rows[j][0]} from Timeouts.SQL", "Smsm0xTRmvRw", ErrorServices.severity.DB_Error);
                    }
                    catch (Exception ex)
                    {
                        await es.SendErrorMessage(ex.Message, "Smsm0xTconvArg", ErrorServices.severity.Warning);
                        continue;
                    }

                    //sends message to #<log-channel> (because there is not event for this)
                    ulong chnId = SqlServices.FindData(serverid, "LogChannel", "ServerSettings");

                    if (chnId != 0)
                    {
                        EmbedBuilder embed = new EmbedBuilder();
                        SocketUser mod = client.GetUser(Convert.ToUInt64(tbl.Rows[j][3]));
                        SocketUser user = client.GetUser(Convert.ToUInt64(tbl.Rows[j][2]));

                        embed.WithTitle("Timeout expired")
                            .WithColor(Color.Purple)
                            .AddField("User", user.ToString())
                            .AddField("Timeout reason", (string)tbl.Rows[j][4])
                            .WithCurrentTimestamp()
                            .WithFooter("Action by: " + mod.ToString(), mod.GetAvatarUrl());

                        await client.GetGuild(serverid).GetTextChannel(chnId).SendMessageAsync(null, false, embed.Build());
                    }
                }
            }
            tbl.Dispose();
            await Task.Delay(5000);

            //calling the void
            TimeoutService(client);
        }

        private async void BanService(DiscordSocketClient client)
        {
            DataTable tbl = SqlServices.FindData("SELECT * FROM Bans");
            await es.SendErrorMessage($"Updated Bans table [{tbl.Rows.Count}]", "Smsm0xB", ErrorServices.severity.Info);

            for (int j = 0; j < tbl.Rows.Count; j++)
            {
                TimeSpan offset = DateTime.Now - Convert.ToDateTime(tbl.Rows[j][6]);
                //activates if time.now is grater then time.expiration and this ban has not been revoked yet
                //  | -> makes sure we don't trigger on infinite bans     | -> checks if field is DBNull or 0 because they !=   (/ ._.)/
                if (offset.TotalMinutes > 0 && offset.TotalMinutes < 0.7 && (tbl.Rows[j][10].Equals(DBNull.Value) || tbl.Rows[j][10].Equals(false)))
                {
                    SocketGuild guild = client.GetGuild(Convert.ToUInt64(tbl.Rows[j][1]));

                    try
                    {
                        await guild.RemoveBanAsync(Convert.ToUInt64(tbl.Rows[j][2]));
                        await SqlServices.AddRevoke(client.CurrentUser.Id, "Time for ban has expired", true, (int)tbl.Rows[j][0]);
                    }
                    catch(Exception ex)
                    {
                        await es.SendErrorMessage(ex.Message, "Smsm0xBtry", ErrorServices.severity.Error);
                    }

                }
            }

            tbl.Dispose();
            await Task.Delay(20000);

            //Calling on the void
            BanService(client);
        }
    }
}
