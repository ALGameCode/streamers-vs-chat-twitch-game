using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public GameObject SkillArea1;
    public GameObject SkillArea2;
    public int damage = 10;

    void OnSlot1()
    {
        if (!SkillArea1.activeSelf) SkillArea1.SetActive(true);
    }

    void OnSlot2()
    {
        if (!SkillArea2.activeSelf) SkillArea2.SetActive(true);
        StartCoroutine(WaitForSeconds());
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSecondsRealtime(2);
        SkillArea2.SetActive(false);
    }
}
