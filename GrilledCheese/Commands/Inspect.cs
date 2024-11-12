using GrilledCheese;

public class Inspect : Command
{
    public Inspect() : base("inspect {target}", Description: "Check a creatures state and print out their stats.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        // identify the target
        PlayerState targetState = shard.GetPlayer(args[1]);

        if (targetState == null)
        {
            // the target isn't here
            return $"{args[1]} isn't here.";
        }

        return targetState.Inspect();
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