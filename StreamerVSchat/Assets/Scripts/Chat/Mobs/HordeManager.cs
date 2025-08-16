using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeManager : MonoBehaviour
{
    [Tooltip("Lista de hordas configuradas. Cada horda é disparada apos a destruição de um cristal")]
    [SerializeField] private List<HordeData> hordes;

    private Dictionary<int, HordeData> hordeDictionary = new Dictionary<int, HordeData>();
    private bool isSpawningHorde = false;

    private void Awake()
    {
        // Monta o dicionario
        foreach (var horde in hordes)
        {
            if(!hordeDictionary.ContainsKey(horde.crystalIndexTrigger))
            {
                hordeDictionary.Add(horde.crystalIndexTrigger, horde);
            }
        }
        // Se inscreve no evento 
        CrystalEventManager.OnCrystalDestroyed += OnCrystalDestroyed;
    }

    private void Start()
    {
        // Dispara a primeira horda imediatamente
        if (hordes.Count > 0)
            StartCoroutine(SpawnHordeRoutine(hordes[0]));
    }

    private void OnDestroy()
    {
        CrystalEventManager.OnCrystalDestroyed -= OnCrystalDestroyed;
    }

    private void OnCrystalDestroyed(int crystalDestroyedCount)
    {
        // Verifico se existe uma horda e inicio o spwan 
        if (hordeDictionary.TryGetValue(crystalDestroyedCount, out HordeData hordeData))
        {
            // Evito invocar a mesma horda ao mesmo tempo
            if(!isSpawningHorde)
            {
                StartCoroutine(SpawnHordeRoutine(hordeData));
            }
        }
    }

    private IEnumerator SpawnHordeRoutine(HordeData hordeData)
    {
        isSpawningHorde = true;

        // Para cada onda espero o delay, depois inicio o spawn
        foreach (var wave in hordeData.waves)
        {
            yield return new WaitForSeconds(wave.startDelay);

            // para cada tipo de mob nessa onda
            foreach (var mob in wave.mobSpawnWaveInfo)
            {
                // Spawn da quantidade desse mob com delay para cada mob
                for(int i = 0; i < mob.quantity; i++)
                {
                    MobSpawn.instance.SpawnMob(mob.mobPrefab, mob.summonerName);
                    yield return new WaitForSeconds(wave.mobSpawnInterval);
                }
            }
        }

        isSpawningHorde = false;
    }
}
