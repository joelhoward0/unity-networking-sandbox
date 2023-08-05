using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public static class StringExtensions {
  public static string Join(this IEnumerable<string> strings) {
    return string.Join(", ", strings);
  }
}

public class GameScript : NetworkBehaviour {
  public TMPro.TMP_Text status;
  public TMPro.TMP_Text startLobby;
  public TMPro.TMP_Text joinLobby;
  public NetworkVariable<GameState> state = new NetworkVariable<GameState>(new GameState());

  void Start() {
    status.text = "Loading...";
  }

  public async void StartLobby() {
    startLobby.gameObject.SetActive(false);
    joinLobby.gameObject.SetActive(false);

    status.text = "Starting lobby...";
    await NetworkSingleton.Instance.StartLobby();
    var lobby = NetworkSingleton.Instance.lobby;
    status.text = $"Hosting Lobby: {lobby.Name}, {GetGameState()}";

    for (int i = 0; i < 100; i++) {
      state.Value.count++;
      //state.Value.strings.Add(i.ToString());
      state.Value.stringContainers.Add(new StringContainer($"Player {i}"));
      state.SetDirty(true);
      status.text = $"Hosting Lobby: {lobby.Name}, {GetGameState()}";
      await Task.Delay(500);
    }
  }

  public async void JoinLobby() {
    startLobby.gameObject.SetActive(false);
    joinLobby.gameObject.SetActive(false);

    status.text = "Finding lobby...";
    var lobbies = await NetworkSingleton.Instance.ListLobbies();

    status.text = $"Found {lobbies.Count}. Joining lobby {lobbies[0].Name}";

    await NetworkSingleton.Instance.JoinLobby(lobbies[0]);
    var lobby = NetworkSingleton.Instance.lobby;
    status.text = $"Joined Lobby: {lobby.Name}, {GetGameState()}";

    state.OnValueChanged += (previousState, newState) => {
      status.text = $"Joined Lobby: {lobby.Name}, {GetGameState()}";
    };
  }

  public string GetGameState() {
    return $"count: {state.Value.count}, strings: {state.Value.stringContainers.Select(c => c.value.ToString()).Join()}";
  }
}
