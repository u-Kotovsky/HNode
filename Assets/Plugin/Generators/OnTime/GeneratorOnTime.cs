using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OnTimeAPI;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class OnTime : Text
{
    //http connection
    private HttpClient _client;

    public DMXChannel dataPayloadChannelStart = 0;

    public override void Construct()
    {
        base.Construct();

        _client = new HttpClient();
    }
    public override void GenerateDMX(ref List<byte> dmxData)
    {
        text = "";
        var task = Task.Run(async () =>
        {
            var response = await _client.GetAsync("http://localhost:4001/api/poll");

            //read as string
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        });
        task.Wait();
        var response = task.Result;
        Root data = JsonConvert.DeserializeObject<Root>(response);

        List<byte> dataPayload = new List<byte>();

        var currentTimeBytes = BitConverter.GetBytes(data.payload.clock);
        dataPayload.AddRange(currentTimeBytes);

        // split the time into hex
        var eventTimeBytes = BitConverter.GetBytes(data.payload.timer.current);
        dataPayload.AddRange(eventTimeBytes);

        //aux timer time
        var auxTimeBytes = BitConverter.GetBytes(data.payload.auxtimer1.current);
        dataPayload.AddRange(auxTimeBytes);

        //progression byte
        float progress = (float)data.payload.timer.elapsed / (float)data.payload.timer.duration;
        Debug.Log("Progress: " + progress);
        //map 0 to 1 -> 0 to 255
        dataPayload.Add((byte)(math.clamp(progress, 0f, 1f) * 255));

        //there is 2 text options, but only one is visible at a time
        //the timer message takes priority over the external message
        //determine which is visible using two flags
        //IF message.timer.visible is true, THAT is the message to show
        //if its FALSE, then check if secondarySource is external and if so, show that message
        //else, no message to show
        if (data.payload.message.timer.visible)
        {
            text = data.payload.message.timer.text;
        }
        else
        {
            if (data.payload.message.timer.secondarySource != null && (string)data.payload.message.timer.secondarySource == "external" && !string.IsNullOrEmpty(data.payload.message.external))
            {
                text = data.payload.message.external;
            }
            else
            {
                text = "";
            }
        }

        //push in some flags
        byte flags = 0;
        if (data.payload.message.timer.visible) flags |= 0b00000001;
        if (data.payload.message.timer.secondarySource != null && (string)data.payload.message.timer.secondarySource == "external") flags |= 0b00000010;
        if (data.payload.message.timer.blink) flags |= 0b00000100;
        if (data.payload.message.timer.blackout) flags |= 0b00001000;
        if (data.payload.message.timer.secondarySource != null && (string)data.payload.message.timer.secondarySource == "aux") flags |= 0b00010000;

        //add flags
        dataPayload.Add(flags);

        //write it to time channel
        dmxData.WriteToListAtPosition(dataPayload, dataPayloadChannelStart);

        //call base now that we have filled the text
        base.GenerateDMX(ref dmxData);
    }

    private protected TMP_InputField dataPayloadChannelStartInputfield;

    public override void ConstructUserInterface(RectTransform rect)
    {
        base.ConstructUserInterface(rect);

        //disable interaction
        if (textInputfield != null)
        {
            textInputfield.interactable = false;
        }

        dataPayloadChannelStartInputfield = Util.AddInputField(rect, "Data Payload Start Channel")
            .WithText(dataPayloadChannelStart)
            .WithCallback((value) => { dataPayloadChannelStart = value; });
    }
}
