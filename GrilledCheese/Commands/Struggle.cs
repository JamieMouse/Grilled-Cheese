using GrilledCheese;

public class Struggle : Command
{
    /// <summary>
    /// Costs energy
    /// Make attempt to escape when dominated
    /// </summary>
    public Struggle() : base("struggle", 1, "Attempt to resist when dominated.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        PlayerState actorState = shard.GetPlayer(actor);
        if (!actorState.Active)
        {
            shard.ActivatePlayer(actorState.FriendlyName, actorState.Id);
        }

        if (actorState.Location != "Lobby")
        {
            // attempt to escape, or at least make things difficult for the dom
            // This action will cost energy from the actor
            if (actorState.Energy < energyCost)
            {
                return $"{actorState.FriendlyName} isn't ready to act.";
            }

            actorState.IncrementEnergy(energyCost*-1);
            if (CheeseRandom.Roll(6) == 6)
            {
                actorState.Location = shard.GetPlayer(actorState.Dom).Location;
                return $"{actorState.FriendlyName} escapes!";
            }
            else
            {
                return $"{actorState.FriendlyName} struggles ineffectively.";
            }
        }
        else
        {
            return "There's nothing to struggle against";
        }
    }

    public override bool ValidateArgs(string[] args)
    {
        return true;
    }
}