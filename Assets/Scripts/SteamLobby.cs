using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using Steamworks;
using MLAPI.Transports.SteamP2P;
using System;

public class SteamLobby : MonoBehaviour
{
    public int maxConnections = 6;
    private CSteamID steamLobbyID;

    bool onTheServer;

    public Text textPing;

    Callback<LobbyCreated_t> lobbyCreated;
    Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";

    private SteamP2PTransport steamP2PTransport;

    // Start is called before the first frame update
    void Start()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);


        steamP2PTransport = FindObjectOfType<SteamP2PTransport>();

        onTheServer = false;
    }

    public void HostLobby()
    {
        // disable buttons

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);

        onTheServer = true;
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            // enable buttons
            return;
        }

        NetworkManager.Singleton.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        steamLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(steamLobbyID, HostAddressKey);

        steamP2PTransport.ConnectToSteamID = Convert.ToUInt64(hostAddress);
        NetworkManager.Singleton.StartClient();

        onTheServer = true;

        // disable buttons
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            if (GUILayout.Button("Host")) HostLobby();

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Leave"))
            {
                SteamMatchmaking.LeaveLobby(steamLobbyID);
                NetworkManager.Singleton.StopClient();

                onTheServer = false;
            }
        }

        if (NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Stop Host"))
            {
                SteamMatchmaking.LeaveLobby(steamLobbyID);
                NetworkManager.Singleton.StopHost();

                onTheServer = false;
            }
        }

        GUILayout.EndArea();
    }
    private void Update()
    {
        if (onTheServer)
        {
            textPing.text = steamP2PTransport.GetCurrentRtt((ulong)steamLobbyID).ToString();
        }

    }
}
