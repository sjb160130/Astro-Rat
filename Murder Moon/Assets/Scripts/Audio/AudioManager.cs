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
	public AudioMixerGroup MixerGroupTitleSFX;

	GameObject _prefab;

	GameObject _container;

	void Start()
	{
		Instance = this;
		_container = new GameObject("Audio Container");
		_battleTrackSource = BuildAudioSource(MixerGroup.Music, true);
		_battleTrackSource.clip = this.BattleTrack;
		_idleTrackSource = BuildAudioSource(MixerGroup.Music, true);
		_idleTrackSource.clip = this.IdleTrack;

		_prefab = BuildAudioSource().gameObject;
	}

	public AudioSource PlaySound(AudioClip sound, Vector3 position, MixerGroup mixerGroup = MixerGroup.SFX, float volume = 0.95f)
	{
		if (_pool == null)
		{
			Debug.LogError("No pool set!");
		}

		var source = SpawnSound(sound, position, group: mixerGroup);
		source.volume = volume;

		return source;
	}

	AudioSource BuildAudioSource(MixerGroup group = MixerGroup.SFX, bool loop = false)
	{
		var soundPrefab = new GameObject();
		soundPrefab.transform.SetParent(_container.transform);
		var source = soundPrefab.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.spatialBlend = 0f;
		source.Stop();
		SetMixerGroup(source, group);
		source.loop = loop;
		return source;
	}

	public enum Song { Nothing, Main, Battle }
	public enum MixerGroup { SFX, Music, Title }

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

	private AudioSource SpawnSound(AudioClip sound, Vector3 position, bool is3D = false, MixerGroup group = MixerGroup.SFX)
	{
		var soundInstance = _pool.spawnObject(
			_prefab,
			position,
			Quaternion.identity).GetCreateComponent<AudioSource>();
		soundInstance.transform.SetParent(_container.transform);
		soundInstance.spatialBlend = is3D ? 1f : 0f;
		soundInstance.clip = sound;
		SetMixerGroup(soundInstance, group);
		soundInstance.Play();

		StartCoroutine(SpawnSoundRoutine(soundInstance));

		return soundInstance;
	}

	void SetMixerGroup(AudioSource source, MixerGroup group) {
		switch (group)
		{
			case MixerGroup.SFX:
				source.outputAudioMixerGroup = MixerGroupSFX;
				break;
			case MixerGroup.Music:
				source.outputAudioMixerGroup = MixerGroupMusic;
				break;
			case MixerGroup.Title:
				source.outputAudioMixerGroup = MixerGroupTitleSFX;
				break;
			default:
				break;
		}
	}

	private IEnumerator SpawnSoundRoutine(AudioSource source)
	{
		yield return new WaitForSeconds(source.clip.length);
		PoolManager.ReleaseObject(source.gameObject);
	}
}