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
      public const string TAVERN = "Tavern";
      public const string VET = "Vet Office";
      public const string CLOTHING = "Clothing Store";
      public const string GENERAL = "General Store";
      public const string PUMPS = "Gas Pumps";
      public const string MARKET = "Supermarket";
      public const string SCHOOL = "School";
      public const string BANK = "Bank";
      public const string DOC = "Doc Office";
      public const string VFW = "VFW";
      public const string BAR = "Bar And Grill";
      public const string SHOP = "Machine Shop";
      public const string STATION = "Sheriff Fire Dept";
      public const string HALL = "Town Hall";
      public const string HOTEL = "Hotel And Restaurant";
      public const string CHURCH = "Church";
      public const string GRAVES = "Graveyard";
      public const string PEN = "Stock Pen";
      public const string TRAIN = "Train Station";
      public const string HOUSEA = "House A";
      public const string HOUSE1 = "House 1";
      public const string HOUSE2 = "House 2";
      public const string HOUSE3 = "House 3";
      public const string HOUSE4 = "House 4";
      public const string HOUSE5 = "House 5";
      public const string HOUSE6 = "House 6";
      public const string HOUSE7 = "House 7";
      public const string HOUSE8 = "House 8";
      public const string HOUSEK = "House K";
      public const string LAWYER = "Lawyers Office";
      //------------------------------------------------
      public const string BANKPRESIDENT = "Bank President";
      public const string DOCTOR = "Doctor";
      public const string MAYOR = "Mayor";
      public const string MINSTER = "Minister";
      public const string TEACHER = "Teacher";
      public const string SHERIFF = "Sheriff";
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
         {TAVERN,VET,CLOTHING,GENERAL,PUMPS,MARKET},
         {SCHOOL,BANK,DOC,VFW,BAR,SHOP},
         {STATION,HALL,HOTEL,CHURCH,GRAVES,PEN},
         {TRAIN,HOUSEA,HOUSE1,HOUSE2,HOUSE3,HOUSE4},
         {HOUSE5,HOUSE6,HOUSE7,HOUSE8,LAWYER,HOUSEK},
      };
      //---------------------------------------------------------------------
      public readonly static string[] theTownPlayerStartingTable = new string[6] { BANKPRESIDENT, DOCTOR, MAYOR, MINSTER, TEACHER, SHERIFF };
      //---------------------------------------------------------------------
      public readonly static string[,] theBuildingSizes = new string[21, 2] { {TAVERN,"3"}, {VET,"2"},{CLOTHING,"2"},{GENERAL,"4"},{PUMPS,"1"},{MARKET,"5"},{SCHOOL,"4"},{BANK,"4"},{DOC,"2"},{VFW,"1"},{BAR,"2"},{SHOP,"4"},{STATION,"4"},{HALL,"3"},{HOTEL,"5"},{CHURCH,"5"},{GRAVES,"1"},{PEN,"1"},{TRAIN,"2"},{"House","10"},{LAWYER,"1"} };
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
         string buildingName = theTargetBuildingTable[die1, die2];   
         string buildingNameWithoutSpaces = Utilities.RemoveSpaces(buildingName);
         if (true ==  buildingNameWithoutSpaces.Contains("House") )
         {
            string modified = buildingName.Replace(' ', '_');
            return modified;
         }
         else
         {
            int arraySize = theBuildingSizes.GetLength(0);
            for (int i = 0; i < arraySize; i++)
            {
               string matchingName = Utilities.RemoveSpaces(theBuildingSizes[i, 0]);
               if (matchingName == buildingNameWithoutSpaces)
               {
                  int maxNum = Convert.ToInt32(theBuildingSizes[i, 1]);
                  int randNum = Utilities.RandomGenerator.Next(maxNum);
                  string bName = buildingNameWithoutSpaces + "_" + randNum.ToString();
                  return bName;
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "TableMgr.GetTargetBuildingName(): reached default with name=" + buildingName);
         return "ERROR";
      }
      static public double GetObservationChance(int range, bool isBuilding)
      {
         if( true == isBuilding )
         {
            switch(range)
            {
               case 0: return 0.666667;
               case 1: return 0.5;
               case 2: return 0.333333;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetObservationChance(): reached default isBuilding=true range=" + range.ToString());
                  return (double)FN_ERROR;
            }
         }
         else
         {
            switch (range)
            {
               case 0: return 0.666667;
               case 1: return 0.5;
               case 2: return 0.333333;
               case 3: return 0.166667;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetObservationChance(): reached default isBuilding=false range=" + range.ToString());
                  return (double)FN_ERROR;
            }
         }
      }
   }
}
