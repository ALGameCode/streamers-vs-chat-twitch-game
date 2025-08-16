using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControllerGameUI : MonoBehaviour
{
    [Header("Streamer:")]
    [SerializeField] private Text textStreamerName;
    [SerializeField] private Slider playerHP;
    [SerializeField] private GameObject energyBallPlayer;
    [SerializeField] private List<Sprite> energyBallPlayerValue = new List<Sprite>();

    [Space(10)]
    [Header("Chat:")]
    [SerializeField] private Text textPoints;
    [SerializeField] private Text textChatCommands;
    [SerializeField] private GameObject energyBallEnemy;
    [SerializeField] private List<Sprite> energyBallEnemyValue = new List<Sprite>();
    [SerializeField] private List<GameObject> lifeCrystalChat = new List<GameObject>();
    [SerializeField] private int linesLimit = 15;
    [SerializeField] private GameObject endGamePopUp;

    [Space(10)]
    [Header("Game:")]
    [SerializeField] private Text textTimer;
    [SerializeField] private Text textWaveName;
    [SerializeField] private float timeShowText;
    private int waveValue = 0;

    private string commmandText;

    void Start()
    {
        ChangeEnergyUI();
        SetChatLifeSprite();
        commmandText = textStreamerName.text;

        CrystalEventManager.OnCrystalDestroyed += SetWaveName;
    }

    private void OnDestroy()
    {
        CrystalEventManager.OnCrystalDestroyed -= SetWaveName;
    }

    void Update()
    {
        SetPlayerHPSlider();
        TimerPrint();
        if (GameManager.instance.OnEndGame)
        {
            endGamePopUp.SetActive(true);
        }
    }

    public void ShowPopUpPainel(GameObject popUp)
    {
        if (popUp.activeSelf)
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
        int index = GetEnergySpriteIndex(ChatStatus.instance.EnergyPercent());
        energyBallEnemy.GetComponent<Image>().sprite = energyBallEnemyValue[index];
    }

    private int GetEnergySpriteIndex(int percent)
    {
        int maxIndex = energyBallEnemyValue.Count - 1;
        float percentage = percent / 100f;
        int spriteIndex = Mathf.RoundToInt(percentage * maxIndex);
        return Mathf.Clamp(spriteIndex, 0, maxIndex);
    }

    public void SetPlayerHPSlider()
    {
        playerHP.maxValue = PlayerStatus.instance.MAX_LIFE;
        playerHP.value = PlayerStatus.instance.life;
    }

    public void SetChatLifeSprite()
    {
        if (lifeCrystalChat == null) return;

        for (int i = 0; i < lifeCrystalChat.Count; i++)
        {
            var go = lifeCrystalChat[i];
            if (!go.activeSelf) continue;

            var img = go.GetComponent<Image>();
            if (img == null) continue;

            // se ainda tiver "vida" para esse cristal, pinta de branco
            if (ChatStatus.instance.life > i)
                img.color = Color.white;
            else
                img.color = Color.black;
        }
    }

    public void SetWaveName(int waveValue)
    {
        textWaveName.text = $"Prepare-se para a onda {waveValue}";
        StartCoroutine(ShowWaveText(timeShowText));
    }

    private IEnumerator ShowWaveText(float time)
    {
        textWaveName.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        textWaveName.gameObject.SetActive(false);
    }

    #region HUD_Text

    public void SetTextStreamName(string streamerName)
    {
        textStreamerName.text = streamerName;
    }

    public void SetTextPoints(string points)
    {
        textPoints.text = points;
    }

    public void SetTextChatCommands((string chatUserName, string command) commandTuple)
    {
        commmandText = $"{commmandText} \n {commandTuple.chatUserName} : {commandTuple.command}";
        SetTextOnChatUI(commmandText);
    }

    public void SetTextChatCommandPlay(string chatUserName)
    {
        commmandText = $"{commmandText} \n The chat user {chatUserName} is now playing!";
        SetTextOnChatUI(commmandText);
    }

    public void SetTextChatWarning(string chatText)
    {
        string textChatWarning = $"{commmandText} \n {chatText}";
        SetTextOnChatUI(textChatWarning);
    }

    public void SetTextOnChatUI(string fullText)
    {
        textChatCommands.text = fullText;
        string[] linesArray = textChatCommands.text.Split('\n');
        List<string> lines = linesArray.ToList();

        if (lines.Count > linesLimit)
        {
            RemoveFirstLine(lines);
        }
    }

    private void RemoveFirstLine(List<string> lines)
    {
        lines.RemoveAt(1);
        commmandText = string.Join("\n", lines);
        textChatCommands.text = commmandText;
    }

    public void TimerPrint()
    {
        // TODO: Passas as duas primeiras linhas para outro código especifico de timer, e deixar só a ultima
        int minuts = ((int)GameManager.instance.GameTime) / 60;
        int seconds = ((int)GameManager.instance.GameTime) - (minuts * 60);
        textTimer.text = $"{minuts} : {seconds}";
    }

    #endregion HUD_Text
}
