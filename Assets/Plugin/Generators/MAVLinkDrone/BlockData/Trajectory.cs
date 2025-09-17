using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generators.MAVLinkDrone
{
    //https://www.bitcraze.io/documentation/repository/crazyflie-firmware/master/functional-areas/trajectory_formats/#compressed-representation
    public class Trajectory
    {
        public List<float> xControlPoints = new();
        public List<float> yControlPoints = new();
        public List<float> zControlPoints = new();
        public List<float> yawControlPoints = new(); //unused
        public Vector3 lastPosition;
        public BezierOrder X_Order;
        public BezierOrder Y_Order;
        public BezierOrder Z_Order;
        public BezierOrder YAW_Order;
        public TimeSpan duration;
        public TimeSpan startTime;
        public TimeSpan endTime;

        public Trajectory()
        {
            //blank trajectory
            startTime = TimeSpan.Zero;
            duration = TimeSpan.FromSeconds(1);
            endTime = startTime + duration;
            xControlPoints.Add(0);
            yControlPoints.Add(0);
            zControlPoints.Add(0);
            yawControlPoints.Add(0);
            lastPosition = new Vector3(0, 0, 0);
            X_Order = BezierOrder.Constant;
            Y_Order = BezierOrder.Constant;
            Z_Order = BezierOrder.Constant;
            YAW_Order = BezierOrder.Constant;
        }

        public Trajectory(ref Queue<byte> data, Vector3 startPos, float startYaw, byte scale)
        {
            xControlPoints.Add(startPos.x);
            yControlPoints.Add(startPos.y);
            zControlPoints.Add(startPos.z);
            yawControlPoints.Add(startYaw);

            //decode the axis order information
            DecodeAxies(ref data);

            //decode the duration info
            DecodeDuration(ref data);

            //control points are stored in this order
            //X Y Z YAW
            xControlPoints.AddRange(DecodeAxisControlPoints(ref data, scale, X_Order));
            yControlPoints.AddRange(DecodeAxisControlPoints(ref data, scale, Y_Order));
            zControlPoints.AddRange(DecodeAxisControlPoints(ref data, scale, Z_Order));
            yawControlPoints.AddRange(DecodeAxisControlPoints(ref data, scale, YAW_Order));

            lastPosition = new Vector3(
                xControlPoints.Last(),
                yControlPoints.Last(),
                zControlPoints.Last()
            );
        }

        public bool InsideEvent(TimeSpan time)
        {
            //check if the time is inside the event
            return time >= startTime && time < endTime;
        }

        public Vector3 evaluate(float t)
        {
            //evaluate the bezier curve at time t
            //t is between 0 and 1

            //get the control points
            float x = BezierEvaluate(xControlPoints, t);
            float y = BezierEvaluate(yControlPoints, t);
            float z = BezierEvaluate(zControlPoints, t);

            return new Vector3(-y, x, z); //blender coord system go brrrr
        }

        public float BezierEvaluate(List<float> controlPoints, float t)
        {
            switch (controlPoints.Count)
            {
                case 1:
                    return controlPoints[0]; //constant
                case 2:
                    return Mathf.Lerp(controlPoints[0], controlPoints[1], t); //straight line
                case 4:
                    return BezierCubicEvaluate(controlPoints, t); //cubic bezier
                case 8:
                    return BezierSeventhDegreeEvaluate(controlPoints, t); //seventh degree bezier
                default:
                    throw new ArgumentException("Invalid number of control points");
            }
        }

        public static float BezierCubicEvaluate(List<float> controlPoints, float t)
        {
            //cubic bezier formula
            return Mathf.Pow(1 - t, 3) * controlPoints[0] +
                   3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1] +
                   3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2] +
                   Mathf.Pow(t, 3) * controlPoints[3];
        }

        public static float BezierSeventhDegreeEvaluate(List<float> controlPoints, float t)
        {
            //seventh degree bezier formula
            return Mathf.Pow(1 - t, 7) * controlPoints[0] +
                   7 * Mathf.Pow(1 - t, 6) * t * controlPoints[1] +
                   21 * Mathf.Pow(1 - t, 5) * Mathf.Pow(t, 2) * controlPoints[2] +
                   35 * Mathf.Pow(1 - t, 4) * Mathf.Pow(t, 3) * controlPoints[3] +
                   35 * Mathf.Pow(1 - t, 3) * Mathf.Pow(t, 4) * controlPoints[4] +
                   21 * Mathf.Pow(1 - t, 2) * Mathf.Pow(t, 5) * controlPoints[5] +
                   7 * (1 - t) * Mathf.Pow(t, 6) * controlPoints[6] +
                   Mathf.Pow(t, 7) * controlPoints[7];
        }

        public void DecodeDuration(ref Queue<byte> data)
        {
            //duration is a signed short in milliseconds
            //duration = TimeSpan.FromMilliseconds(BitConverter.ToInt16(data.DequeueChunk(2).ToArray(), 0));
            //cursed, was getting NEGATIVE durations somehow????
            duration = TimeSpan.FromMilliseconds(BitConverter.ToUInt16(data.DequeueChunk(2).ToArray(), 0));
        }

        public void DecodeAxies(ref Queue<byte> data)
        {
            //deque the byte
            byte dat = data.Dequeue();
            X_Order = DecodeAxisOrder(dat, Axis.X);
            Y_Order = DecodeAxisOrder(dat, Axis.Y);
            Z_Order = DecodeAxisOrder(dat, Axis.Z);
            YAW_Order = DecodeAxisOrder(dat, Axis.YAW);
        }

        public static BezierOrder DecodeAxisOrder(byte data, Axis ax)
        {
            //shift the data 
            byte shifted = (byte)(data >> (byte)ax);
            //grab just the two LSBs
            shifted &= 0x03;

            //convert to the enum
            return (BezierOrder)shifted;
        }

        public List<float> DecodeAxisControlPoints(ref Queue<byte> data, byte scale, BezierOrder ord)
        {
            List<float> points = new();

            int pointCount = 0;
            //this is one less than the actual control point count due to us already having the start position
            switch (ord)
            {
                case BezierOrder.Constant:
                    pointCount = 0;
                    break;
                case BezierOrder.StraightLine:
                    pointCount = 1;
                    break;
                case BezierOrder.Cubic:
                    pointCount = 3;
                    break;
                case BezierOrder.SeventhDegree:
                    pointCount = 7;
                    break;
            }

            for (int i = 0; i < pointCount; i++)
            {
                points.Add(DecodeSpatialCoordinate(ref data, scale));
            }

            return points;
        }

        public static Vector3 DecodeStartSpatialCoordinates(ref Queue<byte> data, byte scale)
        {
            return new Vector3(
                DecodeSpatialCoordinate(ref data, scale),
                DecodeSpatialCoordinate(ref data, scale),
                DecodeSpatialCoordinate(ref data, scale)
            );
        }

        public static float DecodeSpatialCoordinate(ref Queue<byte> data, byte scale)
        {
            //get the start XYZ YAW position, coordinates are in millimeters as signed shorts
            return BitConverter.ToInt16(data.DequeueChunk(2).ToArray(), 0) * scale / 1000f; //convert to meters
        }

        public static float DecodeAngleCoordinate(ref Queue<byte> data)
        {
            //Angles (for the yaw coordinate) are represented as 1/10th of degrees and are stored as signed 2-byte integers.
            return BitConverter.ToInt16(data.DequeueChunk(2).ToArray(), 0) / 10f;
        }

        public enum BezierOrder : byte
        {
            Constant = 0, //00
            StraightLine = 1, //01
            Cubic = 2, //10
            SeventhDegree = 3, //11
        }

        //represents how many bits need to be shifted to get different axis's
        public enum Axis : byte
        {
            X = 0,
            Y = 2,
            Z = 4,
            YAW = 6,
        }
    }
}
