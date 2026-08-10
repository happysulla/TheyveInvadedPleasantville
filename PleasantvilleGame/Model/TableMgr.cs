using PleasantvilleGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PleasantvilleGame
{
   internal class TableMgr
   {
      static public CombatResult[,] theTable = new CombatResult[12, 5];
      public const int FN_ERROR = -1000;
      public const string TAVERN = "Tavern";
      public const string VET_OFFICE = "Vet Office";
      public const string CLOTHING = "Clothing Store";
      public const string GENERAL = "General Store";
      public const string PUMPS = "Gas Pumps";
      public const string MARKET = "Supermarket";
      public const string SCHOOL = "School";
      public const string BANK = "Bank";
      public const string DOC_OFFICE = "Doc Office";
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
      public const string LAWYER_OFFICE = "Lawyers Office";
      //------------------------------------------------
      public const string BANK_GUARD = "Bank Guard";
      public const string BANK_PRESIDENT = "Bank President";
      public const string BAR_OWNER = "Bar and Grill Owner";
      public const string BAR_TENDER = "Bar Tender";
      public const string CHECKOUTGIRL = "Checkout Girl";
      public const string DOCTOR = "Doctor";
      public const string DEPUTY = "Deputy";
      public const string FIRE_CHIEF = "Fire Chief";
      public const string HOTEL_OWNER = "Hotel Owner";
      public const string LAWYER = "Lawyer";
      public const string JUDGE = "Judge";
      public const string MAID = "Maid";
      public const string MAITRE_D = "MaitreD";
      public const string MAYOR = "Mayor";
      public const string MINSTER = "Minister";
      public const string PAPERBOY = "Paperboy";
      public const string PLUMBER = "Plumber";
      public const string REPAIR_SHOP_OWNER = "Repair Shop Owner";
      public const string SHERIFF = "Sheriff";
      public const string STATION_ATTENDANT = "Station Attendant";
      public const string SUPERMARKET_MGR = "Supermarket Manager";
      public const string TAILOR = "Tailor";
      public const string TEACHER = "Teacher";
      public const string TELLER = "Teller";
      public const string TOWN_DRUNK = "Town Drunk";
      public const string VET = "Vet";
      public const string WAITRESS = "Waitress";
      public const string WAR_VET = "War Veteran";
      public const string WELDER = "Welder";
      public const string WIFE = "Wife";
      public readonly static string[,] theTownpersonsTable = new string[5, 6]
      {
         {MAYOR,SHERIFF, PLUMBER, LAWYER, HOTEL_OWNER, JUDGE},
         {REPAIR_SHOP_OWNER, MAITRE_D, DOCTOR, TEACHER, MINSTER, BANK_PRESIDENT},
         {VET, BAR_OWNER, TELLER, STATION_ATTENDANT, CHECKOUTGIRL, PAPERBOY},
         {MAID, BANK_GUARD, TOWN_DRUNK, TAILOR, BAR_TENDER, WAR_VET},
         {WAITRESS, SUPERMARKET_MGR, FIRE_CHIEF, WIFE, WELDER, DEPUTY},
      };
      //---------------------------------------------------------------------
      public readonly static string[,] theTargetBuildingTable = new string[5, 6]
      {
         {TAVERN,VET_OFFICE,CLOTHING,GENERAL,PUMPS,MARKET},
         {SCHOOL,BANK,DOC_OFFICE,VFW,BAR,SHOP},
         {STATION,HALL,HOTEL,CHURCH,GRAVES,PEN},
         {TRAIN,HOUSEA,HOUSE1,HOUSE2,HOUSE3,HOUSE4},
         {HOUSE5,HOUSE6,HOUSE7,HOUSE8,LAWYER_OFFICE,HOUSEK},
      };
      //---------------------------------------------------------------------
      public readonly static string[] theTownPlayerStartingTable = new string[6] { BANK_PRESIDENT, DOCTOR, MAYOR, MINSTER, TEACHER, SHERIFF };
      //---------------------------------------------------------------------
      public readonly static string[,] theBuildingSizes = new string[21, 2] { { TAVERN, "3" }, { VET_OFFICE, "2" }, { CLOTHING, "2" }, { GENERAL, "4" }, { PUMPS, "1" }, { MARKET, "5" }, { SCHOOL, "4" }, { BANK, "4" }, { DOC_OFFICE, "2" }, { VFW, "1" }, { BAR, "2" }, { SHOP, "4" }, { STATION, "4" }, { HALL, "3" }, { HOTEL, "5" }, { CHURCH, "5" }, { GRAVES, "1" }, { PEN, "1" }, { TRAIN, "2" }, { "House", "10" }, { LAWYER_OFFICE, "1" } };
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
      static public bool GetCombatResult(int dieRoll, IMapItemCombat combat)
      {
         if (dieRoll < 2 || dieRoll > 12)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): dieRoll1=" + dieRoll.ToString() + " is out of range");
            return false;
         }
         if( 0 == combat.Attackers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): 0 == combat.Attackers.Count");
            return false;
         }
         if (0 == combat.Defenders.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): 0 == combat.Defenders.Count");
            return false;
         }
         IMapItem? firstAttacker = combat.Attackers[0];
         if( null == firstAttacker )
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): firstAttacker=null");
            return false;
         }
         IMapItem? firstDefender = combat.Attackers[0];
         if (null == firstDefender)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): firstDefender=null");
            return false;
         }
         //----------------------------------------
         if( (true == firstAttacker.IsControlled) && ((false == firstDefender.IsAlienKnown) && (false == firstDefender.IsAlienUnknown) ) ) // Controlled townspeople can attack uncontrolled automatic win
         {
            combat.Result = CombatResult.AttackerWins;
            return true;
         }
         else if ((true == firstDefender.IsControlled) && ((false == firstAttacker.IsAlienKnown) && (false == firstAttacker.IsAlienUnknown))) // Controlled townspeople can attack uncontrolled automatic win
         {
            combat.Result = CombatResult.DefenderWins;
            return true;
         }
         //----------------------------------------
         int totalCombatForAttacker = 0;
         foreach (IMapItem mi in combat.Attackers)
            totalCombatForAttacker += mi.Combat;
         int totalCombatForDefender = 0;
         foreach (IMapItem mi in combat.Defenders)
            totalCombatForDefender += mi.Combat;
         int differential = totalCombatForAttacker - totalCombatForDefender;
         if (differential < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_CombatResult(): 0 < (differential=" + differential.ToString() + ")");
            return false;
         }
         int tableFactor = 0;
         if (differential < 1)
            tableFactor = 0;
         else if (differential < 4)
            tableFactor = 1;
         else if (differential < 7)
            tableFactor = 2;
         else if (differential < 10)
            tableFactor = 3;
         else
            tableFactor = 4;
         //----------------------------------------
         IMapItems aliens = combat.Attackers;
         if (true == firstAttacker.IsControlled)
            aliens = combat.Defenders;
         foreach (IMapItem alien in aliens) // A column shift occurs if any aliens went through an influence attempt this turn.
         {
            if (true == alien.IsInfluencedThisTurn) 
            {
               if (true == firstAttacker.IsControlled)
               {
                  if (0 == differential) // shift column to right
                     differential = 1;
                  else if (1 == tableFactor)
                     tableFactor = 2;
                  else if (2 == tableFactor)
                     tableFactor = 3;
                  else if (3 == tableFactor)
                     tableFactor = 4;
               }
               else                                  
               {
                  if (1 == differential)   // shift column to left
                     tableFactor = 0;
                  else if (2 == differential)
                     tableFactor = 1;
                  else if (3 == differential)
                     tableFactor = 2;
                  else if (4 == differential)
                     tableFactor = 3;
               }
               break;  // only one column shift occurs.
            }
         }
         //----------------------------------------
         combat.Result = theTable[dieRoll, tableFactor];
         Logger.Log(LogEnum.LE_SHOW_COMBATS, "Get_CombatResult(): dr=" + dieRoll.ToString() + " d=" + differential.ToString() + " tf=" + tableFactor.ToString() + " result=" + combat.Result.ToString());
         return true;
      }
      static public bool CreateTownspeople(IGameInstance gi)
      {
         //------------------------------------
         string tName = "";
         ITerritory? t = null;
         int maxNum = 4;
         int randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Bank_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         string name = Utilities.RemoveSpaces(BANK_GUARD);
         string miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem mi = new MapItem(miName, 0.8, name, t, 5, 10, 8);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Bank_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(BANK_PRESIDENT);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 19, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "BarAndGrill_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(BAR_OWNER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Tavern_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(BAR_TENDER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Supermarket_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(CHECKOUTGIRL);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 7, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(DEPUTY);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 9);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "DocOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(DOCTOR);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 18, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(FIRE_CHIEF);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 12, 8);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(HOTEL_OWNER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "TownHall_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(JUDGE);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "LawyersOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(LAWYER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(MAID);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(MAITRE_D);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 4);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "GeneralStore_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(MAYOR);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 16, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.Name + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Church_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(MINSTER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 20, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.Name + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "House_K";
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(PAPERBOY);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 9, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(PLUMBER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 8);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(REPAIR_SHOP_OWNER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(SHERIFF);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 15, 10);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "GasPumps_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(STATION_ATTENDANT);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Supermarket_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(SUPERMARKET_MGR);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "ClothingStore_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(TAILOR);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 11, 5);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "School_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(TEACHER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 17, 4);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Bank_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(TELLER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Tavern_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(TOWN_DRUNK);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 3, 3, 8);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "VetOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(VET);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 13, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(WAITRESS);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "TrainStation_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(WAR_VET);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 12, 4);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(WELDER);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "House_A";
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = Utilities.RemoveSpaces(WIFE);
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 8, 4);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         return true;
      }
      static public string GetTownspersonName(int die1, int die2)
      {
         if (die1 < 0 || 5 < die1)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.Get_Townsperson(): die1 out of range: " + die1);
            return "ERROR";
         }
         if (die2 < 0 || 6 < die2)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgr.Get_Townsperson(): die2 out of range: " + die2);
            return "ERROR";
         }
         string name = Utilities.RemoveSpaces(theTownpersonsTable[die1, die2]);
         return name;
      }
      static public string GetTownspersonName(IMapItem mi)
      {
         for (int i = 0; i < 5; ++i)
         {
            for (int k = 0; k < 6; ++k)
            {
               string matchingName = Utilities.RemoveSpaces(theTownpersonsTable[i, k]);
               if (true == mi.Name.Contains(matchingName))
                  return theTownpersonsTable[i, k];
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "Get_TownspersonName(): no matching mi=" + mi.Name);
         return "ERROR";
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
         if (true == buildingNameWithoutSpaces.Contains("House"))
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
         if (true == isBuilding)
         {
            switch (range)
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
