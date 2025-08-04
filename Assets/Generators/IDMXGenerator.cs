using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDMXGenerator : IUserInterface<IDMXGenerator>
{
    void GenerateDMX(ref List<byte> dmxData);
    void Construct();
    void Deconstruct();
}
