using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Project_Pineapplesummer.Modules.Services;
using System.Linq;
using Project_Pineapplesummer.Modules.Services.SqlServices;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class Basic : ModuleBase<SocketCommandContext>
    {
        readonly ErrorServices ErrorServices = new ErrorServices();
        readonly SqlServices SqlServices = new SqlServices();

        [Command("join")]
        public async Task JoinVoiceChannel([Remainder]SocketVoiceChannel channel = null)
        {
            if (channel == null)
            {
                foreach (SocketVoiceChannel vc in Context.Guild.VoiceChannels)
                {
                    if (vc.Users.Contains(Context.User))
                    {
                        channel = vc;
                        break;
                    }
                }

                if (channel == null)
                {
                    await ErrorServices.SendErrorMessage("User is not in a valid voice channel", "PPSv0xjvc01", Context.Channel, ErrorServices.severity.Error);
                    return;
                }
            }

            try
            {
                await channel.ConnectAsync(false, false, true);
            }
            catch
            {
                await ErrorServices.SendErrorMessage("Something went wrong", "PPSv0xjvc02", Context.Channel, ErrorServices.severity.Error);
                return ;
            }
        }

        /// <summary>
        /// Joins voice channel and returns int value 1 or 0
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async Task<int> JoinVoice(SocketGuild guild, ISocketMessageChannel messageChannel ,SocketVoiceChannel channel = null)
        {
            if (channel == null)
            {
                foreach (SocketVoiceChannel vc in guild.VoiceChannels)
                {
                    if (vc.Users.Contains(Context.User))
                    {
                        channel = vc;
                        break;
                    }
                }

                if (channel == null)
                {
                    await ErrorServices.SendErrorMessage("User is not in a valid voice channel", "PPSv0xjvc01", messageChannel, ErrorServices.severity.Error);
                    return 0;
                }
            }

            try
            {
                await channel.ConnectAsync(false, false, true);
            }
            catch
            {
                await ErrorServices.SendErrorMessage("Something went wrong", "PPSv0xjvc02", messageChannel, ErrorServices.severity.Error);
                return 0;
            }

            return 1;
        }

        [Command("leave")]
        public async Task LeaveVoiceChannel()
        {
            try
            {
                ulong t = SqlServices.FindData(Context.Guild.Id, "VoiceID", "VoiceConn", Context.Channel);

                if (t == 0)
                {
                    await ErrorServices.SendErrorMessage("There was an error retriving voice channel ID", "PPSv0xlvc01", Context.Channel, ErrorServices.severity.Error);
                    return;
                }
                else
                    await Context.Guild.GetVoiceChannel(t).DisconnectAsync();
            }
            catch (Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSv0xlvc02", Context.Channel, ErrorServices.severity.Error);
            }
        }

        [Command("play")]
        public async Task PlayAudio(string url)
        {
            //If bot isn't in a voice channel, then it joins the users channel
            if (SqlServices.Contains("VoiceConn", 0, $"VoiceID", Context.Guild.Id, "ServerID", Context.Channel))
            {
                if (await JoinVoice(Context.Guild, Context.Channel) == 0)
                {
                    await Context.Channel.SendMessageAsync("Goodbye..");
                    return;
                }
            }

            //add song to queue
            //
            // Add after completing api stream (YT/)
            //
        }

    }
}
