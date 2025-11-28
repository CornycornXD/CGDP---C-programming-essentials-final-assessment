using System.Collections;
using UnityEngine;

public class gameplayManager : MonoBehaviour
{
    // Shits to do:
    /* 1. Track score - only add score when enemy is attacking, combo counter - done
     * 2. Manage fever mode (progress, duration) - done
     * 3. Handle game state (start, pause, end) - done
     * 4. Interface with other scripts (playerHandler, enemyBehaviour, UIManager)
     * 5. Read MIDI sync data to align game events with music
     * 6. Decrease player HP when outside of the area eclosed by the barriers per second
     * 7. Restore player HP when inside the area enclosed by the barriers per second and correct input timing
    
    */
    playerHandler _playerHandler;
    EnemyBehaviour _enemyBehaviour;
    MidiParser _midiParser;
    int _totalScore, _comboCounter, _currentTotalBeatToDodge, _noteIndex, _totalNotes;
    float _timer, _timeLeft, _noteInputOffset, _noteStartTime, _trackDuration, _feverProgress, _feverDuration, _feverRemainingDuration, _totalAccuracy, _noteAccuracy, _scoreToAdd;
    string _currentLevelDifficulty;
    bool _gameStarted, _feverModeActive, _loadNextNode;
    
    public MidiParser MidiParser {
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

    public string CurrentLevelDifficulty
    {
        get { return _currentLevelDifficulty; }
        set { _currentLevelDifficulty = value; }
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
    }
    void Start()
    {
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
        _trackDuration =  _midiParser.MidiNotes[_totalNotes - 1].StartTime + _midiParser.MidiNotes[_totalNotes - 1].Duration;
        _noteIndex = 0;
        _loadNextNode = true;

        // Start prelevel countdown
        StartCoroutine(prelevel());
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameStarted) {
            if (_timer <= _trackDuration)
            {
                _timer += Time.deltaTime;
                _timeLeft = _trackDuration - _timer; // can be used for UI display later
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
                    else {
                        _enemyBehaviour.Dashing = true;
                    }
                }
                if (_loadNextNode == false && _timer - _midiParser.MidiNotes[_noteIndex].StartTime > 0.070f) {
                    if (_noteIndex < _totalNotes - 1)
                    {
                        _noteIndex++;
                        if (_enemyBehaviour.ActionPatternAttacking)
                        {
                            _enemyBehaviour.Attacking = false;
                        }
                        else { 
                            _enemyBehaviour.Dashing = false;
                        }
                        _loadNextNode = true;
                    }
                }
                if (!_playerHandler.GetComponent<playerHandler>().ControlLock)
                {
                    if (_enemyBehaviour.Attacking == true)
                    {

                        if ((Input.GetKeyDown("Q") || Input.GetKeyDown("E") || (Input.GetKeyDown("Q") && Input.GetKeyDown("E"))) && _timer - _noteStartTime <= 0.060f)
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
                                // during enemy attack phase, enemy will successfully hit player upon early missed note
                                if (_noteInputOffset < -0.060)
                                {
                                    // this might belong to enemyBehaviour script instead
                                }
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
                                }
                                _comboCounter++;
                                if (!(_comboCounter < 10))
                                {
                                    if (_comboCounter < 20)
                                    {
                                        _scoreToAdd = _scoreToAdd * 1.2f;
                                    }
                                    else if (_comboCounter < 30)
                                    {
                                        _scoreToAdd = _scoreToAdd * 1.4f;
                                    }
                                    else if (_comboCounter < 40)
                                    {
                                        _scoreToAdd = _scoreToAdd * 1.6f;
                                    }
                                    else if (_comboCounter < 50)
                                    {
                                        _scoreToAdd = _scoreToAdd * 1.8f;
                                    }
                                    else
                                    {
                                        _scoreToAdd = _scoreToAdd * 2f;
                                    }
                                }
                                if (_feverProgress >= 100f)
                                {
                                    _feverModeActive = true;
                                }
                                if (_feverModeActive)
                                {
                                    _scoreToAdd *= 2f;
                                }
                                if (_playerHandler.Hp < 100) {
                                    _playerHandler.Hp += 1;
                                }
                            }
                        } else {
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
                            }
                        }
                        _totalScore += (int)_scoreToAdd;
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
            else {
                StageCompleted();
            }

        }

    }

    IEnumerator prelevel() { 
        yield return new WaitForSeconds(3f);
        _gameStarted = true;
    }


    public void StageFailed() {
        _gameStarted = false;
        Debug.Log("Stage Failed");
    }

    public void StageCompleted() {
        _gameStarted = false;
        Debug.Log("Stage Completed");
    }

    public void PauseAndUnpauseGame() { // to be called by UIManager when pause button is pressed - fix later
        _gameStarted = !_gameStarted;
        if (_gameStarted) {
            Debug.Log("Game Unpaused");
        }
        else
        {
            Debug.Log("Game Paused");
        }
    }
    
}
