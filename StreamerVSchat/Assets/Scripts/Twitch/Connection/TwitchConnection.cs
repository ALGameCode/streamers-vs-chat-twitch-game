using System;
using System.IO;
using System.Net.Sockets;

namespace Twitch.Connection
{
    /// <summary>
    /// Utility class for establishing and managing a connection with the Twitch server
    /// </summary>
    public static class TwitchConnection
    {
        private static string username;
        private static string password; // https://twitchapps.com/tmi
        public static string ChannelName { get; private set; }
        public static bool IsConnected { get; private set; }

        private static TcpClient twitchClient;
        public static StreamReader Reader { get; private set; }
        private static StreamWriter writer;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Establishes a connection with the Twitch server
        /// </summary>
        private static void Connect()
        {
            try
            {
                twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
                Reader = new StreamReader(twitchClient.GetStream());
                writer = new StreamWriter(twitchClient.GetStream());
                writer.WriteLine("PASS " + password);
                writer.WriteLine("NICK " + username);
                writer.WriteLine("USER " + username + " 8 * :" + username);
                writer.WriteLine("JOIN #" + ChannelName);
                writer.Flush();
                IsConnected = true;
            }
            catch(Exception ex)
            {
                IsConnected = false;
                Console.WriteLine("Erro na conexão com o Twitch: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets the connection parameters and establishes a connection with the Twitch server
        /// If a connection is already established, it first disconnects before setting the new connection values
        /// </summary>
        /// <param name="user">The username to connect</param>
        /// <param name="pass">The password to authenticate the connection</param>
        /// <param name="name">The channel name to join</param>
        public static void SetConnection(string user, string pass, string name)
        {
            lock (lockObject)
            {
                if (IsConnected)
                {
                    // If already connected, disconnect before setting new connection values
                    Disconnect();
                }
                username = user;
                password = pass;
                ChannelName = name;

                Connect();
            }
        }

        /// <summary>
        /// Disconnects from the Twitch server
        /// TODO: Testar
        /// </summary>
        public static void Disconnect()
        {
            lock (lockObject)
            {
                if (IsConnected)
                {
                    // Disconnect only if already connected
                    twitchClient.Close();
                    twitchClient = null;
                    Reader.Dispose();
                    Reader = null;
                    writer.Dispose();
                    writer = null;
                    IsConnected = false;
                }
            }
        }

        /// <summary>
        /// Checks if the current connection is valid
        /// </summary>
        /// <returns>True if the connection is valid, false otherwise</returns>
        public static bool IsValidConnection()
        {
            if (IsConnected && twitchClient.Available > 0)
            {
                return true;
            }
            return false;
        }
    }
}