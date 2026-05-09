using System.Text;
using System.Windows;
using System.Xml;
using MessageBox=System.Windows.MessageBox;

namespace PleasantvilleGame
{ 
   public class GameInstance : IGameInstance
   {
      static public Logger Logger = new Logger();
      public bool CtorError { get; } = false;
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //----------------------------------------------
      public IPlayerTown? PlayerTown { set; get; } = null;
      public IPlayerAlien? PlayerAlien { set; get; } = null;
      //------------------------------------------------
      public IGameCommands GameCommands { set; get; } = new GameCommands();
      public Options Options { get; set; } = new Options();
      public GameStatistics Statistics { get; set; } = new GameStatistics();
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //------------------------------------------------
      public bool IsMultipleSelectForDieResult { set; get; } = false;
      public bool IsGridActive { set; get; } = false;
      public IUndo? UndoCmd { set; get; } = null;
      //------------------------------------------------
      public Guid GameGuid { get; set; } = Guid.NewGuid();
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      //------------------------------------------------
      public int Day { get; set; } = 0;
      public int GameTurn { get; set; } = 0; 
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public string EndGameReason { set; get; } = "";
      //----------------------------------------------
      public ITerritories ZebulonTerritories { set; get; } = new Territories();
      public IMapItems Townspeople { set; get; } = new MapItems();
      public IMapItems PersonsStunned { set; get; } = new MapItems();
      public IMapItems PersonsKnockedOut { set; get; } = new MapItems();
      public IMapItemCombat? MapItemCombat { set; get; } = null;
      public IMapItemTakeover? Takeover { set; get; } = null;
      public IMapItemMove? PreviousMapItemMove { set; get; } = null;
      //---------------------------------------------------------------
      public string PlayerTurn { set; get; } = "Alien";
      public string NextAction { set; get; } = "";
      public int InfluenceCountTotal { set; get; } = 0;
      public int InfluenceCountTownspeople { set; get; } = 0;
      public int InfluenceCountAlienUnknown { set; get; } = 0;
      public int InfluenceCountAlienKnown { set; get; } = 0;
      public int NumIterogationsThisTurn { set; get; } = 0;
      public bool IsAlienStarted { set; get; } = false;
      public bool IsControlledStarted { set; get; } = false;
      public bool IsAlienDisplayedRandomMovement { set; get; } = false;
      public bool IsControlledDisplayedRandomMovement { set; get; } = false;
      public bool IsAlienAckedRandomMovement { set; get; } = false;
      public bool IsControlledAckedRandomMovement { set; get; } = false;
      public bool IsAlienInitiatedCombat { set; get; } = false;
      public bool IsControlledInitiatedCombat { set; get; } = false;
      public bool IsAlienCombatCompleted { set; get; } = false;
      public bool IsControlledCombatCompleted { set; get; } = false;
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {
         if( false == CreateTownspeople())
         {
            CtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): Create_Townspeople() returned false");
            return;
         }
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         Options = newGameOptions;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(GamePhase.ToString());
         sb.Append("]");
         return sb.ToString();
      }
      //---------------------------------------------------------------
      public bool CreateTownspeople()
      {
         this.Townspeople.Clear();
         //------------------------------------
         int randomNum = Utilities.RandomGenerator.Next(4);
         string tName = "Bank_" + randomNum.ToString();
         ITerritory? t = Territories.theTerritories.Find(tName);
         if( null == t )
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         string name = "BankGuard";
         string miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem mi = new MapItem(miName, 1.0, name, t, 5, 10, 8);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "Bank_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "BankPresident";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 4, 19, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(2);
         tName = "BarAndGrill_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "BarAndGrillOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 10, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(3);
         tName = "Tavern_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "BarTender";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 6, 11, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "Supermarket_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "CheckoutGirl";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 7, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "SheriffFireDept_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Deputy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 6, 11, 9);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(2);
         tName = "DocOffice_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Doctor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 18, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "SheriffFireDept_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "FireChief";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 6, 12, 8);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "HotelAndRestaurant_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "HotelOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 11, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(3);
         tName = "TownHall_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Judge";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 11, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         tName = "LawyersOffice_0";
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Lawyer";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 11, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "HotelAndRestaurant_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Maid";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 10, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "HotelAndRestaurant_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "MaitreD";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 9, 4);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "GeneralStore_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Mayor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 16, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "Church_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Minister";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 20, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         tName = "House_K";
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Paperboy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 6, 9, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "MachineShop_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Plumber";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 8, 8);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "MachineShop_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "RepairShopOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 9, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "SheriffFireDept_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Sheriff";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 6, 15, 10);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         tName = "GasPumps_0";
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "StationAttendant";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 8, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "Supermarket_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "SuperMarketManager";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 10, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(2);
         tName = "ClothingStore_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Tailor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 4, 11, 5);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "School_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Teacher";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 17, 4);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "Bank_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Teller";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 9, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(3);
         tName = "Tavern_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "TownDrunk";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 3, 3, 8);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(2);
         tName = "VetOffice_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Vet";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 13, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(5);
         tName = "HotelAndRestaurant_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Waitress";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 9, 6);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(2);
         tName = "TrainStation_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "WarVeteran";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 4, 12, 4);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         randomNum = Utilities.RandomGenerator.Next(4);
         tName = "MachineShop_" + randomNum.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Welder";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 5, 10, 7);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         //------------------------------------
         tName = "House_A";
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         name = "Wife";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 1.0, name, t, 4, 8, 4);
         this.Townspeople.Add(mi);
         this.Stacks.Add(mi);
         return true;
      }
      public bool AddUnknownAlien(IMapItem newAlien)
      {
         StringBuilder sb = new StringBuilder("AddUnknownAlien():"); 
         IMapItem? mi = this.Stacks.FindMapItem(newAlien.Name);
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): AddUnknownAlien() mi=null for name=" + newAlien.Name);
            return false;
         }
         if (false == mi.IsAlienUnknown)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb.Append(mi.Name); sb.Append(" ---- from TP "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountAlienUnknown += mi.Influence;
               sb.Append(mi.Name); 
               sb.Append(" ++++ to Unknown "); 
               sb.Append(mi.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            mi.IsAlienUnknown = true;
            mi.IsWary = false;
            mi.IsControlled = false;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddUnknownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddKnownAlien(IMapItem newAlien)
      {
         StringBuilder sb = new StringBuilder("AddKnownAlien():"); ;
         IMapItem? mi = this.Stacks.FindMapItem(newAlien.Name);
         if (null == mi)
         {
            Console.WriteLine("GameInstance::AddKnownAlien() - ERROR mi = null ");
            return false;
         }
         if (false == mi.IsAlienKnown)  // Do not add if already known alien
         {
            // Stunned, tied-up, and surrendered townspeople can be taken over. 
            // However, influence probably not going to be added to Alien total until  condition expires.
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb.Append(mi.Name); 
                  sb.Append(" ---- from TP "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               if (true == mi.IsAlienUnknown) // Determine if already an alien.  If already an alien, need to remove from that influence 
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  sb.Append(mi.Name); 
                  sb.Append(" ---- from Unknown "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               InfluenceCountAlienKnown += mi.Influence;
               sb =  sb.Append(mi.Name); 
               sb.Append(" ++++ to Known "); 
               sb.Append(mi.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }

            mi.IsWary = false;
            mi.IsControlled = false;
            mi.IsAlienUnknown = false;
            mi.IsAlienKnown = true;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddKnownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddTownperson(IMapItem mi)
      {
         if (false == mi.IsControlled)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsKilled))
            {
               if ((true == mi.IsAlienKnown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienKnown -= mi.Influence;
                  StringBuilder sb0 = new StringBuilder("AddTownperson():"); sb0.Append(mi.Name); sb0.Append(" ---- from known "); sb0.Append(mi.Influence.ToString());
                  sb0.Append(" T="); sb0.Append(this.InfluenceCountTotal.ToString());
                  sb0.Append(" Known="); sb0.Append(this.InfluenceCountAlienKnown.ToString());
                  sb0.Append(" UnKnown="); sb0.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb0.Append(" TP="); sb0.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb0.ToString());
               }

               if ((true == mi.IsAlienUnknown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  StringBuilder sb1 = new StringBuilder("AddTownperson():"); sb1.Append(mi.Name); sb1.Append(" ---- from unknown "); sb1.Append(mi.Influence.ToString());
                  sb1.Append(" T="); sb1.Append(this.InfluenceCountTotal.ToString());
                  sb1.Append(" Known="); sb1.Append(this.InfluenceCountAlienKnown.ToString());
                  sb1.Append(" UnKnown="); sb1.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb1.Append(" TP="); sb1.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb1.ToString());
               }

               if (true == mi.IsSurrendered)
               {
                  InfluenceCountTotal += mi.Influence; // A surrendered alien that converts gets added back to total influence
                  StringBuilder sb2 = new StringBuilder("AddTownperson():"); sb2.Append(mi.Name); sb2.Append(" ++++ to total "); sb2.Append(mi.Influence.ToString());
                  sb2.Append(" T="); sb2.Append(this.InfluenceCountTotal.ToString());
                  sb2.Append(" Known="); sb2.Append(this.InfluenceCountAlienKnown.ToString());
                  sb2.Append(" UnKnown="); sb2.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb2.Append(" TP="); sb2.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb2.ToString());
               }

               InfluenceCountTownspeople += mi.Influence;
               StringBuilder sb3 = new StringBuilder("AddTownperson():"); sb3.Append(mi.Name); sb3.Append(" ++++ to TP "); sb3.Append(mi.Influence.ToString());
               sb3.Append(" T="); sb3.Append(this.InfluenceCountTotal.ToString());
               sb3.Append(" Known="); sb3.Append(this.InfluenceCountAlienKnown.ToString());
               sb3.Append(" UnKnown="); sb3.Append(this.InfluenceCountAlienUnknown.ToString());
               sb3.Append(" TP="); sb3.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb3.ToString());
            }
            mi.IsAlienKnown = false;
            mi.IsControlled = true;
            mi.IsWary = false;
            mi.IsSurrendered = false;
            mi.IsAlienUnknown = false;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            Logger.Log(LogEnum.LE_ERROR, "AddTownperson() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
   }
}

