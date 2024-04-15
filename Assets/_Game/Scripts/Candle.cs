using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Candle : MonoBehaviour
{
    public UnityEvent OnReignition = new UnityEvent();
    
    public GameObject[] LitObjs;
    public Light Light;
    public FlickingLight FlickerVfx;
    
    public float IgnitionRange;
    public LighterInput Lighter;

    public Vector2 FizzleTimeInterval;

    public AudioSource LitSfx;
    public AudioSource UnLitSfx;

    public bool AutoFizzleOutEnabled;
    
    private bool _isLit;
    private float _fizzleTimer;

    public bool IsLit
    {
        get { return _isLit; }
    }

    void Start()
    {
        Ignite();
        FlickerVfx.enabled = false;
        Light.DOIntensity(0f,0.5f)
            .SetInverted(true)
            .OnComplete(() =>
        {
            FlickerVfx.enabled = true;
        });
    }

    void RenewFizzleTimer()
    {
        _fizzleTimer = Random.Range(FizzleTimeInterval.x, FizzleTimeInterval.y);
    }
    
    void Update()
    {
        if (!_isLit && Lighter.IsLit && DistanceToLighter() < IgnitionRange)
        {
            Ignite();
        }

        if (_isLit && AutoFizzleOutEnabled)
        {
            _fizzleTimer -= Time.deltaTime;
            if (_fizzleTimer <= 0)
            {
                FizzleOut();
            }
        }
    }

    float DistanceToLighter()
    {
        return Vector3.Distance(transform.position, Lighter.transform.position);
    }
    
    public void Ignite()
    {
        _isLit = true;
        RenewFizzleTimer();
        
        foreach (var obj in LitObjs)
        {
            obj.SetActive(true);
        }

        LitSfx.pitch = Random.Range(0.9f, 1.1f);
        LitSfx.Play();
        
        OnReignition.Invoke();
    }
    
    public void FizzleOut()
    {
        _isLit = false;
        
        foreach (var obj in LitObjs)
        {
            obj.SetActive(false);
        }
        
        UnLitSfx.pitch = Random.Range(0.9f, 1.1f);
        UnLitSfx.Play();
    }
}
