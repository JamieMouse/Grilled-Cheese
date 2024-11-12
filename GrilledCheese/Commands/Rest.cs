using GrilledCheese;

public class Rest : Command
{
    public Rest() : base("rest", 1, "Rest to regain Resolve, does not always succeed.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
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

        if (actorState.Resolve < 3)
        {
            actorState.IncrementEnergy(-1);
            
            // There is a 50 % chance of success
            if (CheeseRandom.Roll(100) > 50)
            {
                actorState.IncrementResolve(1);
                return $"{actorState.FriendlyName} rests and recovers some stamina.";
            }

            return $"{actorState.FriendlyName} begins to rest..";
        }
        else
        {
            return $"{actorState.FriendlyName} is already feeling well!";
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