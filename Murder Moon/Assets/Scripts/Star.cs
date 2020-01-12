using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{   
    [HideInInspector]
    public GameObject itemToSpawn;

    public AudioClip soundWhenHittingSomething;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Instance.PlaySound(soundWhenHittingSomething, transform.position);

        GetComponentInChildren<SpriteRenderer>().enabled = false;
        GetComponent<ParticleSystem>().Stop();
        GetComponent<TrailRenderer>().enabled = false;
        Instantiate(itemToSpawn, transform.position, Quaternion.identity);
    }

}
