using GrilledCheese;

public class Seduce : Command
{
    public Seduce() : base("seduce {target}", 1, "Weaken a targets resolve, making them more vulnerable.")
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

        if (actorState.Energy < energyCost)
        {
            return $"{actorState.FriendlyName} isn't ready to act.";
        }

        if (targetState.Location != actorState.Location)
        {
            return $"{targetState.FriendlyName} is out of reach.";
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
            targetState.IncrementResolve(-1);
            return $"{targetState.FriendlyName}'s resolve was weakened. {actorRoll} {targetRoll}";
        }
        else
        {
            return $"{targetState.FriendlyName} resists. {actorRoll} {targetRoll}";
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