// A shard will refer to an individual server, each server should have its own instance
// maybe later each channel will be its own instance?
namespace GrilledCheese
{
    public class Shard
    {
        public string id;
        public Dictionary<string, PlayerState> players;
        public Dictionary<string, string> aliasStore;
        public int postLength = 0;
        public TimeSpan EnergyGainLimit = TimeSpan.FromSeconds(1);

        public Shard(string id) 
        {
            this.id = id;
            players = new Dictionary<string, PlayerState>();
            aliasStore = new Dictionary<string, string>();
        }

        public string TranslateAlias(string token)
        {
            if (aliasStore.ContainsKey(token.ToLower()))
            {
                return aliasStore[token.ToLower()];
            }

            return token;
        }

        public void AddAlias(string token, string alias)
        {
            if (aliasStore.ContainsKey(token.ToLower()))
            {
                aliasStore[token.ToLower()] = alias.ToLower();
            }

            aliasStore.Add(token.ToLower(), alias.ToLower());
        }

        public PlayerState ActivatePlayer(string playerName, string playerId)
        {
            playerId = playerId.ToLower();

            PlayerState state;
            if (players.TryGetValue(playerId, out state))
            {
                state.Active = true;
                return state;
            }

            // player doesn't exist, we need to add them
            state = new PlayerState(playerId.ToLower(), playerName);
            players.Add(playerId, state);

            return state;
        }

        public void DeactivatePlayer(string playerId)
        {
            playerId = playerId.ToLower();

            PlayerState state;
            if (players.TryGetValue(playerId, out state))
            {
                state.Active = false;
                return;
            }
        }

        /// <summary>
        /// Returns null if the player doesn't exist
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public PlayerState GetPlayer(string playerId)
        {
            playerId = playerId.ToLower();
            
            PlayerState state;
            if (players.TryGetValue(playerId, out state))
            {
                return state;
            }

            return null;
        }

        public void ResetPlayer(string playerId)
        {
            playerId = playerId.ToLower();

            var currentState = players[playerId];
            players[playerId] = new PlayerState(currentState.Id, currentState.FriendlyName);
        }
    }
}
