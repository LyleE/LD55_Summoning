using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(PointLight))]
public class FlickingLight : MonoBehaviour
{
    public float SampleSpeed;
    public float SampleOffset;
    public Vector2 IntensityRange;  
    
    private Light _light;
    
    void Start()
    {
        _light = GetComponent <Light>();
    }

    void Update()
    {
        _light.intensity = Mathf.Lerp(IntensityRange.x, IntensityRange.y, Mathf.PerlinNoise1D(Time.time*SampleSpeed + SampleOffset));
    }
}
