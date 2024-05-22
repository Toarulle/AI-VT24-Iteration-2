using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "XStats", menuName = "SO/CarStats")]
public class CarStats : ScriptableObject
{
    public float acceleration;
    public float maxSpeed;
    public float brakeFactor;
    public float boostMultiplier;
    public float turnSpeed;
    public float drift;
    public AudioClip engineSound;
}
