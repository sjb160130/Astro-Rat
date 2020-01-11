using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PKG;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnPoints
{
    [SerializeField]
    public Vector3 SpawnHere;
    [SerializeField]
    public Vector2 DirectionToShoot;
}

public class SpawnManager : MonoBehaviour
{
    public GameObject starPrefab;
    private PoolManager _pool;

    public List<GameObject> items;
    public List<SpawnPoints> spawnPoints;

    private bool hasStartedSpawningStars = false;

    void Start()
    {
        _pool = GetComponent<PoolManager>();     
    }

    private void Update()
    {
        
        if(GameState.Instance.IsPlaying && !hasStartedSpawningStars)
        {
            hasStartedSpawningStars = true;
            StartCoroutine(Spawn(1.0f));
        }

    }



    private IEnumerator Spawn(float waitTime)
    {
        yield return new WaitForSeconds(0.1F);
        var star = SpawnStar();
        StartCoroutine(Release(star));

        yield return new WaitForSeconds(1.0F);

        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            var star2 = SpawnStar();
            StartCoroutine(Release(star2));
        }
    }

    private GameObject SpawnStar()
    {
        SpawnPoints location = getRandomSpawnLocation();
        var star = _pool.spawnObject(starPrefab, location.SpawnHere, Quaternion.identity);
        star.GetComponentInChildren<SpriteRenderer>().enabled = true;

        var forceVariance = new Vector2(
            location.DirectionToShoot.x + Random.Range(-1.0F, 1.0F), 
            location.DirectionToShoot.y + Random.Range(-1.0F, 1.0F)
            );

        star.GetComponent<Rigidbody2D>().AddForce(forceVariance);
        star.GetComponent<Star>().itemToSpawn = ReturnItem();
        return _pool.spawnObject(starPrefab, location.SpawnHere, Quaternion.identity);
    }

    private IEnumerator Release(GameObject star)
    {
        yield return new WaitForSeconds(4.0F);

        _pool.releaseObject(star);
    }

    private GameObject ReturnItem()
    {
        return items[0];
    }

    private SpawnPoints getRandomSpawnLocation()
    {
        var index = Random.Range(0, spawnPoints.Count);
        index = index < 0 ? 0 : index;
        return spawnPoints[index];
    }
}

