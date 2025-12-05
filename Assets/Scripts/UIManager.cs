using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using NUnit.Framework;
using System.ComponentModel.Design;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
    /* Shits to do:
     * 1. create scripts for each of the button as they're prefabs and copy the prefab to different scenes as UI manager is pointing at prefab elements
     
     */
    public List<performanceRankSprite> performanceRankSpriteStorage = new List<performanceRankSprite>();
    // Retrieve UI screens obj
    public GameObject MainMenuUI, SongSelectionUI, DifficultySelectionUI, SettingsUI, StageFailedUI, StageClearedUI, PauseUI, InGameHUD;

    // Retrieve UI elements within mainMenu
    public Button MainMenuToSongSelectionButton, SetingsButton, ExitButton;

    // Retrieve UI elements within songSelection
    public GameObject SongContainer1, SongContainer2, SongContainer3, LockedLayerContainer1, LockedLayerContainer2, LockedLayerContainer3, ErrorMessage;
    public Image Image1, Image2, Image3;
    public TextMeshProUGUI TotalStars, SongLibraryProgressBarValue, SongName1, ArtistName1, Year1, Duration1, SongName2, ArtistName2, Year2, Duration2, SongName3, ArtistName3, Year3, Duration3;
    public Slider SongLibraryProgressBar;
    public Button NextSongButton, PreviousSongButton, SelectSongButton, SongSelectionToMainMenuButton;

    // Retrieve UI elements within difficultySelection
    public Image SelectedSongImage, EasyModeImage, MediumModeImage, HardModeImage, Objective1Image, Objective2Image, Objective3Image;
    public TextMeshProUGUI SelectedSongName, SelectedSongArtist, SelectedSongYear, SelectedSongDuration;
    public Button EasyModeButton, MediumModeButton, HardModeButton, PlayButton, DifficultySelectionToSongSelectionButton;

    // Retrieve UI elements within pauseMenu
    public Button PauseMenuResumeButton, PauseMenuRetryButton, QuitButton;

    // Retrieve UI elements within stageCleared
    public TextMeshProUGUI StageClearedSongName, StageClearedArtistName, Accuracy, Score, Combo, PerfectCount, GreatCount, GoodCount, OkayCount, MissCount;
    public Image StageClearedPerformanceRankImage, StageClearedObjective1Image, StageClearedObjective2Image, StageClearedObjective3Image;
    public Button StageClearedRetryButton, StageClearedToDifficultySelectionButton;

    // Retrieve UI elements within stageFailed
    public Button StageFailedRetryButton, StageFailedToDifficultySelectionButton;

    // Retrieve HUD elements within gameplay
    public Slider HPBar, FeverBar, ProgressBar;
    public TextMeshProUGUI ScoreDisplay, ComboDisplay, MultiplierDisplay, AccuracyDisplay;

    // retrieve UI elements within settingsUI
    public Slider VolumeSlider;

    // Retrieve scripts
    dataManager _dataManager;
    gameplayManager _gameplayManager;
    MidiParser _midiParser;
    playerHandler _playerHandler;

    // Singleton instance
    private static UIManager instance;
    private void Awake() {
        _dataManager = GameObject.Find("Data Manager").GetComponent<dataManager>();
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else { 
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialization of song selection progress bar
        SongLibraryProgressBar.maxValue = _dataManager.SongLibrary.Count;
        SongLibraryProgressBar.minValue = 1;

        // Initialization of settings volumeSlider's value
        VolumeSlider.value = AudioListener.volume;

        // Load main menu
        SceneManager.LoadScene("Hub");
        MainMenuUI.SetActive(true);

        // Event listeners for mainMenu buttons
        MainMenuToSongSelectionButton.onClick.AddListener(mainMenuPlayButtonPressed);
        SetingsButton.onClick.AddListener(settingsButtonPressed);
        ExitButton.onClick.AddListener(exitButtonPressed);

        // Event listeners for songSelection buttons
        NextSongButton.onClick.AddListener(nextSong);
        PreviousSongButton.onClick.AddListener(previousSong);
        SelectSongButton.onClick.AddListener(() => { StartCoroutine(selectSong()); });
        SongSelectionToMainMenuButton.onClick.AddListener(songSelectionToMainMenu);

        // Event listeners for difficultySelection buttons
        EasyModeButton.onClick.AddListener(easyModeSelected);
        MediumModeButton.onClick.AddListener(mediumModeSelected);
        HardModeButton.onClick.AddListener(hardModeSelected);
        PlayButton.onClick.AddListener(startLevel);
        DifficultySelectionToSongSelectionButton.onClick.AddListener(difficultySelectionToSongSelection);

        // Event listeners for pause buttons
        PauseMenuResumeButton.onClick.AddListener(pauseMenuResumeButtonPressed);
        PauseMenuRetryButton.onClick.AddListener(pauseMenuRetryButtonPressed);
        QuitButton.onClick.AddListener(pauseMenuToDifficultySelection);

        // Event listeners for stageFailed buttons
        StageFailedRetryButton.onClick.AddListener(stageFailedRetryButtonPressed);
        StageFailedToDifficultySelectionButton.onClick.AddListener(stageFailedToDifficultySelection);

        // Event listeners for stageCleared buttons
        StageClearedRetryButton.onClick.AddListener(stageClearedRetryButtonPressed);
        StageClearedToDifficultySelectionButton.onClick.AddListener(stageClearedToDifficultySelection);

        // Event listeners for settings volumeSlider
        VolumeSlider.onValueChanged.AddListener(updateVolume);

    }

    private void Update()
    {
        // Update in-game HUDs if in gameplay scene
        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            if (InGameHUD.activeSelf == false)
            {
                InGameHUD.SetActive(true);
            }
            hpBarUpdate();
            feverBarUpdate();
            progressBarUpdate();
            scoreDisplayUpdate();
            comboDisplayUpdate();
            multiplierDisplayUpdate();
            accuracyDisplayUpdate();
        }
    }
    // Interactions

    // Interactions within mainMenu
    public void mainMenuPlayButtonPressed() { // mainMenu --> songSelection
        MainMenuUI.SetActive(false);
        SongSelectionUI.SetActive(true);
        updateSongContainers();
        _dataManager.recalculateTotalStars();
        TotalStars.text = _dataManager.TotalStars.ToString();
    }
     
    public void settingsButtonPressed() { // mainMenu --> settingsUI
        MainMenuUI.SetActive(false);
        SettingsUI.SetActive(true);
    }

    public void exitButtonPressed() { // mainMenu --> exit
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }


    // Interactions within songSelection
    public void nextSong() {
        if (_dataManager.SongIndex < _dataManager.SongLibrary.Count - 1) {
            _dataManager.SongIndex++;
        }
        songIndexPanelUpdate();
        updateSongContainers();
    }

    public void previousSong() {
        if (_dataManager.SongIndex > 0)
        {
            _dataManager.SongIndex--;
        }
        songIndexPanelUpdate();
        updateSongContainers();
    }

    public void updateSongContainers() {
        if (_dataManager.SongIndex == 0)
        {
            if (LockedLayerContainer1.activeSelf) { 
                LockedLayerContainer1.SetActive(false);
            }
            SongContainer1.SetActive(false);
            // middle container
            Image2.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            Year2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            Duration2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(true);
                }
            }
            else {
                if (LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(false);
                }
            }
            // right container
            Image3.sprite = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongSprite;
            SongName3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongName;
            ArtistName3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].ArtistName;
            Year3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Year.ToString();
            Duration3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex + 1].StarsRequired)
            {
                if (!LockedLayerContainer3.activeSelf)
                {
                    LockedLayerContainer3.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer3.activeSelf)
                {
                    LockedLayerContainer3.SetActive(false);
                }
            }
        }
        else if (_dataManager.SongIndex == _dataManager.SongLibrary.Count - 1)
        {
            if (LockedLayerContainer3.activeSelf)
            {
                LockedLayerContainer3.SetActive(false);
            }
            SongContainer3.SetActive(false);
            // left container
            Image1.sprite = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongSprite;
            SongName1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongName;
            ArtistName1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].ArtistName;
            Year1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Year.ToString();
            Duration1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex - 1].StarsRequired)
            {
                if (!LockedLayerContainer1.activeSelf)
                {
                    LockedLayerContainer1.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer1.activeSelf)
                {
                    LockedLayerContainer1.SetActive(false);
                }
            }
            // middle container
            Image2.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            Year2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            Duration2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(false);
                }
            }
        }
        else if (_dataManager.SongIndex > 0 && _dataManager.SongIndex < _dataManager.SongLibrary.Count - 1)
        {
            if (!SongContainer1.activeSelf) {
                SongContainer1.SetActive(true);
            }
            if (!SongContainer3.activeSelf) {
                SongContainer3.SetActive(true);
            }
            // left container
            Image1.sprite = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongSprite;
            SongName1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongName;
            ArtistName1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].ArtistName;
            Year1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Year.ToString();
            Duration1.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex - 1].StarsRequired)
            {
                if (!LockedLayerContainer1.activeSelf)
                {
                    LockedLayerContainer1.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer1.activeSelf)
                {
                    LockedLayerContainer1.SetActive(false);
                }
            }
            // middle container
            Image2.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistName2.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            Year2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            Duration2.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer2.activeSelf)
                {
                    LockedLayerContainer2.SetActive(false);
                }
            }
            // right container
            Image3.sprite = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongSprite;
            SongName3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongName;
            ArtistName3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].ArtistName;
            Year3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Year.ToString();
            Duration3.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex + 1].StarsRequired)
            {
                if (!LockedLayerContainer3.activeSelf)
                {
                    LockedLayerContainer3.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainer3.activeSelf)
                {
                    LockedLayerContainer3.SetActive(false);
                }
            }
        }

    }

    public void songIndexPanelUpdate() {
        SongLibraryProgressBar.value = _dataManager.SongIndex + 1;
        SongLibraryProgressBarValue.text = SongLibraryProgressBar.value.ToString() + " / " + SongLibraryProgressBar.maxValue.ToString();
    }

    public void songSelectionToMainMenu() {
        MainMenuUI.SetActive(true);
        SongSelectionUI.SetActive(false);
    }
    public IEnumerator selectSong() // switch from song selection UI to selected song's difficulty selection UI
    {
        if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
        {
            if (!ErrorMessage.activeSelf)
            {
                ErrorMessage.SetActive(true);
                yield return new WaitForSeconds(1.5f);
                ErrorMessage.SetActive(false);
            }
        }
        else {
            SongSelectionUI.SetActive(false);
            DifficultySelectionUI.SetActive(true);
            displaySelectedSongContainer();
            easyModeSelected();
            displayObjectives();
        }
    }


    // Interactions within difficultySelection
    public void displaySelectedSongContainer()
    {
        SelectedSongImage.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
        SelectedSongName.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
        SelectedSongArtist.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
        SelectedSongYear.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
        SelectedSongDuration.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
    }
    public void easyModeSelected() // manipulate difficulty variable in gameManager script
    {
        if (_dataManager.CurrentLevelDifficulty != "Easy") {
            _dataManager.CurrentLevelDifficulty = "Easy";
            displayDifficulties();
        }
    }

    public void mediumModeSelected() {
        if (_dataManager.CurrentLevelDifficulty != "Medium") {
            _dataManager.CurrentLevelDifficulty = "Medium";
            displayDifficulties();
        }
    }

    public void hardModeSelected() {
        if (_dataManager.CurrentLevelDifficulty != "Hard") {
            _dataManager.CurrentLevelDifficulty = "Hard";
            displayDifficulties();
        }
    }

    public void displayDifficulties() {
        switch (_dataManager.CurrentLevelDifficulty) {
            case "Easy":
                if (EasyModeImage.color != Color.black)
                {
                    EasyModeImage.color = Color.black;
                }
                if (MediumModeImage.color != Color.white) {
                    MediumModeImage.color = Color.white;
                }
                if (HardModeImage.color != Color.white)
                {
                    HardModeImage.color = Color.white;
                }
                break;
            case "Medium":
                if (EasyModeImage.color != Color.white)
                {
                    EasyModeImage.color = Color.white;
                }
                if (MediumModeImage.color != Color.black)
                {
                    MediumModeImage.color = Color.black;
                }
                if (HardModeImage.color != Color.white)
                {
                    HardModeImage.color = Color.white;
                }
                break;
            case "Hard":
                if (EasyModeImage.color != Color.white)
                {
                    EasyModeImage.color = Color.white;
                }
                if (MediumModeImage.color != Color.white)
                {
                    MediumModeImage.color = Color.white;
                }
                if (HardModeImage.color != Color.black)
                {
                    HardModeImage.color = Color.black;
                }
                break;
        }
    }
    public void displayObjectives() {
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective1) {
            if (Objective1Image.color != Color.yellow) {
                Objective1Image.color = Color.yellow;
            }
        }
        else
        {
            if (Objective1Image.color != Color.white)
            {
                Objective1Image.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective2)
        {
            if (Objective2Image.color != Color.yellow)
            {
                Objective2Image.color = Color.yellow;
            }
        }
        else
        {
            if (Objective2Image.color != Color.white)
            {
                Objective2Image.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective3)
        {
            if (Objective3Image.color != Color.yellow)
            {
                Objective3Image.color = Color.yellow;
            }
        }
        else
        {
            if (Objective3Image.color != Color.white)
            {
                Objective3Image.color = Color.white;
            }
        }
    }

    public void difficultySelectionToSongSelection() { 
        DifficultySelectionUI.SetActive(false);
        SongSelectionUI.SetActive(true);
    }

    public void startLevel() { // start gameManager and read midi
        SceneManager.LoadScene("Gameplay");
        // retrieve scripts
        _gameplayManager = GameObject.Find("Game Manager").GetComponent<gameplayManager>();
        _playerHandler = GameObject.Find("Player").GetComponent<playerHandler>();
        _midiParser = GameObject.Find("Game Manager").GetComponent<MidiParser>();

        // pass data to scripts
        _gameplayManager.CurrentLevelDifficulty = _dataManager.CurrentLevelDifficulty;
        _gameplayManager.MidiFilePath = _dataManager.SongLibrary[_dataManager.SongIndex].MidiFilePath;
    }

    // Interactions within Pause
    public void pauseMenuOpen() { 
        // change _gameStarted from gameManager to false - stop deltatime from incrementing, stop player control, enemy control, note index increment
        // switch timescale to 0 to stop physics
        PauseUI.SetActive(true);
    }

    public void pauseMenuResumeButtonPressed() { 
        PauseUI.SetActive(false);
        _gameplayManager.ResumeGame();
    }

    public void pauseMenuRetryButtonPressed() {
        startLevel();
        PauseUI.SetActive(false);
    }

    public void quitButtonPressed() {
        SceneManager.LoadScene("Hub");
        DifficultySelectionUI.SetActive(true);
        displaySelectedSongContainer();
        displayObjectives();
        switch (_dataManager.CurrentLevelDifficulty)
        {
            case "Easy":
                easyModeSelected();
                break;
            case "Medium":
                mediumModeSelected();
                break;
            case "Hard":
                hardModeSelected();
                break;
        }
    }
    public void pauseMenuToDifficultySelection() {
        PauseUI.SetActive(false);
        quitButtonPressed();
    }
    // Interactions within StageFailed
    public void stageFailedMenuOpen() { 
        StageFailedUI.SetActive(true);
    }

    public void stageFailedRetryButtonPressed() {
        StageFailedUI.SetActive(false);
        startLevel();
    }
    
    public void stageFailedToDifficultySelection() {
        StageFailedUI.SetActive(false);
        quitButtonPressed();
    }


    // Interactions within stageCleared
    public void stageClearedMenuOpen() {
        // display performance stats
        // switch songImage sprite to a sprite stating the performance rank based on accuracy. Sprites are placed in graphics folder
        StageClearedSongName.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
        StageClearedArtistName.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
        if (_gameplayManager.Accuracy == 100f)
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankSS;
        }
        else if (_gameplayManager.Accuracy >= 90f)
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankS;
        } else if (_gameplayManager.Accuracy >= 80f)
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankA;
        }
        else if (_gameplayManager.Accuracy >= 70f)
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankB;
        }
        else if (_gameplayManager.Accuracy >= 60f)
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankC;
        }
        else
        {
            StageClearedPerformanceRankImage.sprite = performanceRankSpriteStorage[0].rankD;
        }
        Accuracy.text = _gameplayManager.Accuracy.ToString("F2") + " %";
        Score.text = _gameplayManager.TotalScore.ToString();
        Combo.text = _gameplayManager.HighestCombo.ToString();
        PerfectCount.text = _gameplayManager.PerfectCount.ToString();
        GreatCount.text = _gameplayManager.GreatCount.ToString();
        GoodCount.text = _gameplayManager.GoodCount.ToString();
        OkayCount.text = _gameplayManager.OkayCount.ToString();
        MissCount.text = _gameplayManager.MissCount.ToString();

        // display objectives
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective1)
        {
            if (StageClearedObjective1Image.color != Color.yellow)
            {
                StageClearedObjective1Image.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective1Image.color != Color.white)
            {
                StageClearedObjective1Image.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective2)
        {
            if (StageClearedObjective2Image.color != Color.yellow)
            {
                StageClearedObjective2Image.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective2Image.color != Color.white)
            {
                StageClearedObjective2Image.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective3)
        {
            if (StageClearedObjective3Image.color != Color.yellow)
            {
                StageClearedObjective3Image.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective3Image.color != Color.white)
            {
                StageClearedObjective3Image.color = Color.white;
            }
        }

        StageClearedUI.SetActive(true);
    }

    public void stageClearedRetryButtonPressed()
    {
        StageClearedUI.SetActive(false);
        startLevel();
    }

    public void stageClearedToDifficultySelection() {
        StageClearedUI.SetActive(false);
        quitButtonPressed();
    }

    // in-game HUDs
    public void hpBarUpdate() { // health, fever, track's progress
        HPBar.value = _playerHandler.Hp;
    }

    public void feverBarUpdate() { 
        FeverBar.value = _gameplayManager.FeverProgress;
    }

    public void progressBarUpdate() { 
        ProgressBar.value = _gameplayManager.Timer / _gameplayManager.TrackDuration * 100;
    }

    public void scoreDisplayUpdate() { 
        ScoreDisplay.text = _gameplayManager.TotalScore.ToString();
    }

    public void comboDisplayUpdate() {
        ComboDisplay.text = "x" + _gameplayManager.ComboCounter.ToString();
    }

    public void multiplierDisplayUpdate() { 
        MultiplierDisplay.text = "Multiplier: x" + _gameplayManager.ScoreMultiplier.ToString();
    }

    public void accuracyDisplayUpdate() { 
        AccuracyDisplay.text = "Accuracy: " + _gameplayManager.Accuracy.ToString("F2") + " %";
    }

    // Interactions within settingsUI
    public void updateVolume(float value) { 
        AudioListener.volume = value;
    }
}