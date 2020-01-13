using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
	[HideInInspector]
	public GameObject itemToSpawn;

	public GameObject explosion;

	public AudioClip soundWhenHittingSomething;

	public void OnCollisionEnter2D(Collision2D collision)
	{
		AudioManager.Instance.PlaySound(soundWhenHittingSomething, transform.position);


        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<RatPlayer>().Kill();
        }
        else
        {
            if (itemToSpawn != null)
                Instantiate(itemToSpawn, transform.position, Quaternion.identity);
        }


        GetComponentInChildren<SpriteRenderer>().enabled = false;
		GetComponent<ParticleSystem>().Stop();
		GetComponent<TrailRenderer>().enabled = false;

		if (explosion != null)
		{
			var explosionInstance = Instantiate(explosion, transform.position, Quaternion.identity);
			Destroy(explosionInstance, 0.5F);
		}

		PoolManager.ReleaseObject(this.gameObject);
	}

}
