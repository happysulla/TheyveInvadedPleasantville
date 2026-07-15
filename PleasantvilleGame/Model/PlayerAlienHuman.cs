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
         Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): not implemented");
         return false;
      }
      public bool ShowPossibleTakeover(IGameInstance gi, IStack stack, ref GameAction action)
      {
         Logger.Log(LogEnum.LE_ERROR, "Show_Takeover(): not implemented");
         return false;
      }
      public bool GetAlienTakeoverPair(ITerritory t, IMapItems aliens, IMapItems possibleVictims, out IMapItem? mi1, out IMapItem? mi2)
      {
         mi1 = null;
         mi2 = null;
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienTakeovers(): not implemented");
         return false;
      }
   }
}
