using GrilledCheese;

public class Spin : Command
{
    public Spin() : base("spin", 0, "Select randomly of all the active players.")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        PlayerState[] players = shard.players.Values.Where(x => x.Active).ToArray();

        int index = CheeseRandom.Roll(0, players.Count());

        return $"The bottle selects {players[index].FriendlyName}";
    }

    public override bool ValidateArgs(string[] args)
    {
        return true;
    }
}