using Google.Protobuf.WellKnownTypes;
using PleasantvilleGame.Networking;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
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
            case GamePhase.AlienStart: return new GameStateSetup();
            case GamePhase.AlienMovement: return new GameStateAlienPlayerMovement();
            case GamePhase.AlienTakeover: return new GameStateAlienTakeover();
            case GamePhase.Combat: return new GameStateCombat();
            case GamePhase.Conversations: return new GameStateConversations();
            case GamePhase.ImplantRemoval: return new GameStateImplantRemoval();
            case GamePhase.Influences: return new GameStateInfluences();
            case GamePhase.Iterrogations: return new GameStateIterogations();
            case GamePhase.RandomMovement: return new GameStateRandomMovement();
            case GamePhase.ShowEndGame: return new GameStateEnded();
            case GamePhase.TownspersonMovement: return new GameStateTownPlayerMovement();
            case GamePhase.TownspersonStart: return new GameStateSetup();
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
      public bool PerformMovement(IGameInstance gi, IMapItem mi)
      {
         int r3 = Utilities.RandomGenerator.Next(5);
         int r4 = Utilities.RandomGenerator.Next(6);
         string building = Utilities.RemoveSpaces(TableMgr.theTargetBuildingTable[r3, r4]); // Find the target building location.
                                                                                            //-----------------------------------------
         int numOfSectorsInBuilding = 0;
         for (int i1 = 0; i1 < TableMgr.theBuildingSizes.GetLength(0); i1++)   // If moving to a build, randomly select a space from the building. GetLength(0) gets the length of the array.
         {
            string buildingToCompare = Utilities.RemoveSpaces(TableMgr.theBuildingSizes[i1, 0]);
            if (buildingToCompare == building)
            {
               numOfSectorsInBuilding = Int32.Parse(TableMgr.theBuildingSizes[i1, 1]);
               break;
            }
         }
         int selectedSector = Utilities.RandomGenerator.Next(numOfSectorsInBuilding);
         ++selectedSector;
         ITerritory? newTerritory = Territories.theTerritories.Find(building, selectedSector.ToString());
         if (null == newTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformMovement(): newTerritory is null for building=" + building + " selectedSector=" + selectedSector.ToString());
            return false;
         }
         if ((mi.TerritoryCurrent.Name == newTerritory.Name) && (mi.TerritoryCurrent.Subname == newTerritory.Subname))
         {
            return false;
         }
         //-----------------------------------------
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Move_TaskForceToNewArea(): mi=" + mi.Name + " entering t=" + newTerritory.Name);
         if (false == CreateMapItemMove(gi, mi, newTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_TaskForceToNewArea(): AddMapItemMove() returned false");
            return false;
         }
         mi.IsMoved = true;
         return true;
      }
      public void PerformMovements(IGameInstance gi, int numPeopleToMove)
      {
         int numPeopleSkipped = 0;
         int numPeopleMoved = 0;
         int loopCount = 0;
         while ((numPeopleMoved < numPeopleToMove) && (++loopCount < 200))
         {
            int r1 = Utilities.RandomGenerator.Next(5);
            int r2 = Utilities.RandomGenerator.Next(6);
            string person = Utilities.RemoveSpaces(TableMgr.theTownpersonsTable[r1, r2]);
            IMapItem? personMoving = gi.Townspeople.Find(person);
            if (null == personMoving)
            {
               ++numPeopleSkipped;
               //Console.WriteLine("PerformMovements(): {0} Unknown {1}", numPeopleSkipped.ToString(), person);
               continue;
            }

            // If the counter is moved or tied up or known to be alien controlled, do not move.
            if ((true == personMoving.IsMoved) || (true == personMoving.IsControlled) || (true == personMoving.IsAlienKnown) ||
               (true == personMoving.IsStunned) || (true == personMoving.IsSurrendered) ||
               (true == personMoving.IsTiedUp) || (true == personMoving.IsWary) || (false == personMoving.IsUnconscious))
            {
               ++numPeopleSkipped;
               //                    Console.WriteLine("PerformMovements(): {0} Skipping Person {1}: {2},{3},{4},{5},{6},{7},{8},{9}",
               //                                       numPeopleSkipped.ToString(),
               //                                       personMoving.Name,
               //                                       personMoving.IsMoved.ToString(),
               //                                       personMoving.IsControlled.ToString(), personMoving.IsAlienKnown.ToString(),
               //                                       personMoving.IsStunned.ToString(), personMoving.IsSurrendered.ToString(),
               //                                       personMoving.IsTiedUp.ToString(), personMoving.IsWary.ToString(),
               //                                       personMoving.IsUnconscious.ToString());
               continue;
            }

            if (false == PerformMovement(gi, personMoving))
            {
               ++numPeopleSkipped;
               Console.WriteLine("PerformMovements(): {0} Same Building {1}", numPeopleSkipped.ToString(), person);
               continue;
            }
            else
            {
               ++numPeopleMoved;  // Keep track of number of people moved
                                  //                    Console.WriteLine("PerformMovements(): {0} Moved {1}", numPeopleMoved.ToString(), person);
            }
         }  // end while()
      }
      protected bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.Name);
         gi.MapItemMoves.Insert(0, mim); // add at front
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
                  Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.RemoveSplashScreen: // GameStateSetup.PerformAction()
               if (false == SetupNewGame(gi, ref action))
               {
                  returnStatus = "SetupNewGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UpdateRotateStack:
               if( null == gi.SelectedStack)
               {
                  returnStatus = "gi.SelectedStack=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(UpdateRotateStack): " + returnStatus);
               }
               else
               {
                  int count = gi.SelectedStack.MapItems.Count;
                  if (count < 2 )
                  {
                     returnStatus = "gi.SelectedStack.MapItem.Count=" + count.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(UpdateRotateStack): " + returnStatus);
                  }
                  else
                  {
                     IMapItem? bottom = gi.SelectedStack.MapItems[0];
                     if( null == bottom)
                     {
                        returnStatus = "bottom=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(UpdateRotateStack): " + returnStatus);
                     }
                     else
                     {

                        for(int i=1; i< count; i++)
                           gi.SelectedStack.MapItems[i-1] = gi.SelectedStack.MapItems[i];
                        gi.SelectedStack.MapItems[count - 1] = bottom;
                        double countOffset = 0.0;
                        foreach (IMapItem mi in gi.SelectedStack.MapItems)
                        {
                           double offset = (countOffset * 3.0) + (mi.Zoom * Utilities.theMapItemOffset);
                           mi.Location.X = gi.SelectedStack.Territory.CenterPoint.X - offset;
                           mi.Location.Y = gi.SelectedStack.Territory.CenterPoint.Y - offset;
                        }
                     }
                  }
               }
               break;
            case GameAction.UpdateScatterStack:
               if (null == gi.SelectedStack)
               {
                  returnStatus = "gi.SelectedStack=null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(UpdateRotateStack): " + returnStatus);
               }
               else
               {
                  if( true == gi.SelectedStack.IsStacked)
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
                        GameEngine.theIsAlien = true;
                        GameEngine.theIsComputerOpponent = false;
                        GameEngine.theIsServer = true;
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
                           GameEngine.theIsComputerOpponent = false;
                           GameEngine.theIsServer = false;
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
               GameEngine.theIsAlien = true;
               gi.PlayerAlien = new PlayerAlienHuman();
               gi.PlayerTown = new PlayerTownComputer();
               gi.EventActive = gi.EventDisplayed = "e002";
               gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
               break;
            case GameAction.GameSetupPlayTownsperson:
               GameEngine.theIsAlien = false;
               gi.PlayerAlien = new PlayerAlienComputer();
               gi.PlayerTown = new PlayerTownHuman();
               gi.EventActive = gi.EventDisplayed = "e002";
               gi.DieRollAction = GameAction.GameSetupStartingTownsplayerSetRoll;
               break;
            case GameAction.GameSetupStartingTownsplayerSetRoll:
               if( Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if( false == gi.PlayerTown.GetStartingTownsperson(gi, dieRoll))
                  {
                     returnStatus = "Get_StartingTownsperson() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(GameSetupStartingTownsplayerSetRoll): " + returnStatus);
                  }
               }
               else
               {
                  gi.EventActive = gi.EventDisplayed = "e003";
                  gi.DieRollAction = GameAction.GameSetupStartingAlienSetRoll;
                  if( false == gi.PlayerAlien.GetNextState(gi))
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
                  if( false == gi.PlayerAlien.GetStartingAlien(gi))
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
               if ( false == CreateTownspeople(gi))
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
            case GameAction.GameSetupFinished:
               gi.GamePhase = GamePhase.RandomMovement;
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
      public bool CreateTownspeople(IGameInstance gi)
      {
         gi.Townspeople.Clear();
         //------------------------------------
         string tName = "";
         ITerritory? t = null;
         int maxNum = 4;
         int randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i=0; i< maxNum; i++)
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
         string name = "BankGuard";
         string miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem mi = new MapItem(miName, 0.8, name, t, 5, 10, 8);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "BankPresident";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 19, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): mi=" + mi.Name + " tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "BarAndGrillOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "BarTender";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "CheckoutGirl";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 7, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Deputy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 9);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Doctor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 18, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "FireChief";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 12, 8);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "HotelOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Judge";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Lawyer";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Maid";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "MaitreD";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 4);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Mayor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 16, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Minister";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 20, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Paperboy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 9, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Plumber";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 8);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "RepairShopOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Sheriff";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 15, 10);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "StationAttendant";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "SuperMarketManager";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Tailor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 11, 5);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Teacher";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 17, 4);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Teller";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "TownDrunk";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 3, 3, 8);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Vet";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 13, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Waitress";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "WarVeteran";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 12, 4);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Welder";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Townspeople.Add(mi);
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
            if (null != tStack) // if stack exists, then mapitem already exists at this location. Skip it.
            {
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): tNum=" + tNum.ToString() + " tName=" + tName + " stacks=" + gi.Stacks.ToString());
               continue;
            }
         }
         name = "Wife";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 8, 4);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         return true;
      }
      private bool AssignStartingTownsplayer(IGameInstance gi)
      {
         string name = gi.PlayerTown.StartingTownspeople[0];
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
         string name = gi.PlayerAlien.StartingTownspeople[0];
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
         startingAlien.IsAlienUnknown = true;
         //------------------------------------
         name = gi.PlayerAlien.StartingTownspeople[1];
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
         startingAlien.IsAlienUnknown = true;
         return true;
      }
   }
   //----------------------------------------------------------------
   class AlienStart : GameState
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
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi))
               {
                  returnStatus = "Load_Game() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.AlienStart:
               gi.IsAlienStarted = true;
               if (true == gi.IsControlledStarted)
               {
                  gi.GamePhase = GamePhase.RandomMovement;
                  gi.NextAction = "Display Random Movement";
               }
               else
               {
                  gi.NextAction = "Awaiting Townsperson Start";
               }
               break;
            case GameAction.TownspersonStart:
               gi.IsControlledStarted = true;
               if (true == gi.IsAlienStarted)
               {
                  gi.GamePhase = GamePhase.RandomMovement;
                  gi.NextAction = "Display Random Movement";
               }
               else
               {
                  gi.NextAction = "Awaiting Alien Start";
               }
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::AlienStart.PerformAction():");
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
            case GameAction.ShowAlien:
               break;
            case GameAction.AlienDisplaysRandomMovement:
               gi.IsAlienDisplayedRandomMovement = true;
               RecordIncapacitatedPeople(gi);
               if (false == gi.IsControlledDisplayedRandomMovement)
               {
                  PerformMovements(gi, 4);
                  gi.NextAction = "Awaiting Townsperson Display Random Movement";
               }
               else
               {
                  if (true == gi.IsControlledAckedRandomMovement)
                     gi.NextAction = "Awaiting Alien Ack Random Movement";
                  else
                     gi.NextAction = "Ack Random Movement";
               }
               break;
            case GameAction.TownspersonDisplaysRandomMovement:
               gi.IsControlledDisplayedRandomMovement = true;
               if (false == gi.IsAlienDisplayedRandomMovement)
               {
                  PerformMovements(gi, 4);
                  gi.NextAction = "Awaiting Alien Display Random Movement";
               }
               else
               {
                  if (true == gi.IsAlienAckedRandomMovement)
                     gi.NextAction = "Awaiting Townsperson Ack Random Movement";
                  else
                     gi.NextAction = "Ack Random Movement";
               }
               break;
            case GameAction.AlienAcksRandomMovement:
               gi.IsAlienAckedRandomMovement = true;
               if (true == gi.IsControlledAckedRandomMovement)
               {
                  gi.IsAlienDisplayedRandomMovement = false;
                  gi.IsControlledDisplayedRandomMovement = false;
                  gi.IsAlienAckedRandomMovement = false;
                  gi.IsControlledAckedRandomMovement = false;

                  gi.NextAction = "Alien Performs Movement";
                  gi.GamePhase = GamePhase.AlienMovement;
                  foreach (IMapItemMove mim in gi.MapItemMoves)
                  {
                     IMapItem mi = mim.MapItem;
                     if (null == mim.NewTerritory)
                     {
                        returnStatus = "mim.NewTerritory is null for mi=" + mi.Name;
                        Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
                        break;
                     }
                     mi.TerritoryCurrent = mim.NewTerritory;
                     mi.TerritoryStarting = mim.NewTerritory;
                     mi.IsMoved = false;
                     mi.MovementUsed = 0;
                  }
                  if ("OK" == returnStatus)
                  {
                     gi.MapItemMoves.Clear();
                     if (null == gi.MapItemCombat)
                     {
                        returnStatus = "gi.MapItemCombat is null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        gi.MapItemCombat.IsAnyRetreat = false;
                        gi.Takeover = null;
                     }
                  }
               }
               else
               {
                  if (false == gi.IsControlledDisplayedRandomMovement)
                     gi.NextAction = "Awaiting Townsperson Display Random Movement";
                  else
                     gi.NextAction = "Awaiting Townsperson Ack Random Movement";
               }
               break;
            case GameAction.TownspersonAcksRandomMovement:
               gi.IsControlledAckedRandomMovement = true;
               if (true == gi.IsAlienAckedRandomMovement)
               {

                  gi.IsAlienDisplayedRandomMovement = false;
                  gi.IsControlledDisplayedRandomMovement = false;
                  gi.IsAlienAckedRandomMovement = false;
                  gi.IsControlledAckedRandomMovement = false;
                  gi.Takeover = null;
                  if (null == gi.MapItemCombat)
                  {
                     returnStatus = "gi.MapItemCombat is null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     gi.MapItemCombat.IsAnyRetreat = false;
                     foreach (IMapItemMove mim in gi.MapItemMoves)
                     {
                        IMapItem mi = mim.MapItem;
                        if (null == mim.NewTerritory)
                        {
                           returnStatus = "mim.NewTerritory is null for mi=" + mi.Name;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateRandomMovement.PerformAction(): " + returnStatus);
                           break;
                        }
                        mi.TerritoryCurrent = mim.NewTerritory;
                        mi.TerritoryStarting = mim.NewTerritory;
                        mi.IsMoved = false;
                        mi.MovementUsed = 0;
                     }
                     if ("OK" == returnStatus)
                     {
                        gi.MapItemMoves.Clear();
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                     }
                  }
               }
               else
               {
                  if (false == gi.IsAlienDisplayedRandomMovement)
                     gi.NextAction = "Awaiting Alien Display Random Movement";
                  else
                     gi.NextAction = "Awaiting Alien Ack Random Movement";
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
      public void RecordIncapacitatedPeople(IGameInstance gi)
      {
         foreach (Stack stack in gi.Stacks) // At the end of the turn, all stunned units become unstunned. All knocked out people become stunned.
         {
            foreach (MapItem mi in stack.MapItems)
            {
               if (true == mi.IsStunned)
                  gi.PersonsStunned.Add(mi); // Keep a list of which MapItems are Stunned.
               if (false == mi.IsUnconscious)
                  gi.PersonsKnockedOut.Add(mi); // Keep a list of which MapItems start the turn knocked out.
            }
         }
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
            case GameAction.ShowAlien:
               break;
            case GameAction.AlienMovement:
               if (0 < gi.MapItemMoves.Count)
               {
                  IMapItemMove? mim1 = gi.MapItemMoves[0];
                  if (null == mim1)
                  {
                     returnStatus = "mim1 is null in AlienMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateAlienPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     if (null == mim1.BestPath)
                     {
                        returnStatus = "mim1.BestPath is null in AlienMovement action";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateAlienPlayerMovement.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        mim1.MapItem.MovementUsed += mim1.BestPath.Territories.Count;
                        mim1.MapItem.IsMoved = true;
                     }
                  }
               }
               break;
            case GameAction.ResetMovement:
               break;
            case GameAction.AlienCompletesMovement:
               gi.NextAction = "Townsperson Acks Alien Movement";
               break;
            case GameAction.TownspersonAcksAlienMovement:
               foreach (IMapItem mi in gi.Townspeople)
               {
                  mi.TerritoryStarting = mi.TerritoryCurrent;
                  mi.IsMoved = false;
                  mi.MovementUsed = 0;
               }
               gi.MapItemMoves.Clear();
               gi.NextAction = "Townsperson Selects Counter to Move";
               gi.GamePhase = GamePhase.TownspersonMovement;
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
         bool isZebulonDiscovered = false; // out parameter to IsZebulonDiscovered()...need to initialize to compile
         switch (action)
         {
            case GameAction.ShowAlien:
               break;
            case GameAction.ResetMovement:
               gi.PreviousMapItemMove = null;
               break;
            case GameAction.TownpersonProposesMovement:
               bool isPossibleStopByAlien;
               if (false == ProposeTownMovement(gi, ref action, out isPossibleStopByAlien))
               {
                  returnStatus = "Propose_TownMovement() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.AlienTimeoutOnMovement:
               if (null == gi.PreviousMapItemMove)
               {
                  returnStatus = "gi.PreviousMapItemMove is null in AlienTimeoutOnMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.NextAction = "Townsperson Selects Counter to Move";
                  if (false == IsZebulonDiscovered(gi, gi.PreviousMapItemMove, out isZebulonDiscovered))
                  {
                     returnStatus = "IsZebulonDiscovered() returned false for " + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     if (true == isZebulonDiscovered)
                     {
                        action = GameAction.AlienModifiesTownspersonMovement;
                        gi.PreviousMapItemMove = null;
                     }
                     else
                     {
                        if (null != gi.PreviousMapItemMove)
                        {
                           if (null == gi.PreviousMapItemMove.BestPath)
                           {
                              returnStatus = "gi.PreviousMapItemMove.BestPath is null in AlienTimeoutOnMovement action";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                           }
                           else
                           {
                              foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                                 gi.ZebulonTerritories.Remove(t);
                           }
                        }
                        if (0 < gi.MapItemMoves.Count)
                        {
                           IMapItemMove? mim1 = gi.MapItemMoves[0];
                           if (null == mim1)
                           {
                              returnStatus = "mim1 is null in AlienTimeoutOnMovement action";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                           }
                           else
                           {
                              if (null == mim1.BestPath)
                              {
                                 returnStatus = "mim1.BestPath is null in AlienTimeoutOnMovement action";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                              }
                              else
                              {
                                 mim1.MapItem.IsMoved = true;
                                 mim1.MapItem.MovementUsed += mim1.BestPath.Territories.Count;
                                 gi.PreviousMapItemMove = mim1;
                              }
                           }
                        }
                     }
                  }
               }
               break;
            case GameAction.AlienModifiesTownspersonMovement:
               gi.NextAction = "Townsperson Selects Counter to Move";
               if (null == gi.PreviousMapItemMove)
               {
                  returnStatus = "gi.PreviousMapItemMove is null in AlienModifiesTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               else
               {
                  if (false == IsZebulonDiscovered(gi, gi.PreviousMapItemMove, out isZebulonDiscovered))
                  {
                     returnStatus = "IsZebulonDiscovered() returned false for " + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     if (true == isZebulonDiscovered)
                     {
                        action = GameAction.AlienModifiesTownspersonMovement;
                        gi.PreviousMapItemMove = null;
                     }
                     else
                     {
                        if (0 < gi.MapItemMoves.Count)
                        {
                           IMapItemMove? mim2 = gi.MapItemMoves[0];
                           if (null == mim2)
                           {
                              returnStatus = "mim2 is null in AlienModifiesTownspersonMovement action";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                           }
                           else
                           {
                              if (null == mim2.BestPath)
                              {
                                 returnStatus = "mim2.BestPath is null in AlienModifiesTownspersonMovement action";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                              }
                              else
                              {
                                 foreach (ITerritory t in mim2.BestPath.Territories)
                                    gi.ZebulonTerritories.Remove(t);
                                 mim2.MapItem.IsMoved = true;
                                 mim2.MapItem.MovementUsed += mim2.BestPath.Territories.Count;
                                 gi.PreviousMapItemMove = mim2;
                              }
                           }
                        }
                     }
                  }
               }

               break;
            case GameAction.TownpersonCompletesMovement:
               if (null == gi.PreviousMapItemMove)
               {
                  returnStatus = "gi.PreviousMapItemMove is null in AlienModifiesTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               else
               {
                  if (false == IsZebulonDiscovered(gi, gi.PreviousMapItemMove, out isZebulonDiscovered))
                  {
                     returnStatus = "IsZebulonDiscovered() returned false for " + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     if (true == isZebulonDiscovered)
                     {
                        gi.NextAction = "Townsperson Selects Counter to Move";
                        action = GameAction.AlienModifiesTownspersonMovement;
                     }
                     else if (null != gi.PreviousMapItemMove)
                     {
                        if (null == gi.PreviousMapItemMove.BestPath)
                        {
                           returnStatus = "gi.PreviousMapItemMove.BestPath is null in TownpersonCompletesMovement action";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           gi.NextAction = "Alien Acks Townspeople Movement";
                           foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                              gi.ZebulonTerritories.Remove(t);
                        }
                     }
                     gi.PreviousMapItemMove = null;
                  }
               }
               break;
            case GameAction.AlienAcksTownspersonMovement:
               foreach (IMapItem mi in gi.Townspeople)
               {
                  mi.TerritoryStarting = mi.TerritoryCurrent;
                  mi.IsMoved = false;
                  mi.MovementUsed = 0;
                  mi.IsMoveStoppedThisTurn = false;
               }
               gi.MapItemMoves.Clear();
               //-----------------------------------------------------
               bool isConversation;
               if (false == GameStateChecker.CheckForConversations(gi, out isConversation))
               {
                  returnStatus = "GameStateChecker.CheckForConversations() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isInfluence;
               if (false == GameStateChecker.CheckForInfluence(gi, out isInfluence))
               {
                  returnStatus = "GameStateChecker.CheckForInfluence() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isAlienCombat;
               if (false == GameStateChecker.CheckForAlienCombats(gi, out isAlienCombat))
               {
                  returnStatus = "GameStateChecker.CheckForAlienCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isTownspersonCombat;
               if (false == GameStateChecker.CheckForTownspersonCombats(gi, out isTownspersonCombat))
               {
                  returnStatus = "GameStateChecker.CheckForAlienCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               if ("OK" == returnStatus)
               {
                  //-----------------------------------------------------
                  if (true == isConversation)
                  {
                     gi.NextAction = "Townsperson Select Flashing Space";
                     gi.GamePhase = GamePhase.Conversations;
                  }
                  else if (true == isInfluence)
                  {
                     gi.NextAction = "Townsperson Select Flashing Space";
                     gi.GamePhase = GamePhase.Influences;
                  }
                  else if ((true == isTownspersonCombat) || (true == isAlienCombat))
                  {
                     gi.NextAction = "Each Player Select Flashing Space";
                     gi.GamePhase = GamePhase.Combat;
                  }
                  else if (true == GameStateChecker.CheckForIterogations(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                     gi.GamePhase = GamePhase.Iterrogations;
                  }
                  else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                     gi.GamePhase = GamePhase.ImplantRemoval;
                  }
                  else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                  {
                     gi.GamePhase = GamePhase.AlienTakeover;
                     gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                  }
                  else if (true == GameStateChecker.CheckForEndOfGame(gi))
                  {
                     action = GameAction.ShowEndGame;
                     gi.GamePhase = GamePhase.ShowEndGame;
                     gi.NextAction = "End Game";
                     gi.GameTurn = 13;
                  }
                  else
                  {
                     gi.NextAction = "Display Random Movement";
                     gi.GamePhase = GamePhase.RandomMovement;
                     gi.GameTurn++;
                  }
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
      private bool ProposeTownMovement(IGameInstance gi, ref GameAction outAction, out bool isPossibleStopByAlien)
      {
         // Based on the path taken by the moving MapItem, there 
         // may be no capability for the Alien to stop the movement.  In that event,
         // need to respond right away.  In that event, the returned action
         // is TownspersonMovement.
         isPossibleStopByAlien = false;
         if (null == gi.PreviousMapItemMove)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): gi.PreviousMapItemMove=null");
            return false;
         }
         if (null == gi.PreviousMapItemMove.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): gi.PreviousMapItemMove.BestPath=null");
            return false;
         }
         if (0 == gi.MapItemMoves.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): gi.MapItemMoves.Count=0");
            return false;
         }
         IMapItemMove? mim = gi.MapItemMoves[0];
         if (null == mim)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): mim=null");
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): mim.BestPath=null");
            return false;
         }
         if (null == mim.OldTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): mim.OldTerritory=null");
            return false;
         }
         //-------------------------------------------------
         foreach (IMapItem person in gi.Townspeople)
         {
            if ((false == person.IsWary) && (false == person.IsControlled) && (false == person.IsMoveStoppedThisTurn) && ("Zebulon" != person.Name)
                  && (false == person.IsStunned) && (false == person.IsTiedUp) && (false == person.IsSurrendered) && (false == person.IsKilled))
            {
               if ((person.TerritoryCurrent.Name == mim.OldTerritory.Name) && (person.TerritoryCurrent.Subname == mim.OldTerritory.Subname)) // Check if moving mapitem originates in territory controlled by alien
               {
                  Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Stop Move before started");
                  gi.NextAction = "Alien May Elect to Stop Move if Possible";
                  isPossibleStopByAlien = true;
                  return true;
               }
               for (int i = 0; i < mim.BestPath.Territories.Count - 1; ++i) // Check if moving mapitem originates in territory controlled by alien -- Do not check the last territory moved into
               {
                  ITerritory t = mim.BestPath.Territories[i];
                  if ((person.TerritoryCurrent.Name == t.Name) && (person.TerritoryCurrent.Subname == t.Subname))
                  {
                     Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Modify move");
                     gi.NextAction = "Alien May Elect to Stop Move if Possible";
                     return true;
                  }
               }
            }
            //-------------------------------------------------
            if (null == gi.PreviousMapItemMove)
            {
               Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): gi.PreviousMapItemMove=null");
               return false;
            }
            else
            {
               bool isZebulonDiscovered;
               if (false == IsZebulonDiscovered(gi, gi.PreviousMapItemMove, out isZebulonDiscovered))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): Is_ZebulonDiscovered() return false");
                  return false;
               }
               if (true == isZebulonDiscovered)
               {
                  outAction = GameAction.AlienModifiesTownspersonMovement;
                  gi.PreviousMapItemMove = null;
               }
               else
               {
                  if (null == gi.PreviousMapItemMove.BestPath)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Propose_TownMovement(): gi.PreviousMapItemMove.BestPath=null");
                     return false;
                  }
                  else
                  {
                     foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                        gi.ZebulonTerritories.Remove(t);
                     Logger.Log(LogEnum.LE_TIMER_ELAPED, "Continue Movement Without Waiting for Alien");
                     gi.NextAction = "Townsperson Selects Counter to Move";
                     outAction = GameAction.TownpersonMovement;
                     mim.MapItem.MovementUsed += mim.BestPath.Territories.Count;
                     mim.MapItem.IsMoved = true;
                     gi.PreviousMapItemMove = mim;
                  }
               }
            }
         }
         return true;
      }
      private bool IsZebulonDiscovered(IGameInstance gi, IMapItemMove mim, out bool isZebulanDiscovered)
      {
         isZebulanDiscovered = false;
         IMapItem? movingMi = gi.Townspeople.Find(mim.MapItem.Name);
         if (null == movingMi)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): movingMi=null for mim=" + mim.ToString());
            return false;
         }
         IMapItem? zebulon = gi.Townspeople.Find("Zebulon"); // Determine if Zebulon is along the path of this move. If so, back out to this new territory.
         if (null == zebulon)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): zebulon=null for mim=" + mim.ToString());
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): mim.BestPath=null for mim=" + mim.ToString());
            return false;
         }
         foreach (ITerritory t in mim.BestPath.Territories)
         {
            if (false == zebulon.IsAlienKnown) // Determine if Zebulon is discovered
            {
               if ((t.Name == zebulon.TerritoryCurrent.Name) && (t.Subname == zebulon.TerritoryCurrent.Subname))
               {
                  isZebulanDiscovered = true;
                  zebulon.IsAlienKnown = true;                     // Zebulon is now exposed
                  movingMi.TerritoryCurrent = movingMi.TerritoryStarting; // Back out to the old Territory
                  movingMi.IsMoveAllowedToResetThisTurn = false;
                  movingMi.IsMoved = true;
                  //-------------------------------------------
                  Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Is_ZebulonDiscovered(): mi=" + movingMi.Name + " entering t=" + zebulon.TerritoryCurrent);
                  if (false == CreateMapItemMove(gi, movingMi, zebulon.TerritoryCurrent))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): Create_MapItemMove() returned false");
                     return false;
                  }
                  break;
               }
            }
         }
         return true;
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
            case GameAction.ShowAlien:
               break;
            case GameAction.TownspersonPerformsConversation:
               break;
            case GameAction.TownspersonCompletesConversations:
               //-----------------------------------------------------
               bool isInfluence;
               if (false == GameStateChecker.CheckForInfluence(gi, out isInfluence))
               {
                  returnStatus = "GameStateChecker.CheckForInfluence() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isAlienCombat;
               if (false == GameStateChecker.CheckForAlienCombats(gi, out isAlienCombat))
               {
                  returnStatus = "GameStateChecker.CheckForAlienCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isTownspersonCombat;
               if (false == GameStateChecker.CheckForTownspersonCombats(gi, out isTownspersonCombat))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isAnyMovement;
               if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------------
               if ("OK" == returnStatus)
               {
                  if (true == isInfluence)
                  {
                     gi.NextAction = "Townsperson Select Flashing Space";
                     gi.GamePhase = GamePhase.Influences;
                  }
                  else if ((true == isTownspersonCombat) || (true == isAlienCombat))
                  {
                     gi.NextAction = "Decides Where to Perform Combats";
                     gi.GamePhase = GamePhase.Combat;
                  }
                  else if (true == GameStateChecker.CheckForIterogations(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                     gi.GamePhase = GamePhase.Iterrogations;
                  }
                  else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                     gi.GamePhase = GamePhase.ImplantRemoval;
                  }
                  else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                  {
                     gi.GamePhase = GamePhase.AlienTakeover;
                     gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                  }
                  else if (true == GameStateChecker.CheckForEndOfGame(gi))
                  {
                     action = GameAction.ShowEndGame;
                     gi.GamePhase = GamePhase.ShowEndGame;
                     gi.NextAction = "End Game";
                     gi.GameTurn = 13;
                  }
                  else if (true == isAnyMovement)
                  {
                     gi.NextAction = "Display Random Movement";
                     gi.GamePhase = GamePhase.RandomMovement;
                  }
                  else
                  {
                     gi.NextAction = "Alien Performs Movement";
                     gi.GamePhase = GamePhase.AlienMovement;
                  }
               }
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
            case GameAction.ShowAlien:
               break;
            case GameAction.TownspersonPerformsInfluencing:
               break;
            case GameAction.TownspersonCompletesInfluencing:
               bool isAlienCombat;
               if (false == GameStateChecker.CheckForAlienCombats(gi, out isAlienCombat))
               {
                  returnStatus = "GameStateChecker.CheckForAlienCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isTownspersonCombat;
               if (false == GameStateChecker.CheckForTownspersonCombats(gi, out isTownspersonCombat))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               bool isAnyMovement;
               if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------------
               if ("OK" == returnStatus)
               {
                  if ((true == isTownspersonCombat) || (true == isAlienCombat))
                  {
                     gi.NextAction = "Decides Where to Perform Combats";
                     gi.GamePhase = GamePhase.Combat;
                  }
                  else if (true == GameStateChecker.CheckForIterogations(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                     gi.GamePhase = GamePhase.Iterrogations;
                  }
                  else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                  {
                     gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                     gi.GamePhase = GamePhase.ImplantRemoval;
                  }
                  else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                  {
                     gi.GamePhase = GamePhase.AlienTakeover;
                     gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                  }
                  else if (true == GameStateChecker.CheckForEndOfGame(gi))
                  {
                     action = GameAction.ShowEndGame;
                     gi.GamePhase = GamePhase.ShowEndGame;
                     gi.NextAction = "End Game";
                     gi.GameTurn = 13;
                  }
                  else if (true == isAnyMovement)
                  {
                     gi.NextAction = "Display Random Movement";
                     gi.GamePhase = GamePhase.RandomMovement;
                  }
                  else
                  {
                     gi.NextAction = "Alien Performs Movement";
                     gi.GamePhase = GamePhase.AlienMovement;
                  }
               }
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
            case GameAction.ShowAlien:
               break;
            case GameAction.AlienInitiateCombat:
               if (true == gi.IsAlienInitiatedCombat)
                  action = GameAction.TownspersonNackCombatSelection;
               if ((false == gi.IsAlienInitiatedCombat) && (false == gi.IsControlledInitiatedCombat))
               {
                  gi.IsAlienInitiatedCombat = true;
                  gi.MapItemMoves.Clear();    // Clear any previous retreats
                  gi.NextAction = "Alien Initiated Combat";
               }
               else
               {
                  action = GameAction.TownspersonNackCombatSelection;
               }
               break;
            case GameAction.TownspersonInitiateCombat:
               if (true == gi.IsControlledCombatCompleted)
                  action = GameAction.AlienNackCombatSelection;
               if ((false == gi.IsAlienInitiatedCombat) && (false == gi.IsControlledInitiatedCombat))
               {
                  gi.IsControlledInitiatedCombat = true;
                  gi.MapItemMoves.Clear();    // Clear any previous retreats
                  gi.NextAction = "Townsperson Initiated Combat";
               }
               else
               {
                  action = GameAction.AlienNackCombatSelection;
               }
               break;
            case GameAction.AlienPerformCombat:
               PerformCombat(gi);
               gi.IsAlienInitiatedCombat = false;
               gi.NextAction = "Select Flashing Region to Initiate Combat";
               IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
               if (null == zebulon)
               {
                  returnStatus = "Could not find Zebulon in gi.Stacks in AlienPerformCombat action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
               }
               else
               {
                  bool isAlienCombat;
                  if (false == GameStateChecker.CheckForAlienCombats(gi, out isAlienCombat))
                  {
                     returnStatus = "GameStateChecker.CheckForAlienCombats() returned false in AlienAcksTownspersonMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  bool isAnyMovement;
                  if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
                  {
                     returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  //-----------------------------------------------------
                  if ("OK" == returnStatus)
                  {
                     if (true == zebulon.IsKilled)
                     {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = 13;
                     }
                     else if ((true == gi.IsControlledCombatCompleted) && (false == isAlienCombat))
                     {
                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                           gi.NextAction = "Townsperson chooses Black Building Space";
                           gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                           gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                           gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                           gi.GamePhase = GamePhase.AlienTakeover;
                           gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                           action = GameAction.ShowEndGame;
                           gi.GamePhase = GamePhase.ShowEndGame;
                           gi.NextAction = "End Game";
                           gi.GameTurn = 13;
                        }
                        else if (true == isAnyMovement)
                        {
                           gi.NextAction = "Display Random Movement";
                           gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                           gi.NextAction = "Alien Performs Movement";
                           gi.GamePhase = GamePhase.AlienMovement;
                        }
                     }
                  }
               }
               break;
            case GameAction.TownspersonPerformCombat:
               PerformCombat(gi);
               gi.IsControlledInitiatedCombat = false;
               IMapItem? zebulon1 = gi.Stacks.FindMapItem("Zebulon");
               if (null == zebulon1)
               {
                  returnStatus = "Could not find Zebulon in gi.Stacks in AlienPerformCombat action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
               }
               else
               {
                  bool isTownspersonCombat;
                  if (false == GameStateChecker.CheckForTownspersonCombats(gi, out isTownspersonCombat))
                  {
                     returnStatus = "CheckForTownspersonCombats() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
                  }
                  bool isAnyMovement;
                  if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
                  {
                     returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  //-----------------------------------------------------
                  if ("OK" == returnStatus)
                  {
                     if (true == zebulon1.IsKilled)
                     {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = 13;
                     }
                     else if ((true == gi.IsAlienCombatCompleted) && (false == isTownspersonCombat))
                     {
                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                           gi.NextAction = "Townsperson chooses Black Building Space";
                           gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                           gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                           gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                           gi.GamePhase = GamePhase.AlienTakeover;
                           gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                           action = GameAction.ShowEndGame;
                           gi.GamePhase = GamePhase.ShowEndGame;
                           gi.NextAction = "End Game";
                           gi.GameTurn = 13;
                        }
                        else if (true == isAnyMovement)
                        {
                           gi.NextAction = "Display Random Movement";
                           gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                           gi.NextAction = "Alien Performs Movement";
                           gi.GamePhase = GamePhase.AlienMovement;
                        }
                     }
                  }
               }
               break;
            case GameAction.TownspersonCompletesCombat:
               gi.IsControlledCombatCompleted = true;
               gi.IsControlledInitiatedCombat = false;
               if (true == gi.IsAlienCombatCompleted)
               {
                  gi.IsAlienCombatCompleted = false;
                  gi.IsControlledCombatCompleted = false;
                  foreach (IMapItemMove mim in gi.MapItemMoves)
                  {
                     IMapItem mi = mim.MapItem;
                     if (null == mim.NewTerritory)
                     {
                        returnStatus = "mim.NewTerritory is null in TownpersonCompletesCombat action";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        mi.TerritoryCurrent = mim.NewTerritory;
                        mi.TerritoryStarting = mim.NewTerritory;
                        mi.IsMoved = false;
                        mi.MovementUsed = 0;
                     }
                  }
                  gi.MapItemMoves.Clear();
                  bool isAnyMovement;
                  if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
                  {
                     returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  //-----------------------------------------------------
                  if ("OK" == returnStatus)
                  {
                     if (true == GameStateChecker.CheckForIterogations(gi))
                     {
                        gi.NextAction = "Townsperson chooses Black Building Space";
                        gi.GamePhase = GamePhase.Iterrogations;
                     }
                     else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                     {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                     }
                     else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                     {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                     }
                     else if (true == GameStateChecker.CheckForEndOfGame(gi))
                     {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = 13;
                     }
                     else if (true == isAnyMovement)
                     {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                     }
                     else
                     {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                     }
                  }
               }
               else
               {
                  gi.NextAction = "Awaiting Alien Complete Combat";
               }
               break;
            case GameAction.AlienCompletesCombat:
               gi.IsAlienCombatCompleted = true;
               gi.IsAlienInitiatedCombat = false;
               if (true == gi.IsControlledCombatCompleted)
               {
                  foreach (IMapItemMove mim in gi.MapItemMoves)
                  {
                     IMapItem mi = mim.MapItem;
                     if (null == mim.NewTerritory)
                     {
                        returnStatus = "mim.NewTerritory is null in AlienCompletesCombat action";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        mi.TerritoryCurrent = mim.NewTerritory;
                        mi.TerritoryStarting = mim.NewTerritory;
                        mi.IsMoved = false;
                        mi.MovementUsed = 0;
                     }
                  }
                  gi.MapItemMoves.Clear();
                  gi.MapItemMoves.Clear();
                  //-----------------------------------------------------
                  bool isAnyMovement;
                  if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
                  {
                     returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
                  }
                  //-----------------------------------------------------
                  if ("OK" == returnStatus)
                  {
                     gi.IsAlienCombatCompleted = false;
                     gi.IsControlledCombatCompleted = false;
                     if (true == GameStateChecker.CheckForIterogations(gi))
                     {
                        gi.NextAction = "Townsperson chooses Black Building Space";
                        gi.GamePhase = GamePhase.Iterrogations;
                     }
                     else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                     {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                     }
                     else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                     {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                     }
                     else if (true == GameStateChecker.CheckForEndOfGame(gi))
                     {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = 13;
                     }
                     else if (true == isAnyMovement)
                     {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                     }
                     else
                     {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                     }
                  }
               }
               else
               {
                  gi.NextAction = "Awaiting Townsperson Complete Combat";
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
                                       // In each stack, get the count in the stack of the number of aliens 
                                       // and controlled townspeople
         IMapItems aliens = new MapItems();
         IMapItems controlled = new MapItems();
         IMapItems uncontrolled = new MapItems();
         IMapItems wary = new MapItems();
         foreach (MapItem mi in gi.Townspeople)
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
                  if (false == gi.AddKnownAlien(mi)) // All aliens in this combat become exposed.
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): AddKnownAlien() returned false");
                     return false;
                  }
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
         //-------------------------------------------------------
         if (0 == controlled.Count) // If there is no combat, return from this method
         {
            if ((0 == aliens.Count) || (0 == wary.Count))
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): aliens.Count=0 wary.Count=0 controlled.Count=0");
               return false;
            }
         }
         // Determine the attack strength of the aliens.  
         // Limit it to top three counters.
         int alienAttackCombat = 0;
         int alienCount = 0;
         aliens = aliens.SortOnCombat();
         foreach (IMapItem alien in aliens)
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
                  if (false == PerformMovement(gi, attacker))
                     Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", attacker.Name);
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
                  if (false == PerformMovement(gi, defender))
                  {

                  }
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
               if (false == PerformMovement(gi, mi))
                  Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", mi.Name);
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
            case GameAction.ShowAlien:
               break;
            case GameAction.TownspersonIterrogates:
               if (0 == gi.NumIterogationsThisTurn)
               {
                  gi.NextAction = "Alien Acknowledges Iterogations";
                  action = GameAction.TownspersonCompletesIterogations;
               }
               break;
            case GameAction.TownspersonCompletesIterogations:
               gi.NextAction = "Alien Acknowledges Iterogations";
               break;
            case GameAction.AlienAcksIterogations:
               bool isAnyMovement;
               if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------------
               if (true == GameStateChecker.CheckForImplantRemoval(gi))
               {
                  gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                  gi.GamePhase = GamePhase.ImplantRemoval;
               }
               else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
               {
                  gi.GamePhase = GamePhase.AlienTakeover;
                  gi.NextAction = "Alien Chooses Flashing Space for Takeover";
               }
               else if (true == GameStateChecker.CheckForEndOfGame(gi))
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = 13;
               }
               else if (true == isAnyMovement)
               {
                  gi.NextAction = "Display Random Movement";
                  gi.GamePhase = GamePhase.RandomMovement;
               }
               else
               {
                  gi.NextAction = "Alien Performs Movement";
                  gi.GamePhase = GamePhase.AlienMovement;
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
            case GameAction.TownspersonCompletesInfluencing:
               break;
            case GameAction.ShowAlien:
               break;
            case GameAction.TownspersonCompletesRemoval:
               bool isAnyMovement;
               if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------------
               if (true == GameStateChecker.CheckForAlienTakeovers(gi))
               {
                  gi.GamePhase = GamePhase.AlienTakeover;
                  gi.NextAction = "Alien Chooses Flashing Space for Takeover";
               }
               else if (true == GameStateChecker.CheckForEndOfGame(gi))
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = 13;
               }
               else if (true == isAnyMovement)
               {
                  gi.NextAction = "Display Random Movement";
                  gi.GamePhase = GamePhase.RandomMovement;
               }
               else
               {
                  gi.NextAction = "Alien Performs Movement";
                  gi.GamePhase = GamePhase.AlienMovement;
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
            case GameAction.ShowAlien:
               break;
            case GameAction.AlienTakeover:
               returnStatus = PerformTakeover(ref gi);
               break;
            case GameAction.AlienCompletesTakeovers:
               if (true == GameStateChecker.CheckForEndOfGame(gi))
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = 13;
               }
               bool isAnyMovement;
               if (false == GameStateChecker.CheckForRandomMoves(gi, out isAnyMovement))
               {
                  returnStatus = "GameStateChecker.CheckForTownspersonCombats() returned false in AlienAcksTownspersonMovement action";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               //-----------------------------------------------------
               if ("OK" == returnStatus)
               {
                  if (true == isAnyMovement)
                  {
                     gi.NextAction = "Display Random Movement";
                     gi.GamePhase = GamePhase.RandomMovement;
                  }
                  else
                  {
                     gi.NextAction = "Alien Performs Movement";
                     gi.GamePhase = GamePhase.AlienMovement;
                  }
               }
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
      private string PerformTakeover(ref IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         if (null == gi.Takeover)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): takeover = null ");
            return "PerformTakeover() ERROR";
         }
         if (null == gi.Takeover.Alien)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): Alien = null ");
            return "PerformTakeover() ERROR";
         }
         if (null == gi.Takeover.Uncontrolled)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): Uncontrolled = null ");
            return "PerformTakeover() ERROR";
         }
         // Determine if there are any observations.  If so, create a string to hold who and with what roll the observation happened.  
         foreach (String observation in gi.Takeover.Alien.TerritoryCurrent.Observations)
         {
            IStack? stack = gi.Stacks.Find(observation);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTakeover(): stack is null for observation=" + observation);
               return "ERROR";
            }
            ITerritory? obsTerritory = Territories.theTerritories.Find(observation);
            if (null == obsTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTakeover(): obsTerritory is null for observation=" + observation);
               return "ERROR";
            }
            IMapPath? path = Territory.GetBestPath(Territories.theTerritories, gi.Takeover.Alien.TerritoryCurrent, obsTerritory, 3); // Get distance between two territories
            if (null == path)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTakeover(): path is null for observation=" + observation);
               return "ERROR";
            }
            foreach (IMapItem person in stack.MapItems)
            {
               if (gi.Takeover.Uncontrolled.Name == person.Name)
                  continue;
               if ((true == person.IsWary) || (true == person.IsAlienKnown) || (true == person.IsAlienUnknown) || (false == person.IsUnconscious) || (true == person.IsStunned) || (true == person.IsKilled))
                  continue;
               int dieRoll = Utilities.RandomGenerator.Next(6) + 1;
               switch (path.Territories.Count)
               {
                  case 0:
                     if (dieRoll < 5)
                     {
                        person.IsWary = true;
                        person.IsSkeptical = false;  // wary people are never skeptical
                        sb.Append(person.Name);
                        sb.Append(" observed with a die roll = ");
                        sb.Append(dieRoll.ToString());
                        sb.Append("\n");
                     }
                     break;
                  case 1:
                     if (dieRoll < 4)
                     {
                        person.IsWary = true;
                        person.IsSkeptical = false;  // wary people are never skeptical
                        sb.Append(person.Name);
                        sb.Append(" observed with a die roll = ");
                        sb.Append(dieRoll.ToString());
                        sb.Append("\n");
                     }
                     break;
                  case 2:
                     if (dieRoll < 3)
                     {
                        person.IsWary = true;
                        person.IsSkeptical = false;  // wary people are never skeptical
                        sb.Append(person.Name);
                        sb.Append(" observed with a die roll = ");
                        sb.Append(dieRoll.ToString());
                        sb.Append("\n");
                     }
                     break;
                  case 3:
                     if (dieRoll < 2)
                     {
                        person.IsWary = true;
                        person.IsSkeptical = false;  // wary people are never skeptical
                        sb.Append(person.Name);
                        sb.Append(" observed with a die roll = ");
                        sb.Append(dieRoll.ToString());
                        sb.Append("\n");
                     }
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "PerformTakeover(): reached default");
                     return "PerformTakeover() ERROR";
               } // end switch
            }
         }  //  end foreach (String observation in gi.Takeover.Alien.Territory.Observations)
         gi.Takeover.Observations = sb.ToString();
         if (0 == gi.Takeover.Observations.Count())
         {
            gi.Takeover.Observations = "Nobody Noticed";
            if ((true == gi.Takeover.Uncontrolled.IsControlled) || (true == gi.Takeover.Uncontrolled.IsWary))
            {
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS, "PerformTakeover(): Taking over controlled or wary ==> " + gi.Takeover.ToString());
               if (false == gi.AddKnownAlien(gi.Takeover.Alien))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()1 returned error for " + gi.Takeover.Alien.Name);
                  return "PerformTakeover() ERROR";
               }
               if (false == gi.AddKnownAlien(gi.Takeover.Uncontrolled))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()2 returned error for " + gi.Takeover.Uncontrolled.Name);
                  return "PerformTakeover() ERROR";
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled without notice ==> " + gi.Takeover.ToString());
               if (false == gi.AddUnknownAlien(gi.Takeover.Uncontrolled))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()3 returned error for " + gi.Takeover.Uncontrolled.Name);
                  return "PerformTakeover() ERROR";
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled w/ observation ==> " + gi.Takeover.ToString());
            if (false == gi.AddKnownAlien(gi.Takeover.Alien))
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()4 returned error for " + gi.Takeover.Alien.Name);
               return "PerformTakeover() ERROR";
            }
            if (false == gi.AddKnownAlien(gi.Takeover.Uncontrolled))
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()5 returned error for " + gi.Takeover.Uncontrolled.Name);
               return "PerformTakeover() ERROR";
            }
         }
         return "OK";
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
            case GameAction.ShowAlien:
               break;
            case GameAction.ShowEndGame:
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
