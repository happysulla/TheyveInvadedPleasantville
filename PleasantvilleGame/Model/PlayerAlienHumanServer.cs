using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienHumanServer : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienHumanServer() : base(false)
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
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHuman.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      public bool ChooseStartingHqArea()
      {
         return true;
      }
      public bool GetStartingAlien(IGameInstance gi)
      {
         return true;
      }
      //---------------------------------------------------------------
      public override bool CreateRandomMoves(IGameInstance gi)
      {
         return true;
      }
      public override bool PerformRandomMoves(IGameInstance gi)
      {
         return true;
      }
      public override bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         return true;
      }
   }
}
