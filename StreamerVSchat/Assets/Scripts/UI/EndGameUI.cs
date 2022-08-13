using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    public Text winner;
    public Text userDamage;
    public Text userLastHit;

    void OnEnable()
    {
        userDamage.text = ChatStatus.instance.SearchUserDamage(); //ChatStatus.instance.userDamage;
        userLastHit.text = ChatStatus.instance.userLastHit;
    }
}
