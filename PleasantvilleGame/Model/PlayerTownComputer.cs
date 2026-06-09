using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerTownComputer : PlayerBase, IPlayerTown
   {
      public PlayerTownComputer() : base(true)
      {

      }
      //===============================================================
      public override bool GetNextState(IGameInstance gi, ref GameAction action)
      {
         string key = gi.EventActive;
         switch (key)
         {
            case "e003":
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerTownComputer.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      //===============================================================
      public bool GetStartingTownCounter(IGameInstance gi, int die1)
      {
         switch (die1)
         {
            case 1: gi.StartingTownspeople[0] = "BankPresident"; break;
            case 2: gi.StartingTownspeople[0] = "Doctor"; break;
            case 3: gi.StartingTownspeople[0] = "Mayor"; break;
            case 4: gi.StartingTownspeople[0] = "Minister"; break;
            case 5: gi.StartingTownspeople[0] = "Teacher"; break;
            case 6: gi.StartingTownspeople[0] = "Sheriff"; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingTownsperson(): reached default dieRoll=" + die1.ToString());
               return false;
         }
         Logger.Log(LogEnum.LE_SHOW_TOWNS_ADD, "Get_StartingTownsperson(): Added name=" + gi.StartingTownspeople[0]);
         return true;
      }
      public bool BlockRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetStartingAlienCounters(): not implemented");
         return false;
      }
   }
}
