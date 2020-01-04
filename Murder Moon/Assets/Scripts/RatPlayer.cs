using System;
using System.Collections;
using UnityEngine;

internal class RatPlayer : MonoBehaviour
{
	[SerializeField]
	int _playerID;

	public bool Dead { get; private set; }

	public void Kill()
	{
		if (Dead)
			return;
		Dead = true;

		StartCoroutine(HandleDeathAndRespawn());
	}

	public void ResetDeath()
	{
		Dead = false;
	}

	IEnumerator HandleDeathAndRespawn()
	{
		//die
		yield return StartCoroutine(Die());

		//respawn
		yield return StartCoroutine(Spawn());
	}

	IEnumerator Die()
	{
		//die

		const float RespawnTime = 2f;
		yield return new WaitForSeconds(RespawnTime);
	}

	IEnumerator Spawn()
	{
		SpawnPoint sp = SpawnPoint.GetSpawnPoint(_playerID, false);
		this.transform.position = sp.Point;
		const float InvicibilityDuration = 1f;
		yield return new WaitForSeconds(InvicibilityDuration);
		ResetDeath();
	}


	public void Configure(int playerID)
	{
		this._playerID = playerID;
		ResetDeath();
	}

	internal void EndGame()
	{
		//despawn
	}
}