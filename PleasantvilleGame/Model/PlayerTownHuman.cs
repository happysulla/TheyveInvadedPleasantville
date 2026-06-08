using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerTownHuman: PlayerBase, IPlayerTown
   {
      public PlayerTownHuman() : base(false)
      {

      }
      //===============================================================
      public override bool GetNextState(IGameInstance gi)
      {
         string key = gi.EventActive;
         switch (key)
         {
            case "e003":
               break;
            case "e005":

               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerTownHuman.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      public override bool CreateRandomMoves(IGameInstance gi)
      {
         if (false == gi.CreateRandomMoves())
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerTownHuman.CreateRandomMoves(): failed to create random moves");
            return false;
         }
         return true;
      }
      public override bool PerformRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerTownComputer.PerformRandomMoves(): not implemented");
         return false;
      }
      public override bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerTownComputer.CreateMapItemMove(): not implemented");
         return false;
      }
      //---------------------------------------------------------------
      public bool GetStartingTownsperson(IGameInstance gi, int die1)
      {
         switch (die1)
         {
            case 1: StartingTownspeople[0] = "BankPresident"; break;
            case 2: StartingTownspeople[0] = "Doctor"; break;
            case 3: StartingTownspeople[0] = "Mayor"; break;
            case 4: StartingTownspeople[0] = "Minister"; break;
            case 5: StartingTownspeople[0] = "Teacher"; break;
            case 6: StartingTownspeople[0] = "Sheriff"; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingTownsperson(): reached default dieRoll=" + die1.ToString());
               return false;
         }
         Logger.Log(LogEnum.LE_SHOW_TOWNS_ADD, "Get_StartingTownsperson(): Added name=" + StartingTownspeople[0]);
         return true;
      }
      public bool AlienConfirmedRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerTownComputer.AlienConfirmedRandomMoves(): not implemented");
         return false;
      }
   }
}
