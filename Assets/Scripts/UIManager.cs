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

    // Retrieve UI screens obj
    public GameObject MainMenuUI, SongSelectionUI, DifficultySelectionUI, SettingsUI;

    // Retrieve UI elements within mainMenu
    public Button MainMenuToSongSelectionButton, SetingsButton, ExitButton;

    // Retrieve UI elements within songSelection
    public GameObject SongContainerLeft, SongContainerMiddle, SongContainerRight, LockedLayerContainerLeft, LockedLayerContainerMiddle, LockedLayerContainerRight, ErrorMessage;
    public Image SongSpriteLeft, SongSpriteMiddle, SongSpriteRight;
    public TextMeshProUGUI TotalStars, SongLibraryProgressBarValue, SongNameLeft, ArtistNameLeft, YearLeft, DurationLeft, SongNameMiddle, ArtistNameMiddle, YearMiddle, DurationMiddle, SongNameRight, ArtistNameRight, YearRight, DurationRight;
    public Slider SongLibraryProgressBar;
    public Button NextSongButton, PreviousSongButton, SelectSongButton, SongSelectionToMainMenuButton;

    // Retrieve UI elements within difficultySelection
    public Image SelectedSongSprite, EasyModeSprite, MediumModeSprite, HardModeSprite, Objective1Sprite, Objective2Sprite, Objective3Sprite;
    public TextMeshProUGUI SelectedSongName, SelectedSongArtist, SelectedSongYear, SelectedSongDuration;
    public Button EasyModeButton, MediumModeButton, HardModeButton, PlayButton, DifficultySelectionToSongSelectionButton;

    // retrieve UI elements within settingsUI
    public Slider VolumeSlider;

    // Retrieve scripts
    dataManager _dataManager;

    // Singleton instance
    private void Awake() {
        _dataManager = GameObject.Find("Data Manager").GetComponent<dataManager>();
    }

    private void Start()
    {
        // Initialization of song selection progress bar
        SongLibraryProgressBar.maxValue = _dataManager.SongLibrary.Count;
        SongLibraryProgressBar.minValue = 1;

        // Initialization of settings volumeSlider's value
        VolumeSlider.value = AudioListener.volume;

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

        // Event listeners for settings volumeSlider
        VolumeSlider.onValueChanged.AddListener(updateVolume);

        if (!_dataManager.ProgramStartFlag)
        {
            // Load difficulty selection UI when coming back from gameplay scene
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
        else { 
            // Load main menu UI at the start of the program
            MainMenuUI.SetActive(true);
            _dataManager.ProgramStartFlag = false;
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
            if (LockedLayerContainerLeft.activeSelf) { 
                LockedLayerContainerLeft.SetActive(false);
            }
            SongContainerLeft.SetActive(false);
            // middle container
            SongSpriteMiddle.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            YearMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            DurationMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(true);
                }
            }
            else {
                if (LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(false);
                }
            }
            // right container
            SongSpriteRight.sprite = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongSprite;
            SongNameRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongName;
            ArtistNameRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].ArtistName;
            YearRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Year.ToString();
            DurationRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex + 1].StarsRequired)
            {
                if (!LockedLayerContainerRight.activeSelf)
                {
                    LockedLayerContainerRight.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerRight.activeSelf)
                {
                    LockedLayerContainerRight.SetActive(false);
                }
            }
        }
        else if (_dataManager.SongIndex == _dataManager.SongLibrary.Count - 1)
        {
            if (LockedLayerContainerRight.activeSelf)
            {
                LockedLayerContainerRight.SetActive(false);
            }
            SongContainerRight.SetActive(false);
            // left container
            SongSpriteLeft.sprite = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongSprite;
            SongNameLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongName;
            ArtistNameLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].ArtistName;
            YearLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Year.ToString();
            DurationLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex - 1].StarsRequired)
            {
                if (!LockedLayerContainerLeft.activeSelf)
                {
                    LockedLayerContainerLeft.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerLeft.activeSelf)
                {
                    LockedLayerContainerLeft.SetActive(false);
                }
            }
            // middle container
            SongSpriteMiddle.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            YearMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            DurationMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(false);
                }
            }
        }
        else if (_dataManager.SongIndex > 0 && _dataManager.SongIndex < _dataManager.SongLibrary.Count - 1)
        {
            if (!SongContainerLeft.activeSelf) {
                SongContainerLeft.SetActive(true);
            }
            if (!SongContainerRight.activeSelf) {
                SongContainerRight.SetActive(true);
            }
            // left container
            SongSpriteLeft.sprite = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongSprite;
            SongNameLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].SongName;
            ArtistNameLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].ArtistName;
            YearLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Year.ToString();
            DurationLeft.text = _dataManager.SongLibrary[_dataManager.SongIndex - 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex - 1].StarsRequired)
            {
                if (!LockedLayerContainerLeft.activeSelf)
                {
                    LockedLayerContainerLeft.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerLeft.activeSelf)
                {
                    LockedLayerContainerLeft.SetActive(false);
                }
            }
            // middle container
            SongSpriteMiddle.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
            SongNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].SongName;
            ArtistNameMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].ArtistName;
            YearMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Year.ToString();
            DurationMiddle.text = _dataManager.SongLibrary[_dataManager.SongIndex].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex].StarsRequired)
            {
                if (!LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerMiddle.activeSelf)
                {
                    LockedLayerContainerMiddle.SetActive(false);
                }
            }
            // right container
            SongSpriteRight.sprite = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongSprite;
            SongNameRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].SongName;
            ArtistNameRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].ArtistName;
            YearRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Year.ToString();
            DurationRight.text = _dataManager.SongLibrary[_dataManager.SongIndex + 1].Duration;
            if (_dataManager.TotalStars < _dataManager.SongLibrary[_dataManager.SongIndex + 1].StarsRequired)
            {
                if (!LockedLayerContainerRight.activeSelf)
                {
                    LockedLayerContainerRight.SetActive(true);
                }
            }
            else
            {
                if (LockedLayerContainerRight.activeSelf)
                {
                    LockedLayerContainerRight.SetActive(false);
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
        SelectedSongSprite.sprite = _dataManager.SongLibrary[_dataManager.SongIndex].SongSprite;
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
                if (EasyModeSprite.color != Color.black)
                {
                    EasyModeSprite.color = Color.black;
                }
                if (MediumModeSprite.color != Color.white) {
                    MediumModeSprite.color = Color.white;
                }
                if (HardModeSprite.color != Color.white)
                {
                    HardModeSprite.color = Color.white;
                }
                break;
            case "Medium":
                if (EasyModeSprite.color != Color.white)
                {
                    EasyModeSprite.color = Color.white;
                }
                if (MediumModeSprite.color != Color.black)
                {
                    MediumModeSprite.color = Color.black;
                }
                if (HardModeSprite.color != Color.white)
                {
                    HardModeSprite.color = Color.white;
                }
                break;
            case "Hard":
                if (EasyModeSprite.color != Color.white)
                {
                    EasyModeSprite.color = Color.white;
                }
                if (MediumModeSprite.color != Color.white)
                {
                    MediumModeSprite.color = Color.white;
                }
                if (HardModeSprite.color != Color.black)
                {
                    HardModeSprite.color = Color.black;
                }
                break;
        }
    }
    public void displayObjectives() {
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective1) {
            if (Objective1Sprite.color != Color.yellow) {
                Objective1Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (Objective1Sprite.color != Color.white)
            {
                Objective1Sprite.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective2)
        {
            if (Objective2Sprite.color != Color.yellow)
            {
                Objective2Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (Objective2Sprite.color != Color.white)
            {
                Objective2Sprite.color = Color.white;
            }
        }
        if (_dataManager.SongLibrary[_dataManager.SongIndex].Objective3)
        {
            if (Objective3Sprite.color != Color.yellow)
            {
                Objective3Sprite.color = Color.yellow;
            }
        }
        else
        {
            if (Objective3Sprite.color != Color.white)
            {
                Objective3Sprite.color = Color.white;
            }
        }
    }

    public void difficultySelectionToSongSelection() { 
        DifficultySelectionUI.SetActive(false);
        SongSelectionUI.SetActive(true);
    }

    public void startLevel() {
        SceneManager.LoadScene("Gameplay");
    }

    // Interactions within settingsUI
    public void updateVolume(float value) { 
        AudioListener.volume = value;
    }
}