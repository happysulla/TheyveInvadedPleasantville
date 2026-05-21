using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PleasantvilleGame.Networking
{
   public sealed class MultiplayerSessionManager : IDisposable
   {
      private static readonly HashSet<GameAction> theTownAllowedActions = new HashSet<GameAction>
      {
         GameAction.GameSetupStartingTownsplayerSetRoll,
         GameAction.GameSetupShowMap,
         GameAction.GameSetupFinished,
         GameAction.UpdateEventViewerActive,
         GameAction.UpdateEventViewerDisplay,
         GameAction.TownpersonProposesMovement,
         GameAction.TownpersonCompletesMovement,
         GameAction.TownspersonPerformsConversation,
         GameAction.TownspersonCompletesConversations,
         GameAction.TownspersonPerformsInfluencing,
         GameAction.TownspersonCompletesInfluencing,
         GameAction.TownspersonInitiateCombat,
         GameAction.TownspersonPerformCombat,
         GameAction.TownspersonCompletesCombat,
         GameAction.TownspersonIterrogates,
         GameAction.TownspersonCompletesIterogations,
         GameAction.TownspersonRemovesImplant,
         GameAction.TownspersonCompletesRemoval
      };

      private static readonly HashSet<GameAction> theAlienAllowedActions = new HashSet<GameAction>
      {
         GameAction.GameSetupStartingAlienSetRoll,
         GameAction.UpdateEventViewerActive,
         GameAction.UpdateEventViewerDisplay,
         GameAction.AlienMovement,
         GameAction.AlienCompletesMovement,
         GameAction.AlienInitiateCombat,
         GameAction.AlienPerformCombat,
         GameAction.AlienCompletesCombat,
         GameAction.AlienAcksTownspersonMovement,
         GameAction.AlienStopsTownspersonMovement,
         GameAction.AlienModifiesTownspersonMovement,
         GameAction.AlienAcksIterogations,
         GameAction.AlienTakeover,
         GameAction.AlienCompletesTakeovers
      };

      private readonly object mySync = new object();
      private readonly IGameEngine myGameEngine;

      private IHost? myGrpcHost;
      private GrpcChannel? myGrpcChannel;
      private PleasantvilleMultiplayer.PleasantvilleMultiplayerClient? myRemoteClient;
      private IGameInstance? myTrackedGameInstance;
      private SessionDescriptorDataTranferObject? mySession;

      public MultiplayerSessionManager(IGameEngine gameEngine)
      {
         myGameEngine = gameEngine;
      }

      public SessionDescriptorDataTranferObject? CurrentSession
      {
         get
         {
            lock (mySync)
            {
               return mySession;
            }
         }
      }

      public void TrackGameInstance(IGameInstance gameInstance)
      {
         lock (mySync)
         {
            myTrackedGameInstance = gameInstance;
         }
      }

      public HostSessionResultDataTranferObject StartHosting(IGameInstance gameInstance, string sessionName, int listenPort)
      {
         lock (mySync)
         {
            if (myGrpcHost is not null || myRemoteClient is not null)
            {
               return new HostSessionResultDataTranferObject { ErrorMessage = "A multiplayer session is already active." };
            }
         }

         string safeSessionName = string.IsNullOrWhiteSpace(sessionName) ? "Pleasantville Session" : sessionName.Trim();
         SessionDescriptorDataTranferObject session = new SessionDescriptorDataTranferObject
         {
            SessionId = Guid.NewGuid().ToString("N"),
            SessionName = safeSessionName,
            JoinCode = GenerateJoinCode(),
            HostAddress = DiscoverAdvertisedHost(),
            HostPort = listenPort,
            IsHost = true,
            IsConnected = true,
            LocalRole = MultiplayerRole.Alien
         };

         try
         {
            IHost host = Host.CreateDefaultBuilder()
               .ConfigureLogging(logging => logging.ClearProviders())
               .ConfigureWebHostDefaults(webBuilder =>
               {
                  webBuilder.UseKestrel(options =>
                  {
                     options.ListenAnyIP(listenPort, listenOptions =>
                     {
                        listenOptions.Protocols = HttpProtocols.Http2;
                     });
                  });
                  webBuilder.ConfigureServices(services =>
                  {
                     services.AddSingleton(this);
                     services.AddGrpc();
                  });
                  webBuilder.Configure(app =>
                  {
                     app.UseRouting();
                     app.UseEndpoints(endpoints =>
                     {
                        endpoints.MapGrpcService<PleasantvilleMultiplayerService>();
                     });
                  });
               })
               .Build();

            host.StartAsync().GetAwaiter().GetResult();

            lock (mySync)
            {
               myGrpcHost = host;
               myTrackedGameInstance = gameInstance;
               mySession = session;
            }

            return new HostSessionResultDataTranferObject
            {
               IsSuccess = true,
               Session = session,
               State = MultiplayerSnapshotFactory.CreateVisibleState(gameInstance, session.LocalRole)
            };
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "StartHosting(): " + ex);
            return new HostSessionResultDataTranferObject { ErrorMessage = "Unable to start the gRPC host." };
         }
      }

      public JoinSessionResultDataTranferObject JoinSession(string serverAddress, string sessionId, string joinCode)
      {
         lock (mySync)
         {
            if (myGrpcHost is not null || myRemoteClient is not null)
            {
               return new JoinSessionResultDataTranferObject { ErrorMessage = "A multiplayer session is already active." };
            }
         }

         try
         {
            string normalizedAddress = NormalizeAddress(serverAddress);
            GrpcChannel channel = GrpcChannel.ForAddress(normalizedAddress);
            PleasantvilleMultiplayer.PleasantvilleMultiplayerClient client = new PleasantvilleMultiplayer.PleasantvilleMultiplayerClient(channel);
            JoinSessionResponse response = client.JoinSession(new JoinSessionRequest
            {
               SessionId = sessionId.Trim(),
               JoinCode = joinCode.Trim()
            });

            if (!response.Success)
            {
               channel.Dispose();
               return new JoinSessionResultDataTranferObject { ErrorMessage = response.ErrorMessage };
            }

            SessionDescriptorDataTranferObject session = MultiplayerProtoMapper.ToDataTranferObject(response.Session);
            lock (mySync)
            {
               myGrpcChannel = channel;
               myRemoteClient = client;
               mySession = session;
            }

            return new JoinSessionResultDataTranferObject
            {
               IsSuccess = true,
               Session = session,
               State = response.State is null ? null : MultiplayerProtoMapper.ToDataTranferObject(response.State)
            };
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "JoinSession(): " + ex);
            return new JoinSessionResultDataTranferObject { ErrorMessage = "Unable to join the remote gRPC host." };
         }
      }

      public VisibleGameStateDataTranferObject? GetRemoteVisibleState()
      {
         PleasantvilleMultiplayer.PleasantvilleMultiplayerClient? client;
         SessionDescriptorDataTranferObject? session;

         lock (mySync)
         {
            client = myRemoteClient;
            session = mySession;
         }

         if (client is null || session is null)
         {
            return null;
         }

         try
         {
            GetVisibleStateResponse response = client.GetVisibleState(new GetVisibleStateRequest
            {
               SessionId = session.SessionId,
               JoinCode = session.JoinCode,
               PlayerRole = MultiplayerProtoMapper.ToProtoRole(session.LocalRole)
            });

            if (!response.Success || response.State is null)
            {
               return null;
            }

            return MultiplayerProtoMapper.ToDataTranferObject(response.State);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetRemoteVisibleState(): " + ex);
            return null;
         }
      }

      internal CreateSessionResponse HandleCreateSessionRequest()
      {
         SessionDescriptorDataTranferObject? session = CurrentSession;
         if (session is null)
         {
            return new CreateSessionResponse
            {
               Success = false,
               ErrorMessage = "No session is currently being hosted."
            };
         }

         VisibleGameStateDataTranferObject? state = BuildStateForRole(MultiplayerRole.Alien);
         return new CreateSessionResponse
         {
            Success = state is not null,
            ErrorMessage = state is null ? "The hosted game state is unavailable." : string.Empty,
            Session = MultiplayerProtoMapper.ToProto(session),
            State = state is null ? new VisibleGameState() : MultiplayerProtoMapper.ToProto(state)
         };
      }

      internal JoinSessionResponse HandleJoinSessionRequest(string sessionId, string joinCode)
      {
         SessionDescriptorDataTranferObject? session = CurrentSession;
         if (session is null || !string.Equals(session.SessionId, sessionId, StringComparison.Ordinal) || !string.Equals(session.JoinCode, joinCode, StringComparison.Ordinal))
         {
            return new JoinSessionResponse
            {
               Success = false,
               ErrorMessage = "The session could not be found or the join code was invalid."
            };
         }

         VisibleGameStateDataTranferObject? state = BuildStateForRole(MultiplayerRole.Town);
         if (state is null)
         {
            return new JoinSessionResponse
            {
               Success = false,
               ErrorMessage = "The hosted game state is unavailable."
            };
         }

         SessionDescriptorDataTranferObject remoteSession = new SessionDescriptorDataTranferObject
         {
            SessionId = session.SessionId,
            SessionName = session.SessionName,
            JoinCode = session.JoinCode,
            HostAddress = session.HostAddress,
            HostPort = session.HostPort,
            IsHost = false,
            IsConnected = true,
            LocalRole = MultiplayerRole.Town
         };

         lock (mySync)
         {
            if (mySession is not null)
            {
               mySession.IsConnected = true;
            }
         }

         return new JoinSessionResponse
         {
            Success = true,
            Session = MultiplayerProtoMapper.ToProto(remoteSession),
            State = MultiplayerProtoMapper.ToProto(state)
         };
      }

      internal GetVisibleStateResponse HandleGetVisibleStateRequest(string sessionId, string joinCode, PlayerRole playerRole)
      {
         SessionDescriptorDataTranferObject? session = CurrentSession;
         if (session is null || !string.Equals(session.SessionId, sessionId, StringComparison.Ordinal) || !string.Equals(session.JoinCode, joinCode, StringComparison.Ordinal))
         {
            return new GetVisibleStateResponse
            {
               Success = false,
               ErrorMessage = "The session could not be found or the join code was invalid."
            };
         }

         VisibleGameStateDataTranferObject? state = BuildStateForRole(MultiplayerProtoMapper.ToDataTranferObjectRole(playerRole));
         return new GetVisibleStateResponse
         {
            Success = state is not null,
            ErrorMessage = state is null ? "The hosted game state is unavailable." : string.Empty,
            State = state is null ? new VisibleGameState() : MultiplayerProtoMapper.ToProto(state)
         };
      }

      internal SubmitActionResponse HandleSubmitActionRequest(string sessionId, string joinCode, PlayerRole playerRole, MultiplayerAction? actionMessage)
      {
         SessionDescriptorDataTranferObject? session = CurrentSession;
         if (session is null || !string.Equals(session.SessionId, sessionId, StringComparison.Ordinal) || !string.Equals(session.JoinCode, joinCode, StringComparison.Ordinal))
         {
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The session could not be found or the join code was invalid."
            };
         }

         if (actionMessage is null || string.IsNullOrWhiteSpace(actionMessage.ActionName))
         {
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The multiplayer action was missing."
            };
         }

         MultiplayerRole role = MultiplayerProtoMapper.ToDataTranferObjectRole(playerRole);
         if (!Enum.TryParse(actionMessage.ActionName, true, out GameAction parsedAction))
         {
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The requested game action is not recognized."
            };
         }

         if (!IsActionAllowed(role, parsedAction))
         {
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The requested game action is not permitted for this player."
            };
         }

         IGameInstance? trackedGameInstance;
         lock (mySync)
         {
            trackedGameInstance = myTrackedGameInstance;
         }

         if (trackedGameInstance is null)
         {
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The hosted game state is unavailable."
            };
         }

         try
         {
            GameAction nextAction = parsedAction;
            myGameEngine.PerformAction(ref trackedGameInstance, ref nextAction, actionMessage.DieRoll);
            TrackGameInstance(trackedGameInstance);
            VisibleGameStateDataTranferObject? state = BuildStateForRole(role);
            return new SubmitActionResponse
            {
               Accepted = state is not null,
               ErrorMessage = state is null ? "The hosted game state is unavailable." : string.Empty,
               State = state is null ? new VisibleGameState() : MultiplayerProtoMapper.ToProto(state)
            };
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "HandleSubmitActionRequest(): " + ex);
            return new SubmitActionResponse
            {
               Accepted = false,
               ErrorMessage = "The requested game action failed."
            };
         }
      }

      public void Dispose()
      {
         lock (mySync)
         {
            try
            {
               myGrpcChannel?.Dispose();
            }
            catch
            {
            }

            if (myGrpcHost is not null)
            {
               try
               {
                  myGrpcHost.StopAsync().GetAwaiter().GetResult();
               }
               catch
               {
               }

               myGrpcHost.Dispose();
            }

            myGrpcHost = null;
            myGrpcChannel = null;
            myRemoteClient = null;
            mySession = null;
            myTrackedGameInstance = null;
         }
      }

      private VisibleGameStateDataTranferObject? BuildStateForRole(MultiplayerRole role)
      {
         IGameInstance? trackedGameInstance;
         lock (mySync)
         {
            trackedGameInstance = myTrackedGameInstance;
         }

         if (trackedGameInstance is null)
         {
            return null;
         }

         MultiplayerRole effectiveRole = role == MultiplayerRole.Unknown ? MultiplayerRole.Town : role;
         return MultiplayerSnapshotFactory.CreateVisibleState(trackedGameInstance, effectiveRole);
      }

      private static bool IsActionAllowed(MultiplayerRole role, GameAction action)
      {
         return role switch
         {
            MultiplayerRole.Alien => theAlienAllowedActions.Contains(action),
            MultiplayerRole.Town => theTownAllowedActions.Contains(action),
            _ => false
         };
      }

      private static string NormalizeAddress(string serverAddress)
      {
         string trimmed = serverAddress.Trim();
         if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
         {
            trimmed = "http://" + trimmed;
         }

         return trimmed;
      }

      private static string DiscoverAdvertisedHost()
      {
         try
         {
            IPAddress? candidate = Dns.GetHostAddresses(Dns.GetHostName())
               .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address));
            return candidate?.ToString() ?? "127.0.0.1";
         }
         catch
         {
            return "127.0.0.1";
         }
      }

      private static string GenerateJoinCode()
      {
         const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
         Span<byte> buffer = stackalloc byte[6];
         RandomNumberGenerator.Fill(buffer);
         char[] chars = new char[buffer.Length];
         for (int i = 0; i < buffer.Length; i++)
         {
            chars[i] = alphabet[buffer[i] % alphabet.Length];
         }

         return new string(chars);
      }
   }
}
