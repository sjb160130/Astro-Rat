using PKG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	//singleton
	public static AudioManager Instance { get; private set; }
	private PoolManager _pool { get { return PoolManager.Instance; } }

	public AudioClip BattleTrack;
	AudioSource _battleTrackSource;

	public AudioClip IdleTrack;
	AudioSource _idleTrackSource;

	public AudioMixer Mixer;
	public AudioMixerGroup MixerGroupSFX;
	public AudioMixerGroup MixerGroupMusic;

	GameObject _prefab;

	GameObject _container;

	void Start()
	{
		Instance = this;
		_container = new GameObject("Audio Container");
		_battleTrackSource = BuildAudioSource(MixerGroup.Music, true);
		_idleTrackSource = BuildAudioSource(MixerGroup.Music, true);

		_prefab = BuildAudioSource().gameObject;
	}

	public void PlaySound(AudioClip sound, Vector3 position)
	{
		if (_pool == null)
		{
			Debug.LogError("No pool set!");
		}

		StartCoroutine(SpawnSound(sound, position));
	}

	AudioSource BuildAudioSource(MixerGroup group = MixerGroup.SFX, bool loop = false)
	{
		var soundPrefab = new GameObject();
		soundPrefab.transform.SetParent(_container.transform);
		var source = soundPrefab.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.spatialBlend = 0f;
		source.Stop();
		switch (group)
		{
			case MixerGroup.SFX:
				source.outputAudioMixerGroup = MixerGroupSFX;
				break;
			case MixerGroup.Music:
				source.outputAudioMixerGroup = MixerGroupMusic;
				break;
			default:
				break;
		}
		source.loop = loop;
		return source;
	}

	public enum Song { Nothing, Main, Battle }
	public enum MixerGroup { SFX, Music }

	public void PlaySong(Song song)
	{
		SetPlayState(this._battleTrackSource, song == Song.Battle);
		SetPlayState(this._idleTrackSource, song == Song.Main);
	}

	void SetPlayState(AudioSource source, bool val)
	{
		if (val)
		{
			if (source.isPlaying == true)
				source.time = 0f;
			source.Play();
		}
		else
			source.Stop();
	}

	private IEnumerator SpawnSound(AudioClip sound, Vector3 position, bool is3D = false)
	{
		var soundInstance = _pool.spawnObject(
			_prefab,
			position,
			Quaternion.identity).GetComponent<AudioSource>();
		soundInstance.transform.SetParent(_container.transform);
		soundInstance.spatialBlend = is3D ? 1f : 0f;
		soundInstance.clip = sound;
		soundInstance.outputAudioMixerGroup = MixerGroupSFX;
		soundInstance.Play();

		yield return new WaitForSeconds(sound.length);
		PoolManager.ReleaseObject(soundInstance.gameObject);
	}
}