namespace PleasantvilleGame.Networking
{
   public static class MultiplayerSnapshotFactory
   {
      public static VisibleGameStateDataTranferObject CreateVisibleState(IGameInstance gameInstance, MultiplayerRole role)
      {
         VisibleGameStateDataTranferObject dto = new VisibleGameStateDataTranferObject
         {
            GameGuid = gameInstance.GameGuid.ToString(),
            EventActive = gameInstance.EventActive,
            EventDisplayed = gameInstance.EventDisplayed,
            PlayerTurn = gameInstance.PlayerTurn,
            NextAction = gameInstance.NextAction,
            GamePhase = gameInstance.GamePhase.ToString(),
            GameTurn = gameInstance.GameTurn,
            Day = gameInstance.Day,
            InfluenceTotal = gameInstance.InfluenceCountTotal,
            InfluenceTownspeople = gameInstance.InfluenceCountTownspeople,
            InfluenceAlienUnknown = role == MultiplayerRole.Alien ? gameInstance.InfluenceCountAlienUnknown : 0,
            InfluenceAlienKnown = gameInstance.InfluenceCountAlienKnown
         };

         foreach (IMapItem mapItem in gameInstance.Townspeople)
         {
            VisibleCounterDataTranferObject counter = new VisibleCounterDataTranferObject
            {
               Name = mapItem.Name,
               TerritoryName = mapItem.TerritoryCurrent.Name,
               TerritorySubname = mapItem.TerritoryCurrent.Subname,
               Movement = mapItem.Movement,
               MovementUsed = mapItem.MovementUsed,
               Influence = mapItem.Influence,
               Combat = mapItem.Combat,
               IsUnconscious = mapItem.IsUnconscious,
               IsStunned = mapItem.IsStunned,
               IsSurrendered = mapItem.IsSurrendered,
               IsTiedUp = mapItem.IsTiedUp,
               IsWary = mapItem.IsWary,
               IsKilled = mapItem.IsKilled,
               IsImplantHeld = mapItem.IsImplantHeld
            };

            if (role == MultiplayerRole.Alien)
            {
               counter.IsControlledByLocalPlayer = mapItem.IsAlienKnown || mapItem.IsAlienUnknown;
               counter.IsControlledByRemotePlayer = mapItem.IsControlled;
               counter.IsAlienControlledVisible = mapItem.IsAlienKnown || mapItem.IsAlienUnknown;
            }
            else
            {
               counter.IsControlledByLocalPlayer = mapItem.IsControlled;
               counter.IsControlledByRemotePlayer = mapItem.IsAlienKnown;
               counter.IsAlienControlledVisible = mapItem.IsAlienKnown;
            }

            dto.Counters.Add(counter);
         }

         return dto;
      }
   }
}
