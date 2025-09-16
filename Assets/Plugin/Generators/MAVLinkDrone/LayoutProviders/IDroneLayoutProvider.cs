using System.Collections.Generic;
using static Generators.MAVLinkDrone.MAVLinkDroneNetwork;

namespace Generators.MAVLinkDrone
{
    [TagMapped]
    public interface IDroneLayoutProvider
    {
        public void LayoutDrones(ref Dictionary<byte, Drone> drones);
    }
}
