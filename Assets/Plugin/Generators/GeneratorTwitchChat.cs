using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Lexone.UnityTwitchChat;
using CircularBuffer;

public class TwitchChat : Text
{
    public string ChannelName = "";
    public EquationNumber chatMessages = 15;

    private IRC irc;
    private GameObject managedObject;
    private CircularBuffer<Chatter> buf;

    public override void Construct()
    {
        base.Construct();

        buf = new CircularBuffer<Chatter>(chatMessages);

        //we need a object because the IRC client needs to run on a MonoBehaviour
        managedObject = new GameObject("TwitchChatManager");

        irc = managedObject.AddComponent<IRC>();

        //ugh useAnonymousLogin is private, use reflection to set it
        var type = irc.GetType();
        var field = type.GetField("useAnonymousLogin", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field == null)
        {
            Debug.LogError("Failed to find useAnonymousLogin field in IRC class.");
            return;
        }
        field.SetValue(irc, true);

        irc.channel = ChannelName;
        irc.showIRCDebug = false;
        irc.showThreadDebug = false;

        irc.OnChatMessage += (sender) =>
        {
            buf.PushBack(sender);
            text = "";
            foreach (var chatter in buf)
            {
                text += $"{chatter.login}: {chatter.message}\n";
            }
        };
    }

    public override void Deconstruct()
    {
        base.Deconstruct();
        if (managedObject != null)
        {
            UnityEngine.Object.Destroy(managedObject);
        }
    }

    private protected TMP_InputField ChannelNameInputField;
    private protected TMP_InputField ChatMessagesInputField;
    public override void ConstructUserInterface(RectTransform rect)
    {
        base.ConstructUserInterface(rect);

        ChannelNameInputField = Util.AddInputField(rect, "Channel");
        ChannelNameInputField.text = ChannelName;
        ChannelNameInputField.onValueChanged.AddListener((value) =>
        {
            ChannelName = value;
            if (irc != null)
            {
                irc.channel = ChannelName;
            }
        });

        ChatMessagesInputField = Util.AddInputField(rect, "Max Messages");
        ChatMessagesInputField.text = chatMessages;
        ChatMessagesInputField.onValueChanged.AddListener((value) =>
        {
            chatMessages = value;
            
            buf = new CircularBuffer<Chatter>(chatMessages);
        });
    }
}
