using Google.Protobuf.WellKnownTypes;
using PleasantvilleGame.Networking;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using static PleasantvilleGame.EventViewerRandomMovement;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;

namespace PleasantvilleGame
{
   //-------------------------------------------
   public abstract class GameState : IGameState
   {
      abstract public string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll); // abstract function...GameEngine calls PerformAction() 
      static public IGameState GetGameState(GamePhase phase) // static method that returns the next GameState object based on GamePhase
      {
         switch (phase)
         {
            case GamePhase.GameSetup: return new GameStateSetup();
            case GamePhase.AlienMovement: return new GameStateAlienPlayerMovement();
            case GamePhase.AlienTakeovers: return new GameStateAlienTakeover();
            case GamePhase.Combats: return new GameStateCombat();
            case GamePhase.Conversations: return new GameStateConversations();
            case GamePhase.ImplantRemovals: return new GameStateImplantRemoval();
            case GamePhase.Influences: return new GameStateInfluences();
            case GamePhase.Iterrogations: return new GameStateIterogations();
            case GamePhase.RandomMovement: return new GameStateRandomMovement();
            case GamePhase.ShowEndGame: return new GameStateEnded();
            case GamePhase.TownspersonMovement: return new GameStateTownPlayerMovement();
            case GamePhase.UnitTest: return new GameStateUnitTest();
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetGameState(): reached default phase=" + phase.ToString());
               return new GameStateEnded();
         }
      }
      protected void PrintDiagnosticInfoToLog()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("\n\tGameVersion=");
         Version? version = Assembly.GetExecutingAssembly().GetName().Version;
         if (null != version)
         {
            sb.Append(version.ToString());
            sb.Append("_");
            DateTime linkerTime = Utilities.GetBuildDate(Assembly.GetExecutingAssembly());
            sb.Append(linkerTime.ToString());
         }
         //--------------------------------------------
         Assembly assem = Assembly.GetExecutingAssembly();
         var attributes = assem.CustomAttributes;
         foreach (var attribute in attributes)
         {
            if (attribute.AttributeType == typeof(TargetFrameworkAttribute))
            {
               var arg = attribute.ConstructorArguments.FirstOrDefault();
               sb.Append("\n\tTargetFramework=");
               sb.Append(arg.Value);
               break;
            }
         }
         sb.Append("\n\tOsVersion=");
         sb.Append(Environment.OSVersion.Version.Build.ToString());
         sb.Append("\n\tOS Desc=");
         sb.Append(RuntimeInformation.OSDescription.ToString());
         sb.Append("\n\tOS Arch=");
         sb.Append(RuntimeInformation.OSArchitecture.ToString());
         sb.Append("\n\tProcessorArch=");
         sb.Append(RuntimeInformation.ProcessArchitecture.ToString());
         sb.Append("\n\tnetVersion=");
         sb.Append(Environment.Version.ToString());
         sb.Append("\n\tCultureInfo=");
         sb.Append(CultureInfo.CurrentCulture.ToString());
         //--------------------------------------------
         Screen? screen = Screen.PrimaryScreen;
         if (null != screen)
         {
            var dpi = screen.Bounds.Width / System.Windows.SystemParameters.PrimaryScreenWidth;
            sb.Append("\n\tDPI=(");
            sb.Append(dpi.ToString("000.0"));
         }
         sb.Append(")\n\tAppDir=");
         sb.Append(MainWindow.theAssemblyDirectory);
         Logger.Log(LogEnum.LE_GAME_INIT_VERSION, sb.ToString());
      }
      protected bool ResetDieResults(IGameInstance gi)
      {
         try
         {
            Logger.Log(LogEnum.LE_RESET_ROLL_STATE, "Reset_DieResults(): resetting die rolls gi.DieResults.Count=" + gi.DieResults.Count.ToString());
            if (0 == gi.DieResults.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "Reset_DieResults(): count=0;");
               return false;
            }
            foreach (KeyValuePair<string, int[]> kvp in gi.DieResults)
            {
               for (int i = 0; i < 3; ++i)
                  kvp.Value[i] = Utilities.NO_RESULT;
            }
         }
         catch (Exception)
         {
            Logger.Log(LogEnum.LE_ERROR, "Reset_DieResults(): reset rolls");
            return false;
         }
         return true;
      }
      protected bool ResetPhase(IGameInstance gi, GamePhase phase)
      {
         Logger.Log(LogEnum.LE_SHOW_RESET_PHASE, "Reset_Phase(): ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" + phase.ToString());
         gi.GamePhase = phase;
         gi.DieRollAction = GameAction.DieRollActionNone;
         gi.IsAlienDisplayedRandomMovement = false;
         gi.IsTownDisplayedRandomMovement = false;
         gi.IsAlienAckedRandomMovement = false;
         gi.IsTownsAckedRandomMovement = false;
         gi.NumTownGuessesForZebulonLocation = 0;
         gi.AlienTakeovers.Clear();
         Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "Reset_Phase()");
         gi.MapItemMoves.Clear();
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         if (false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "Reset_Phase(): Reset_DieResults() returned false");
            return false;
         }
         foreach (IStack stack in gi.Stacks)
         {
            foreach(IMapItem mi in stack.MapItems)
            {
               mi.MovementUsed = 0;
               mi.Movement = mi.MovementOriginal;
               mi.IsMoved = false;
               mi.IsMovingThisTurn = false;
               mi.IsConversedThisTurn = false;
               mi.IsInfluencedThisTurn = false;
               mi.IsTakeoverThisTurn = false;
            }
         }
         return true;
      }
      protected bool LoadGame(ref IGameInstance gi)
      {
         //--------------------------------------------
         IGameCommand? cmd = gi.GameCommands.GetLast();
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateViewForNewGame(): cmd=null");
            return false;
         }
         GameAction action = cmd.Action;
         gi.GamePhase = cmd.Phase;
         gi.DieRollAction = cmd.ActionDieRoll;
         Logger.Log(LogEnum.LE_SHOW_UPLOAD_GAME, " Load_Game(): p=" + cmd.Phase.ToString() + " a=" + action.ToString() + " dra=" + cmd.ActionDieRoll.ToString() + " e=" + gi.EventActive);
         return true;
      }
      //------------
      protected bool RotateStack(IGameInstance gi)
      {
         if (null == gi.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(Update_RotateStack): gi.SelectedStack=null");
            return false;
         }
         int count = gi.SelectedStack.MapItems.Count;
         if (count < 2)
            return true;
         IMapItem? bottom = gi.SelectedStack.MapItems[0];
         if (null == bottom)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(Update_RotateStack): bottom = null");
            return false;
         }
         for (int i = 1; i < count; i++)
            gi.SelectedStack.MapItems[i - 1] = gi.SelectedStack.MapItems[i];
         gi.SelectedStack.MapItems[count - 1] = bottom;
         count = 0;
         foreach (IMapItem mi in gi.SelectedStack.MapItems)
         {
            double offset = ((double)count * 3.0) + (mi.Zoom * Utilities.theMapItemOffset);
            mi.Location.X = gi.SelectedStack.Territory.CenterPoint.X - offset;
            mi.Location.Y = gi.SelectedStack.Territory.CenterPoint.Y - offset;
            ++count;
         }
         return true;
      }
      protected bool ScatterStack(IGameInstance gi)
      {
         if (null == gi.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(Update_RotateStack): gi.SelectedStack=null");
            return false;
         }
         if (true == gi.SelectedStack.IsStacked)
         {
            gi.SelectedStack.IsStacked = false;
            foreach (IMapItem mi in gi.SelectedStack.MapItems)
               mi.Location = Territory.GetRandomPoint(gi.SelectedStack.Territory, mi.Zoom * Utilities.theMapItemOffset);
         }
         else
         {
            gi.SelectedStack.IsStacked = true;
            double count = 0;
            foreach (IMapItem mi in gi.SelectedStack.MapItems)
            {
               double offset = (count * 3.0) + (mi.Zoom * Utilities.theMapItemOffset);
               mi.Location.X = gi.SelectedStack.Territory.CenterPoint.X - offset;
               mi.Location.Y = gi.SelectedStack.Territory.CenterPoint.Y - offset;
            }
         }
         return true;
      }
      protected bool CheckForConversations(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         action = GameAction.Error;
         foreach (Stack stack in gi.Stacks)
         {
            IMapItems controlledPeps = new MapItems();
            IMapItems uncontrolledPeps = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (true == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
                  continue;
               if (true == mi.IsControlled)
                  controlledPeps.Add(mi);
               else if(false == mi.IsAlienKnown) 
                  uncontrolledPeps.Add(mi);
            }
            if ((0 < controlledPeps.Count) && (0 < uncontrolledPeps.Count))
            {
               if (GamePhase.Conversations != gi.GamePhase)
               {
                  if (false == ResetPhase(gi, GamePhase.Conversations))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_Conversations(): Reset_Phase() returned error");
                     return false;
                  }
                  gi.EventDisplayed = gi.EventActive = "e009t";
               }
               Logger.Log(LogEnum.LE_SHOW_CONVERSATIONS, "CheckFor_Conversations(): adding stack=" + stack.ToString());
               gi.SelectedTerritories.Add(stack.Territory);
               action = GameAction.ConversationsSelect;
            }
         }
         if (GameAction.ConversationsSelect == action)
            return true;
         //--------------------------------------------------
         if( false == CheckForInfluences(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_Conversations(): CheckFor_Influence() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForInfluences(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         action = GameAction.Error;
         foreach (Stack stack in gi.Stacks)
         {
            IMapItems controlledPeps = new MapItems();
            IMapItems uncontrolledPeps = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsInfluencedThisTurn) || (true == mi.IsKilled) || (true == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
                  controlledPeps.Add(mi);
               else if (false == mi.IsAlienKnown) 
                  uncontrolledPeps.Add(mi);
            }
            if ((0 < controlledPeps.Count) && (0 < uncontrolledPeps.Count))
            {
               if (GamePhase.Influences != gi.GamePhase)
               {
                  if (false == ResetPhase(gi, GamePhase.Influences))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_Influences(): Reset_Phase() returned error");
                     return false;
                  }
                  gi.EventDisplayed = gi.EventActive = "e010t";
               }
               Logger.Log(LogEnum.LE_SHOW_INFLUENCES, "CheckFor_Influences(): adding stack=" + stack.ToString());
               gi.SelectedTerritories.Add(stack.Territory);
               action = GameAction.InfluencesSelect;
            }
         }
         if (GameAction.InfluencesSelect == action)
            return true;
         //--------------------------------------------------
         if (false == CheckForCombats(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_Influences(): Reset_Phase() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForCombats(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         action = GameAction.Error;
         foreach (Stack stack in gi.Stacks)
         {
            IMapItems controlledPeps = new MapItems();
            IMapItems uncontrolledPeps = new MapItems();
            IMapItems knownAliens = new MapItems();
            IMapItems unknownAliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsCombatThisTurn) || (true == mi.IsKilled) || (true == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
                  controlledPeps.Add(mi);
               else if (true == mi.IsAlienKnown) 
                  knownAliens.Add(mi);
               else if (true == mi.IsAlienUnknown)
                  unknownAliens.Add(mi);
               else
                  uncontrolledPeps.Add(mi)
;           }
            if ((0 < controlledPeps.Count) && ( (0 < uncontrolledPeps.Count) || (0 < knownAliens.Count) || (0 < unknownAliens.Count)) )
            {
               if (GamePhase.Combats != gi.GamePhase)
               {
                  if (false == ResetPhase(gi, GamePhase.Combats))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_Combats(): Reset_Phase() returned error");
                     return false;
                  }
                  gi.EventDisplayed = gi.EventActive = "e011t";
               }
               Logger.Log(LogEnum.LE_SHOW_COMBATS, "CheckFor_Combats(): Adding t=" + stack.Territory.ToString() + " c=" + controlledPeps.Count.ToString() + " u=" + uncontrolledPeps.Count.ToString() + " ka=" + knownAliens.Count.ToString() + " ua=" + unknownAliens.Count.ToString());
               gi.SelectedTerritories.Add(stack.Territory); 
               gi.DieRollAction = GameAction.DieRollActionNone;
               action = GameAction.CombatsSelect;
            }
         }
         if (GameAction.CombatsSelect == action)
            return true;
         if (false == CheckForIterogations(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_Combats(): CheckFor_Iterogations() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForIterogations(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         if (false == gi.Zebulon.IsAlienKnown)  // If Zebulon is already on the map board, no need to iterogate
         {
            foreach (Stack stack in gi.Stacks)
            {
               if (stack.MapItems.Count < 2)
                  continue;
               IMapItems controlled = new MapItems();
               IMapItems surrenderedAliens = new MapItems();
               foreach (MapItem mi in stack.MapItems)
               {
                  if ((true == mi.IsInterrogated) || (true == mi.IsKilled) || (true == mi.IsUnconscious) || (true == mi.IsStunned))
                     continue;
                  if (true == mi.IsControlled)
                  {
                     controlled.Add(mi);
                  }
                  else
                  {
                     if ((true == mi.IsAlienKnown) && ((true == mi.IsSurrendered) || (true == mi.IsTiedUp)))
                     {
                        surrenderedAliens.Add(mi);
                     }
                  }
               }
               if (((0 < controlled.Count) && (0 < surrenderedAliens.Count)))
               {
                  if (GamePhase.Iterrogations != gi.GamePhase)
                  {
                     if (false == ResetPhase(gi, GamePhase.Iterrogations))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "CheckFor_Iterogations(): Reset_Phase() returned error");
                        return false;
                     }
                     gi.EventDisplayed = gi.EventActive = "e012t";
                  }
                  Logger.Log(LogEnum.LE_SHOW_ITEROGATIONS, "CheckFor_Iterogations(): adding stack=" + stack.ToString());
                  gi.SelectedTerritories.Add(stack.Territory);
                  action = GameAction.InterrogationsSelect;
               }
            }
            if (GameAction.InterrogationsSelect == action)
               return true;
         }
         if (false == CheckForImplantRemovals(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_Iterogations(): CheckFor_ImplantRemovals() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForImplantRemovals(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            IMapItems controlled = new MapItems();
            IMapItems aliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsImplantRemovalAttempt) || (true == mi.IsImplantRemovalAttemptThisTurn) || (true == mi.IsKilled))
                  continue;
               if ((true == mi.IsControlled) && (false == mi.IsUnconscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  controlled.Add(mi);
               else if ((true == mi.IsAlienKnown) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (true == mi.IsUnconscious)))
                  aliens.Add(mi);
            }
            if ( (0 < controlled.Count) && (0 < aliens.Count) )
            {
               if (GamePhase.ImplantRemovals != gi.GamePhase)
               {
                  if (false == ResetPhase(gi, GamePhase.ImplantRemovals))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_ImplantRemovals(): Reset_Phase() returned error");
                     return false;
                  }
                  gi.EventDisplayed = gi.EventActive = "e013t";
               }
               Logger.Log(LogEnum.LE_SHOW_REMOVALS, "CheckFor_ImplantRemovals(): adding stack=" + stack.ToString());
               gi.SelectedTerritories.Add(stack.Territory);
               action = GameAction.ImplantRemovalsSelect;
            }
         }
         if (GameAction.ImplantRemovalsSelect == action)
            return true;
         if (false == CheckForAlienTakeovers(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_ImplantRemovals(): CheckFor_AlienTakeovers() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForAlienTakeovers(IGameInstance gi, ref GameAction action)
      {
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            IMapItems possibleVictims = new MapItems();
            IMapItems knownAliens = new MapItems();
            IMapItems unknownAliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (true == mi.IsUnconscious))  // Unconscious or dead cannot be taken over
                  continue;
               if ((true == mi.IsControlled) || (true == mi.IsWary))
               {
                  if ((true == mi.IsStunned) || (true == mi.IsTiedUp))
                     possibleVictims.Add(mi);
               }
               else
               {
                  if (true == mi.IsAlienKnown)
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp))
                        knownAliens.Add(mi);
                  }
                  else if (true == mi.IsAlienUnknown)
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp))
                        unknownAliens.Add(mi);
                  }
                  else
                  {
                     possibleVictims.Add(mi);
                  }
               }
            }
            int alienCount = knownAliens.Count + unknownAliens.Count;
            if ( (1 < possibleVictims.Count) || ((0 < unknownAliens.Count) && (0 < knownAliens.Count) ) || ((0 < possibleVictims.Count) && (0 < alienCount)))   // at least two non-town controlled mapitems  - cannnot be all known aliens
            {
               if (GamePhase.AlienTakeovers != gi.GamePhase)
               {
                  if (false == ResetPhase(gi, GamePhase.AlienTakeovers))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): Reset_Phase() returned error");
                     return false;
                  }
                  gi.EventDisplayed = gi.EventActive = "e014t";
               }
               Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "CheckFor_AlienTakeovers(): t=" + stack.Territory.ToString() + " v=" + possibleVictims.ToString() + " ua=" + unknownAliens.ToString() + " ka=" + knownAliens.ToString());
               if ( false == gi.PlayerAlien.ShowPossibleTakeover(gi, stack, ref action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): Perform_AlienTakeover() returned error");
                  return false;
               }
            }
         }
         if ((GameAction.AlienTakeoversSelect == action) || (GameAction.AlienTakeoversShow == action))
            return true;
         if (false == CheckForEndOfGame(gi, ref action))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): CheckFor_EndOfGame() returned error");
            return false;
         }
         return true;
      }
      protected bool CheckForEndOfGame(IGameInstance gi, ref GameAction action)
      {
         StringBuilder sb;
         gi.SelectedMapItems.Clear();
         gi.SelectedTerritories.Clear();
         gi.NumTownGuessesForZebulonLocation = 0;
         //--------------------------------------------------------
         foreach (Stack stack in gi.Stacks) //  Tied Up MapItems - Tied up players are freed if a friendly counter is in the same hex at the end of the turn.
         {
            IMapItems alienTiedUpPersons = new MapItems();
            IMapItems controlledTiedUpPersons = new MapItems();
            bool isFriendlyAlienHelping = false;
            bool isFriendlyControlledHelping = false;
            //--------------------------------------------------------
            foreach (MapItem mi in stack.MapItems)
            {
               mi.IsMoveStoppedThisTurn = false;
               mi.IsMoveAllowedToResetThisTurn = true;
               mi.IsConversedThisTurn = false;
               mi.IsInfluencedThisTurn = false;
               mi.IsCombatThisTurn = false;
               mi.IsImplantRemovalAttemptThisTurn = false;
               mi.IsTakeoverThisTurn = false;
               if ((true == mi.IsSurrendered) || (true == mi.IsKilled))
                  continue;
               if (true == mi.IsTiedUp) // Cound be stunned or unconscious
               {
                  if (true == mi.IsAlienKnown)
                     alienTiedUpPersons.Add(mi);
                  else if (true == mi.IsControlled)
                     controlledTiedUpPersons.Add(mi);
               }
               if ((false == mi.IsTiedUp) && (false == mi.IsUnconscious) && (false == mi.IsStunned))
               {
                  if (true == mi.IsAlienKnown)
                     isFriendlyAlienHelping = true;
                  else if (true == mi.IsControlled)
                     isFriendlyControlledHelping = true;
               }
               if( true == mi.IsStunned)// Unstunned - For each person who was stunned returns to the game
               {
                  mi.IsStunned = false;
                  gi.InfluenceCountTotal += mi.Influence;
                  sb = new StringBuilder("CheckFor_EndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to TOTAL "); sb.Append(mi.Influence.ToString());
                  sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

                  if (true == mi.IsAlienUnknown)
                  {
                     gi.InfluenceCountAlienUnknown += mi.Influence;
                     sb = new StringBuilder("CheckFor_EndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to unknown "); sb.Append(mi.Influence.ToString());
                     sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
                  else if (true == mi.IsAlienKnown)
                  {
                     gi.InfluenceCountAlienKnown += mi.Influence;
                     sb = new StringBuilder("CheckFor_EndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to known "); sb.Append(mi.Influence.ToString());
                     sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
                  else if (true == mi.IsControlled)
                  {
                     gi.InfluenceCountTownspeople += mi.Influence;
                     sb = new StringBuilder("CheckFor_EndOfGame() : unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to TP "); sb.Append(mi.Influence.ToString());
                     sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
               }
               if( true == mi.IsUnconscious )
               {
                  mi.IsUnconscious = false;
                  mi.IsStunned = true;
               }
            }
            //--------------------------------------------------------
            if (true == isFriendlyAlienHelping)
            {
               foreach (IMapItem alien in alienTiedUpPersons) // known aliens tied up
               {
                  alien.IsTiedUp = false;
                  if ((false == alien.IsUnconscious) && (false == alien.IsStunned))
                  {
                     gi.InfluenceCountTotal += alien.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(alien.Name); sb.Append(" ++++ to Total "); sb.Append(alien.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

                     gi.InfluenceCountAlienKnown += alien.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(alien.Name); sb.Append(" ++++ to known "); sb.Append(alien.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
               }
            }
            //--------------------------------------------------------
            if (true == isFriendlyControlledHelping)
            {
               foreach (IMapItem controlled in controlledTiedUpPersons)
               {
                  controlled.IsTiedUp = false;
                  if ((true == controlled.IsUnconscious) && (false == controlled.IsStunned))
                  {
                     gi.InfluenceCountTotal += controlled.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(controlled.Name); sb.Append(" ++++ to Total "); sb.Append(controlled.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                     gi.InfluenceCountTownspeople += controlled.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie"); sb.Append(controlled.Name); sb.Append(" ++++ to TP "); sb.Append(controlled.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
               }
            }
         }
         //-----------------------------------------------------------
         if (false == CheckForInfluenceError(gi)) // check for any errors
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_EndOfGame(): CheckFor_InfluenceError() returned error");
            return false;
         }
         //-----------------------------------------------------------
         if (true == gi.Zebulon.IsKilled)
         {
            gi.EndGameReason = "Zebulon is defeated";
            gi.GamePhase = GamePhase.ShowEndGame;
            action = GameAction.EndGame;
            gi.EventDisplayed = gi.EventActive = "e502";
         }
         if (((gi.InfluenceCountAlienUnknown <= 0) && (gi.InfluenceCountAlienKnown <= 0)) || (gi.InfluenceCountTownspeople <= 0))  // If either the Alien or Townscontrolled influcence reaches zero, game over
         {
            gi.EndGameReason = "Zebulon is defeated";
            gi.GamePhase = GamePhase.ShowEndGame;
            action = GameAction.EndGame;
            gi.EventDisplayed = gi.EventActive = "e502";
         }
         gi.GameTurn++;
         if (12 < gi.GameTurn) // Determine turn number.  If reach 12, game is over.
         {
            gi.EndGameReason = "Game ends on turns";
            gi.GamePhase = GamePhase.ShowEndGame;
            action = GameAction.EndGame;
            gi.EventDisplayed = gi.EventActive = "e502";
         }
         //-----------------------------------------------------------
         if (false == ResetPhase(gi, GamePhase.RandomMovement))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): Reset_Phase() returned error");
            return false;
         }
         action = GameAction.RandomMovementStartTowns;
         gi.EventActive = gi.EventDisplayed = "e005";
         return true;
      }
      protected bool CheckForInfluenceError(IGameInstance gi)
      {
         int totalInfluence = 0;
         int cogentInfluence = 0;
         int knownInfluence = 0;
         int unknownInfluence = 0;
         int controlledInfluence = 0;
         int uncontrolledInfluence = 0;
         int incapacitatedInfluence = 0;
         int tiedUpInfluence = 0;
         int stunnedInfluence = 0;
         int unconsciousInfluence = 0;
         int surrenderedInfluence = 0;
         int killedInfluence = 0;
         int errorInfluence = 0;
         foreach(IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               totalInfluence += mi.Influence;
               if ((false == mi.IsTiedUp) && (false == mi.IsUnconscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
               {
                  cogentInfluence += mi.Influence;
                  if (true == mi.IsControlled)
                     controlledInfluence += mi.Influence;
                  if (true == mi.IsAlienKnown)
                     knownInfluence += mi.Influence;
                  if (true == mi.IsAlienUnknown)
                     unknownInfluence += mi.Influence;
                  if ((false == mi.IsControlled) && (false == mi.IsAlienKnown) && (false == mi.IsAlienUnknown))
                     uncontrolledInfluence += mi.Influence;
               }
               else
               {
                  incapacitatedInfluence += mi.Influence;
                  if (true == mi.IsKilled)
                     killedInfluence += mi.Influence;
                  else if (true == mi.IsSurrendered)
                     surrenderedInfluence += mi.Influence;
                  else if (true == mi.IsUnconscious)
                     unconsciousInfluence += mi.Influence;
                  else if (true == mi.IsStunned)
                     stunnedInfluence += mi.Influence;
                  else if (true == mi.IsTiedUp)
                     tiedUpInfluence += mi.Influence;
                  else
                     errorInfluence += mi.Influence;
               }
            }
         }
         //--------------------------------------------------------
         if (337 != totalInfluence)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_InfluenceError(): 337 != (total=" + totalInfluence.ToString() + ")");
            return false;
         }
         if (totalInfluence != (cogentInfluence + incapacitatedInfluence))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_InfluenceError(): (t=" + totalInfluence.ToString() + ") != (cog=" + cogentInfluence.ToString() + " ) + " + "(inc=" + incapacitatedInfluence.ToString()+")");
            return false;
         }
         if ( (cogentInfluence != (controlledInfluence + knownInfluence + unknownInfluence + uncontrolledInfluence)))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_InfluenceError(): (cog=" + cogentInfluence.ToString() + ") != (c=" + controlledInfluence.ToString() + ") + " + "(k=" + knownInfluence.ToString() + ") " + "(uk=" + unknownInfluence.ToString() + ") " + "(uc=" + uncontrolledInfluence.ToString() + ")");
            return false;
         }
         if (0 != errorInfluence)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckFor_InfluenceError(): 0 != (e=" + errorInfluence.ToString() + ")");
            return false;
         }
         return true;
      }
   }
   //----------------------------------------------------------------
   class GameStateSetup : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi))
               {
                  returnStatus = "Load_Game() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateRotateStack:
               if( false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RemoveSplashScreen: // GameStateSetup.PerformAction()
               if (false == SetupNewGame(gi, ref action))
               {
                  returnStatus = "SetupNewGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.GameSetupHostGame:
               HostGameDialog hostDialog = new HostGameDialog();
               if (MainWindow.theGameViewerWindow is null)
               {
                  returnStatus = "MainWindow.theGameViewerWindow=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupJoinGame): " + returnStatus);
               }
               else
               {
                  if (true == hostDialog.ShowDialog())
                  {
                     if (null == GameEngine.theMultiplayerSessionManager)
                     {
                        returnStatus = "Multiplayer session manager is not available";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        GameEngine.theGameType = GameType.MultiPlayerHost;
                          HostSessionResultDataTranferObject hostResult = GameEngine.theMultiplayerSessionManager.StartHosting(gi, hostDialog.SessionName, hostDialog.Port);
                        if (false == hostResult.IsSuccess || null == hostResult.Session)
                        {
                           returnStatus = "Unable to host multiplayer game: " + hostResult.ErrorMessage;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                           MessageBox.Show("Unable to host multiplayer game.\n\n" + hostResult.ErrorMessage, "Host Game");
                        }
                        else
                        {
                           gi.EventActive = gi.EventDisplayed = "e002";
                           gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
                           StringBuilder hostMessage = new StringBuilder();
                           hostMessage.Append("Hosting started.\n\n");
                           hostMessage.Append("Session Id: ");
                           hostMessage.Append(hostResult.Session.SessionId);
                           hostMessage.Append("\nJoin Code: ");
                           hostMessage.Append(hostResult.Session.JoinCode);
                           hostMessage.Append("\nAddress: ");
                           hostMessage.Append(hostResult.Session.HostAddress);
                           hostMessage.Append(":");
                           hostMessage.Append(hostResult.Session.HostPort.ToString());
                           hostMessage.Append("\n\nThis first scaffold hosts the Alien side locally and exposes a gRPC endpoint for the Town player.");
                           MessageBox.Show(hostMessage.ToString(), "Host Game");
                        }
                     }
                  }
               }
               break;
            case GameAction.GameSetupJoinGame:
               JoinGameDialog joinDialog = new JoinGameDialog();
               if (MainWindow.theGameViewerWindow is null)
               {
                  returnStatus = "MainWindow.theGameViewerWindow=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupJoinGame): " + returnStatus);
               }
               else
               {
                  GameEngine.theGameType = GameType.MultiPlayerJoin;
                  joinDialog.Owner = MainWindow.theGameViewerWindow;
                  if (true == joinDialog.ShowDialog())
                  {
                     if (null == GameEngine.theMultiplayerSessionManager)
                     {
                        returnStatus = "Multiplayer session manager is not available";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        JoinSessionResultDataTranferObject joinResult = GameEngine.theMultiplayerSessionManager.JoinSession(joinDialog.ServerAddress, joinDialog.SessionId, joinDialog.JoinCode);
                        if (false == joinResult.IsSuccess || null == joinResult.Session || null == joinResult.State)
                        {
                           returnStatus = "Unable to join multiplayer game: " + joinResult.ErrorMessage;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           if (false == MultiplayerStateApplier.ApplyVisibleState(gi, joinResult.State, MultiplayerRole.Town))
                           {
                              returnStatus = "Failed to apply the visible host state";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                           }
                           else
                           {
                              StringBuilder joinMessage = new StringBuilder();
                              joinMessage.Append("Joined multiplayer session as the Town side.\n\n");
                              joinMessage.Append("Session: ");
                              joinMessage.Append(joinResult.Session.SessionName);
                              joinMessage.Append("\nHost: ");
                              joinMessage.Append(joinResult.Session.HostAddress);
                              joinMessage.Append(":");
                              joinMessage.Append(joinResult.Session.HostPort.ToString());
                              joinMessage.Append("\n\nThis first scaffold loads the host's visible state into the client. Full live turn replication is the next step.");
                              MessageBox.Show(joinMessage.ToString(), "Join Game");
                           }
                        }
                     }
                  }
               }
               break;
            case GameAction.GameSetupPlayAlien:
               GameEngine.theGameType = GameType.SinglePlayerAlien;
               gi.PlayerAlien = new PlayerAlienHuman();
               gi.PlayerTown = new PlayerTownComputer();
               gi.EventActive = gi.EventDisplayed = "e002";
               gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
               break;
            case GameAction.GameSetupPlayTownsperson:
               GameEngine.theGameType = GameType.SinglePlayerTown;
               gi.PlayerAlien = new PlayerAlienComputer();
               gi.PlayerTown = new PlayerTownHuman();
               gi.EventActive = gi.EventDisplayed = "e002";
               gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
               string startingHq = StartingHqMgr.GetStartingHqTerritory();
               ITerritory? tZebutonStart = Territories.theTerritories.Find(startingHq);
               if( null  == tZebutonStart)
               {
                  returnStatus = "zTerritory=null for " + startingHq;
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupPlayTownsperson): " + returnStatus);
               }
               else
               {
                  gi.Zebulon.TerritoryCurrent = gi.Zebulon.TerritoryStarting = tZebutonStart;
               }
               break;
            case GameAction.GameSetupStartingTownsplayerSetRoll:
               if( Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if( false == gi.PlayerTown.GetStartingTownCounter(gi, dieRoll))
                  {
                     returnStatus = "Get_StartingTownsperson() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupStartingTownsplayerSetRoll): " + returnStatus);
                  }
               }
               else
               {
                  if( false == gi.PlayerAlien.GetNextState(gi, ref action))
                  {
                     returnStatus = "Get_NextState() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupStartingTownsplayerSetRoll): " + returnStatus);
                  }
               }
               break;
            case GameAction.GameSetupStartingAlienSetRoll:
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
               }
               else if (Utilities.NO_RESULT == gi.DieResults[key][1])
               {
                  if (gi.DieResults[key][0] < 5)
                     gi.DieResults[key][0] = Utilities.NO_RESULT;
                  else
                     gi.DieResults[key][1] = dieRoll;
               }
               else
               {
                  if( false == gi.PlayerAlien.GetStartingAlienCounters(gi))
                  {
                     returnStatus = "Get_StartingAlien() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupStartingAlienSetRoll): " + returnStatus);
                  }
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
                  gi.DieResults[key][1] = Utilities.NO_RESULT;
               }
               break;
            case GameAction.GameSetupShowMap:
               gi.EventActive = gi.EventDisplayed = "e004";
               if ( false == TableMgr.CreateTownspeople(gi))
               {
                  returnStatus = "Create_Townspeople() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else if( false == AssignStartingTownsplayer(gi))
               {
                  returnStatus = "Assign_StartingTownsplayer() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else if (false == AssignStartingAlien(gi))
               {
                  returnStatus = "Assign_StartingAlien() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.GameSetupRandomMovementSetup:
               gi.GameTurn++;
               if (false == ResetPhase(gi, GamePhase.RandomMovement))
               {
                  returnStatus = "Reset_Phase() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               gi.EventActive = gi.EventDisplayed = "e005";
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSetup.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      private bool SetupNewGame(IGameInstance gi, ref GameAction outAction)
      {
         PrintDiagnosticInfoToLog();
         gi.GamePhase = GamePhase.GameSetup;
         gi.Statistics = new GameStatistics();
         gi.Statistics.SetOriginalGameStatistics();
         //-------------------------------------------------------
         gi.DieRollAction = GameAction.DieRollActionNone;
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "Setup_NewGame(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         //---------------------------------------------
         if (false == AddStartingTestingState(gi)) // TestingStartAmbush
         {
            Logger.Log(LogEnum.LE_ERROR, "Setup_NewGame():  Add_StartingTestingState() returned false");
            return false;
         }
         return true;
      }
      private bool AddStartingTestingState(IGameInstance gi)
      {
         return true;
      }
      private bool AssignStartingTownsplayer(IGameInstance gi)
      {
         string name = gi.StartingTownspeople[0];
         if (true == String.IsNullOrEmpty(name))
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingTownsplayer():  gi.PlayerTown.StartingTownspeople[0] is empty");
            return false;
         }
         IMapItem? startingTownsplayer = gi.Stacks.FindMapItem(name);
         if( null == startingTownsplayer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingTownsplayer(): startingTownsplayer=null for name=" + name);
            return false;
         }
         startingTownsplayer.IsControlled = true;
         return true;
      }
      private bool AssignStartingAlien(IGameInstance gi)
      {
         string name = gi.StartingTownspeople[1];
         if (true == String.IsNullOrEmpty(name))
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingTownsplayer():  gi.PlayerAlien.StartingTownspeople[0] is empty");
            return false;
         }
         IMapItem? startingAlien = gi.Stacks.FindMapItem(name);
         if (null == startingAlien)
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingAlien(): startingAlien=null for name=" + name);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Assign_StartingAlien(): AddUnknownAlien() startingAlien=" + startingAlien.ToString());
         gi.AddUnknownAlien(startingAlien);
         //------------------------------------
         name = gi.StartingTownspeople[2];
         if (true == String.IsNullOrEmpty(name))
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingTownsplayer():  gi.PlayerAlien.StartingTownspeople[1] is empty");
            return false;
         }
         startingAlien = gi.Stacks.FindMapItem(name);
         if (null == startingAlien)
         {
            Logger.Log(LogEnum.LE_ERROR, "Assign_StartingAlien(): startingAlien=null for name=" + name);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Assign_StartingAlien(): AddUnknownAlien() startingAlien=" + startingAlien.ToString() );
         gi.AddUnknownAlien(startingAlien);
         return true;
      }
   }
   //----------------------------------------------------------------
   class GameStateRandomMovement : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi))
               {
                  returnStatus = "Load_Game() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RandomMovementStartTowns:
               if (false == ChooseRandomMovePeopleAndDest(gi))
               {
                  returnStatus = "Create_RandomMoves() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RandomMovementTownsShow:
               if( false == gi.PlayerAlien.BlockRandomMoves(gi))
               {
                  returnStatus = "Create_RandomMoves() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
               }
               else if (false == PerformRandomMoves(gi))
               {
                  returnStatus = "PerformRandomMoves() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(RandomMovementConfirmed): " + returnStatus);
               }
               gi.EventActive = gi.EventDisplayed = "e005t";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.RandomMovementTownAck:
               gi.IsTownsAckedRandomMovement = true;
               if (true == gi.IsAlienAckedRandomMovement)
               {
                  if( false == ResetPhase(gi, GamePhase.AlienMovement))
                  {
                     returnStatus = "Reset_Phase() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(RandomMovementConfirmed): " + returnStatus);
                  }
                  gi.EventActive = gi.EventDisplayed = "e006t";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == gi.PlayerAlien.PerformAlienMoves(gi))
                  {
                     returnStatus = "Perform_AlienMoves() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
                  }
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateRandomMovement.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      public bool ChooseRandomMovePeopleAndDest(IGameInstance gi)
      {
         gi.RandomMoves.Clear();
         const int numPeopleToMove = 4;
         int numPeopleMoved = 0;
         int loopCount = 200;
         while ((numPeopleMoved < numPeopleToMove) && (0 < loopCount--))
         {
            int die1 = Utilities.RandomGenerator.Next(5);
            int die2 = Utilities.RandomGenerator.Next(6);
            string name = TableMgr.GetTownspersonName(die1, die2);
            if ("ERROR" == name)
            {
               Logger.Log(LogEnum.LE_ERROR, "Choose_RandomMovePeopleAndDest(): TableMgr.GetTownspersonName() returned ERROR");
               return false;
            }
            //------------------------------------------------------------
            int die3 = Utilities.RandomGenerator.Next(5);
            int die4 = Utilities.RandomGenerator.Next(6);
            string fullBuildingName = TableMgr.GetTargetBuildingName(die3, die4); // Find the target building location.
            if ("ERROR" == fullBuildingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "Choose_RandomMovePeopleAndDest(): GetTargetBuildingName() returned ERROR for d3=" + die3.ToString() + " d4=" + die4.ToString());
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): moving " + name + " to " + fullBuildingName + " d1=" + die1.ToString() + " d2=" + die2.ToString() + " d3=" + die3.ToString() + " d4=" + die4.ToString());
            //------------------------------------------------------------
            bool isDuplicate = false;
            RandomMoveData randomMove = new RandomMoveData(name, fullBuildingName);
            foreach (RandomMoveData rmd in gi.RandomMoves)
            {
               if(rmd.myName == name) 
               {
                  Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): skipping name=" + name + " to building=" + fullBuildingName + " because it is in the RandomMovesData list");
                  isDuplicate = true;
                  break;
               }
            }
            if( false == isDuplicate)
            {
               gi.RandomMoves.Add(randomMove);
               numPeopleMoved++;
               Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): adding moving " + name + " to " + fullBuildingName);
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): skipping " + name + " to " + fullBuildingName + " since already moving");
            }
         }
         if (loopCount < 0)
         {
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): invalid state loopCount=" + loopCount.ToString());
            return false;
         }
         return true;
      }
      public bool PerformRandomMoves(IGameInstance gi)
      {
         for (int i = 0; i < gi.RandomMoves.Count; i++)
         {
            RandomMoveData rmd = gi.RandomMoves[i];
            IMapItem? mi = gi.Stacks.FindMapItem(rmd.myName);
            if (mi == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Choose_RandomMovePeopleAndDest(): mi=null for " + rmd.myName);
               return false;
            }
            if ((true == mi.IsTiedUp) || (true == mi.IsUnconscious) || (true == mi.IsKilled))
               continue;
            string buildingName = rmd.myBuildingName;
            mi.Movement *= 2; // Movement is doubled during Random Movement
            ITerritory? newTerritory = Territories.theTerritories.Find(buildingName);
            if (null == newTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): unable to find buildingName=" + buildingName);
               return false;
            }
            //-----------------------------------------
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Perform_RandomMoves(): mi=" + mi.Name + " entering t=" + newTerritory.ToString());
            IMapItemMove? mim = gi.CreateMapItemMove(mi, newTerritory);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): Create_MapItemMove() returned null");
               return false;
            }
            gi.MapItemMoves.Add(mim);
         }
         return true;
      }
   }
   //----------------------------------------------------------------
   class GameStateAlienPlayerMovement : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.AlienMovementTownsShow:
               gi.EventDisplayed = gi.EventActive = "e007t";
               break;
            case GameAction.AlienMovementTownsAck:
               if (false == ResetPhase(gi, GamePhase.TownspersonMovement))
               {
                  returnStatus = "Reset_Phase() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(RandomMovementConfirmed): " + returnStatus);
               }
               gi.EventDisplayed = gi.EventActive = "e008t";
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateAlienPlayerMovement.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateAlienPlayerMovement.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateTownPlayerMovement : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.TownMovementTownPerforms:
               Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "GameStateTownPlayerMovement.PerformAction(TownMovementTownPerforms)");
               gi.MapItemMoves.Clear();
               if (false == gi.PlayerTown.PerformTownMove(gi, ref action))
               {
                  returnStatus = "Perform_TownMove() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(TownMovementTownPerforms): " + returnStatus);
               }
               break;
            case GameAction.TownMovementTownCompletes:
               if (false == CheckForConversations(gi, ref action))
               {
                  returnStatus = "CheckFor_Conversations() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateTownPlayerMovement.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateConversations : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               if (false == CheckForConversations(gi, ref action))
               {
                  returnStatus = "CheckFor_Conversations() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(ConversationsRoll): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               if (false == CheckForConversations(gi, ref action))
               {
                  returnStatus = "CheckFor_Conversations() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(ConversationsRoll): " + returnStatus);
               }
               break;
            case GameAction.ConversationsRoll:
               if(2 != gi.SelectedMapItems.Count)
               {
                  returnStatus = " 2 != (gi.Conversations.Count=" + gi.SelectedMapItems.Count + ")";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(ConversationsRoll): " + returnStatus);
               }
               else
               {
                  IMapItem? leftMapItem = gi.SelectedMapItems[0];
                  IMapItem? rightMapItem = gi.SelectedMapItems[1];
                  if( null == leftMapItem || null == rightMapItem )
                  {
                     returnStatus = " leftMapItem or rightMapItem = null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(ConversationsRoll): " + returnStatus);
                  }
                  else
                  {
                     leftMapItem.IsConversedThisTurn = true;
                     int dieRollModifier = 0;
                     if (15 < rightMapItem.Influence)
                        dieRollModifier = 3;
                     else if (10 < rightMapItem.Influence)
                        dieRollModifier = 2;
                     else if (5 < rightMapItem.Influence)
                        dieRollModifier = 1;
                     if (8 < dieRoll + dieRollModifier)
                     {
                        rightMapItem.IsConversedThisTurn = true; // if successful, no need to talk to in same turn
                        if (true == rightMapItem.IsAlienUnknown)
                        {
                           Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "GameStateInfluences.PerformAction(): AddKnownAlien() rightMapItem=" + rightMapItem.ToString() + " dr=" + dieRoll.ToString() + " drm=" + dieRollModifier.ToString());
                           gi.AddKnownAlien(rightMapItem);    // GameStateConversations.PerformAction(ConversationsRoll)
                        }
                     }
                     if (false == CheckForConversations(gi, ref action))
                     {
                        returnStatus = "CheckFor_Conversations() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(ConversationsRoll): " + returnStatus);
                     }
                  }
               }
               break;
            case GameAction.ConversationsFinish:
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateConversations.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateInfluences : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.InfluencesRoll:
               if (gi.SelectedMapItems.Count < 2)
               {
                  returnStatus = " 2 > (gi.SelectedMapItems.Count=" + gi.SelectedMapItems.Count + ")";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(InfluencesRoll): " + returnStatus);
               }
               else
               {
                  int indexOfLast = gi.SelectedMapItems.Count - 1;
                  IMapItem? rightMapItem = gi.SelectedMapItems[indexOfLast];
                  if( null == rightMapItem )
                  {
                     returnStatus = "rightMapItem = null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(InfluencesRoll): " + returnStatus);
                  }
                  else
                  {
                     rightMapItem.IsInfluencedThisTurn = true; // only allow one influence per turn
                     double totalInfluence = 0;
                     bool isImplantHeld = false;
                     for (int i = 0; i < indexOfLast; ++i)
                     {
                        IMapItem? influencer = gi.SelectedMapItems[i];
                        if (null == influencer)
                        {
                           returnStatus = "influencer=null for i=" + i.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(InfluencesRoll): " + returnStatus);
                        }
                        else
                        {
                           influencer.IsInfluencedThisTurn = true; // only allow one influence per turn
                           totalInfluence += (double)influencer.Influence;
                           if (true == influencer.IsImplantHeld)
                              isImplantHeld = true;
                        }
                     }
                     double odds = totalInfluence / ((double)rightMapItem.Influence);
                     int dieThreshold;
                     if (3.999 < odds)
                        dieThreshold = 3;
                     else if (2.999 < odds)
                        dieThreshold = 4;
                     else if (1.999 < odds)
                        dieThreshold = 5;
                     else if (1.499 < odds)
                        dieThreshold = 6;
                     else if (0.999 < odds)
                        dieThreshold = 7;
                     else if (0.665 < odds)
                        dieThreshold = 8;
                     else if (0.499 < odds)
                        dieThreshold = 9;
                     else
                        dieThreshold = 10;
                     int dieRollModifier = 0;
                     if (true == isImplantHeld) // Subtact one if a controlled person holds evidence of an implant.
                        --dieRollModifier;
                     if (true == rightMapItem.IsSkeptical) 
                        ++dieRollModifier;
                     if (true == rightMapItem.IsWary)  
                        --dieRollModifier;
                     int dieRollWithMod = dieRoll + dieRollModifier;
                     Logger.Log(LogEnum.LE_SHOW_INFLUENCES, "GameStateInfluences.PerformAction(): odds=" + odds.ToString("F1") + " r=" + rightMapItem.ToString() + " (dr=" + dieRoll.ToString() + ") + (m=" + dieRollModifier.ToString() + ") ??? (t=" + dieThreshold.ToString() + ")"); 
                     if (dieThreshold <= dieRollWithMod) // Check for alien.  If alien, let user know it is discovered. Else, make the townsperson controlled.
                     {
                        if (true == rightMapItem.IsAlienUnknown)
                        {
                           Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "GameStateInfluences.PerformAction(): AddKnownAlien() rightMapItem=" + rightMapItem.ToString() + " (dr=" + dieRoll.ToString() + ") + (m=" + dieRollModifier.ToString()  + ") >= (t=" + dieThreshold.ToString() + ")");
                           gi.AddKnownAlien(rightMapItem); // GameStateInfluences.PerformAction(InfluencesRoll)
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_SHOW_TOWNS_ADD, "GameStateInfluences.PerformAction(): NO EFFECT rightMapItem=" + rightMapItem.ToString() + " (dr=" + dieRoll.ToString() + ") + (m=" + dieRollModifier.ToString() + ") >= (t=" + dieThreshold.ToString() + ")");
                           gi.AddControlled(rightMapItem); // GameStateInfluences.PerformAction(InfluencesRoll)
                        }
                     }
                     else
                     {
                        if (false == rightMapItem.IsWary)  // wary people cannot become skeptical
                        {
                           Logger.Log(LogEnum.LE_SHOW_SKEPTICAL_ADD, "GameStateInfluences.PerformAction(): Add Skeptical rightMapItem=" + rightMapItem.ToString() + " (dr=" + dieRoll.ToString() + ") + (m=" + dieRollModifier.ToString() + ") <>=> (t=" + dieThreshold.ToString() + ")"); 
                           rightMapItem.IsSkeptical = true;
                        }
                     }
                     if ("OK" == returnStatus)
                     {
                        if (false == CheckForInfluences(gi, ref action))
                        {
                           returnStatus = "CheckFor_Influences() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(): " + returnStatus);
                        }
                     }
                  }
               }
               break;
            case GameAction.InfluencesFinish:
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateInfluences.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateCombat : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
            case GameAction.CombatsSelect: // handled in the GameViewWindow.xaml.cs file
               if (false == CheckForCombats(gi, ref action))
               {
                  returnStatus = "Check_ForCombats() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsSelect): " + returnStatus);
               }
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.CombatsRoll:
               Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "GameStateCombat.PerformAction(CombatsRoll)");
               gi.MapItemMoves.Clear();
               if (null == gi.MapItemCombat)
               {
                  returnStatus = "MapItemCombat=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsRoll): " + returnStatus);
               }
               else
               {
                  foreach (IMapItem attacker in gi.MapItemCombat.Attackers)
                     attacker.IsCombatThisTurn = true;
                  foreach (IMapItem defender in gi.MapItemCombat.Defenders)
                     defender.IsCombatThisTurn = true;
                  IMapItem? firstAttacker = gi.MapItemCombat.Attackers[0];
                  if( null == firstAttacker )
                  {
                     returnStatus = "firstAttacker=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsRoll): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_COMBATS, "GameStateCombat.PerformAction(CombatsRoll): Combat=" + gi.MapItemCombat.ToString() + " action=" + action.ToString() + " dr=" + dieRoll.ToString() + " 1stA=" + firstAttacker.Name + " in " + firstAttacker.TerritoryCurrent.ToString());
                     switch (gi.MapItemCombat.Result)
                     {
                        case CombatResult.DefenderWins:
                           action = GameAction.CombatDefenderWin;
                           break;
                        case CombatResult.AttackerWins:
                           action = GameAction.CombatAttackerWin;
                           break;
                        case CombatResult.AttackerFlees:
                           if (true == firstAttacker.IsControlled)
                           {
                              gi.EventActive = gi.EventDisplayed = "e011tf";
                              action = GameAction.CombatTownFlee;
                           }
                           else
                           {
                              gi.EventActive = gi.EventDisplayed = "e011af";
                              action = GameAction.CombatAlienFlee;
                           }
                           if (false == CreateMapItemFlees(gi, gi.MapItemCombat.Defenders))
                           {
                              returnStatus = "CreateMapItemFlee() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsRoll): " + returnStatus);
                           }
                           break;
                        case CombatResult.DefenderFlees:
                           if (true == firstAttacker.IsControlled)
                           {
                              gi.EventActive = gi.EventDisplayed = "e011tf";
                              action = GameAction.CombatTownFlee;
                           }
                           else
                           {
                              gi.EventActive = gi.EventDisplayed = "e011af";
                              action = GameAction.CombatAlienFlee;
                           }
                           action = GameAction.CombatTownFlee;
                           if (false == CreateMapItemFlees(gi, gi.MapItemCombat.Defenders))
                           {
                              returnStatus = "CreateMapItemFlee() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsRoll): " + returnStatus);
                           }
                           break;                   
                        default:
                           returnStatus = "invalid CombatResult=" + gi.MapItemCombat.Result.ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsRoll): " + returnStatus);
                           break;
                     }
                  }
               }
               break;
            case GameAction.CombatShowFleeMove:
               if (false == CheckForCombats(gi, ref action))
               {
                  returnStatus = "Check_ForCombats() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.CombatsFinish:
               Logger.Log(LogEnum.LE_SHOW_COMBATS, "GameStateCombat.PerformAction(CombatsFinish): Combat Finished");
               if (false == CheckForIterogations(gi, ref action))
               {
                  returnStatus = "CheckFor_Iterogations() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsFinish): " + returnStatus);
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateCombat.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      public bool CreateMapItemFlees(IGameInstance gi, IMapItems mapItems)
      {
         foreach (IMapItem mi in mapItems)
         {
            int die1 = Utilities.RandomGenerator.Next(5);
            int die2 = Utilities.RandomGenerator.Next(6);
            string name = TableMgr.GetTownspersonName(die1, die2);
            if ("ERROR" == name)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): TableMgr.GetTownspersonName() returned ERROR");
               return false;
            }
            //------------------------------------------------------------
            string? buildingName = mi.TerritoryCurrent.ToString();
            if( null == buildingName )
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): TableMgr.GetTownspersonName() buildingName=null");
               return false;
            }
            string fullBuildingName = "";
            int count = 1000;
            while(0 < count) // need to find a building name that is different than current buildname
            {
               die1 = Utilities.RandomGenerator.Next(5);
               die2 = Utilities.RandomGenerator.Next(6);
               fullBuildingName = TableMgr.GetTargetBuildingName(die1, die2); // Find the target building location.
               if ("ERROR" == fullBuildingName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): GetTargetBuildingName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
                  return false;
               }
               if (false == fullBuildingName.Contains(buildingName))
                  break;
            }
            if(count < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): count < 0");
               return false;
            }

            mi.Movement *= 2; // Movement is doubled when combat results in Attacker or Defender fleeing
            ITerritory? newTerritory = Territories.theTerritories.Find(fullBuildingName);
            if (null == newTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): unable to find buildingName=" + fullBuildingName);
               return false;
            }
            //-----------------------------------------
            Logger.Log(LogEnum.LE_SHOW_COMBATS, "Create_MapItemFlee(): mi=" + mi.Name + " entering t=" + newTerritory.Name);
            IMapItemMove? mim = gi.CreateMapItemMove(mi, newTerritory);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_MapItemFlees(): Create_MapItemMove() returned null");
               return false;
            }
            gi.MapItemMoves.Add(mim);
         }
         return true;
      }
   }
   //----------------------------------------------------------------
   class GameStateIterogations : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.InterrogationsSelect:
               break;
            case GameAction.InterrogationsPerform:
               foreach(IMapItem mi in gi.SelectedMapItems)
                  mi.IsInterrogated = true;
               gi.NumTownGuessesForZebulonLocation = 4;
               break;
            case GameAction.InterrogationsGuess:
               if (null == gi.SelectedTerritory)
               {
                  returnStatus = "SelectedTerritory=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(): " + returnStatus);
               }
               else
               {
                  if( gi.SelectedTerritory.ToString() == gi.Zebulon.TerritoryCurrent.ToString())
                  {
                     gi.Zebulon.IsAlienKnown = true;
                     gi.NumTownGuessesForZebulonLocation = 0;
                  }
                  else
                  {
                     gi.ZebulonTerritories.Add(gi.SelectedTerritory);
                     gi.SelectedTerritory = null;  
                     gi.NumTownGuessesForZebulonLocation--;
                  }
                  if( 0 == gi.NumTownGuessesForZebulonLocation)
                  {
                     if (false == CheckForIterogations(gi, ref action))
                     {
                        returnStatus = "CheckFor_Iterogations() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(InterrogationsGuess): " + returnStatus);
                     }
                  }
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateIterogations.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateImplantRemoval : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.ImplantRemovalsRoll:
               if (2 != gi.SelectedMapItems.Count)
               {
                  returnStatus = " 2 != (gi.SelectedMapItems.Count=" + gi.SelectedMapItems.Count + ")";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(ImplantRemovalsRoll): " + returnStatus);
               }
               else
               {
                  IMapItem? leftMapItem = gi.SelectedMapItems[0];
                  IMapItem? rightMapItem = gi.SelectedMapItems[1];
                  if (null == leftMapItem || null == rightMapItem)
                  {
                     returnStatus = " leftMapItem or rightMapItem = null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(ImplantRemovalsRoll): " + returnStatus);
                  }
                  else
                  {
                     string result = TableMgr.GetImplantRemovalResult(dieRoll);
                     switch (result)
                     {
                        case "Implant Explodes!":
                           rightMapItem.IsKilled = true;           // Kill the townsperson counter
                           leftMapItem.IsKilled = true;            // Kill the Alien counter
                           rightMapItem.IsImplantRemovalAttempt = true;
                           break;
                        case "Implant is tighly attached. Try again next turn.":
                           rightMapItem.IsImplantRemovalAttemptThisTurn = true;
                           break;
                        case "Implant is removed but disintegrates.":
                           rightMapItem.IsImplantRemovalAttempt = true;
                           break;
                        case "Implant is removed intact! Use as evidence.":
                           rightMapItem.IsImplantRemovalAttempt = true;
                           leftMapItem.IsImplantHeld = true;
                           break;
                        default:
                           returnStatus = "reached default with result=" + result;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
                           break;
                     }
                  }
                  if (false == CheckForImplantRemovals(gi, ref action))
                  {
                     returnStatus = "CheckForImplantRemovals() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
                  }
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateImplantRemoval.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateAlienTakeover : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.AlienTakeoversShow: // Handled by EventViewer
               break;
            case GameAction.AlienTakeoversFinish:
               if (false == CheckForEndOfGame(gi, ref action))
               {
                  returnStatus = "CheckFor_EndOfGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateAlienTakeover.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateEnded : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.ExitGame:
               Application.Current.Shutdown();
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateEnded.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateEnded.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //----------------------------------------------------------------
   class GameStateUnitTest : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         string returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowCharacterDescription:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateGameOptions:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
            case GameAction.UpdateNewGameEnd:
               break;
            case GameAction.UpdateRotateStack:
               if (false == RotateStack(gi))
               {
                  returnStatus = "Rotate_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateScatterStack:
               if (false == ScatterStack(gi))
               {
                  returnStatus = "Scatter_Stack() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.RemoveSplashScreen:  // GameStateUnitTest.PerformAction() - Unit Test PerintDiagnosticInfoToLog()
               PrintDiagnosticInfoToLog();
               break;
            case GameAction.UnitTestCommand: // call the unit test's Command() function
               IUnitTest ut = gi.UnitTests[gi.GameTurn];
               if (false == ut.Command(ref gi))
               {
                  returnStatus = "Command() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestNext: // call the unit test's NextTest() function
               IUnitTest ut1 = gi.UnitTests[gi.GameTurn];
               if (false == ut1.NextTest(ref gi))
               {
                  returnStatus = "NextTest() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestCleanup: // Call the unit test's NextTest() function
               IUnitTest ut2 = gi.UnitTests[gi.GameTurn];
               if (false == ut2.Cleanup(ref gi))
               {
                  returnStatus = "Cleanup() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateUnitTest.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a=");
         sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra=");
         sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e=");
         sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
}
