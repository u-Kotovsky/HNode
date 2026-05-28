using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AudioLink;
using TMPro;

public enum AudioLinkState
{
    Fail,
    Ok
}

public class GeneratorAudioLink : IDMXGenerator
{
    private static GeneratorAudioLink _instance;

    public static GeneratorAudioLink GetInstance()
    {
        return _instance;
    }
    
    private AudioLinkState _state = AudioLinkState.Ok;
    private string _errorMessage;
    private AudioLink.AudioLink _audioLink;
    private List<string> microphoneDevices = new List<string>();
    private int _currentMicrophoneDeviceIndex = 0;
    private string _currentDeviceName = null;
    
    public AudioLink.AudioLink AudioLink => _audioLink;
    
    private TMP_InputField deviceIndexInputfield;
    private TMP_InputField deviceNameInputfield;
    private TextMeshProUGUI errorMessageText;
    private RectTransform errorMessageRoot;

    private void SetState(AudioLinkState state, string errorMessage = null)
    {
        _state = state;
        if (errorMessage != null)
        {
            _errorMessage = errorMessage;
        }
    }

    private void ApplyAudioLink()
    {
        _audioLink.DisableAudioLink();
        _audioLink.DisableReadback();
        
        _audioLink.audioSource.Stop();
        _audioLink.audioSource.clip = null;
        
        _audioLink.EnableReadback();
        _audioLink.EnableAudioLink();
        
        _audioLink.audioSource.clip = Microphone.Start(_currentDeviceName, true, 10, 44100);
        _audioLink.audioSource.loop = true;
        _audioLink.audioSource.Play();
    }

    public void ConstructUserInterface(RectTransform rect)
    {
        if (_state == AudioLinkState.Fail)
        {
            //errorMessageRoot = Util.add
            errorMessageText = Util.AddText(rect, $"<color=red>{_errorMessage}</color>");
            Util.SetHeight(errorMessageText.rectTransform, 50);
            //Util.SetRectCenterStretch(errorMessageText.rectTransform);
            return;
        }
        
        // todo: load config from file
        
        deviceIndexInputfield = Util.AddInputField(rect, "Device index")
            .WithText(_currentDeviceName)
            .WithCallback((value) =>
            {
                microphoneDevices = Microphone.devices.ToList();
                
                if (int.TryParse(value, out var index))
                {
                    if (microphoneDevices.Count < index || index < 0)
                    {
                        Debug.LogError("index is out of range");
                        return;
                    }
                    
                    _currentMicrophoneDeviceIndex = index;
                    _currentDeviceName = microphoneDevices[_currentMicrophoneDeviceIndex];
                }
                
                deviceNameInputfield.text = _currentDeviceName;
                
                ApplyAudioLink();
            });

        deviceNameInputfield = Util.AddInputField(rect, "Device name (Read-only)");
        deviceNameInputfield.interactable = false;
    }

    public void DeconstructUserInterface()
    {
        
    }

    public void UpdateUserInterface()
    {
    }

    public void Construct()
    {
        // todo: place audiolink prefab in scene

        if (_instance != null)
        {
            SetState(AudioLinkState.Fail, "Another instance of GeneratorAudioLink is already active, this instance won't do anything");
            return;
        }
        
        _instance = this;
        SetState(AudioLinkState.Ok);

        _audioLink = Object.FindFirstObjectByType<AudioLink.AudioLink>();
        ApplyAudioLink();
    }

    public void Deconstruct()
    {
        // todo: delete audio link prefab

        _instance = null;

        if (_audioLink == null)
        {
            return;
        }
        
        _audioLink.DisableReadback();
        _audioLink.DisableAudioLink();
        _audioLink.audioSource.Stop();
    }

    public void GenerateDMX(ref List<byte> dmxData)
    {
        
    }
}
