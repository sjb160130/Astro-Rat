using System.Collections.Generic;
using UnityEngine;
using Rewired;
using PKG;

public class PlayerManager : Singleton<PlayerManager>
{
	public bool Locked;

	public class PlayerData
	{
		public bool Ready;
		public int PlayerID;

		public PlayerData(bool ready, int playerID)
		{
			Ready = ready;
			PlayerID = playerID;
		}
	}

	const int MaxPlayers = 8;

	Dictionary<int, PlayerData> _players = new Dictionary<int, PlayerData>();

	public bool IsAllReady()
	{
		if (_players.Count <= 0)
			return false;
		foreach (var pair in _players)
		{
			if (pair.Value.Ready == false)
				return false;
		}
		return true;
	}

	private void Update()
	{
		if (Locked)
			return;

		for (int i = 0; i < MaxPlayers; i++)
		{
			var p = Rewired.ReInput.players.GetPlayer(i);

			if (_players.ContainsKey(i))
			{
				var data = _players[i];
				if (p.GetButtonDown("Leave Game"))
				{
					//unready
					if (data.Ready)
					{
						data.Ready = false;
					}
					//leave
					else
					{
						_players.Remove(i);
					}
				}
				//ready
				else if (p.GetButtonDown("Join Game"))
				{
					data.Ready = true;
				}
			}
			else
			{
				//join
				if (p.GetButtonDown("Join Game"))
					_players.Add(i, new PlayerData(false, i));
			}
		}
	}
}
