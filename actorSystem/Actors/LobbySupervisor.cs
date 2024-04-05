﻿using Akka.Actor;
using Akka.Event;
using shared.Models;

namespace actorSystem;

public record CreateLobbyCommand(string Username);
public record LobbyCreated(LobbyInfo Info, string ActorPath);
public record GetLobbiesQuery();


public class LobbySupervisor : ReceiveActor
{
  public Dictionary<Guid, IActorRef> Lobbies { get; set; } = new Dictionary<Guid, IActorRef>();

  public LobbySupervisor()
  {
    Receive<CreateLobbyCommand>(CreateLobby);
    Receive<JoinLobbyCommand>(JoinLobby);
    ReceiveAsync<GetLobbiesQuery>(async _ => await GetLobbies());
    Receive<StartGameCommand>(StartGame);
    Receive<Guid>(GetLobby);
  }

  private void GetLobby(Guid lobbyId)
  {
    if (Lobbies.TryGetValue(lobbyId, out var lobby))
    {
      lobby.Forward(new GetLobbyInfoQuery());
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Lobby {lobbyId} not found.")));
    }
  }

  private async Task GetLobbies()
  {
    var lobbiesTasks = Lobbies.Values.Select(lobby => lobby.Ask<LobbyInfo>(new GetLobbyInfoQuery()));
    if (!lobbiesTasks.Any())
    {
      Sender.Tell(new LobbyList());
      return;
    }

    var lobbies = (await Task.WhenAll(lobbiesTasks)).ToList();
    var lobbyList = new LobbyList();
    lobbyList.AddRange(lobbies);
    Sender.Tell(lobbyList);
  }

  private void JoinLobby(JoinLobbyCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Forward(command);
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Lobby {command.LobbyId} not found.")));
    }
  }

  private void CreateLobby(CreateLobbyCommand command)
  {
    var lobbyInfo = new LobbyInfo(command.Username);
    var lobbyActor = Context.ActorOf(LobbyActor.Props(lobbyInfo), $"lobby_{command.Username}");
    Lobbies.Add(lobbyInfo.Id, lobbyActor);
    Sender.Tell(new LobbyCreated(lobbyInfo, lobbyActor.Path.ToString()));
    Log.Info($"Lobby created: {lobbyActor.Path}");
  }

  private void StartGame(StartGameCommand command)
  {
    if (Lobbies.TryGetValue(command.LobbyId, out var lobby))
    {
      lobby.Forward(command);
    }
    else
    {
      Sender.Tell(new Status.Failure(new KeyNotFoundException($"Unable to start game. Lobby {command.LobbyId} not found.")));
    }
  }

  protected ILoggingAdapter Log { get; } = Context.GetLogger();

  public static Props Props()
  {
    return Akka.Actor.Props.Create<LobbySupervisor>();
  }
}
