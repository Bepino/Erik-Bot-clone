using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Project_Pineapplesummer.Modules.Services;

namespace Project_Pineapplesummer.Modules.Command_s
{
    [Group("account")]
    [Alias("acc")]
    public class Banking : ModuleBase<SocketCommandContext>
    {
        [Command("create")]
        [Alias("+")]
        public async Task CreateBankAccount()
        {
            try
            {
                BankingServices bs = new BankingServices();
                await bs.CreateAccount(Context.User.Id);
            }
            catch
            { 
                return;
            }

            await Context.Channel.SendMessageAsync("Done did");
        }
    }
}
