using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienComputer : Player, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienComputer() : base(true)
      {

      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         return true;
      }
      public bool GetStartingTownspeople(IGameInstance gi, int die1, int die2)
      {
         string startingTownplayer = gi.PlayerTown.StartingTownspeople[0];
         int count = 1000;
         while (0 < count--)
         {
            StartingTownspeople[0] = TableMgr.GetTownspersonName(die1, die2);
            if ("ERROR" == StartingTownspeople[0])
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): first TableMgr.GetTownspersonName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
               return false;
            }
            if (startingTownplayer == StartingTownspeople[0])
               continue;
            Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Get_StartingAlien(): Added name=" + StartingTownspeople[1]);
            //----------------------------------------------
            die1 = Utilities.RandomGenerator.Next(0, 5);
            if (6 == die1) // first die cannot be a 6
               continue;
            die2 = Utilities.RandomGenerator.Next(0, 6);
            StartingTownspeople[1] = TableMgr.GetTownspersonName(die1, die2);
            if ("ERROR" == StartingTownspeople[1])
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): 2nd TableMgr.GetTownspersonName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
               return false;
            }
            if (startingTownplayer == StartingTownspeople[1])
               continue;
            Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Get_StartingAlien(): Added name=" + StartingTownspeople[1]);
            break;
         }
         if (count < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): never found aliens");
            return false;
         }
         return true;
      }
   }
}
