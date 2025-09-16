using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using static MAVLink;

namespace Generators.MAVLinkDrone
{
    public class Drone
    {
        private MavlinkParse parse;
        public byte uid; // the valid range for this is 1 to 255

        //lat long and altitude are fixed point numbers
        private Vector3 latlongaltposition = Vector3.zero;

        private Vector3 showOrigin = Vector3.zero;
        private TimeSpan flashStartTime = TimeSpan.Zero;
        private TimeSpan flashEndTime = TimeSpan.Zero;
        private TimeSpan flashInterval = TimeSpan.FromSeconds(0.5f);
        private Vector3 Position
        {
            get
            {
                var x = measure(showOrigin.x, latlongaltposition.y, showOrigin.x, showOrigin.y);
                var y = measure(latlongaltposition.x, showOrigin.y, showOrigin.x, showOrigin.y);
                //adjust for sign
                if (showOrigin.y - latlongaltposition.y > 0)
                {
                    x = -x;
                }
                if (showOrigin.x - latlongaltposition.x > 0)
                {
                    y = -y;
                }
                return new Vector3((float)x, (float)y, latlongaltposition.z);
            }
        }

        public Dictionary<string, float> parameters = new Dictionary<string, float>()
        {
            //{"FENCE_ALT_MAX", 1000},
        };

        UdpClient client;
        IPEndPoint remoteEndPoint;

        //constructed here https://github.com/skybrush-io/skybrush-server/blob/4fd65199a0578c56928981a709f0cabb69b15bd8/src/flockwave/server/ext/mavlink/driver.py#L2225
        public List<byte> showFileRaw = new List<byte>();
        public ShowFile showFile;

        public Drone(byte uid, UdpClient client, IPEndPoint remoteEndPoint)
        {
            //latlongaltposition = new Vector3(1,1,1);
            this.uid = uid; //0 is not valid
            this.client = client;
            this.remoteEndPoint = remoteEndPoint;
            //SetPosition(globalID * 0.0001f, globalID * 0.0001f, globalID * 0.0001f); // set initial position

            parse = new MavlinkParse();
        }

        public Vector3 GetDronePosition()
        {
            if (showFile != null)
            {
                //get position at real time
                return showFile.GetPositionAtRealTime(DateTime.Now);
            }

            //if no show file, return the GPS based position
            return Position;
        }

        public void SetFlashing()
        {
            //start a timer for 2 seconds flashing the LED
            flashStartTime = DateTime.Now.TimeOfDay;
            flashEndTime = flashStartTime + TimeSpan.FromSeconds(2);
        }

        public Color32 GetDroneColor()
        {
            //determine if we should be flashing
            if (DateTime.Now.TimeOfDay < flashEndTime && (DateTime.Now.TimeOfDay - flashStartTime).TotalMilliseconds % flashInterval.TotalMilliseconds < (flashInterval.TotalMilliseconds / 2))
            {
                return Color.white;
            }

            if (showFile != null)
            {
                //get color at time
                return showFile.GetColorAtRealTime(DateTime.Now);
            }
            return Color.black;
        }

        public int GetPyroIndex()
        {
            if (showFile != null)
            {
                return showFile.GetPyroAtRealTime(DateTime.Now);
            }
            return 0;
        }

        private double measure(float lat1, float lon1, float lat2, float lon2)
        {
            // generally used geo measurement function
            const double R = 6378.137; // Radius of earth in KM
            var dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
            var dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000; // meters
        }

        public Vector3 XYZtoLatLonAlt(Vector3 xyz)
        {
            const double R = 6378.137; // Radius of earth in KM
                                       //convert the meters to kilometers
            xyz.x /= 1000;
            xyz.y /= 1000;
            //pass altitude through
            var new_longitude = showOrigin.y + (xyz.y / R) * (180 / Math.PI) / Math.Cos(showOrigin.y * Math.PI / 180);
            var new_latitude = showOrigin.x + (xyz.x / R) * (180 / Math.PI);

            return new Vector3((float)new_longitude, (float)new_latitude, xyz.z);
        }

        DateTime GetFromGps(int weeknumber, double seconds)
        {
            DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0);
            DateTime week = datum.AddDays(weeknumber * 7);
            DateTime time = week.AddSeconds(seconds);
            //subtract leap seconds
            time = time.AddSeconds(-18);
            //offset backwards to be aligned with UTC time
            time = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            return time;
        }

        public void SetParameter(string name, float value)
        {
            if (parameters.ContainsKey(name))
            {
                parameters[name] = value;

                //check if the show start time parameter was set
                if (name == "SHOW_START_TIME")
                {
                    //set the show file time
                    if (showFile != null)
                    {
                        //time is seconds in the week
                        //get what the current week is
                        int weekNumber = (int)(DateTime.UtcNow - new DateTime(1980, 1, 6)).TotalDays / 7;
                        //check if its different to what it currently is
                        if (showFile.showStartTime != GetFromGps(weekNumber, value))
                        {
                            //reset program counters
                            showFile.LightProgramPointer = 0;
                            showFile.TrajectoryProgramPointer = 0;
                        }
                        showFile.showStartTime = GetFromGps(weekNumber, value);
                        //render militiary time
                        //Debug.Log($"Show start time set to: {showFile.showStartTime:yyyy-MM-dd HH:mm:ss} UTC for drone {uid}");
                    }
                    else
                    {
                        Debug.LogWarning("Show file is not initialized, cannot set show start time.");
                    }
                }
            }
            else
            {
                parameters.Add(name, value);
            }
        }

        public void SetShowOrigin(float lat, float lon, float alt)
        {
            showOrigin = new Vector3(lat, lon, alt);
            //latlongaltposition = showOrigin;
        }

        public void SetPosition(float lat, float lon, float alt)
        {
            //if any of the values are 0, make them as SMALL as possible
            float epsilon = 0.0000001f;
            if (lat == 0) lat = epsilon;
            if (lon == 0) lon = epsilon;
            if (alt == 0) alt = epsilon;
            latlongaltposition = new Vector3(lat, lon, alt);
        }

        public void ReloadShow()
        {
            //Debug.Log($"Reloading show for drone {uid}");

            showFile = new ShowFile(showFileRaw);

            //now that we are done with making the show file, drop it
            showFileRaw.Clear();
        }

        public void SendHeartbeat()
        {
            var message = new mavlink_heartbeat_t(
                127,
                (byte)MAV_TYPE.GENERIC,
                (byte)MAV_AUTOPILOT.ARDUPILOTMEGA,
                (byte)MAV_MODE_FLAG.AUTO_ENABLED,
                (byte)MAV_STATE.STANDBY,
                MAVLINK_VERSION);

            // Sends a message to the host to which you have connected.
            Transmit(MAVLINK_MSG_ID.HEARTBEAT, message);
        }

        public void Transmit<T>(MAVLINK_MSG_ID id, T message)
            where T : struct
        {
            var sendBytes = parse.GenerateMAVLinkPacket20(id, message, sysid: uid, compid: 1);

            client.Send(sendBytes, sendBytes.Length, remoteEndPoint.Address.ToString(), remoteEndPoint.Port);
        }

        public void SendAutopilotCapabilities()
        {
            //this is here because its not part of the official standard
            const ulong DRONE_SHOW_MODE = 0x4000000;
            var message = new mavlink_autopilot_version_t(
                (ulong)(MAV_PROTOCOL_CAPABILITY.PARAM_FLOAT
                | MAV_PROTOCOL_CAPABILITY.FTP
                | MAV_PROTOCOL_CAPABILITY.SET_POSITION_TARGET_GLOBAL_INT
                | MAV_PROTOCOL_CAPABILITY.SET_POSITION_TARGET_LOCAL_NED
                | MAV_PROTOCOL_CAPABILITY.MAVLINK2
                ) | DRONE_SHOW_MODE,
                uid, //random capabilities
                (uint)FIRMWARE_VERSION_TYPE.BETA,
                (uint)FIRMWARE_VERSION_TYPE.BETA,
                (uint)FIRMWARE_VERSION_TYPE.BETA,
                0,
                0,
                0,
                new byte[8],
                new byte[8],
                new byte[8],
                new byte[18] { (byte)uid, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            );

            Transmit(MAVLINK_MSG_ID.AUTOPILOT_VERSION, message);
        }

        public void SendCommandACK(MAV_CMD command, MAV_RESULT result, byte target_system, byte target_component, byte progress = byte.MaxValue, int result_param2 = 0)
        {
            var message = new mavlink_command_ack_t(
                (ushort)command,
                (byte)result,
                progress,
                result_param2,
                target_system,
                target_component
            );

            Transmit(MAVLINK_MSG_ID.COMMAND_ACK, message);
        }

        public void SendMissionACK(MAV_MISSION_RESULT result, MAV_MISSION_TYPE type, byte target_system, byte target_component)
        {
            var message = new mavlink_mission_ack_t(
                target_system,
                target_component,
                (byte)result,
                (byte)type
            );
            Transmit(MAVLINK_MSG_ID.MISSION_ACK, message);
        }

        public void SendFTPMessage(FTPMessage ftpMessage, byte target_system, byte target_component)
        {
            //convert to a mavlink_file_transfer_protocol_t
            var ftpobj = new mavlink_file_transfer_protocol_t
            (
                0,
                target_system,
                target_component,
                MavlinkUtil.StructureToByteArray(ftpMessage)
            );

            Transmit(MAVLINK_MSG_ID.FILE_TRANSFER_PROTOCOL, ftpobj);
        }

        public void SendGPSRaw()
        {
            //get the drone position
            var pos = GetDronePosition();
            //convert to lat long and altitude
            var latlongalt = XYZtoLatLonAlt(pos);
            var message = new mavlink_gps_raw_int_t(
                //send current unix time in seconds
                (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                (int)(latlongalt.x * 10000000),
                (int)(latlongalt.y * 10000000),
                (int)(latlongalt.z * 1000),
                ushort.MaxValue,
                ushort.MaxValue,
                ushort.MaxValue,
                ushort.MaxValue,
                (byte)GPS_FIX_TYPE._3D_FIX,
                4, //satallites
                0,
                0,
                0,
                0,
                0,
                0
            );

            Transmit(MAVLINK_MSG_ID.GPS_RAW_INT, message);
        }

        public void SendGPSFiltered()
        {
            //get the drone position
            var pos = GetDronePosition();
            //convert to lat long and altitude
            var latlongalt = XYZtoLatLonAlt(pos);
            var message = new mavlink_global_position_int_t(
                (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                (int)(latlongalt.x * 10000000),
                (int)(latlongalt.y * 10000000),
                (int)(latlongalt.z * 1000),
                0,
                0,
                0,
                0,
                0
            );

            Transmit(MAVLINK_MSG_ID.GLOBAL_POSITION_INT, message);
        }

        public void SendSYSStatus()
        {
            var flags = MAV_SYS_STATUS_SENSOR.GPS |
                        MAV_SYS_STATUS_SENSOR.MOTOR_OUTPUTS |
                        MAV_SYS_STATUS_SENSOR.RC_RECEIVER |
                        MAV_SYS_STATUS_SENSOR.BATTERY |
                        MAV_SYS_STATUS_SENSOR.SATCOM |
                        MAV_SYS_STATUS_SENSOR.PROPULSION;
            var message = new mavlink_sys_status_t(
                (uint)flags,
                (uint)flags,
                (uint)flags,
                50,
                12 * 1000,
                -1,
                0,
                0,
                0,
                0,
                0,
                0,
                -1
            );

            Transmit(MAVLINK_MSG_ID.SYS_STATUS, message);
        }
    }
}
