using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Generators.MAVLinkDrone
{
    public class GridLayout : IDroneLayoutProvider
    {
        [YamlMember(Description = "The Bottom Left corner of the grid in degrees, Longitude (X)")]
        public float gridLon = 0;//x
        [YamlMember(Description = "The Bottom Left corner of the grid in degrees, Latitude (Y)")]
        public float gridLat = 0;//y
        [YamlMember(Description = "How many drones to place in a single row of the grid, along the Longitude (X) axis")]
        public EquationNumber gridLonCount = 1;
        [YamlMember(Description = "Horizontal spacing between drones in degrees, Longitude (X) axis")]
        public float gridSpacingLon = 0.0001f; //spacing in degrees
        [YamlMember(Description = "Vertical spacing between drones in degrees, Latitude (Y) axis")]
        public float gridSpacingLat = 0.0001f; //spacing in degrees
        [YamlMember(Description = "The initial altitude of the drones in meters")]
        public float initialAltitude = 0f;
        public float rotation = 0f;
        public void LayoutDrones(ref Dictionary<byte, Drone> drones)
        {
            //layout all the drones
            int dronesLeft = drones.Count;
            while (dronesLeft > 0)
            {
                for (int j = 0; j < gridLonCount; j++)
                {
                    //get the drone at the index
                    Drone d = drones[(byte)(dronesLeft)];
                    Vector2 pos = new Vector2();
                    pos.x = gridLon + (j * gridSpacingLat);
                    pos.y = gridLat + ((dronesLeft - 1) / gridLonCount) * gridSpacingLon;
                    pos = RotateAbout(pos, new Vector2(gridLon, gridLat), rotation);
                    //set the position based on the grid
                    d.SetPosition(pos.x,
                                  pos.y,
                                  initialAltitude);
                    dronesLeft--;
                    if (dronesLeft <= 0)
                    {
                        break;
                    }
                }
            }
        }

        private Vector2 RotateAbout(Vector2 pos, Vector2 center, float rotation)
        {
            //rotate the position around the center by rotation degrees
            float radians = Mathf.Deg2Rad * rotation;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            Vector2 translated = pos - center;
            Vector2 rotated = new Vector2();
            rotated.x = translated.x * cos - translated.y * sin;
            rotated.y = translated.x * sin + translated.y * cos;
            return rotated + center;
        }
    }
}
