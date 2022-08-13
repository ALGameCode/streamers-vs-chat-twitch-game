using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : EntityStatus
{
    [SerializeField] public string PlayerName {get; private set;}
    
    public bool isLife = true;

    public static PlayerStatus instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        InitStatus();
    }

    public void GetPlayerStreamName()
    {
        PlayerName = Connection.instance.channelName.Substring(0);
        ControllerGameUI.instance.SetTextStreamName(PlayerName);
    }

    

}
