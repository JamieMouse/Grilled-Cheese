using GrilledCheese;

public class SetAlias : Command
{
    public SetAlias() : base("setalias {alias} {token}", Description: "Create an alias to remap command names or make characters easier to reference.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        string alias = args[1];
        IEnumerable<string> tokenParts = args.Skip(2);
        string token = string.Join(' ', tokenParts);

        // make sure we can't break the bot by making the setalias command impossible to use
        if (string.Equals(alias.ToLower(), nameof(SetAlias), StringComparison.InvariantCultureIgnoreCase))
        {
            return "No.";
        }

        shard.AddAlias(alias.ToLower(), token.ToLower());
        return "Alias added";
    }

    public override bool ValidateArgs(string[] args)
    {
        // should be made up of setalias {alias} {token} but the original token may have spaces in it, so expect at least 3 parts
        if (args.Count() < 3)
        {
            return false;
        }

        return true;
    }
}