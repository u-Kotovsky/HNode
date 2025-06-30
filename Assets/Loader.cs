using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Loader : MonoBehaviour
{
    List<IDMXSerializer> serializers;
    public TMP_Dropdown serializerDropdown;
    public IDMXSerializer currentSerializer;
    private const string indexKey = "SelectedSerializer";

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
        List<string> options = new List<string>();
        foreach (var typei in serializers)
        {
            options.Add(typei.GetType().Name);
        }

        serializerDropdown.AddOptions(options);

        serializerDropdown.onValueChanged.AddListener((s) =>
        {
            currentSerializer = serializers[s];
            Debug.Log($"Selected serializer: {currentSerializer.GetType().Name}");
            PlayerPrefs.SetString(indexKey, currentSerializer.GetType().Name);
        });

        //select the first serializer by default
        if (serializers.Count > 0)
        {
            string prefSavedName = PlayerPrefs.GetString(indexKey, "0");
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

            currentSerializer = serializers[prefSavedIndex];
            serializerDropdown.value = prefSavedIndex;
            serializerDropdown.RefreshShownValue();
            Debug.Log($"Default serializer: {currentSerializer.GetType().Name}");
        }
        else
        {
            Debug.LogError("No serializers found!");
        }
    }
}
