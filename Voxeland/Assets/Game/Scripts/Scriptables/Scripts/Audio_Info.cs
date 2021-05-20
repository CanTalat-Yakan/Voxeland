using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(menuName = "Scriptables/Audio Info", fileName = "Audio Info", order = 1)]
public class Audio_Info : ScriptableObject
{
    [Header("Music")]
    public AudioClip MainMusic;
    [Header("Menu")]
    public AudioClip ButtonSelect;
    public AudioClip ButtonMove;
    public AudioClip Joined;

    [Header("Gameplay")]
    public AudioClip Attack;
    public AudioClip Fall;
    [Tooltip("0 = Dirt, 1 = Stone, 2 = Glass")]
    public AudioClip[] BlockAdded = new AudioClip[3];
    [Tooltip("0 = Dirt, 1 = Stone, 2 = Glass")]
    public AudioClip[] BlockRemoved = new AudioClip[3];
}
public enum BlockSoundTypes
{
    DIRT,
    STONE,
    GLASS
}