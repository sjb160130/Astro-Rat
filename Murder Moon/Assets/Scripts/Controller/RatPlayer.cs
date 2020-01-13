using PKG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RatPlayer : MonoBehaviour
{
	private static List<RatPlayer> _players = new List<RatPlayer>(PlayerManager.MaxPlayers);

	[SerializeField]
	int _playerID;
	public int PlayerID { get { return _playerID; } }

	public bool Dead { get; private set; }
	public int Score { get; private set; }
	public bool IsPlaying { get { return PlayerManager.Instance.IsPlaying(_playerID); } }

	public GameObject MyShip;

	public RatSounds Sounds;

	public static RatPlayer FindCurrentWinner()
	{
		RatPlayer winner = null;
		int highestScore = int.MinValue;
		bool tie = false;
		foreach (RatPlayer player in _players)
		{
            if (PlayerManager.Instance.IsPlaying(player.PlayerID) == false)
            {
                player.GetComponent<RatBrain>().SetCrownWinner(false);
                continue;
            }
			if (player.Score > highestScore)
			{
				tie = false;
				winner = player;
				highestScore = player.Score;
                player.GetComponent<RatBrain>().SetCrownWinner(true);
			}
			else if (player.Score == highestScore)
				tie = true;
		}
		if (tie)
			return null;
		return winner;
	}

	public static void SetupPlayersForPlay(float delay = 0f)
	{
		foreach (RatPlayer rp in _players)
		{
			rp.gameObject.SetActive(false);
			if (PlayerManager.Instance.IsPlaying(rp.PlayerID))
			{
				//setup
				rp.ResetPlayer();
				GameObject ship = PoolManager.SpawnObject(rp.MyShip);
				ship.GetCreateComponent<RatShipRespawner>().Respawn(rp, delay);
			}
			else
			{
			}
		}
	}

	private void Awake()
	{
		_players.Add(this);
	}

	private void OnDestroy()
	{
		_players.Remove(this);
	}

	public void ResetPlayer()
	{
		ResetDeath();
		Score = 0;
	}

	public void Kill()
	{
		if (Dead)
			return;
		Dead = true;

		Debug.Log("Player " + this.PlayerID + " killed");

		StartCoroutine(HandleDeathAndRespawn());
		this.gameObject.layer = LayerMask.NameToLayer("Player Dead");

		AudioManager.Instance.PlaySound(this.Sounds.Die, this.transform.position);
	}

	public void ResetDeath()
	{
		Dead = false;
		this.gameObject.layer = LayerMask.NameToLayer("Player");
		this.gameObject.SetActive(true);
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
		RatShipRespawner.Spawn(this, 0f);
		yield return null;
		//ResetDeath();
		//const float InvicibilityDuration = 1f;
		//yield return new WaitForSeconds(InvicibilityDuration);
		//ResetDeath();
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

	internal void AwardPoint()
	{
		if (GameState.Instance.IsPlaying)
			this.Score++;
	}
}