using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
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
      public IPlayerTown PlayerTown { set; get; } = new PlayerTownHuman();
      public IPlayerAlien PlayerAlien { set; get; } = new PlayerAlienComputer();
      //------------------------------------------------
      public String[] StartingTownspeople { get; set; } = new String[3];
      public List<RandomMoveData> RandomMoves { get; set; } = new List<RandomMoveData>();
      //------------------------------------------------
      public IGameCommands GameCommands { set; get; } = new GameCommands();
      public Options Options { get; set; } = new Options();
      public GameStatistics Statistics { get; set; } = new GameStatistics();
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
      public IStack? SelectedStack { get; set; } = null;
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
      public ITerritories SelectedTerritories { set; get; } = new Territories();
      public ITerritory? SelectedTerritory { set; get; } = null;
      public IMapItem Zebulon { set; get; } = new MapItem("Zebulon", 0.8, "ZebulonBlack", new Territory(), 0, 0, 10);
      public IMapItems SelectedMapItems { set; get; } = new MapItems();
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
      public int NumTownGuessesForZebulonLocation { set; get; } = 0;
      public bool IsAlienStarted { set; get; } = false;
      public bool IsTownsStarted { set; get; } = false;
      public bool IsAlienDisplayedRandomMovement { set; get; } = false;
      public bool IsTownDisplayedRandomMovement { set; get; } = false;
      public bool IsAlienAckedRandomMovement { set; get; } = false;
      public bool IsTownsAckedRandomMovement { set; get; } = false;
      public bool IsAlienInitiatedCombat { set; get; } = false;
      public bool IsTownsInitiatedCombat { set; get; } = false;
      public bool IsAlienCombatCompleted { set; get; } = false;
      public bool IsTownsCombatCompleted { set; get; } = false;
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {

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
         StringBuilder sb = new StringBuilder("AddUnknownAlien():"); 
         if (false == newAlien.IsAlienUnknown)
         {
            if ((false == newAlien.IsTiedUp) && (false == newAlien.IsUnconscious) && (false == newAlien.IsStunned) && (false == newAlien.IsSurrendered) && (false == newAlien.IsKilled))
            {
               if (true == newAlien.IsControlled)
               {
                  InfluenceCountTownspeople -= newAlien.Influence;
                  sb.Append(newAlien.Name); sb.Append(" ---- from TP "); 
                  sb.Append(newAlien.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountAlienUnknown += newAlien.Influence;
               sb.Append(newAlien.Name); 
               sb.Append(" ++++ to Unknown "); 
               sb.Append(newAlien.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            newAlien.IsAlienUnknown = true;
            newAlien.IsWary = false;
            newAlien.IsControlled = false;
         }
         return true;
      }
      public bool AddKnownAlien(IMapItem newAlien)
      {
         StringBuilder sb = new StringBuilder("Add_KnownAlien():");
         if (false == newAlien.IsAlienKnown)  // Do not add if already known alien
         {
            if ((false == newAlien.IsTiedUp) && (false == newAlien.IsUnconscious) && (false == newAlien.IsStunned) && (false == newAlien.IsSurrendered) && (false == newAlien.IsKilled))
            {
               if (true == newAlien.IsControlled)
               {
                  InfluenceCountTownspeople -= newAlien.Influence;
                  sb.Append(newAlien.Name); 
                  sb.Append(" ---- from TP "); 
                  sb.Append(newAlien.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               if (true == newAlien.IsAlienUnknown) // DeternewAlienne if already an alien.  If already an alien, need to remove from that influence 
               {
                  InfluenceCountAlienUnknown -= newAlien.Influence;
                  sb.Append(newAlien.Name); 
                  sb.Append(" ---- from Unknown "); 
                  sb.Append(newAlien.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               InfluenceCountAlienKnown += newAlien.Influence;
               sb =  sb.Append(newAlien.Name); 
               sb.Append(" ++++ to Known "); 
               sb.Append(newAlien.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            newAlien.IsWary = false;
            newAlien.IsControlled = false;
            newAlien.IsAlienUnknown = false;
            newAlien.IsAlienKnown = true;
         }
         return true;
      }
      public bool AddControlled(IMapItem controlled)
      {
         if (false == controlled.IsControlled)
         {
            if ((false == controlled.IsTiedUp) && (false == controlled.IsUnconscious) && (false == controlled.IsStunned) && (false == controlled.IsKilled))
            {
               if ((true == controlled.IsAlienKnown) && (false == controlled.IsSurrendered))
               {
                  InfluenceCountAlienKnown -= controlled.Influence;
                  StringBuilder sb0 = new StringBuilder("AddControlled():"); sb0.Append(controlled.Name); sb0.Append(" ---- from known "); sb0.Append(controlled.Influence.ToString());
                  sb0.Append(" T="); sb0.Append(this.InfluenceCountTotal.ToString());
                  sb0.Append(" Known="); sb0.Append(this.InfluenceCountAlienKnown.ToString());
                  sb0.Append(" UnKnown="); sb0.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb0.Append(" TP="); sb0.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb0.ToString());
               }

               if ((true == controlled.IsAlienUnknown) && (false == controlled.IsSurrendered))
               {
                  InfluenceCountAlienUnknown -= controlled.Influence;
                  StringBuilder sb1 = new StringBuilder("AddControlled():"); sb1.Append(controlled.Name); sb1.Append(" ---- from unknown "); sb1.Append(controlled.Influence.ToString());
                  sb1.Append(" T="); sb1.Append(this.InfluenceCountTotal.ToString());
                  sb1.Append(" Known="); sb1.Append(this.InfluenceCountAlienKnown.ToString());
                  sb1.Append(" UnKnown="); sb1.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb1.Append(" TP="); sb1.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb1.ToString());
               }

               if (true == controlled.IsSurrendered)
               {
                  InfluenceCountTotal += controlled.Influence; // A surrendered alien that converts gets added back to total influence
                  StringBuilder sb2 = new StringBuilder("AddControlled():"); sb2.Append(controlled.Name); sb2.Append(" ++++ to total "); sb2.Append(controlled.Influence.ToString());
                  sb2.Append(" T="); sb2.Append(this.InfluenceCountTotal.ToString());
                  sb2.Append(" Known="); sb2.Append(this.InfluenceCountAlienKnown.ToString());
                  sb2.Append(" UnKnown="); sb2.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb2.Append(" TP="); sb2.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb2.ToString());
               }

               InfluenceCountTownspeople += controlled.Influence;
               StringBuilder sb3 = new StringBuilder("AddControlled():"); sb3.Append(controlled.Name); sb3.Append(" ++++ to TP "); sb3.Append(controlled.Influence.ToString());
               sb3.Append(" T="); sb3.Append(this.InfluenceCountTotal.ToString());
               sb3.Append(" Known="); sb3.Append(this.InfluenceCountAlienKnown.ToString());
               sb3.Append(" UnKnown="); sb3.Append(this.InfluenceCountAlienUnknown.ToString());
               sb3.Append(" TP="); sb3.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb3.ToString());
            }
            controlled.IsAlienKnown = false;
            controlled.IsControlled = true;
            controlled.IsWary = false;
            controlled.IsSurrendered = false;
            controlled.IsAlienUnknown = false;
         }
         return true;
      }
      public IMapItemMove? CreateMapItemMove(IMapItem mi, ITerritory newT)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.ToString());
         return mim;
      }
   }
}

