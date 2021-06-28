using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(menuName = "Scriptables/Audio Info", fileName = "Audio Info", order = 1)]
public class AudioInfo : ScriptableObject
{
    [Header("Main")]
    public AudioClip[] Music;

    [Tooltip("0 = Sky, 1 = Surface, 2 = Water, 3 = Lava, 4 = Underground, 5 = Cave")]
    public AudioCollection[] Ambient = new AudioCollection[6];

    [Header("Menu")]
    [Tooltip("0 = Move, 1 = Select")]
    public AudioClip[] Button = new AudioClip[2];


    [Header("Gameplay")]
    [Tooltip("0 = Small, 1 = Big, 2 = SkyFall")]
    public AudioClip[] Fall = new AudioClip[3];

    [Space]
    [Tooltip("0 = Dirt, 1 = Stone, 2 = Gravel, 3 = Glass, 4 = Sand, 5 = Wood")]
    public AudioClip[] BlockPlaced = new AudioClip[4];

    [Tooltip("0 = Dirt, 1 = Stone, 2 = Gravel, 3 = Glass, 4 = Sand, 5 = Wood")]
    public AudioCollection[] BlockRemoved = new AudioCollection[2];

    [Tooltip("0 = Dirt, 1 = Stone, 2 = Gravel, 3 = Glass, 4 = Sand, 5 = Wood")]
    public AudioCollection[] FootSteps = new AudioCollection[2];
}

public enum BlockTypes { DIRT, STONE, GRAVEL, GLASS, SAND, WOOD }
public enum AmbientTypes { SKY, SURFACE, WATER, LAVA, UNDERGROUND, CAVE }
[Serializable] public struct AudioCollection { public AudioClip[] clips; }