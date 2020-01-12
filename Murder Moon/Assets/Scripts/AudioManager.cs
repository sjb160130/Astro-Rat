using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //singleton
    public static AudioManager Instance { get; private set; }
    private PoolManager _pool;

    void Start()
    {
        Instance = this;

        _pool = GetComponent<PoolManager>();
    }

    public void PlaySound(AudioClip sound, Vector3 position)
    {
        if (_pool == null)
        {
            Debug.LogError("No pool set!");
        }

        StartCoroutine(SpawnSound(sound, position));
    }

    private IEnumerator SpawnSound(AudioClip sound, Vector3 position)
    {
        var soundPrefab = new GameObject();
        soundPrefab.AddComponent<AudioSource>();
        soundPrefab.GetComponent<AudioSource>().clip = sound;
        soundPrefab.GetComponent<AudioSource>().Play();

        var soundInstance = _pool.spawnObject(soundPrefab, position, Quaternion.identity);

        yield return new WaitForSeconds(0.1F);
    }
}
