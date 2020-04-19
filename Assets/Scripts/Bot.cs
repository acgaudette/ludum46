using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Bot", menuName = "Custom/Bot")]
public class Bot : ScriptableObject
{
    [Range(0, 128)] public float aim = 1;
    [Range(0, 1)] public float spread = .015f;
    [Range(0, 64)] public float hysteresis = 1;
    [Range(.01f, 1)] public float brainRate = .01f;
    [Range(0, 1)] public float coverSpeed = .7f;
    [Range(0, 1)] public float peekSpeed = .2f;
    [Range(0, 1)] public float switchSpeed = .7f;
}
