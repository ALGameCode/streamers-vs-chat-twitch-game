using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool OnStream {get; set;}
    public bool OnPlayGame {get; private set;}
    public bool OnEndGame {get; private set;}
    public float GameTime {get; private set;}

    public GameWinner Winner { get; private set;}


    public static GameManager instance;
    
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        OnPlayGame = true;
    }

    void Update()
    {
        if(OnPlayGame)
        {
            GameTime += Time.deltaTime;
        }
    }

    public void PauseGame()
    {
        if(Time.timeScale == 1) 
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        OnPlayGame = !OnPlayGame;
        
    }

    public void EndGame(GameWinner winner)
    {
        OnPlayGame = false;
        OnEndGame = true;
        Winner = winner;
        // Pegar Infos do Chat
        // Chamar PopUp
    }
}

public enum GameWinner
{
    Player,
    Chat
}