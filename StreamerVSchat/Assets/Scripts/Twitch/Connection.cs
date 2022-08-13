using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net.Sockets;
using System;
using System.ComponentModel;

public class Connection : MonoBehaviour
{
    private string username;
    private string password; // https://twitchapps.com/tmi
    public string channelName {get; private set;}
    public bool ConnectStatus {get; set;}

    private TcpClient twitchClient;
    public StreamReader reader {get; private set;}
    private StreamWriter writer;

    public static Connection instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());
        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
    }

    public void InsertConnectionValue(string user, string pass, string name)
    {
        username = user;
        password = pass;
        channelName = name;

        Connect();
    }

    public bool ValidConnection()
    {
        if(twitchClient.Available > 0) return true;
        return false;
    }
}
