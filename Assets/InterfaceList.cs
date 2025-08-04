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

    public void Initialize<T>(List<IUserInterface<T>> possibleUserInterfaces, Action<int> del, Action<int, int> swap) where T : class
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
        for (int i = 0; i < possibleUserInterfaces.Count; i++)
        {
            //create a new gameobject
            GameObject newObject = new GameObject(possibleUserInterfaces[i].GetType().Name);
            newObject.transform.SetParent(transform, false);

            //get the rect transform
            RectTransform rectTransform = newObject.AddComponent<RectTransform>();

            //add text
            var tex = Util.AddText(rectTransform, possibleUserInterfaces[i].GetType().Name);
            tex.rectTransform.SetHeight(50f);

            //construct the user interface
            possibleUserInterfaces[i].ConstructUserInterface(rectTransform);

            //needed because I gets hoisted once, this makes a hoist per iteration
            int copy = i;

            //add a button
            Util.AddButton(rectTransform, "Delete").onClick.AddListener(() =>
            {
                del?.Invoke(copy);
            });

            Util.AddButton(rectTransform, "Move Up").onClick.AddListener(() =>
            {
                if (copy > 0)
                {
                    swap?.Invoke(copy, copy - 1);
                }
            });

            Util.AddButton(rectTransform, "Move Down").onClick.AddListener(() =>
            {
                if (copy < possibleUserInterfaces.Count - 1)
                {
                    swap?.Invoke(copy, copy + 1);
                }
            });
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
