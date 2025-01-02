using Microsoft.Extensions.Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace GrilledCheese.Discord
{
    public class DiscordRelay
    {
        public Dictionary<ulong, Shard> Shards;
        private DateTime lastMemberListRefresh = DateTime.MinValue;

        public async Task<bool> Setup(IConfiguration config, GameProcessor processor)
        {
            Shards = new Dictionary<ulong, Shard>();

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = config["DiscordToken"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discord.MessageCreated += async (s, e) =>
            {
                string messageBody = e.Message.Content;
                string resultMessage = string.Empty;

                if (!(messageBody.StartsWith("!") || (messageBody.StartsWith('_') && messageBody.EndsWith('_'))))
                {
                    return;
                }

                Shard activeShard;
                if (!Shards.TryGetValue(e.Guild.Id, out activeShard))
                {
                    // this is a new discord server using the bot, we need to initialize it
                    Shards.Add(e.Guild.Id, new Shard(e.Guild.Id.ToString()));
                    activeShard = Shards[e.Guild.Id];
                }

                if (DateTime.Now - lastMemberListRefresh > TimeSpan.FromHours(1))
                {
                    // refresh the member list once per hour
                    var guildMembers = await e.Guild.GetAllMembersAsync();
                    foreach (var guildMember in guildMembers)
                    {
                        string friendlyName = ExtractFriendlyName(guildMember);
                        var existingPlayer = activeShard.GetPlayer(friendlyName);
                        Console.WriteLine($"Adding player {friendlyName}");
                        var state = activeShard.ActivatePlayer(friendlyName, friendlyName);
                    }

                    lastMemberListRefresh = DateTime.Now;
                }
                
                var actorGuildMember = await e.Guild.GetMemberAsync(e.Author.Id);
                var actor = activeShard.GetPlayer(ExtractFriendlyName(actorGuildMember));
                if (actor == null)
                {
                    // create the player
                    var actorMemberObject = await e.Guild.GetMemberAsync(e.Author.Id);
                    actor = activeShard.ActivatePlayer(actorMemberObject.Nickname ?? actorMemberObject.DisplayName ?? actorMemberObject.Username, actorMemberObject.Id.ToString());
                }

                string response = processor.ExecuteCommand(actor.FriendlyName, messageBody, activeShard);

                await e.Message.RespondAsync(response);
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);

            return true;
        }

        private static string ExtractFriendlyName(DiscordMember guildMember)
        {
            return guildMember.Nickname ?? guildMember.DisplayName ?? guildMember.Username;
        }
    }
}