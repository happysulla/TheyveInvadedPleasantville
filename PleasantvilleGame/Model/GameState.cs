using System;
using System.Text;
using System.Windows;

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
            default: return new GameStateEnded();
         }
      }
      public bool PerformMovement(IGameInstance gi, IMapItem mi)
      {
         int r3 = Utilities.RandomGenerator.Next(5);
         int r4 = Utilities.RandomGenerator.Next(6);
         string building = Utilities.RemoveSpaces(Constants.targetBuildingTable[r3, r4]); // Find the target building location.
         //-----------------------------------------
         int numOfSectorsInBuilding = 0;
         for (int i1 = 0; i1 < Constants.buildingSizes.GetLength(0); i1++)   // If moving to a build, randomly select a space from the building. GetLength(0) gets the length of the array.
         {
            string buildingToCompare = Utilities.RemoveSpaces(Constants.buildingSizes[i1, 0]);
            if (buildingToCompare == building)
            {
               numOfSectorsInBuilding = Int32.Parse(Constants.buildingSizes[i1, 1]);
               break;
            }
         }
         int selectedSector = Utilities.RandomGenerator.Next(numOfSectorsInBuilding);
         ++selectedSector;
         ITerritory? newTerritory = Territories.theTerritories.Find(building, selectedSector);
         if (null == newTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformMovement(): newTerritory is null for building=" + building + " selectedSector=" + selectedSector.ToString());
            return false;
         }
         if ((mi.Territory.Name == newTerritory.Name) && (mi.Territory.Sector == newTerritory.Sector))
         {
            return false;
         }
         //-----------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "Move_TaskForceToNewArea(): mi=" + mi.Name + " entering t=" + newTerritory.Name);
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
            string person = Utilities.RemoveSpaces(Constants.townsPersonTable[r1, r2]);
            IMapItem personMoving = gi.Persons.Find(person);
            if (null == personMoving)
            {
               ++numPeopleSkipped;
               //Console.WriteLine("PerformMovements(): {0} Unknown {1}", numPeopleSkipped.ToString(), person);
               continue;
            }

            // If the counter is moved or tied up or known to be alien controlled, do not move.
            if ((true == personMoving.IsMoved) || (true == personMoving.IsControlled) || (true == personMoving.IsAlienKnown) ||
                (true == personMoving.IsStunned) || (true == personMoving.IsSurrendered) ||
                (true == personMoving.IsTiedUp) || (true == personMoving.IsWary) || (false == personMoving.IsConscious))
            {
               ++numPeopleSkipped;
               //                    Console.WriteLine("PerformMovements(): {0} Skipping Person {1}: {2},{3},{4},{5},{6},{7},{8},{9}",
               //                                       numPeopleSkipped.ToString(),
               //                                       personMoving.Name,
               //                                       personMoving.IsMoved.ToString(),
               //                                       personMoving.IsControlled.ToString(), personMoving.IsAlienKnown.ToString(),
               //                                       personMoving.IsStunned.ToString(), personMoving.IsSurrendered.ToString(),
               //                                       personMoving.IsTiedUp.ToString(), personMoving.IsWary.ToString(),
               //                                       personMoving.IsConscious.ToString());
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
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.Name);
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
            #region ShowAlien
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
                     mi.Territory = mim.NewTerritory;
                     mi.TerritoryStarting = mim.NewTerritory;
                     mi.IsMoved = false;
                     mi.MovementUsed = 0;
                  }
                  gi.MapItemMoves.Clear();
                  gi.MapItemCombat.IsAnyRetreat = false;
                  gi.Takeover = null;
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
                  gi.MapItemCombat.IsAnyRetreat = false;
                  foreach (IMapItemMove mim in gi.MapItemMoves)
                  {
                     IMapItem mi = mim.MapItem;
                     mi.Territory = mim.NewTerritory;
                     mi.TerritoryStarting = mim.NewTerritory;
                     mi.IsMoved = false;
                     mi.MovementUsed = 0;
                  }
                  gi.MapItemMoves.Clear();

                  gi.NextAction = "Alien Performs Movement";
                  gi.GamePhase = GamePhase.AlienMovement;

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
               if (false == mi.IsConscious)
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region AlienMovement
            case GameAction.AlienMovement:
               if (0 < gi.MapItemMoves.Count)
               {
                  IMapItemMove mim1 = gi.MapItemMoves[0];
                  IMapItem movingMi1 = gi.Persons.Find(mim1.MapItem.Name);
                  movingMi1.MovementUsed += mim1.BestPath.Territories.Count;
                  movingMi1.IsMoved = true;
               }
               break;
            #endregion

            #region ResetMovement
            case GameAction.ResetMovement:
               break;
            #endregion

            #region AlienCompletesMovement
            case GameAction.AlienCompletesMovement:
               gi.NextAction = "Townsperson Acks Alien Movement";
               break;
            #endregion

            #region TownspersonAcksAlienMovement
            case GameAction.TownspersonAcksAlienMovement:
               foreach (IMapItem mi in gi.Persons)
               {
                  mi.TerritoryStarting = mi.Territory;
                  mi.IsMoved = false;
                  mi.MovementUsed = 0;
               }
               gi.MapItemMoves.Clear();
               gi.NextAction = "Townsperson Selects Counter to Move";
               gi.GamePhase = GamePhase.TownspersonMovement;
               break;
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateAlienPlayerMovement.PerformAction(): " + returnStatus);
               break;
               #endregion
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            case GameAction.ResetMovement:
               gi.PreviousMapItemMove = null;
               break;
            case GameAction.TownpersonProposesMovement:
               if( false == ProposeTownMovement(gi, ref action) )
               {
                  returnStatus = "Propose_TownMovement() returned false for " + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateTownPlayerMovement.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.AlienTimeoutOnMovement:
               gi.NextAction = "Townsperson Selects Counter to Move";
               if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
               {
                  action = GameAction.AlienModifiesTownspersonMovement;
                  gi.PreviousMapItemMove = null;
               }
               else
               {
                  if (null != gi.PreviousMapItemMove)
                  {
                     foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                        gi.ZebulonTerritories.Remove(t);
                  }
                  if (0 < gi.MapItemMoves.Count)
                  {
                     IMapItemMove mim1 = gi.MapItemMoves[0];
                     IMapItem movingMi1 = gi.Persons.Find(mim1.MapItem.Name);
                     movingMi1.IsMoved = true;
                     movingMi1.MovementUsed += mim1.BestPath.Territories.Count;
                     gi.PreviousMapItemMove = mim1;
                  }
               }
               break;
            case GameAction.AlienModifiesTownspersonMovement:
               gi.NextAction = "Townsperson Selects Counter to Move";
               if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
               {
                  action = GameAction.AlienModifiesTownspersonMovement;
                  gi.PreviousMapItemMove = null;
               }
               else
               {
                  if (0 < gi.MapItemMoves.Count)
                  {
                     IMapItemMove mim2 = gi.MapItemMoves[0];
                     IMapItem movingMi2 = gi.Persons.Find(mim2.MapItem.Name);
                     movingMi2.IsMoveAllowedToResetThisTurn = false;
                     movingMi2.MovementUsed += mim2.BestPath.Territories.Count;
                     movingMi2.IsMoved = true;
                     if (null != gi.PreviousMapItemMove)
                     {
                        foreach (ITerritory t in mim2.BestPath.Territories)
                           gi.ZebulonTerritories.Remove(t);
                     }
                     gi.PreviousMapItemMove = null;
                  }
               }

               break;
            case GameAction.TownpersonCompletesMovement:
               if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
               {
                  gi.NextAction = "Townsperson Selects Counter to Move";
                  action = GameAction.AlienModifiesTownspersonMovement;
               }
               else if (null != gi.PreviousMapItemMove)
               {
                  gi.NextAction = "Alien Acks Townspeople Movement";
                  foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                     gi.ZebulonTerritories.Remove(t);
               }
               gi.PreviousMapItemMove = null;
               break;
            case GameAction.AlienAcksTownspersonMovement:
               foreach (IMapItem mi in gi.Persons)
               {
                  mi.TerritoryStarting = mi.Territory;
                  mi.IsMoved = false;
                  mi.MovementUsed = 0;
                  mi.IsMoveStoppedThisTurn = false;
               }
               gi.MapItemMoves.Clear();

               if (true == GameStateChecker.CheckForConversations(gi))
               {
                  gi.NextAction = "Townsperson Select Flashing Space";
                  gi.GamePhase = GamePhase.Conversations;
               }
               else if (true == GameStateChecker.CheckForInfluence(gi))
               {
                  gi.NextAction = "Townsperson Select Flashing Space";
                  gi.GamePhase = GamePhase.Influences;
               }
               else if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
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
      private bool ProposeTownMovement(IGameInstance gi, ref GameAction outAction)
      {
         // Based on the path taken by the moving MapItem, there 
         // may be no capability for the Alien to stop the movement.  In that event,
         // need to respond right away.  In that event, the returned action
         // is TownspersonMovement.
         if (0 < gi.MapItemMoves.Count)
         {
            IMapItemMove? mim = gi.MapItemMoves[0];
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_TownMovement(): mim=null");
               return false;
            }
            IMapItem? movingMi = gi.Persons.Find(mim.MapItem.Name);
            if (null == movingMi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_TownMovement(): movingMi=null");
               return false;
            }
            //-------------------------------------------------
            foreach (IMapItem person in gi.Persons)
            {

               if ((false == person.IsWary) && (false == person.IsControlled) && (false == person.IsMoveStoppedThisTurn) && ("Zebulon" != person.Name)
                     && (false == person.IsStunned) && (false == person.IsTiedUp) && (false == person.IsSurrendered) && (false == person.IsKilled))
               {
                  if ((person.Territory.Name == mim.OldTerritory.Name) && (person.Territory.Sector == mim.OldTerritory.Sector)) // Check if moving mapitem originates in territory controlled by alien
                  {
                     Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Stop Move before started");
                     gi.NextAction = "Alien May Elect to Stop Move if Possible";
                     return true;
                  }
                  for (int i = 0; i < mim.BestPath.Territories.Count - 1; ++i) // Check if moving mapitem originates in territory controlled by alien -- Do not check the last territory moved into
                  {
                     ITerritory t = mim.BestPath.Territories[i];
                     if ((person.Territory.Name == t.Name) && (person.Territory.Sector == t.Sector))
                     {
                        Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Modify move");
                        gi.NextAction = "Alien May Elect to Stop Move if Possible";
                        return true;
                     }
                  }
               }
               //-------------------------------------------------
               bool isZebulonDiscovered;
               if (false == IsZebulonDiscovered(gi, gi.PreviousMapItemMove, out isZebulonDiscovered))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Perform_TownMovement(): Is_ZebulonDiscovered() return false");
                  return false;
               }
               if (true == isZebulonDiscovered)
               {
                  outAction = GameAction.AlienModifiesTownspersonMovement;
                  gi.PreviousMapItemMove = null;
               }
               else
               {
                  if (null != gi.PreviousMapItemMove)
                  {
                     foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                        gi.ZebulonTerritories.Remove(t);
                  }
                  Logger.Log(LogEnum.LE_TIMER_ELAPED, "Continue Movement Without Waiting for Alien");
                  gi.NextAction = "Townsperson Selects Counter to Move";
                  outAction = GameAction.TownpersonMovement;
                  movingMi.MovementUsed += mim.BestPath.Territories.Count;
                  movingMi.IsMoved = true;
                  gi.PreviousMapItemMove = mim;
               }
            }
         }
         return true;
      }
      private bool IsZebulonDiscovered(IGameInstance gi, IMapItemMove mim, out bool isZebulanDiscovered)
      {
         isZebulanDiscovered = false;
         IMapItem? movingMi = gi.Persons.Find(mim.MapItem.Name);
         if (null == movingMi)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): movingMi=null for mim=" + mim.ToString());
            return false;
         }
         IMapItem? zebulon = gi.Persons.Find("Zebulon"); // Determine if Zebulon is along the path of this move. If so, back out to this new territory.
         if (null == zebulon)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): zebulon=null for mim=" + mim.ToString());
            return false;
         }
         if( null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Is_ZebulonDiscovered(): mim.BestPath=null for mim=" + mim.ToString());
            return false;
         }
         foreach (ITerritory t in mim.BestPath.Territories)
         {
            if (false == zebulon.IsAlienKnown) // Determine if Zebulon is discovered
            {
               if ((t.Name == zebulon.Territory.Name) && (t.Sector == zebulon.Territory.Sector))
               {
                  isZebulanDiscovered = true;
                  zebulon.IsAlienKnown = true;                     // Zebulon is now exposed
                  movingMi.TerritoryCurrent = movingMi.TerritoryStarting; // Back out to the old Territory
                  movingMi.IsMoveAllowedToResetThisTurn = false;
                  movingMi.IsMoved = true;
                  //-------------------------------------------
                  Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "Is_ZebulonDiscovered(): mi=" + movingMi.Name + " entering t=" + zebulon.TerritoryCurrent);
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region TownspersonPerformsConversation
            case GameAction.TownspersonPerformsConversation:
               break;
            #endregion

            #region TownspersonCompletesConversations
            case GameAction.TownspersonCompletesConversations:
               if (true == GameStateChecker.CheckForInfluence(gi))
               {
                  gi.NextAction = "Townsperson Select Flashing Space";
                  gi.GamePhase = GamePhase.Influences;
               }
               else if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
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
               else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateConversations.PerformAction(): " + returnStatus);
               break;
               #endregion
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region TownspersonPerformsInfluencing
            case GameAction.TownspersonPerformsInfluencing:
               break;
            #endregion

            #region TownspersonCompletesInfluencing
            case GameAction.TownspersonCompletesInfluencing:
               if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
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
               else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateInfluences.PerformAction(): " + returnStatus);
               break;
               #endregion
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region AlienInitiateCombat
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
            #endregion

            #region TownspersonInitiateCombat
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
            #endregion

            #region AlienPerformCombat
            case GameAction.AlienPerformCombat:
               PerformCombat(gi);
               gi.IsAlienInitiatedCombat = false;
               gi.NextAction = "Select Flashing Region to Initiate Combat";
               IMapItem zebulon = gi.Persons.Find("Zebulon");
               if (true == zebulon.IsKilled)
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = 13;
               }
               else if ((true == gi.IsControlledCombatCompleted) && ((false == GameStateChecker.CheckForAlienCombats(gi))))
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
                  else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region TownspersonPerformCombat
            case GameAction.TownspersonPerformCombat:
               PerformCombat(gi);
               gi.IsControlledInitiatedCombat = false;
               IMapItem zebulon1 = gi.Persons.Find("Zebulon");
               if (true == zebulon1.IsKilled)
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = 13;
               }
               else if ((true == gi.IsAlienCombatCompleted) && ((false == GameStateChecker.CheckForTownspersonCombats(gi))))
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
                  else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region TownspersonCompletesCombat
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
                     mi.Territory = mim.NewTerritory;
                     mi.TerritoryStarting = mim.NewTerritory;
                     mi.IsMoved = false;
                     mi.MovementUsed = 0;
                  }
                  gi.MapItemMoves.Clear();

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
                  else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
               else
               {
                  gi.NextAction = "Awaiting Alien Complete Combat";
               }
               break;
            #endregion

            #region AlienCompletesCombat
            case GameAction.AlienCompletesCombat:
               gi.IsAlienCombatCompleted = true;
               gi.IsAlienInitiatedCombat = false;
               if (true == gi.IsControlledCombatCompleted)
               {
                  foreach (IMapItemMove mim in gi.MapItemMoves)
                  {
                     IMapItem mi = mim.MapItem;
                     mi.Territory = mim.NewTerritory;
                     mi.TerritoryStarting = mim.NewTerritory;
                     mi.IsMoved = false;
                     mi.MovementUsed = 0;
                  }
                  gi.MapItemMoves.Clear();

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
                  else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
               else
               {
                  gi.NextAction = "Awaiting Townsperson Complete Combat";
               }
               break;
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateCombat.PerformAction(): " + returnStatus);
               break;
               #endregion
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
      static private CombatResult[,] theTable = new CombatResult[12, 5];
      public GameStateCombat()
      {
         theTable[0, 0] = CombatResult.DefenderWins;
         theTable[1, 0] = CombatResult.DefenderWins;
         theTable[2, 0] = CombatResult.DefenderWins;
         theTable[3, 0] = CombatResult.DefenderWins;
         theTable[4, 0] = CombatResult.DefenderFlees;
         theTable[5, 0] = CombatResult.AttackerFlees;
         theTable[6, 0] = CombatResult.DefenderFlees;
         theTable[7, 0] = CombatResult.AttackerWins;
         theTable[8, 0] = CombatResult.AttackerWins;
         theTable[9, 0] = CombatResult.AttackerWins;
         theTable[10, 0] = CombatResult.AttackerWins;

         theTable[0, 1] = CombatResult.AttackerFlees;
         theTable[1, 1] = CombatResult.AttackerWins;
         theTable[2, 1] = CombatResult.DefenderWins;
         theTable[3, 1] = CombatResult.DefenderWins;
         theTable[4, 1] = CombatResult.AttackerFlees;
         theTable[5, 1] = CombatResult.AttackerWins;
         theTable[6, 1] = CombatResult.DefenderFlees;
         theTable[7, 1] = CombatResult.AttackerWins;
         theTable[8, 1] = CombatResult.DefenderWins;
         theTable[9, 1] = CombatResult.AttackerWins;
         theTable[10, 1] = CombatResult.DefenderFlees;

         theTable[0, 2] = CombatResult.DefenderFlees;
         theTable[1, 2] = CombatResult.DefenderWins;
         theTable[2, 2] = CombatResult.AttackerFlees;
         theTable[3, 2] = CombatResult.AttackerWins;
         theTable[4, 2] = CombatResult.AttackerWins;
         theTable[5, 2] = CombatResult.AttackerWins;
         theTable[6, 2] = CombatResult.DefenderWins;
         theTable[7, 2] = CombatResult.DefenderFlees;
         theTable[8, 2] = CombatResult.AttackerWins;
         theTable[9, 2] = CombatResult.DefenderWins;
         theTable[10, 2] = CombatResult.AttackerWins;

         theTable[0, 3] = CombatResult.DefenderWins;
         theTable[1, 3] = CombatResult.DefenderWins;
         theTable[2, 3] = CombatResult.AttackerWins;
         theTable[3, 3] = CombatResult.DefenderFlees;
         theTable[4, 3] = CombatResult.AttackerWins;
         theTable[5, 3] = CombatResult.AttackerWins;
         theTable[6, 3] = CombatResult.AttackerWins;
         theTable[7, 3] = CombatResult.AttackerWins;
         theTable[8, 3] = CombatResult.DefenderWins;
         theTable[9, 3] = CombatResult.AttackerFlees;
         theTable[10, 3] = CombatResult.AttackerWins;

         theTable[0, 4] = CombatResult.AttackerWins;
         theTable[1, 4] = CombatResult.AttackerFlees;
         theTable[2, 4] = CombatResult.AttackerWins;
         theTable[3, 4] = CombatResult.AttackerWins;
         theTable[4, 4] = CombatResult.AttackerWins;
         theTable[5, 4] = CombatResult.AttackerWins;
         theTable[6, 4] = CombatResult.AttackerWins;
         theTable[7, 4] = CombatResult.DefenderFlees;
         theTable[8, 4] = CombatResult.AttackerWins;
         theTable[9, 4] = CombatResult.DefenderWins;
         theTable[10, 4] = CombatResult.DefenderWins;
      }
      public void PerformCombat(IGameInstance gi)
      {
         if (null == gi.MapItemCombat)
         {
            MessageBox.Show("No Combat");
            return;
         }

         IMapItemCombat combat = gi.MapItemCombat;
         if (null == combat.Territory)
         {
            MessageBox.Show("No combat territory");
            return;
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

         foreach (MapItem mi in gi.Persons)
         {
            if ((combat.Territory.Name == mi.TerritoryCurrent.Name) && (combat.Territory.Sector == mi.TerritoryCurrent.Sector))
            {
               if ((false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || (true == mi.IsSurrendered))
                  continue;

               if (true == mi.IsAlienKnown)
               {
                  aliens.Add(mi);
               }
               else if (true == mi.IsAlienUnknown)
               {
                  if (false == gi.AddKnownAlien(mi)) // All aliens in this combat become exposed.
                     Logger.Log(LogEnum.LE_ERROR, "PerformCombat()1: returned error");
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

         // If there is no combat, return from this method

         if (0 == controlled.Count)
         {
            if ((0 == aliens.Count) || (0 == wary.Count))
               return;
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
            // If there is no combat, ignore this stack

            if ((0 == alienCount) || (0 == controlledCount))
               return;

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

            combat.Result = theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.
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
            // If there is no combat, ignore this stack

            if ((0 == alienCount) || (0 == waryCount))
               return;

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

            combat.Result = theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.

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
                     IMapItem zebulon = gi.Persons.Find("Zebulon");
                     zebulon.IsKilled = true;
                  }
                  attacker.TerritoryStarting = attacker.Territory;  // If there are any pending moves, make sure they are removed
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
                     IMapItem zebulon = gi.Persons.Find("Zebulon");
                     zebulon.IsKilled = true;
                     return;
                  }
                  defender.TerritoryStarting = defender.Territory;  // If there are any pending moves, make sure they are removed
                  if (false == PerformMovement(gi, defender))
                     Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", defender.Name);
               }
               break;

            default:
               Console.WriteLine("ERRROR - Reached default");
               break;
         }
      } // end function
      void PerformCombatResolveLoss(IGameInstance gi, IMapItem mi)
      {
         StringBuilder sb = null;
         if (mi.Name == "Zebulon")
         {
            IMapItem zebulon = gi.Persons.Find("Zebulon");
            zebulon.IsKilled = true;
            return;
         }

         // First perfom the actions that occur no matter what the result.
         // The influence factors are adjusted downward.

         gi.InfluenceCountTotal -= mi.Influence;
         sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from Total "); sb.Append(mi.Influence.ToString());
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

         int die1 = GameEngine.RandomGenerator.Next(6) + 1;
         int die2 = GameEngine.RandomGenerator.Next(6) + 1;
         int lossTableRoll = die1 + die2;

         if (lossTableRoll < 5)
         {
            mi.IsKilled = true;
         }
         else if (lossTableRoll < 7)
         {
            mi.IsConscious = false;
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
               mi.TerritoryStarting = mi.Territory;  // If there are any pending moves, make sure they are removed
               if (false == PerformMovement(gi, mi))
                  Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", mi.Name);
               mi.Movement = tempMovement;     // return MapItem movement to original value
            }
         }
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region TownspersonIterrogates
            case GameAction.TownspersonIterrogates:
               if (0 == gi.NumIterogationsThisTurn)
               {
                  gi.NextAction = "Alien Acknowledges Iterogations";
                  action = GameAction.TownspersonCompletesIterogations;
               }
               break;
            #endregion

            #region TownspersonCompletesIterogations
            case GameAction.TownspersonCompletesIterogations:
               gi.NextAction = "Alien Acknowledges Iterogations";
               break;
            #endregion

            #region AlienAcksIterogations
            case GameAction.AlienAcksIterogations:
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
               else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateIterogations.PerformAction(): " + returnStatus);
               break;
               #endregion
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
            #region TownspersonCompletesInfluencing
            case GameAction.TownspersonCompletesInfluencing:
               break;
            #endregion

            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region TownspersonCompletesRemoval
            case GameAction.TownspersonCompletesRemoval:
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
                  gi.GameTurn = "Completed";
               }
               else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateImplantRemoval.PerformAction(): " + returnStatus);
               break;
               #endregion
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
            #region ShowAlien
            case GameAction.ShowAlien:
               break;
            #endregion

            #region AlienTakeover
            case GameAction.AlienTakeover:
               returnStatus = PerformTakeover(ref gi);
               break;
            #endregion

            #region AlienCompletesTakeovers
            case GameAction.AlienCompletesTakeovers:
               if (true == GameStateChecker.CheckForEndOfGame(gi))
               {
                  action = GameAction.ShowEndGame;
                  gi.GamePhase = GamePhase.ShowEndGame;
                  gi.NextAction = "End Game";
                  gi.GameTurn = "Completed";
               }
               else if (true == GameStateChecker.CheckForRandomMoves(gi))
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
            #endregion

            #region default
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover.PerformAction(): " + returnStatus);
               break;
               #endregion
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

         // Determine if there are any observations.  If so, create a string to hold 
         // who and with what roll the observation happended.  

         List<Stack> stacks = new List<Stack>();
         stacks.AssignPeople(gi.Persons);
         foreach (String observation in gi.Takeover.Alien.Territory.Observations)
         {
            ITerritory obsTerritory = Territory.Find(observation);
            IMapItems people = stacks.FindPeople(obsTerritory);
            if (null != people)
            {
               // Get distance between two territories

               IMapPath path = MapItemMove.GetBestPath(gi.Takeover.Alien.Territory, obsTerritory, 3);
               foreach (IMapItem person in people)
               {
                  if (gi.Takeover.Uncontrolled.Name == person.Name)
                     continue;

                  if ((true == person.IsWary) || (true == person.IsAlienKnown) || (true == person.IsAlienUnknown) || (false == person.IsConscious) || (true == person.IsStunned) || (true == person.IsKilled))
                     continue;

                  int dieRoll = GameEngine.RandomGenerator.Next(6) + 1;
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

               }  // end foreach (IMapItem person in people)

            }  // end else 

         }  //  end foreach (String observation in gi.Takeover.Alien.Territory.Observations)

         gi.Takeover.Observations = sb.ToString();
         if (0 == gi.Takeover.Observations.Count())
         {
            gi.Takeover.Observations = "Nobody Noticed";
            if ((true == gi.Takeover.Uncontrolled.IsControlled) || (true == gi.Takeover.Uncontrolled.IsWary))
            {
               Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over controlled or wary ==> " + gi.Takeover.ToString());
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
               Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled without notice ==> " + gi.Takeover.ToString());
               if (false == gi.AddUnknownAlien(gi.Takeover.Uncontrolled))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()3 returned error for " + gi.Takeover.Uncontrolled.Name);
                  return "PerformTakeover() ERROR";
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled w/ observation ==> " + gi.Takeover.ToString());
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

      } // end function
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
}
