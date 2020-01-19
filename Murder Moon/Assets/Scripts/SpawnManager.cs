using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PKG;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnPoint
{
	[SerializeField]
	public Vector3 SpawnHere;
	[SerializeField]
	public Vector2 DirectionToShoot;
}


public class SpawnManager : MonoBehaviour
{
	public GameObject starPrefab;
	private PoolManager _pool { get { return PoolManager.Instance; } }

	public List<GameObject> items;
	public GameObject[] spawnPoints;

	private bool hasStartedSpawningStars = false;

	static public SpawnManager Instance { get; private set; }

	public bool SpawningActive;

	private void Awake()
	{
		Instance = this;
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");
	}

	private void Update()
	{

		if (GameState.Instance.IsPlaying && !hasStartedSpawningStars)
		{
			hasStartedSpawningStars = true;
			StartCoroutine(SpawnStarsWhileGameIsRunning(1.0f));
		}

	}

	private IEnumerator SpawnStarsWhileGameIsRunning(float waitTime)
	{

		while (GameState.Instance.IsPlaying)
		{
			while (SpawningActive == false)
				yield return null;
			yield return new WaitForSeconds(waitTime);
			if (GameObject.FindGameObjectsWithTag("Item").Length >= (PlayerManager.Instance.PlayerCount + 3))
				continue;
			var star = SpawnStar();
			StartCoroutine(Release(star));
		}

		hasStartedSpawningStars = false;
	}

	private GameObject SpawnStar()
	{
		SpawnPoint location = getRandomStarSpawnLocation();
		var star = _pool.spawnObject(starPrefab, location.SpawnHere, Quaternion.identity);
		star.GetComponentInChildren<SpriteRenderer>().enabled = true;
		star.GetComponent<TrailRenderer>().enabled = true;
		star.GetComponent<ParticleSystem>().Play();

		var forceVariance = new Vector2(
			location.DirectionToShoot.x + Random.Range(-1.0F, 1.0F),
			location.DirectionToShoot.y + Random.Range(-1.0F, 1.0F)
			);

		star.GetComponent<Rigidbody2D>().AddForce(forceVariance);
		star.GetComponent<Star>().itemToSpawn = ReturnItem();
		return star;
	}

	private IEnumerator Release(GameObject star)
	{
		yield return new WaitForSeconds(4.0F);
		_pool.releaseObject(star);
	}

	private GameObject ReturnItem()
	{
		var index = Random.Range(0, items.Count);
		index = index < 0 ? 0 : index;
		return items[index];
	}

	private SpawnPoint getRandomStarSpawnLocation()
	{
		GameObject go = this.spawnPoints.GetRandom();
		return getSpawnPoint(go.transform.position);
	}

	SpawnPoint getSpawnPoint(Vector3 point)
	{
		GameObject planet = GameObject.FindGameObjectsWithTag("Planet").GetRandom();
		Vector2 direction = (planet.transform.position - point).normalized;
		return new SpawnPoint() { SpawnHere = point, DirectionToShoot = direction };
	}



	//private GameObject RespawnPlayer()
	//{
	//    SpawnPoints location = getRandomPlayerSpawnLocation();


	//    var star = _pool.spawnObject(starPrefab, location.SpawnHere, Quaternion.identity);


	//    star.GetComponent<Rigidbody2D>().AddForce(location.DirectionToShoot);

	//    return _pool.spawnObject(starPrefab, location.SpawnHere, Quaternion.identity);
	//}

	int lastPlayerSpawnIndex = 1;
	public SpawnPoint getRandomPlayerSpawnLocation()
	{
		lastPlayerSpawnIndex++;
		if (lastPlayerSpawnIndex >= this.spawnPoints.Length)
		{
			lastPlayerSpawnIndex = 0;
		}
		return getSpawnPoint(this.spawnPoints[lastPlayerSpawnIndex].transform.position);
	}

}

