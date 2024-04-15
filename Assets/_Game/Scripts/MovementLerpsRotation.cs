using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLerpsRotation : MonoBehaviour
{
    public float Scale = 1f;
    public float LerpFactor = 0.9f;
    
    private Vector3 _TrailingOffset;

    void Start()
    {
        _TrailingOffset = transform.position;
    }
    
    void Update()
    {
        var newPos = Vector3.Slerp(_TrailingOffset, transform.position, LerpFactor);

        var offSet = transform.position - newPos;
        transform.rotation = Quaternion.Euler(Quaternion.Euler(Vector3.up*90)*offSet*Scale);

        _TrailingOffset = newPos;
    }
}
