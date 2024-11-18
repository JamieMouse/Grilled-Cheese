using System.Text;
using System.Threading.Tasks.Sources;
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
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(FormatLine(0, FriendlyName, string.Empty));
            builder.AppendLine(FormatLine(1, nameof(Resolve), Resolve.ToString()));
            builder.AppendLine(FormatLine(1, nameof(Energy), Energy.ToString()));
            builder.AppendLine(FormatLine(1, nameof(Location), Location.ToString()));
            return builder.ToString();
        }

        private string FormatLine(int tabs, string key, string value)
        {
            const string tab = "   ";
            string output = string.Empty;
            for (int i = 0; i < tabs; i++)
            {
                output+=tab;
            }

            output += key + ":" + value;
            return output;
        }
    }
}
