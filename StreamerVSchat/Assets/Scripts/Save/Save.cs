using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save : MonoBehaviour
{
    
    public static Save instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SaveConnectPlayerInfo(string username, string channelname)
    {
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("channelname", channelname);
        PlayerPrefs.Save();
    }
}
