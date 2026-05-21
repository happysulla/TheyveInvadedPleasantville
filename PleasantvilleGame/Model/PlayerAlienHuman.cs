using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienHuman : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienHuman() : base(false)
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
   }
}
