using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        //startup tasks
        UniTask.Void(SendData);
        UniTask.Void(SendHeartbeat);
        UniTask.Void(ReceiveData);
    }

    public void GenerateDMX(ref List<byte> dmxData)
    {
        return;
    }


    public async UniTaskVoid SendData()
    {
        while (Application.isPlaying)
        {
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
    public async UniTaskVoid SendHeartbeat()
    {
        while (Application.isPlaying)
        {
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
    public async UniTaskVoid ReceiveData()
    {
        while (Application.isPlaying)
        {
            //listen for messages on 14555
            if (client.Available > 0)
            {
                byte[] buffer = client.Receive(ref selfEndPoint);
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
                                d.SetShowOrigin(commandInt.x, commandInt.y, commandInt.z);
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
                                d.showFileRaw.EnsureCapacity((int)(ftpIncomingMessage.size + ftpIncomingMessage.offset));
                                //write the data to the list
                                d.showFileRaw.InsertRange((int)ftpIncomingMessage.offset, ftpIncomingMessage.data.Take((int)ftpIncomingMessage.size));
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
                                    false,
                                    false
                                );

                                var crc32 = new Crc(crcParams);

                                var crc = crc32.CalculateAsNumeric(d.showFileRaw.ToArray());

                                //Debug.Log($"Calculated CRC32: {crc:X8} for file with size: {d.showFileRaw.Count} bytes");

                                //fake the CRC for now
                                //TODO: Actually get this shit workin frfr
                                crc = Convert.ToUInt64("0x4B6F7F6B", 16);

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

            await UniTask.Delay(5);
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
        private byte uid; // the valid range for this is 1 to 255

        //lat long and altitude are fixed point numbers
        private Vector3 latlongaltposition = Vector3.zero;

        private Vector3 showOrigin = Vector3.zero;

        UdpClient client;
        IPEndPoint remoteEndPoint;

        public List<byte> showFileRaw = new List<byte>();

        public Drone(byte uid, UdpClient client, IPEndPoint remoteEndPoint)
        {
            //latlongaltposition = new Vector3(1,1,1);
            this.uid = uid; //0 is not valid
            this.client = client;
            this.remoteEndPoint = remoteEndPoint;
            //SetPosition(globalID * 0.0001f, globalID * 0.0001f, globalID * 0.0001f); // set initial position

            parse = new MavlinkParse();
        }

        public void SetShowOrigin(float lat, float lon, float alt)
        {
            showOrigin = new Vector3(lat * 0.0000001f, lon * 0.0000001f, alt * 0.001f);
            latlongaltposition = showOrigin;
            Debug.Log($"Drone {uid} set show origin to: {showOrigin}");
        }

        public void SendHeartbeat()
        {
            var message = new mavlink_heartbeat_t(
                127,
                (byte)MAV_TYPE.GENERIC,
                (byte)MAV_AUTOPILOT.GENERIC,
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
            var message = new mavlink_autopilot_version_t(
                (ulong)MAV_PROTOCOL_CAPABILITY.SET_POSITION_TARGET_GLOBAL_INT,
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

        public void SendFTPMessage(FTPMessage ftpMessage, byte target_system, byte target_component)
        {
            //convert to a mavlink_file_transfer_protocol_t
            var ftpobj = new mavlink_file_transfer_protocol_t
            (
                0,
                target_system = target_system,
                target_component = target_component,
                MavlinkUtil.StructureToByteArray(ftpMessage)
            );

            Transmit(MAVLINK_MSG_ID.FILE_TRANSFER_PROTOCOL, ftpobj);
        }

        public int FTPSEQ = 0;

        public void SendFTPACK()
        {

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
    }
}
