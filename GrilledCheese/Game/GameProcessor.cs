using System.Text;
using GrilledCheese;
using Microsoft.Extensions.Configuration;

public class GameProcessor
{
    Dictionary<string, Command> commandDictionary;

    public GameProcessor(IConfiguration config)
    {
        commandDictionary = new Dictionary<string, Command>()
        {
            { nameof(Echo).ToLower(), new Echo() },
            { nameof(SetAlias).ToLower(), new SetAlias() },
            { nameof(Inspect).ToLower(), new Inspect() },
            { nameof(Rest).ToLower(), new Rest() },
            { nameof(Seduce).ToLower(), new Seduce() },
            { nameof(Dominate).ToLower(), new Dominate() },
            { nameof(Struggle).ToLower(), new Struggle() },
            { nameof(Spin).ToLower(), new Spin() },
            { nameof(Roll).ToLower(), new Roll() },
            { nameof(Release).ToLower(), new Release() },
            { nameof(PrintAlias).ToLower(), new PrintAlias() },
        };
    }

    public string ExecuteCommand(string actorId, string fullSubmission, Shard shard)
    {
        StringBuilder response = new StringBuilder();

        if (fullSubmission.StartsWith("/me") || (fullSubmission.StartsWith('_') && fullSubmission.EndsWith('_')))
        {
            // This is a post not a command, reward the user with some energy
            PlayerState player = shard.GetPlayer(actorId);
            shard.ActivatePlayer(player.FriendlyName, player.Id);
            if (DateTime.UtcNow - player.LastEnergy > shard.EnergyGainLimit && fullSubmission.Split(" ").Count() > shard.postLength)
            {
                player.LastEnergy = DateTime.UtcNow;
                player.IncrementEnergy(1);
            }
            return "";
        }

        // Commands always begin with !, we can remove it now
        fullSubmission = fullSubmission.TrimStart('!');
        
        if (fullSubmission == string.Empty)
        {
            return string.Empty;
        }

        // The shape of a command is {command} {arguments}
        // Where arguments often contain targets as well as a thing to do
        var argGroup = fullSubmission.Split(' ');
        if (!argGroup[0].Equals(nameof(SetAlias), StringComparison.InvariantCultureIgnoreCase))
        {
            argGroup = argGroup.Select(x => shard.TranslateAlias(x)).ToArray();
        }

        string command = argGroup[0];

        Console.WriteLine($"{command} executing");

        if (command.ToLower() == "help")
        {
            PrintCommandDictionary(response);
            return response.ToString();
        }

        if (!commandDictionary.ContainsKey(command))
        {
            response.Append("Command not recognized");
            return response.ToString();
        }

        Command executingCommand = commandDictionary[command];

        if (!executingCommand.ValidateArgs(argGroup))
        {
            response.AppendLine("Command not formatted correctly");
            response.AppendLine(executingCommand.Definition);
            return response.ToString();
        }

        response.Append(executingCommand.Execute(actorId, argGroup, shard));

        return response.ToString();
    }

    public void PrintCommandDictionary(StringBuilder response)
    {
        foreach (Command command in commandDictionary.Values)
        {
            response.AppendLine(command.Print());
        }
    }
}