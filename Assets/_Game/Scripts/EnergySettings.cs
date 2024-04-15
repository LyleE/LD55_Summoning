using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnergySettings : ScriptableObject
{
    public float EnergyGain;
    public float EnergyDecayPerSecond;
    public float EnergyThreshold;
    public float SmoothingFactor;
}
