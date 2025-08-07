using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HordeData", menuName = "Configs/Horde/HordeData")]
public class HordeData : ScriptableObject
{
    [Tooltip("Numero de cristais a partir do qual essa horda deve ser invocada")]
    public int crystalIndexTrigger;
    [Tooltip("Lista de ondas dessa horda")]
    public List<WaveData> waves;
}

[System.Serializable]
public class WaveData
{
    [Tooltip("Mobs a serem spawnados nessa onda")]
    public List<MobSpawnWaveInfo> mobSpawnWaveInfo;
    [Tooltip("Tempo após a ativação da horda para iniciar essa onda")]
    public float startDelay;
    [Tooltip("Intervalo entre o spawn de cada mob nessa onda")]
    public float mobSpawnInterval = 0.5f;
}

[System.Serializable]
public class MobSpawnWaveInfo
{
    [Tooltip("Prefab do mob a ser spawnado")]
    public GameObject mobPrefab;
    [Tooltip("Quantidade de mobs desse tipo a serem spawnados")]
    public int quantity;
    [Tooltip("Nome padrão para mobs spawnados pelo sistemas")]
    public string summonerName = "DefaultSummoner";
}
