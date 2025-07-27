using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CrcSharp;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static MAVLink;

public class MAVLinkDroneNetwork : IDMXGenerator
{
    private Dictionary<byte, Drone> drones = new Dictionary<byte, Drone>();
    private UdpClient client;
    private IPEndPoint selfEndPoint = new IPEndPoint(IPAddress.Any, 0);
    private IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 14550);
    private MavlinkParse parse;

    public int droneCount = 254;
    public int networkPort = 14550;
    public int channelStart = 0;
    public float gridLon = 0;//x
    public float gridLat = 0;//y
    public int gridLonCount = 1;
    public float gridSpacingLon = 0.0001f; //spacing in degrees
    public float gridSpacingLat = 0.0001f; //spacing in degrees
    public float initialAltitude = 0f;
    private List<UniTask> tasks = new List<UniTask>();
    private CancellationTokenSource cancellationTokenSource = new();
    public void Construct()
    {
        client = new UdpClient();
        remoteEndPoint.Port = networkPort;
        client.Client.Bind(selfEndPoint);
        parse = new MavlinkParse();

        Debug.Log($"MAVLink Drone Network started on port {networkPort} with {droneCount} drones.");

        //initialize a certain ammount of drones
        //drones.Add(5, new Drone(5, client, remoteEndPoint));
        for (byte i = 1; i <= droneCount; i++)
        {
            //Debug.Log(i);
            drones.Add(i, new Drone(i, client, remoteEndPoint));
        }

        //layout all the drones
        int dronesLeft = drones.Count;
        while (dronesLeft > 0)
        {
            for (int j = 0; j < gridLonCount; j++)
            {
                //get the drone at the index
                Drone d = drones[(byte)(dronesLeft)];
                //set the position based on the grid
                d.SetPosition(gridLon + (j * gridSpacingLat),
                              gridLat + ((dronesLeft - 1) / gridLonCount) * gridSpacingLon,
                              initialAltitude);
                //set the LED color to a random color
                dronesLeft--;
                if (dronesLeft <= 0)
                {
                    break;
                }
            }
        }

        //startup tasks
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        tasks.Add(UniTask.Create(SendData, cancellationToken));
        tasks.Add(UniTask.Create(SendHeartbeat, cancellationToken));
        tasks.Add(UniTask.Create(ReceiveData, cancellationToken));
    }

    public void Deconstruct()
    {
        //end all the tasks
        cancellationTokenSource.Cancel();
    }

    public void GenerateDMX(ref List<byte> dmxData)
    {
        foreach (Drone d in drones.Values)
        {
            //make their X move cleanly based on the time
            //float offset = ((float)Math.Sin(DateTime.UtcNow.Ticks * 0.00000001d) - 0.5f) * 0.01f;
            //d.SetPosition(((float)Math.Sin(DateTime.UtcNow.Ticks * 0.00000001d) - 0.5f) * 0.001f, 0, 40);
            //Debug.Log((float)DateTime.UtcNow.Millisecond * 0.001f);
            //force color to 255
            //d.LEDColor = new Color32(255, 255, 255, 255);
            //d.SetPosition(d.uid, 0, 0);
            //d.SetPosition(0.000950f, 0.000950f, -50);

            //convert the XYZ to bytes
            //do this by converting them to -1 to 1 range from -800 to 800
            Vector3 pos = d.GetDronePosition();
            //Debug.Log($"Drone {d.uid} position: {pos.x}, {pos.y}, {pos.z}");
            float scaledX = Mathf.InverseLerp(-800, 800, pos.x);
            float scaledY = Mathf.InverseLerp(-800, 800, pos.y);
            float scaledZ = Mathf.InverseLerp(-800, 800, pos.z);

            //convert to 16 bit ushorts
            ushort x = (ushort)(scaledX * ushort.MaxValue);
            ushort y = (ushort)(scaledY * ushort.MaxValue);
            ushort z = (ushort)(scaledZ * ushort.MaxValue);

            //merge into a single list
            List<byte> droneValues = new List<byte>();

            //append to dmxData
            droneValues.AddRange(BitConverter.GetBytes(x).Reverse());
            droneValues.AddRange(BitConverter.GetBytes(y).Reverse());
            droneValues.AddRange(BitConverter.GetBytes(z).Reverse());

            Color32 color = d.GetDroneColor();
            //Debug.Log($"Drone {d.uid} color: {color.r}, {color.g}, {color.b}");

            //append the LED color
            droneValues.Add(color.r);
            droneValues.Add(color.g);
            droneValues.Add(color.b);

            //set the data
            dmxData.SetRange(channelStart + ((d.uid - 1) * droneValues.Count), droneValues.Count, droneValues.ToArray());
        }
        //Debug.Log(dmxData.Count);
        return;
    }


    public async UniTask SendData(CancellationToken cancellationToken)
    {
        while (Application.isPlaying)
        {
            //check cancellation token
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            //send data for all drones
                const int perUpdate = 10; //how many drones to process per update
            int currentCount = 0;
            foreach (Drone d in drones.Values)
            {
                d.SendGPSRaw();
                d.SendGPSFiltered();
                d.SendSYSStatus();
                currentCount++;
                if (currentCount == perUpdate)
                {
                    //wait for a bit before sending the next batch
                    await UniTask.Delay(20);
                    currentCount = 0;
                }
            }
            if (perUpdate > drones.Count)
            {
                //wait a bit before sending the next batch because we wont hit the call in the for loop
                await UniTask.Delay(100);
            }
        }
    }

    //update coroutine
    public async UniTask SendHeartbeat(CancellationToken cancellationToken)
    {
        while (Application.isPlaying)
        {
            //check cancellation token
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            //process X ammount every time
            const int perUpdate = 10; //how many drones to process per update
            int currentCount = 0;
            foreach (Drone d in drones.Values)
            {
                //send heartbeat
                d.SendHeartbeat();
                currentCount++;
                if (currentCount == perUpdate)
                {
                    //wait for a bit before sending the next batch
                    await UniTask.Delay(100);
                    currentCount = 0;
                }
            }
            if (perUpdate > drones.Count)
            {
                //wait a bit before sending the next batch because we wont hit the call in the for loop
                await UniTask.Delay(100);
            }
        }
    }
    Stream stream = new MemoryStream();
    public async UniTask ReceiveData(CancellationToken cancellationToken)
    {
        while (Application.isPlaying)
        {
            //check cancellation token
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            //listen for messages on 14555
            while (client.Available > 0)
            {
                byte[] buffer;
                try
                {
                    buffer = client.Receive(ref selfEndPoint);
                }
                //connection forcibly closed
                catch (SocketException ex)
                {
                    //Debug.LogWarning(ex.Message);
                    continue;
                }
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(buffer, 0, buffer.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var message = parse.ReadPacket(stream);

                MAVLINK_MSG_ID messageId = (MAVLINK_MSG_ID)message.msgid;

                //Debug.Log(messageId);

                switch (messageId)
                {
                    case MAVLINK_MSG_ID.COMMAND_LONG:
                        //figure out the command
                        var commandLong = (mavlink_command_long_t)message.data;
                        //find the drone based on the target sysid
                        bool found = drones.TryGetValue(commandLong.target_system, out Drone d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {commandLong.target_system}");
                            continue;
                        }

                        //Debug.Log((MAV_CMD)obj.command);
                        bool handled = false;
                        switch ((MAV_CMD)commandLong.command)
                        {
                            case MAV_CMD.REQUEST_AUTOPILOT_CAPABILITIES:
                                handled = true;
                                d.SendAutopilotCapabilities();
                                break;
                            case MAV_CMD.SET_MESSAGE_INTERVAL:
                                //we dont actually CARE about message intervals, so just ACK it
                                handled = true;
                                break;
                            case MAV_CMD.USER_1:
                                //show control information, such as reload show, remove show or test pyro
                                //https://github.com/skybrush-io/skybrush-server/blob/4fd65199a0578c56928981a709f0cabb69b15bd8/src/flockwave/server/ext/mavlink/enums.py#L629
                                //ack them automatically
                                switch (commandLong.param1)
                                {
                                    case 0:
                                        d.ReloadShow();
                                        break;
                                    case 1:
                                        //d.RemoveShow();
                                        break;
                                    case 2:
                                        //d.TestPyro();
                                        break;
                                }
                                handled = true;
                                break;
                            default:
                                //handle other commands
                                Debug.Log($"Unhandled command: {(MAV_CMD)commandLong.command}");
                                handled = false;
                                break;
                        }
                        if (handled)
                        {
                            d.SendCommandACK((MAV_CMD)commandLong.command, MAV_RESULT.ACCEPTED, message.sysid, message.compid);
                        }
                        break;
                    case MAVLINK_MSG_ID.COMMAND_INT:
                        //figure out the command
                        var commandInt = (mavlink_command_int_t)message.data;
                        //find the drone based on the target sysid
                        found = drones.TryGetValue(commandInt.target_system, out d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {commandInt.target_system}");
                            continue;
                        }

                        //Debug.Log((MAV_CMD)obj.command);
                        handled = false;
                        switch ((MAV_CMD)commandInt.command)
                        {
                            //USER_2 is show origin info
                            //https://github.com/skybrush-io/skybrush-server/blob/4fd65199a0578c56928981a709f0cabb69b15bd8/src/flockwave/server/ext/mavlink/driver.py#L2260
                            case MAV_CMD.USER_2:
                                handled = true;
                                d.SetShowOrigin(commandInt.x * 0.0000001f, commandInt.y * 0.0000001f, commandInt.z * 0.001f);
                                break;
                            default:
                                //handle other commands
                                Debug.Log($"Unhandled command: {(MAV_CMD)commandInt.command}");
                                handled = false;
                                break;
                        }
                        if (handled)
                        {
                            d.SendCommandACK((MAV_CMD)commandInt.command, MAV_RESULT.ACCEPTED, message.sysid, message.compid);
                        }
                        break;
                    case MAVLINK_MSG_ID.PARAM_REQUEST_READ:
                        var paramRequestRead = (mavlink_param_request_read_t)message.data;
                        //find the drone based on the target sysid
                        found = drones.TryGetValue(paramRequestRead.target_system, out d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {paramRequestRead.target_system}");
                            continue;
                        }

                        //convert the byte array to a string
                        string paramName = Encoding.UTF8.GetString(paramRequestRead.param_id).TrimEnd('\0');
                        //get the parameter type
                        found = d.parameters.TryGetValue(paramName, out float paramValue);
                        if (!found)
                        {
                            //Debug.LogError($"Parameter not found: {paramName}, sending 0");
                            paramValue = 0f;
                        }

                        //send it back
                        var paramValueMessage = new mavlink_param_value_t(
                            paramValue,
                            (ushort)d.parameters.Count,
                            (ushort)d.parameters.Keys.ToList().IndexOf(paramName),
                            paramRequestRead.param_id,
                            (byte)MAV_PARAM_TYPE.REAL32
                        );
                        d.Transmit(MAVLINK_MSG_ID.PARAM_VALUE, paramValueMessage);
                        break;
                    case MAVLINK_MSG_ID.PARAM_SET:
                        var paramSet = (mavlink_param_set_t)message.data;
                        //find the drone based on the target sysid
                        found = drones.TryGetValue(paramSet.target_system, out d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {paramSet.target_system}");
                            continue;
                        }

                        //convert the byte array to a string
                        paramName = Encoding.UTF8.GetString(paramSet.param_id).TrimEnd('\0');
                        d.SetParameter(paramName, paramSet.param_value);
                        //send it back as a confirmation
                        var paramSetMessage = new mavlink_param_value_t(
                            paramSet.param_value,
                            (ushort)d.parameters.Count,
                            (ushort)d.parameters.Keys.ToList().IndexOf(paramName),
                            paramSet.param_id,
                            (byte)MAV_PARAM_TYPE.REAL32
                        );
                        d.Transmit(MAVLINK_MSG_ID.PARAM_VALUE, paramSetMessage);
                        //Debug.Log($"Parameter set: {paramName} = {paramSet.param_value}");
                        break;
                    case MAVLINK_MSG_ID.MISSION_COUNT:
                        var missionCountMessage = (mavlink_mission_count_t)message.data;
                        //find the drone based on the target sysid
                        found = drones.TryGetValue(missionCountMessage.target_system, out d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {missionCountMessage.target_system}");
                            continue;
                        }

                        //switch on the mission type itself, we dont care about geofencing
                        switch ((MAV_MISSION_TYPE)missionCountMessage.mission_type)
                        {
                            case MAV_MISSION_TYPE.FENCE:
                                break;
                            default:
                                Debug.LogWarning($"Unhandled mission type: {(MAV_MISSION_TYPE)missionCountMessage.mission_type}");
                                break;
                        }

                        //acknowledge the mission count
                        d.SendMissionACK(MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED, (MAV_MISSION_TYPE)missionCountMessage.mission_type, message.sysid, message.compid);
                        break;
                    case MAVLINK_MSG_ID.FILE_TRANSFER_PROTOCOL:
                        var ftpobj = (mavlink_file_transfer_protocol_t)message.data;
                        //find the drone based on the target sysid
                        found = drones.TryGetValue(ftpobj.target_system, out d);

                        if (!found)
                        {
                            Debug.LogError($"Invalid system target: {ftpobj.target_system}");
                            continue;
                        }
                        //convert to a ftpmessage packet
                        var ftpIncomingMessage = MavlinkUtil.ByteArrayToStructureGC<FTPMessage>(ftpobj.payload, 0);
                        //print all the data on it
                        //Debug.Log($"FTP Message: Seq: {ftpIncomingMessage.seq_number}, Session: {ftpIncomingMessage.session}, Opcode: {ftpIncomingMessage.opcode}, Size: {ftpIncomingMessage.size}, ReqOpcode: {ftpIncomingMessage.req_opcode}, BurstComplete: {ftpIncomingMessage.burst_complete}, Offset: {ftpIncomingMessage.offset}, Data Length: {ftpIncomingMessage.data.Length}");
                        FTPMessage? sendMessage = null;
                        switch (ftpIncomingMessage.opcode)
                        {
                            case FTPMessage.ftp_opcode.CreateFile:
                                //clear out the existing file data
                                d.showFileRaw.Clear();
                                //reply with ack because we don't actually care about the file name
                                sendMessage = new FTPMessage(
                                    (ushort)(ftpIncomingMessage.seq_number + 1),
                                    ftpIncomingMessage.session,
                                    FTPMessage.ftp_opcode.ACK,
                                    0,
                                    ftpIncomingMessage.opcode,
                                    1,
                                    0,
                                    null
                                );
                                break;
                            case FTPMessage.ftp_opcode.WriteFile:
                                //ensure the list is large enough
                                //d.showFileRaw.EnsureCapacity((int)(ftpIncomingMessage.size + ftpIncomingMessage.offset));
                                //write the data to the list
                                d.showFileRaw.SetRange((int)ftpIncomingMessage.offset, ftpIncomingMessage.size, ftpIncomingMessage.data);
                                //ack the write
                                sendMessage = new FTPMessage(
                                    (ushort)(ftpIncomingMessage.seq_number + 1),
                                    ftpIncomingMessage.session,
                                    FTPMessage.ftp_opcode.ACK,
                                    0,
                                    ftpIncomingMessage.opcode,
                                    1,
                                    0,
                                    null
                                );
                                break;
                            case FTPMessage.ftp_opcode.TerminateSession:
                                //print the total size of the file
                                //Debug.Log($"File received with size: {d.showFileRaw.Count} bytes");
                                //ack the termination
                                sendMessage = new FTPMessage(
                                    (ushort)(ftpIncomingMessage.seq_number + 1),
                                    ftpIncomingMessage.session,
                                    FTPMessage.ftp_opcode.ACK,
                                    0,
                                    ftpIncomingMessage.opcode,
                                    1,
                                    0,
                                    null
                                );
                                break;
                            case FTPMessage.ftp_opcode.CalcFileCRC32:
                                //calculate the CRC32 of the received data
                                var crcParams = new CrcParameters(
                                    32,
                                    Convert.ToUInt64("0x04C11DB7", 16),
                                    0,
                                    0,
                                    true,
                                    true
                                );

                                var crc32 = new Crc(crcParams);

                                var crc = crc32.CalculateAsNumeric(d.showFileRaw.ToArray());

                                //Debug.Log($"Calculated CRC32: {crc:X8} for file with size: {d.showFileRaw.Count} bytes");

                                //conver the CRC to a byte array
                                byte[] crcBytes = BitConverter.GetBytes(crc);

                                //ack with the CRC32 value
                                sendMessage = new FTPMessage(
                                    (ushort)(ftpIncomingMessage.seq_number + 1),
                                    ftpIncomingMessage.session,
                                    FTPMessage.ftp_opcode.ACK,
                                    (byte)crcBytes.Length,
                                    ftpIncomingMessage.opcode,
                                    1,
                                    0,
                                    crcBytes
                                );
                                break;
                            case FTPMessage.ftp_opcode.ResetSessions:
                                //reply with ack because we don't actually care about the file name
                                sendMessage = new FTPMessage(
                                    (ushort)(ftpIncomingMessage.seq_number + 1),
                                    ftpIncomingMessage.session,
                                    FTPMessage.ftp_opcode.ACK,
                                    0,
                                    ftpIncomingMessage.opcode,
                                    1,
                                    0,
                                    null
                                );
                                break;
                            default:
                                Debug.Log($"Unhandled FTP message: {ftpIncomingMessage.opcode}");
                                break;
                        }

                        if (sendMessage != null)
                        {
                            d.SendFTPMessage(sendMessage.Value, message.sysid, message.compid);
                        }
                        break;
                    default:
                        Debug.Log($"Unhandled message: {messageId}");
                        break;
                }
                }

            await UniTask.Delay(1);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 251)]
    public struct FTPMessage
    {
        public ushort seq_number;
        public byte session;
        public ftp_opcode opcode;
        public byte size;
        public ftp_opcode req_opcode;
        public byte burst_complete;
        public byte padding;
        public uint offset;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 251 - 12)]
        public byte[] data;

        public FTPMessage(ushort seq_number, byte session, ftp_opcode opcode, byte size, ftp_opcode req_opcode, byte burst_complete, uint offset, byte[] data)
        {
            this.seq_number = seq_number;
            this.session = session;
            this.opcode = opcode;
            this.size = size;
            this.req_opcode = req_opcode;
            this.burst_complete = burst_complete;
            this.padding = 0; //padding to align to 4 bytes
            this.offset = offset;
            this.data = data ?? new byte[251 - 12];
        }

        public enum ftp_opcode : byte
        {
            None = 0,
            TerminateSession = 1,
            ResetSessions = 2,
            ListDirectory = 3,
            OpenFileRO = 4,
            ReadFile = 5,
            CreateFile = 6,
            WriteFile = 7,
            RemoveFile = 8,
            CreateDirectory = 9,
            RemoveDirectory = 10,
            OpenFileWO = 11,
            TruncateFile = 12,
            Rename = 13,
            CalcFileCRC32 = 14,
            BurstReadFile = 15,



            ACK = 128,
            NAK = 129,
        }
    }

    public class Drone
    {
        private MavlinkParse parse;
        public byte uid; // the valid range for this is 1 to 255

        //lat long and altitude are fixed point numbers
        private Vector3 latlongaltposition = Vector3.zero;

        private Vector3 showOrigin = Vector3.zero;
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

        public Color32 GetDroneColor()
        {
            if (showFile != null)
            {
                //get color at time
                return showFile.GetColorAtRealTime(DateTime.Now);
            }
            return Color.black;
        }

        private double measure(float lat1, float lon1, float lat2, float lon2)
        {  // generally used geo measurement function
            var R = 6378.137; // Radius of earth in KM
            var dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
            var dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000; // meters
        }

        DateTime GetFromGps(int weeknumber, double seconds)
        {
            DateTime datum = new DateTime(1980,1,6,0,0,0);
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
            var message = new mavlink_gps_raw_int_t(
                //send current unix time in seconds
                (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                (int)(latlongaltposition.x * 10000000),
                (int)(latlongaltposition.y * 10000000),
                (int)(latlongaltposition.z * 1000),
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
            var message = new mavlink_global_position_int_t(
                (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                (int)(latlongaltposition.x * 10000000),
                (int)(latlongaltposition.y * 10000000),
                (int)(latlongaltposition.z * 1000),
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
        public class ShowFile
        {
            //programs
            public List<LightEvent> LightProgram = new();
            public int LightProgramPointer = 0;
            public List<Trajectory> TrajectoryProgram = new();
            public int TrajectoryProgramPointer = 0;
            const int PointerLookahead = 5;

            public DateTime showStartTime;

            public ShowFile(List<byte> rawData)
            {
                //make a copy to operate on
                Queue<byte> fileData = new Queue<byte>(rawData);

                //file header is the first 10 bytes, pull that out of the buffer
                List<byte> header = fileData.DequeueChunk(10).ToList();

                //next we should have a block header that is 3 bytes long.
                while (fileData.Count > 0)
                {
                    ExtractBlock(fileData);
                }
            }

            private void ExtractBlock(Queue<byte> fileData)
            {
                //the first byte is the block type
                //the next 2 bytes are the block size
                BlockType blockType = (BlockType)fileData.DequeueChunk(1).First();
                int blockSize = BitConverter.ToUInt16(fileData.DequeueChunk(2).ToArray(), 0);
                //Debug.Log($"Block Type: {blockType}, Block Size: {blockSize}");
                Queue<byte> blockData = new(fileData.DequeueChunk(blockSize));

                switch (blockType)
                {
                    case BlockType.LIGHT_PROGRAM:
                        while (blockData.Count > 0)
                        {
                            LightProgram.Add(new LightEvent(ref blockData));
                        }
                        //compute the start and end time for all of the events
                        if (LightProgram.Count > 0)
                        {
                            //set the start time of the first event
                            LightProgram[0].startTime = TimeSpan.Zero;
                            for (int i = 1; i < LightProgram.Count; i++)
                            {
                                //setup the end time of the last light event
                                LightProgram[i - 1].endTime = LightProgram[i - 1].startTime + LightProgram[i - 1].duration;
                                LightProgram[i].startTime = LightProgram[i - 1].endTime;
                            }

                            //setup the cached previous event value
                            //cache the previous color and the start time
                            Color32 lastColor = Color.black;
                            foreach (var ev in LightProgram)
                            {
                                ev.previousEventColor = lastColor; //set the previous event color to the last color

                                if (ev.setsColor) //only do it if this event defines a color. This will make sure that the color is the latest event WITH a color define
                                {
                                    lastColor = ev.color; //if no color is set, use black
                                }
                            }

                            //sort it
                            LightProgram.Sort((a, b) => a.startTime.CompareTo(b.startTime));
                            //reset the program counter
                            LightProgramPointer = 0;
                        }

                        //debug print start and end time of events
                        /* foreach (var lightProgram in LightProgram)
                        {
                            Debug.Log($"Light Event: Opcode: {lightProgram.opcode}, Start Time: {lightProgram.startTime}, End Time: {lightProgram.endTime}, Duration: {lightProgram.duration}, Color: {lightProgram.color}, Counter: {lightProgram.counter}, Address: {lightProgram.address}");
                        } */
                        break;
                    case BlockType.TRAJECTORY:
                        //decode the scale/flags, and the start xyz
                        //flags is the first byte
                        byte flags = blockData.DequeueChunk(1).First();
                        //MSB is unused, scale is the remaining 7 bits
                        // 0x7f is 01111111
                        byte scale = (byte)(flags & 0x7F);
                        Vector3 startPos = Trajectory.DecodeStartSpatialCoordinates(ref blockData, scale);
                        float startYaw = Trajectory.DecodeAngleCoordinate(ref blockData);

                        //print this info
                        //Debug.Log($"Scale: {scale}, Trajectory Start: {startPos}, Yaw: {startYaw}");

                        while (blockData.Count > 0)
                        {
                            TrajectoryProgram.Add(new Trajectory(ref blockData, startPos, startYaw, scale));
                            //next startpos is the trajectory we just added
                            startPos = TrajectoryProgram.Last().lastPosition;
                            startYaw = TrajectoryProgram.Last().yawControlPoints.Last();
                        }

                        //compute the start and end time for all of the events
                        if (TrajectoryProgram.Count > 0)
                        {
                            //set the start time of the first event
                            TrajectoryProgram[0].startTime = TimeSpan.Zero;
                            for (int i = 1; i < TrajectoryProgram.Count; i++)
                            {
                                //setup the end time of the last trajectory
                                TrajectoryProgram[i - 1].endTime = TrajectoryProgram[i - 1].startTime + TrajectoryProgram[i - 1].duration;
                                TrajectoryProgram[i].startTime = TrajectoryProgram[i - 1].endTime;
                            }

                            //sort it
                            TrajectoryProgram.Sort((a, b) => a.startTime.CompareTo(b.startTime));
                            //reset the program counter
                            TrajectoryProgramPointer = 0;
                        }
                        break;
                    default:
                        Debug.LogWarning($"Unhandled block type: {blockType}");
                        break;
                }
            }

            public Vector3 GetPositionAtRealTime(DateTime time)
            {
                //convert the time to a timespan since the show start time
                TimeSpan elapsed = time - showStartTime;
                return GetPositionAtTime(elapsed);
            }

            public Vector3 GetPositionAtTime(TimeSpan time)
            {
                //if we are PAST the last one, just use that final value
                if (time > TrajectoryProgram.Last().endTime)
                {
                    return TrajectoryProgram.Last().evaluate(1.0f);
                }

                //if we are before the first one, just use the first one
                if (time < TrajectoryProgram.First().startTime)
                {
                    return TrajectoryProgram.First().evaluate(0.0f);
                }

                //check if we are not in a light event now
                if (!TrajectoryProgram[TrajectoryProgramPointer].InsideEvent(time))
                {
                    //look ahead at the next 5 and see if we are in one of them
                    for (int i = TrajectoryProgramPointer + 1; i < TrajectoryProgram.Count && i < TrajectoryProgramPointer + PointerLookahead; i++)
                    {
                        if (TrajectoryProgram[i].InsideEvent(time))
                        {
                            //we are in this light event, set the pointer to that
                            TrajectoryProgramPointer = i;
                            break;
                        }
                    }
                }

                Trajectory tevent = TrajectoryProgram[TrajectoryProgramPointer];

                //get the time inside the bezier curve
                float t = (float)((time - tevent.startTime) / tevent.duration);
                return tevent.evaluate(t);
            }

            public Color32 GetColorAtRealTime(DateTime time)
            {
                //convert the time to a timespan since the show start time
                TimeSpan elapsed = time - showStartTime;
                return GetColorAtTime(elapsed);
            }

            public Color32 GetColorAtTime(TimeSpan time)
            {
                //check if we are not in a light event now
                if (!LightProgram[LightProgramPointer].InsideEvent(time))
                {
                    //look ahead at the next 5 and see if we are in one of them
                    for (int i = LightProgramPointer + 1; i < LightProgram.Count && i < LightProgramPointer + PointerLookahead; i++)
                    {
                        if (LightProgram[i].InsideEvent(time))
                        {
                            //we are in this light event, set the pointer to that
                            LightProgramPointer = i;
                            break;
                        }
                    }
                }

                LightEvent startevent = LightProgram[LightProgramPointer];

                //if this event is a fade
                if (startevent.IsFade)
                {
                    //lerp between the last color and the current color
                    if (startevent.setsColor)
                    {
                        //if the color is set, use it
                        return Color32.Lerp(startevent.previousEventColor, startevent.color, (float)(time - startevent.startTime).TotalMilliseconds / (float)startevent.duration.TotalMilliseconds);
                    }
                }

                //otherwise its a instantaneous set command
                if (startevent.setsColor)
                {
                    //if the color is set, use it
                    return startevent.color;
                }
                return startevent.previousEventColor;
            }

            enum BlockType : byte
            {
                TRAJECTORY = 1,
                LIGHT_PROGRAM = 2,
                COMMENT = 3,
                RTH_PLAN = 4,
                YAW_CONTROL = 5,
                EVENT_LIST = 6
            }

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

            public class LightEvent
            {
                public TimeSpan duration;
                public Color32 color;
                public Color32 previousEventColor = new Color32(0, 0, 0, 255); //default to black as the previous color
                public bool setsColor => opcode switch
                {
                    Opcode.SET_COLOR or
                    Opcode.SET_COLOR_FROM_CHANNELS or
                    Opcode.SET_GRAY or
                    Opcode.FADE_TO_COLOR or
                    Opcode.FADE_TO_COLOR_FROM_CHANNELS or
                    Opcode.FADE_TO_GRAY or
                    Opcode.FADE_TO_BLACK or
                    Opcode.FADE_TO_WHITE => true,
                    _ => false,
                };
                public Opcode opcode;
                public byte? counter = null;
                public int? address = null;
                public TimeSpan startTime = new TimeSpan(-50);
                public TimeSpan endTime = new TimeSpan(-1);

                public bool IsFade => opcode switch
                {
                    Opcode.FADE_TO_COLOR or
                    Opcode.FADE_TO_COLOR_FROM_CHANNELS or
                    Opcode.FADE_TO_GRAY or
                    Opcode.FADE_TO_BLACK or
                    Opcode.FADE_TO_WHITE => true,
                    _ => false,
                };

                public LightEvent(ref Queue<byte> data)
                {
                    //pop the first byte to get the opcode
                    opcode = (Opcode)data.DequeueChunk(1).First();
                    //Debug.Log($"Opcode: {opcode}");

                    //assign duration as 0
                    duration = TimeSpan.Zero;

                    byte tempByte;

                    switch (opcode)
                    {
                        case Opcode.END:
                        case Opcode.NOP:
                        case Opcode.LOOP_END:
                        case Opcode.RESET_CLOCK:
                        case Opcode.UNUSED_1:
                            break;
                        case Opcode.SLEEP:
                        case Opcode.WAIT_UNTIL: //this one isnt technically correct but its not used anymore anyway so whatever
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.SET_COLOR:
                        case Opcode.SET_COLOR_FROM_CHANNELS: //99% sure this works the same way
                            color = new Color32(
                                data.DequeueChunk(1).First(),
                                data.DequeueChunk(1).First(),
                                data.DequeueChunk(1).First(),
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.SET_GRAY:
                            tempByte = data.DequeueChunk(1).First();
                            color = new Color32(
                                tempByte,
                                tempByte,
                                tempByte,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.SET_BLACK:
                            color = new Color32(
                                0,
                                0,
                                0,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.SET_WHITE:
                            color = new Color32(
                                255,
                                255,
                                255,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.FADE_TO_COLOR:
                        case Opcode.FADE_TO_COLOR_FROM_CHANNELS:
                            color = new Color32(
                                data.DequeueChunk(1).First(),
                                data.DequeueChunk(1).First(),
                                data.DequeueChunk(1).First(),
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.FADE_TO_GRAY:
                            tempByte = data.DequeueChunk(1).First();
                            color = new Color32(
                                tempByte,
                                tempByte,
                                tempByte,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.FADE_TO_BLACK:
                            color = new Color32(
                                0,
                                0,
                                0,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.FADE_TO_WHITE:
                            color = new Color32(
                                255,
                                255,
                                255,
                                255
                            );
                            duration = GetDuration(ref data);
                            break;
                        case Opcode.LOOP_BEGIN:
                            counter = data.DequeueChunk(1).First();
                            break;
                        case Opcode.JUMP:
                            address = GetVarInt(ref data);
                            break;
                        default:
                            throw new NotImplementedException($"Unhandled opcode: {opcode}");
                    }

                    //Debug.Log($"Light Event: Opcode: {opcode}, Duration: {duration}, Color: {color}, Counter: {counter}, Address: {address}");
                }

                private static int GetVarInt(ref Queue<byte> data)
                {
                    //duration is encoded in varint format
                    //MSB is 1 if the integer continues to the next byte, 0 if it is the last byte
                    //duration is encoded as number of frames at 50 FPS, so each value is 20ms
                    int val = 0;
                    int shift = 0;
                    byte b;
                    do
                    {
                        b = data.DequeueChunk(1).First();
                        val |= (b & 0x7F) << shift;
                        shift += 7;
                    } while ((b & 0x80) != 0);

                    return val;
                }

                public bool InsideEvent(TimeSpan time)
                {
                    //check if the time is inside the event
                    return time >= startTime && time < endTime;
                }

                private static TimeSpan GetDuration(ref Queue<byte> data)
                {
                    return TimeSpan.FromMilliseconds(GetVarInt(ref data) * 20);
                }

                //https://github.com/skybrush-io/libskybrush/blob/9ecf48d6fcf258c5be77bf31d88a91241b1a5700/src/lights/commands.h#L39
                //https://github.com/skybrush-io/libskybrush/blob/9ecf48d6fcf258c5be77bf31d88a91241b1a5700/src/lights/commands.cpp#L104
                public enum Opcode
                {
                    END = 0,
                    NOP = 1,
                    SLEEP = 2,
                    WAIT_UNTIL = 3, //apparently not used anymore
                    SET_COLOR = 4,
                    SET_GRAY = 5,
                    SET_BLACK = 6,
                    SET_WHITE = 7,
                    FADE_TO_COLOR = 8,
                    FADE_TO_GRAY = 9,
                    FADE_TO_BLACK = 10,
                    FADE_TO_WHITE = 11,
                    LOOP_BEGIN = 12, //ignore, shouldnt be used anymore
                    LOOP_END = 13, //ignore, shouldnt be used anymore
                    RESET_CLOCK = 14, //ignore, shouldnt be used anymore
                    UNUSED_1 = 15, //ignore, shouldnt be used anymore
                    SET_COLOR_FROM_CHANNELS = 16, //ignore, shouldnt be used anymore
                    FADE_TO_COLOR_FROM_CHANNELS = 17, //ignore, shouldnt be used anymore
                    JUMP = 18, //ignore, shouldnt be used anymore
                    TRIGGERED_JUMP = 19, //ignored for now
                    SET_PYRO = 20, //ignored for now TODO: Implement for pyro control
                    SET_PYRO_ALL = 21, //ignored for now TODO: Implement for pyro control
                    NUMBER_OF_COMMANDS, //automatic assignment to match the number of commands
                }
            }
        }
    }
}
