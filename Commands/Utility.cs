﻿using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using JifBot.Models;
using JIfBot;

namespace JifBot.Commands
{
    public class Utility : ModuleBase
    {
        List<Quote> quoteList = new List<Quote>();

        [Command("invitelink")]
        [Remarks("-c-")]
        [Summary("Provides a link which can be used should you want to spread Jif Bot to another server.")]
        public async Task InviteLink()
        {
            await ReplyAsync("The following is a link to add me to another server. NOTE: You must have permissions on the server in order to add. Once on the server I must be given permission to send and delete messages, otherwise I will not work.\nhttps://discordapp.com/oauth2/authorize?client_id=315569278101225483&scope=bot");
        }

        [Command("bigtext")]
        [Remarks("-c- phrase, -c- phrase -d")]
        [Summary("Takes the user input for messages and turns it into large letters using emotes. If you end your command call with -d, it will delete your message calling the bot.")]
        public async Task bigtext([Remainder]string orig)
        {
            if (orig.EndsWith("-d"))
            {
                orig = orig.Remove(orig.Length - 2);
                await Context.Message.DeleteAsync();
            }
            string final = "";
            orig = orig.ToLower();
            for (int i = 0; i < orig.Length; i++)
            {
                final += getBig(orig[i]);
                final += " ";
            }
            if (final.Length > 2000)
                await ReplyAsync("This command does not support messages of that length, please shorten your message.");
            else
                await ReplyAsync(final);
        }

        [Command("tinytext")]
        [Alias("smalltext")]
        [Remarks("-c- phrase, -c- phrase -d")]
        [Summary("Takes the user input for messages and turns it into small letters. If you end your command call with -d, it will delete your message calling the bot.")]
        public async Task tinytext([Remainder]string orig)
        {
            if (orig.EndsWith("-d"))
            {
                orig = orig.Remove(orig.Length - 2);
                await Context.Message.DeleteAsync();
            }
            string final = "";
            orig = orig.ToLower();
            for (int i = 0; i < orig.Length; i++)
            {
                final += getSmol(orig[i]);
            }
            if (final.Length > 2000)
                await ReplyAsync("This command does not support messages of that length, please shorten your message.");
            else
                await ReplyAsync(final);
        }

        [Command("widetext")]
        [Remarks("-c- phrase, -c- phrase -d")]
        [Summary("Takes the user input for messages and turns it into a ＷＩＤＥ  ＢＯＩ. If you end your command call with -d, it will delete your message calling the bot.")]
        public async Task WideText([Remainder] string message)
        {
            if (message.EndsWith("-d"))
            {
                message = message.Remove(message.Length - 2);
                await Context.Message.DeleteAsync();
            }
            message = message.Replace(" ", "   ");
            string alpha = "QWERTYUIOPASDFGHJKLÇZXCVBNMqwertyuiopasdfghjklçzxcvbnm,.-~+´«'0987654321!\"#$%&/()=?»*`^_:;";
            string fullwidth = "ＱＷＥＲＴＹＵＩＯＰＡＳＤＦＧＨＪＫＬÇＺＸＣＶＢＮＭｑｗｅｒｔｙｕｉｏｐａｓｄｆｇｈｊｋｌçｚｘｃｖｂｎｍ,.－~ ´«＇０９８７６５４３２１！＂＃＄％＆／（）＝？»＊`＾＿：；";

            for (int i = 0; i < alpha.Length; i++)
            {
                message = message.Replace(alpha[i], fullwidth[i]);
            }
            await ReplyAsync(message);
        }

        [Command("owo")]
        [Alias("uwu")]
        [Remarks("-c- phrase, -c- phrase -d")]
        [Summary("Takes the user input, and translates it into degenerate owo speak. If you end your command call with -d, it will delete your message calling the bot..")]
        public async Task Owo([Remainder] string message)
        {
            if (message.EndsWith("-d"))
            {
                message = message.Remove(message.Length - 2);
                await Context.Message.DeleteAsync();
            }
            string[] faces = new string[] { "(・ω・)", ";;w;;", "owo", "UwU", ">w<", "^w^" };
            Random rnd = new Random();
            message = Regex.Replace(message, @"(?:r|l)", "w");
            message = Regex.Replace(message, @"(?:R|L)", "W");
            message = Regex.Replace(message, @"n([aeiou])", @"ny$1");
            message = Regex.Replace(message, @"N([aeiou])", @"Ny$1");
            message = Regex.Replace(message, @"N([AEIOU])", @"NY$1");
            message = Regex.Replace(message, @"ove", @"uv");
            message = Regex.Replace(message, @"\!+", (match) => string.Format("{0}", " " + faces[rnd.Next(faces.Length)] + " "));

            await ReplyAsync(message);
        }

        [Command("timer")]
        [Remarks("-c- -h2 -m30 message, -c- 150 message")]
        [Summary("Sets a reminder to ping you after a certain amount of time has passed. A message can be specified along with the time to be printed back to you at the end of the timer. Times can be specified using any combination of -m[minutes], -h[hours], -d[days], and -w[weeks] anywhere in the message. Additionally, to set a quick timer for a number of minutes, just do -p-timer [minutes] message.")]
        public async Task Timer([Remainder]string message = "")
        {
            int waitTime = 0;

            if (Regex.IsMatch(message, @"-m *[0-9]+"))
                waitTime += Convert.ToInt32(Regex.Match(message, @"-m *[0-9]+").Value.Replace("-m", ""));

            if (Regex.IsMatch(message, @"-h *[0-9]+"))
            {
                waitTime += (Convert.ToInt32(Regex.Match(message, @"-h *[0-9]+").Value.Replace("-h", "")) * 60);
            }

            if (Regex.IsMatch(message, @"-d *[0-9]+"))
            {
                waitTime += (Convert.ToInt32(Regex.Match(message, @"-d *[0-9]+").Value.Replace("-d", "")) * 1440);
            }

            if (Regex.IsMatch(message, @"-w *[0-9]+"))
            {
                waitTime += (Convert.ToInt32(Regex.Match(message, @"-w *[0-9]+").Value.Replace("-w", "")) * 10080);
            }

            if (waitTime == 0)
            {
                if (Regex.IsMatch(message, @" *[0-9]+"))
                    waitTime = Convert.ToInt32(message.Split(" ")[0]);
                else
                {
                    var db = new BotBaseContext();
                    var config = db.Configuration.AsQueryable().Where(cfg => cfg.Name == Program.configName).First();
                    await ReplyAsync($"Please provide an amount of time to wait for. For assistance, use {config.Prefix}help.");
                    return;
                }
            }

            message = Regex.Replace(message, @"-[m,h,d] *[0-9]+", "");
            if (message.Replace(" ", "") == "")
                message = "Times up!";
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "../../../Scripts/sendmessage.sh " + Context.Channel.Id + " \"" + Context.User.Mention + " " + message + "\" " + waitTime;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            await ReplyAsync("Setting timer for " + formatMinutesToString(waitTime) + " from now.");
        }

        [Command("choose")]
        [Remarks("-c- choice \"choice but with spaces\"")]
        [Summary("Randomly makes a choice for you. You can use as many choices as you want, but seperate all choices using a space. If you wish for a choice to contain spaces, surround the choice with \"\"\n")]
        public async Task Choose([Remainder]string message)
        {
            int quotes = message.Split('\"').Length - 1;
            if (quotes % 2 != 0)
            {
                await ReplyAsync("please ensure all quotations are closed");
                return;
            }

            List<string> choices = new List<string>();
            int count = 0;
            message = message.TrimEnd();
            while (true)
            {
                message = message.TrimStart();
                string choice;
                if (message == "")
                {
                    break;
                }
                if (message[0] == '\"')
                {
                    message = message.Remove(0, 1);
                    choice = message.Remove(message.IndexOf("\""));
                    message = message.Remove(0, message.IndexOf("\"") + 1);
                }
                else
                {
                    if (message.Contains(" "))
                    {
                        choice = message.Remove(message.IndexOf(" "));
                        message = message.Remove(0, message.IndexOf(" "));
                    }
                    else
                    {
                        choice = message;
                        message = "";
                    }
                }
                choices.Add(choice);
                count++;
            }

            if (count < 2)
            {
                await ReplyAsync("Please provide at least two choices.");
                return;
            }

            Random rnd = new Random();
            int num = rnd.Next(count);
            await ReplyAsync("The robot overlords have chosen: **" + choices[num] + "**");
        }

        [Command("youtube")]
        [Remarks("-c- video title")]
        [Summary("Takes whatever you give it and searches for it on YouTube, it will return the first search result that appears.")]
        public async Task Youtube([Remainder]string vid)
        {
            vid = "https://www.youtube.com/results?search_query=" + vid.Replace(" ", "+");
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string html = await client.GetStringAsync(vid);
            html = html.Remove(0, html.IndexOf("?v=") + 3);
            html = html.Remove(html.IndexOf("\""));
            await ReplyAsync("https://www.youtube.com/watch?v=" + html);
        }

        [Command("mock")]
        [Remarks("-c-, -c- message, -c- message -d")]
        [Summary("Mocks the text you provide it. If you end your command call with -d, it will delete your message calling the bot. If you do not specify any message, it will mock the most recent message sent in the text channel, and delete your command call.")]
        public async Task Mock([Remainder] string words = "")
        {
            if (words == "")
            {
                await Context.Message.DeleteAsync();
                var msg = Context.Channel.GetMessagesAsync(1).FlattenAsync().Result;
                words = msg.ElementAt(0).Content;
            }
            else if (words.EndsWith("-d"))
            {
                words = words.Remove(words.Length - 2);
                await Context.Message.DeleteAsync();
            }
            string end = string.Empty;
            int i = 0;
            foreach (char c in words)
            {
                if (c == ' ' || c == '"' || c == '.' || c == ',')
                    end += c;
                else if (i == 0)
                {
                    char temp = Char.ToLower(c);
                    end += temp;
                    i = 1;
                }
                else if (i == 1)
                {
                    char temp = Char.ToUpper(c);
                    end += temp;
                    i = 0;
                }
            }
            await ReplyAsync(end);
        }

        [Command("8ball")]
        [Remarks("-c-")]
        [Summary("asks the magic 8 ball a question.")]
        public async Task eightBall([Remainder] string useless = "")
        {
            string[] responses = new string[] { "it is certain", "It is decidedly so", "Without a doubt", "Yes definitely", "You may rely on it", "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes", "Reply hazy try again", "Ask again later", "Better not tell you now", "Cannot predict now", "Concentrate and ask again", "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good", "Very doubtful" };
            Random rnd = new Random();
            int num = rnd.Next(20);
            await ReplyAsync(responses[num]);
        }

        [Command("s8ball")]
        [Remarks("-c-")]
        [Summary("asks the sassy 8 ball a question.")]
        public async Task SeightBall([Remainder] string useless = "")
        {
            string[] responses = new string[] { "Fuck yeah.", "Sure, why not?", "Well, duh.", "Do bears shit in the woods?", "Is water wet?", "I mean, I guess.", "If it gets you to fuck off, then sure.", "011110010110010101110011", "Whatever floats your boat.", "Fine, sure, whatever.", "Fuck you.", "Why do you feel the need to ask a BOT for validation?", "Figure it out yourself.", "Does it really matter?", "Leave me alone.", "Fuck no.", "Why would you even consider that a possibility?", "It's cute you think that could happen.", "Not a chance shitlord.", "Not in a million years." };
            Random rnd = new Random();
            int num = rnd.Next(20);
            await ReplyAsync(responses[num]);
        }

        [Command("tiltycat")]
        [Remarks("-c- degree")]
        [Summary("Creates a cat at any angle you specify.\nSpecial thanks to Erik (Assisting#8734) for writing the program. Accessed via ```http://www.writeonlymedia.com/tilty_cat/(degree).png``` where (degree) is the desired angle.")]
        public async Task TiltyCat(int degree, [Remainder] string useless = "")
        {
            string temp = "http://www.writeonlymedia.com/tilty_cat/" + degree + ".png";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(temp), "tiltycat.png");
            }
            await Context.Channel.SendFileAsync("tiltycat.png");
        }

        [Command("tiltydog")]
        [Remarks("-c- degree")]
        [Summary("Creates a dog at any angle you specify.\nSpecial thanks to Erik (Assisting#8734) for writing the program. Accessed via ```http://www.writeonlymedia.com/tilty_dog/(degree).png``` where (degree) is the desired angle.")]
        public async Task TiltyDat(int degree, [Remainder] string useless = "")
        {
            string temp = "http://www.writeonlymedia.com/tilty_dog/" + degree + ".png";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(temp), "tiltydog.png");
            }
            await Context.Channel.SendFileAsync("tiltydog.png");
        }

        [Command("joke")]
        [Remarks("-c-")]
        [Summary("Tells a joke.")]
        public async Task Joke()
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string source = await client.GetStringAsync("http://www.rinkworks.com/jokes/random.cgi");
            string ptr = "< div class='content'>";
            source = source.Remove(0, source.IndexOf(ptr) + ptr.Length);
            ptr = "</h2>";
            source = source.Remove(0, source.IndexOf(ptr) + ptr.Length);
            ptr = "</td><td class='ad'>";
            source = source.Remove(source.IndexOf(ptr));
            source = source.Replace("<p>", string.Empty);
            source = source.Replace("</p>", "\n");
            source = source.Replace("<ul>", "\n");
            source = source.Replace("<li>", "\n");
            source = source.Replace("</ul>", "\n");
            source = source.Replace("</li>", "\n");
            await ReplyAsync(source);
        }


        [Command("inspire")]
        [Remarks("-c-")]
        [Summary("Gives an inspirational quote.")]
        public async Task Inspire()
        {
            if (quoteList.Count == 0)
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync("https://type.fit/api/quotes"))
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        jsonResponse = jsonResponse.Replace("[", "{\"list\":[");
                        jsonResponse = jsonResponse.Replace("]", "]}");
                        try
                        {
                            QuoteResult result = JsonConvert.DeserializeObject<QuoteResult>(jsonResponse);
                            quoteList = result.List;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
            int count = quoteList.Count;
            Random rnd = new Random();
            int num = rnd.Next(count);
            Quote quote = quoteList[num];
            await ReplyAsync("\"" + quote.text + "\"\n-" + quote.author);


        }

        public string getSmol(char orig)
        {
            switch (orig)
            {
                case 'e':
                    return "ᵉ";
                case 't':
                    return "ᵗ";
                case 'a':
                    return "ᵃ";
                case 'o':
                    return "ᵒ";
                case 'i':
                    return "ᶦ";
                case 'n':
                    return "ᶰ";
                case 's':
                    return "ˢ";
                case 'r':
                    return "ʳ";
                case 'h':
                    return "ʰ";
                case 'd':
                    return "ᵈ";
                case 'l':
                    return "ᶫ";
                case 'u':
                    return "ᵘ";
                case 'c':
                    return "ᶜ";
                case 'm':
                    return "ᵐ";
                case 'f':
                    return "ᶠ";
                case 'y':
                    return "ʸ";
                case 'w':
                    return "ʷ";
                case 'g':
                    return "ᵍ";
                case 'p':
                    return "ᵖ";
                case 'b':
                    return "ᵇ";
                case 'v':
                    return "ᵛ";
                case 'k':
                    return "ᵏ";
                case 'x':
                    return "ˣ";
                case 'q':
                    return "ᑫ";
                case 'j':
                    return "ʲ";
                case 'z':
                    return "ᶻ";
            }
            return Convert.ToString(orig);
        }

        public string getBig(char orig)
        {
            switch (orig)
            {
                case 'e':
                    return "🇪";
                case 't':
                    return "🇹";
                case 'a':
                    return "🇦";
                case 'o':
                    return "🇴";
                case 'i':
                    return "🇮";
                case 'n':
                    return "🇳";
                case 's':
                    return "🇸";
                case 'r':
                    return "🇷";
                case 'h':
                    return "🇭";
                case 'd':
                    return "🇩";
                case 'l':
                    return "🇱";
                case 'u':
                    return "🇺";
                case 'c':
                    return "🇨";
                case 'm':
                    return "🇲";
                case 'f':
                    return "🇫";
                case 'y':
                    return "🇾";
                case 'w':
                    return "🇼";
                case 'g':
                    return "🇬";
                case 'p':
                    return "🇵";
                case 'b':
                    return "🅱";
                case 'v':
                    return "🇻";
                case 'k':
                    return "🇰";
                case 'x':
                    return "🇽";
                case 'q':
                    return "🇶";
                case 'j':
                    return "🇯";
                case 'z':
                    return "🇿";
                case ' ':
                    return "  ";
            }
            return Convert.ToString(orig);
        }
        string formatMinutesToString(int minutes)
        {
            string format = "";

            if (minutes / 10080 > 0)
            {
                format += Convert.ToString(minutes / 10080) + " week";
                if (minutes / 10080 > 1)
                    format += "s";
                format += ", ";
                minutes = minutes % 10080;
            }

            if (minutes / 1440 > 0)
            {
                format += Convert.ToString(minutes / 1440) + " day";
                if (minutes / 1440 > 1)
                    format += "s";
                format += ", ";
                minutes = minutes % 1440;
            }

            if (minutes / 60 > 0)
            {
                format += Convert.ToString(minutes / 60) + " hour";
                if (minutes / 60 > 1)
                    format += "s";
                format += ", ";
                minutes = minutes % 60;
            }

            if (minutes > 0)
            {
                format += Convert.ToString(minutes) + " minute";
                if (minutes > 1)
                    format += "s";
            }
            else
                format = format.Remove(format.Length - 2, 2);

            return format;
        }
    }
    class UrbanDictionaryDefinition
    {
        public string Definition { get; set; }
        public string Example { get; set; }
        public string Word { get; set; }
        public string Written_On { get; set; }
    }

    class UrbanDictionaryResult
    {
        public List<UrbanDictionaryDefinition> List { get; set; }
    }

    class Quote
    {
        public string text { get; set; }
        public string author { get; set; }
    }

    class QuoteResult
    {
        public List<Quote> List { get; set; }
    }
}