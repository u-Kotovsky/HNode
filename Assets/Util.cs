using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
    /// <summary>
    /// Helper function for block alpha, to support various automatic masking features
    /// check out <see cref="ShowConfiguration.autoMaskOnZero"/>
    /// </summary>
    /// <param name="channelValue"></param>
    /// <returns></returns>
    internal static byte GetBlockAlpha(byte channelValue)
    {
        if (Loader.showconf.autoMaskOnZero && channelValue == 0)
        {
            return 0;
        }

        return 255;
    }



    //helper funcs for UI

    internal static ColorBlock normalColorBlock = new ColorBlock
    {
        normalColor = new Color(0.16f, 0.16f, 0.16f, 1),
        highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1),
        pressedColor = new Color(0.3f, 0.3f, 0.3f, 1),
        selectedColor = new Color(0.4f, 0.4f, 0.4f, 1),
        disabledColor = new Color(0.16f, 0.16f, 0.16f, 1),
        colorMultiplier = 1,
        fadeDuration = 0.1f
    };

    internal static void SetRectCenterStretch(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    internal static TextMeshProUGUI AddText(RectTransform rect, string text = "")
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(rect, false);
        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        SetRectCenterStretch((RectTransform)textObject.transform);
        textComponent.text = text;
        textComponent.color = Color.white; //default color
        return textComponent;
    }

    internal static Toggle AddToggle(RectTransform rect)
    {
        
    }

    internal static TMP_InputField AddInputField(RectTransform rect)
    {
        GameObject inputFieldObject = new GameObject("TextInputField");
        inputFieldObject.transform.SetParent(rect, false);
        var inputfield = inputFieldObject.AddComponent<TMP_InputField>();
        inputfield.colors = normalColorBlock;
        //add image
        var img = inputFieldObject.AddComponent<Image>();
        inputfield.targetGraphic = img;

        //set the height of the input field
        ((RectTransform)inputFieldObject.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);

        //sub object text area
        GameObject textAreaObject = new GameObject("TextArea");
        textAreaObject.transform.SetParent(inputFieldObject.transform, false);
        textAreaObject.AddComponent<RectMask2D>();
        //stretch
        SetRectCenterStretch((RectTransform)textAreaObject.transform);

        //placeholder and text components
        GameObject placeholderTextObject = new GameObject("Placeholder");
        placeholderTextObject.transform.SetParent(textAreaObject.transform, false);
        var placeholderText = placeholderTextObject.AddComponent<TextMeshProUGUI>();
        SetRectCenterStretch((RectTransform)placeholderTextObject.transform);
        placeholderText.text = "....";

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(textAreaObject.transform, false);
        var text = textObject.AddComponent<TextMeshProUGUI>();
        SetRectCenterStretch((RectTransform)textObject.transform);

        inputfield.textComponent = text;
        inputfield.placeholder = placeholderText;
        inputfield.textViewport = textAreaObject.GetComponent<RectTransform>();

        //disable, re enable
        inputfield.enabled = false;
        inputfield.enabled = true;

        return inputfield;
    }
}
