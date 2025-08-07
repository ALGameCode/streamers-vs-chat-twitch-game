using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCrystal : MonoBehaviour
{
    private int life = 10;

    private static int totalCrystalDestroyed = 0; // Contador global de cristais destruidos

    private void CrystalDamage(int damage)
    {
        if(life > 0)
        {
            life -= damage;
        }
        else
        {
            ChatStatus.instance.DecreaseLife(1);
            FindObjectOfType<ControllerGameUI>().SetChatLifeSprite(); // TODO: Change
            // TODO: Atualizar UI, desabilitando o ultimo cristal da lista
            // TODO: Chamar animação de cristal quebrando
            totalCrystalDestroyed++; // Implementando contador de cristais destruidos
            CrystalEventManager.CrystalDestroyed(totalCrystalDestroyed); // Distaprando o evento para o HordeManager
            Destroy(this.gameObject); // Destruir cristal
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
