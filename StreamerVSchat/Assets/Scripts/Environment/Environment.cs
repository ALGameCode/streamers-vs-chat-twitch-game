using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    //Mapa
    public int MapMaxX = 10;
    public int MapMinX = 10;
    public int MapMaxY = 10;
    public int MapMinY = 10;



    public GameObject enemyCrystal;

    void Start()
    {
        GenerateEnemyCrystal();
    }

    void Update()
    {
        
    }

    private void GenerateEnemyCrystal()
    {
        for(int i = 0; i < ChatStatus.instance.MAX_LIFE; i++)
        {
            Instantiate(enemyCrystal, new Vector3(Random.Range(MapMinX, MapMaxX), Random.Range(MapMaxY, MapMinY), 0), Quaternion.identity);
        }
    }
}
