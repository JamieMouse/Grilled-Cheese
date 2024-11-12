using GrilledCheese;

public class Roll : Command
{
    public Roll() : base("roll {n}d{s}", 0, "Roll a die")
    {

    }

    public override string Execute(string actor, string[] args, Shard shard)
    {
        string diceFunc = args[1];

        // should be two numbers around a d
        string[] parts = diceFunc.Split('d');
        if (parts.Count() != 2)
        {
            return "Incorrect format.";
        }

        int count, size;
        int.TryParse(parts[0], out count);
        int.TryParse(parts[1], out size);

        List<int> results = new List<int>();
        for (int i = 0; i < count; i++)
        {
            results.Add(CheeseRandom.Roll(size));
        }

        return $"{actor} rolls {string.Join(',', results.Select(x => x.ToString()).ToArray())} = {results.Sum(x => x)}";
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