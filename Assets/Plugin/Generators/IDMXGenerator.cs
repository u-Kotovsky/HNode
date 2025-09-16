using System;
using System.Collections.Generic;
using UnityEngine;

[TagMapped]
public interface IDMXGenerator : IUserInterface<IDMXGenerator>, IConstructable
{
    void GenerateDMX(ref List<byte> dmxData);
}
