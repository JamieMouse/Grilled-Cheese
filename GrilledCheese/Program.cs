using Microsoft.Extensions.Configuration;
using GrilledCheese.FChat;
using GrilledCheese.Discord;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("config.json", true)
    .Build();

if (config == null)
{
    throw new Exception("Configuration not provided");
}

CheeseRandom.Initialize();

GameProcessor gameProcessor = new GameProcessor(config);

DiscordRelay discord = new DiscordRelay();
var discordTask = discord.Setup(config, gameProcessor);

if (true)
{
    FChatRelay fChat = new FChatRelay();
    await fChat.Setup(config, gameProcessor);

    // give the f-chat client a while to connect
    await Task.Delay(10000);

    // Periodically check if the f-chat client has disconnected
    while (fChat.loggedIn)
    {
        await Task.Delay(1000);
    }
}

await discordTask;

