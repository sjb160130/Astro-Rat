using UnityEngine;

[CreateAssetMenu(menuName = "MurderMoons/Player Sounds")]
public class RatSounds : ScriptableObject
{
	[Header("Sounds")]
	public AudioClip Jump;
	public AudioClip Land;
	public AudioClip Die;
	public AudioClip ChargeYeet;
	public AudioClip Yeet;
	public AudioClip Spawn;
	public AudioClip Grab;
}
