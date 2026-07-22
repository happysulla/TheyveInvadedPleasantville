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
         gi.GamePhase = phase;
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
         double countOffset = 0.0;
         foreach (IMapItem mi in gi.SelectedStack.MapItems)
         {
            double offset = (countOffset * 3.0) + (mi.Zoom * Utilities.theMapItemOffset);
            mi.Location.X = gi.SelectedStack.Territory.CenterPoint.X - offset;
            mi.Location.Y = gi.SelectedStack.Territory.CenterPoint.Y - offset;
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
         IMapItems controlledPeps = new MapItems();
         IMapItems uncontrolledPeps = new MapItems();
         foreach (Stack stack in gi.Stacks)
         {
            controlledPeps.Clear();
            uncontrolledPeps.Clear();
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
               gi.SelectedTerritories.Add(stack.Territory);
               action = GameAction.ConversationsSelect;
               Logger.Log(LogEnum.LE_SHOW_CONVERSATIONS, "CheckFor_Conversations(): adding stack=" + stack.Territory.ToString() ); 
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
               action = GameAction.InfluencesSelect;
               gi.SelectedTerritories.Add(stack.Territory);
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
               action = GameAction.CombatsSelect;
               gi.SelectedTerritories.Add(stack.Territory);
               Logger.Log(LogEnum.LE_SHOW_COMBATS, "CheckFor_Combats(): Adding t=" + stack.Territory.ToString() + " c=" + controlledPeps.Count.ToString() + " u=" + uncontrolledPeps.Count.ToString() + " ka=" + knownAliens.Count.ToString() + " ua=" + unknownAliens.Count.ToString());
               gi.DieRollAction = GameAction.DieRollActionNone;
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
         if (true == gi.Zebulon.IsAlienKnown)  // If Zebulon is already on the map board, no need to iterogate
         {
            IMapItems controlled = new MapItems();
            IMapItems surrenderedAliens = new MapItems();
            foreach (Stack stack in gi.Stacks)
            {
               if (stack.MapItems.Count < 2)
                  continue;
               foreach (MapItem mi in stack.MapItems)
               {
                  if ((true == mi.IsInterrogatedThisTurn) || (true == mi.IsInterrogated) || (true == mi.IsKilled) || (true == mi.IsUnconscious) || (true == mi.IsStunned))
                     continue;
                  if (true == mi.IsControlled)
                  {
                     if (false == mi.IsTiedUp)
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
                  action = GameAction.InterrogationsSelect;
                  gi.SelectedTerritories.Add(stack.Territory);
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
               if ((true == mi.IsImplantRemovalThisTurn) || (true == mi.IsKilled))
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
               action = GameAction.ImplantRemovalsSelect;
               gi.SelectedTerritories.Add(stack.Territory);
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
            IMapItems aliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (true == mi.IsUnconscious) )  // Unconscious or dead cannot be taken over
                  continue;
               if ((true == mi.IsControlled) || (true == mi.IsWary))
               {
                  if ((true == mi.IsStunned) || (true == mi.IsTiedUp))
                     possibleVictims.Add(mi);
               }
               else
               {
                  if ((true == mi.IsAlienKnown) || (true == mi.IsAlienUnknown) )
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp))
                        aliens.Add(mi);
                  }
                  else
                  {
                     possibleVictims.Add(mi);
                  }
               }
               if ((1 < possibleVictims.Count) || (1 < aliens.Count) || ((0 < possibleVictims.Count) && (0 < aliens.Count)))      
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
                  Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "CheckFor_AlienTakeovers(): t=" + stack.Territory.ToString() + " v=" + possibleVictims.ToString() + " a=" + aliens.ToString());
                  if ( false == gi.PlayerAlien.ShowPossibleTakeover(gi, stack, ref action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckFor_AlienTakeovers(): Perform_AlienTakeover() returned error");
                     return false;
                  }
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
            StringBuilder sb1 = new StringBuilder("CheckFor_EndOfGame(): Tied Up Units in t=\n"); sb1.Append(stack.Territory.ToString());
            foreach (MapItem mi in stack.MapItems)
            {
               mi.IsMoveStoppedThisTurn = false;
               mi.IsMoveAllowedToResetThisTurn = true;
               mi.IsConversedThisTurn = false;
               mi.IsInfluencedThisTurn = false;
               mi.IsCombatThisTurn = false;
               mi.IsInterrogatedThisTurn = false;
               mi.IsImplantRemovalThisTurn = false;
               mi.IsTakeoverThisTurn = false;
               if ((true == mi.IsSurrendered) || (true == mi.IsKilled))
                  continue;
               if (true == mi.IsTiedUp) // Cound be stunned or unconscious
               {
                  if (true == mi.IsAlienKnown)
                     alienTiedUpPersons.Add(mi);
                  else if (true == mi.IsControlled)
                     controlledTiedUpPersons.Add(mi);
                  sb1.Append(" ");
                  sb1.Append(mi.Name);
               }
               if ((false == mi.IsTiedUp) && (false == mi.IsUnconscious) && (false == mi.IsStunned))
               {
                  if (true == mi.IsAlienKnown)
                  {
                     isFriendlyAlienHelping = true;
                     sb1.Append(" FRIENDLY ALIEN=");
                  }
                  else if (true == mi.IsControlled)
                  {
                     isFriendlyControlledHelping = true;
                     sb1.Append(" FRIENDLY TP=");
                  }
                  sb1.Append(mi.Name);
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
                  sb1.Append(" untied alien=");
                  sb1.Append(alien.Name);
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
                  sb1.Append(" untied TP=");
                  sb1.Append(controlled.Name);
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
            Logger.Log(LogEnum.LE_GAMESTATE_CHECKER_TIED_UP, sb1.ToString());
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
                        GameEngine.theIsAlien = false;
                        GameEngine.theIsHost = true;
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
                           GameEngine.theIsAlien = false;
                           GameEngine.theIsHost = false;
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
               GameEngine.theIsHost = true;
               GameEngine.theIsAlien = true;
               gi.PlayerAlien = new PlayerAlienHuman();
               gi.PlayerTown = new PlayerTownComputer();
               gi.EventActive = gi.EventDisplayed = "e002";
               gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
               break;
            case GameAction.GameSetupPlayTownsperson:
               GameEngine.theIsHost = true;
               GameEngine.theIsAlien = false;
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
         gi.AddUnknownAlien(startingAlien);
         Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Assign_StartingAlien(): startingAlien=" + name);
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
         gi.AddUnknownAlien(startingAlien);
         Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Assign_StartingAlien(): startingAlien=" + name);
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
            die1 = Utilities.RandomGenerator.Next(5);
            die2 = Utilities.RandomGenerator.Next(6);
            string fullBuildingName = TableMgr.GetTargetBuildingName(die1, die2); // Find the target building location.
            if ("ERROR" == fullBuildingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "Choose_RandomMovePeopleAndDest(): GetTargetBuildingName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
               return false;
            }
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
               Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): prep moving " + name + " to " + fullBuildingName);
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
               Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Choose_RandomMovePeopleAndDest(): mi=null for " + rmd.myName);
               return false;
            }
            string buildingName = rmd.myBuildingName;
            mi.Movement *= 2; // Movement is doubled during Random Movement
            ITerritory? newTerritory = Territories.theTerritories.Find(buildingName);
            if (null == newTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): unable to find buildingName=" + buildingName);
               return false;
            }
            //-----------------------------------------
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Perform_RandomMoves(): mi=" + mi.Name + " entering t=" + newTerritory.Name);
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
                        if (true == rightMapItem.IsAlienUnknown)
                           gi.AddKnownAlien(rightMapItem);
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
                     else if (0.666 < odds)
                        dieThreshold = 8;
                     else if (0.499 < odds)
                        dieThreshold = 9;
                     else
                        dieThreshold = 10;
                     int dieRollModifier = 0;
                     if (true == isImplantHeld) // Subtact one if a controlled person holds evidence of an implant.
                        --dieRollModifier;
                     if (true == rightMapItem.IsSkeptical) // Check if MapItem is skeptical.  If both skeptical and wary,
                        ++dieRollModifier;
                     if (true == rightMapItem.IsWary)  // If not skeptical, check if wary.  This adds to the die roll.
                        --dieRollModifier;
                     dieThreshold += dieRollModifier;
                     if (dieThreshold <= dieRoll) // Check for alien.  If alien, let user know it is discovered. Else, make the townsperson controlled.
                     {
                        if (true == rightMapItem.IsAlienUnknown)
                        {
                           gi.AddKnownAlien(rightMapItem);
                           Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "GameStateInfluences.PerformAction(): AddingKnownAlien() rightMapItem=" + rightMapItem.Name + " dr=" + dieRoll.ToString() + " t=" + dieThreshold.ToString());
                        }
                        else
                        {
                           gi.AddControlled(rightMapItem);
                           Logger.Log(LogEnum.LE_SHOW_TOWNS_ADD, "GameStateInfluences.PerformAction(): AddingKnownAlien() rightMapItem=" + rightMapItem.Name + " dr=" + dieRoll.ToString() + " t=" + dieThreshold.ToString());
                        }
                     }
                     else
                     {
                        if (false == rightMapItem.IsWary)  // wary people cannot become skeptical
                        {
                           rightMapItem.IsSkeptical = true;
                           Logger.Log(LogEnum.LE_SHOW_WARY_ADD, "GameStateInfluences.PerformAction(): AddingKnownAlien() rightMapItem=" + rightMapItem.Name + " dr=" + dieRoll.ToString() + " t=" + dieThreshold.ToString());
                        }
                     }
                     if ("OK" == returnStatus)
                     {
                        if (false == CheckForInfluences(gi, ref action))
                        {
                           returnStatus = "CheckFor_Influences() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(): " + returnStatus);
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
            case GameAction.CombatsFinish:
               if (false == CheckForIterogations(gi, ref action))
               {
                  returnStatus = "CheckFor_Iterogations() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsFinish): " + returnStatus);
               }
               break;
            case GameAction.CombatsRoll:
               if( null == gi.MapItemCombat )
               {
                  returnStatus = "MapItemCombat=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsFinish): " + returnStatus);
               }
               else
               {
                  int totalCombatForAttacker = 0;
                  foreach (IMapItem mi in gi.MapItemCombat.Attackers)
                     totalCombatForAttacker += mi.Combat;
                  int totalCombatForDefender = 0;
                  foreach (IMapItem mi in gi.MapItemCombat.Defenders)
                     totalCombatForDefender += mi.Combat;

                  if (false == CheckForIterogations(gi, ref action))
                  {
                     returnStatus = "CheckFor_Iterogations() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(CombatsFinish): " + returnStatus);
                  }
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
      public bool PerformCombat(IGameInstance gi)
      {
         if (null == gi.MapItemCombat)
         {
            MessageBox.Show("No Combat");
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): No Combat");
            return false;
         }
         IMapItemCombat combat = gi.MapItemCombat;
         if (null == combat.Territory)
         {
            MessageBox.Show("No combat territory");
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): No combat territory");
            return false;
         }
         combat.DieRoll1 = Utilities.RandomGenerator.Next(6) + 1; // Assignment increases roll by one
         combat.DieRoll2 = Utilities.RandomGenerator.Next(6) + 1; // Assignment increases roll by one
         int resultsRoll = combat.DieRoll1 + combat.DieRoll2 - 2;
         combat.IsAnyRetreat = false;  // assume no retreats until the results are known
         //-------------------------------------------------------------------------------
         IMapItems aliens = new MapItems();
         IMapItems controlled = new MapItems();
         IMapItems uncontrolled = new MapItems();
         IMapItems wary = new MapItems();
         foreach(IStack stack in gi.Stacks) // In each stack, get the count in the stack of the number of aliens and controlled townspeople
         {
            foreach (MapItem mi in stack.MapItems)
            {
               if ((combat.Territory.Name == mi.TerritoryCurrent.Name) && (combat.Territory.Subname == mi.TerritoryCurrent.Subname))
               {
                  if ((false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || (true == mi.IsSurrendered))
                     continue;

                  if (true == mi.IsAlienKnown)
                  {
                     aliens.Add(mi);
                  }
                  else if (true == mi.IsAlienUnknown)
                  {
                     gi.AddKnownAlien(mi); // All aliens in this combat become exposed.
                     aliens.Add(mi);
                  }
                  else if (true == mi.IsControlled)
                  {
                     controlled.Add(mi);
                  }
                  else
                  {
                     if (true == mi.IsWary)
                        wary.Add(mi);
                     uncontrolled.Add(mi);
                  }
               }
            }
         }
         //-------------------------------------------------------
         if (0 == controlled.Count) // If there is no combat, return from this method
         {
            if ((0 == aliens.Count) || (0 == wary.Count))
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): aliens.Count=0 wary.Count=0 controlled.Count=0");
               return false;
            }
         }
         //-------------------------------------------------------
         int alienAttackCombat = 0;
         int alienCount = 0;
         aliens = aliens.SortOnCombat();
         foreach (IMapItem alien in aliens) // Determine the attack strength of the aliens.  Limit it to top three counters.
         {
            alienAttackCombat += alien.Combat;
            StringBuilder sb = new StringBuilder("PerformCombat():"); sb.Append(alien.Name); sb.Append(" ++++ "); sb.Append(alien.Combat.ToString()); sb.Append(" to Alien="); sb.Append(alienAttackCombat.ToString());
            Logger.Log(LogEnum.LE_COMBAT_SUMS, sb.ToString());
            if (3 <= ++alienCount)
               break;
         }
         // Determine the attack strength of the townspeople.  
         // Limit it to top three counters.
         int controlledAttackCombat = 0;
         int controlledCount = 0;
         controlled = controlled.SortOnCombat();
         foreach (IMapItem person in controlled)
         {
            controlledAttackCombat += person.Combat;
            StringBuilder sb = new StringBuilder("PerformCombat():"); sb.Append(person.Name); sb.Append(" ++++ "); sb.Append(person.Combat.ToString()); sb.Append(" to TP="); sb.Append(controlledAttackCombat.ToString());
            Logger.Log(LogEnum.LE_COMBAT_SUMS, sb.ToString());
            if (3 <= ++controlledCount)
               break;
         }
         // Determine the attack strength of the wary townspeople.  
         // Limit it to top three counters.
         int waryAttackCombat = 0;
         int waryCount = 0;
         wary = wary.SortOnCombat();
         foreach (IMapItem person in wary)
         {
            waryAttackCombat += person.Combat;
            if (3 <= ++waryCount)
               break;
         }
         int combatFactorDifference = 0;
         if ((0 < aliens.Count) && (0 < controlled.Count)) // A normal attack with known aliens in the same hex as controlled townspeople
         {
            if ((0 == alienCount) || (0 == controlledCount)) // If there is no combat, ignore this stack
            {
               return true;
            }
            // Determine who is attackers and who are defenders based
            // on which side has the most Combat Factors.
            if (controlledAttackCombat < alienAttackCombat)
            {
               combatFactorDifference = alienAttackCombat - controlledAttackCombat;
               combat.Attackers = aliens;
               combat.Defenders = controlled;
            }
            else
            {
               combatFactorDifference = controlledAttackCombat - alienAttackCombat;
               combat.Attackers = controlled;
               combat.Defenders = aliens;
            }
            // Determine one index into the Combat Results Table.
            int tableFactor = 0;
            if (combatFactorDifference < 1)
               tableFactor = 0;
            else if (combatFactorDifference < 4)
               tableFactor = 1;
            else if (combatFactorDifference < 7)
               tableFactor = 2;
            else if (combatFactorDifference < 10)
               tableFactor = 3;
            else
               tableFactor = 4;
            foreach (IMapItem alien in aliens) // A column shift occurs if any aliens went through an influence attempt this turn.
            {
               if (true == alien.IsInfluencedThisTurn)
               {
                  if (controlledAttackCombat < alienAttackCombat)  // aliens are attackers
                  {
                     if (0 == tableFactor)                        // shift column to right
                        tableFactor = 1;
                     else if (1 == tableFactor)
                        tableFactor = 2;
                     else if (2 == tableFactor)
                        tableFactor = 3;
                     else if (3 == tableFactor)
                        tableFactor = 4;
                  }
                  else                                             // aliens are defenders
                  {
                     if (1 == tableFactor)                        // shift column to left
                        tableFactor = 0;
                     else if (2 == tableFactor)
                        tableFactor = 1;
                     else if (3 == tableFactor)
                        tableFactor = 2;
                     else if (4 == tableFactor)
                        tableFactor = 3;
                  }
                  break;  // only one column shift occurs.
               }
            }
            combat.Result = TableMgr.theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.
         }
         //***********************************************************************************************
         else if (0 < controlled.Count)  // Controlled townspeople attacking uncontrolled is automatic win
         {
            combat.Result = CombatResult.AttackerWins;
            combat.Attackers = controlled;
            combat.Defenders = uncontrolled;
         }
         //***********************************************************************************************
         else  // Alien townspeople attacking wary 
         {
            if ((0 == alienCount) || (0 == waryCount)) // If there is no combat, ignore this stack
            {
               return true;
            }
            // Determine who is attackers and who are defenders based
            // on which side has the most Combat Factors.
            bool isAlienAttacker = false;
            if (waryAttackCombat < alienAttackCombat)
            {
               combatFactorDifference = alienAttackCombat - waryAttackCombat;
               combat.Attackers = aliens;
               combat.Defenders = wary;
               isAlienAttacker = true;
            }
            else
            {
               combatFactorDifference = waryAttackCombat - alienAttackCombat;
               combat.Attackers = wary;
               combat.Defenders = aliens;
            }
            // Determine one index into the Combat Results Table.
            int tableFactor = 0;
            if (combatFactorDifference < 1)
               tableFactor = 0;
            else if (combatFactorDifference < 4)
               tableFactor = 1;
            else if (combatFactorDifference < 7)
               tableFactor = 2;
            else if (combatFactorDifference < 10)
               tableFactor = 3;
            else
               tableFactor = 4;
            combat.Result = TableMgr.theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.
                                                                         // If Aliens lose to Wary people, the results is that the aliens immediately flee.
            if ((true == isAlienAttacker) && (CombatResult.DefenderWins == combat.Result))
               combat.Result = CombatResult.DefenderFlees;
            if ((false == isAlienAttacker) && (CombatResult.AttackerWins == combat.Result))
               combat.Result = CombatResult.AttackerFlees;
         }
         // Indicate who participated in the attack
         foreach (IMapItem defender in combat.Defenders)
            defender.IsCombatThisTurn = true;
         foreach (IMapItem attacker in combat.Attackers)
            attacker.IsCombatThisTurn = true;
         // Resolve the results
         switch (combat.Result)
         {
            case CombatResult.AttackerWins:
               foreach (IMapItem defender in combat.Defenders)
               {
                  PerformCombatResolveLoss(gi, defender);
                  if (true == defender.IsStunned) // If the defender is stunned, they must retreat one territory
                     combat.IsAnyRetreat = true;
               }
               break;
            case CombatResult.DefenderWins:
               foreach (IMapItem attacker in combat.Attackers)
               {
                  PerformCombatResolveLoss(gi, attacker);
                  if (true == attacker.IsStunned) // If the attacker is stunned, they must retreat one territory
                     combat.IsAnyRetreat = true;
               }
               break;
            case CombatResult.AttackerFlees:
               combat.IsAnyRetreat = true;
               foreach (IMapItem attacker in combat.Attackers)
               {
                  if (attacker.Name == "Zebulon")
                  {
                     IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
                     if (null == zebulon)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PerformCombatResolveLoss(): Could not find Zebulon in gi.Persons");
                        return false;
                     }
                     zebulon.IsKilled = true;
                  }
                  attacker.TerritoryStarting = attacker.TerritoryCurrent;  // If there are any pending moves, make sure they are removed
                  //if (false == PerformMovement(gi, attacker))
                  //   Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", attacker.Name);
               }
               break;
            case CombatResult.DefenderFlees:
               combat.IsAnyRetreat = true;
               foreach (IMapItem defender in combat.Defenders)
               {
                  if (defender.Name == "Zebulon")
                  {
                     IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
                     if (null == zebulon)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PerformCombatResolveLoss(): Could not find Zebulon in gi.Persons");
                        return false;
                     }
                     zebulon.IsKilled = true;
                     return true;
                  }
                  defender.TerritoryStarting = defender.TerritoryCurrent;  // If there are any pending moves, make sure they are removed
                  //if (false == PerformMovement(gi, defender))
                  //{

                  //}
                  Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", defender.Name);
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PerformCombatResolveLoss(): reached default combat.Result=" + combat.Result.ToString());
               break;
         }
         return true;
      } // end function
      bool PerformCombatResolveLoss(IGameInstance gi, IMapItem mi)
      {
         if (mi.Name == "Zebulon")
         {
            IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
            if (null == zebulon)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombatResolveLoss(): Could not find Zebulon in gi.Persons");
               return false;
            }
            zebulon.IsKilled = true;
            return true;
         }
         // First perfom the actions that occur no matter what the result.
         // The influence factors are adjusted downward.
         gi.InfluenceCountTotal -= mi.Influence;
         StringBuilder sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from Total "); sb.Append(mi.Influence.ToString());
         sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
         sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
         sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
         sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
         Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
         if (true == mi.IsAlienUnknown)
         {
            gi.InfluenceCountAlienUnknown -= mi.Influence;
            sb = new StringBuilder("PerformCombatResolveLoss(): "); sb.Append(mi.Name); sb.Append(" ---- from unknown "); sb.Append(mi.Influence.ToString());
            sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
         }
         else if (true == mi.IsAlienKnown)
         {
            gi.InfluenceCountAlienKnown -= mi.Influence;
            sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from known "); sb.Append(mi.Influence.ToString());
            sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
         }

         if (true == mi.IsControlled)
         {
            gi.InfluenceCountTownspeople -= mi.Influence;
            sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from TP "); sb.Append(mi.Influence.ToString());
            sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
         }
         // Next see what the dice roll shows.
         int die1 = Utilities.RandomGenerator.Next(6) + 1;
         int die2 = Utilities.RandomGenerator.Next(6) + 1;
         int lossTableRoll = die1 + die2;

         if (lossTableRoll < 5)
         {
            mi.IsKilled = true;
         }
         else if (lossTableRoll < 7)
         {
            mi.IsUnconscious = false;
            if (true == mi.IsAlienKnown)
               mi.IsTiedUp = true;
         }
         else
         {
            if (true == mi.IsAlienKnown)
            {
               mi.IsSurrendered = true;
               mi.IsTiedUp = true;
            }
            else
            {
               mi.IsStunned = true;
               int tempMovement = mi.Movement; // Set up to only retreat one space 
               mi.Movement = 1;                // by setting the IMapItems movement to one.
               mi.IsMoved = false;
               mi.MovementUsed = 0;
               mi.TerritoryStarting = mi.TerritoryCurrent;  // If there are any pending moves, make sure they are removed
               //if (false == PerformMovement(gi, mi))
               //   Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", mi.Name);
               mi.Movement = tempMovement;     // return MapItem movement to original value
            }
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
               if (false == ResetPhase(gi, GamePhase.RandomMovement))
               {
                  returnStatus = "Reset_Phase() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               gi.EventActive = gi.EventDisplayed = "e005";
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover.PerformAction(): " + returnStatus);
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
