public static class CheeseRandom
{
    private static Random rand;

    public static void Initialize()
    {
        rand = new Random((int)DateTime.Now.Millisecond);
    }

    public static int Roll(int min, int max)
    {
        return rand.Next(min, max+1);
    }

    public static int Roll(int max)
    {
        return rand.Next(1, max+1);
    }
}