using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SFB;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class CommandPacket
{
    public CommandType command;
    public int frame_number;
    public string file_path;
}

public enum CommandType
{
    save_frame,
}

public class FrameSnapshotExporter : IExporter
{
    private bool imageSaveQueued = false;
    private CommandPacket latestCommandPacket;
    private UdpClient _udpClient;
    private CancellationTokenSource _cts;

    public void CompleteFrame(ref List<byte> channelValues) {}
    public void Construct()
    {
        _cts = new CancellationTokenSource();
        _udpClient = new UdpClient(9123);

        // Run the listener in a background task
        Task.Run(() => ListenForPackets(_cts.Token));
    }

    private async Task ListenForPackets(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Receive a packet
                UdpReceiveResult receivedBytes = await _udpClient.ReceiveAsync();

                // Convert the received bytes to a string (assuming UTF-8 encoding)
                string receivedMessage = System.Text.Encoding.UTF8.GetString(receivedBytes.Buffer);

                // Trigger the event
                PacketReceived(receivedMessage);
            }
            catch (SocketException ex)
            {
                Debug.LogError($"SocketException: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                // This exception is expected when the UdpClient is closed, so we can ignore it.
                break;
            }
        }
    }

    private void PacketReceived(string message)
    {
        try
        {
            CommandPacket packet = JsonUtility.FromJson<CommandPacket>(message);
            // Debug.Log($"Packet received - Command: {packet.command}, Frame: {packet.frame_number}, File Path: {packet.file_path}");
            
            if (packet.command == CommandType.save_frame)
            {
                imageSaveQueued = true;
            }

            latestCommandPacket = packet;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse JSON packet: {ex.Message}");
        }
    }

    public void Deconstruct()
    {
        _cts?.Cancel();
        _udpClient?.Close();
    }
    public void InitFrame(ref List<byte> channelValues) {}
    public void FrameRendered(ref Texture2D texture) {
        if (!imageSaveQueued)
        {
            return; // No save requested
        }

        if (texture == null)
        {
            Debug.LogError("Texture is null. Cannot save frame.");
            return;
        }

        // Save the texture to a PNG file
        byte[] bytes = texture.EncodeToPNG();
        // Ensure the directory exists
        string directory = System.IO.Path.GetDirectoryName(latestCommandPacket.file_path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        System.IO.File.WriteAllBytes(latestCommandPacket.file_path, bytes);
        // Debug.Log($"Frame {latestCommandPacket.frame_number} saved to {latestCommandPacket.file_path}");

        imageSaveQueued = false;
    }
    public void SerializeChannel(byte channelValue, int channel) {}
    public void ConstructUserInterface(RectTransform rect)
    {
        
    }
    public void DeconstructUserInterface() {}
    public void UpdateUserInterface() {}
}
