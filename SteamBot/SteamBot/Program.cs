using System;using System.Collections.Generic;using System.Linq;using System.Text;using System.Threading.Tasks;using SteamKit2;using System.Threading;using System.IO;using System.Security.Cryptography;using ChatterBotAPI;

namespace SteamBot{    class Program    {        static SteamClient steamClient;        static CallbackManager manager;        static SteamFriends steamFriends;        static SteamUser steamUser;        static bool isRunning;        static string user, pass;        static string authCode, twoFactorAuth;
        static string senderName;
        static ChatterBotSession botSession;        static void Main(string[] args)        {            Console.Title = "[ SteamBot ]";            Console.ForegroundColor = ConsoleColor.Red;            Console.WriteLine("\n  _______  _______  _______  _______  __   __  _______  _______  _______ ");            Console.WriteLine(" |       ||       ||       ||   _   ||  |_|  ||  _    ||       ||       |");            Console.WriteLine(" |  _____||_     _||    ___||  |_|  ||       || |_|   ||   _   ||_     _|");            Console.WriteLine(" | |_____   |   |  |   |___ |       ||       ||       ||  | |  |  |   |  ");            Console.WriteLine(" |_____  |  |   |  |    ___||       ||       ||  _   | |  |_|  |  |   |  ");            Console.WriteLine("  _____| |  |   |  |   |___ |   _   || ||_|| || |_|   ||       |  |   |  ");            Console.WriteLine(" |_______|  |___|  |_______||__| |__||_|   |_||_______||_______|  |___|  \n");            Console.ForegroundColor = ConsoleColor.Cyan;            Console.WriteLine("[SteamBot] Thank you for using SteamBot.");            Console.ForegroundColor = ConsoleColor.Yellow;            Console.Write("[SteamBot] Username: ");            Console.ForegroundColor = ConsoleColor.Gray;            user = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Yellow;            Console.Write("[SteamBot] Password: ");
            Console.ForegroundColor = ConsoleColor.Gray;            pass = Console.ReadLine();            Console.ForegroundColor = ConsoleColor.Green;            steamClient = new SteamClient();            manager = new CallbackManager(steamClient);            steamUser = steamClient.GetHandler<SteamUser>();            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnChatMessage);
            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);
            isRunning = true;            Console.WriteLine("[SteamBot] Connecting to Steam...");            steamClient.Connect();            while (isRunning)            {               manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));            }        }        static void OnConnected(SteamClient.ConnectedCallback callback)        {            if (callback.Result != EResult.OK)            {                Console.WriteLine("[SteamBot] Unable to connect to Steam: {0}", callback.Result);                isRunning = false;                return;            }            Console.WriteLine("[SteamBot] Connected to Steam! Logging in '{0}'...", user);            byte[] sentryHash = null;            if (File.Exists("sentry.bin"))            {                                byte[] sentryFile = File.ReadAllBytes("sentry.bin");                sentryHash = CryptoHelper.SHAHash(sentryFile);            }            steamUser.LogOn(new SteamUser.LogOnDetails            {                Username = user,                Password = pass,                AuthCode = authCode,                TwoFactorCode = twoFactorAuth,                SentryFileHash = sentryHash,            });        }        static void OnDisconnected(SteamClient.DisconnectedCallback callback)        {                                    Console.WriteLine("[SteamBot] Disconnected from Steam, reconnecting in 5...");            Thread.Sleep(TimeSpan.FromSeconds(5));            steamClient.Connect();        }        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)        {            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;            if (isSteamGuard || is2FA)            {                Console.WriteLine("[SteamBot] This account is SteamGuard protected!");                if (is2FA)                {                    Console.Write("[SteamBot] Please enter your 2 factor auth code from your authenticator app: ");                    twoFactorAuth = Console.ReadLine();                }                else                {                    Console.Write("[SteamBot] Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);                    authCode = Console.ReadLine();                }                return;            }            if (callback.Result != EResult.OK)            {                Console.WriteLine("[SteamBot] Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);                isRunning = false;                return;            }
            Console.WriteLine("[SteamBot] Successfully logged on!");
            Thread.Sleep(TimeSpan.FromSeconds(3));            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red;            Console.WriteLine("\n  _______  _______  _______  _______  __   __  _______  _______  _______ ");            Console.WriteLine(" |       ||       ||       ||   _   ||  |_|  ||  _    ||       ||       |");            Console.WriteLine(" |  _____||_     _||    ___||  |_|  ||       || |_|   ||   _   ||_     _|");            Console.WriteLine(" | |_____   |   |  |   |___ |       ||       ||       ||  | |  |  |   |  ");            Console.WriteLine(" |_____  |  |   |  |    ___||       ||       ||  _   | |  |_|  |  |   |  ");            Console.WriteLine("  _____| |  |   |  |   |___ |   _   || ||_|| || |_|   ||       |  |   |  ");            Console.WriteLine(" |_______|  |___|  |_______||__| |__||_|   |_||_______||_______|  |___|  \n");            Console.ForegroundColor = ConsoleColor.Cyan;            Console.WriteLine("Log:");            Console.ForegroundColor = ConsoleColor.Magenta;            Console.WriteLine("---------------------------------------------------------------------------------");            Console.ForegroundColor = ConsoleColor.Green;           }        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)        {            Console.WriteLine("[SteamBot] Logged off of Steam: {0}", callback.Result);        }        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)        {            int fileSize;            byte[] sentryHash;            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))            {                fs.Seek(callback.Offset, SeekOrigin.Begin);                fs.Write(callback.Data, 0, callback.BytesToWrite);                fileSize = (int)fs.Length;                fs.Seek(0, SeekOrigin.Begin);                using (var sha = new SHA1CryptoServiceProvider())                {                    sentryHash = sha.ComputeHash(fs);                }            }
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });        }        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }
        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {

                var msg = callback.Message.Trim();
                var msg2 = msg.Split(' ');
                senderName = steamFriends.GetFriendPersonaName(callback.Sender);

                Console.WriteLine($"[SteamBot] Message received: {senderName}: {callback.Message}");
                string[] args;
                switch (msg2[0].ToLower())
                {
                    case "Hello":
                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Hello");
                        break;

                    default:
                        string botReply;
                        ChatterBotFactory factory = new ChatterBotFactory();
                        ChatterBot bot = factory.Create(ChatterBotType.CLEVERBOT);
                        botSession = bot.CreateSession();
                        try
                        {
                           botReply = botSession.Think(callback.Message);
                        }
                        catch (Exception)
                        {
                           return;
                        }

                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, botReply);
                        Console.WriteLine($"[SteamBot] Responded to {senderName}: {botReply}");

                    break;
                }

            }
        }        static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            int friendCount = steamFriends.GetFriendCount();

            Console.WriteLine($"[SteamBot] Bot has {0} friends", friendCount);

            for (int x = 0; x < friendCount; x++)
            {
                SteamID steamIdFriend = steamFriends.GetFriendByIndex(x);

                Console.WriteLine($"[SteamBot] Friend: {0}", steamIdFriend.Render());
            }


            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }        static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            if (callback.Result == EResult.OK)
            {
                Console.WriteLine($"[SteamBot] {0} added friend.", callback.PersonaName);
                steamFriends.SendChatMessage(callback.SteamID, EChatEntryType.ChatMsg, "Thank you for adding me friend.");
            }
        }

        public static string[] Separate(int numberOfArguments, string command)
        {
            string[] separated = new string[4]; // will contain command split by space delimeter

            int length = command.Length;
            int partitionLength = 0;

            int i = 0;
            foreach (char c in command)
            {
                if (i != numberOfArguments)
                {
                    if (partitionLength > length || numberOfArguments > 5)
                    { // error handling
                        separated[0] = "-1";
                        return separated;
                    }
                    else if (c == ' ')
                    {
                        separated[i++] = command.Remove(command.IndexOf(c));
                        command = command.Remove(0, command.IndexOf(c) + 1);
                    }
                    partitionLength++;
                    if (partitionLength == length && i != numberOfArguments)
                    {
                        separated[0] = "-1";
                        return separated;
                    }
                }
                else
                {
                    separated[i] = command;
                }
            }
            return separated;
        }
    }}