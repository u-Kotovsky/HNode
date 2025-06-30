using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    List<IDMXSerializer> serializers;
    public TMP_Dropdown serializerDropdown;
    public TMP_Dropdown deserializerDropdown;
    public Toggle transcodeToggle;
    public IDMXSerializer currentSerializer;
    public IDMXSerializer currentDeserializer;
    
    public bool Transcode = true;
    private const string serializerIndexKey = "SelectedSerializer";
    private const string deserializerIndexKey = "SelectedDeserializer";
    public string transcodeKey = "Transcode";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //load in all the serializers
        var type = typeof(IDMXSerializer);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p));

        serializers = types
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Select(t => (IDMXSerializer)Activator.CreateInstance(t))
            .ToList();

        Debug.Log($"Loaded {serializers.Count} serializers");

        //populate the dropdown
        serializerDropdown.ClearOptions();
        deserializerDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var typei in serializers)
        {
            options.Add(typei.GetType().Name);
        }

        serializerDropdown.AddOptions(options);
        deserializerDropdown.AddOptions(options);

        serializerDropdown.onValueChanged.AddListener((s) =>
        {
            currentSerializer = serializers[s];
            Debug.Log($"Selected serializer: {currentSerializer.GetType().Name}");
            PlayerPrefs.SetString(serializerIndexKey, currentSerializer.GetType().Name);
        });

        deserializerDropdown.onValueChanged.AddListener((s) =>
        {
            currentDeserializer = serializers[s];
            Debug.Log($"Selected deserializer: {serializers[s].GetType().Name}");
            PlayerPrefs.SetString(deserializerIndexKey, serializers[s].GetType().Name);
        });

        //select the first serializer by default
        if (serializers.Count > 0)
        {
            int prefSavedIndexSerializer = loadPlayerPref(serializerIndexKey);
            int prefSavedIndexDeserializer = loadPlayerPref(deserializerIndexKey);

            currentSerializer = serializers[prefSavedIndexSerializer];
            serializerDropdown.value = prefSavedIndexSerializer;
            serializerDropdown.RefreshShownValue();
            currentDeserializer = serializers[prefSavedIndexDeserializer];
            deserializerDropdown.value = prefSavedIndexDeserializer;
            deserializerDropdown.RefreshShownValue();
            Debug.Log($"Default serializer: {currentSerializer.GetType().Name}");
            Debug.Log($"Default deserializer: {currentDeserializer.GetType().Name}");
        }
        else
        {
            Debug.LogError("No serializers found!");
        }

        bool transcodeValue = PlayerPrefs.GetInt(transcodeKey, 0) == 1;
        transcodeToggle.isOn = transcodeValue;
        Transcode = transcodeValue;

        //setup callback
        transcodeToggle.onValueChanged.AddListener((value) =>
        {
            Transcode = value;
            PlayerPrefs.SetInt(transcodeKey, value ? 1 : 0);
        });
    }

    private int loadPlayerPref(string key)
    {
        string prefSavedName = PlayerPrefs.GetString(key, "0");
        int prefSavedIndex = 0;
        //check if a type exists with the name
        if (serializers.Any(s => s.GetType().Name == prefSavedName))
        {
            prefSavedIndex = serializers.FindIndex(s => s.GetType().Name == prefSavedName);
            Debug.Log($"Found saved serializer: {prefSavedName} at index {prefSavedIndex}");
        }
        else
        {
            Debug.LogWarning($"No serializer found with name {prefSavedName}, using index 0 instead.");
        }

        return prefSavedIndex;
    }
}
