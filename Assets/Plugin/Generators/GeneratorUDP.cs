using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Lexone.UnityTwitchChat;
using CircularBuffer;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;

public class UDP : IDMXGenerator
{
    //used to asynchronously manage channel data
    ConcurrentDictionary<int, byte> channelData = new ConcurrentDictionary<int, byte>();

    private int port = 5065;

    public void Construct()
    {
        //setup a UDP listener
        Task.Run(async () =>
        {
            using (var udpClient = new UdpClient(port))
            {
                udpClient.Client.ReceiveBufferSize = 1024 * 8;
                while (true)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    var receivedResults = await udpClient.ReceiveAsync();

                    //data is expected in the format "channel:value", e.g. "1:255"
                    string receivedData = Encoding.UTF8.GetString(receivedResults.Buffer);
                    //additional encoding is using # for each key value pair
                    string[] pairs = receivedData.Split('#');
                    foreach (string pair in pairs)
                    {
                        string[] parts = pair.Split(':');
                        if (parts.Length == 2)
                        {
                            if (int.TryParse(parts[0], out int channel) && byte.TryParse(parts[1], out byte value))
                            {
                                //store the channel and value in the concurrent dictionary
                                channelData.AddOrUpdate(channel, value, (key, oldValue) => value);
                            }
                        }
                    }
                }
            }
        });
    }

    public void ConstructUserInterface(RectTransform rect)
    {
        // throw new NotImplementedException();
    }

    public void Deconstruct()
    {
        // throw new NotImplementedException();
    }

    public void DeconstructUserInterface()
    {
        // throw new NotImplementedException();
    }

    public void GenerateDMX(ref List<byte> dmxData)
    {
        //read from the concurrent dictionary and update the dmxData list
        foreach (var kvp in channelData)
        {
            int channel = kvp.Key;
            byte value = kvp.Value;

            //used as it ensures space
            Util.WriteToListAtPosition(dmxData, new List<byte> { value }, channel);
        }
    }

    public void UpdateUserInterface()
    {
        // throw new NotImplementedException();
    }
}
