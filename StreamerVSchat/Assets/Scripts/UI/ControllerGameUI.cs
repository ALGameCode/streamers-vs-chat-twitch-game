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

    public void SetTextChatCommandPlay(string chatUserName)
    {
        commmandText = $"{commmandText} \n The chat user {chatUserName} is now playing!";
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
        int index = GetEnergyIndex(ChatStatus.instance.EnergyPercent());        
        energyBallEnemy.GetComponent<Image>().sprite = energyBallEnemyValue[index];
    }

    private int GetEnergyIndex(int percent)
    {
        if (percent > 86)
            return 8;
        if (percent > 74)
            return 7;
        if (percent > 61)
            return 6;
        if (percent > 49)
            return 5;
        if (percent > 36)
            return 4;
        if (percent > 24)
            return 3;
        if (percent > 11)
            return 2;
        if (percent > 0)
            return 1;

        return 0;
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
