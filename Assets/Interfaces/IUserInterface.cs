using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUserInterface<T> where T : class
{
    void ConstructUserInterface(RectTransform rect);

    void DeconstructUserInterface();
    
    void UpdateUserInterface();
}
