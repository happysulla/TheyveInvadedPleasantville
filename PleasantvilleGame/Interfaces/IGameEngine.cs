using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace PleasantvilleGame
{
   [Serializable]
   public enum GamePhase
   {
      GameSetup,
      RandomMovement,
      AlienMovement,
      TownspersonMovement,
      Conversations,
      Influences,
      Combat,
      Iterrogations,
      ImplantRemoval,
      AlienTakeover,
      ShowEndGame,
      UnitTest,
      Error
   };
   public enum GameAction
   {
      RemoveSplashScreen,
      UpdateStatusBar,
      UpdateShowRegion,
      UpdateEventViewerDisplay,
      UpdateEventViewerActive,
      DieRollActionNone,          // The field in IGameInstance indicates what the roll apply. If none expected, it is set to this value.

      UpdateView,
      UpdateNewGame,              // Menu Options
      UpdateNewGameEnd,           // finish setting up for new game
      UpdateGameOptions, 
      UpdateLoadingGame,
      UpdateUndo,
      UpdateScatterStack,
      UpdateRotateStack,

      ShowRuleListingDialog,
      ShowEventListingDialog,
      ShowTableListing,
      ShowCharacterDescription,
      ShowGameFeatsDialog,
      ShowReportErrorDialog,
      ShowAboutDialog,

      GameSetupHostGame,
      GameSetupJoinGame,
      GameSetupPlayAlien,
      GameSetupPlayTownsperson,
      GameSetupStartingTownsplayerSetRoll,
      GameSetupStartingTownsplayerSet,
      GameSetupStartingAlienSetRoll,
      GameSetupStartingAlienSet,
      GameSetupShowMap,
      GameSetupRandomMovementSetup,

      RandomMovementStartTowns,           // Initiate EventViewerRandomMovement.Perform_Movement()
      RandomMovementTownsShow,
      RandomMovementStartAlien,           // Initiate EventViewerRandomMovement.Perform_Movement()
      RandomMovementConfirmAlien,
      RandomMovementTownAck,

      AlienMovementTownsShow,
      AlienMovementTownsAck,

      TownMovementTownPerforms,
      TownMovementTownCompletes,

      ConversationsSelect,
      ConversationsRoll,
      ConversationsRollEnd,
      ConversationsFinish,

      InfluencesSelect,
      InfluencesRoll,
      InfluencesRollEnd,
      InfluencesFinish,

      AlienDisplaysRandomMovement,
      TownspersonDisplaysRandomMovement,
      AlienAcksRandomMovement,

      ResetMovement,
      AlienMovement,
      AlienCompletesMovement,

      AlienTimeoutOnMovement,
      AlienModifiesTownspersonMovement,
      TownpersonMovement,
      TownpersonCompletesMovement,
      AlienAcksTownspersonMovement,
      AlienStopsTownspersonMovement,


      AlienInitiateCombat,
      TownspersonNackCombatSelection,
      AlienPerformCombat,
      TownspersonInitiateCombat,
      AlienNackCombatSelection,
      TownspersonPerformCombat,
      TownspersonCompletesCombat,
      AlienCompletesCombat,
      TownspersonIterrogates,
      TownspersonCompletesIterogations,
      AlienAcksIterogations,
      TownspersonRemovesImplant,
      TownspersonCompletesRemoval,
      AlienTakeover,
      AlienCompletesTakeovers,

      ShowAlien,
      ShowEndGame,

      UnitTestStart,
      UnitTestCommand,
      UnitTestNext,
      UnitTestTest,
      UnitTestCleanup,

      EndGameWin,
      EndGameLost,
      EndGameShowFeats,
      EndGameShowStats,
      EndGameClose,
      EndGameExit,
      ExitGame,

      Error
   };
   //================================================================================================
   // GameState is a subclass representing the state pattern. For each game state, there can be different
   // game phases and game actions. The GameEngine makes a call that each class can act on..
   // GameEngine.PerformAction() ==> GameState.PerformAction()
   // GameState.PerformAction() ==> GameState.PerformAction()
   public interface IGameEngine
   {
      List<IView> Views { get; }
      void RegisterForUpdates(IView view);
      void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll = 0);
      bool CreateUnitTests(IGameInstance gi, DockPanel dp, GameViewerWindow gvw, EventViewer ev, IDieRoller dr, CanvasImageViewer civ);
   }
}
