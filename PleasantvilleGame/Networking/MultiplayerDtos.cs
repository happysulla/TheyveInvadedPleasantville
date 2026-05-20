using System.Collections.Generic;

namespace PleasantvilleGame.Networking
{
   public enum MultiplayerRole
   {
      Unknown = 0,
      Alien = 1,
      Town = 2
   }

   public sealed class SessionDescriptorDto
   {
      public string SessionId { get; set; } = string.Empty;
      public string SessionName { get; set; } = string.Empty;
      public string JoinCode { get; set; } = string.Empty;
      public string HostAddress { get; set; } = string.Empty;
      public int HostPort { get; set; }
      public bool IsHost { get; set; }
      public bool IsConnected { get; set; }
      public MultiplayerRole LocalRole { get; set; } = MultiplayerRole.Unknown;
   }

   public sealed class VisibleCounterDto
   {
      public string Name { get; set; } = string.Empty;
      public string TerritoryName { get; set; } = string.Empty;
      public string TerritorySubname { get; set; } = string.Empty;
      public int Movement { get; set; }
      public int MovementUsed { get; set; }
      public int Influence { get; set; }
      public int Combat { get; set; }
      public bool IsControlledByLocalPlayer { get; set; }
      public bool IsControlledByRemotePlayer { get; set; }
      public bool IsAlienControlledVisible { get; set; }
      public bool IsUnconscious { get; set; }
      public bool IsStunned { get; set; }
      public bool IsSurrendered { get; set; }
      public bool IsTiedUp { get; set; }
      public bool IsWary { get; set; }
      public bool IsKilled { get; set; }
      public bool IsImplantHeld { get; set; }
   }

   public sealed class VisibleGameStateDto
   {
      public string GameGuid { get; set; } = string.Empty;
      public string EventActive { get; set; } = string.Empty;
      public string EventDisplayed { get; set; } = string.Empty;
      public string PlayerTurn { get; set; } = string.Empty;
      public string NextAction { get; set; } = string.Empty;
      public string GamePhase { get; set; } = string.Empty;
      public int GameTurn { get; set; }
      public int Day { get; set; }
      public int InfluenceTotal { get; set; }
      public int InfluenceTownspeople { get; set; }
      public int InfluenceAlienUnknown { get; set; }
      public int InfluenceAlienKnown { get; set; }
      public List<VisibleCounterDto> Counters { get; } = new List<VisibleCounterDto>();
   }

   public sealed class HostSessionResultDto
   {
      public bool IsSuccess { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public SessionDescriptorDto? Session { get; set; }
      public VisibleGameStateDto? State { get; set; }
   }

   public sealed class JoinSessionResultDto
   {
      public bool IsSuccess { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public SessionDescriptorDto? Session { get; set; }
      public VisibleGameStateDto? State { get; set; }
   }

   public sealed class SubmitActionResultDto
   {
      public bool IsAccepted { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public VisibleGameStateDto? State { get; set; }
   }
}
