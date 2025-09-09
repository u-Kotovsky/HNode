using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class DMXPacket : IDMXGenerator
{
    public virtual void Construct() { }
    public virtual void Deconstruct() { }
    
    private List<byte> lastFrameData = new List<byte>();
    private Dictionary<int, byte> lastdiff = new Dictionary<int, byte>();

    private int idleScanPointer = 0;
    private const int idleScanSize = 50;

    public virtual void GenerateDMX(ref List<byte> dmxData)
    {
        Dictionary<int, byte> diffs = new Dictionary<int, byte>();
        //look for differences to last frame
        for (int i = 0; i < dmxData.Count; i++)
        {
            if (i >= lastFrameData.Count || dmxData[i] != lastFrameData[i])
            {
                diffs[i] = dmxData[i];
            }
        }

        Debug.Log($@"DMX packet - Channels changed: {diffs.Count}");

        //if no diffs, use the last diff
        if (diffs.Count == 0)
        {
            diffs = new Dictionary<int, byte>(lastdiff);
        }
        else
        {
            lastdiff = new Dictionary<int, byte>(diffs);
        }

        //get a set of "packets" based on the diffs
        List<DMXPacketInfo> packets = new List<DMXPacketInfo>();
        //generate a idle scan packet first
        DMXPacketInfo idlePacket = new DMXPacketInfo();
        idlePacket.channelOffset = (ushort)(idleScanPointer);
        idlePacket.data = new List<byte>();
        //grab the range we want
        for (int i = idleScanPointer; i < idleScanSize + idleScanPointer; i++)
        {
            if (i < dmxData.Count)
            {
                idlePacket.data.Add(dmxData[i]);
                idlePacket.length++;
                //remove it from the diffs if it exists there
                if (diffs.ContainsKey(i))
                {
                    diffs.Remove(i);
                }
            }
            else
            {
                break;
            }
        }
        //reset the pointer 
        idleScanPointer += idleScanSize;
        if (idleScanPointer >= dmxData.Count)
        {
            idleScanPointer = 0;
        }
        packets.Add(idlePacket);

        //iterate over the diff
        for (int i = 0; i < diffs.Count; i++)
        {
            int startChannel = diffs.ElementAt(i).Key;
            byte length = 1;

            //look ahead to see if the next channel is also in the diff
            while (i + 1 < diffs.Count &&
                diffs.ElementAt(i + 1).Key == startChannel + length &&
                length < DMXPacketInfo.maxDataLength)
            {
                length++;
                i++;
            }

            //create a new packet
            DMXPacketInfo packet = new DMXPacketInfo();
            packet.channelOffset = (ushort)(startChannel);
            packet.length = length;
            packet.data = new List<byte>();

            //add the data to the packet
            for (int j = 0; j < length; j++)
            {
                packet.data.Add(diffs[startChannel + j]);
            }

            packets.Add(packet);
        }

        //save this frame as the last frame for next iteration
        lastFrameData = dmxData.ToArray().ToList();
        dmxData.Clear();

        //serialize the datapackets
        foreach (var packet in packets)
        {
            dmxData.AddRange(packet.Serialize());
        }
    }

    private struct DMXPacketInfo
    {
        public ushort channelOffset;
        public byte length;
        public const int maxDataLength = byte.MaxValue;
        public const int headerLength = 3; //2 bytes for channel offset, 1 byte for length
        public List<byte> data;

        public List<byte> Serialize()
        {
            List<byte> bytes = new List<byte>();
            //channel offset (2 bytes)
            bytes.Add((byte)(channelOffset & 0xFF)); //low byte
            bytes.Add((byte)((channelOffset >> 8) & 0xFF)); //high byte
            //length
            bytes.Add(length);
            //data
            bytes.AddRange(data);

            return bytes;
        }
    }

    public virtual void ConstructUserInterface(RectTransform rect)
    {

    }

    public void DeconstructUserInterface()
    {
        //throw new NotImplementedException();
    }

    public void UpdateUserInterface()
    {
        
    }
}
