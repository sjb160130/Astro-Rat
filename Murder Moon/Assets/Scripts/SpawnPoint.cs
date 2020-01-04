using UnityEngine;

internal class SpawnPoint
{
	public int PlayerID { get { return _playerID; } }
	[SerializeField]
	int _playerID;

	[SerializeField]
	GameObject _playerPrefab;

	RatPlayer _spawendPlayer;

	public void Spawn()
	{
		
	}
}