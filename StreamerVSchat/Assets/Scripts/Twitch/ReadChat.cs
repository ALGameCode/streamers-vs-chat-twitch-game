using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadChat : MonoBehaviour
{
    void Update()
    {
        GetChat();
    }
    
    public void GetChat()
    {
        if(Connection.instance.ConnectStatus)
        {
            if(Connection.instance.ValidConnection())
            {
                string message = Connection.instance.reader.ReadLine();
                //Debug.Log($"Message 0: {message}");
                string chatName = "";
                if(message.Contains("PRIVMSG"))
                {
                    var splitPoint = message.IndexOf("!", 1);
                    chatName = message.Substring(1, splitPoint);  
                    splitPoint = message.IndexOf(":", 1);
                    message = message.Substring(splitPoint + 1);

                    ValidCommand.instance.ValidChatCommand(chatName, message);
                }
            }
        }        
    }
}
