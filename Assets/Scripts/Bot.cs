using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Bot", menuName = "Custom/Bot")]
public class Bot : ScriptableObject
{
    [Range(.01f, 1)] public float brainRate = .01f;
    [Range(0, 20)] public float hysteresis = 1;
    [Range(0, 128)] public float aim = 1;
    [Range(0, 0.2f)] public float spread = .015f;
    [Range(0, 1)] public float baseSpeed = .5f;
    [HideInInspector] [Range(0, 1)] public float coverSpeed = .7f;
    [HideInInspector] [Range(0, 1)] public float peekSpeed = .2f;
    [HideInInspector] [Range(0, 1)] public float switchSpeed = .7f;

    [Range(0, 1)] public float aggro = 0.0f;
    [Range(0, 1)] public float sneak = 0.5f;
    [Range(0, 1)] public float mobile = 0.5f;
    [Range(0, 1)] public float forget = 0.0f;
    [Range(0, 1)] public float trhappy = 0.5f;
    [Range(0, 1)] public float fear = 0.0f;
    [Range(0, 1)] public float stress = 0.0f; // TODO: urg
}
