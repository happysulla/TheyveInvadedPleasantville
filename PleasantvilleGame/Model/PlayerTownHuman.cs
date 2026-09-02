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
      public override bool GetNextState(IGameInstance gi, ref GameAction action)
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
      //---------------------------------------------------------------
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
         gi.IsTownsAckedRandomMovement = true;
         return true;
      }
      public bool PerformTownMove(IGameInstance gi, ref GameAction outAction)
      {
         if (null == gi.SelectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_TownMove(): gi.SelectedTerritory=null");
            return false;
         }
         foreach (IMapItem mi in gi.SelectedMapItems)
         {
            if (gi.SelectedTerritory.ToString() == mi.TerritoryCurrent.ToString())
               continue;
            if ((false == mi.IsControlled) || (true == mi.IsKnockedout) || (true == mi.IsTiedUp) || (true == mi.IsStunned) || (true == mi.IsKilled))
               continue;
            IMapItemMove? mim = gi.CreateMapItemMove(mi, gi.SelectedTerritory);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_TownMove(): mim=null for mi=" + mi.ToString() + " moving to t=" + gi.SelectedTerritory.ToString());
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "PerformPerform_TownMoveTownMove(): mi=" + mi.ToString() + " moving to t=" + gi.SelectedTerritory.ToString());
            gi.MapItemMoves.Add(mim);
         }
         gi.SelectedMapItems.Clear();
         return true;
      }
   }
}
