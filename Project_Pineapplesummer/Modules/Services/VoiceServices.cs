using System;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Services.VoiceServices
{
    public class VoiceServices
    {
        DiscordSocketClient _client;
        SqlServices.SqlServices SqlServices = new SqlServices.SqlServices();

        internal async Task VoiceEventHandler(DiscordSocketClient client)
        {
            _client = client;

            //If bot (dis)connects or moves -> update SQL DB
            client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            await Task.Delay(-1);
        }

        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState from, SocketVoiceState to)
        {
            //Checks if the SocketUser is the bot
            if (arg1.Id != _client.CurrentUser.Id)
                return;

            //Updates DB
            if (from.VoiceChannel == null )
            {
                if (SqlServices.Contains("VoiceConn", to.VoiceChannel.Guild.Id, "ServerID"))
                    await SqlServices.UpdateData(to.VoiceChannel.Guild.Id, to.VoiceChannel.Id, 1);
                else
                    await SqlServices.InsertData(to.VoiceChannel.Guild.Id, to.VoiceChannel.Id);
            }
            else
            {
                if (SqlServices.Contains("VoiceConn", from.VoiceChannel.Guild.Id, "ServerID"))
                    await SqlServices.UpdateData(from.VoiceChannel.Guild.Id, 0, 0);
                else
                    await SqlServices.InsertData(from.VoiceChannel.Guild.Id, from.VoiceChannel.Id);
            }

            return;
        }

        internal async Task AddToQueue(string name, string url)
        {
            throw new NotImplementedException();
        }
    }
}
