using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDMXGenerator
{
    void GenerateDMX(ref List<byte> dmxData);
}
