using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienHumanClient : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienHumanClient() : base(false)
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
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      //---------------------------------------------------------------
      public override bool CreateRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.CreateRandomMoves(): not implemented");
         return false;
      }
      public override bool PerformRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.PerformRandomMoves(): not implemented");
         return false;
      }
      public override bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.CreateMapItemMove(): not implemented");
         return false;
      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.ChooseStartingHqArea(): not implemented");
         return false;
      }
      public bool GetStartingAlien(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.GetStartingAlien(): not implemented");
         return false;
      }
      public bool TownConfirmedRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanClient.TownConfirmedRandomMoves(): not implemented");
         return false;
      }
   }
}
