namespace PleasantvilleGame.Networking
{
   public static class MultiplayerProtoMapper
   {
      public static PlayerRole ToProtoRole(MultiplayerRole role)
      {
         return role switch
         {
            MultiplayerRole.Alien => PlayerRole.Alien,
            MultiplayerRole.Town => PlayerRole.Town,
            _ => PlayerRole.Unspecified
         };
      }

      public static MultiplayerRole ToDtoRole(PlayerRole role)
      {
         return role switch
         {
            PlayerRole.Alien => MultiplayerRole.Alien,
            PlayerRole.Town => MultiplayerRole.Town,
            _ => MultiplayerRole.Unknown
         };
      }

      public static SessionDescriptor ToProto(SessionDescriptorDto dto)
      {
         return new SessionDescriptor
         {
            SessionId = dto.SessionId,
            SessionName = dto.SessionName,
            JoinCode = dto.JoinCode,
            HostAddress = dto.HostAddress,
            HostPort = dto.HostPort,
            IsHost = dto.IsHost,
            IsConnected = dto.IsConnected,
            LocalRole = ToProtoRole(dto.LocalRole)
         };
      }

      public static SessionDescriptorDto ToDto(SessionDescriptor proto)
      {
         return new SessionDescriptorDto
         {
            SessionId = proto.SessionId,
            SessionName = proto.SessionName,
            JoinCode = proto.JoinCode,
            HostAddress = proto.HostAddress,
            HostPort = proto.HostPort,
            IsHost = proto.IsHost,
            IsConnected = proto.IsConnected,
            LocalRole = ToDtoRole(proto.LocalRole)
         };
      }

      public static VisibleGameState ToProto(VisibleGameStateDto dto)
      {
         VisibleGameState state = new VisibleGameState
         {
            GameGuid = dto.GameGuid,
            EventActive = dto.EventActive,
            EventDisplayed = dto.EventDisplayed,
            PlayerTurn = dto.PlayerTurn,
            NextAction = dto.NextAction,
            GamePhase = dto.GamePhase,
            GameTurn = dto.GameTurn,
            Day = dto.Day,
            InfluenceTotal = dto.InfluenceTotal,
            InfluenceTownspeople = dto.InfluenceTownspeople,
            InfluenceAlienUnknown = dto.InfluenceAlienUnknown,
            InfluenceAlienKnown = dto.InfluenceAlienKnown
         };

         foreach (VisibleCounterDto counter in dto.Counters)
         {
            state.Counters.Add(new VisibleCounter
            {
               Name = counter.Name,
               TerritoryName = counter.TerritoryName,
               TerritorySubname = counter.TerritorySubname,
               Movement = counter.Movement,
               MovementUsed = counter.MovementUsed,
               Influence = counter.Influence,
               Combat = counter.Combat,
               IsControlledByLocalPlayer = counter.IsControlledByLocalPlayer,
               IsControlledByRemotePlayer = counter.IsControlledByRemotePlayer,
               IsAlienControlledVisible = counter.IsAlienControlledVisible,
               IsUnconscious = counter.IsUnconscious,
               IsStunned = counter.IsStunned,
               IsSurrendered = counter.IsSurrendered,
               IsTiedUp = counter.IsTiedUp,
               IsWary = counter.IsWary,
               IsKilled = counter.IsKilled,
               IsImplantHeld = counter.IsImplantHeld
            });
         }

         return state;
      }

      public static VisibleGameStateDto ToDto(VisibleGameState proto)
      {
         VisibleGameStateDto dto = new VisibleGameStateDto
         {
            GameGuid = proto.GameGuid,
            EventActive = proto.EventActive,
            EventDisplayed = proto.EventDisplayed,
            PlayerTurn = proto.PlayerTurn,
            NextAction = proto.NextAction,
            GamePhase = proto.GamePhase,
            GameTurn = proto.GameTurn,
            Day = proto.Day,
            InfluenceTotal = proto.InfluenceTotal,
            InfluenceTownspeople = proto.InfluenceTownspeople,
            InfluenceAlienUnknown = proto.InfluenceAlienUnknown,
            InfluenceAlienKnown = proto.InfluenceAlienKnown
         };

         foreach (VisibleCounter counter in proto.Counters)
         {
            dto.Counters.Add(new VisibleCounterDto
            {
               Name = counter.Name,
               TerritoryName = counter.TerritoryName,
               TerritorySubname = counter.TerritorySubname,
               Movement = counter.Movement,
               MovementUsed = counter.MovementUsed,
               Influence = counter.Influence,
               Combat = counter.Combat,
               IsControlledByLocalPlayer = counter.IsControlledByLocalPlayer,
               IsControlledByRemotePlayer = counter.IsControlledByRemotePlayer,
               IsAlienControlledVisible = counter.IsAlienControlledVisible,
               IsUnconscious = counter.IsUnconscious,
               IsStunned = counter.IsStunned,
               IsSurrendered = counter.IsSurrendered,
               IsTiedUp = counter.IsTiedUp,
               IsWary = counter.IsWary,
               IsKilled = counter.IsKilled,
               IsImplantHeld = counter.IsImplantHeld
            });
         }

         return dto;
      }
   }
}
