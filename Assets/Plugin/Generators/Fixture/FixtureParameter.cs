using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class FixtureParameter : IUserInterface<FixtureParameter>, IConstructable
{
    public string Name;
    public string Description;

    private EquationNumber value;
    public EquationNumber Value
    {
        get => value;
        set
        {
            this.value = value;
            OnValueChanged?.Invoke();
        }
    }
    public Action OnValueChanged = delegate { };
    public Action OnRemove = delegate { };

    public void Remove()
    {
        Debug.Log("Remove parameter " + Name);
        OnRemove?.Invoke();
        DeconstructUserInterface();
        Deconstruct();
    }
    
    public void Construct() { }
    public void Deconstruct() { }
    
    private protected TextMeshProUGUI titleText;
    private protected Slider valueSlider;
    private protected TMP_InputField valueInputField;
    
    private protected TMP_InputField editNameInputField;
    private protected Button deleteButton;
    private protected Button moveUpButton;
    private protected Button moveDownButton;
    
    public void ConstructUserInterface(RectTransform rect)
    {
        titleText = Util.AddText(rect, Name);
        valueInputField = Util.AddInputField(rect, Name, false)
            .WithPlaceholder("0")
            .WithText(Value.ToString())
            .WithContentType(TMP_InputField.ContentType.Alphanumeric)
            .WithCallback(text =>
            {
                EquationNumber.TryParse(text, out var result);
                Value = result;
            });
        
        ((RectTransform)valueInputField.transform.parent).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
        
        editNameInputField = Util.AddInputField(rect, Name, false)
            .WithPlaceholder("Name")
            .WithText(Name)
            .WithCallback(text =>
            {
                Name = text;
                //titleText.text = Name;
            });
        
        deleteButton = Util.AddButton(rect, "X")
            .WithCallback(Remove);
        
        moveUpButton = Util.AddButton(rect, "Up")
            .WithCallback(() =>
            {
                
            });
        moveDownButton = Util.AddButton(rect, "Down")
            .WithCallback(() =>
            {
                
            });
        
        EditMode(false);
    }

    public void EditMode(bool enabled)
    {
        valueInputField.transform.parent.gameObject.SetActive(!enabled);
        editNameInputField.transform.parent.gameObject.SetActive(enabled);
        titleText.text = enabled ? "Name" : Name;
        deleteButton.gameObject.SetActive(enabled);
        moveUpButton.gameObject.SetActive(enabled);
        moveDownButton.gameObject.SetActive(enabled);
    }

    public void DeconstructUserInterface()
    {
        Object.Destroy(titleText);
        Object.Destroy(valueSlider);
        Object.Destroy(valueInputField);
        Object.Destroy(editNameInputField);
        Object.Destroy(deleteButton);
        Object.Destroy(moveUpButton);
        Object.Destroy(moveDownButton);
    }

    public void UpdateUserInterface()
    {
        
    }
}
