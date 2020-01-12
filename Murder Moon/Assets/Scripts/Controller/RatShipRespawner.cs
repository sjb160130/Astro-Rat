using PKG;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RatShipRespawner : MonoBehaviour
{
	RatPlayer _myRatPlayer;

	const float AnimationLength = 1.5f;

	public void Respawn(RatPlayer rp, float delay = 0f)
	{
		this._myRatPlayer = rp;
		if (rp.Dead)
		{

		}
		else
		{
			rp.gameObject.SetActive(false);
		}
		StartCoroutine(RespawnRoutine(delay));
	}

	public static void Spawn(RatPlayer rp, float delay = 0f)
	{
		GameObject ship = PoolManager.SpawnObject(rp.MyShip);
		ship.GetCreateComponent<RatShipRespawner>().Respawn(rp, delay);
	}

	IEnumerator RespawnRoutine(float delay)
	{
		Vector3 scale = this.transform.localScale;
		this.transform.localScale = Vector3.zero;
		SpawnPoint sp = SpawnManager.Instance.getRandomPlayerSpawnLocation();

		Vector3 dropPoint = Planet.GetClosestPointOnPlanet(sp.SpawnHere, 1.5f);
		Vector3 dir = dropPoint - sp.SpawnHere;
		Quaternion startRot = Quaternion.LookRotation(Vector3.forward, dir);

		this.transform.position = sp.SpawnHere;

		yield return new WaitForSeconds(delay);

		this.transform.position = sp.SpawnHere;
		this.transform.rotation = startRot;
		this.transform.DOMove(dropPoint, AnimationLength);
		this.transform.DOScale(scale, AnimationLength).SetEase(Ease.OutElastic);
		yield return new WaitForSeconds(AnimationLength);

		_myRatPlayer.transform.position = dropPoint;
		_myRatPlayer.gameObject.SetActive(true);
		_myRatPlayer.ResetDeath();

		yield return new WaitForSeconds(0.25f);
		this.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir * -1f);
		this.transform.DOMove(sp.SpawnHere, AnimationLength / 3f);
		this.transform.DOScale(scale, AnimationLength / 3f).SetEase(Ease.InExpo);
		yield return new WaitForSeconds(AnimationLength / 3f);

		PoolManager.ReleaseObject(this.gameObject);
	}
}
