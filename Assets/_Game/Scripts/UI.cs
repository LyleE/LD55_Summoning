using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public UnityEvent OnLidsOpened = new UnityEvent();
    public UnityEvent OnLidsClosed = new UnityEvent();

    public UnityEvent NextPage = new UnityEvent();

    public RectTransform TopLid;
    public RectTransform BotLid;
    public TextMeshProUGUI RitualistVoice;
    public float NextPageMarkerFlashSpeed;

    public Image TopLayoutVignette;
    
    private string _currentText;

    private float _markerTimer;
    private bool _flashMarker;
    private int _markerIndex;
    private string[] nextPageMarker = new string[]
    {
        " <font=\"Catholicon SDF\"><alpha=#00>>></font>",
        " <font=\"Catholicon SDF\"><alpha=#FF>>></font>",
    };
    
    [ContextMenu("OpenLids")]
    public void OpenLids()
    {
        TopLid.DOAnchorMin(new Vector2(0f, 1f), 1.5f);
        BotLid.DOAnchorMax(new Vector2(1f, 0f), 1.5f).OnComplete(() => { OnLidsOpened.Invoke(); });
    }

    [ContextMenu("CloseLids")]
    public void CloseLids()
    {
        TopLid.DOAnchorMin(new Vector2(0f, 0f), 1.5f);
        BotLid.DOAnchorMax(new Vector2(1f, 1f), 1.5f).OnComplete(() =>
        {
            OnLidsClosed.Invoke();
        });
    }

    public void ForceCloseLids()
    {
        TopLid.anchorMin = new Vector2(0f, 0f);
        BotLid.anchorMax = new Vector2(1f, 1f);

    }

    public void ClearRitualistVoice()
    {
        _flashMarker = false;
        RitualistVoice.SetText("");
        RitualistVoice.enabled = false;
        TopLayoutVignette.DOKill();
        TopLayoutVignette.DOFade(0f, 0.5f);
        Debug.Log("ClearRitualistVoice");
    }

    public void RitualistSays(string text, bool showNextPageMarker=true, bool useTopLayout=false)
    {
        Cursor.visible = true;

        _currentText = text;
        if (showNextPageMarker)
        {
            _markerTimer = 0f;
            _markerIndex = 0;
            text += nextPageMarker[0];
        }

        RitualistVoice.enabled = true;
        RitualistVoice.fontSize = useTopLayout ? 24 : 35;
        RitualistVoice.verticalAlignment = useTopLayout ? VerticalAlignmentOptions.Top : VerticalAlignmentOptions.Bottom;

        if (useTopLayout)
        {
            TopLayoutVignette.DOKill();
            TopLayoutVignette.DOFade(1f, 0.5f);
        }
        
        // RitualistVoice.alpha = 0f;
        RitualistVoice.SetText(text);
        // RitualistVoice.CrossFadeAlpha(1f, 1f, true);
        RitualistVoice.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, -1f);
        RitualistVoice.fontMaterial.DOFloat(0f, ShaderUtilities.ID_FaceDilate, 1f).OnComplete(() =>
        {
            if (showNextPageMarker)
            {
                _flashMarker = true;
            }
        });
        
        if (Application.platform == RuntimePlatform.OSXEditor && showNextPageMarker)
        {
            _flashMarker = true;
        }
    }

    void Update()
    {
        if (_flashMarker)
        {
            _markerTimer += Time.deltaTime;
            if (_markerTimer > NextPageMarkerFlashSpeed)
            {
                _markerIndex = 1 - _markerIndex;
                RitualistVoice.SetText(_currentText + nextPageMarker[_markerIndex]);
                _markerTimer = 0f;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                NextPage.Invoke();
            }
        }
    }
}
