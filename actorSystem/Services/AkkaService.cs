﻿
using Akka.Actor;
using Akka.DependencyInjection;
using DotNetty.Common.Utilities;

namespace actorSystem.Services;


public class AkkaService : IHostedService, IActorBridge
{
  private ActorSystem? _actorSystem;
  private readonly IConfiguration _configuration;
  private readonly ILogger<AkkaService> logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly IHostApplicationLifetime _applicationLifetime;
  private IActorRef? _clientSupervisor;

  public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration, ILogger<AkkaService> logger)
  {
    _serviceProvider = serviceProvider;
    _applicationLifetime = appLifetime;
    _configuration = configuration;
    this.logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    Console.WriteLine("Connecting to akka service...");
    logger.LogInformation("logger: Connecting to akka service...");
    var bootstrap = BootstrapSetup.Create();

    var diSetup = DependencyResolverSetup.Create(_serviceProvider);

    var actorSystemSetup = bootstrap.And(diSetup);

    _actorSystem = _serviceProvider.GetRequiredService<ActorSystem>();
    _clientSupervisor = _actorSystem.ActorSelection("/user/client-supervisor").ResolveOne(TimeSpan.FromSeconds(3)).Result;

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

  public void RegisterClient(RegisterClientCommand command)
  {
    _clientSupervisor.Tell(command);
  }
}
