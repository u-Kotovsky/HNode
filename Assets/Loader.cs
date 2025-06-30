using System;
using System.Collections.Generic;
using System.Linq;
using SFB;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Loader : MonoBehaviour
{
    List<IDMXSerializer> serializers;
    public TMP_Dropdown serializerDropdown;
    public TMP_Dropdown deserializerDropdown;
    public Button saveButton;
    public Button loadButton;
    public Toggle transcodeToggle;
    public IDMXSerializer currentSerializer => showconf.Serializer;
    public IDMXSerializer currentDeserializer => showconf.Deserializer;

    public bool Transcode => showconf.Transcode;

    public ShowConfiguration showconf = new ShowConfiguration();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        showconf.mappingsChannels.Add(new ChannelRemapper.ChannelMapping(0, 50, 10));
        showconf.mappingsUV.Add(new UVRemapper.UVMapping(0, 0, 100, 100, 500, 500));

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
            showconf.Serializer = serializers[s];
            Debug.Log($"Selected serializer: {currentSerializer.GetType().Name}");
        });

        deserializerDropdown.onValueChanged.AddListener((s) =>
        {
            showconf.Deserializer = serializers[s];
            Debug.Log($"Selected deserializer: {serializers[s].GetType().Name}");
        });

        //select the first serializer by default
        if (serializers.Count > 0)
        {
            if (showconf.Serializer != null)
            {
                serializerDropdown.value = serializerDropdown.options.FindIndex(o => o.text == showconf.Serializer.GetType().Name);
            }
            else
            {
                serializerDropdown.value = 0;
                showconf.Serializer = serializers[0];
            }
            serializerDropdown.RefreshShownValue();

            if (showconf.Deserializer != null)
            {
                deserializerDropdown.value = deserializerDropdown.options.FindIndex(o => o.text == showconf.Deserializer.GetType().Name);
            }
            else
            {
                deserializerDropdown.value = 0;
                //set it into the current serializer
                showconf.Deserializer = serializers[0];
            }
            deserializerDropdown.RefreshShownValue();
            Debug.Log($"Default serializer: {currentSerializer.GetType().Name}");
            Debug.Log($"Default deserializer: {currentDeserializer.GetType().Name}");
        }
        else
        {
            Debug.LogError("No serializers found!");
        }

        transcodeToggle.isOn = showconf.Transcode;

        //setup callback
        transcodeToggle.onValueChanged.AddListener((value) =>
        {
            showconf.Transcode = value;
        });

        //setup save load buttons
        saveButton.onClick.AddListener(SaveShowConfiguration);
        loadButton.onClick.AddListener(LoadShowConfiguration);
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

    public void SaveShowConfiguration()
    {
        //serialize to a yml string
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance);

        foreach (var serializerType in serializers)
        {
            //needed to tag each serializer type
            serializer.WithTagMapping("!" + serializerType.GetType().Name, serializerType.GetType());
        }

        //build it
        var finished = serializer.Build();
        var yaml = finished.Serialize(showconf);

        //open save file dialog
        var extensionList = new[] {
            new ExtensionFilter("Show Configurations", "shwcfg"),
        };
        var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "NewShowConfig", extensionList);

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Save cancelled or path is empty.");
            return;
        }

        //write the yaml to the file
        System.IO.File.WriteAllText(path, yaml);
    }

    public void LoadShowConfiguration()
    {
        //open a file dialog
        var extensions = new[] {
            new ExtensionFilter("Show Configurations", "shwcfg"),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length != 1)
        {
            Debug.LogError("Please select exactly one file.");
            return;
        }

        //read from the first path
            var content = System.IO.File.ReadAllText(paths[0]);

        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("File is empty or not found.");
            return;
        }

        //load from a yml string
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance);

        foreach (var serializerType in serializers)
        {
            //needed to tag each serializer type
            deserializer.WithTagMapping("!" + serializerType.GetType().Name, serializerType.GetType());
        }

        //build it
        var finished = deserializer.Build();
        var tempshowconf = finished.Deserialize<ShowConfiguration>(content);

        if (tempshowconf == null)
        {
            Debug.LogError("Failed to load ShowConfiguration");
            return;
        }
        
        showconf = tempshowconf;
    }
}
