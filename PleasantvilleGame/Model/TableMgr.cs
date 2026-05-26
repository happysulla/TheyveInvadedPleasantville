using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   internal class TableMgr
   {
      static public CombatResult[,] theTable = new CombatResult[12, 5];
      public const int FN_ERROR = -1000;
      public readonly static string[,] theTownpersonsTable = new string[5, 6]
      {
         {"Mayor","Sheriff","Plumber","Lawyer","HotelOwner","Judge"},
         {"RepairShopOwner","MaitreD","Doctor","Teacher","Minister","BankPresident"},
         {"Vet","BarAndGrillOwner","Teller","StationAttendant","CheckoutGirl","Paperboy"},
         {"Maid","BankGuard","TownDrunk","Tailor","BarTender","WarVeteran"},
         {"Waitress","SuperMarketManager","FireChief","Wife","Welder", "Deputy"},
      };
      //---------------------------------------------------------------------
      public readonly static string[,] theTargetBuildingTable = new string[5, 6]
      {
         {"Tavern","Vet Office","Clothing Store","General Store","Gas Pumps","Supermarket"},
         {"School","Bank","Doc Office","VFW","Bar And Grill","Machine Shop"},
         {"Sheriff Fire Dept","Town Hall","Hotel And Restaurant","Church","Graveyard","Stock Pen"},
         {"Train Station","House_A","House_1","House_2","House_3","House_4"},
         {"House_5","House_6","House_7","House_8","Lawyers Office","House_K"},
      };
      //---------------------------------------------------------------------
      public readonly static string[] theTownPlayerStartingTable = new string[6]
      {
          "BankPresident",
          "Doctor",
          "Mayor",
          "Minister",
          "Teacher",
          "Sheriff"
      };
      //---------------------------------------------------------------------
      public readonly static string[,] theBuildingSizes = new string[21, 2]
      {
         {"Tavern","3"},
         {"Vet Office","2"},
         {"Clothing Store","2"},
         {"General Store","4"},
         {"Gas Pumps","1"},
         {"Supermarket","5"},
         {"School","4"},
         {"Bank","4"},
         {"Doc Office","2"},
         {"VFW","1"},
         {"Bar And Grill","2"},
         {"Machine Shop","4"},
         {"Sheriff Fire Dept","4"},
         {"Town Hall","3"},
         {"Hotel And Restaurant","5"},
         {"Church","5"},
         {"Graveyard","1"},
         {"Stock Pen","1"},
         {"Train Station","2"},
         {"House","10"},
         {"Lawyers Office","1"}
      };
      //=====================================================================
      public TableMgr()
      {
         CreateCombatTable();
      }
      private void CreateCombatTable()
      {
         theTable[0, 0] = CombatResult.DefenderWins;
         theTable[1, 0] = CombatResult.DefenderWins;
         theTable[2, 0] = CombatResult.DefenderWins;
         theTable[3, 0] = CombatResult.DefenderWins;
         theTable[4, 0] = CombatResult.DefenderFlees;
         theTable[5, 0] = CombatResult.AttackerFlees;
         theTable[6, 0] = CombatResult.DefenderFlees;
         theTable[7, 0] = CombatResult.AttackerWins;
         theTable[8, 0] = CombatResult.AttackerWins;
         theTable[9, 0] = CombatResult.AttackerWins;
         theTable[10, 0] = CombatResult.AttackerWins;

         theTable[0, 1] = CombatResult.AttackerFlees;
         theTable[1, 1] = CombatResult.AttackerWins;
         theTable[2, 1] = CombatResult.DefenderWins;
         theTable[3, 1] = CombatResult.DefenderWins;
         theTable[4, 1] = CombatResult.AttackerFlees;
         theTable[5, 1] = CombatResult.AttackerWins;
         theTable[6, 1] = CombatResult.DefenderFlees;
         theTable[7, 1] = CombatResult.AttackerWins;
         theTable[8, 1] = CombatResult.DefenderWins;
         theTable[9, 1] = CombatResult.AttackerWins;
         theTable[10, 1] = CombatResult.DefenderFlees;

         theTable[0, 2] = CombatResult.DefenderFlees;
         theTable[1, 2] = CombatResult.DefenderWins;
         theTable[2, 2] = CombatResult.AttackerFlees;
         theTable[3, 2] = CombatResult.AttackerWins;
         theTable[4, 2] = CombatResult.AttackerWins;
         theTable[5, 2] = CombatResult.AttackerWins;
         theTable[6, 2] = CombatResult.DefenderWins;
         theTable[7, 2] = CombatResult.DefenderFlees;
         theTable[8, 2] = CombatResult.AttackerWins;
         theTable[9, 2] = CombatResult.DefenderWins;
         theTable[10, 2] = CombatResult.AttackerWins;

         theTable[0, 3] = CombatResult.DefenderWins;
         theTable[1, 3] = CombatResult.DefenderWins;
         theTable[2, 3] = CombatResult.AttackerWins;
         theTable[3, 3] = CombatResult.DefenderFlees;
         theTable[4, 3] = CombatResult.AttackerWins;
         theTable[5, 3] = CombatResult.AttackerWins;
         theTable[6, 3] = CombatResult.AttackerWins;
         theTable[7, 3] = CombatResult.AttackerWins;
         theTable[8, 3] = CombatResult.DefenderWins;
         theTable[9, 3] = CombatResult.AttackerFlees;
         theTable[10, 3] = CombatResult.AttackerWins;

         theTable[0, 4] = CombatResult.AttackerWins;
         theTable[1, 4] = CombatResult.AttackerFlees;
         theTable[2, 4] = CombatResult.AttackerWins;
         theTable[3, 4] = CombatResult.AttackerWins;
         theTable[4, 4] = CombatResult.AttackerWins;
         theTable[5, 4] = CombatResult.AttackerWins;
         theTable[6, 4] = CombatResult.AttackerWins;
         theTable[7, 4] = CombatResult.DefenderFlees;
         theTable[8, 4] = CombatResult.AttackerWins;
         theTable[9, 4] = CombatResult.DefenderWins;
         theTable[10, 4] = CombatResult.DefenderWins;
      }
      static public string GetTownspersonName(int die1, int die2)
      {
         if (die1 < 0 || 5 < die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTownspersonName(): die1 out of range: " + die1);
            return "ERROR";
         }
         if (die2 < 0 || 6 < die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTownspersonName(): die2 out of range: " + die2);
            return "ERROR";
         }
         return theTownpersonsTable[die1, die2];
      }
      //---------------------------------------------------------------------
      static public string GetTargetBuildingName(int die1, int die2)
      {
         if (die1 < 0 || 5 < die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTargetBuildingName(): die1 out of range: " + die1);
            return "ERROR";
         }
         if (die2 < 0 || 6 < die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTargetBuildingName(): die2 out of range: " + die2);
            return "ERROR";
         }
         string buildingName = Utilities.RemoveSpaces(theTargetBuildingTable[die1, die2]);
         if (true ==  buildingName.Contains("House") )
         {
            return buildingName;
         }
         else
         {
            int arraySize = theBuildingSizes.GetLength(0);
            for (int i = 0; i < arraySize; i++)
            {
               string matchingName = Utilities.RemoveSpaces(theBuildingSizes[i, 0]);
               if (matchingName == buildingName)
               {
                  int maxNum = Convert.ToInt32(theBuildingSizes[i, 1]);
                  int randNum = Utilities.RandomGenerator.Next(maxNum);
                  string bName = buildingName + "_" + randNum.ToString();
                  return bName;
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTargetBuildingName(): reached default with name=" + buildingName);
         return "ERROR";
      }
   }
}
