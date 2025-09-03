using ArtNet;
using UnityEngine;
using WebSocketSharp.Server;

namespace Plugin.Receivers
{
    public class WebSocketReceiver : MonoBehaviour
    {
        public ArtNetReceiver artNetReceiver;
        public static WebSocketReceiver Instance { get; private set; }
        private WebSocketServer wsServer;
        private string _serverUrl = "ws://127.0.0.1:4546";
        private string _servicePath = "/dmx512";

        public void Initialize()
        {
            wsServer = new WebSocketServer(_serverUrl);
            wsServer.AddWebSocketService<WebSocketDMXBehaviour>(_servicePath);
            Debug.Log($"{nameof(WebSocketReceiver)} was initialized with url: '{_serverUrl}{_servicePath}'.");
        }

        public void Uninitialize()
        {
            Debug.Log($"{nameof(WebSocketReceiver)} was uninitialized.");
            wsServer = null;
        }

        public void StartServer()
        {
            wsServer.Start();
            Debug.Log($"{nameof(WebSocketReceiver)} started");
        }

        public void StopServer()
        {
            Debug.Log($"{nameof(WebSocketReceiver)} was stopped.");
            wsServer.Stop();
        }

        public void ChangeUrl(string url)
        {
            string[] args = url.Split("/");
            // count of / is 3
            _serverUrl = $"{args[0]}//{args[2]}";
            _servicePath = $"/{args[3]}";
            
            StopServer();
            Uninitialize();
            Initialize();
            StartServer();
            
            Debug.Log($"{nameof(WebSocketReceiver)} Url changed to '{_serverUrl}{_servicePath}'");
            StopServer();
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{nameof(WebSocketReceiver)} instance already exists");
                enabled = false;
                return;
            }
            Instance = this;
            Initialize();
            StartServer();
        }

        private void OnDestroy()
        {
            StopServer();
            Uninitialize();
        }
    }
}
