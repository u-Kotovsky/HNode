using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Plugin.Receivers
{
    public class WebSocketDMXBehaviour : WebSocketBehavior
    {
        private const int ARTNETPACKETLENGTH = 530;
        private const int WSPACKETLENGTH = 513;

        protected override void OnMessage(MessageEventArgs e)
        {
            if (!e.IsBinary) return; // do not care about non-binary ones
            if (e.RawData.Length != ARTNETPACKETLENGTH)
            {
                Debug.LogError($"WebSocketDMXBehaviour: New unhandled message from client {e.RawData.Length}/{ARTNETPACKETLENGTH}");
            }
            else
            {
                WebSocketReceiver.Instance.artNetReceiver.OnReceivedPacket(e.RawData, e.RawData.Length, UserEndPoint);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log($"WebSocketDMXBehaviour {UserEndPoint} closed connection");
        }

        protected override void OnOpen()
        {
            Debug.Log($"WebSocketDMXBehaviour {UserEndPoint} opened connection");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Debug.LogError($"WebSocketDMXBehaviour {UserEndPoint} error {e.Exception}");
        }
    }
}
