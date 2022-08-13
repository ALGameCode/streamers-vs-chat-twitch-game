using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public GameObject SkillArea;
    public int damage = 10;

    void OnSlot1()
    {
        if(!SkillArea.activeSelf) SkillArea.SetActive(true);
        StartCoroutine(WaitForSeconds());
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSecondsRealtime(2);
        SkillArea.SetActive(false);
    }
}
