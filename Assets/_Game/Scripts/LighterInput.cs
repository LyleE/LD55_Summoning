using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TerrainTools;

public class LighterInput : MonoBehaviour
{
    public Transform LighterWheel;
    public GameObject[] LitObjs;
    public bool IsLit;

    public bool DebugAlwaysOn;
    
    private int _layerMask;
    private AudioSource _flickSfx;
    
    void Start()
    {
        _flickSfx = GetComponent<AudioSource>();
        _layerMask = LayerMask.GetMask(new string[]
        {
            "LighterHitPlane"
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Cursor.visible = false;
            IsLit = true;
            _flickSfx.pitch = Random.Range(0.95f, 1.05f);
            _flickSfx.Play();
            
            LighterWheel.DOLocalRotate(Vector3.forward * -90f, 0.2f).SetRelative(true);
            
            foreach (var obj in LitObjs)
            {
                obj.SetActive(true);
            }
        }
        else if (!DebugAlwaysOn && Input.GetMouseButtonUp(0))
        {
            IsLit = false;
            foreach (var obj in LitObjs)
            {
                obj.SetActive(false);
            }
        }
        
        // Debug.Log($"Input.mousePosition = {Input.mousePosition}");
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100000f, _layerMask))
        {
            // Debug.Log($"Hit! at {hit.point}");
            transform.position = hit.point;
        }
    }

    public void ForceOff()
    {
        IsLit = false;
        foreach (var obj in LitObjs)
        {
            obj.SetActive(false);
        }
    }
}
