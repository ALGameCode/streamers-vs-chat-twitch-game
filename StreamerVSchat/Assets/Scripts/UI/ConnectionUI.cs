using System.Collections;
using System.Collections.Generic;
using Twitch.Connection;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    public InputField userNameInput;
    public InputField channelNameInput;
    public InputField passwordInput;

    public Text connectionStatusText;

    void Start()
    {
        if(PlayerPrefs.HasKey("username"))
        {
            userNameInput.text = PlayerPrefs.GetString("username");
            channelNameInput.text = PlayerPrefs.GetString("channelname");
        }
    }

    public void ConnectChat()
    {
        TwitchConnection.SetConnection(userNameInput.text.ToString(), passwordInput.text, channelNameInput.text);
        //TwitchConnection.ConnectStatus = true;
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    void Update()
    {
        if(TwitchConnection.IsConnected)
        {
            if(TwitchConnection.IsValidConnection())
            {
                connectionStatusText.text = "Successfully Connected!";
                PlayerStatus.instance.GetPlayerStreamName();
                Save.instance.SaveConnectPlayerInfo(userNameInput.text, channelNameInput.text);
                this.gameObject.SetActive(false);
                //GameManager.instance.PauseGame();
            }
        }
            
    }

}
