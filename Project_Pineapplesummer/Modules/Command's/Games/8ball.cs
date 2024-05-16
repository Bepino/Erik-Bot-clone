using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Project_Pineapplesummer.Modules.Command_s.Games
{
    public class _8ball : ModuleBase<SocketCommandContext>
    {
        string[] answers = { "Yes", "No", "Maybe", "I don't know", "Can you repeat the question?", "You're not the boss of me now", "Life is unfair" };

        [Command("8ball")]
        public async Task EightBall([Remainder] string q)
        {
            int r = new Random().Next(0, answers.Length);

            await Context.Channel.SendMessageAsync(answers[r]);
        }
    }
}
