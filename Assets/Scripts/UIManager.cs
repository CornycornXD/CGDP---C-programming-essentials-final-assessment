using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _mainMenuUI;
    [SerializeField] GameObject _songSelectionUI;
    [SerializeField] GameObject _difficultySelectionUI;
    [SerializeField] GameObject _settingsUI;
    [SerializeField] GameObject _stageFailedUI;
    [SerializeField] GameObject _stageClearedUI;
    [SerializeField] GameObject _pauseUI;
    GameObject _previousScreen;

    // 
    public void playButtonPressed() { // switch from main menu to song selectionUI
        _previousScreen = _mainMenuUI;
        _mainMenuUI.SetActive(false);
        _songSelectionUI.SetActive(true);
    }

    public void settingsButtonPressed() {
        _previousScreen = _mainMenuUI;
        _mainMenuUI.SetActive(false);
        _settingsUI.SetActive(true);
    }
    public void selectSong() // switch from song selection UI to selected song's difficulty selection UI
    {
        _previousScreen = _songSelectionUI;
        _songSelectionUI.SetActive(false);
        _difficultySelectionUI.SetActive(true);
        // extract midi file path attached to the selected game obj
    }

    public void selectDifficulty() // manipulate difficulty variable in gameManager script
    {

    }

    public void playMap() { // start gameManager and read midi
        
    }
    public void pauseGame() { 
        // change _gameStarted from gameManager to false - stop deltatime from incrementing, stop player control, enemy control, note index increment
        // switch timescale to 0 to stop physics
        _pauseUI.SetActive(true);
    }

    public void resumeGame() { 
        _pauseUI.SetActive(false);
    }

    public void gameOver() { 
        _stageFailedUI.SetActive(true);
    }

    public void displayResult() { 
        _stageClearedUI.SetActive(false);
    }

    // in-game HUDs
    public void sliderUpdate() { // health, fever, track's progress
        
    }

}
