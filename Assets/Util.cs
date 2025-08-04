using System.Collections.Generic;
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

    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static void SetHeight(this RectTransform rt, float height)
    {
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }

    public static void SetPadding(this RectTransform rt, float left, float right, float top, float bottom)
    {
        rt.SetLeft(left);
        rt.SetRight(right);
        rt.SetTop(top);
        rt.SetBottom(bottom);
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

    internal static Button AddButton(RectTransform rect, string title)
    {
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(rect, false);
        var button = buttonObject.AddComponent<Button>();
        button.colors = normalColorBlock;
        var img = buttonObject.AddComponent<Image>();
        button.targetGraphic = img;

        ((RectTransform)buttonObject.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = title;

        SetRectCenterStretch((RectTransform)textObject.transform);

        return button;
    }

    internal static Toggle AddToggle(RectTransform rect, string title)
    {
        GameObject toggleObject = new GameObject("Toggle");
        toggleObject.transform.SetParent(rect, false);
        var toggle = toggleObject.AddComponent<Toggle>();
        toggle.colors = normalColorBlock;

        ((RectTransform)toggleObject.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(toggleObject.transform, false);
        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = title;

        SetRectCenterStretch((RectTransform)textObject.transform);

        //background
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(toggleObject.transform, false);
        var backgroundImage = backgroundObject.AddComponent<Image>();

        SetRectCenterStretch((RectTransform)backgroundObject.transform);

        //set the height of the input field
        ((RectTransform)backgroundObject.transform).SetLeft(300f);

        toggle.targetGraphic = backgroundImage;

        GameObject checkmarkObject = new GameObject("Checkmark");
        checkmarkObject.transform.SetParent(backgroundObject.transform, false);
        var checkmarkImage = checkmarkObject.AddComponent<Image>();

        SetRectCenterStretch((RectTransform)checkmarkObject.transform);
        ((RectTransform)checkmarkObject.transform).SetPadding(10f, 10f, 10f, 10f);

        toggle.graphic = checkmarkImage;

        return toggle;
    }

    internal static TMP_InputField AddInputField(RectTransform rect, string title)
    {
        //setup horizontal layout
        GameObject layoutObject = new GameObject("InputFieldLayout");
        layoutObject.transform.SetParent(rect, false);
        layoutObject.AddComponent<RectTransform>();

        //set height
        ((RectTransform)layoutObject.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);

        var textObj = AddText((RectTransform)layoutObject.transform, title);

        GameObject inputFieldObject = new GameObject("TextInputField");
        inputFieldObject.transform.SetParent(layoutObject.transform, false);
        var inputfield = inputFieldObject.AddComponent<TMP_InputField>();
        inputfield.colors = normalColorBlock;
        //add image
        var img = inputFieldObject.AddComponent<Image>();
        inputfield.targetGraphic = img;

        SetRectCenterStretch((RectTransform)inputFieldObject.transform);

        //set the height of the input field
        ((RectTransform)inputFieldObject.transform).SetLeft(300f);

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
