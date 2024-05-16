using Discord.Commands;
using Project_Pineapplesummer.Modules.Services.SqlServices;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Project_Pineapplesummer.Modules.Command_s.Search;
using System.Data;
using Newtonsoft.Json;
using Project_Pineapplesummer.Modules.Services;
using Discord;
using Discord.WebSocket;

namespace Project_Pineapplesummer.Modules.Command_s
{
    public class ImageSearchCommands : ModuleBase<SocketCommandContext>
    {
        SqlServices SqlServices = new SqlServices();
        ErrorServices ErrorServices = new ErrorServices();

        private string apikey = new TokenService().GetToken("Google_api_key");

        //Search engine Id
        private string cx = "011906946170505250517:codlmgzw19s";

        internal int page = 0;
        internal ulong id = 0;
        internal string query = "";
        internal string[] title = new string[10];
        internal string[] contextlink = new string[10];
        internal string[] link = new string[10];

        [Command("img")]
        [Alias("image")]
        public async Task ImageSearchCommand([Remainder] string q)
        {
            if(q.Contains("dick"))
            {
                await Context.Channel.SendMessageAsync("Ahh... I see you are a Jojo fan aswell");
                return;
            }
            if (q.Contains("porn"))
            {
                await Context.Channel.SendMessageAsync("No. Bad user, bad!!!");
                return;
            }
            if (q.Contains("hentai"))
            {
                await Context.Channel.SendMessageAsync("https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fi.pinimg.com%2Foriginals%2F71%2Ffa%2F2d%2F71fa2dec5038fa8f74a879fce8ae8457.png&f=1&nofb=1");
                return;
            }

            page = 0;
            ImageClass.Root data = null;

            try
            { 
                string json = await GetJson(q);
                data = JsonConvert.DeserializeObject<ImageClass.Root>(json);
            }
            catch{ return; }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{q} images")
                .WithDescription($"[{data.items[page].title}]({data.items[page].image.contextLink})")
                .WithImageUrl(data.items[page].link)
                .WithFooter($"Result [{page+1}]", "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Ftse1.mm.bing.net%2Fth%3Fid%3DOIP.7ZvVP00p4WDHmErvpPw88gHaHa%26pid%3DApi&f=1");

            var msg = await Context.Channel.SendMessageAsync(null, false, embed.Build());

            id = msg.Id;
            query = q;
            for(int i = 0; i < 10; i++)
            {
                title[i] = data.items[i].title;
                contextlink[i] = data.items[i].image.contextLink;
                link[i] = data.items[i].link;
            }

            IEmote[] emotes = {new Emoji("⏮️"), new Emoji("⬅️"), new Emoji("➡️"), new Emoji("⏭️"), new Emoji("❌") };
            await msg.AddReactionsAsync(emotes);

            Context.Client.ReactionAdded += UpdateEmbed;
        }

        private async Task UpdateEmbed(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg1.Value.Id != id ||                              // if its not the message 
                arg3.UserId == Context.Client.CurrentUser.Id)       // if it was the Bot who reacted with the emotes
                return;

            //Removes the users reaction
            await arg1.Value.RemoveReactionAsync(arg3.Emote, arg3.UserId);

            switch(arg3.Emote.Name)
            {
                case "➡️":
                    await NextPage(arg1.Value);
                    break;
                case "⬅️":
                    await LastPage(arg1.Value);
                    break;
                case "❌":
                    await End(arg1.Value);
                    break;
                case "⏮️":
                    await ToFirst(arg1.Value);
                    break;
                case "⏭️":
                    await ToLast(arg1.Value);
                    break;
                default:
                    break;
            }
        }

        private async Task ToFirst(IUserMessage value)
        {
            if (page == 0)
                return;

            page = 0;

            ModifyEmbed(value);
        }

        private async Task ToLast(IUserMessage value)
        {
            if (page == 9)
                return;
            
            page = 9;

            ModifyEmbed(value);
        }

        private async Task NextPage(IUserMessage message)
        {
            if (page == 9)
                return;

            ++page;

            ModifyEmbed(message);
        }

        private async Task LastPage(IUserMessage message)
        {
            if (page == 0)
                return;

            --page;

            ModifyEmbed(message);
        }

        private async void ModifyEmbed(IUserMessage message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"{query} images")
                .WithDescription($"[{title[page]}]({contextlink[page]})")
                .WithImageUrl(link[page])
                .WithFooter($"Page [{page + 1}]", "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Ftse1.mm.bing.net%2Fth%3Fid%3DOIP.7ZvVP00p4WDHmErvpPw88gHaHa%26pid%3DApi&f=1");

            await message.ModifyAsync(msg => msg.Embed = embed.Build());
        }

        private async Task End(IUserMessage message)
        {
            message.RemoveAllReactionsAsync();

            Context.Client.ReactionAdded -= UpdateEmbed;
        }

        private async Task<string> GetJson(string query)
        {
            string json = "";

            if (SqlServices.Contains("ImageSearch", query, "Query"))
            {
                TimeSpan Fqtn = DateTime.Now - SqlServices.FindData(query, "Time", "ImageSearch", Context.Channel);
                if (Fqtn.TotalDays < 1)
                {
                    DataTable tbl = SqlServices.FindData($"SELECT Json FROM ImageSearch WHERE Query = {query}");
                    json = (string)tbl.Rows[0][0];

                    return json;
                }
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1");

            try
            {
                json = await client.GetStringAsync($"?key={apikey}&cx={cx}&searchType=image&num=10&q={query}");
                SqlServices.CacheJson("ImageSearch", query, json, Context.Channel);
            }
            catch(Exception ex)
            {
                await ErrorServices.SendErrorMessage(ex.Message, "PPSs0xImgSGJ00", Context.Channel, ErrorServices.severity.Error);
            }

            return json;
        }
    }
}
