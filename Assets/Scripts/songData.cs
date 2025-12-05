using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Song data", menuName = "Scriptable Objects/New song")]
public class SongData : ScriptableObject
{
    public Sprite SongSprite;
    public string SongName;
    public string ArtistName;
    public string MidiFilePath;
    public string Duration;
    public int Year;
    public int StarsRequired;
    public Boolean Objective1;
    public Boolean Objective2;
    public Boolean Objective3;
}
