using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Spell : MonoBehaviour
{
    public UnityEvent OnRuneCompleted = new UnityEvent();
    public UnityEvent OnSuccess = new UnityEvent();
    
    public Rune RunePrefab;
    public float Spacing;
    public float Scale;

    public Rune InputRune;
    public Rune ProgressRune;

    private Rune[] _runes;
    private int _index;


    public void ClearRune()
    {
        if (_runes != null)
        {
            for (int i = _runes.Length - 1; i >= 0; i--)
            {
                Destroy(_runes[i].gameObject);
            }
        }
    }
    
    public void Init(int size)
    {
        ClearRune();

        _index = 0;
        RuneType left = RuneType.None;
        RuneType right = RuneType.None;

        _runes = new Rune[size];
        
        for (int i = 0; i < size; i++)
        {
            var rune = Instantiate(RunePrefab, transform);
            rune.Glow = true;
            if (i == 0)
            {
                rune.Randomise();
            }
            else
            {
                rune.RandomiseNew(left, right);
            }

            left = rune.LeftRune;
            right = rune.RightRune;

            var pos = Vector3.right * Spacing * (i - (size - 1) / 2.0f);
            rune.transform.localPosition = pos;
            rune.transform.localScale = Vector3.one * Scale;

            _runes[i] = rune;
        }
    }

    void LateUpdate()
    {
        if (_runes != null && _index < _runes.Length)
        {
            var nextRune = _runes[_index];
            if (InputRune.LeftRune == nextRune.LeftRune && InputRune.RightRune == nextRune.RightRune)
            {
                PlayProgress(_index);
                _index++;
            }
        }
    }

    void PlayProgress(int index)
    {
        var endRune = _runes[index];
        Debug.Log($"Progress! {endRune.LeftRune} --- {endRune.RightRune}");
        
        ProgressRune.transform.DOKill(true);

        ProgressRune.Glow = true;
        ProgressRune.Success = true;
        ProgressRune.LeftRune = endRune.LeftRune;
        ProgressRune.RightRune = endRune.RightRune;

        ProgressRune.gameObject.SetActive(true);
        
        ProgressRune.transform.position = InputRune.transform.position;
        ProgressRune.transform.rotation = InputRune.transform.rotation;
        ProgressRune.transform.localScale = InputRune.transform.localScale;

        var tweenTime = 1.5f;

        ProgressRune.transform.DOMove(endRune.transform.position, tweenTime);
        ProgressRune.transform.DORotateQuaternion(endRune.transform.rotation, tweenTime);
        ProgressRune.transform.DOScale(endRune.transform.localScale, tweenTime).OnComplete(() =>
        {
            ProgressRune.gameObject.SetActive(false);

            endRune.Success = true;
            endRune.Redraw();
            
            OnRuneCompleted.Invoke();
            
            if (index == _runes.Length - 1)
            {
                PlaySuccess();
            }
        });
    }

    void PlaySuccess()
    {
        foreach (var rune in _runes)
        {
            rune.transform.DOShakePosition(100f, 0.2f, 10, 90F, false, false)
                .SetLoops(-1);
        }

        OnSuccess.Invoke();
    }
}
