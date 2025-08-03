using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    public enum Status : int
    {
        Disconnected, //Not connected to MIDI
        ConnectedWait, //Connected to MIDI, not connected to world.
        ConnectedSendingData, //Connected to world and sending data
    }

    public enum ControlCode : int
    {
        KnockStart = 101,
        KnockMiddle = 120,
        KnockFinish = 107,
        Watchdog = 127,
        Clear = 100,
        ChangeToBank0 = 0,
        ChangeToBank1 = 1,
        ChangeToBank2 = 2,
        ChangeToBank3 = 3,
        ChangeToBank4 = 4,
        ChangeToBank5 = 5,
        ChangeToBank6 = 6,
        ChangeToBank7 = 7,
    }

    const int maxChannels = 16384;
    public int channelsPerUpdate = 100; //KEEP THIS AT 100 until VRC fixes their buffers :)
    public int idleScanChannels = 10; //How many channels to send at a time during idle scans. Keep this low so we have bandwidth for actively changing channels.

    private List<byte> midiData = new List<byte>(maxChannels);

    private int bankStatus = 0;
    private OutputDevice midiOutput;
    private FileStream logStream;
    private int midiUpdates = 0;
    private int midiCatchup = 0;
    private int midiScanPosition = 0;
    private long midiLastUpdate = 0;

    /// <summary>
    /// Gets list of MIDI devices
    /// </summary>
    /// <returns>String List of devices</returns>
    public List<string> GetMidiDevices()
    {
        List<string> tDevices = new List<string> { "(none)" };
        ICollection<OutputDevice> devices = OutputDevice.GetAll();

        tDevices.AddRange(devices.Select(device => device.Name));

        return tDevices;
    }

    /// <summary>
    /// Sets active midi device, connects immediately.
    /// </summary>
    /// <param name="device">String name of device to connect to.</param>
    /// <returns>True if connected, false on any failure.</returns>
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
    /// Gets the status of the Midi Exporter
    /// </summary>
    /// <returns>
    /// Returns a MIDIDMX.Status indicating the status.
    /// 
    /// Status.Disconnected - No MIDI connection.
    /// Status.ConnectedWait - Connected to MIDI, waiting on a compatible world to connect to.
    /// Status.ConnectedSendingData - Connected to MIDI and actively sending data to a compatible world.
    /// </returns>
    public Status MidiStatus()
    {
        float midiTimeout = (float)(Stopwatch.GetTimestamp() - midiLastUpdate) / (float)Stopwatch.Frequency;

        if (midiOutput == null)
        {
            return Status.Disconnected;
        }
        else if (midiTimeout > 1)
        {
            return Status.ConnectedWait;
        }
        else
        {
            return Status.ConnectedSendingData;
        }
    }

    public void SerializeChannel(byte channelValue, int channel) { }

    public void InitFrame() { }

    public void CompleteFrame(ref List<byte> channelValues)
    {
        if (isMidiReady())
        {
            //Finds all channels that need data sent over
            //Sends only up to 
            midiUpdates = 0;
            for (int i = midiCatchup; i < channelValues.Count; i++)
            {
                if ((channelValues[i] != midiData[i] || (i >= midiScanPosition && i < midiScanPosition + idleScanChannels)) && i < channelLimit)
                {
                    if (midiUpdates >= channelsPerUpdate)
                    {
                        midiCatchup = i;
                        break;
                    }
                    midiData[i] = channelValues[i];

                    SendMidi(i, channelValues[i]);
                }
            }

            if (midiUpdates < channelsPerUpdate)
            {
                midiCatchup = 0;
            }

            midiScanPosition += idleScanChannels;
            if (midiScanPosition > channelLimit)
            {
                midiScanPosition = 0;
            }

            midiWatchdog();
            midiLastUpdate = Stopwatch.GetTimestamp();
        }
        else
        {
            float midiTimeout = (float)(Stopwatch.GetTimestamp() - midiLastUpdate) / (float)Stopwatch.Frequency;
            if (midiTimeout > 1)
            {
                Reset();

                midiLastUpdate = Stopwatch.GetTimestamp();
            }
        }
    }

    /// <summary>
    /// Called to initialize the exporter
    /// </summary>
    public void Construct()
    {
        Reset();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Deconstruct()
    {
        midiOutput?.Dispose();
        logStream?.Close();
    }

    /// <summary>
    /// Resets entire midi chain, clearing all data and beginning world knocking again.
    /// </summary>
    private void Reset()
    {
        midiData = new List<byte>(maxChannels);
        midiUpdates = 0;
        midiScanPosition = 0;
        midiCatchup = 0;

        findVRCLog();
        ChangeBanks(0);
        midiKnock();
        midiWatchdog();
    }

    /// <summary>
    /// Sends a DMX channel as MIDI
    /// </summary>
    /// <param name="channel">DMX channel number with universe added</param>
    /// <param name="value">DMX value</param>
    private void SendMidi(int channel, byte value)
    {
        if (midiOutput == null) return;

        int bank = (int)(channel / 2048);
        if (bankStatus != bank)
        {
            ChangeBanks(bank);
        }

        channel -= bank * 2048;

        if (channel < 1024)
        {
            Send18BitMessage<NoteOnEvent>(channel, value, 0);
        }
        else
        {
            Send18BitMessage<NoteOffEvent>(channel, value, 1024);
        }
        midiUpdates++;
    }

    private void Send18BitMessage<T>(int channel, byte channelvalue, int channeloffset) where T : NoteEvent, new()
    {
        int t = channel - channeloffset;
        T noteOff = new()
        {
            Channel = (FourBitNumber)((t >> 6) & 0xF),
            NoteNumber = (SevenBitNumber)(((t << 1) & 0x7F) + ((channelvalue >> 7) & 0x1)),
            Velocity = (SevenBitNumber)(channelvalue & 0x7F)
        };
        midiOutput.SendEvent(noteOff);
    }

    /// <summary>
    /// Sends a MIDI control signal
    /// </summary>
    /// <param name="channel">Channel, usually 15 for MIDIDMX</param>
    private void SendMidiControl(int channel, ControlCode code)
    {
        if (midiOutput == null) return;

        ControlChangeEvent midiWD = new ControlChangeEvent();
        midiWD.Channel = (FourBitNumber)channel;
        midiWD.ControlNumber = (SevenBitNumber)127; //constant magic number for control codes
        midiWD.ControlValue = (SevenBitNumber)(int)code;

        midiOutput.SendEvent(midiWD);
    }

    /// <summary>
    /// Changes data banks.
    /// A data bank is a set of 2048 channels.
    /// </summary>
    /// <param name="bank">Bank number from 0 to 7.</param>
    private void ChangeBanks(int bank)
    {
        bankStatus = bank;

        Mathf.Clamp(bank, (int)ControlCode.ChangeToBank0, (int)ControlCode.ChangeToBank7);

        SendMidiControl(15, (ControlCode)bank);
        midiUpdates++;
    }

    /// <summary>
    /// Checks if we're good to process and send MIDI data.
    /// This reads the VRC log file to ensure the world responded to our watchdog signal.
    /// A watchdog packet MUST be sent before checking this.
    /// </summary>
    /// <returns>True if ready for more data, false if we're not.</returns>
    private bool isMidiReady()
    {
        if (logStream == null)
        {
            findVRCLog();
            return false;
        }

        if (midiOutput == null)
        {
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

    /// <summary>
    /// Sends watchdog packet to the world.
    /// </summary>
    private void midiWatchdog()
    {
        SendMidiControl(15, ControlCode.Watchdog);
    }

    /// <summary>
    /// Sends a sequence of commands to 'knock' the world receiver into accepting data.
    /// MIDIDMX on the world side may not respond until this is done.
    /// </summary>
    private void midiKnock()
    {
        SendMidiControl(15, ControlCode.KnockStart);
        SendMidiControl(15, ControlCode.KnockMiddle);
        SendMidiControl(15, ControlCode.KnockFinish);
    }

    /// <summary>
    /// Finds the VRC log to watch for watchdog signals.
    /// This will close the stream and find the latest log available if called again.
    /// </summary>
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
