using UnityEngine;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
public class MidiParser : MonoBehaviour
{
    float _noteStartTime, _noteDuration;
    int _noteIndex;
    public List<MidiNoteData> MidiNotes = new List<MidiNoteData>();

    public void ReadMidi(string MidiPath) // Called by other scripts that has access to the MidiPath
    {
        _noteIndex = 0;
        MidiFile midiFile = MidiFile.Read(MidiPath);
        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();
        foreach (var note in notes)
        {
            _noteStartTime = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;
            _noteDuration = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Length, tempoMap).TotalSeconds;
            // Only counts two or more notes playing instantaneously at the same time as one note
            if (_noteIndex == 0 || (_noteIndex > 0 && _noteStartTime != MidiNotes[_noteIndex].StartTime)) {
                MidiNotes.Add(new MidiNoteData()
                {
                    StartTime = _noteStartTime,
                    Duration = _noteDuration, // Duration is redundant for instantaneous notes but kept for potential future use  
                });
                _noteIndex++;
            }
        }

        MidiNotes.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        
    }

}
