using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtNet;
using Klak.Spout;
using SFB;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static ChannelRemapper;
using static TextureWriter;
using static UVRemapper;

public class Loader : MonoBehaviour
{
    List<IDMXSerializer> serializers;

    //dmx generator source
    List<IDMXGenerator> generators;
    public TMP_Dropdown serializerDropdown;
    public TMP_Dropdown deserializerDropdown;
    public TMP_InputField transcodeUniverseInput;
    public Button saveButton;
    public Button loadButton;
    public Toggle transcodeToggle;
    public SpoutReceiver spoutReceiver;
    public SpoutSender spoutSender;

    public static ShowConfiguration showconf = new ShowConfiguration();

    IDeserializer ymldeserializer;
    ISerializer ymlserializer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //showconf.mappingsChannels.Add(new ChannelRemapper.ChannelMapping(0, 50, 10));
        //showconf.mappingsUV.Add(new UVRemapper.UVMapping(0, 0, 100, 100, 500, 500));

        //load in all the serializers
        serializers = GetAllInterfaceImplementations<IDMXSerializer>();
        generators = GetAllInterfaceImplementations<IDMXGenerator>();

        //find the VRSL one
        VRSL vrsl = serializers.OfType<VRSL>().FirstOrDefault();

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
            Debug.Log($"Selected serializer: {showconf.Serializer.GetType().Name}");
        });

        deserializerDropdown.onValueChanged.AddListener((s) =>
        {
            showconf.Deserializer = serializers[s];
            Debug.Log($"Selected deserializer: {showconf.Deserializer.GetType().Name}");
        });

        //default the serializers to VRSL and have transcode off
        showconf.Serializer = vrsl;
        showconf.Deserializer = vrsl;
        //showconf.Transcode = false;
        //showconf.TranscodeUniverseCount = 3;

        //TODO: REMOVE THIS LATER AFTER TESTING
        /* showconf.Generators.Add(new Text()
        {
            text = "Hello World",
            channelStart = 50
        }); */

        //select the first serializer by default
        InvalidateDropdownsAndToggles();

        //setup callback
        transcodeToggle.onValueChanged.AddListener((value) =>
        {
            showconf.Transcode = value;
        });

        transcodeUniverseInput.onValueChanged.AddListener((value) =>
        {
            if (int.TryParse(value, out int universeCount))
            {
                showconf.TranscodeUniverseCount = universeCount;
            }
            else
            {
                Debug.LogWarning($"Invalid universe count: {value}");
            }
        });

        //setup save load buttons
        saveButton.onClick.AddListener(SaveShowConfiguration);
        loadButton.onClick.AddListener(LoadShowConfiguration);

        //serialize to a yml string
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance);

        foreach (var serializerType in serializers)
        {
            //needed to tag each serializer type
            serializer.WithTagMapping("!" + serializerType.GetType().Name, serializerType.GetType());
        }

        foreach (var generatorType in generators)
        {
            //needed to tag each generator type
            serializer.WithTagMapping("!" + generatorType.GetType().Name, generatorType.GetType());
        }

        //build it
        ymlserializer = serializer.Build();


        //load from a yml string
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance);

        foreach (var serializerType in serializers)
        {
            //needed to tag each serializer type
            deserializer.WithTagMapping("!" + serializerType.GetType().Name, serializerType.GetType());
        }

        foreach (var generatorType in generators)
        {
            //needed to tag each generator type
            deserializer.WithTagMapping("!" + generatorType.GetType().Name, generatorType.GetType());
        }

        //build it
        ymldeserializer = deserializer.Build();
    }

    private List<T> GetAllInterfaceImplementations<T>()
    {
        var type = typeof(T);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p));

        return types
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Select(t => (T)Activator.CreateInstance(t))
            .ToList();
    }

    public void SaveShowConfiguration()
    {
        var yaml = ymlserializer.Serialize(showconf);

        #region Example ShowConfiguration
        //make a new debugging showconf for a helper comment at the top of the file
        var debugShowconf = new ShowConfiguration()
        {
            Serializer = new FuralitySomna()
            {
                mergedChannels = new Dictionary<string, ColorChannel>()
                {
                    {"7", ColorChannel.Red},
                    {"8", ColorChannel.Green},
                    {"9", ColorChannel.Blue},
                    {"7 + 13", ColorChannel.Red},
                    {"8 + 13", ColorChannel.Green},
                    {"9 + 13", ColorChannel.Blue},
                    {"7 + (13 * 2)", ColorChannel.Red},
                    {"8 + (13 * 2)", ColorChannel.Green},
                    {"9 + (13 * 2)", ColorChannel.Blue},
                }
            },
            Deserializer = new VRSL(),
            Generators = new List<IDMXGenerator>()
            {
                new Text()
                {
                    text = "Hello World",
                    channelStart = 0
                },
                new Time(),
                new LRC()
            },
            Transcode = showconf.Transcode,
            mappingsChannels = new List<ChannelMapping>()
            {
                new ChannelMapping(0, 255, 1),
                new ChannelMapping(0, 255, 10),
            },
            mappingsUV = new List<UVMapping>()
            {
                new UVMapping(0, 0, 100, 100, 500, 500),
                new UVMapping(100, 100, 200, 200, 500, 500),
            },
            maskedChannels = new List<int>()
            {
                0, 1, 2
            }
        };

        var debugyaml = ymlserializer.Serialize(debugShowconf);
        //comment it by adding "# " to the start of each line
        var commentedDebugYaml = string.Join("\n", debugyaml.Split('\n').Select(line => "# " + line));
        commentedDebugYaml = "# Example ShowConfiguration:\n" + commentedDebugYaml + "\n# End Example\n\n";

        yaml = commentedDebugYaml + "\n\n\n" + yaml;
        #endregion

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

        showconf = ymldeserializer.Deserialize<ShowConfiguration>(content);

        //invalidate the dropdowns and toggles
        InvalidateDropdownsAndToggles();

        //start coroutine
        //this is stupid dumb shit but this YML library is being weird and this fixes the issue
        StartCoroutine(DeferredLoad(content));
    }

    IEnumerator DeferredLoad(string content)
    {
        //returning 0 will make it wait 1 frame
        yield return new WaitForEndOfFrame();

        //yayyyyy double load to fix dumb race condition bullshit
        showconf = ymldeserializer.Deserialize<ShowConfiguration>(content);

        //run initialization on all generators
        foreach (var generator in showconf.Generators)
        {
            generator.Construct();
        }

        showconf.Serializer.Construct();
        showconf.Deserializer.Construct();

        //setup spout input/outputs
        spoutSender.spoutName = showconf.SpoutOutputName;
        spoutReceiver.sourceName = showconf.SpoutInputName;

        //TODO: Add artnet setup here
    }

    private void InvalidateDropdownsAndToggles()
    {
        //invalidate the dropdowns and toggles to match the loaded showconf
        if (showconf.Serializer != null)
        {
            int serializerIndex = serializerDropdown.options.FindIndex(o => o.text == showconf.Serializer.GetType().Name);
            if (serializerIndex >= 0)
            {
                serializerDropdown.value = serializerIndex;
                //serializerDropdown.RefreshShownValue();
                Debug.Log($"Loaded serializer: {showconf.Serializer.GetType().Name}");
            }
            else
            {
                Debug.LogWarning($"Loaded serializer {showconf.Serializer.GetType().Name} not found in dropdown options.");
            }
        }

        if (showconf.Deserializer != null)
        {
            int deserializerIndex = deserializerDropdown.options.FindIndex(o => o.text == showconf.Deserializer.GetType().Name);
            if (deserializerIndex >= 0)
            {
                deserializerDropdown.value = deserializerIndex;
                //deserializerDropdown.RefreshShownValue();
                Debug.Log($"Loaded deserializer: {showconf.Deserializer.GetType().Name}");
            }
            else
            {
                Debug.LogWarning($"Loaded deserializer {showconf.Deserializer.GetType().Name} not found in dropdown options.");
            }
        }

        transcodeToggle.isOn = showconf.Transcode;
        transcodeUniverseInput.text = showconf.TranscodeUniverseCount.ToString();
    }
}
