using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCrystal : MonoBehaviour
{
    private int life = 10;

    private void CrystalDamage(int damage)
    {
        if(life > 0)
        {
            life -= damage;
        }
        else
        {
            ChatStatus.instance.DecreaseLife(1);
            ControllerGameUI.instance.SetChatLifeSprite();
            // TODO: Atualizar UI, desabilitando o ultimo cristal da lista
            // TODO: Chamar animação de cristal quebrando
            Destroy(this.gameObject);
        }
    }

    /*public void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log($"LifeCrystal life: {life}");
        if(coll.gameObject.tag.Equals("PlayerSkill"))
        {
            Debug.Log($"LifeCrystal life: {life}");
            CrystalDamage(coll.gameObject.GetComponentInParent<PlayerSkills>().damage);
        }
    }*/

    public void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag.Equals("PlayerSkill"))
        {
            CrystalDamage(coll.gameObject.GetComponentInParent<PlayerSkills>().damage);
        }
    }
}
