namespace GrilledCheese.FChat
{
    class TicketApiResult
    {
        public string default_character;
        public string[] characters;
        public string ticket;
    }

    class ORSResponse
    {
        public Channel[] channels;   
    }
    class JCHResponse
    {
        public User character;
        public string title;
        public string channel;
    }

// PRI { "character": string, "message": string }
    class PRIResponse
    {
        public string character;
        public string message;
    }

    class LCHResponse
    {
        public string character;
        public string channel;
    }

    // >> MSG { "character": string, "message": string, "channel": string }
    class MSGResponse
    {
        public string character;
        public string message;
        public string channel;
    }

    class ICHResponse
    {
        public User[] users;
        public string channel;
        public string mode;
    }

    struct User
    {
        public string identity;
    }

    class Channel
    {
        public string name;
        public int characters;
        public string title;
    }
}