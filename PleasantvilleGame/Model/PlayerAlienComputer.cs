using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PleasantvilleGame
{
   public class PlayerAlienComputer : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienComputer() : base(true)
      {

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
               if( false == GetStartingAlien(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): GetStartingAliens() returned false");
                  return false;
               }
               if (false == GetStartingAlien(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): GetStartingAliens() returned false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): unhandled key=" + key);
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
         string startingTownplayer = gi.PlayerTown.StartingTownspeople[0];
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

            if (gi.PlayerAlien.StartingTownspeople[0] == startingAlien)
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
         if (true == String.IsNullOrEmpty(gi.PlayerAlien.StartingTownspeople[0]))
            gi.PlayerAlien.StartingTownspeople[0] = startingAlien;
         else
            gi.PlayerAlien.StartingTownspeople[1] = startingAlien;
         return true;
      }
   }
}
