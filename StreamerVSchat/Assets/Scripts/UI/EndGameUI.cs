using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    public Text winner;
    public Text userDamage;
    public Text userLastHit;

    public string chatWinnerMessage;
    public string playerWinnerMessage;

    void OnEnable()
    {
        if(GameManager.instance.Winner.Equals(GameWinner.Player))
        {
            PlayerWinnerUI();
        }
        else
        {
            ChatWinnerUI();
        }
    }

    private void PlayerWinnerUI()
    {
        winner.text = playerWinnerMessage;
        userLastHit.gameObject.SetActive(false);
        userDamage.gameObject.SetActive(false);
    }

    private void ChatWinnerUI()
    {
        winner.text = chatWinnerMessage;
        userDamage.text = ChatStatus.instance.SearchUserDamage(); //ChatStatus.instance.userDamage;
        userLastHit.text = ChatStatus.instance.userLastHit;
    }
}
