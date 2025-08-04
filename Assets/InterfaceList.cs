using System;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceList : MonoBehaviour
{
    public string interfaceName; //cant think of a better way to do this right now
    public Type @interface;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Startup()
    {
        // Initialize the interface type
        if (string.IsNullOrEmpty(interfaceName))
        {
            Debug.LogError("Interface name is not set.");
            return;
        }

        @interface = Type.GetType(interfaceName);
        if (@interface == null)
        {
            Debug.LogError($"Type '{interfaceName}' could not be found.");
            return;
        }
    }

    public void Initialize<T>(List<IUserInterface<T>> possibleUserInterfaces) where T : class
    {
        //clear out children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        //check types
        if (typeof(T) != @interface)
        {
            //just exit, this isnt what we want
            return;
        }

        //make new objects for each
        foreach (var userInterface in possibleUserInterfaces)
        {
            //create a new gameobject
            GameObject newObject = new GameObject(userInterface.GetType().Name);
            newObject.transform.SetParent(transform, false);

            //get the rect transform
            RectTransform rectTransform = newObject.AddComponent<RectTransform>();

            //construct the user interface
            userInterface.ConstructUserInterface(rectTransform);
        }
    }

    public void UpdateInterface<T>(List<IUserInterface<T>> possibleUserInterfaces) where T : class
    {
        //check types
        if (typeof(T) != @interface)
        {
            //just exit, this isnt what we want
            return;
        }

        //update each user interface
        foreach (var userInterface in possibleUserInterfaces)
        {
            userInterface.UpdateUserInterface();
        }
    }
}
