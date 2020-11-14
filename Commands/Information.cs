﻿using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Globalization;
using System.Threading;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Web;
using Newtonsoft.Json;
using JifBot.Models;
using JIfBot;

namespace JifBot.Commands
{
    public class Information : ModuleBase
    {
        [Command("define")]
        [Remarks("-c- word OR -c- word -m")]
        [Summary("Defines any word in the Oxford English dictionary. For multiple definitions, use -m at the end of the command.")]
        public async Task Define([Remainder] string word)
        {
            var db = new BotBaseContext();
            var config = db.Configuration.AsQueryable().Where(cfg => cfg.Name == Program.configName).First();
            bool multiple = false;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://od-api.oxforddictionaries.com/api/v2/entries/en/");
            client.DefaultRequestHeaders.Add("app_id", config.DictId);
            client.DefaultRequestHeaders.Add("app_key", config.DictKey);
            if (word.EndsWith(" -m"))
            {
                word = word.Replace(" -m", "");
                multiple = true;
            }
            HttpResponseMessage response = await client.GetAsync(word);
            if (response.StatusCode.ToString() == "NotFound")
            {
                await Context.Channel.SendFileAsync("Media/damage.png");
                return;
            }

            if (response.StatusCode.ToString() == "Forbidden")
            {
                await ReplyAsync("Unable to retrieve definition");
                return;
            }
            HttpContent content = response.Content;
            string stuff = await content.ReadAsStringAsync();
            var json = JObject.Parse(stuff);
            if (multiple)
            {
                var embed = new EmbedBuilder();
                var color = db.Variable.AsQueryable().Where(V => V.Name == "embedColor").FirstOrDefault();
                embed.WithColor(new Color(Convert.ToUInt32(color.Value, 16)));
                string def = "1.) " + (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].definitions[0]");
                string example = (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].examples[0].text");
                if (example == null)
                {
                    example = "(no example available)";
                }
                embed.AddField(def, example);
                for (int i = 0; i < 4; i++)
                {
                    def = (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].subsenses[" + i.ToString() + "].definitions[0]");
                    example = (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].subsenses[" + i.ToString() + "].examples[0].text");
                    if (def != null)
                    {
                        def = (i + 2).ToString() + ".) " + def;
                        if (example == null)
                        {
                            example = "(no example available)";
                        }
                        embed.AddField(def, example);
                    }
                }
                embed.WithFooter("Made with love");
                embed.WithCurrentTimestamp();
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;
                string name = textInfo.ToTitleCase(word);
                var type = json.SelectToken("results[0].lexicalEntries[0].lexicalCategory.text");
                string spelling = (string)json.SelectToken("results[0].lexicalEntries[0].pronunciations[0].phoneticSpelling");
                string definition = (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].definitions[0]");
                string example = (string)json.SelectToken("results[0].lexicalEntries[0].entries[0].senses[0].examples[0].text");
                string message = name + " (" + type + ")";
                if (spelling != null)
                {
                    message += "     /" + spelling + "/";
                }
                message += "\n**Definition: **" + definition;
                if (example != null)
                {
                    message += "\n**Example: **" + example;
                }
                await ReplyAsync(message);
            }
        }

        [Command("udefine")]
        [Remarks("-c- term")]
        [Alias("slang")]
        [Summary("Gives the top definition for the term from urbandictionary.com")]
        public async Task DefineUrbanDictionary([Remainder] string phrase)
        {
            string URBAN_DICTIONARY_ENDPOINT = "http://api.urbandictionary.com/v0/define?term=";

            string encodedSearchTerm = HttpUtility.UrlEncode(phrase);
            List<UrbanDictionaryDefinition> definitionList = new List<UrbanDictionaryDefinition>();

            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync(URBAN_DICTIONARY_ENDPOINT + encodedSearchTerm))
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        UrbanDictionaryResult udefineResult = JsonConvert.DeserializeObject<UrbanDictionaryResult>(jsonResponse);
                        definitionList = udefineResult.List;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            if (definitionList.Count > 0)
            {
                // Urban Dictionary uses square brackets for links in its markup; they'll never appear as part of the definition text.
                var cleanDefinition = definitionList[0].Definition.Replace("[", "").Replace("]", "");
                var cleanExample = definitionList[0].Example.Replace("[", "").Replace("]", "");
                var year = definitionList[0].Written_On.Substring(0, definitionList[0].Written_On.IndexOf("-"));
                var dayMonth = definitionList[0].Written_On.Substring(definitionList[0].Written_On.IndexOf("-") + 1, 5);
                var cleanDate = dayMonth.Replace("-", "/") + "/" + year;
                var word = definitionList[0].Word;
                var msg = $"{word} - {cleanDate}\n**Definition:** {cleanDefinition}\n**Example:** {cleanExample}";

                while(msg.Length > 2000)
                {
                    var index = msg.Substring(0, 2000).LastIndexOf(" ");
                    await ReplyAsync(msg.Substring(0, index));
                    msg = msg.Substring(index, msg.Length - index);
                }

                await ReplyAsync(msg);
            }
            else
            {
                await ReplyAsync($"{phrase} is not an existing word/phrase");
            }

        }

        [Command("movie")]
        [Alias("imdb")]
        [Remarks("-c- airplane!")]
        [Summary("Provides information for a movie as specified by name.")]
        public async Task Movie([Remainder] string word)
        {
            var db = new BotBaseContext();
            var config = db.Configuration.AsQueryable().Where(cfg => cfg.Name == Program.configName).First();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://www.omdbapi.com");
            HttpResponseMessage response = await client.GetAsync($"?t={word}&plot=full&apikey={config.OmdbKey}");
            HttpContent content = response.Content;
            string stuff = await content.ReadAsStringAsync();
            var json = JObject.Parse(stuff);
            if ((string)json.SelectToken("Response") == "False")
            {
                await ReplyAsync("Movie not found");
                return;
            }
            var embed = new EmbedBuilder();
            var color = db.Variable.AsQueryable().Where(V => V.Name == "embedColor").FirstOrDefault();
            embed.WithColor(new Color(Convert.ToUInt32(color.Value, 16)));
            string rt = (string)json.SelectToken("Ratings[1].Value");
            string imdb = (string)json.SelectToken("Ratings[0].Value");
            string plot = (string)json.SelectToken("Plot");
            if (plot.Length > 1024)
            {
                int excess = plot.Length - 1024;
                plot = plot.Remove(plot.Length - excess - 3);
                plot += "...";
            }

            embed.Title = (string)json.SelectToken("Title");
            embed.Description = (string)json.SelectToken("Genre");
            if ((string)json.SelectToken("Poster") != "N/A")
                embed.ThumbnailUrl = (string)json.SelectToken("Poster");
            if (rt != null)
                embed.AddField($"Rotten Tomatoes: {rt}, IMDb: {imdb}", plot);
            else
                embed.AddField($"IMDb Rating: {imdb}", plot);
            embed.AddField("Released", (string)json.SelectToken("Released"), inline: true);
            embed.AddField("Run Time", (string)json.SelectToken("Runtime"), inline: true);
            embed.AddField("Rating", (string)json.SelectToken("Rated"), inline: true);
            embed.AddField("Starring", (string)json.SelectToken("Actors"));
            embed.AddField("Directed By", (string)json.SelectToken("Director"), inline: true);
            embed.WithUrl("https://www.imdb.com/title/" + (string)json.SelectToken("imdbID"));


            embed.WithFooter("Made with love");
            embed.WithCurrentTimestamp();
            await ReplyAsync("", false, embed.Build());
        }

        [Command("stats")]
        [Remarks("-c- region username")]
        [Summary("Gives the stats for a league player on any region. The region name is the abbreviated verson of the region name. Example: na = North America.")]
        public async Task Stats(string region, [Remainder] string name)
        {
            name = name.Replace(" ", string.Empty);

            string SearchText = "<meta name=\"description\" content=\"";
            string SearchText2 = "\"/>";

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string source = "";
            if (region == "kr")
            {
                if (await RemoteFileExists("http://www.op.gg/summoner/userName=" + name))
                    source = await client.GetStringAsync("http://www.op.gg/summoner/userName=" + name);
                else
                {
                    await ReplyAsync("That is not a valid summoner name / region");
                    return;
                }
            }
            else
            {
                if (await RemoteFileExists("http://" + region + ".op.gg/summoner/userName=" + name))
                    source = await client.GetStringAsync("http://" + region + ".op.gg/summoner/userName=" + name);
                else
                {
                    await ReplyAsync("That is not a valid summoner name / region");
                    return;
                }
            }
            if (source.IndexOf("This summoner is not registered at OP.GG. Please check spelling.") != -1)
            {
                await ReplyAsync("That Summoner does not exist");
                return;
            }
            else
            {
                var db = new BotBaseContext();
                var embed = new EmbedBuilder();
                var color = db.Variable.AsQueryable().Where(V => V.Name == "embedColor").FirstOrDefault();
                embed.WithColor(new Color(Convert.ToUInt32(color.Value, 16)));
                {
                    string kdsource = source.Remove(0, source.IndexOf("summoner-id=\"") + 13);
                    kdsource = kdsource.Remove(kdsource.IndexOf("\""));
                    if (region == "kr")
                        kdsource = "http://www." + "op.gg/summoner/champions/ajax/champions.most/summonerId=" + kdsource + "&season=11";
                    else
                        kdsource = "http://" + region + ".op.gg/summoner/champions/ajax/champions.most/summonerId=" + kdsource + "&season=11";
                    System.Net.Http.HttpClient client2 = new System.Net.Http.HttpClient();
                    kdsource = await client2.GetStringAsync(kdsource);
                    string url = source.Remove(0, source.IndexOf("ProfileIcon"));
                    url = url.Remove(0, url.IndexOf("<img src=\"//") + 12);
                    url = url.Remove(url.IndexOf("\""));
                    url = "http://" + url;
                    embed.ThumbnailUrl = url;
                    Int32 start = source.IndexOf(SearchText) + SearchText.Length;
                    source = source.Remove(0, start);
                    Int32 end = source.IndexOf(SearchText2);
                    source = source.Remove(end);

                    source = source.Replace("&#039;", "'");
                    if (source.IndexOf("Lv. ") == -1 && source.IndexOf("Unranked") == -1)
                    {
                        string def = "Information for: " + source.Remove(source.IndexOf("/")) + "\n";
                        source = source.Remove(0, source.IndexOf("/") + 1);
                        embed.Title = def;
                        def = "Current Ranking: " + source.Remove(source.IndexOf("/")) + "\n";
                        source = source.Remove(0, source.IndexOf("/") + 1);
                        def = def + "Win Record: " + source.Remove(source.IndexOf("Win")) + "  (";
                        source = source.Remove(0, source.IndexOf("o") + 1);
                        def = def + source.Remove(source.IndexOf("/")) + ")\n\nTop 5 Champions:\n";
                        source = source.Remove(0, source.IndexOf("/") + 1);
                        embed.Description = def;
                        for (int i = 0; i < 4; i++)
                        {
                            if (source.IndexOf(",") != -1)
                            {
                                def = source.Remove(source.IndexOf("Win")) + "(";
                                source = source.Remove(0, source.IndexOf("Win") + 9);
                                def = def + source.Remove(source.IndexOf(",")) + " )";
                                def = def.Remove(def.IndexOf("-")) + def.Remove(0, def.IndexOf("-")).PadRight(30, ' ');
                                source = source.Remove(0, source.IndexOf(",") + 1);
                                kdsource = kdsource.Remove(0, kdsource.IndexOf("span class=\"KDA") + 17);
                                def = def + "KDA: **" + kdsource.Remove(kdsource.IndexOf(":")) + "**     ( ";
                                kdsource = kdsource.Remove(0, kdsource.IndexOf("KDAEach"));
                                kdsource = kdsource.Remove(0, kdsource.IndexOf("Kill") + 6);
                                def = def + kdsource.Remove(kdsource.IndexOf("<"));
                                kdsource = kdsource.Remove(0, kdsource.IndexOf("Death") + 7);
                                def = def + " / " + kdsource.Remove(kdsource.IndexOf("<"));
                                kdsource = kdsource.Remove(0, kdsource.IndexOf("Assist") + 8);
                                def = def + " / " + kdsource.Remove(kdsource.IndexOf("<")) + " )";
                                embed.AddField(def.Remove(def.IndexOf("-")), def.Remove(0, def.IndexOf("-") + 1));

                            }
                        }
                        def = source.Remove(source.IndexOf("Win")) + "  (";
                        source = source.Remove(0, source.IndexOf("Win") + 9);
                        def = def + source + " )";
                        def = def.Remove(def.IndexOf("-")) + def.Remove(0, def.IndexOf("-")).PadRight(30, ' ');
                        kdsource = kdsource.Remove(0, kdsource.IndexOf("span class=\"KDA") + 17);
                        def = def + "KDA: **" + kdsource.Remove(kdsource.IndexOf(":")) + "**     ( ";
                        kdsource = kdsource.Remove(0, kdsource.IndexOf("KDAEach"));
                        kdsource = kdsource.Remove(0, kdsource.IndexOf("Kill") + 6);
                        def = def + kdsource.Remove(kdsource.IndexOf("<"));
                        kdsource = kdsource.Remove(0, kdsource.IndexOf("Death") + 7);
                        def = def + " / " + kdsource.Remove(kdsource.IndexOf("<"));
                        kdsource = kdsource.Remove(0, kdsource.IndexOf("Assist") + 8);
                        def = def + " / " + kdsource.Remove(kdsource.IndexOf("<")) + " )";
                        embed.AddField(def.Remove(def.IndexOf("-")), def.Remove(0, def.IndexOf("-") + 1));
                    }
                    else
                    {
                        await ReplyAsync("That Summoner has not been placed yet this season");
                        return;
                    }
                }
                embed.WithFooter("Made with love");
                embed.WithCurrentTimestamp();
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("mastery")]
        [Remarks("-c- region username")]
        [Summary("Gives the number of mastery points for the top 10 most played champions for a user on any server.")]
        public async Task Mastery(string region, [Remainder] string name)
        {
            var db = new BotBaseContext();
            var embed = new EmbedBuilder();
            var color = db.Variable.AsQueryable().Where(V => V.Name == "embedColor").FirstOrDefault();
            embed.WithColor(new Color(Convert.ToUInt32(color.Value, 16)));
            {
                name = name.Replace(" ", string.Empty);
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                string html = "";
                try
                {
                    html = await client.GetStringAsync("https://championmasterylookup.derpthemeus.com/summoner?summoner=" + name + "&region=" + region.ToUpper());
                }
                catch
                {
                    await ReplyAsync("That summoner does not exist");
                    return;
                }
                html = html.Remove(0, html.IndexOf("/img/profile"));
                embed.ThumbnailUrl = "https://championmasterylookup.derpthemeus.com" + html.Remove(html.IndexOf("\""));
                html = html.Remove(0, html.IndexOf("userName=") + 9);
                embed.Title = "Top ten mastery scores for " + (html.Remove(html.IndexOf("\"")).Replace("%20", " "));
                string champ = "";
                string nums = "";
                int count = 0;
                for (int i = 1; i <= 10; i++)
                {
                    if (html.IndexOf("/champion?") == html.IndexOf("/champion?champion=-1"))
                        break;
                    html = html.Remove(0, html.IndexOf("/champion?"));
                    html = html.Remove(0, html.IndexOf(">") + 1);
                    champ = html.Remove(html.IndexOf("<"));
                    champ = champ.Replace("&#x27;", "'");
                    html = html.Remove(0, html.IndexOf("\"") + 1);
                    nums = html.Remove(html.IndexOf("\""));
                    count = count + Convert.ToInt32(nums);
                    for (int j = nums.Length - 3; j > 0; j = j - 3)
                        nums = nums.Remove(j) + "," + nums.Remove(0, j);

                    embed.AddField(i + ". " + champ, nums + " points", inline: true);
                }

                nums = Convert.ToString(count);
                for (int j = nums.Length - 3; j > 0; j = j - 3)
                    nums = nums.Remove(j) + "," + nums.Remove(0, j);
                embed.Description = "Total score across top ten: " + nums;

                embed.WithFooter("Made with love");
                embed.WithCurrentTimestamp();
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("info")]
        [Remarks("-c-, -c- @person1 @person2, -c- person1id person2id")]
        [Summary("Gets varying pieces of Discord information for one or more users. Mention a user or provide their id to get their information, or do neither to get your own. To do more than 1 person, separate mentions/ids with spaces.")]
        public async Task MyInfo([Remainder] string ids = "")
        {
            var mention = Context.Message.MentionedUserIds;
            if (mention.Count != 0)
            {
                foreach (ulong id in mention)
                {
                    var embed = ConstructEmbedInfo(Context.Guild.GetUserAsync(id).Result);
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else if (ids != "")
            {
                string[] idList = ids.Split(' ');
                foreach (string id in idList)
                {
                    var embed = ConstructEmbedInfo(await Context.Guild.GetUserAsync(Convert.ToUInt64(id)));
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                var embed = ConstructEmbedInfo(await Context.Guild.GetUserAsync(Context.User.Id));
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("avatar")]
        [Remarks("-c-, -c- @person1 @person2, -c- person1id person2id")]
        [Summary("Gets the avatar for one or more users. Mention a user or provide their id to get their avatar, or do neither to get your own. To do more than 1 person, separate mentions/ids with spaces.")]
        public async Task Avatar([Remainder] string ids = "")
        {
            var mention = Context.Message.MentionedUserIds;
            if (mention.Count != 0)
            {
                foreach (ulong id in mention)
                {
                    var embed = new EmbedBuilder();
                    IGuildUser user = Context.Guild.GetUserAsync(id).Result;
                    string url = user.GetAvatarUrl();
                    url = url.Remove(url.IndexOf("?size=128"));
                    url = url + "?size=256";
                    embed.ImageUrl = url;
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else if (ids != "")
            {
                string[] idList = ids.Split(' ');
                foreach (string id in idList)
                {
                    var embed = new EmbedBuilder();
                    IGuildUser user = await Context.Guild.GetUserAsync(Convert.ToUInt64(id));
                    string url = user.GetAvatarUrl();
                    url = url.Remove(url.IndexOf("?size=128"));
                    url = url + "?size=256";
                    embed.ImageUrl = url;
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                var embed = new EmbedBuilder();
                string url = Context.User.GetAvatarUrl();
                url = url.Remove(url.IndexOf("?size=128"));
                url = url + "?size=256";
                embed.ImageUrl = url;
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("beefact")]
        [Remarks("-c-")]
        [Alias("beefacts", "bee", "bees")]
        [Summary("Provides a fact about bees.")]
        public async Task beeFact([Remainder] string useless = "")
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string fact = await client.GetStringAsync("http://thebuzz.writeonlymedia.com/api");
            await ReplyAsync(fact.TrimStart('\"').TrimEnd('\"'));
        }

        async Task<bool> RemoteFileExists(string url)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            try
            {
                string response = await client.GetStringAsync(url);
                if (response.Length == 0) return false;
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public string FormatTime(DateTimeOffset orig)
        {
            string str = "";
            str = str + orig.LocalDateTime.DayOfWeek + ", ";
            str = str + orig.LocalDateTime.Month + "/" + orig.LocalDateTime.Day + "/" + orig.LocalDateTime.Year;
            str = str + " at " + orig.LocalDateTime.Hour + ":" + orig.LocalDateTime.Minute + " CST";
            return str;
        }

        public EmbedBuilder ConstructEmbedInfo(IGuildUser user)
        {
            var db = new BotBaseContext();
            var embed = new EmbedBuilder();
            var color = db.Variable.AsQueryable().Where(V => V.Name == "embedColor").FirstOrDefault();
            embed.WithColor(new Color(Convert.ToUInt32(color.Value, 16)));
            embed.WithAuthor(user.Username + "#" + user.Discriminator, user.GetAvatarUrl());
            embed.ThumbnailUrl = user.GetAvatarUrl();
            embed.AddField("User ID", user.Id);
            if (user.Nickname == null)
                embed.AddField("Nickname", user.Username);
            else
                embed.AddField("Nickname", user.Nickname);
            if (user.Activity == null)
                embed.AddField("Currently Playing", "[nothing]");
            else
                embed.AddField("Currently " + user.Activity.Type.ToString(), user.Activity.Name);
            embed.AddField("Account Creation Date", FormatTime(user.CreatedAt));
            embed.AddField("Server Join Date", FormatTime(user.JoinedAt.Value));
            string roles = "";
            foreach (ulong id in user.RoleIds)
            {
                if (roles != "")
                    roles = roles + ", ";
                if (Context.Guild.GetRole(id).Name != "@everyone")
                    roles = roles + Context.Guild.GetRole(id).Name;
            }
            embed.AddField("Roles", roles);
            embed.WithFooter("Made with love");

            embed.WithCurrentTimestamp();
            return embed;
        }
    }
}