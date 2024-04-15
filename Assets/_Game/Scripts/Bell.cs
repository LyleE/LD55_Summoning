using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Bell : MonoBehaviour
{
    public KeyCode Number;
    public TextMeshPro BellLabel;
    public Transform Model;
    public float DebugPitch = 1f;
    
    AudioSource _bellSfx;
    private bool _isPlayed;
    
    // Start is called before the first frame update
    public void Init(float pitch)
    {
        _bellSfx = GetComponent<AudioSource>();
        _bellSfx.pitch = pitch;
    }

    public void Play()
    {
        _isPlayed = true;
        // Debug.Log($"{this.gameObject.name} pitch = {DebugPitch}");
        // _bellSfx.pitch = DebugPitch;
        _bellSfx.Play();
        BellLabel.color = Color.yellow;
        BellLabel.transform.DOPunchScale(Vector3.one, 0.2f, 10);
        Model.DOPunchRotation(Vector3.forward * 15, 0.1f);
    }

    public void CancelProgress()
    {
        if (_isPlayed)
        {
            BellLabel.color = Color.red;
            BellLabel.transform.DOShakePosition(0.2f, Vector3.one * 0.1f, 100);
            _isPlayed = false;
        }
    }
}
