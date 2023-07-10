using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

public class ConnectionApprovalHandler : MonoBehaviour
{
    public static int maxPlayers = 2;

    private void Start()
    {
        
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        //DontDestroyOnLoad(this.gameObject);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Connection Approval: " + request.ClientNetworkId);
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            response.Approved = false;
        }
        response.Pending = false;
    }


    //public List<uint> AlternatePlayerPrefabs;
    //public static int maxPlayers = 2;

    //public void SetClientPlayerPrefab(int index)
    //{
    //    if (index > AlternatePlayerPrefabs.Count)
    //    {
    //        Debug.LogError($"Trying to assign player Prefab index of {index} when there are only {AlternatePlayerPrefabs.Count} entries!");
    //        return;
    //    }
    //    if (NetworkManager.IsListening || IsSpawned)
    //    {
    //        Debug.LogError("This needs to be set this before connecting!");
    //        return;
    //    }
    //    NetworkManager.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(index);
    //}

    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
    //    }
    //}

    //private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    //{
    //    //var playerPrefabIndex = System.BitConverter.ToInt32(request.Payload);
    //    //if (AlternatePlayerPrefabs.Count < playerPrefabIndex)
    //    //{
    //    //    response.PlayerPrefabHash = AlternatePlayerPrefabs[playerPrefabIndex];
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogError($"Client provided player Prefab index of {playerPrefabIndex} when there are only {AlternatePlayerPrefabs.Count} entries!");
    //    //    return;
    //    //}
    //    Debug.Log("Connection Approval: " + request.ClientNetworkId);
    //    response.Approved = true;
    //    response.PlayerPrefabHash = null;

    //    if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
    //    {
    //        response.Approved = false;
    //    }
    //    response.Pending = false;
    //    response.PlayerPrefabHash = 0;
    //    // Continue filling out the response
}

