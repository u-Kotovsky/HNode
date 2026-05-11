using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebDMX : WebSocketBehavior
{
    public static readonly List<WebDMX> Clients = new List<WebDMX>();
    
    protected override void OnMessage(MessageEventArgs e) { }

    protected override void OnOpen()
    {
        Debug.Log($"WebDMX Connected client {ID}");
        Clients.Add(this);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log($"WebDMX Disconnected client {ID}");
        Clients.RemoveAll(x => x.ID == ID);
    }

    public static void Broadcast(ref List<byte> channelValues)
    {
        var channelValues1 = channelValues.ToArray();
        
        foreach (var client in Clients)
        {
            client.Send(channelValues1);
        }
    }

    public static void BroadcastAsync(ref List<byte> channelValues, Action<bool> completed = null)
    {
        var channelValues1 = channelValues.ToArray();
        
        foreach (var client in Clients)
        {
            client.SendAsync(channelValues1, completed);
        }
    }
}

public class WebSocketExporter : IExporter
{
    private int targetPort = 5000;
    private List<byte> data = new List<byte>();

    public static WebSocketExporter Instance { get; private set; }

    private TMP_InputField _port, _userCounter;
    private Button _startButton, _stopButton;
    private Toggle _isListeningToggle;

    [CanBeNull] private WebSocketServer _server;
    
    private bool IsListening => _server != null && _server.IsListening;
    
    private void StartWebSocketServer()
    {
        _server?.Stop();
        _server = new WebSocketServer(targetPort);
        _server.AddWebSocketService<WebDMX>("/WebDMX");
        _server.Start();
        RefreshButtonStates();
    }

    private void StopWebSocketServer()
    {
        _server?.Stop();
        _userCounter.text = "-";
        RefreshButtonStates();
    }
    
    private void OnPortChanged(string value)
    {
        if (!int.TryParse(value, out targetPort))
        {
            return;
        }
        
        StopWebSocketServer();
        StartWebSocketServer();
    }
    
    private void RefreshButtonStates()
    {
        _startButton.interactable = !IsListening;
        _stopButton.interactable = IsListening;
        _isListeningToggle.isOn = IsListening;
    }
    
    public void ConstructUserInterface(RectTransform rect)
    {
        if (Instance != null)
        {
            throw new Exception("Only one instance of WebSocketExporter is allowed");
        }
        
        Instance = this;
        
        _port = Util.AddInputField(rect, "Port")
            .WithText(targetPort.ToString())
            .WithCallback(OnPortChanged);

        _isListeningToggle = Util.AddToggle(rect, "IsListening");
        _isListeningToggle.interactable = false;

        _userCounter = Util.AddInputField(rect, "User Counter");

        _startButton = Util.AddButton(rect, "Start")
            .WithCallback(StartWebSocketServer);
        
        _stopButton = Util.AddButton(rect, "Stop")
            .WithCallback(StopWebSocketServer);
        
        RefreshButtonStates();
    }
    public void UpdateUserInterface()
    {
        if (_server != null && _server.IsListening)
        {
            _userCounter.text = WebDMX.Clients.Count.ToString();
        }
        else
        {
            _userCounter.text = "-";
        }
    }
    
    public void CompleteFrame(ref List<byte> channelValues)
    {
        if (!IsListening)
        {
            return;
        }
        
        data = channelValues;

        WebDMX.Broadcast(ref data);
    }
    
    public void DeconstructUserInterface() { }
    public void Construct() { }
    public void Deconstruct() { }
    public void SerializeChannel(byte channelValue, int channel) { }
    public void InitFrame(ref List<byte> channelValues) { }

}
