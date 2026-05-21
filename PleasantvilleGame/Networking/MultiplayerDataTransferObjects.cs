using System.Collections.Generic;

namespace PleasantvilleGame.Networking
{
   public enum MultiplayerRole
   {
      Unknown = 0,
      Alien = 1,
      Town = 2
   }
   //--------------------------------------------------
   public sealed class SessionDescriptorDataTranferObject
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
   //--------------------------------------------------
   public sealed class VisibleCounterDataTranferObject
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
   //--------------------------------------------------
   public sealed class VisibleGameStateDataTranferObject
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
      public List<VisibleCounterDataTranferObject> Counters { get; } = new List<VisibleCounterDataTranferObject>();
   }
   //--------------------------------------------------
   public sealed class HostSessionResultDataTranferObject
   {
      public bool IsSuccess { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public SessionDescriptorDataTranferObject? Session { get; set; }
      public VisibleGameStateDataTranferObject? State { get; set; }
   }
   //--------------------------------------------------
   public sealed class JoinSessionResultDataTranferObject
   {
      public bool IsSuccess { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public SessionDescriptorDataTranferObject? Session { get; set; }
      public VisibleGameStateDataTranferObject? State { get; set; }
   }
   //--------------------------------------------------
   public sealed class SubmitActionResultDataTranferObject
   {
      public bool IsAccepted { get; set; }
      public string ErrorMessage { get; set; } = string.Empty;
      public VisibleGameStateDataTranferObject? State { get; set; }
   }
}
