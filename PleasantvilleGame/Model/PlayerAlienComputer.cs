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
      public override bool GetNextState(IGameInstance gi, ref GameAction action)
      {
         string key = gi.EventActive;
         switch(key)
         {
            case "e002":
               gi.EventActive = gi.EventDisplayed = "e003t";
               gi.DieRollAction = GameAction.DieRollActionNone;
               if( false == GetStartingAlienCounters(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): GetStartingAlienCounters() returned false");
                  return false;
               }
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
      public bool GetStartingAlienCounters(IGameInstance gi)
      {
         string startingTownplayer = gi.StartingTownspeople[0];
         if (true == String.IsNullOrEmpty(startingTownplayer))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_StartingAliens():  gi.PlayerTown.StartingTownspeople[0] is empty");
            return false;
         }
         //---------------------------------
         string startingAlien = "";
         int count = 1000;
         while (0 < count--)
         {
            int die1 = Utilities.RandomGenerator.Next(0, 5);
            int die2 = Utilities.RandomGenerator.Next(0, 6);
            startingAlien = TableMgr.GetTownspersonName(die1, die2);
            if ("ERROR" == startingAlien)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): first TableMgr.GetTownspersonName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
               return false;
            }
            if (startingTownplayer == startingAlien)
               continue;

            if (gi.StartingTownspeople[0] == startingAlien)
               continue;
            break;
         }
         if (count < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): never found aliens");
            return false;
         }
         //---------------------------------
         Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Get_StartingAlien(): Added name=" + startingAlien);
         if (true == String.IsNullOrEmpty(gi.StartingTownspeople[0]))
            gi.StartingTownspeople[0] = startingAlien;
         else
            gi.StartingTownspeople[1] = startingAlien;
         return true;
      }
      public bool BlockRandomMoves(IGameInstance gi)
      {
         gi.IsAlienAckedRandomMovement = true; // computer does not need to see random moves
         // Determine if alien wants to block any movement.
         // If early in game, do not want to block and expose
         // If strategy is PROTECT_ZEBULON and exposed, then block
         // If strategy is to DEFEND_ZEUBLON, block no matter what
         // If late in game, and winning on influence, maybe block to keep winning position.
         // If blocking, remove from Random Moves.
         gi.EventDisplayed = gi.EventActive = "e006t";          // Set next state.
         return true;
      }
      public bool PerformMovement(IGameInstance gi)
      {
         // Choose 5 counters to be random.
         // Need to get the aliens to comingle with other uncontrolled townspeople
         // If strategy is FIENT_ZEBULON, move away from ZEBULON
         // If strategy is MAX_TAKEOVER, get aliens to other uncontrolled locations at all costs
         // Move aliens to locations that are away from observations
         // Move aliesn to locations away from controlled townspeople
         // Move aliens furthest away from other controlled townspeople.
         return true;
      }
   }
}
