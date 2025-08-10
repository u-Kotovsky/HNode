using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private List<TabButton> TabButtons;
    public TabButton startupButton;

    public Action OnSave;
    public Button SaveButton;
    public Action OnLoad;
    public Button LoadButton;


    #region Serializer Settings
    public TMP_Dropdown serializerDropdown;
    public RectTransform serializerDynamicSettingsRect;
    public TMP_InputField serializeUniverseCountField;
    public RectTransform maskedChannelsDynamicRect;
    public Toggle invertMaskToggle;
    public Toggle autoMaskOnZeroToggle;
    public TMP_InputField spoutOutputNameField;
    public TMP_InputField artNetPortField;
    #endregion

    #region Deserializer Settings
    public TMP_Dropdown deserializerDropdown;
    public RectTransform deserializerDynamicSettingsRect;
    public Toggle transcodeToggle;
    public TMP_InputField transcodeUniverseCountField;
    public TMP_InputField spoutInputNameField;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get all the tab buttons
        TabButtons = new List<TabButton>(FindObjectsByType<TabButton>(FindObjectsSortMode.None));

        //add a listener to each tab button
        foreach (var button in TabButtons)
        {
            button.SetupListener(this);
            button.SetSelected(false);
        }

        OnTabButtonClicked(startupButton);

        //add listeners to save and load buttons
        SaveButton.onClick.AddListener(() => OnSave?.Invoke());
        LoadButton.onClick.AddListener(() => OnLoad?.Invoke());
    }

    public void InvalidateUIState()
    {
        //serializer setup
        RebuildDropdown(serializerDropdown, Loader.serializers.Select(x => x.GetType().Name).ToList(), Loader.showconf.Serializer?.GetType().Name ?? string.Empty);
        serializeUniverseCountField.text = Loader.showconf.SerializeUniverseCount.ToString();
        invertMaskToggle.isOn = Loader.showconf.invertMask;
        autoMaskOnZeroToggle.isOn = Loader.showconf.autoMaskOnZero;
        spoutOutputNameField.text = Loader.showconf.SpoutOutputName;
        artNetPortField.text = Loader.showconf.ArtNetPort.ToString();

        //deserializer setup
        RebuildDropdown(deserializerDropdown, Loader.serializers.Select(x => x.GetType().Name).ToList(), Loader.showconf.Deserializer?.GetType().Name ?? string.Empty);
        transcodeToggle.isOn = Loader.showconf.Transcode;
        transcodeUniverseCountField.text = Loader.showconf.TranscodeUniverseCount.ToString();
        spoutInputNameField.text = Loader.showconf.SpoutInputName;
    }

    private void RebuildDropdown(TMP_Dropdown dropdown, List<string> options, string search)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        //search for the option
        int index = options.FindIndex(x => x.Contains(search, StringComparison.OrdinalIgnoreCase));
        dropdown.value = index >= 0 ? index : 0;

        dropdown.RefreshShownValue();
    }

    //event for all the tab buttons to call when clicked
    public void OnTabButtonClicked(TabButton clickedButton)
    {
        //hide all tab content
        foreach (var button in TabButtons)
        {
            foreach (var content in button.TabContent)
            {
                content.SetActive(false);
            }
            button.SetSelected(false);
        }

        //show the clicked button's tab content
        foreach (var content in clickedButton.TabContent)
        {
            content.SetActive(true);
        }
        clickedButton.SetSelected(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
