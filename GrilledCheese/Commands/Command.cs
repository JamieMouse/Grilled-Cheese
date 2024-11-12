using GrilledCheese;

/// <summary>
/// A command has the definition: CommandName {Target} prop1... propn with spaces separating properties
/// </summary>
public abstract class Command : ICommand
{
    public string Definition;
    public string Description;
    public int energyCost;

    public Command(string definition, int energy = 0, string Description = "")
    {
        Definition = definition;
        energyCost = energy;
        this.Description = Description;
    }

    public string Print()
    {
        return string.Join(' ', Definition, $"Costs {energyCost} energy.", Description);
    }

    public abstract string Execute(string actor, string[] args, Shard shard);

    public abstract bool ValidateArgs(string[] args);
}


public interface ICommand
{
    public string Execute(string actor, string[] args, Shard shard);

    public bool ValidateArgs(string[] args);
}