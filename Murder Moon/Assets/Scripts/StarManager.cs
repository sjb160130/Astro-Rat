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

    public List<GameObject> items;

    public List<StarSpawnPoints> spawnPoints;



    void Start()
    {
        _pool = GetComponent<PoolManager>();
        StartCoroutine(Spawn(0.5f));
    }


    private IEnumerator Spawn(float waitTime)
    {
        yield return new WaitForSeconds(1.0F);
        _pool.spawnObject(star);


        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            StarSpawnPoints location = getRandomSpawnLocation();

            var star2 = _pool.spawnObject(star, location.SpawnHere, Quaternion.identity);
            star2.GetComponentInChildren<SpriteRenderer>().enabled = true;
            star2.GetComponent<Rigidbody2D>().AddForce(location.DirectionToShoot);
            star2.GetComponent<Star>().itemToSpawn = ReturnItem();
            StartCoroutine(Release(star2));
        }
    }


    private IEnumerator Release(GameObject star)
    {
        yield return new WaitForSeconds(5.0F);

        _pool.releaseObject(star);
    }

    private GameObject ReturnItem()
    {
        return items[0];
    }

    private StarSpawnPoints getRandomSpawnLocation()
    {
        var index = Random.Range(0, spawnPoints.Count);
        index = index < 0 ? 0 : index;
        return spawnPoints[index];
    }
}

