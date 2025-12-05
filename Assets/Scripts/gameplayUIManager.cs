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

public class gameplayUIManager : MonoBehaviour
{
    public List<performanceRankSprite> performanceRankSpriteStorage = new List<performanceRankSprite>();

    // Retrieve main UI screen
    public GameObject StageFailedUI, StageClearedUI, PauseUI, InGameHUD;

    // Retrieve UI elements within pauseMenu
    public Button PauseMenuResumeButton, PauseMenuRetryButton, QuitButton;

    // Retrieve UI elements within stageCleared
    public TextMeshProUGUI StageClearedSongName, StageClearedArtistName, Accuracy, Score, Combo, PerfectCount, GreatCount, GoodCount, OkayCount, MissCount;
    public Image StageClearedPerformanceRankSprite, StageClearedObjective1Sprite, StageClearedObjective2Sprite, StageClearedObjective3Sprite;
    public Button StageClearedRetryButton, StageClearedToDifficultySelectionButton;

    // Retrieve UI elements within stageFailed
    public Button StageFailedRetryButton, StageFailedToDifficultySelectionButton;

    // Retrieve HUD elements within gameplay
    public Slider HPBar, FeverBar, ProgressBar;
    public TextMeshProUGUI ScoreDisplay, ComboDisplay, MultiplierDisplay, AccuracyDisplay;


    dataManager _dataManager;
    gameplayManager _gameplayManager;
    playerHandler _playerHandler;


    private void Awake()
    {
        _gameplayManager = GameObject.Find("Game Manager").GetComponent<gameplayManager>();
        _playerHandler = GameObject.Find("Player").GetComponent<playerHandler>();
        _dataManager = GameObject.Find("Data Manager").GetComponent<dataManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _gameplayManager.CurrentLevelDifficulty = _dataManager.CurrentLevelDifficulty;
        _gameplayManager.MidiFilePath = _dataManager.SongLibrary[_dataManager.SongIndex].MidiFilePath;

        // buttons event listeners
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

        
    }

    // Update is called once per frame
    void Update()
    {
        if (InGameHUD.activeSelf == false)
        {
            InGameHUD.SetActive(true);
        }
        if (_gameplayManager.GameStarted) {
            hpBarUpdate();
            feverBarUpdate();
            progressBarUpdate();
            scoreDisplayUpdate();
            comboDisplayUpdate();
            multiplierDisplayUpdate();
            accuracyDisplayUpdate();
        }
    }

    // Interactions within Pause
    public void pauseMenuOpen()
    {
        // change _gameStarted from gameManager to false - stop deltatime from incrementing, stop player control, enemy control, note index increment
        // switch timescale to 0 to stop physics
        PauseUI.SetActive(true);
    }

    public void pauseMenuResumeButtonPressed()
    {
        PauseUI.SetActive(false);
        _gameplayManager.ResumeGame();
    }

    public void pauseMenuRetryButtonPressed()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void quitButtonPressed()
    {
        SceneManager.LoadScene("Hub");

    }
    public void pauseMenuToDifficultySelection()
    {
        PauseUI.SetActive(false);
        quitButtonPressed();
    }
    // Interactions within StageFailed
    public void stageFailedMenuOpen()
    {
        StageFailedUI.SetActive(true);
    }

    public void stageFailedRetryButtonPressed()
    {
        StageFailedUI.SetActive(false);
        SceneManager.LoadScene("Gameplay");
    }

    public void stageFailedToDifficultySelection()
    {
        StageFailedUI.SetActive(false);
        quitButtonPressed();
    }


    // Interactions within stageCleared
    public void stageClearedMenuOpen()
    {
        // display performance stats
        // switch songImage sprite to a sprite stating the performance rank based on accuracy. Sprites are placed in graphics folder
        StageClearedSongName.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
        StageClearedArtistName.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
        if (_gameplayManager.Accuracy == 100f)
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankSS;
        }
        else if (_gameplayManager.Accuracy >= 90f)
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankS;
        }
        else if (_gameplayManager.Accuracy >= 80f)
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankA;
        }
        else if (_gameplayManager.Accuracy >= 70f)
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankB;
        }
        else if (_gameplayManager.Accuracy >= 60f)
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankC;
        }
        else
        {
            StageClearedPerformanceRankSprite.sprite = performanceRankSpriteStorage[0].rankD;
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
            if (StageClearedObjective1Sprite.color != Color.yellow)
            {
                StageClearedObjective1Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective1Sprite.color != Color.white)
            {
                StageClearedObjective1Sprite.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective2)
        {
            if (StageClearedObjective2Sprite.color != Color.yellow)
            {
                StageClearedObjective2Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective2Sprite.color != Color.white)
            {
                StageClearedObjective2Sprite.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective3)
        {
            if (StageClearedObjective3Sprite.color != Color.yellow)
            {
                StageClearedObjective3Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (StageClearedObjective3Sprite.color != Color.white)
            {
                StageClearedObjective3Sprite.color = Color.white;
            }
        }

        StageClearedUI.SetActive(true);
    }

    public void stageClearedRetryButtonPressed()
    {
        StageClearedUI.SetActive(false);
        // reset scene
        SceneManager.LoadScene("Gameplay");
    }

    public void stageClearedToDifficultySelection()
    {
        StageClearedUI.SetActive(false);
        quitButtonPressed();
    }

    // in-game HUDs
    public void hpBarUpdate()
    { // health, fever, track's progress
        HPBar.value = _playerHandler.Hp;
    }

    public void feverBarUpdate()
    {
        FeverBar.value = _gameplayManager.FeverProgress;
    }

    public void progressBarUpdate()
    {
        ProgressBar.value = _gameplayManager.Timer / _gameplayManager.TrackDuration * 100;
    }

    public void scoreDisplayUpdate()
    {
        ScoreDisplay.text = _gameplayManager.TotalScore.ToString();
    }

    public void comboDisplayUpdate()
    {
        ComboDisplay.text = "x" + _gameplayManager.ComboCounter.ToString();
    }

    public void multiplierDisplayUpdate()
    {
        MultiplierDisplay.text = "Multiplier: x" + _gameplayManager.ScoreMultiplier.ToString();
    }

    public void accuracyDisplayUpdate()
    {
        AccuracyDisplay.text = "Accuracy: " + _gameplayManager.Accuracy.ToString("F2") + " %";
    }
}
