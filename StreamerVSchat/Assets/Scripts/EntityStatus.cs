using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStatus : MonoBehaviour
{
    [HideInInspector]
    public int life;

    [Tooltip("Dano da Entidade")]
    public int damage;

    [Tooltip("Velocidade da Entidade")]
    public float speed = 3f;

    [HideInInspector]
    public int energy;

    [Tooltip("Maximo de energia que a entidade pode obter")]
    public int MAX_ENERGY = 100;

    [Tooltip("Maximo de vida que a entidade pode obter")]
    public int MAX_LIFE = 100;

    [Tooltip("Maximo de velocidade que a entidade pode obter")]
    public const float MAX_SPEED = 10;

    [Tooltip("Minimo de velocidade que a entidade pode obter")]
    public const float MIN_SPEED = 0.5f;

    protected void InitStatus()
    {
        life = MAX_LIFE;
        energy = MAX_ENERGY;
    }

    public void AddLife(int qtd)
    {
        if(life < MAX_LIFE) life += qtd;
    }

    public void DecreaseLife(int qtd)
    {
        if(life > 0) life -= qtd;
    }

    public void AddDamage(int qtd)
    {
        damage += qtd;
    }

    public void DecreaseDamage(int qtd)
    {
        if(damage > 1) damage -= qtd;
    }

    public void AddSpeed(int qtd)
    {
        if(speed < MAX_SPEED) speed += qtd;
    }

    public void DecreaseSpeed(int qtd)
    {
        if(speed > MIN_SPEED) speed -= qtd;
    }
    
    public void AddEnergy(int qtd)
    {
        if(energy < MAX_ENERGY) energy += qtd;
    }

    public void DecreaseEnergy(int qtd)
    {
        if(energy > 0) energy -= qtd;
    }

    public int EnergyPercent()
    {
        return (energy * 100) / MAX_ENERGY;
    }
}
