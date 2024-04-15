using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RuneInput : MonoBehaviour
{
    public UnityEvent OnRuneConjured = new UnityEvent();
    
    public string[] KeysPerKeyboard;
    public string InputKeys;
    public TextMeshPro KeyDisplay;
    
    public EnergySettings Settings;

    public Material GoodMat;
    public Material BadMat;

    public Rune TargetRune;
    public RuneType TargetRuneType;
    public bool InputEnabled;

    private KeyCode[] _inputKeys;
    private bool[] _isKeyHeldDown;
    private KeyCode _lastSeenKey = KeyCode.None;
    private float _energyPercentage;
    private float _energyPercentageSmoothed;

    private GameObject _bar;
    private MeshRenderer _debugRenderer;
    private bool _atThreshold;
    
    void Start()
    {
        _bar = transform.GetChild(0).gameObject;
        _debugRenderer = _bar.transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    public void InitKeyboard(int keyboardID)
    {
        InputKeys = KeysPerKeyboard[keyboardID].ToUpper();
        
        List<KeyCode> keyCodes = new List<KeyCode>();
        foreach(char key in InputKeys)
        {
            var parsedKeyCode = GetKeyCode(key);
            if (!keyCodes.Contains(parsedKeyCode))
            {
                // Debug.Log($"Processed {parsedKeyCode}");
                keyCodes.Add(parsedKeyCode);
            }
        }

        _inputKeys = keyCodes.ToArray();
        _isKeyHeldDown = new bool[_inputKeys.Length];

        UpdateKeyDisplay();
    }

    private KeyCode GetKeyCode(char c)
    {
        switch (c)
        {
            case ';': return KeyCode.Semicolon;
            case '\'': return KeyCode.Quote;
            case ',': return KeyCode.Comma;
            case '.': return KeyCode.Period;
            case '/': return KeyCode.Slash;
            case '\\': return KeyCode.Backslash;
            case '`': return KeyCode.BackQuote;
            case '[': return KeyCode.LeftBracket;
            case ']': return KeyCode.RightBracket;
            case '{': return KeyCode.LeftCurlyBracket;
            case '}': return KeyCode.RightCurlyBracket;
            case ':': return KeyCode.Colon;
            case '"': return KeyCode.DoubleQuote;
            case '|': return KeyCode.Pipe;
            case '~': return KeyCode.Tilde;
            case '<': return KeyCode.Less;
            case '>': return KeyCode.Greater;
            case '?': return KeyCode.Question;
            case '!': return KeyCode.Exclaim;
            case '-': return KeyCode.Minus;

            default: return (KeyCode)Enum.Parse(typeof(KeyCode), ""+c);
        }
    }

    public void SetInputEnabled(bool isEnabled)
    {
        InputEnabled = isEnabled;

        if (!InputEnabled)
        {
            for (int i = 0; i < _isKeyHeldDown.Length; i++)
            {
                _isKeyHeldDown[i] = false;
            }

            UpdateKeyDisplay();
        }
    }
    
    void Update()
    {
        if (_inputKeys == null)
            return;
        
        var keysChanged = false;
        if (InputEnabled)
        {
            for (int i = 0; i < _inputKeys.Length; i++)
            {
                var key = _inputKeys[i];
                if (Input.GetKeyDown(key))
                {
                    _isKeyHeldDown[i] = true;
                    keysChanged = true;

                    if (key != _lastSeenKey)
                    {
                        _lastSeenKey = key;
                        _energyPercentage += Settings.EnergyGain;
                        if (_energyPercentage > 1) _energyPercentage = 1;
                    }
                }
                else if (Input.GetKeyUp(key))
                {
                    _isKeyHeldDown[i] = false;
                    keysChanged = true;
                }
            }
        }

        if (keysChanged)
        {
            UpdateKeyDisplay();
        }
        
        _energyPercentage -= Settings.EnergyDecayPerSecond * Time.deltaTime;
        if (_energyPercentage < 0) _energyPercentage = 0;

        _energyPercentageSmoothed = Mathf.Lerp(_energyPercentageSmoothed, _energyPercentage, Settings.SmoothingFactor);
        
        _bar.transform.localScale = new Vector3(_energyPercentageSmoothed, 1, 1);
        _bar.SetActive(_energyPercentageSmoothed > 0.01f);

        var atThreshold = _energyPercentageSmoothed > Settings.EnergyThreshold;

        if (atThreshold != _atThreshold)
        {
            _atThreshold = atThreshold;
            _debugRenderer.material = atThreshold ? GoodMat : BadMat;

            if (atThreshold)
            {
                OnRuneConjured.Invoke();
                TargetRune.SetRune(TargetRuneType);
            }
            // else if(IsRight)
            // {
            //     TargetRune.ClearRight();
            // }
            // else
            // {
            //     TargetRune.ClearLeft();
            // }
        }
    }

    void UpdateKeyDisplay()
    {
        string displayText = "";
        for (int i = 0; i < InputKeys.Length; i++)
        {
            if (_isKeyHeldDown[i])
            {
                displayText += $"<color=green>{InputKeys[i]}</color> ";
            }
            else
            {
                displayText += $"{InputKeys[i]} ";
            }
        }
        
        KeyDisplay.SetText(displayText);
    }
}
