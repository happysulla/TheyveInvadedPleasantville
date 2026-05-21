using System;
using System.Text.RegularExpressions;

namespace PleasantvilleGame.Networking
{
   public static class MultiplayerStateApplier
   {
      public static bool ApplyVisibleState(IGameInstance gameInstance, VisibleGameStateDataTranferObject state, MultiplayerRole localRole)
      {
         if (gameInstance is null)
         {
            return false;
         }
         gameInstance.Townspeople.Clear();
         gameInstance.Stacks.Clear();
         gameInstance.PersonsKnockedOut.Clear();
         gameInstance.PersonsStunned.Clear();
         gameInstance.SelectedMapItem = null;
         gameInstance.SelectedStack = null;
         gameInstance.MapItemCombat = null;
         gameInstance.Takeover = null;
         gameInstance.PreviousMapItemMove = null;
         gameInstance.MapItemMoves.Clear();
         gameInstance.ZebulonTerritories.Clear();
         if (Guid.TryParse(state.GameGuid, out Guid parsedGuid))
         {
            gameInstance.GameGuid = parsedGuid;
         }
         gameInstance.EventActive = state.EventActive;
         gameInstance.EventDisplayed = state.EventDisplayed;
         gameInstance.PlayerTurn = state.PlayerTurn;
         gameInstance.NextAction = state.NextAction;
         gameInstance.GameTurn = state.GameTurn;
         gameInstance.Day = state.Day;
         gameInstance.InfluenceCountTotal = state.InfluenceTotal;
         gameInstance.InfluenceCountTownspeople = state.InfluenceTownspeople;
         gameInstance.InfluenceCountAlienUnknown = state.InfluenceAlienUnknown;
         gameInstance.InfluenceCountAlienKnown = state.InfluenceAlienKnown;
         gameInstance.DieRollAction = GameAction.DieRollActionNone;
         if (Enum.TryParse(state.GamePhase, true, out GamePhase parsedPhase))
         {
            gameInstance.GamePhase = parsedPhase;
         }
         foreach (VisibleCounterDataTranferObject counter in state.Counters)
         {
            string baseName = GetBaseCounterName(counter.Name);
            ITerritory? territory = Territories.theTerritories.Find(counter.TerritoryName, counter.TerritorySubname);
            if (territory is null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ApplyVisibleState(): missing territory for " + counter.Name + " at " + counter.TerritoryName + ":" + counter.TerritorySubname);
               continue;
            }
            MapItem mapItem = new MapItem(counter.Name, 0.8, baseName, territory, counter.Movement, counter.Influence, counter.Combat)
            {
               MovementUsed = counter.MovementUsed,
               IsControlled = counter.IsControlledByLocalPlayer,
               IsAlienKnown = counter.IsAlienControlledVisible && counter.IsControlledByRemotePlayer,
               IsAlienUnknown = localRole == MultiplayerRole.Alien && counter.IsAlienControlledVisible && !counter.IsControlledByLocalPlayer ? true : false,
               IsImplantHeld = counter.IsImplantHeld,
               IsUnconscious = counter.IsUnconscious,
               IsStunned = counter.IsStunned,
               IsSurrendered = counter.IsSurrendered,
               IsTiedUp = counter.IsTiedUp,
               IsWary = counter.IsWary,
               IsKilled = counter.IsKilled
            };
            gameInstance.Townspeople.Add(mapItem);
            gameInstance.Stacks.Add(mapItem);
            if (counter.IsUnconscious)
            {
               gameInstance.PersonsKnockedOut.Add(mapItem);
            }
            if (counter.IsStunned)
            {
               gameInstance.PersonsStunned.Add(mapItem);
            }
         }
         return true;
      }
      //----------------------------------------------------------------------------------------
      private static string GetBaseCounterName(string counterName)
      {
         if (string.IsNullOrWhiteSpace(counterName))
         {
            return string.Empty;
         }
         return Regex.Replace(counterName, "\\d+$", string.Empty);
      }
   }
}
