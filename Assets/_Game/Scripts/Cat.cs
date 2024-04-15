using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public Vector3 JumpPos;

    public GameObject Happy;
    public GameObject Hungry;
    public GameObject Angry;
    public GameObject Jumping;

    private Vector3 _startPos;
    
    void Start()
    {
        _startPos = transform.position;
    }

    public void SetState(int state)
    {
        Happy.SetActive(state == 0);
        Hungry.SetActive(state == 1);
        Angry.SetActive(state == 2);
        Jumping.SetActive(state == 3);
    }
    
    public void Jump(Action callback)
    {
        transform.position = _startPos;
        transform.DOMove(JumpPos, 0.2f)
            .SetEase(Ease.Linear)
            .SetDelay(0.3f)
            .OnComplete(() =>
        {
            callback.Invoke();
        });
    }
}
