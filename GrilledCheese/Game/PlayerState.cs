using Newtonsoft.Json;

namespace GrilledCheese
{
    public class PlayerState
    {
        public string Id;
        public bool Active;
        public string FriendlyName;
        public int Energy { get; private set; }
        public int Resolve { get; private set; }
        public string Location;
        public string Dom;
        public DateTime LastEnergy = DateTime.MinValue;

        public PlayerState(string Identifier, string Name)
        {
            this.Id = Identifier;
            this.FriendlyName = Name;
            Active = true;

            Energy = 1;
            Resolve = 3;

            Location = "Lobby";
        }

        public void IncrementEnergy(int modifier)
        {
            Console.WriteLine($"Awarding energy to {this.Id}");
            Energy = Math.Clamp(Energy+= modifier, 0, 3);
        }

        public void IncrementResolve(int modifier)
        {
            Resolve = Math.Clamp(Resolve+= modifier, 0, 3);
        }

        public string Inspect()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
