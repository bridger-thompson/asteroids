﻿
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.DependencyInjection;
using DotNetty.Common.Utilities;
using shared.Models;

namespace actorSystem.Services;


public class AkkaService : IHostedService, IActorBridge
{
  private ActorSystem? _actorSystem;
  private readonly IConfiguration _configuration;
  private readonly ILogger<AkkaService> logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly IHostApplicationLifetime _applicationLifetime;
  private IActorRef? _lobbySupervisor;

  public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration, ILogger<AkkaService> logger)
  {
    _serviceProvider = serviceProvider;
    _applicationLifetime = appLifetime;
    _configuration = configuration;
    this.logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var diSetup = DependencyResolverSetup.Create(_serviceProvider);

    var configVar = Environment.GetEnvironmentVariable("CLUSTER_CONFIG") ?? "";
    var config = ConfigurationFactory.ParseString(configVar);

    var bootstrap = BootstrapSetup.Create().WithConfig(config);

    var actorSystemSetup = bootstrap.And(diSetup);

    _actorSystem = ActorSystem.Create("asteroid-system", actorSystemSetup);

    var cluster = Akka.Cluster.Cluster.Get(_actorSystem);

    Console.WriteLine("Here -------------------------------------------------------------------------------");

    var lobbySupervisorProps = DependencyResolver.For(_actorSystem).Props<LobbySupervisor>();
    var singletonProps = ClusterSingletonManager.Props(
      singletonProps: lobbySupervisorProps,
      settings: ClusterSingletonManagerSettings.Create(_actorSystem).WithRole("lobby")
    );

    Console.WriteLine("Here -------------------------------------------------------------------------------");

    var x = _actorSystem.ActorOf(singletonProps, "lobbies-singleton-manager");

    Console.WriteLine(x.Path.ToString());

    // var proxyProps = ClusterSingletonProxy.Props(
    //   singletonManagerPath: "/user/lobbies-singleton-manager",
    //   settings: ClusterSingletonProxySettings.Create(_actorSystem)
    // );

    // _lobbySupervisor = _actorSystem.ActorOf(proxyProps, "lobby-supervisor-proxy");
    // Console.WriteLine($"Lobby supervisor proxy created {_lobbySupervisor.Path.ToString()}");

#pragma warning disable CS4014
    _actorSystem.WhenTerminated.ContinueWith(_ =>
    {
      _applicationLifetime.StopApplication();
    });
#pragma warning restore CS4014 
    await Task.CompletedTask;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
  }

  public void Tell(object message)
  {
    throw new NotImplementedException();
  }

  public Task<T> Ask<T>(object message)
  {
    throw new NotImplementedException();
  }

  public async Task<Guid> CreateLobby(string username)
  {
    var result = await _lobbySupervisor.Ask<LobbyCreated>(new CreateLobbyCommand(username));
    return result.Info.Id;
  }

  public void JoinLobby(string username, Guid lobbyId)
  {
    _lobbySupervisor?.Tell(new JoinLobbyCommand(username, lobbyId));
  }

  public async Task<LobbyList> GetLobbies()
  {
    var result = await _lobbySupervisor.Ask<LobbyList>(new GetLobbiesQuery());
    return result ?? new LobbyList();
  }

  public void StartGame(StartGameCommand command)
  {
    _lobbySupervisor.Tell(command);
  }

  public async Task<LobbyInfo> GetLobbyInfo(Guid lobbyId)
  {
    var result = await _lobbySupervisor.Ask<LobbyInfo>(lobbyId);
    return result;
  }

  public void UpdatePlayerInputState(string username, Guid lobbyId, InputState inputState)
  {
    _lobbySupervisor.Tell(new UpdatePlayerInputStateCommand(username, lobbyId, inputState));
  }
}
