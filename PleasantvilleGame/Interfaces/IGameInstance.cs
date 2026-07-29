using System;
using System.Collections.Generic;

namespace PleasantvilleGame
{
   public struct RandomMoveData
   {
      public string myName;
      public string myBuildingName;
      public int myBrushIndex;
      public RandomMoveData(string name, string buildingName)
      {
         myName = name;
         myBuildingName = buildingName;
      }
   }
   public interface IGameInstance
   {
      bool CtorError { get; }
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      IPlayerTown PlayerTown { set; get; }
      IPlayerAlien PlayerAlien { set; get; }
      //----------------------------------------------
      String[] StartingTownspeople { get; set; }
      List<RandomMoveData> RandomMoves { get; set; }
      Dictionary<IMapItem, IMapItem> AlienTakeovers { get; set; }
      //----------------------------------------------
      IGameCommands GameCommands { set; get; }
      Options Options { get; set; }
      GameStatistics Statistics { get; set; }
      //----------------------------------------------
      IMapItemMoves MapItemMoves { set; get; }
      IStacks Stacks { set; get; }
      IStack? SelectedStack { get; set; }
      List<EnteredHex> EnteredHexes { get; }
      //----------------------------------------------
      bool IsMultipleSelectForDieResult { set; get; } // In EventViewer, show buttons instead of die results for user to choose from
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      IUndo? UndoCmd { set; get; }
      //----------------------------------------------
      Guid GameGuid { get; set; }
      string EventActive { set; get; }
      string EventDisplayed { set; get; }
      //----------------------------------------------
      int Day { get; set; }
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      String EndGameReason { set; get; }
      //----------------------------------------------
      ITerritories ZebulonTerritories { set; get; }
      ITerritories SelectedTerritories { set; get; }
      ITerritory? SelectedTerritory { set; get; }
      IMapItems SelectedMapItems { set; get; }
      IMapItem Zebulon { set; get; }
      IMapItemCombat MapItemCombat { set; get; } 
      IMapItemMove? PreviousMapItemMove { set; get; }
      //----------------------------------------------
      string PlayerTurn { set; get; }
      string NextAction { set; get; }
      int InfluenceCountTotal { set; get; }
      int InfluenceCountTownspeople { set; get; }
      int InfluenceCountAlienUnknown { set; get; }
      int InfluenceCountAlienKnown { set; get; }
      int NumTownGuessesForZebulonLocation { set; get; }
      bool IsAlienStarted { set; get; }
      bool IsTownsStarted { set; get; }
      bool IsAlienDisplayedRandomMovement { set; get; }
      bool IsTownDisplayedRandomMovement { set; get; }
      bool IsAlienAckedRandomMovement { set; get; }
      bool IsTownsAckedRandomMovement { set; get; }
      bool IsAlienInitiatedCombat { set; get; }
      bool IsTownsInitiatedCombat { set; get; }
      bool IsAlienCombatCompleted { set; get; }
      bool IsTownsCombatCompleted { set; get; }
      //----------------------------------------------
      List<IUnitTest> UnitTests { get; }
      //=========================================================
      void AddUnknownAlien(IMapItem newAlien);
      void AddKnownAlien(IMapItem newAlien);
      void AddControlled(IMapItem newPerson);
      IMapItemMove? CreateMapItemMove(IMapItem mi, ITerritory newT);
   }
}
