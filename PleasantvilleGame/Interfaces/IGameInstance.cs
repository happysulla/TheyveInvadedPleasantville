using System;
using System.Collections.Generic;

namespace PleasantvilleGame
{
   public interface IGameInstance
   {
      bool CtorError { get; }
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      IGameCommands GameCommands { set; get; }
      Options Options { get; set; }
      GameStatistics Statistics { get; set; }
      //----------------------------------------------
      IMapItemMoves MapItemMoves { set; get; }
      IStacks Stacks { set; get; }
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
      IMapItems Persons { set; get; }
      IMapItems PersonsStunned { set; get; }
      IMapItems PersonsKnockedOut { set; get; }
      IMapItemCombat? MapItemCombat { set; get; }
      IMapItemTakeover? Takeover { set; get; }
      IMapItemMove? PreviousMapItemMove { set; get; }
      //----------------------------------------------
      string PlayerTurn { set; get; }
      string NextAction { set; get; }
      int InfluenceCountTotal { set; get; }
      int InfluenceCountTownspeople { set; get; }
      int InfluenceCountAlienUnknown { set; get; }
      int InfluenceCountAlienKnown { set; get; }
      int NumIterogationsThisTurn { set; get; }
      bool IsAlienStarted { set; get; }
      bool IsControlledStarted { set; get; }
      bool IsAlienDisplayedRandomMovement { set; get; }
      bool IsControlledDisplayedRandomMovement { set; get; }
      bool IsAlienAckedRandomMovement { set; get; }
      bool IsControlledAckedRandomMovement { set; get; }
      bool IsAlienInitiatedCombat { set; get; }
      bool IsControlledInitiatedCombat { set; get; }
      bool IsAlienCombatCompleted { set; get; }
      bool IsControlledCombatCompleted { set; get; }
      //----------------------------------------------
      List<IUnitTest> UnitTests { get; }
      //=========================================================
      bool AddUnknownAlien(IMapItem newAlien);
      bool AddKnownAlien(IMapItem newAlien);
      bool AddTownperson(IMapItem newPerson);
   }
}
