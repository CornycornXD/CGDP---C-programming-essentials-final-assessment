using System.Collections;
using UnityEngine;

public class gameplayManager : MonoBehaviour
{
    // Shits to do:
    /* 1. Track score - only add score when enemy is attacking, combo counter - done
     * 2. Manage fever mode (progress, duration) - done
     * 3. Handle game state (start, pause, end) - done
     * 4. Interface with other scripts (playerHandler, enemyBehaviour, UIManager) -done
     * 5. Read MIDI sync data to align game events with music -done
     * 6. Decrease player HP when outside of the area eclosed by the barriers per second - done
     * 7. Restore player HP when inside the area enclosed by the barriers per second and correct input timing -done
    
    */
    playerHandler _playerHandler;
    EnemyBehaviour _enemyBehaviour;
    MidiParser _midiParser;
    dataManager _dataManager;
    gameplayUIManager _gameplayUIManager;
    int _totalScore, _comboCounter, _currentTotalBeatToDodge, _noteIndex, _totalNotes, _highestCombo, _perfectCount, _greatCount, _goodCount, _okayCount, _missCount;
    float _timer, _noteInputOffset, _noteStartTime, _trackDuration, _feverProgress, _feverDuration, _feverRemainingDuration, _totalAccuracy, _noteAccuracy, _scoreToAdd, _comboMultiplier, _feverMultiplier, _scoreMultiplier;
    string _currentLevelDifficulty, _midiFilePath;
    bool _gameStarted, _feverModeActive, _loadNextNode;

    public MidiParser MidiParser
    {
        get { return _midiParser; }
    }
    public int TotalScore
    {
        get { return _totalScore; }
        set { _totalScore = value; }
    }
    public int ComboCounter
    {
        get { return _comboCounter; }
        set { _comboCounter = value; }
    }
    public int NoteIndex
    {
        get { return _noteIndex; }
        set { _noteIndex = value; }
    }
    public float Timer
    {
        get { return _timer; }
        set { _timer = value; }
    }
    public float TrackDuration
    {
        get { return _trackDuration; }
        set { _trackDuration = value; }
    }
    public int HighestCombo
    {
        get { return _highestCombo; }
        set { _highestCombo = value; }
    }
    public int PerfectCount
    {
        get { return _perfectCount; }
        set { _perfectCount = value; }
    }
    public int GreatCount
    {
        get { return _greatCount; }
        set { _greatCount = value; }
    }
    public int GoodCount
    {
        get { return _goodCount; }
        set { _goodCount = value; }
    }
    public int OkayCount
    {
        get { return _okayCount; }
        set { _okayCount = value; }
    }
    public int MissCount
    {
        get { return _missCount; }
        set { _missCount = value; }
    }

    public float NoteInputOffset
    {
        get { return _noteInputOffset; }
        set { _noteInputOffset = value; }
    }
    public float NoteStartTime
    {
        get { return _noteStartTime; }
        set { _noteStartTime = value; }
    }
    public float FeverProgress
    {
        get { return _feverProgress; }
        set { _feverProgress = value; }
    }
    public float FeverDuration
    {
        get { return _feverDuration; }
        set { _feverDuration = value; }
    }
    public float Accuracy
    {
        get { return _totalAccuracy; }
        set { _totalAccuracy = value; }
    }
    public float ScoreToAdd
    {
        get { return _scoreToAdd; }
        set { _scoreToAdd = value; }
    }
    public float ScoreMultiplier
    {
        get { return _scoreMultiplier; }
        set { _scoreMultiplier = value; }
    }

    public string CurrentLevelDifficulty
    {
        get { return _currentLevelDifficulty; }
        set { _currentLevelDifficulty = value; }
    }

    public string MidiFilePath
    {
        get { return _midiFilePath; }
        set { _midiFilePath = value; }
    }
    public bool GameStarted
    {
        get { return _gameStarted; }
        set { _gameStarted = value; }
    }

    public bool FeverModeActive
    {
        get { return _feverModeActive; }
        set { _feverModeActive = value; }
    }

    private void Awake()
    {
        // Get components/scripts
        _enemyBehaviour = GameObject.Find("Enemy").GetComponent<EnemyBehaviour>();
        _playerHandler = GameObject.Find("Player").GetComponent<playerHandler>();
        _midiParser = GameObject.Find("Game Manager").GetComponent<MidiParser>();
        _dataManager = GameObject.Find("Data Manager").GetComponent<dataManager>();
        _gameplayUIManager = GameObject.Find("Gameplay UI Manager").GetComponent<gameplayUIManager>();
    }
    void Start()

    { // load MIDI notes list data from the specified MIDI file path
        _midiParser.ReadMidi(_midiFilePath);

        // Initialise variables
        _timer = 0f;
        _totalScore = 0;
        _currentTotalBeatToDodge = 0;
        _feverProgress = 0f;
        _feverDuration = 30f;
        _feverRemainingDuration = _feverDuration;
        _feverModeActive = false;
        _gameStarted = false;
        _totalNotes = _midiParser.MidiNotes.Count;
        _trackDuration = _midiParser.MidiNotes[_totalNotes - 1].StartTime + _midiParser.MidiNotes[_totalNotes - 1].Duration;
        Debug.Log("Total ndoes: " + _totalNotes + " | " + "Start Time: " + _midiParser.MidiNotes[_totalNotes - 1].StartTime + " | Last Note Duration: " + _midiParser.MidiNotes[_totalNotes - 1].Duration + " | track duration: " + _trackDuration);
        _noteIndex = 0;
        _loadNextNode = true;
        _highestCombo = 0;
        _perfectCount = 0;
        _greatCount = 0;
        _goodCount = 0;
        _okayCount = 0;
        _missCount = 0;

        // Load data from dataManager
        _currentLevelDifficulty = _dataManager.CurrentLevelDifficulty;
        _midiFilePath = _dataManager.SongLibrary[_dataManager.SongIndex].MidiFilePath;

        // Start prelevel countdown
        StartCoroutine(prelevel());

    }

    // Update is called once per frame
    void Update()
    {
        if (_gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            if (_timer <= _trackDuration)
            {
                _timer += Time.deltaTime;
                // you need to read MIDI data to get the start time of each note first  - save them all to a list/array
                // get current note data from the list/array based on the timer
                // then assign _noteStartTime to the start time of the current note data
                // and increment _currentTotalBeatToDodge accordingly
                // get next note data after the input is analysed with the current note data (increment _noteIndex)
                // load next _noteStartTime for a specific time frame before the actual  note start time to allow for early input - this might need to be reconsidered later
                if (_loadNextNode == true && (_timer - _midiParser.MidiNotes[_noteIndex].StartTime >= -0.150f && _timer - _midiParser.MidiNotes[_noteIndex].StartTime <= -0.060f)) // work on this condition later as there can still be time gaps between notes despite the minus -0.80f for early input handling. E.g you can divide note start time of 2 notes by 2 or smth involves the duration of one and the note start time of another. This can actually be handled by switching enemy's state to chasing during the time gap
                {
                    _noteStartTime = _midiParser.MidiNotes[_noteIndex].StartTime;
                    _loadNextNode = false;
                    // only increment total beat to dodge when enemy is attacking
                    if (_enemyBehaviour.ActionPatternAttacking)
                    {
                        _enemyBehaviour.Attacking = true;
                        _currentTotalBeatToDodge++;
                    }
                    else
                    {
                        _enemyBehaviour.Dashing = true;
                    }
                }
                if (_loadNextNode == false && _timer - _midiParser.MidiNotes[_noteIndex].StartTime > 0.070f)
                {
                    if (_noteIndex < _totalNotes - 1)
                    {
                        _noteIndex++;
                        if (_enemyBehaviour.ActionPatternAttacking)
                        {
                            _enemyBehaviour.Attacking = false;
                        }
                        else
                        {
                            _enemyBehaviour.Dashing = false;
                        }
                        _loadNextNode = true;
                    }
                }
                if (!_playerHandler.GetComponent<playerHandler>().ControlLock)
                {
                    if (_enemyBehaviour.Attacking == true)
                    {

                        if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.Q) && Input.GetKeyDown(KeyCode.E))) && _timer - _noteStartTime <= 0.060f)
                        {
                            _scoreToAdd = 0f;
                            _noteInputOffset = _timer - _noteStartTime;
                            if (_noteInputOffset < -0.060f)
                            {
                                // Missed note - early
                                Debug.Log("Miss"); // replace with UI feedback later
                                _noteAccuracy = 0f;
                                _comboCounter = 0;
                                if (_feverModeActive)
                                {
                                    _feverRemainingDuration -= 8f;
                                }
                                else
                                {
                                    _feverProgress = 0f;
                                }
                                _missCount++;
                            }
                            else
                            {
                                if (_noteInputOffset <= 0.030f && _noteInputOffset >= -0.030f)
                                {
                                    Debug.Log("Perfect"); // replace with UI feedback later
                                    _scoreToAdd = 500;
                                    _noteAccuracy = 100f;
                                    // fever progress different based on difficulty per accuracy threshold
                                    if (_feverProgress < 100f)
                                    {
                                        switch (_currentLevelDifficulty)
                                        {
                                            case "Easy":
                                                _feverProgress += 15f;
                                                break;
                                            case "Medium":
                                                _feverProgress += 10f;
                                                break;

                                            case "Hard":
                                                _feverProgress += 5f;
                                                break;
                                        }
                                    }
                                    _perfectCount++;
                                }
                                else if (_noteInputOffset <= 0.040f || _noteInputOffset >= -0.040f)
                                {
                                    Debug.Log("Great"); // replace with UI feedback later
                                    _scoreToAdd = 200;
                                    _noteAccuracy = 75f;
                                    if (_feverProgress < 100f)
                                    {
                                        switch (_currentLevelDifficulty)
                                        {
                                            case "Easy":
                                                _feverProgress += 6f;
                                                break;
                                            case "Medium":
                                                _feverProgress += 3f;
                                                break;

                                            case "Hard":
                                                _feverProgress += 1.5f;
                                                break;
                                        }
                                    }
                                    _greatCount++;
                                }
                                else if (_noteInputOffset <= 0.050f || _noteInputOffset >= -0.050f)
                                {
                                    Debug.Log("Good"); // replace with UI feedback later
                                    _scoreToAdd = 100;
                                    _noteAccuracy = 50f;
                                    if (_feverProgress < 100f)
                                    {
                                        switch (_currentLevelDifficulty)
                                        {
                                            case "Easy":
                                                _feverProgress += 3f;
                                                break;
                                            case "Medium":
                                                _feverProgress += 1.5f;
                                                break;

                                            case "Hard":
                                                _feverProgress += 0.75f;
                                                break;
                                        }
                                    }
                                    _goodCount++;
                                }
                                else if (_noteInputOffset <= 0.060f || _noteInputOffset >= -0.060f)
                                {
                                    Debug.Log("Okay"); // replace with UI feedback later
                                    _scoreToAdd = 50;
                                    _noteAccuracy = 25f;
                                    if (_feverProgress < 100f)
                                    {
                                        switch (_currentLevelDifficulty)
                                        {
                                            case "Easy":
                                                _feverProgress += 1f;
                                                break;
                                            case "Medium":
                                                _feverProgress += 0.5f;
                                                break;

                                            case "Hard":
                                                _feverProgress += 0.25f;
                                                break;
                                        }
                                    }
                                    _okayCount++;
                                }
                                _comboCounter++;
                                if (_comboCounter > _highestCombo)
                                {
                                    _highestCombo = _comboCounter;
                                }
                                if (!(_comboCounter < 10))
                                {
                                    if (_comboCounter < 20)
                                    {
                                        _comboMultiplier = 1.2f;
                                    }
                                    else if (_comboCounter < 30)
                                    {
                                        _comboMultiplier = 1.4f;
                                    }
                                    else if (_comboCounter < 40)
                                    {
                                        _comboMultiplier = 1.6f;
                                    }
                                    else if (_comboCounter < 50)
                                    {
                                        _comboMultiplier = 1.8f;
                                    }
                                    else
                                    {
                                        _comboMultiplier = 2f;
                                    }
                                }
                                else
                                {
                                    _comboMultiplier = 1f;
                                }
                                if (_feverProgress >= 100f)
                                {
                                    _feverModeActive = true;
                                }
                                if (_feverModeActive)
                                {
                                    _feverMultiplier = 2f;
                                }
                                if (_playerHandler.Hp < 100)
                                {
                                    _playerHandler.Hp += 1;
                                    if (_playerHandler.Hp > 100)
                                    {
                                        _playerHandler.Hp = 100;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Missed note - late
                            Debug.Log("Miss"); // replace with UI feedback later
                            _noteAccuracy = 0f;
                            _comboCounter = 0;
                            if (_feverModeActive)
                            {
                                _feverRemainingDuration -= 8f;
                            }
                            else
                            {
                                _feverProgress = 0f;
                                _feverMultiplier = 1f;
                            }
                            _missCount++;
                        }
                        _scoreMultiplier = _comboMultiplier * _feverMultiplier;
                        _totalScore += (int)(_scoreToAdd * _scoreMultiplier);
                        _totalAccuracy = (_totalAccuracy * (_currentTotalBeatToDodge - 1) + _noteAccuracy) / _currentTotalBeatToDodge;
                    }
                }
                if (_feverModeActive)
                {
                    _feverRemainingDuration -= Time.deltaTime;
                    if (_feverRemainingDuration <= 0f)
                    {
                        _feverModeActive = false;
                        _feverRemainingDuration = _feverDuration;
                        _feverProgress = 0f;
                    }
                }
            }
            else
            {
                if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective1 != true)
                {
                    _dataManager.SongLibrary[_dataManager.SongIndex].Objective1 = true;
                }
                if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective2 != true && _totalAccuracy >= 75f && _currentLevelDifficulty == "Medium")
                {
                    _dataManager.SongLibrary[_dataManager.SongIndex].Objective2 = true;
                }
                if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective3 != true && _playerHandler.NoDamageReceived == true && _currentLevelDifficulty == "Hard")
                {
                    _dataManager.SongLibrary[_dataManager.SongIndex].Objective3 = true;
                }
                StageComplete();
            }

        }

    }

    IEnumerator prelevel()
    {
        yield return new WaitForSeconds(3f);
        _gameStarted = true;
    }


    public void StageFailed()
    {
        _gameStarted = false;
        Time.timeScale = 0f;
        Debug.Log("Stage Failed");
        _gameplayUIManager.stageFailedMenuOpen();
    }

    public void StageComplete()
    {
        _gameStarted = false;
        Time.timeScale = 0f;
        Debug.Log("Stage Completed");
        _gameplayUIManager.stageClearedMenuOpen();
    }

    public void PauseGame()
    {
        _gameStarted = false;
        Debug.Log("Game Paused");
        _gameplayUIManager.pauseMenuOpen();
        Time.timeScale = 0f;

    }

    public void ResumeGame()
    {
        _gameStarted = true;
        Debug.Log("Game Unpaused");
        Time.timeScale = 1f;
    }

}
