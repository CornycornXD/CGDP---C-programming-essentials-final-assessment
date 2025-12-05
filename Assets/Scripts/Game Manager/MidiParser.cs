using UnityEngine;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using System.Linq;
using System;
public class MidiParser : MonoBehaviour
{
    float _noteStartTime, _noteDuration;
    int _noteIndex;
    public List<MidiNoteData> MidiNotes = new List<MidiNoteData>();

    public void ReadMidi(string MidiPath) // Called by other scripts that has access to the MidiPath
    {
        ConvertMidiToBytes();
        //_noteIndex = 0;

        //MidiFile _midiFile = MidiFile.Read(MidiPath);
        //var tempoMap = _midiFile.GetTempoMap();
        //var notes = _midiFile.GetNotes();
        //foreach (var note in notes)
        //{
        //    _noteStartTime = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;
        //    _noteDuration = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Length, tempoMap).TotalSeconds;
        //    // Only counts two or more notes playing instantaneously at the same time as one note
        //    if (_noteIndex == 0 || (_noteIndex > 0 && _noteStartTime != MidiNotes[_noteIndex].StartTime)) {
        //        MidiNotes.Add(new MidiNoteData()
        //        {
        //            StartTime = _noteStartTime,
        //            Duration = _noteDuration, // Duration is redundant for instantaneous notes but kept for potential future use  
        //        });
        //        _noteIndex++;
        //    }
        //}

        //MidiNotes.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

    }

    public string midiFileName = "130_Piano1"; // Name of your MIDI file without extension

    private void ConvertMidiToBytes()
    {
        //MidiFile md = MidiFile.Read(Path.Combine(Application.dataPath, "Resources", midiFileName + ".mid"));

        //byte[] midibytes = File.ReadAllBytes(Path.Combine(Application.dataPath, "Resources", midiFileName + ".mid"));
        //ByteArrayToFile(Path.Combine(Application.dataPath, "Resources", midiFileName + ".bytes"), midibytes);
        //ReadMifiFromBytes(midibytes);

        TextAsset midiTextAsset = Resources.Load<TextAsset>(midiFileName);
        Debug.Log(midiFileName + " | " + midiTextAsset.text);

        if (midiTextAsset != null)
        {
            // Access the raw bytes of the MIDI file
            byte[] midiBytes = midiTextAsset.bytes;
            ReadMifiFromBytes(midiBytes);

            Debug.Log($"MIDI file '{midiFileName}' loaded successfully. Size: {midiBytes.Length} bytes.");
        }
        else
        {
            Debug.LogError($"MIDI file '{midiFileName}' not found in Resources folder.");
        }
    }

    private void ReadMifiFromBytes(byte[] midiData)
    {
        try
        {
            using (var stream = new MemoryStream(midiData))
            {
                MidiFile _midiFile = MidiFile.Read(stream);

                var tempoMap = _midiFile.GetTempoMap();
                var notes = _midiFile.GetNotes();
                Debug.Log("Notes count: " + notes.Count());
                foreach (var note in notes)
                {
                    _noteStartTime = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;
                    _noteDuration = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Length, tempoMap).TotalSeconds;
                    if (_noteIndex == 0 || (_noteIndex > 0 && _noteStartTime != MidiNotes[_noteIndex - 1].StartTime))
                    {
                        MidiNotes.Add(new MidiNoteData()
                        {
                            StartTime = _noteStartTime,
                            Duration = _noteDuration, // Duration is redundant for instantaneous notes but kept for potential future use  
                        });
                        _noteIndex++;
                    }
                }

                MidiNotes.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                foreach (var midiNote in MidiNotes)
                {
                    Console.WriteLine($"Note Start Time: {midiNote.StartTime}, Duration: {midiNote.Duration}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading MIDI file: {ex.Message}");
        }
    }

    public bool ByteArrayToFile(string fileName, byte[] byteArray)
    {
        try
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught in process: {0}", ex);
            return false;
        }
    }

}
