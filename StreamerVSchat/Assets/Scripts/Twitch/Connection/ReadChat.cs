using Utils;
namespace Twitch.Connection
{
    /// <summary>
    /// Reads chat messages from Twitch and processes them
    /// </summary>
    public static class ReadChat
    {

        /// <summary>
        /// Reads the next chat message from Twitch
        /// </summary>
        /// <returns>A tuple containing the user chat name and chat message</returns>
        public static (string, string) GetChat()
        {
            if (!TwitchConnection.IsConnected || !TwitchConnection.IsValidConnection())
            {
                return (string.Empty, string.Empty);
            }

            string message = TwitchConnection.Reader.ReadLine();
            ChatInfo chatInfo = ProcessChatMessage(message);
            string chatName = chatInfo.ChatName;
            string chatMessage = chatInfo.ChatMessage;
            return (chatName, chatMessage);
        }

        /// <summary>
        /// Processes the chat message and extracts the chat name and message content
        /// </summary>
        /// <param name="message">The chat message to process</param>
        /// <returns>An instance of ChatInfo with the chat name and message content</returns>
        private static ChatInfo ProcessChatMessage(string message)
        {
            ChatInfo chatInfo = new ChatInfo();

            if (!message.Contains("PRIVMSG"))
            {
                return chatInfo;
            }

            var splitPoint = message.IndexOf("!", 1);
            chatInfo.ChatName = message.Substring(1, splitPoint);
            splitPoint = message.IndexOf(":", 1);
            chatInfo.ChatMessage = message.Substring(splitPoint + 1);

            return chatInfo;
        }
    }

    /// <summary>
    /// Represents information about a chat message, including the user chat name and the message content
    /// </summary>
    public class ChatInfo
    {
        /// <summary>
        /// Gets or sets the user name of the chat
        /// </summary>
        public string ChatName { get; set; }
        /// <summary>
        /// Gets or sets the content of the chat message
        /// </summary>
        public string ChatMessage { get; set; }
    }
}