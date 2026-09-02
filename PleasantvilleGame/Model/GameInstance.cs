using Microsoft.AspNetCore.Mvc;
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
      public Dictionary<IMapItem, IMapItem> AlienTakeovers { get; set; } = new Dictionary<IMapItem, IMapItem>();
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
      public IMapItems SelectedMapItems { set; get; } = new MapItems();
      public IMapItems DeadPeople { set; get; } = new MapItems();
      public IMapItem Zebulon { set; get; } = new MapItem("Zebulon", 0.8, "ZebulonBlack", new Territory(), 0, 0, 10);
      public IMapItemCombat MapItemCombat { set; get; } = new MapItemCombat();
      public IMapItemMove? PreviousMapItemMove { set; get; } = null;
      //---------------------------------------------------------------
      public string PlayerTurn { set; get; } = "Alien";
      public string NextAction { set; get; } = "";
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
      public void AddUnknownAlien(IMapItem newAlien)
      {
         newAlien.IsControlled = false;
         newAlien.IsAlienUnknown = true;
         newAlien.IsAlienKnown = false;
         newAlien.IsWary = false;
         newAlien.IsControlled = false;
      }
      public void AddKnownAlien(IMapItem newAlien)
      {
         newAlien.IsControlled = false;
         newAlien.IsAlienUnknown = false;
         newAlien.IsAlienKnown = true;
         newAlien.IsWary = false;
         newAlien.IsControlled = false;
         newAlien.IsSkeptical = false;
      }
      public void AddControlled(IMapItem controlled)
      {
         controlled.IsControlled = true;
         controlled.IsAlienUnknown = false;
         controlled.IsAlienKnown = false;
         controlled.IsWary = false;
         controlled.IsSurrendered = false;
         controlled.IsSkeptical = false;
         controlled.IsTiedUp = false;
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
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.ToString());
            return null;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.ToString());
            return null;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.ToString());
            return null;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.ToString());
         return mim;
      }
   }
}

