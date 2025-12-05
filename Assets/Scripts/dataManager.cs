using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class dataManager : MonoBehaviour
{
    // Purpose of this mfstore song library, progression data (stars num to unlock songs, quest completed for each track), pass game-related info to gameplay scene and UI scene back and forth (UI --> gameplay: difficulty, modifiers, midiPath; gameplay --> UI: score, combo, accuracy, perfect count, great count, good count, okay count, miss count, )
    public List<SongData> SongLibrary = new List<SongData>();
    public int TotalStars = 0;
    private static dataManager Instance;
    int _songIndex;
    string _currentLevelDifficulty = "Easy";
    // only loads data to these variables in dataManager when stage is completed
    // all player stats info are reset upon clicking back to menu only when stage is completed
    public int SongIndex {
        get { return _songIndex; }
        set { _songIndex = value; }
    }

    public string CurrentLevelDifficulty { 
        get { return _currentLevelDifficulty; }
        set { _currentLevelDifficulty = value; }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { 
            Destroy(gameObject);
        }
    }

    public void recalculateTotalStars() {
        TotalStars = 0;
        foreach (SongData song in SongLibrary) {
            if (song.Objective1) {
                TotalStars++;
            }
            if (song.Objective2) {
                TotalStars++;
            }
            if (song.Objective3) { 
                TotalStars++;
            }
        }
    }
}
