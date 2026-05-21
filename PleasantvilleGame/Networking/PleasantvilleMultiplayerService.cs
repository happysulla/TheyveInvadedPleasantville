using System.Threading.Tasks;
using Grpc.Core;

namespace PleasantvilleGame.Networking
{
   public sealed class PleasantvilleMultiplayerService : PleasantvilleMultiplayer.PleasantvilleMultiplayerBase
   {
      private readonly MultiplayerSessionManager mySessionManager;
      public PleasantvilleMultiplayerService(MultiplayerSessionManager sessionManager)
      {
         mySessionManager = sessionManager;
      }
      public override Task<CreateSessionResponse> CreateSession(CreateSessionRequest request, ServerCallContext context)
      {
         return Task.FromResult(mySessionManager.HandleCreateSessionRequest());
      }
      public override Task<JoinSessionResponse> JoinSession(JoinSessionRequest request, ServerCallContext context)
      {
         return Task.FromResult(mySessionManager.HandleJoinSessionRequest(request.SessionId, request.JoinCode));
      }
      public override Task<GetVisibleStateResponse> GetVisibleState(GetVisibleStateRequest request, ServerCallContext context)
      {
         return Task.FromResult(mySessionManager.HandleGetVisibleStateRequest(request.SessionId, request.JoinCode, request.PlayerRole));
      }
      public override Task<SubmitActionResponse> SubmitAction(SubmitActionRequest request, ServerCallContext context)
      {
         return Task.FromResult(mySessionManager.HandleSubmitActionRequest(request.SessionId, request.JoinCode, request.PlayerRole, request.Action));
      }
   }
}
