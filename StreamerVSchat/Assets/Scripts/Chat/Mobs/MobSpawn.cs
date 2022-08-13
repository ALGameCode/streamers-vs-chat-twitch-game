using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawn : MonoBehaviour
{
    public static MobSpawn instance;
    private GameObject[] waypoints;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        GetWaypoints();
    }

    public void SpawnMob(GameObject mob, Transform target, string summonerName)
    {
        GameObject mobObject = Instantiate(mob, target.position, Quaternion.identity);
        mobObject.GetComponent<MobController>().summonerName = summonerName;
    }

    public void SpawnMob(GameObject mob, string summonerName)
    {
        int index = Random.Range(0, waypoints.Length);
        Transform target = waypoints[index].GetComponent<Transform>();
        GameObject mobObject = Instantiate(mob, target.position, Quaternion.identity);
        mobObject.GetComponent<MobController>().summonerName = summonerName;
    }

    private void GetWaypoints()
    {
        waypoints = GameObject.FindGameObjectsWithTag("waypoint");
    }
}
