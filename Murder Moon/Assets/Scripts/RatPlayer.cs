using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatPlayer : MonoBehaviour
{
	private static List<RatPlayer> _players = new List<RatPlayer>(PlayerManager.MaxPlayers);

	[SerializeField]
	int _playerID;

	public bool Dead { get; private set; }
	public int Score { get; private set; }
	public bool IsPlaying { get { return PlayerManager.Instance.IsPlaying(_playerID); } }

	public static RatPlayer FindCurrentWinner()
	{
		RatPlayer winner = null;
		int highestScore = int.MinValue;
		bool tie = false;
		foreach (RatPlayer player in _players)
		{
			if (player.Score > highestScore)
			{
				tie = false;
				winner = player;
				highestScore = player.Score;
			}
			else if (player.Score == highestScore)
				tie = true;
		}
		if (tie)
			return null;
		return winner;
	}

	private void Awake()
	{
		_players.Add(this);
	}

	private void OnDestroy()
	{
		_players.Remove(this);
	}

	private void ResetPlayer()
	{
		ResetDeath();
		Score = 0;
	}

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

	internal static void ResetAllPlayers()
	{
		foreach (var p in _players)
		{
			p.ResetPlayer();
		}
	}
}