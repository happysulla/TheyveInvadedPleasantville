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
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      public override bool CreateRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.CreateRandomMoves(): not implemented");
         return false;
      }
      public override bool PerformRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.PerformRandomMoves(): not implemented");
         return false;
      }
      public override bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.CreateMapItemMove(): not implemented");
         return false;
      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.ChooseStartingHqArea(): not implemented");
         return false;
      }
      public bool GetStartingAlien(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.GetStartingAlien(): not implemented");
         return false;
      }
      public bool TownConfirmedRandomMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienHumanServer.TownConfirmedRandomMoves(): not implemented");
         return false;
      }
   }
}
