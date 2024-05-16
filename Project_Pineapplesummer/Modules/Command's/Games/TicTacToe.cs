using Discord.Commands;
using Discord;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Project_Pineapplesummer.Modules.Services;
using System.Timers;
using Discord.Rest;
using System.IO;

namespace Project_Pineapplesummer.Modules.Command_s.Games
{
    public class TicTacToe : ModuleBase<SocketCommandContext>
    {
        ErrorServices ErrorServices = new ErrorServices();

        private MatchData mData = new MatchData();
        private GameData gData = new GameData();

        Timer t = new Timer() { Interval = 20 * 1000, AutoReset = false, Enabled = false };

        [Command("TicTacToe")]
        [Alias("xo")]
        public async Task StartTicTacToe(SocketUser p2)
        {
            Console.WriteLine(InProgress());
            if (InProgress())
            {
                await ErrorServices.SendErrorMessage(gData.InProgress, "PPSg0xSTTTfg", Context.Channel, ErrorServices.severity.Warning);
                return;
            }

            mData.channel = Context.Channel;
            mData.field = new string[3][]
            {
                    new string[3],
                    new string[3],
                    new string[3]
            };
            mData.GuildID = Context.Guild.Id;

            if (p2.Id != Context.User.Id)
            {
                mData.p1ID = Context.User.Id;
                mData.p2ID = p2.Id;
                SetProgress();
                await SendInvite(p2, Context.Channel);
            }
            else
            {
                await Context.Channel.SendMessageAsync("AI games are not yet supported");
                return;

                //Bool indcating which players turn it is
                //1 = p1 (x)
                //0 = p2 (o)
                mData.turn = true;
                mData.p1ID = Context.User.Id;
                mData.p2ID = Context.Client.CurrentUser.Id; //Change to Client id when finished testing

                mData.Message = await Context.Channel.SendMessageAsync("Starting game...");

                await DrawField(mData.channel);
                Context.Client.MessageReceived += GetInput;
            }
        }

        private async Task GetInput(SocketMessage arg)
        {
            if (arg.Channel.Id == mData.channel.Id)
            {
                if (mData.turn)
                {
                    if (arg.Author.Id == mData.p1ID && validInput(arg))
                    {
                        await DrawField(mData.channel);

                        if (CheckForWin())
                        {
                            WinScreen();
                        }

                        mData.turn = !mData.turn;
                    }
                }
                else
                {
                    if (arg.Author.Id == mData.p2ID && validInput(arg))
                    {
                        await DrawField(mData.channel);

                        if (CheckForWin())
                        {
                            WinScreen();
                        }

                        mData.turn = !mData.turn;
                    }
                }
            }
        }

        private async void WinScreen()
        {
            SetProgress();
            Context.Client.MessageReceived -= GetInput;

            IUser user;
            
            if(mData.turn)
                user = await mData.channel.GetUserAsync(mData.p1ID);
            else
                user = await mData.channel.GetUserAsync(mData.p2ID);

            Embed embed = new EmbedBuilder()
                .WithTitle($"{user} won")
                .WithDescription("+50 social credit points")
                .Build();

            await new BankingServices().AddToAccount(50, user.Id);

            _ = mData.channel.SendMessageAsync(null, false, embed);
        }

        private bool validInput(SocketMessage arg)
        {
            int col_a;
            int col_b;

            switch (arg.Content.ToLower()[0])
            {
                case 'a':
                    col_a = 0;
                    break;
                case 'b':
                    col_a = 1;
                    break;
                case 'c':
                    col_a = 2;
                    break;
                case 'x':
                    mData.turn = !mData.turn;
                    Context.Client.MessageReceived -= GetInput;
                    WinScreen();
                    return false;
                default:
                    return false;
            }

            try
            {
                col_b = Convert.ToInt32(arg.Content.Substring(1)) -1;
            }
            catch(FormatException)
            { return false; }

            if (col_b > 2 && col_b < 0)
                return false;

            if (mData.field[col_a][col_b] != null)
                return false;

            if (mData.turn)
                mData.field[col_a][col_b] = gData.x;
            else
                mData.field[col_a][col_b] = gData.o;

            return true;
        }

        private bool CheckForWin()
        {
            //Check the horizontal
            for (int i = 0; i < 3; i++)
            {
                if (mData.field[i][0] == null)
                    continue;
                if (mData.field[i][0] == mData.field[i][1] && mData.field[i][1] == mData.field[i][2])
                    return true;
            }

            //Check the vertical
            for (int i = 0; i < 3; i++)
            {
                if (mData.field[0][i] == null)
                    continue;
                if (mData.field[0][i] == mData.field[1][i] && mData.field[1][i] == mData.field[2][i])
                    return true;
            }

            //Check the diagonals
            if (mData.field[1][1] != null)
            {
                if (mData.field[0][0] == mData.field[1][1] && mData.field[1][1] == mData.field[2][2])
                    return true;

                if (mData.field[0][2] == mData.field[1][1] && mData.field[1][1] == mData.field[2][0])
                    return true;
            }

            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if (mData.field[i][j] == null)
                        return false;
                }
            }

            DrawScreen();
            return false;
        }

        private async void DrawScreen()
        {
            SetProgress();
            Context.Client.MessageReceived -= GetInput;

            Embed embed = new EmbedBuilder()
                .WithTitle($"Draw")
                .WithDescription("nobody gets social credit points")
                .Build();

            _ = mData.channel.SendMessageAsync(null, false, embed);
        }

        private async Task DrawField(ISocketMessageChannel channel)
        {
            try
            {
                string field = "";

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (string.IsNullOrEmpty(mData.field[i][j]))
                            field += gData.NullField;
                        else
                            field += mData.field[i][j];

                        if (j != 2)
                            field += ":heavy_plus_sign: ";
                    }
                    field += "\n";

                    if (i != 2)
                        field += ":heavy_plus_sign: :heavy_plus_sign: :heavy_plus_sign: :heavy_plus_sign: :heavy_plus_sign: \n";
                }

                IUser p1 = await mData.channel.GetUserAsync(mData.p1ID);
                IUser p2 = await mData.channel.GetUserAsync(mData.p2ID);

                Embed embed = new EmbedBuilder().WithTitle("TicTacToe")
                    .WithDescription($":x: {p1}\n:o: {p2}\n--------------------------\n{field}")
                    .Build();

                await mData.Message.DeleteAsync();
                mData.Message = await channel.SendMessageAsync(null, false, embed);
            }
            catch (Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "Fuck", channel, ErrorServices.severity.Error);
            }
        }

        private async Task SendInvite(SocketUser p2, ISocketMessageChannel channel)
        {
            Context.Client.ReactionAdded += CheckReaction;

            mData.Message = await channel.SendMessageAsync($"{channel.GetUserAsync(mData.p1ID).Result.Username} invited {p2.Mention} to play TicTacToe\nReact with {gData.Postive} to accept, {gData.Negative} to deny");

            //Requires unicode
            IEmote[] e = { new Emoji(gData.Postive), new Emoji(gData.Negative) };

            await mData.Message.AddReactionsAsync(e);

            t.Start();
            t.Elapsed += T_Elapsed;
        }

        private async Task CheckReaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg1.Value.Id == mData.p2ID && arg2.Id == mData.channel.Id)
            {
                //why can't i use gData.Positive/Negative in a switch case
                if (arg3.Emote.Name == gData.Postive)
                {
                    t.Elapsed -= T_Elapsed;
                    t.Dispose();
                    //Bool indcating which players turn it is
                    //1 = p1 (x)
                    //0 = p2 (o)
                    mData.turn = true;

                    await DrawField(mData.channel);
                    Context.Client.MessageReceived += GetInput;
                }
                else if (arg3.Emote.Name == gData.Negative)
                {
                    t.Elapsed -= T_Elapsed;
                    t.Dispose();
                    SetProgress();

                    await arg2.SendMessageAsync("User denied the invite :weary:");
                }
                else
                {
                    await arg2.SendMessageAsync("e");
                }

                Context.Client.ReactionAdded -= CheckReaction;
            }
        }

        //If the user doesn't react to the message, automatically cancels the game
        private async void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            await mData.Message.DeleteAsync();
            mData.Message = await mData.channel.SendMessageAsync($"{mData.channel.GetUserAsync(mData.p2ID).Result.Username} has denied to play TicTacToe :weary:");

            SetProgress();

            t.Elapsed -= T_Elapsed;
            t.Dispose();
        }

        private void SetProgress()
        {
            if (File.Exists("../Inprogress.txt"))
                File.Delete("../Inprogress.txt");
            else File.CreateText("../Inprogress.txt");
        }

        private bool InProgress()
        {
            if (File.Exists("../Inprogress.txt"))
                return true;
            else return false;
        }

        //Data for the match in progress
        private class MatchData
        {
            internal RestUserMessage Message { get; set; }
            internal bool turn { get; set; }
            internal int Victor { get; set; }
            internal ulong GuildID { get; set; }
            internal ISocketMessageChannel channel { get; set; }

            internal bool p2Invite { get; set; }

            internal ulong p1ID { get; set; }
            internal ulong p2ID { get; set; }

            internal string[][] field { get; set; }
        }

        //Data for drawing fields and sending messages
        private class GameData
        {
            internal readonly string InProgress = "There is a game in progress already";

            //set with the Id of a 
            internal readonly ulong verticalLineId = 0;

            internal readonly string Postive = "✅";
            internal readonly string Negative = "❌";
            internal readonly string x = "❌ ";
            internal readonly string o = "⭕ ";
            internal readonly string NullField = ":black_large_square: ";
        }
    }
}
