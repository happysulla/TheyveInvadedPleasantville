
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;

namespace PleasantvilleGame
{ 
   public class GameInstance : IGameInstance
   {
      static public Logger Logger = new Logger();
      public bool CtorError { get; } = false;
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //------------------------------------------------
      public IGameCommands GameCommands { set; get; } = new GameCommands();
      public Options Options { get; set; } = new Options();
      public GameStatistics Statistics { get; set; } = new GameStatistics();
      public int MaxDayBetweenCombat { get; set; } = 0;
      public int MaxRollsForAirSupport { get; set; } = 0;
      public int MaxRollsForArtillerySupport { get; set; } = 0;
      public int MaxEnemiesInOneBattle { get; set; } = 0;
      public int RoundsOfCombat { get; set; } = 0;
      public int NumOfBattles { get; set; } = 0;
      public int NumKiaSherman { get; set; } = 0;
      public int NumKia { get; set; } = 0;
      public bool IsFirstSpottingOccurred { get; set; } = false;
      public bool Is1stEnemyStrengthCheckTerritory { get; set; } = true;
      //------------------------------------------------
      public bool IsMultipleSelectForDieResult { set; get; } = false;
      public bool IsGridActive { set; get; } = false;
      public IUndo? UndoCmd { set; get; } = null;
      //------------------------------------------------
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      //------------------------------------------------
      public Guid GameGuid { get; set; } = Guid.NewGuid();
      public int Day { get; set; } = 0;
      public int GameTurn { get; set; } = 0;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public string EndGameReason { set; get; } = "";
      //------------------------------------------------
      public IAfterActionReports Reports { get; set; } = new AfterActionReports();
      public BattlePhase BattlePhase { set; get; } = BattlePhase.None;
      public CrewActionPhase CrewActionPhase { set; get; } = CrewActionPhase.None;
      public string MovementEffectOnSherman { set; get; } = "unintialized";
      public string MovementEffectOnEnemy { set; get; } = "unintialized";
      public string ShermanTypeOfFire { set; get; } = "";
      public string FiredAmmoType { set; get; } = "";
      //---------------------------------------------------------------
      public IMapItems ReadyRacks { set; get; } = new MapItems();
      public IMapItems Hatches { set; get; } = new MapItems();
      public IMapItems CrewActions { set; get; } = new MapItems();
      public IMapItems GunLoads { set; get; } = new MapItems();
      public IMapItems Targets { set; get; } = new MapItems();
      public IMapItems AdvancingEnemies { set; get; } = new MapItems();   // enemies that appear on Move board for advancing to lower edge of board
      public IMapItems ShermanAdvanceOrRetreatEnemies { set; get; } = new MapItems(); // enemies that appear on Move board if Sherman Advances or Retreats
      //---------------------------------------------------------------
      public ICrewMembers NewMembers { set; get; } = new CrewMembers();
      public ICrewMembers InjuredCrewMembers { set; get; } = new CrewMembers();
      public ICrewMember Commander { get; set; } = new CrewMember("Commander", "Sgt", "c07Commander");
      public ICrewMember Gunner { get; set; } = new CrewMember("Gunner", "Cpl", "c11Gunner");
      public ICrewMember Loader { get; set; } = new CrewMember("Loader", "Cpl", "c09Loader");
      public ICrewMember Driver { get; set; } = new CrewMember("Driver", "Pvt", "c08Driver");
      public ICrewMember Assistant { get; set; } = new CrewMember("Assistant", "Pvt", "c10Assistant");
      //---------------------------------------------------------------
      public IMapItem Sherman { set; get; } = new MapItem("Sherman75" + Utilities.MapItemNum.ToString(), 2.0, "t01", new Territory());
      public IMapItem? TargetMainGun { set; get; } = null;
      public IMapItem? TargetMg { set; get; } = null;
      public IMapItem? ShermanFiringAtFront { set; get; } = null;
      public IMapItem? ShermanHvss { set; get; } = null;
      public ICrewMember? ReturningCrewman { set; get; } = null;
      //------------------------------------------------
      public ITerritories AreaTargets { get; set; } = new Territories();
      public ITerritories CounterattachRetreats { get; set; } = new Territories();
      //---------------------------------------------------------------
      public ITerritory Home { get; set; } = new Territory();
      public ITerritory? EnemyStrengthCheckTerritory { get; set; } = null;
      public ITerritory? ArtillerySupportCheck { get; set; } = null;
      public ITerritory? AirStrikeCheckTerritory { get; set; } = null;
      public ITerritory? EnteredArea { get; set; } = null;
      public ITerritory? AdvanceFire { get; set; } = null;
      public ITerritory? FriendlyAdvance { get; set; } = null;
      public ITerritory? EnemyAdvance { get; set; } = null;
      //------------------------------------------------
      public bool IsHatchesActive { set; get; } = false;
      public bool IsRetreatToStartArea { set; get; } = false;
      public bool IsShermanAdvancingOnBattleBoard { set; get; } = false;
      public bool IsShermanAdvancingOnMoveBoard { set; get; } = false;
      public bool IsLoaderSpotThisTurn { set; get; } = false;
      public bool IsCommanderSpotThisTurn { set; get; } = false;
      //---------------------------------------------------------------
      public string SwitchedCrewMemberRole { set; get; } = "";
      public int AssistantOriginalRating { set; get; } = 0;
      //---------------------------------------------------------------
      public bool IsFallingSnowStopped { set; get; } = false;
      public int HoursOfRainThisDay { set; get; } = 0;
      public int MinSinceLastCheck { set; get; } = 0;
      //---------------------------------------------------------------
      public double ShermanTurretRotationOld { set; get; } = 0.0;
      public bool IsShermanTurretRotatedThisRound { set; get; } = false;
      public int ShermanConsectiveMoveAttempt { set; get; } = 0;
      public bool IsShermanDeliberateImmobilization { set; get; } = false;
      public int NumSmokeAttacksThisRound { set; get; } = 0;
      public bool IsMalfunctionedMainGun { set; get; } = false;
      public bool IsMainGunRepairAttempted { set; get; } = false;
      public bool IsBrokenMainGun { set; get; } = false;
      public bool IsBrokenGunSight { set; get; } = false;
      public List<string> FirstShots { set; get; } = new List<string>();
      public List<string> TrainedGunners { get; } = new List<string>();
      public List<string> EnteredWoodedAreas { set; get; } = new List<string>();
      public List<ShermanAttack> ShermanHits { set; get; } = new List<ShermanAttack>();
      public ShermanDeath? Death { set; get; } = null;
      public ShermanSetup BattlePrep { set; get; } = new ShermanSetup();  // only used if option AutoPreparation is enabled
      //---------------------------------------------------------------
      public string IdentifiedTank { set; get; } = "";
      public string IdentifiedAtg { set; get; } = "";
      public string IdentifiedSpg { set; get; } = "";
      //---------------------------------------------------------------
      public bool IsShermanFiringAaMg { set; get; } = false;
      public bool IsShermanFiringBowMg { set; get; } = false;
      public bool IsShermanFiringCoaxialMg { set; get; } = false;
      public bool IsShermanFiringSubMg { set; get; } = false;
      public bool IsCommanderDirectingMgFire { set; get; } = false;
      public bool IsShermanFiredAaMg { set; get; } = false;
      public bool IsShermanFiredBowMg { set; get; } = false;
      public bool IsShermanFiredCoaxialMg { set; get; } = false;
      public bool IsShermanFiredSubMg { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsMalfunctionedMgAntiAircraft { set; get; } = false;
      public bool IsMalfunctionedMgBow { set; get; } = false;
      public bool IsMalfunctionedMgCoaxial { set; get; } = false;
      public bool IsCoaxialMgRepairAttempted { set; get; } = false;
      public bool IsBowMgRepairAttempted { set; get; } = false;
      public bool IsAaMgRepairAttempted { set; get; } = false;
      public bool IsBrokenMgAntiAircraft { set; get; } = false;
      public bool IsBrokenMgBow { set; get; } = false;
      public bool IsBrokenMgCoaxial { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsBrokenPeriscopeDriver { set; get; } = false;
      public bool IsBrokenPeriscopeLoader { set; get; } = false;
      public bool IsBrokenPeriscopeAssistant { set; get; } = false;
      public bool IsBrokenPeriscopeGunner { set; get; } = false;
      public bool IsBrokenPeriscopeCommander { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsCounterattackAmbush { set; get; } = false;
      public bool IsLeadTank { set; get; } = false;
      public bool IsAirStrikePending { set; get; } = false;
      public bool IsAdvancingFireChosen { set; get; } = false;
      public int AdvancingFireMarkerCount { set; get; } = 0;
      public EnumResistance BattleResistance { set; get; } = EnumResistance.None;
      //---------------------------------------------------------------
      public bool IsMinefieldAttack { set; get; } = false;
      public bool IsHarrassingFireBonus { set; get; } = false;
      public bool IsFlankingFire { set; get; } = false;
      public bool IsEnemyAdvanceComplete { set; get; } = false;
      public PanzerfaustAttack? Panzerfaust { set; get; } = null;
      public int NumCollateralDamage { set; get; } = 0;
      //---------------------------------------------------------------
      public int TankReplacementNumber { set; get; } = 0;
      public int Fuel { set; get; } = 35;
      public int VictoryPtsTotalCampaign { get; set; } = 0;
      public Dictionary<string, int> CommanderPromoPoints { set; get; } = new Dictionary<string, int>();
      public int PromotionDay { get; set; } = -1;
      public int NumPurpleHeart { get; set; } = 0;
      public bool IsCommanderRescuePerformed { set; get; } = false;
      public bool IsCommnderFightiingFromOpenHatch { set; get; } = false;
      public bool IsCommanderKilled { set; get; } = false;
      public bool IsPromoted { set; get; } = false;
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks MoveStacks { get; set; } = new Stacks();
      public IStacks BattleStacks { get; set; } = new Stacks();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {
         try
         {
            GameLoadMgr gameLoadMgr = new GameLoadMgr();
            string filename = ConfigFileReader.theConfigDirectory + Territories.FILENAME;
            XmlTextReader? reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): reader=null");
               return;
            }
            if (false == gameLoadMgr.ReadXmlTerritories(reader, Territories.theTerritories))
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned false for filename=" + filename);
            ITerritory? tHome = Territories.theTerritories.Find("Home");
            if (null == tHome)
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): tHome=null");
            else
               Home = tHome;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() exception=\n" + e.ToString());
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
      public bool AddUnknownAlien(IMapItem newAlien)
      {
         StringBuilder sb = null;
         IMapItem mi = this.Persons.Find(newAlien.Name);
         if (null == mi)
         {
            Console.WriteLine("GameInstance::AddUnknownAlien() - ERROR mi = null ");
            return false;
         }

         if (false == mi.IsAlienUnknown)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb = new StringBuilder("AddUnknownAlien():"); sb.Append(mi.Name); sb.Append(" ---- from TP "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountAlienUnknown += mi.Influence;
               sb = new StringBuilder("AddUnknownAlien():"); sb.Append(mi.Name); sb.Append(" ++++ to Unknown "); sb.Append(mi.Influence.ToString());
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
         if (false == Utilities.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddUnknownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddKnownAlien(IMapItem newAlien)
      {
         StringBuilder sb = null;
         IMapItem mi = this.Persons.Find(newAlien.Name);
         if (null == mi)
         {
            Console.WriteLine("GameInstance::AddKnownAlien() - ERROR mi = null ");
            return false;
         }

         if (false == mi.IsAlienKnown)  // Do not add if already known alien
         {
            // Stunned, tied-up, and surrendered townspeople can be taken over. 
            // However, influence probably not going to be added to Alien total until 
            // condition expires.


            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb = new StringBuilder("AddKnownAlien():"); sb.Append(mi.Name); sb.Append(" ---- from TP "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               // Determine if already an alien.  If already an alien, need to remove from that influence 

               if (true == mi.IsAlienUnknown)
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  sb = new StringBuilder("AddKnownAlien():"); sb.Append(mi.Name); sb.Append(" ---- from Unknown "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountAlienKnown += mi.Influence;
               sb = new StringBuilder("AddKnownAlien():"); sb.Append(mi.Name); sb.Append(" ++++ to Known "); sb.Append(mi.Influence.ToString());
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
         if (false == Utilities.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddKnownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddTownperson(IMapItem newPerson)
      {
         StringBuilder sb = null;

         IMapItem mi = this.Persons.Find(newPerson.Name);
         if (null == mi)
         {
            Console.WriteLine("GameInstance::AddTownperson() - ERROR mi = null ");
            return false;
         }

         if (false == mi.IsControlled)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsKilled))
            {
               if ((true == mi.IsAlienKnown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienKnown -= mi.Influence;
                  sb = new StringBuilder("AddTownperson():"); sb.Append(mi.Name); sb.Append(" ---- from known "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               if ((true == mi.IsAlienUnknown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  sb = new StringBuilder("AddTownperson():"); sb.Append(mi.Name); sb.Append(" ---- from unknown "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               if (true == mi.IsSurrendered)
               {
                  InfluenceCountTotal += mi.Influence; // A surrendered alien that converts gets added back to total influence
                  sb = new StringBuilder("AddTownperson():"); sb.Append(mi.Name); sb.Append(" ++++ to total "); sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountTownspeople += mi.Influence;
               sb = new StringBuilder("AddTownperson():"); sb.Append(mi.Name); sb.Append(" ++++ to TP "); sb.Append(mi.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            mi.IsAlienKnown = false;
            mi.IsControlled = true;
            mi.IsWary = false;
            mi.IsSurrendered = false;
            mi.IsAlienUnknown = false;
         }
         if (false == Utilities.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddTownperson() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
   }
}

