using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkSingleton {
  private static NetworkSingleton instance;
  public static NetworkSingleton Instance {
    get {
      if (instance == null)
        instance = new NetworkSingleton();

      return instance;
    }
  }

  private bool networkingInitialized;

  public Lobby lobby { get; private set; }

  public NetworkSingleton() {
    networkingInitialized = false;
    instance = this;
  }

  public async Task InitializeNetworking() {
    if (networkingInitialized)
      return;

    var randomProfileName = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 6).Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    var initializationOptions = new InitializationOptions();
    initializationOptions.SetProfile(randomProfileName);
    await UnityServices.InitializeAsync(initializationOptions);
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    networkingInitialized = true;
  }

  public async Task<List<Lobby>> ListLobbies() {
    await InitializeNetworking();
    return (await Lobbies.Instance.QueryLobbiesAsync()).Results;
  }

  public async Task JoinLobby(Lobby lobby) {
    var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);

    string joinCode = joiningLobby.Data["JoinCode"].Value;
    await StartClient(joinCode);
    this.lobby = joiningLobby;
  }

  public async Task<JoinAllocation> StartClient(string joinCode) {
    JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
    NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
    ConnectClient();
    return allocation;
  }

  private void ConnectClient() {
    if (NetworkManager.Singleton.StartClient()) {

    } else
      Debug.LogWarning("Could not start Client!");
  }

  public event Action LobbyUpdated;

  public async void RefreshLobby() {
    lobby = await Lobbies.Instance.GetLobbyAsync(this.lobby.Id);
    if (LobbyUpdated != null)
      LobbyUpdated();
  }

  public async Task StartLobby() {
    if (lobby != null) {
      return;
    }

    var maxConnections = 6;
    await NetworkSingleton.Instance.InitializeNetworking();

    var allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
    var joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

    NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

    NetworkManager.Singleton.StartHost();

    NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) => RefreshLobby();
    NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => RefreshLobby();

    CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions() {
      IsPrivate = false,
      Data = new Dictionary<string, DataObject>() {
        {
          "JoinCode",
          new DataObject(
            visibility: DataObject.VisibilityOptions.Member,
            value: joinCode
          )
        }
      }
    };

    var randomLobbyName = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 6).Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    Lobby joinedLobby = await Lobbies.Instance.CreateLobbyAsync(randomLobbyName, maxConnections, createLobbyOptions);
    this.lobby = joinedLobby;
  }
}
