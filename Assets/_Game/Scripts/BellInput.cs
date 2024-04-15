using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


public class BellInput : MonoBehaviour
{
    public UnityEvent OnBellsRung = new UnityEvent();
    
    public Bell[] Bells;
    public BellSettings Settings;
    
    private int _index;
    private int _direction;

    void Start()
    {
        for (int i = 0; i < Bells.Length; i++)
        {
            var pitch = Mathf.Lerp(Settings.PitchStart, Settings.PitchEnd, (float)i / (Bells.Length - 1));
            Bells[i].Init(pitch);
        }
    }
    
    void Update()
    {
        var rungBells = UpdateIndex();

        // if (Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     Bells[_index].DebugPitch += 0.01f;
        //     Bells[_index].Play();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.DownArrow))
        // {
        //     Bells[_index].DebugPitch -= 0.01f;
        //     Bells[_index].Play();
        // }
        
        for (int i = 0; i < Bells.Length; i++)
        {
            if (i >= rungBells.x && i <= rungBells.y) continue;

            if (Input.GetKeyDown(Bells[i].Number))
            {
                CancelAll();
                _index = 0;
                return;
            }
        }

        var backwardsOffset = (_direction < 0 ? 1 : 0);
        for (int i = (int)rungBells.x; i < rungBells.y; i++)
        {
            Bells[i + backwardsOffset].Play();
        }

        if (rungBells.y == Bells.Length)
        {
            _index = 0;
            OnBellsRung.Invoke();
            CancelAll();
        }
    }
    
    private Vector2 UpdateIndex()
    {
        var lastIndex = _index;
        
        //permit backwards direction too
        if (_index == 0)
        {
            if (Input.GetKeyDown(Bells[0].Number))
            {
                lastIndex = 0;
                _direction = 1;
                _index = lastIndex + _direction;
            }
            // else if (Input.GetKeyDown(Bells[Bells.Length - 1].Number))
            // {
            //     lastIndex = Bells.Length - 1;
            //     _direction = -1;
            //     _index = lastIndex + _direction;
            // }
        }
        
        while(_index >= 0 && _index < Bells.Length && Input.GetKeyDown(Bells[_index].Number))
        {
            _index += _direction;
        }
        
        var minIndex = Mathf.Min(lastIndex, _index);
        var maxIndex = Mathf.Max(lastIndex, _index);

        return new Vector2(minIndex, maxIndex);
    }
    
    private void CancelAll()
    {
        for (int i = 0; i < Bells.Length; i++)
        {
            Bells[i].CancelProgress();
        }
    }

    public void SetBellsRequesting(bool b)
    {
        if (b)
        {
            transform.DOMoveY(1f, 0.5f).SetEase(Ease.InBounce).SetInverted(true).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            transform.DOKill();
            transform.transform.position = Vector3.zero;
        }
    }
}
