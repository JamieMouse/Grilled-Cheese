using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GrilledCheese.FChat
{
    public class FChatRelay : IDisposable
    {
        private string channelId;
        private ClientWebSocket connection;
        private Task messageProcessor;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;

        public bool loggedIn = false;

        private Stopwatch lastMessageSent;
        const int messageDelay = 1000; // TODO move to config
        GameProcessor gameProcessor;
        Shard shard;

        public FChatRelay()
        {
            connection = new();
            lastMessageSent = new Stopwatch();
            lastMessageSent.Start();
        }

        public async Task<bool> Setup(IConfiguration config, GameProcessor gameProcessor)
        {
            channelId = config["F_ChannelId"];
            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("Channel id not defined");
            }

            shard = new Shard(channelId);
            string loginTicket = await GetTicketAsync(config);
            this.gameProcessor = gameProcessor;

            Uri uri = new("wss://chat.f-list.net/chat2");
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            await connection.ConnectAsync(uri, cancellationToken);

            // run the reader
            messageProcessor = processMessages(cancellationToken, config);

            await IdentifyClientAsync(loginTicket, config);

            // wait up to 10 seconds for a login response
            int iterations = 20;
            while (!loggedIn && iterations > 0)
            {
                iterations--;
                await Task.Delay(500);
            }

            // Join the private channel
            await JoinChannelAsync(config);

            return true;
        }

        private async Task JoinChannelAsync(IConfiguration config)
        {
            // JCH { "channel": string }
            var channelJoinRequest = new
            {
                channel = channelId
            };

            Console.WriteLine($"Attempt to join {config["F_ChannelName"]}");
            await SendWebsocketMessage("JCH", channelJoinRequest);
        }

        private async Task IdentifyClientAsync(string loginTicket, IConfiguration config)
        {
            // Identify
            var identificationMessage = new
            {
                character = config["F_CharacterName"],
                cversion = config["Version"],
                method = "ticket",
                account = config["F_Account"],
                ticket = loginTicket,
                cname = "Grilled Cheese"
            };

            Console.WriteLine("Sending identification message");
            await SendWebsocketMessage("IDN", identificationMessage);
        }

        private async Task<string> GetTicketAsync(IConfiguration config)
        {
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "account", config["F_Account"] },
                    { "password", config["F_Password"] },
                    { "no_bookmarks", "true" },
                    { "no_friends", "true" }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://www.f-list.net/json/getApiTicket.php", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Acquire the ticket
                    var loginResult = JsonConvert.DeserializeObject<TicketApiResult>(responseContent);
                    return loginResult.ticket;
                }
                else
                {
                    Disconnect("Failed to acquire f-list ticket");
                    throw new Exception("Failed to acquire f-list ticket");
                }
            }
        }

        async Task SendMessageToChat(string message)
        {
            var messageObj = new {
                channel = channelId,
                message
            };

            await SendWebsocketMessage("MSG", messageObj);
        }

        async Task SendWebsocketMessage(string header, object commandBody = null)
        {
            if (messageProcessor.Exception != null || 
            messageProcessor.Status == TaskStatus.Faulted ||
            messageProcessor.Status == TaskStatus.RanToCompletion ||
            messageProcessor.Status == TaskStatus.Canceled)
            {
                // The connection is terminated
                Disconnect("Connection terminated");
            }

            if (!loggedIn && header != "IDN")
            {
                Disconnect("Attempting to send a message before we've logged in");
                // If you're logged out, the only command you can issue is identify until you're logged in
            }

            while (lastMessageSent.IsRunning && lastMessageSent.ElapsedMilliseconds < messageDelay)
            {
                // we can't send a message yet as an anti-spam measure, wait a moment
                await Task.Delay(500);
                Console.WriteLine("Too many messages, waiting to send..");
            }
            lastMessageSent.Reset();

            string trailingCommand = string.Empty;
            if (commandBody != null)
            {
                trailingCommand = " " + JsonConvert.SerializeObject(commandBody);
            }
            string fullMessage = header + trailingCommand;
            Console.WriteLine($"Sending message... {fullMessage}");

            byte[] messageBytes = Encoding.UTF8.GetBytes(fullMessage);
            ArraySegment<byte> messageSegment = new ArraySegment<byte>(messageBytes);

            await connection.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
            lastMessageSent.Start();
        }

        private async Task processMessages(CancellationToken cancellationToken, IConfiguration config)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                string res = string.Empty;
                string buffer = string.Empty;

                // if the last character isn't '}' we're not at the end of the message
                while (!res.EndsWith("}") || res.Length == 3) // some commands come without parameters
                {
                    var bytes = new byte[1024];
                    var result = await connection.ReceiveAsync(bytes, cancellationToken);
                    res = Encoding.UTF8.GetString(bytes, 0, result.Count);
                    buffer += res;
                }

                res = buffer;

                // the first 3 characters is the command
                string messageIdentifier = res.Substring(0, 3);
                switch (messageIdentifier)
                {
                    case "CON":
                        // chat has sent us the count of users
                        string countString = res.Substring(4);
                        dynamic countObject = JsonConvert.DeserializeObject<dynamic>(countString);
                        Console.WriteLine($"There are {countObject.count} users in chat");
                    break;
                    case "LIS":// chat is telling us all the characters online, we don't care
                    case "NLN":// new log in update, we don't care
                    case "STA":// status update, we don't care
                    case "ADL":// returns the list of chatops
                    case "AOP":// the current character has been promoted to chatop
                    case "BRO":// a broadcast from the admin system
                    case "CDS":// the channel description has changed
                    case "CHA":// a list of public channels
                    case "CIU":// recieve an invite to a channel, TODO always accept invites from Jamie
                    case "CBU":// a user was removed from the channel
                    case "CKU":// a user has been kicked from the channel
                    case "COA":// a user has been promoted to channel admin
                    case "COL":// Gives a list of channel ops. Sent in response to JCH.
                    case "COR":// This command requires channel op or higher. Removes a channel operator.
                    case "CSO":// Sets the owner of the current channel to the character provided.
                    case "CTU":// Temporarily bans a user from the channel for 1-90 minutes. A channel timeout.
                    case "DOP":// The given character has been stripped of chatop status.
                    case "FKS":// Sent by as a response to the client's FKS command, containing the results of the search.
                    case "FLN":// Sent by the server to inform the client a given character went offline.
                    case "HLO":// Server hello command. Tells which server version is running and who wrote it.
                    case "KID":// Kinks data in response to a KIN client command.
                    case "IGN":// Handles the ignore list.
                    case "FRL":// Initial friends list.
                    case "PRD":// Profile data commands sent in response to a PRO client command.
                    case "LRP"://A roleplay ad is received from a user in a channel.
                    case "RLL":// Rolls dice or spins the bottle.
                    case "RMO":// Change room mode to accept chat, ads, or both.
                    case "RTB"://Real-time bridge. Indicates the user received a note or message, right at the very moment this is received.
                    case "SFC"://Alerts admins and chatops (global moderators) of an issue.
                    case "SYS"://An informative autogenerated message from the server. This is also the way the server responds to some commands, such as RST, CIU, CBL, COL, and CUB. The server will sometimes send this in concert with a response command, such as with SFC, COA, and COR.
                    case "TPN"://A user informs you of his typing status.
                    case "UPT"://Informs the client of the server's self-tracked online time, and a few other bits of information
                    case "VAR"://Variables the server sends to inform the client about server variables.
                    break;
                    case "PRI":// A private message is received from another user. TODO: should the bot tell the user to message the owner?
                        // recieved a private message
                        var priResponse = JsonConvert.DeserializeObject<PRIResponse>(res.Substring(4));
                        if (priResponse.character == config["F_OwnerName"])
                        {
                            // this is the owner, she might be giving a command.
                            if (priResponse.message == "Disconnect")
                            {
                                Disconnect($"Disconnect initiated by {priResponse.character}");
                            }

                            if (priResponse.message == "Debug")
                            {
                                Console.WriteLine(JsonConvert.SerializeObject(shard));
                            }

                            if (priResponse.message.StartsWith("SetState "))
                            {
                                // the rest of this message is the json representation of a shard, update it
                                try
                                {
                                    string newState = priResponse.message.Substring(8);
                                    Console.WriteLine($"Attempting to set new state {newState}");
                                    Shard newShard = JsonConvert.DeserializeObject<Shard>(newState);
                                    if (newShard == null)
                                    {
                                        Console.WriteLine("Failed to parse state");
                                    }
                                    else
                                    {
                                        var players = shard.players;
                                        shard = newShard;
                                        shard.players = players;
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Failed to set new state");
                                }
                            }
                        }
                    break;
                    case "JCH":// Indicates the given user has joined the given channel. This may also be the client's character.
                        Console.WriteLine("Someone joined the channel");
                        var jchResponse = JsonConvert.DeserializeObject<JCHResponse>(res.Substring(4));
                        if (jchResponse.character.identity != config["F_CharacterName"])
                        {
                            Console.WriteLine($"{jchResponse.character.identity} has joined the channel.");
                            // Add this person to the list of active participants
                            shard.ActivatePlayer(jchResponse.character.identity, jchResponse.character.identity);
                        }
                    break;
                    case "LCH":// An indicator that the given character has left the channel. This may also be the client's character.
                        Console.WriteLine("Someone left the channel");
                        var lchResponse = JsonConvert.DeserializeObject<LCHResponse>(res.Substring(4));
                        if (lchResponse.character != config["F_CharacterName"])
                        {
                            Console.WriteLine($"{lchResponse.character} has left the channel.");
                            shard.DeactivatePlayer(lchResponse.character);
                        }
                    break;
                    case "ICH":// Initial channel data. Received in response to JCH, along with CDS. TODO, we need to process this
                        Console.WriteLine("Joined the channel");
                        var ichResponse = JsonConvert.DeserializeObject<ICHResponse>(res.Substring(4));
                        Console.WriteLine($"Found {ichResponse.users.Count()} users");
                        foreach (User user in ichResponse.users)
                        {
                            if (user.identity != config["F_CharacterName"])
                            {
                                Console.WriteLine($"Activating player {user.identity}");
                                shard.ActivatePlayer(user.identity, user.identity);
                            }
                        }
                    break;
                    case "ORS":// returns a list of open private rooms
                        // We only care about our channel
                        Console.WriteLine("Processing ORS response");
                        var ORSResponse = JsonConvert.DeserializeObject<ORSResponse>(res.Substring(4));
                        Console.WriteLine($"Response parsed {ORSResponse.channels.Count()} channels found");
                        var channelInfo = ORSResponse.channels.FirstOrDefault(x => x.title.Equals(config["F_ChannelName"]));
                        if (!ORSResponse.channels.Any(x => x.title == config["F_ChannelName"]))
                        {
                            // our channel wasn't found
                            Console.WriteLine(res);
                            Console.WriteLine($"Couldn't find {config["F_ChannelName"]}");
                            Disconnect($"Couldn't find {config["F_ChannelName"]}");
                        }

                        channelId = channelInfo.name;
                    break;
                    case "MSG":// A message is received from a user in a channel. TODO: This is the main thing I care about
                        var MSGResponse = JsonConvert.DeserializeObject<MSGResponse>(res.Substring(4));
                        if (MSGResponse.message.StartsWith("!") || MSGResponse.message.StartsWith("/me"))
                        {
                            // this is a command, pass it to the processor
                            string response = gameProcessor.ExecuteCommand(MSGResponse.character, MSGResponse.message, shard);

                            if (response != string.Empty)
                            {
                                await SendMessageToChat(response);
                            }
                        }
                    break;
                    case "PIN":// Ping command from the server, requiring a response, to keep the connection alive.
                        Console.WriteLine("Ping!");
                        await SendWebsocketMessage("PIN");
                    break;
                    case "IDN":// login response
                        loggedIn = true;
                    break;
                    case "ERR":
                        // error, log it and disconnect
                        Console.WriteLine(res);
                        Disconnect(res);
                    break;
                    default:
                        // we don't recognize this command, disconnect
                        Console.WriteLine("Unexpected command detected, disconnecting");
                        Console.WriteLine(res);
                        Disconnect(res);
                    break;
                }
            }
        }

        public async void Dispose()
        {
            Console.WriteLine("Disposing of the relay");
            loggedIn = false;
            cancellationTokenSource.Cancel();
            if (connection != default)
            {
                await connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
            }
        }

        public void Disconnect(string reason)
        {
            Console.WriteLine("Disconnecting");
            Console.WriteLine(reason);
            Dispose();
        }
    }
}


