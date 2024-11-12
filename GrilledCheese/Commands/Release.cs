using GrilledCheese;

public class Release : Command
{
    /// <summary>
    /// Costs energy
    /// Make attempt to escape when dominated
    /// </summary>
    public Release() : base("release {target}", 0, "Release a dominated target, freeing them to participate in the lobby.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        PlayerState actorState = shard.GetPlayer(actor);
        if (!actorState.Active)
        {
            shard.ActivatePlayer(actorState.FriendlyName, actorState.Id);
        }

        PlayerState targetState = shard.GetPlayer(args[1]);

        if (targetState == null || !targetState.Active)
        {
            // the target isn't active, leave them alone
            return $"{args[1]} is currently inactive";
        }

        targetState.Location = actorState.Location;
        targetState.Dom = default;

        return $"{targetState.FriendlyName} is freed.";
    }

    public override bool ValidateArgs(string[] args)
    {
        if (args.Count() < 2)
        {
            return false;
        }

        return true;
    }
}