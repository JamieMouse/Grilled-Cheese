using GrilledCheese;

public class PrintAlias : Command
{
    public PrintAlias() : base("printalias", Description: "Output the currently set aliases used by the bot.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        return string.Join(',', shard.aliasStore.Keys.Select(x => $"{x}, {shard.aliasStore[x]}").ToArray());
    }

    public override bool ValidateArgs(string[] args)
    {
        return true;
    }
}