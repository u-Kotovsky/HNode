using System;
using System.Collections.Generic;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

public class GeneratorFixture : IDMXGenerator
{
    public DMXChannel channelStart = 0;
    
    public Fixture Fixture = new()
    {
        Parameters = new()
        {
            new FixtureParameter { Name = "parameter 1", Value = 127},
            new FixtureParameter { Name = "parameter 2", Value = 255}
        }
    };
    
    private static IDeserializer _ymldeserializer;
    private static ISerializer _ymlserializer;

    static GeneratorFixture()
    {
        // Setup yml serializer and deserializer
        _ymlserializer = Util.SetupBuilder<SerializerBuilder>().Build();
        _ymldeserializer = Util.SetupBuilder<DeserializerBuilder>().Build();
    }
    
    public virtual void Construct() { }
    public virtual void Deconstruct() { }

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        dmxData.EnsureCapacity(channelStart + 1);
        //dmxData.EnsureCapacity(channelStart + (channelEnd - channelStart) + 1);

        //we need to write to the dmx data list directly
        //for (int i = channelStart; i < channelEnd + 1; i++)
        //{
        //    dmxData[i] = (byte)value;
        //}

        for (var i = 0; i < Fixture.Parameters.Count; i++)
        {
            dmxData[i + channelStart] = (byte)Fixture.Parameters[i].Value;
        }
    }

    private protected TMP_InputField ChannelStartInputField;
    private protected Button LoadFixtureButton;
    private protected Button EditFixtureButton;
    private protected RectTransform Root;
    
    private protected Button SaveFixtureButton;
    private protected Button CancelFixtureButton;
    private protected Button AddFixtureParameterButton;
    private protected RectTransform ActionsRect;
    private protected RectTransform ParametersRect;
    
    public virtual void ConstructUserInterface(RectTransform rect)
    {
        Root = rect;
        
        ChannelStartInputField = Util.AddInputField(rect, "Channel Start")
            .WithText(channelStart)
            .WithCallback((value) => { channelStart = value; });

        ActionsRect = (Util.AddRect(Root)
                .WithHorizontalLayoutGroup().transform as RectTransform)
            .SetDefaultVerticalSizeWithCurrentAnchors();

        ParametersRect = Util.AddRect(Root);
        
        var layoutGroup = ParametersRect.WithVerticalLayoutGroup();
        layoutGroup.childControlHeight = false;

        LoadFixtureButton = Util.AddButton(ActionsRect, "Load")
            .WithCallback(LoadFixture);
        EditFixtureButton = Util.AddButton(ActionsRect, "Edit")
            .WithCallback(EditFixture);
        
        // Edit
        SaveFixtureButton = Util.AddButton(ActionsRect, "Save")
            .WithCallback(SaveFixture);
        CancelFixtureButton = Util.AddButton(ActionsRect, "Cancel")
            .WithCallback(CancelFixture);
        AddFixtureParameterButton = Util.AddButton(ActionsRect, "Add parameter")
            .WithCallback(() =>
            {
                var parameter = new FixtureParameter { Name = "parameter " + Fixture.Parameters.Count, Value = 0 };
                Fixture.Parameters.Add(parameter);
                var parameterRect = AddRectForElement(ParametersRect);
                parameter.Construct();
                parameter.ConstructUserInterface(parameterRect);
                // todo: update height of parameter list
            });
        
        SetActions(false);
        try
        {
            SetupDynamicUI();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }
    

    private void LoadFixture()
    {
        // todo: load json or yaml file that has mapping for fixture
        // ex.
        // fixture name
        // fixture description
        // tl;dr parameters:
        // channel name
        // channel id
        // channel description
        // channel dmx offset
        // has fine
        // has ultra
        // has super
        
        //open a file dialog
        var extensions = new[] {
            new ExtensionFilter("Show Configurations", "json"),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length != 1)
        {
            Debug.LogError("Please select exactly one file.");
            return;
        }

        LoadFixtureFile(paths[0]);
    }

    private void LoadFixtureFile(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("File not found");
            return;
        }
        
        //read from the path
        var content = System.IO.File.ReadAllText(path);

        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("File is empty.");
            return;
        }
        
        Fixture = _ymldeserializer.Deserialize<Fixture>(content);
        
        //start coroutine
        //this is stupid dumb shit but this YML library is being weird and this fixes the issue
        //StartCoroutine(DeferredLoad(content));
        LoadFixtureConfig();
    }
    
    public void LoadFixtureConfig(Action preCallback = null)
    {
        if (preCallback != null)
            preCallback();
        
        //run initialization on all generators
        foreach (var parameter in Fixture.Parameters)
        {
            parameter.Construct();
        }
        
        SetupDynamicUI();
    }

    private void SetupDynamicUI()
    {
        foreach (var parameter in Fixture.Parameters)
        {
            var rect = AddRectForElement(ParametersRect);
            
            parameter.ConstructUserInterface(rect);
            
            rect.gameObject.SetActive(false);
            rect.gameObject.SetActive(true);
        }
    }

    private RectTransform AddRectForElement(RectTransform rect)
    {
        var obj = Util.AddRect(rect);

        var layoutGroup = obj.WithHorizontalLayoutGroup();
        layoutGroup.childControlHeight = false;
            
        obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);

        return obj;
    }

    public void SaveFixtureFile()
    {
        var yaml = _ymlserializer.Serialize(Fixture);
        
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
    
    private void EditFixture()
    {
        Debug.Log("Edit Fixture");
        SetActions(true);
        Fixture.EditMode(true);
    }

    private void SaveFixture()
    {
        Debug.Log("Save Edit Fixture");
        SetActions(false);
        Fixture.EditMode(false);
    }
    private void CancelFixture()
    {
        Debug.Log("Cancel Edit Fixture");
        SetActions(false);
        Fixture.EditMode(false);
    }
    private void SetActions(bool edit)
    {
        Debug.Log("Reset Actions Fixture");
        LoadFixtureButton.gameObject.SetActive(!edit);
        EditFixtureButton.gameObject.SetActive(!edit);
        
        SaveFixtureButton.gameObject.SetActive(edit);
        CancelFixtureButton.gameObject.SetActive(edit);
    }

    public void DeconstructUserInterface() { }
    public void UpdateUserInterface() { }
}
