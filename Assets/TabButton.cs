using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//execute first
[DefaultExecutionOrder(-100)]
public class TabButton : MonoBehaviour
{
    public List<GameObject> TabContent;

    private Button button;
    private TextMeshProUGUI buttonText;

    void Start()
    {
        button = GetComponent<Button>();
        //find the text as the first child of the button
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetupListener(UIController uiController)
    {
        button.onClick.AddListener(() => uiController.OnTabButtonClicked(this));
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
        }
        else
        {
            buttonText.color = Color.gray;
            buttonText.fontStyle = FontStyles.Normal;
        }
    }
}
