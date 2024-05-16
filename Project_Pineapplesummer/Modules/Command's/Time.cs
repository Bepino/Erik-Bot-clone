using Discord.Commands;
using System;
using System.Threading.Tasks;
using Project_Pineapplesummer.Modules.Services;
using Project_Pineapplesummer.Modules.Services.SqlServices;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class Time : ModuleBase<SocketCommandContext>
    {
        ErrorServices ErrorServices = new ErrorServices();

        [Command("timer")]
        public async Task SetTimer(int minutes, [Remainder] string msg = null)
        {
            if (minutes < 1)
            {
                await ErrorServices.SendErrorMessage("Invalid input", "PPSt0xST01", Context.Channel, ErrorServices.severity.Error);
                return;
            }

            if (msg == null || msg.Length >= 255)
            {
                msg = $"Timer for {Context.User.Mention} has expired";
            }

            DateTime time = DateTime.Now.AddMinutes(minutes);

            SqlServices services = new SqlServices();

            int result = services.InsertData(Context.Guild.Id, time, msg, $"{Context.User.Username}#{Context.User.Discriminator}", Context.Channel);

            if (result > 0)
            {
                await Context.Channel.SendMessageAsync($"Timer has been set for {time.ToShortTimeString()}");
            }
        }

    }
}
