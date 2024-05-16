using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Project_Pineapplesummer.Modules.Services
{
    internal class TimeServices
    {
        internal async Task EventHandler(DiscordSocketClient client)
        {
            SqlServices.SqlServices sqlServices = new SqlServices.SqlServices();
            ErrorServices es = new ErrorServices();

            await Task.Delay(3000);

            await es.SendErrorMessage("TimeServices has started...", "null", ErrorServices.severity.Success);

            //the forever loop
            for(; ; )
            {
                await es.SendErrorMessage("Refresing DataTable for TimeServices...", "null", ErrorServices.severity.Info);

                var data = sqlServices.FindData("SELECT * FROM Time");
                int count = data.Rows.Count;

                await es.SendErrorMessage("Refreshed DataTable for TimeServices...", "null", ErrorServices.severity.Info);

                await es.SendErrorMessage($"Collected {data.Rows.Count} rows from Time table", "null", ErrorServices.severity.Message);

                //Goes through rows in DataTable
                for (int i=0; i < count; i++)
                {
                    //Checks if timer needs to be activated
                    DateTime time = (DateTime)data.Rows[i][1];
                    if (DateTime.Now >= time.AddSeconds(-10))
                    {
                        //Converting SQL's Bigint into a ulong because yes
                        long foo = (long)data.Rows[i][0];
                        ulong server = Convert.ToUInt64(foo);

                        SocketTextChannel channel = client.GetGuild(server).GetTextChannel(Convert.ToUInt64(data.Rows[i][3]));
                        try
                        {
                            string msg = Convert.ToString(data.Rows[i][2]);
                            string usr = Convert.ToString(data.Rows[i][5]);

                            EmbedBuilder embed = new EmbedBuilder();
                            embed.WithTitle("Timer expired")
                                .WithColor(Color.DarkOrange)
                                .WithDescription(msg)
                                .WithFooter(usr)
                                .WithCurrentTimestamp();

                            await channel.SendMessageAsync(null, false, embed.Build());
                        }
                        catch(Exception ex)
                        {
                            if(channel == null)
                                await es.SendErrorMessage(ex.Message, "Tserv0xTimTE", ErrorServices.severity.Error);
                            else
                                await es.SendErrorMessage(ex.Message, "Tserv0xTimTE", channel, ErrorServices.severity.Error);
                        }

                        sqlServices.RemoveData("Time", "ID", data.Rows[i][4].ToString(), null, channel);
                    }

                    await es.SendErrorMessage($"Going thorugh row {i}", "null", ErrorServices.severity.Message);
                }

                await Task.Delay(30000);
            }
            //|-------------|
            //| the void !!!| (/ ._.)/ Hemlp
            //|-------------|
        }
    }

}
