using GrilledCheese;

public class Echo : Command
{
    public Echo() : base("echo {args}", Description: "Echo back the arguments recieved by the command for debugging.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        return string.Join(' ', args);
    }

    public override bool ValidateArgs(string[] args)
    {
        return true;
    }
}