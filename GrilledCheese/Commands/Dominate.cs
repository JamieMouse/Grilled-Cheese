using GrilledCheese;

public class Dominate : Command
{
    public Dominate() : base("dominate {target}", 1, "Attempt to overwhelm a target, taking control or otherwise 'defeating' them.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        // identify the target
        PlayerState targetState = shard.GetPlayer(args[1]);

        if (targetState == null || !targetState.Active)
        {
            // the target isn't active, leave them alone
            return $"{args[1]} is currently inactive";
        }

        // identify the actor
        PlayerState actorState = shard.GetPlayer(actor);
        if (!actorState.Active)
        {
            shard.ActivatePlayer(actorState.FriendlyName, actorState.Id);
        }

        if (targetState.Location != actorState.Location)
        {
            return $"{targetState.FriendlyName} is out of reach.";
        }

        if (actorState.Energy < energyCost)
        {
            return $"{actorState.FriendlyName} isn't ready to act.";
        }

        // This action will cost energy from the actor
        actorState.IncrementEnergy(energyCost*-1);

        // roll dice
        //{resolve}d20k1
        int actorRoll = 0;
        for (int i = 0; i < actorState.Resolve; i++)
        {
            actorRoll = Math.Max(actorRoll, CheeseRandom.Roll(20));
        }

        int targetRoll = 0;
        for (int i = 0; i < targetState.Resolve; i++)
        {
            targetRoll = Math.Max(targetRoll, CheeseRandom.Roll(20));
        }

        if (actorRoll >= targetRoll)
        {
            // The action is successful, reduce the targets resolve
            targetState.Location = actor;
            return $"{targetState.FriendlyName} is dominated by {actorState.FriendlyName}. {actorRoll} {targetRoll}";
        }
        else
        {
            actorState.IncrementResolve(-1);
            return $"{targetState.FriendlyName} resists. {actorState.FriendlyName}'s resolve is weakened. {actorRoll} {targetRoll}";
        }
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