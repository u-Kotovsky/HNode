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

    private int port = 7000;

    public void Construct()
    {
        //setup a UDP listener
        Task.Run(async () =>
        {
            using (var udpClient = new UdpClient(port))
            {
                while (true)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    var receivedResults = await udpClient.ReceiveAsync();

                    //data is expected as each channel value combination being a short,byte pair
                    Queue<byte> dataQueue = new Queue<byte>(receivedResults.Buffer);

                    while (dataQueue.Count > 0)
                    {
                        //read channel
                        if (dataQueue.Count < 2)
                            break; //not enough data

                        byte channelHigh = dataQueue.Dequeue();
                        byte channelLow = dataQueue.Dequeue();
                        int channel = (channelHigh << 8) | channelLow;

                        //read value
                        if (dataQueue.Count < 1)
                            break; //not enough data

                        byte value = dataQueue.Dequeue();

                        //update the channel data
                        channelData.AddOrUpdate(channel, value, (key, oldValue) => value);
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
