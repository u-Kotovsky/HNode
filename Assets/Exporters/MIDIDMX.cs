using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Standards;
using UnityEngine;

// Requires MIDIDMX installed in a world
// https://github.com/micksam7/VRC-MIDIDMX
// Original code by Micca
// me@micksam7.com

//This code was ported from a non-unity project so it may be very slightly weird :)

public class MIDIDMX : IExporter
{
    public bool useEditorLog;
    public int channelLimit = 2048; //Limits the number of channels we scan through for MIDIDMX, so full range scans are kept to a minimum.

    byte[] midiData = new byte[16384];

    int bankStatus = 0;
    OutputDevice midiOutput;
    FileStream logStream;

    int midiUpdates = 0;
    int midiCatchup = 0;
    int midiScanPosition = 0;

    long midiUpdate = 0;

    //Gets a list of MIDI devices
    public ICollection<string> GetMidiDevices()
    {
        ICollection<string> tDevices = new List<string> { "(none)" };
        ICollection<OutputDevice> devices = OutputDevice.GetAll();

        foreach (OutputDevice device in devices)
        {
            tDevices.Add(device.Name);
        }

        return tDevices;
    }

    //Sets active midi device, connects immediately.
    //Returns false on failure
    public bool MidiConnectDevice(string device)
    {
        try
        {
            midiOutput = OutputDevice.GetByName(device);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a channel from a raw byte representation
    /// </summary>
    /// <param name="channelValue"></param>
    /// <param name="channel"></param>
    public void SerializeChannel(byte channelValue, int channel)
    {
    }

    /// <summary>
    /// Called at the start of each frame to reset any state.
    /// </summary>
    public void InitFrame()
    {
    }

    /// <summary>
    /// Called after all channels have been serialized for the current frame.
    /// Can be used to for example generate a CRC block area, or operate on multiple channels at once.
    /// </summary>
    /// <param name="pixels"></param>
    /// <param name="channelValues"></param>
    public void CompleteFrame(ref List<byte> channelValues)
    {
        if (midiOutput != null)
        {
            if (isMidiReady())
            {
                //Midi updates
                int midiUpdates = 0;
                for (int i = midiCatchup; i < channelValues.Count; i++)
                {
                    if ((channelValues[i] != midiData[i] || (i >= midiScanPosition && i < midiScanPosition + 10)) && i < channelLimit)
                    {
                        if (midiUpdates >= 100)
                        {
                            midiCatchup = i;
                            break;
                        }
                        midiData[i] = channelValues[i];

                        SendMidi(i, channelValues[i]);
                    }
                }

                if (midiUpdates < 100)
                {
                    midiCatchup = 0;
                }

                midiScanPosition += 10;
                if (midiScanPosition > channelLimit)
                {
                    midiScanPosition = 0;
                }

                midiWatchdog();
                midiUpdate = Stopwatch.GetTimestamp();
            }
            else
            {
                float midiTimeout = (float)(Stopwatch.GetTimestamp() - midiUpdate) / (float)Stopwatch.Frequency;
                if (midiTimeout > 1)
                {
                    midiCatchup = 0;

                    midiReset();

                    midiUpdate = Stopwatch.GetTimestamp();
                }
            }
        }
    }

    /// <summary>
    /// Called to initialize the exporter
    /// </summary>
    public void Construct()
    {
        midiReset();
    }

    public void Deconstruct()
    {
        if (midiOutput != null)
            midiOutput.Dispose();

        if (logStream != null)
            logStream.Close();
    }


    private void midiReset()
    {
        findVRCLog();
        ChangeBanks(0);
        midiKnock();
        midiWatchdog();
    }

    private void SendMidi(int channel, byte value)
    {
        int bank = (int)(channel / 2048);
        if (bankStatus != bank)
        {
            ChangeBanks(bank);
        }

        channel -= bank * 2048;

        if (channel < 1024)
        {
            int t = channel;
            NoteOnEvent noteOn = new NoteOnEvent();
            noteOn.Channel = (FourBitNumber)((t >> 6) & 0xF);
            noteOn.NoteNumber = (SevenBitNumber)(((t << 1) & 0x7F) + ((value >> 7) & 0x1));
            noteOn.Velocity = (SevenBitNumber)(value & 0x7F);
            midiOutput.SendEvent(noteOn);
        }
        else
        {
            int t = channel - 1024;
            NoteOffEvent noteOff = new NoteOffEvent();
            noteOff.Channel = (FourBitNumber)((t >> 6) & 0xF);
            noteOff.NoteNumber = (SevenBitNumber)(((t << 1) & 0x7F) + ((value >> 7) & 0x1));
            noteOff.Velocity = (SevenBitNumber)(value & 0x7F);
            midiOutput.SendEvent(noteOff);
        }
        midiUpdates++;
    }

    //Todo: Wait for callback to ensure the bank swap was triggered
    private void ChangeBanks(int bank)
    {
        bankStatus = bank;

        SendMidiControl(15, 127, bank);
        midiUpdates++;
    }

    //Are we good to send more midi data?
    private bool isMidiReady()
    {
        if (logStream == null)
        {
            findVRCLog();
            return false;
        }

        int length = (int)(logStream.Length - logStream.Position);

        //there's a better way for this but I'm sleepy and don't feel like parsing through C# documentation
        //listen I write too many languages as it is ok
        byte[] searchWord = { (byte)'M', (byte)'I', (byte)'D', (byte)'I', (byte)'R', (byte)'E', (byte)'A', (byte)'D', (byte)'Y', };

        if (length > 1)
        {
            int c;
            int i = 0;
            while ((c = logStream.ReadByte()) != -1)
            {
                if (c == searchWord[i])
                {
                    i++;

                    if (i >= searchWord.Length)
                    {
                        logStream.Position = logStream.Length - 1;
                        return true;
                    }
                }
            }
        }
        else
        {
            return false;
        }

        return false;
    }

    //Sends watchdog "packet"
    private void midiWatchdog()
    {
        if (midiOutput != null)
        {
            SendMidiControl(15, 127, 127);
        }
    }

    private void SendMidiControl(int channel, int control, int value)
    {
        ControlChangeEvent midiWD = new ControlChangeEvent();
        midiWD.Channel = (FourBitNumber)channel;
        midiWD.ControlNumber = (SevenBitNumber)control;
        midiWD.ControlValue = (SevenBitNumber)value;

        midiOutput.SendEvent(midiWD);
    }

    //Unlocks world receiver
    private void midiKnock()
    {
        if (midiOutput != null)
        {
            SendMidiControl(15, 127, 101);
            SendMidiControl(15, 127, 120);
            SendMidiControl(15, 127, 107);
        }
    }

    //Finds and opens VRC Log
    //Closes the stream if it's already open [will find latest file if needed]
    private void findVRCLog()
    {
        if (logStream != null)
        {
            logStream.Close();
            logStream = null;
        }

        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string[] logs = Directory.GetFiles(path + "\\..\\LocalLow\\VRChat\\VRChat", "output_log_*.txt", SearchOption.TopDirectoryOnly);
        if (logs.Length == 0) return;

        Array.Sort(logs);
        string log = logs[logs.Length - 1];

        //Editor!!
        if (useEditorLog)
        {
            log = path + "\\..\\Local\\Unity\\Editor\\Editor.log";
        }

        logStream = new FileStream(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        //forward to the end to wait on it
        logStream.Position = logStream.Length - 1;
    }
}