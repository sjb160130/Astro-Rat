using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PKG;
using Random = UnityEngine.Random;

[Serializable]
public class StarSpawnPoints
{
    [SerializeField]
    public Vector3 SpawnHere;
    [SerializeField]
    public Vector2 DirectionToShoot;
}

public class StarManager : MonoBehaviour
{
    public GameObject star;
    private PoolManager _pool;


    public List<StarSpawnPoints> spawnPoints;



    void Start()
    {
        _pool = GetComponent<PoolManager>();
        StartCoroutine(Spawn(0.1f));
    }


    private IEnumerator Spawn(float waitTime)
    {
        _pool.spawnObject(star);
        _pool.PrintStatus();

        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            StarSpawnPoints location = getRandomSpawnLocation();

            var star2 = _pool.spawnObject(star, location.SpawnHere, Quaternion.identity);
            star2.GetComponent<Rigidbody2D>().AddForce(location.DirectionToShoot);
        }
    }


    private StarSpawnPoints getRandomSpawnLocation()
    {
        var index = Random.Range(0, spawnPoints.Count);
        index = (index -1) < 0 ? 0 : (index - 1);
        return spawnPoints[index];
    }
}

