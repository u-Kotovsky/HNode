using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDMXGenerator : IUserInterface<IDMXGenerator>, IConstructable
{
    void GenerateDMX(ref List<byte> dmxData);
}
