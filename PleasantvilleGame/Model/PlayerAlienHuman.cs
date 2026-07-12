using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;

namespace PleasantvilleGame
{
   public class PlayerAlienHuman : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation { set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienHuman() : base(true)
      {
      }
      //===============================================================
      public override bool GetNextState(IGameInstance gi, ref GameAction action)
      {
         string key = gi.EventActive;
         switch (key)
         {
            case "e003":
               gi.EventActive = gi.EventDisplayed = "e003a";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.ChooseStartingHqArea(): not implemented");
         return false;
      }
      public bool GetStartingAlien(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetStartingAlien(): not implemented");
         return false;
      }
      public bool GetStartingAlienCounters(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetStartingAlienCounters(): not implemented");
         return false;
      }
      public bool BlockRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetStartingAlienCounters(): not implemented");
         return false;
      }
      public bool PerformAlienMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienMoves(): not implemented");
         return false;
      }
      public bool PerformAlienTakeover(IGameInstance gi, IMapItems aliens, IMapItems victims, ref GameAction action)
      {
         if (0 == aliens.Count || 0 == victims.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienTakeovers(): aliens=" + aliens.Count.ToString() + " victims=" + victims.Count.ToString());
            return false;
         }
         IMapItem? firstAlien = aliens[0];
         if( null == firstAlien )
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienTakeovers(): firstAlien=null");
            return false;
         }
         ITerritory t = firstAlien.TerritoryCurrent;
         gi.SelectedTerritories.Add(t);
         action = GameAction.AlienTakeoversSelect;
         Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "CheckFor_AlienTakeovers(): adding t=" + t.ToString());
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienTakeovers(): not implemented");
         return false;
      }
   }
}
