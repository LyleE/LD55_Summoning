using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public class GameFlowEntry
{
    public string Name;
    public string ParsedMessage;
    public bool NoNext;
    public bool ShouldOpenEyesOnNextPage;
    public bool ShouldCloseEyesOnNextPage;
    public bool UseTopLayout;
    public int Index;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public UI UI;
    public Spell Spell;
    public Candle[] Candles;
    public LighterInput Lighter;
    public BellInput Bells;
    public RuneInput[] RunesInputs;
    public GameObject RuneTutorialHintArrow;
    public GameObject CandleTutorialHintArrow;
    public Rune TargetRune;
    public GameObject Monster;

    public TextMeshPro TimerText;
    public float _gameTimerSpeed;
    private float _gameTimer;
    public bool DebugWin;
    
    public AudioSource TickSfx;
    public AudioSource TockSfx;
    public AudioSource SuccessSfx;
    public AudioSource FailureSfx;
    private float _lastSecond;
    
    private int _activeGameFlowIndex;
    [SerializeField]
    private GameFlowEntry[] _gameFlowList;
    private Dictionary<string, GameFlowEntry> _gameFlowLookup;

    private int _levelNumber;
    private int _runeCount;

    public AudioSource BellRequestSfx;
    private bool _waitingOnBells;
    private float _bellCountdown;

    public Cat Cat;
    public Light CatLight;
    public GameObject RuneInputParent;
    public AudioSource CatSpawnSfx;
    public AudioSource CatHungerSfx;
    public AudioSource CatAngrySfx;
    public AudioSource CatPounceSfx;
    
    void Awake()
    {
        Instance = this;
        
        // UI.OnLidsOpened.AddListener(() =>
        // {
        //     Debug.Log("Open");
        // });
        // // UI.OpenLids();
        //
        // UI.NextPage.AddListener(() =>
        // {
        //     Debug.Log(("Next page clicked!"));
        //     UI.ClearRitualistVoice();
        //     UI.OpenLids();
        // });
        // UI.RitualistSays("Ack! My bloodsworn have failed to wake me from my meditations! If I don't complete tonight's ritual, our Dark Lord will turn his malevolent gaze upon us.");

        var gameFlowFile = Resources.Load<TextAsset>("gameflow");

        _gameFlowLookup = new Dictionary<string, GameFlowEntry>();
        var list = new List<GameFlowEntry>();
        
        var rawGameFlow = gameFlowFile.text.Split("--");
        foreach (var raw in rawGameFlow)
        {
            var processedText = raw.Trim();
            var entry = new GameFlowEntry();
            entry.Index = list.Count;
            
            if (processedText.StartsWith('['))
            {
                var tagEndIndex = processedText.IndexOf(']');
                var tags = processedText.Substring(1, tagEndIndex - 1).Split(',');
                processedText = processedText.Substring(tagEndIndex+1).Trim();
                Debug.Log($"Tags: {tags.Length}, Content: {processedText}");

                foreach (var rawTag in tags)
                {
                    var tag = rawTag.Trim();
                    if (tag.StartsWith("name"))
                    {
                        entry.Name = tag.Split('=')[1].Trim();
                        _gameFlowLookup.Add(entry.Name, entry);
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "no_next":
                                entry.NoNext = true;
                                break;
                            case "open_eyes":
                                entry.ShouldOpenEyesOnNextPage = true;
                                break;
                            case "close_eyes":
                                entry.ShouldCloseEyesOnNextPage = true;
                                break;
                            case "top_layout":
                                entry.UseTopLayout = true;
                                break;
                            default: Debug.LogError($"Unknown tag in gameflow file: {tag}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"Tags: N/A, Content: {processedText}");
            }

            entry.ParsedMessage = processedText;
            
            list.Add(entry);
        }

        _gameFlowList = list.ToArray();
        
        ProcessFlow(_gameFlowList[0]);
    }

    private void GoToNextGameFlow()
    {
        var entry = _gameFlowList[_activeGameFlowIndex+1];
        ProcessFlow(entry);
    }

    private void GoToNamedGameFlow(string entryName)
    {
        var entry = _gameFlowLookup[entryName];
        ProcessFlow(entry);
    }

    private void ProcessFlow(GameFlowEntry entry)
    {
        UI.ClearRitualistVoice();
            
        _activeGameFlowIndex = entry.Index;
        
        var showNextMarker = !entry.NoNext;
        
        if (showNextMarker)
        {
            UI.NextPage.RemoveListener(HandleNextClicked);
            UI.NextPage.AddListener(HandleNextClicked);
        }
        
        UI.RitualistSays(entry.ParsedMessage, showNextMarker, entry.UseTopLayout);
        if (entry.Name != null)
        {
            HandleEnteredNamedState(entry.Name);
        }
    }

    private void HandleNextClicked()
    {
        UI.NextPage.RemoveListener(HandleNextClicked);
        
        var activeEntry = _gameFlowList[_activeGameFlowIndex];
        Debug.Log(activeEntry.Name);
        
        if (activeEntry.Name == "kitten_pounce")
        {
            Cat.SetState(3);
            Cat.Jump(HandleEndState);
            StartCoroutine(DelayedPounceSfx());
            return;
        }
        
        if (activeEntry.ShouldOpenEyesOnNextPage)
        {
            UI.ClearRitualistVoice();
            UI.OnLidsOpened.RemoveListener(GoNextAfterTransition);
            UI.OnLidsOpened.AddListener(GoNextAfterTransition);
            UI.OpenLids();
        }
        else if (activeEntry.ShouldCloseEyesOnNextPage)
        {
            UI.ClearRitualistVoice();
            UI.OnLidsClosed.RemoveListener(GoNextAfterTransition);
            UI.OnLidsClosed.AddListener(GoNextAfterTransition);
            UI.CloseLids();
        }
        else
        {
            GoToNextGameFlow();
        }
        
        if (activeEntry.Name != null)
        {
            HandleExitedNamedState(activeEntry.Name);
        }
    }

    private IEnumerator DelayedPounceSfx()
    {
        yield return new WaitForSeconds(0.3f);
        CatPounceSfx.Play();
        
        yield return new WaitForSeconds(3f);
        
        UI.RitualistVoice.color = Color.magenta;
        GoToNextGameFlow();
    }

    private void HandleEndState()
    {
        UI.ForceCloseLids();
    }

    private void GoNextAfterTransition()
    {
        UI.OnLidsOpened.RemoveListener(GoNextAfterTransition);
        UI.OnLidsClosed.RemoveListener(GoNextAfterTransition);

        GoToNextGameFlow();
    }

    public void TextLinkClicked(string linkID)
    {
        Debug.Log($"GameManager received link: {linkID}");
        switch (linkID)
        {
            case "misc_keyboard":
                GoToNamedGameFlow("misc_keyboard");
                break;
            case "qwerty":
            case "azerty":
            case "qwertz":
                SetKeyboard(linkID);
                GoToNamedGameFlow("l1_intro");
                break;
            case "start_game":
                StartGame();
                break;
            case "restart_game":
                InitGame();
                StartGame();
                break;
            default: Debug.LogError($"Unknown link ID: {linkID}");
                break;
        }
    }

    private void SetKeyboard(string keyboardType)
    {
        var keyboardID = keyboardType == "qwerty" ? 0 : keyboardType == "azerty" ? 1 : 2;
        foreach (var runeInput in RunesInputs)
        {
            runeInput.InitKeyboard(keyboardID);
        }
    }

    private void InitGame()
    {
        TargetRune.ClearLeft();
        TargetRune.ClearRight();
        
        Spell.Init(5+2*_levelNumber);
        Monster.SetActive(false);

        _gameTimer = 0f;
        _waitingOnBells = false;
        _lastSecond = -1f;
        _gameTimerSpeed = 0f;
        TimerText.color = Color.white;
        UpdateClock();
        
        if (_levelNumber >= 2)
        {
            Bells.gameObject.SetActive(true);
        }

        foreach (var c in Candles)
        {
            c.Light.color = Color.yellow;
            c.Ignite();
        }
    }
    
    private void StartGame()
    {
        Camera.main.transform.DOMove(GetCameraPosZoomedOut(false), 0.5f);
        UI.ClearRitualistVoice();
        
        foreach (var runeInput in RunesInputs)
        {
            runeInput.SetInputEnabled(true);
        }
        
        if (_levelNumber > 0)
        {
            Lighter.gameObject.SetActive(true);

            _runeCount = 0;
            Spell.OnRuneCompleted.RemoveListener(HandleCandleFizzles);
            Spell.OnRuneCompleted.AddListener(HandleCandleFizzles);
        }

        if (_levelNumber >= 2)
        {
            Bells.OnBellsRung.RemoveListener(HandleBellsRung);
            Bells.OnBellsRung.AddListener(HandleBellsRung);
            
            _bellCountdown = Random.Range(15f, 30f);
        }
        
        Spell.OnSuccess.RemoveListener(HandleSuccess);
        Spell.OnSuccess.AddListener(HandleSuccess);

        Spell.gameObject.SetActive(true);
        
        _gameTimer = 0f;
        _lastSecond = -1f;
        _gameTimerSpeed = 1f;
        TimerText.color = Color.white;
    }

    void RequestBellRinging(bool skipSpeedChange = false)
    {
        _waitingOnBells = true;
        BellRequestSfx.Play();
        Bells.SetBellsRequesting(true);
        if (!skipSpeedChange)
        {
            _gameTimerSpeed = 2.5f;
        }
    }

    void HandleBellsRung()
    {
        Debug.Log($"{_waitingOnBells}");
        if (_waitingOnBells)
        {
            _waitingOnBells = false;
            Bells.SetBellsRequesting(false);
            _gameTimerSpeed = 1f;

            _bellCountdown = Random.Range(15f, 30f);
        }
    }

    void Update()
    {
        if (DebugWin)
        {
            DebugWin = false;
            HandleSuccess();
        }
        
        if (_gameTimerSpeed > 0f)
        {
            _gameTimer += Time.deltaTime * _gameTimerSpeed;
            UpdateClock();

            if (_levelNumber >= 2 && !_waitingOnBells)
            {
                _bellCountdown -= Time.deltaTime;
                if (_bellCountdown <= 0f)
                {
                    RequestBellRinging();
                }
            }

            var currentSecond = Mathf.FloorToInt(_gameTimer);
            if (_lastSecond != currentSecond)
            {
                _lastSecond = currentSecond;
                var audioSrc = currentSecond % 2 == 0 ? TickSfx : TockSfx;
                audioSrc.Play();

                if (_gameTimer >= 90f)
                {
                    TimerText.color = currentSecond % 2 == 0 ? Color.white : Color.red;
                }
            }
            
            if (_gameTimer > 120f)
            {
                HandleFailure();
            }
        }
    }

    void UpdateClock()
    {
        int minutes = Mathf.FloorToInt(_gameTimer / 60f);
        int seconds = Mathf.FloorToInt(_gameTimer % 60f);

        string time = $"23:{58 + minutes}:{seconds:00}";
        TimerText.SetText(time);
    }

    void HandleCandleFizzles()
    {
        _runeCount++;
        if (_levelNumber < 3)
        {
            FizzleRandomCandle();
            if (Random.value < 0.5f)
            {
                FizzleRandomCandle();
            }
        }
        else if(_runeCount%3 == 0)
        {
            FizzleAllCandles();
        }
    }

    void FizzleRandomCandle()
    {
        var randomIndex = Random.Range(0, Candles.Length);
        Candle targetCandle = Candles[randomIndex];
        int i = 0;
        while (!targetCandle.IsLit && i < Candles.Length)
        {
            i++;
            targetCandle = Candles[(randomIndex + i) % Candles.Length];
        }

        if (!targetCandle.IsLit) return;

        targetCandle.FizzleOut();
    }

    void HandleLighterTutorial()
    {
        foreach (var c in Candles)
        {
            c.OnReignition.RemoveListener(HandleLighterTutorial);
        }
        
        Lighter.gameObject.SetActive(false);
        Lighter.ForceOff();

        CandleTutorialHintArrow.transform.DOKill();
        CandleTutorialHintArrow.SetActive(false);
        
        GoToNamedGameFlow("lighter_tutorial_complete");

    }

    void FizzleAllCandles()
    {
        foreach (var c in Candles)
        {
            c.FizzleOut();
        }
        Monster.SetActive(true);
    }

    private void HandleSuccess()
    {
        Bells.OnBellsRung.RemoveListener(HandleBellsRung);
        Spell.OnSuccess.RemoveListener(HandleSuccess);
        Spell.OnRuneCompleted.RemoveListener(HandleCandleFizzles);

        Bells.SetBellsRequesting(false);
        
        _gameTimerSpeed = 0f;
        _waitingOnBells = false;
        
        SuccessSfx.Play();
        
        foreach (var runeInput in RunesInputs)
        {
            runeInput.SetInputEnabled(false);
        }
        
        foreach (var c in Candles)
        {
            c.Ignite();
            c.Light.color = Color.yellow;
        }
        
        Lighter.gameObject.SetActive(false);
        
        _levelNumber++;
        GoToNamedGameFlow($"l{_levelNumber}_win");
    }

    private void HandleFailure()
    {
        Bells.OnBellsRung.RemoveListener(HandleBellsRung);
        Spell.OnSuccess.RemoveListener(HandleSuccess);
        Spell.OnRuneCompleted.RemoveListener(HandleCandleFizzles);

        Bells.SetBellsRequesting(false);
        
        _gameTimerSpeed = 0f;
        _waitingOnBells = false;
        
        string time = $"00:00:00";
        TimerText.SetText(time);
        
        FailureSfx.Play();
        
        foreach (var c in Candles)
        {
            c.Ignite();
            c.Light.color = Color.red;
        }
        
        GoToNamedGameFlow("game_over");
    }

    private void HandleEnteredNamedState(string stateName)
    {
        switch (stateName)
        {
            case "l1_intro":
            case "l2_intro":
            case "l3_intro":
            case "l4_intro":
                Camera.main.transform.localPosition = GetCameraPosZoomedOut(true);
                InitGame();
            break;
            case "rune_tutorial":
                foreach (var runeInput in RunesInputs)
                {
                    runeInput.SetInputEnabled(true);
                    runeInput.OnRuneConjured.RemoveListener(HandleRuneTutorialProgress);
                    runeInput.OnRuneConjured.AddListener(HandleRuneTutorialProgress);
                }
            break;
            case "lighter_tutorial":
                var tutorialCandle = Candles[5];
                    
                Spell.gameObject.SetActive(false);

                tutorialCandle.OnReignition.RemoveListener(HandleLighterTutorial);
                tutorialCandle.OnReignition.AddListener(HandleLighterTutorial);
                tutorialCandle.FizzleOut();
                
                CandleTutorialHintArrow.SetActive(true);
                CandleTutorialHintArrow.transform.DOMoveZ(2f, 1f).SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
                CandleTutorialHintArrow.transform.DOMoveX(-2f, 1f).SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Yoyo);

                Lighter.gameObject.SetActive(true);
                break;
            case "bells_tutorial":
                StartCoroutine(DelayedTutorialBells());
            break;
            case "kitten_spawn":
                TargetRune.gameObject.SetActive(false);
                Cat.gameObject.SetActive(true);
                Cat.SetState(0);
                foreach (var c in Candles)
                {
                    c.Light.color = Color.white;
                }
                CatLight.gameObject.SetActive(true);
                RuneInputParent.SetActive(false);
                CatSpawnSfx.Play();
                Spell.ClearRune();
                
                break;
            case "kitten_hungry":
                Cat.SetState(1);
                CatHungerSfx.Play();
                break;
            case "kitten_angry":
                Cat.SetState(2);
                CatAngrySfx.Play();
                break;
            case "kitten_pounce":
                // Cat.SetState(3);
                break;
            default: Debug.Log($"Entered state: {stateName}");
                break;
        }
    }

    private IEnumerator DelayedTutorialBells()
    {
        yield return new WaitForSeconds(3f);
        
        Bells.OnBellsRung.RemoveListener(HandleBellsTutorial);
        Bells.OnBellsRung.AddListener(HandleBellsTutorial);
        RequestBellRinging(true);
    }

    private void HandleBellsTutorial()
    {
        Bells.OnBellsRung.RemoveListener(HandleBellsTutorial);
        _waitingOnBells = false;
        Bells.SetBellsRequesting(false);
        
        GoToNamedGameFlow("bells_tutorial_complete");
    }

    private void HandleRuneTutorialProgress()
    {
        foreach (var runeInput in RunesInputs)
        {
            runeInput.OnRuneConjured.RemoveListener(HandleRuneTutorialProgress);
        }
        
        GoToNamedGameFlow("rune_tutorial_hint");

        RuneTutorialHintArrow.SetActive(true);
        RuneTutorialHintArrow.transform.DOMoveZ(2f, 1f).SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
        RuneTutorialHintArrow.transform.DOMoveX(-2f, 1f).SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
        
        Spell.OnRuneCompleted.RemoveListener(HandleRuneTutorialComplete);
        Spell.OnRuneCompleted.AddListener(HandleRuneTutorialComplete);
    }

    private void HandleRuneTutorialComplete()
    {
        Spell.OnRuneCompleted.RemoveListener(HandleRuneTutorialComplete);

        foreach (var runeInput in RunesInputs)
        {
            runeInput.SetInputEnabled(false);
        }

        RuneTutorialHintArrow.transform.DOKill();
        RuneTutorialHintArrow.SetActive(false);
        GoToNamedGameFlow("rune_tutorial_complete");
    }

    private Vector3 GetCameraPosZoomedOut(bool isZoomedOut)
    {
        return new Vector3(
            Camera.main.transform.localPosition.x,
            isZoomedOut? 23f : 16.26f,
            Camera.main.transform.localPosition.z);
    }
    
    private void HandleExitedNamedState(string stateName)
    {
        switch (stateName)
        {
            //TODO
            default: Debug.Log($"Exited state: {stateName}");
                break;
        }
    }
}
