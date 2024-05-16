using Discord.Commands;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Discord;
using Project_Pineapplesummer.Modules.Services;
using Discord.WebSocket;

namespace Project_Pineapplesummer.Modules.Command_s.Search
{
    public class Search : ModuleBase<SocketCommandContext>
    {
        private int page;
        private ulong messageId;
        private ABigSearchClass.Root SearchData = new ABigSearchClass.Root();

        private string cx = "011906946170505250517:codlmgzw19s";
        private string apikey = new TokenService().GetToken("Google_api_key");

        [Command("Search")]
        [Alias("g")]
        public async Task NormalSearch([Remainder]string q)
        {
            page = 1;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1");
            string json;

            try
            {
                json = await client.GetStringAsync($"?key={apikey}&cx={cx}&q={q}");
                SearchData = JsonConvert.DeserializeObject<ABigSearchClass.Root>(json);
            }
            catch(Exception ex)
            {
                ErrorServices es = new ErrorServices();
                await es.SendErrorMessage(ex.Message, "PPSs0xNSgetError", Context.Channel, ErrorServices.severity.Error);
                return;
            }

            var msg = await Context.Channel.SendMessageAsync(null, false, MakeEmbed(0, 5));

            await msg.AddReactionAsync(new Emoji("➡️"));

            messageId = msg.Id;

            Context.Client.ReactionAdded += UpdateEmbed;
        }

        private async Task UpdateEmbed(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg1.Value.Id != messageId ||                              // if its not the message 
                arg3.UserId == Context.Client.CurrentUser.Id)             // if it was the Bot who reacted with the emotes
                return;

            //Removes the users reaction
            await arg1.Value.RemoveReactionAsync(arg3.Emote, arg3.UserId);

            switch(arg3.Emote.Name)
            {
                case "➡️":
                    NextPage(arg1.Value);
                    break;
                case "⬅️":
                    LastPage(arg1.Value);
                    break;
            }
        }

        private void LastPage(IUserMessage value)
        {
            if (page < 1)
                return;

            value.AddReactionAsync(new Emoji("➡️"));
            value.RemoveReactionAsync(new Emoji("⬅️"), value.Author);
            page--;

            value.ModifyAsync(msg => msg.Embed = MakeEmbed(0, 5));
        }

        private void NextPage(IUserMessage value)
        {
            if (page > 1)
                return;

            value.AddReactionAsync(new Emoji("⬅️"));
            value.RemoveReactionAsync(new Emoji("➡️"), value.Author);
            page++;

            value.ModifyAsync(msg => msg.Embed = MakeEmbed(5, 10));
        }

        private Embed MakeEmbed(int min, int max)
        {
            string result = "";
            for (; min < max; min++)
            {
                result += $"**[{SearchData.items[min].title}]({SearchData.items[min].link})**\n"  //Title with link
                    + $"*{SearchData.items[min].snippet}*\n";     //Snippet
            }

            if (result.Length > 2000)
                result = result.Remove(2001);

            return new EmbedBuilder()
                .WithAuthor($"Search page [{page}]", "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Ftse1.mm.bing.net%2Fth%3Fid%3DOIP.7ZvVP00p4WDHmErvpPw88gHaHa%26pid%3DApi&f=1")
                .WithTitle($"Results for: {SearchData.queries.request[0].searchTerms}")
                .WithDescription(result)
                .WithFooter($"Time: {SearchData.searchInformation.searchTime}ms / Results : {Convert.ToInt32(SearchData.searchInformation.totalResults) / 1000000} milion")
                .Build();
        }
    }
}
