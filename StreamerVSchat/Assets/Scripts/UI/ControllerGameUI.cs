using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerGameUI : MonoBehaviour
{
    [Header("Streamer")]
    public Text textStreamerName;
    public Slider playerHP;
    public GameObject energyBallPlayer;
    public List<Sprite> energyBallPlayerValue = new List<Sprite>();

    [Space(10)]
    [Header("Chat")]
    public Text textPoints;
    public Text textChatCommands;
    public GameObject energyBallEnemy;
    public List<Sprite> energyBallEnemyValue = new List<Sprite>();
    public List<GameObject> lifeCrystalChat = new List<GameObject>();
    public Sprite chatCrystal;
    public Sprite chatBrokenCrystal;


    [Space(10)]
    [Header("Game")]
    public Text textTimer;

    private string commmandText;

    // PopUp
    public GameObject endGamePopUp;

    public static ControllerGameUI instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        ChangeEnergyUI();
        SetChatLifeSprite();
    }

    void Update()
    {
        SetPlayerHPSlider();
        TimerPrint();
        if(GameManager.instance.OnEndGame)
        {
            endGamePopUp.SetActive(true);
        }
    }

    public void SetTextStreamName(string streamerName)
    {
        textStreamerName.text = streamerName;
    }

    public void SetTextPoints(string points)
    {
        textPoints.text = points;
    }

    public void SetTextChatCommands(string commands)
    {
        textChatCommands.text = commands;
    }

    public void SetTextChatCommands((string chatUserName, string command) commandTuple)
    {
        commmandText = $"{commmandText} \n {commandTuple.chatUserName} : {commandTuple.command}";
        textChatCommands.text = commmandText;
    }

    public void ShowPopUpPainel(GameObject popUp)
    {
        if(popUp.activeSelf)
        {
            popUp.SetActive(false);
        }
        else
        {
            popUp.SetActive(true);
        }
    }

    public void ChangeEnergyUI()
    {
        int index = 0;
        int percent = ChatStatus.instance.EnergyPercent();
        if(percent > 0 && percent < 11) index = 1;
        else if(percent > 11 && percent < 24) index = 2;
        else if(percent > 24 && percent < 36) index = 3;
        else if(percent > 36 && percent < 49) index = 4;
        else if(percent > 49 && percent < 61) index = 5;
        else if(percent > 61 && percent < 74) index = 6;
        else if(percent > 74 && percent < 86) index = 7;
        else if(percent > 86) index = 8;
        
        energyBallEnemy.GetComponent<Image>().sprite = energyBallEnemyValue[index];
    }

    public void SetPlayerHPSlider()
    {
        playerHP.maxValue = PlayerStatus.instance.MAX_LIFE;
        playerHP.value  = PlayerStatus.instance.life;
    }

    public void TimerPrint()
    {
        int minuts = ((int)GameManager.instance.GameTime) / 60;
        int seconds = ((int)GameManager.instance.GameTime) - (minuts * 60);
        textTimer.text = $"{minuts} : {seconds}";
    }

    public void SetChatLifeSprite()
    {
        if(lifeCrystalChat != null)
        {
            for(int i = 0; i < lifeCrystalChat.Count; i++)
            {
                Debug.Log($"LIFE: index: {i}");
                if(ChatStatus.instance.life > i)
                {
                    if(lifeCrystalChat[i].activeSelf)
                        lifeCrystalChat[i].GetComponent<Image>().sprite = chatCrystal;
                }
                else
                {
                    if(lifeCrystalChat[i].activeSelf)
                        lifeCrystalChat[i].GetComponent<Image>().sprite = chatBrokenCrystal;
                }
            }
        }
        
    }

}
