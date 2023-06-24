using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatStatus : EntityStatus
{   

    //public List<ChatUser> chatUsers = new List<ChatUser>();
    public Dictionary<string, ChatUser> chatUsers = new Dictionary<string, ChatUser>();

    // End Game
    public string userDamage;
    public string userLastHit;
    public string userSupport;


    public int TotalDamageDone {get; set;}
    public int TotalLife {get; set;}

    public static ChatStatus instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        InitStatus();
        // Definir vida maxima do chat
        MAX_LIFE = 8;
    }

    void Start()
    {
        // Vida inicial do chat
        life = MAX_LIFE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    public void AddNewChatUser(string userName)
    {
        if (chatUsers.ContainsKey(userName))
        {
            return;
        }
        chatUsers.Add(userName, new ChatUser(userName, 0, 1, 0));
    }

    public string SearchUserDamage()
    {
        // TODO: Adicionar uma forma de pegar mais de um usuario casa haja empate e desempatar pela quantidade de mob
        string name = "void";
        int damage = 0;

        foreach (var user in chatUsers)
        {
            if(user.Value.DamageDone > damage)
            {
                name = user.Value.UserName;
                damage = user.Value.DamageDone;
            }
        }

        return name;
    }
    
}

public class ChatUser
{
    public string UserName {get; set;}
    public int DamageDone {get; set;}
    public int SpawnedMonsters {get; set;}
    public int SupportDone {get; set;}

    public ChatUser (string name, int damage, int spawned, int support)
    {
        UserName = name;
        DamageDone = damage;
        SpawnedMonsters = spawned;
        SupportDone = support;
    }
}
