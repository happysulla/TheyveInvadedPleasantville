using System;
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
      public bool PerformMovement(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.PerformMovement(): not implemented");
         return false;
      }
   }
}
