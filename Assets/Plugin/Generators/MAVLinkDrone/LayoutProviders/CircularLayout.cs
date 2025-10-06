using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Generators.MAVLinkDrone
{
    public class CircularLayout : IDroneLayoutProvider
    {
        [YamlMember(Description = "The center of the circle in degrees, Longitude (X)")]
        public float Lon = 0;//x
        [YamlMember(Description = "The center of the circle in degrees, Latitude (Y)")]
        public float Lat = 0;//y
        [YamlMember(Description = "The radius of the circle in degrees")]
        public float Radius = 0.0001f; //spacing in degrees
        [YamlMember(Description = "The initial altitude of the drones in meters")]
        public float initialAltitude = 0f;
        [YamlMember(Description = "0 to 1 float representing the rotation offset from the default, and the next drone position")]
        public float rotationOffset = 0f;
        public void LayoutDrones(ref Dictionary<byte, Drone> drones)
        {
            float angleStep = 360f / drones.Count;
            //layout all the drones
            for (int j = 0; j < drones.Count; j++)
            {
                //get the drone at the index
                Drone d = drones[(byte)(j + 1)];
                //set the position based on the grid
                float angle = angleStep * j;
                angle += rotationOffset * angleStep;
                //get a direction vector2
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 pos = dir * Radius;
                d.SetPosition(Lon + pos.x, Lat + pos.y, initialAltitude);
            }
        }
    }
}
