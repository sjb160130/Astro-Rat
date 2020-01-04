using UnityEngine;

internal class SpawnPoint : MonoBehaviour
{
	public int PlayerID { get { return _playerID; } }
	[SerializeField]
	int _playerID;

	public Vector3 Point { get { return this.transform.position; } }

	public static SpawnPoint GetSpawnPoint(int id, bool random = false)
	{
		var spawnsGO = GameObject.FindGameObjectsWithTag("Spawn Point");
		if (random)
			spawnsGO.Shuffle();
		foreach (var go in spawnsGO)
		{
			SpawnPoint sp = go.GetComponent<SpawnPoint>();
			if (sp.PlayerID == id)
				return sp;
		}
		return null;
	}
}