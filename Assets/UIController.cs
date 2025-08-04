using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private List<TabButton> TabButtons;
    public TabButton startupButton;
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
