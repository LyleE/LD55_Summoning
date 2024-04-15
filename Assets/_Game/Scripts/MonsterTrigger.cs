using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTrigger : MonoBehaviour
{
    public LighterInput Lighter;
    public MonsterEye[] Eyes;
    
    public float Range;

    private bool _isOpen;
    void Update()
    {
        var distance = Vector3.Distance(transform.position, Lighter.transform.position);
        if (!_isOpen && Lighter.IsLit && distance < Range)
        {
            _isOpen = true;
            foreach (var eye in Eyes)
            {
                eye.Open();
            }
        }
        else if (_isOpen && distance > Range)
        {
            _isOpen = false;
            foreach (var eye in Eyes)
            {
                eye.Close();
            }
        }
        
    }
}
