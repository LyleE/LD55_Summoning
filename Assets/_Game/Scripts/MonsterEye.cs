using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MonsterEye : MonoBehaviour
{
    public float OpenAngle;
    public float OpenDuration = 1f;
    
    public float CloseAngle;
    public float CloseDuration = 1f;

    public Transform TopLid;
    public Transform BotLid;

    void Start()
    {
        // Open();
    }
    
    [ContextMenu("Open")]
    public void Open()
    {
        TopLid.DOKill();
        BotLid.DOKill();

        var topOpenAngle = new Vector3(
            OpenAngle,
            0f,
            TopLid.rotation.eulerAngles.z
        );
        var botOpenAngle = new Vector3(
            -OpenAngle,
            0f,
            BotLid.rotation.eulerAngles.z
        );
        
        TopLid.DOLocalRotate(topOpenAngle, OpenDuration);
        BotLid.DOLocalRotate(botOpenAngle, OpenDuration);
    }
    
    [ContextMenu("Close")]
    public void Close()
    {
        TopLid.DOKill();
        BotLid.DOKill();
        
        var topOpenAngle = new Vector3(
            CloseAngle,
            0f,
            TopLid.rotation.eulerAngles.z
        );
        var botOpenAngle = new Vector3(
            CloseAngle,
            0f,
            BotLid.rotation.eulerAngles.z
        );
        
        TopLid.DOLocalRotate(topOpenAngle, CloseDuration);
        BotLid.DOLocalRotate(botOpenAngle, CloseDuration);
    }
}
