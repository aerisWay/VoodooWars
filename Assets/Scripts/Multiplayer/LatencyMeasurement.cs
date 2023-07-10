using UnityEngine;
using Unity.Netcode;

public class LatencyMeasurement : NetworkBehaviour
{
    //public NetworkPing pingComponent;
    //public NetworkVariable<float> latency = new NetworkVariable<float>(0f);

    //public NetworkObject targetClient; // The client to measure the latency to

    //private void Start()
    //{
    //    pingComponent.OnPingUpdated += OnPingUpdated;
    //}

    //private void OnDestroy()
    //{
    //    pingComponent.OnPingUpdated -= OnPingUpdated;
    //}

    //private void Update()
    //{
    //    if (!IsLocalPlayer) return;

    //    if (Input.GetKeyDown(KeyCode.L))
    //    {
    //        MeasureLatency();
    //    }
    //}

    //private void MeasureLatency()
    //{
    //    if (!targetClient) return;

    //    // Send a ping message to the target client
    //    pingComponent.SendPing(targetClient.NetworkObjectId);
    //}

    //private void OnPingUpdated(float ping)
    //{
    //    latency.Value = ping;
    //    Debug.Log("Latency: " + ping);
    //}
}