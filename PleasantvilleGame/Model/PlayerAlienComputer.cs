using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PleasantvilleGame
{
   internal enum AlienStrategyEnum
   {
      DEFEND_ZEBULON,
      SURROUND_ZEBULON,
      FIENT_ZEBULON,
      KEEP_HIDDEN,
      ATTACK_TOWNSPEOPLE,
      MAX_TAKEOVER
   }
   internal class Behavior
   {
      public AlienStrategyEnum StrategyPrimary { set; get; } = AlienStrategyEnum.KEEP_HIDDEN;
      public AlienStrategyEnum StrategySecondary { set; get; } = AlienStrategyEnum.MAX_TAKEOVER;
      public int Risky { set; get; }
      public int Stealthy { set; get; }
   }
   //===============================================================
   public class PlayerAlienComputer : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation { set; get; } = new Territory();
      private Behavior myBehavior = new Behavior();
      //---------------------------------------------------------------
      public PlayerAlienComputer() : base(true)
      {
         myBehavior.StrategyPrimary = AlienStrategyEnum.KEEP_HIDDEN;
      }
      //===============================================================
      public override bool GetNextState(IGameInstance gi)
      {
         string key = gi.EventActive;
         switch(key)
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
      public bool TownConfirmedRandomMoves(IGameInstance gi)
      {
         // Determine if alien wants to block any movement.
         // If early in game, do not want to block and expose
         // If strategy is PROTECT_ZEBULON and exposed, then block
         // If strategy is to DEFEND_ZEUBLON, block no matter what
         // If late in game, and winning on influence, maybe block to keep winning position.

         // If blocking, remove from Random Moves.
         gi.EventDisplayed = gi.EventActive = "e006t";          // Set next state.
         return true;
      }

   }
}
