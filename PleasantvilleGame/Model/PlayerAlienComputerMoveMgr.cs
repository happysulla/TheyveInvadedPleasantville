using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienComputerMoveMgr
   {
      public const int MAX_CANDIDATE_MOVES = 8;
      public const int MAX_STACKING_COUNT = 3;
      public const int MAX_ALIEN_MOVES = 5;
      public PlayerAlienComputerMoveMgr(){}
      public bool CreateMoves(IGameInstance gi)
      {
         // BEHAVIOR STRATEGIES:
         // --------------------
         // SURROUND_ZEBULON - Zebulon is discovered - Surrond and protect at all costs
         // DEFEND_ZEBULON - tend to move toward Zebulon location to form a protective ring - more likely when Zebulon is discovered
         // FIENT_ZEBULON - Move away from Zebulon to cause Human player to move wrong direction
         // FIND_FRIENDS - Town Person is attacking... Find Friends to form groups
         // ATTACK_TOWNSPEOPLE - Losing on influence. Need to win over controlled townspeople
         // MAX_TAKEOVER - attempt to maximize takeover of uncontrolled
         // =================================
         // Choose 5 counters to be moved.
         // Create a metric:
         //    --- Early Game
         //        *** Out of Range of Controlled
         //        *** Lower Observation Chance
         //        *** Greater Influence
         //        *** Greater Combat
         //        *** Closer to Zebulon
         //        *** Closer to Alien Center
         //        *** Away from Controlled Center
         //    --- Middle Game
         //        *** Takeover Stunned Controlled
         //        *** Free Tied-up Aliens
         //        *** Defensive Stacking
         //        *** Advantage Combats
         //        *** Closer to Zebulon
         //        *** Away from Controlled Townspeople
         //        *** Isolated from Observations 
         //    --- End Game
         //        *** Protect to Zebulon
         //        *** Combats to Win
         //        *** Takeover Stunned Controlled
         //        *** Free Tied-up Aliens
         //        *** Defensive Stacking
         //        *** Advantage Combats
         //        *** Closer to Zebulon
         // Identify who Aliens move to or what uncontrolled move to Aliens
         // Should move away observing units?
         // Pick remaining uncontrolled townspeople to move.
         //    --- Move away from Town controlled units
         //    --- Add deception on what is being taken over
         // NOTES:
         //    --- Do not move an uncontrolled away from an Alien Takeover
         //    --- If an alien cannot get to an area, consider moving the uncontrolled to it
         //    --- Move units away from controlled townsperson
         //-------------------------------------------------------
         IMetricAlienMoves moves = new MetricAlienMoves();
         IStacks newStacks = gi.Stacks.Clone();
         //-------------------------------------------------------
         if (false == AddMoveAwayFromControlledMoves(newStacks, gi.Zebulon.TerritoryCurrent.CenterPoint, ref moves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Moves(): Add_MoveAwayFromControlledMoves() returned error");
            return false;
         }
         //-------------------------------------------------------
         if (false == AddAlienTakeoverMoves(newStacks, gi.Zebulon.TerritoryCurrent.CenterPoint, ref moves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Moves(): Add_MoveAwayFromControlledMoves() returned error");
            return false;
         }
         //-------------------------------------------------------
         if (false == AddUncontrolledMoves(newStacks, gi.Zebulon.TerritoryCurrent.CenterPoint, ref moves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Moves(): Add_UncontrolledMoves() returned error");
            return false;
         }
         //-------------------------------------------------------
         //if (false == AddCandidatesMovesToStunned(gi, stunnedPeoples, uncontrolledPeoples, myMoves))
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "Add_andidatesMovesToStunned(): MoveUnknownAliens() returned error");
         //   return false;
         //}
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Create_Moves(): ==================================================");
         foreach (IMetricAlienMove mam in moves)
            Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_MoveAwayFromControlledMoves(): " + mam.ToString());
         IMetricAlienMoves currentMoves = new MetricAlienMoves();
         List<IMetricAlienMoves> combinations = new List<IMetricAlienMoves>();
         GenerateCombinations(moves, MAX_ALIEN_MOVES, currentMoves, 0, ref combinations); // returns list of all combinations of 5 out of the moves
         IMetricAlienMoves? bestMoves = FindBestMoves(gi.Stacks, combinations);
         if( null == bestMoves)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Moves(): Find_BestMoves() returned null");
            return false;
         }
         //--------------------------------------------------
         if (true == Logger.theLogLevel[(int)LogEnum.LE_SHOW_ALIEN_MOVE_METRIC])
         {
            Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Create_Moves(): ==================================================");
            bestMoves = bestMoves.Sort();
            int totalMetric = 0;
            foreach (IMetricAlienMove mam in bestMoves)
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Create_Moves(): " + mam.ToString());
               totalMetric += mam.MetricTotal;
            }
            Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Create_Moves(): ==================" + totalMetric.ToString() + "=========================");
         }
         foreach(IStack stack in gi.Stacks) // not sure why I need to do this
         {
            foreach(IMapItem mi in stack.MapItems)
               mi.TerritoryCurrent = mi.TerritoryStarting;
         }
         //--------------------------------------------------
         IMapItemMoves mapItemMoves = new MapItemMoves();
         foreach (IMetricAlienMove mam in bestMoves)
         {

            IMapItemMove? mim = gi.CreateMapItemMove(mam.MovingMapItem, mam.TargetTerritory);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_SHOW_MIM, "Create_Moves(): Create_MapItemMove() returned null");
               return false;
            }
            mapItemMoves.Add(mim);
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Create_Moves(): mims=" + mapItemMoves.ToString());
         gi.MapItemMoves = mapItemMoves.Shuffle();
         return true;
      }
      //================================================================
      private void GenerateCombinations(IMetricAlienMoves items, int r, IMetricAlienMoves currentMetrics, int depth, ref List<IMetricAlienMoves> combinations)
      {
         if (currentMetrics.Count == r)
         {
            combinations.Add(currentMetrics.Clone());
            return;
         }
         for (int i = depth; i < items.Count; i++)
         {
            IMetricAlienMove? tempMetric = items[i];
            if ( null == tempMetric)
            {
               Logger.Log(LogEnum.LE_ERROR, "Generate_Combinations(): item[i]=null");
               return;
            }
            currentMetrics.Add(tempMetric);
            GenerateCombinations(items, r, currentMetrics, i + 1, ref combinations);
            currentMetrics.RemoveAt(currentMetrics.Count - 1);
         }
      }
      private IMetricAlienMoves? FindBestMoves(IStacks stacks, List<IMetricAlienMoves> combinations)
      {

         IMetricAlienMoves? bestMoves = null;
         int maxValue = 0;
         foreach (IMetricAlienMoves moveMetrics in combinations)
         {
            IStacks newStacks = stacks.Clone();
            foreach (IMetricAlienMove moveMetric in moveMetrics)  //  move each moving map item to proposed location 
            {
               newStacks.Remove(moveMetric.MovingMapItem);
               newStacks.Add(moveMetric.MovingMapItem, moveMetric.TargetTerritory);
            }
            //-------------------------------------
            foreach (IMetricAlienMove moveMetric in moveMetrics)  // update metrics for each mapitem based on proposed location
               moveMetric.UpdateTotalMetric(newStacks);
            //-------------------------------------
            int tempValue = 0;
            foreach (IMetricAlienMove moveMetric in moveMetrics) // get a sum value of move metrics - if best value, make that the proposed move metrics
               tempValue += moveMetric.MetricTotal;
            if (maxValue < tempValue)
            {
               bestMoves = moveMetrics.Clone();
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Find_BestMoves(): ***" + maxValue.ToString() + "->" + tempValue.ToString() + "*** bestMoves=" + bestMoves.ToString());
               foreach (IMetricAlienMove mam in moveMetrics)
                  Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_MoveAwayFromControlledMoves(): " + mam.ToString());
               maxValue = tempValue;
            }
         }
         if (null == bestMoves)
         {
            Logger.Log(LogEnum.LE_ERROR, "Find_BestMoves(): bestMoves=null");
            return null;
         }
         return bestMoves;
      }
      public bool AddMoveAwayFromControlledMoves(IStacks stacks, IMapPoint mpZebulon, ref IMetricAlienMoves candidateMoves)
      {
         IMapItems townPeoples = new MapItems();
         IMapItems unknowns = new MapItems();
         foreach (IStack stack in stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               bool isAlien = mi.IsAlien();
               if ((true == mi.IsMovingThisTurn) || (true == mi.IsKnockedout) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || ("Zebulon" == mi.Name) || (((true == mi.IsStunned) || (true == mi.IsWary)) && (false == isAlien)))
                  continue;
               else if (true == mi.IsControlled)
                  townPeoples.Add(mi);
               else if ( (true == mi.IsAlienUnknown) || (false == isAlien) )
                  unknowns.Add(mi);
               }
         }
         if ((0 == townPeoples.Count) || (0==unknowns.Count) ) // do nothing
            return true;
         townPeoples = townPeoples.Shuffle();
         //------------------------------------
         IMapItems totalMapItems = new MapItems();
         foreach (IMapItem controlled in townPeoples) 
         {
            IMapItems closeMapItems = Territory.GetMapItemsOfInterest(stacks, Territories.theTerritories, controlled); // MapItems this mapitem can move to
            foreach (IMapItem mi in closeMapItems)
            {
               if( false == totalMapItems.Contains(mi))
                  totalMapItems.Add(mi);
            }
            if (MAX_CANDIDATE_MOVES < (totalMapItems.Count + candidateMoves.Count) ) // too many mapitems to move away - alien only gets to move 5 people
               return true;
         }
         if (0 == totalMapItems.Count)
            return true;
         totalMapItems.Remove("Zebulon"); // remove if part of stack
         totalMapItems = totalMapItems.Shuffle();
         //------------------------------------
         IMapPoint mpCenterControlled = Territory.GetCenterPointControlled(stacks);
         IMapPoint mpCenterAlien = Territory.GetCenterPointAlien(stacks);
         foreach (IMapItem candidate in totalMapItems)
         {
            ITerritories nonOverlappingTerritories = new Territories();
            bool isInitialized = false;
            foreach (IMapItem controlled in townPeoples)
            {
               ITerritories territories = Territory.GetTerritoriesOfInterest(candidate, controlled, false);
               if (false == isInitialized)
               {
                  isInitialized = true;
                  nonOverlappingTerritories = territories;
                  continue;
               }
               nonOverlappingTerritories = territories.And(nonOverlappingTerritories);
            }
            //------------------------------------
            IMetricAlienMoves alienMoveMetrics = new MetricAlienMoves();
            foreach (Territory t in nonOverlappingTerritories)
            {
               IStack? stack = stacks.Find(t);
               int stackCount = Territory.GetStackingCount(stack, candidate);
               if (3 < stackCount)
                  continue;
               IMetricAlienMove mam = new MetricAlienMove(candidate, t);
               mam.MetricDenyTown = 100; // this gets added to in SetTotalMetric()
               mam.SetTotalMetric(stacks, mpZebulon, mpCenterAlien, mpCenterControlled);
               alienMoveMetrics.Add(mam);
            }
            //------------------------------------
            if (0 < alienMoveMetrics.Count)
            {
               IMetricAlienMoves sortedMetrics = alienMoveMetrics.Sort();
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_MoveAwayFromControlledMoves(): SORTED METRICS for candidate=" + candidate.Name + "->" + sortedMetrics.ToString());
               if (0 < sortedMetrics.Count)
               {
                  IMetricAlienMove? mam1 = sortedMetrics[0];
                  if (null == mam1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Add_MoveAwayFromControlledMoves(): mam1=null");
                     return false;
                  }
                  candidateMoves.Add(mam1);
                  //-------------------------------
                  mam1.MovingMapItem.IsMovingThisTurn = true;
                  stacks.Remove(mam1.MovingMapItem); // reset stacks to show change
                  stacks.Add(mam1.MovingMapItem, mam1.TargetTerritory);
               }
            }
         }
         if( true == Logger.theLogLevel[(int)LogEnum.LE_SHOW_ALIEN_MOVE_METRIC])
         {
            candidateMoves = candidateMoves.Sort();
            foreach (IMetricAlienMove mam in candidateMoves)
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_MoveAwayFromControlledMoves(): " + mam.ToString());
         }
         return true;
      }
      public bool AddAlienTakeoverMoves(IStacks stacks, IMapPoint mpZebulon, ref IMetricAlienMoves candidateMoves)
      {
         IMapItems aliens = new MapItems();
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolledPeoples = new MapItems();
         foreach (IStack stack in stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsMovingThisTurn) || (true == mi.IsKnockedout) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || ("Zebulon" == mi.Name) || (true == mi.IsStunned) || (true == mi.IsWary))
                  continue;
               else if (true == mi.IsAlien())
                  aliens.Add(mi);
               else if (true == mi.IsControlled)
                  townPeoples.Add(mi);
               else 
                  uncontrolledPeoples.Add(mi);
            }
         }
         aliens = aliens.Shuffle();
         //------------------------------------------
         IMapPoint mpCenterControlled = Territory.GetCenterPointControlled(stacks);
         IMapPoint mpCenterAlien = Territory.GetCenterPointAlien(stacks);
         //------------------------------------------
         foreach (IMapItem candidate in aliens)
         {
            IMetricAlienMoves alienMoveMetrics = new MetricAlienMoves();
            IMapItems closeMapItems = Territory.GetMapItemsOfInterest(stacks, Territories.theTerritories, candidate); // MapItems this mapitem can move to
            closeMapItems = closeMapItems.Shuffle();
            foreach (IMapItem closeMapItem in closeMapItems)
            {
               if ((true == closeMapItem.IsKnockedout) || (true == closeMapItem.IsTiedUp) || (true == closeMapItem.IsKilled) || ("Zebulon" == closeMapItem.Name) || (true == closeMapItem.IsStunned) || (true == closeMapItem.IsWary) || (true == closeMapItem.IsControlled) || (true == closeMapItem.IsAlien()))
                  continue;
               if ((true == closeMapItem.IsMovingThisTurn) && (true == candidate.IsMovingThisTurn))
                  continue;
               if (closeMapItem.TerritoryCurrent.ToString() == candidate.TerritoryCurrent.ToString())
                  continue;
               IStack? candidateStack = stacks.Find(candidate.TerritoryCurrent);
               int candidateStackCount = Territory.GetStackingCount(candidateStack, candidate);
               IStack? closeMapItemStack = stacks.Find(closeMapItem.TerritoryCurrent);
               int closeMapItemStackCount = Territory.GetStackingCount(closeMapItemStack, closeMapItem);
               if ((3 < candidateStackCount) && (3 < closeMapItemStackCount))
                  continue;
               //------------------------------------------
               IMetricAlienMove mamCandidateMove = new MetricAlienMove(candidate, closeMapItem.TerritoryCurrent);
               IMetricAlienMove mamCloseItemMove = new MetricAlienMove(closeMapItem, candidate.TerritoryCurrent);
               bool isCloseMapItemWithinRange = Territory.IsMapItemWithingRange(candidate, closeMapItem);
               //------------------------------------------
               if ((closeMapItemStackCount <= MAX_STACKING_COUNT) && (false == candidate.IsMovingThisTurn))
                  mamCandidateMove.SetTotalMetric(stacks, mpZebulon, mpCenterAlien, mpCenterControlled);
               //------------------------------------------
               if ((candidateStackCount <= MAX_STACKING_COUNT) && (false == closeMapItem.IsMovingThisTurn) && (true == isCloseMapItemWithinRange))
                  mamCloseItemMove.SetTotalMetric(stacks, mpZebulon, mpCenterAlien, mpCenterControlled);
               //------------------------------------------
               if (mamCloseItemMove.MetricTotal < mamCandidateMove.MetricTotal)
               {
                  IStack? stack = stacks.Find(mamCandidateMove.TargetTerritory);
                  if (null == stack)
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "-->" + closeMapItem.Name + " in " + mamCandidateMove.TargetTerritory.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "-->" + closeMapItem.Name + " stack=" + stack.ToString());
                  alienMoveMetrics.Add(mamCandidateMove);
               }
               else
               {
                  IStack? stack = stacks.Find(mamCloseItemMove.TargetTerritory);
                  if (null == stack)
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "<--" + closeMapItem.Name + " in " + mamCloseItemMove.TargetTerritory.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "<--" + closeMapItem.Name + " stack=" + stack.ToString());
                  alienMoveMetrics.Add(mamCloseItemMove);
               }
            }
            if (0 < alienMoveMetrics.Count)
            {
               IMetricAlienMoves sortedMetrics = alienMoveMetrics.Sort();
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): >>>>>>>>>>>>>>>>>>>>>>>> candidate=" + candidate.Name + " candidateMoves=" + candidateMoves.ToString());
               foreach (IMetricAlienMove mam in sortedMetrics)
               {
                  IStack? stack = stacks.Find(mam.TargetTerritory);
                  if( null == stack )
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): " + mam.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): " + mam.ToString() + " stack=" + stack.ToString());
               }
               if (0 < sortedMetrics.Count)
               {
                  IMetricAlienMove? newMetric = sortedMetrics[0];
                  if (null == newMetric)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Add_AlienTakeoverMoves(): mam1=null");
                     return false;
                  }
                  if (candidateMoves.Count < MAX_CANDIDATE_MOVES)
                  {
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): aaaaaaaaaaaaaaaaaaaaaaaa " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString() + ")");
                     candidateMoves.Add(newMetric);
                     newMetric.MovingMapItem.IsMovingThisTurn = true;
                     stacks.Remove(newMetric.MovingMapItem); // reset stacks to show change
                     stacks.Add(newMetric.MovingMapItem, newMetric.TargetTerritory);
                  }
                  else
                  {
                     candidateMoves = candidateMoves.Sort();
                     IMetricAlienMove? lastMetric = candidateMoves[candidateMoves.Count - 1];
                     if (null == lastMetric)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Add_AlienTakeoverMoves(): lastMetric=null");
                        return false;
                     }
                     if (lastMetric.MetricTotal < newMetric.MetricTotal)
                     {
                        Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): rrrrrrrrrrrrrrrrrrrrrrrr " + lastMetric.MovingMapItem.Name + "->" + lastMetric.TargetTerritory.ToString() + "(" + lastMetric.MetricTotal.ToString() + ") WITH " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString() + ")");
                        lastMetric.MovingMapItem.IsMovingThisTurn = false;
                        stacks.Remove(lastMetric.MovingMapItem); // return the last moving mapitem to its original stack
                        stacks.Add(lastMetric.MovingMapItem, lastMetric.MovingMapItem.TerritoryStarting);
                        candidateMoves.Remove(lastMetric);
                        //--------------------------------
                        candidateMoves.UpdateMetrics(stacks);
                        //--------------------------------
                        candidateMoves.Add(newMetric);
                        newMetric.MovingMapItem.IsMovingThisTurn = true;
                        stacks.Remove(newMetric.MovingMapItem); // reset stacks to show change
                        stacks.Add(newMetric.MovingMapItem, newMetric.TargetTerritory);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_AlienTakeoverMoves(): iiiiiiiiiiiiiiiiiiiiiiii " + lastMetric.MovingMapItem.Name + "->" + lastMetric.TargetTerritory.ToString() + "(" + lastMetric.MetricTotal.ToString() + ") AND " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString() + ")");
                     }
                  }
               }
            }
         }
         return true;
      }
      public bool AddUncontrolledMoves(IStacks stacks, IMapPoint mpZebulon, ref IMetricAlienMoves candidateMoves)
      {
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolledPeoples = new MapItems();
         foreach (IStack stack in stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsMovingThisTurn) || (true == mi.IsKnockedout) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || ("Zebulon" == mi.Name) || (true == mi.IsStunned) || (true == mi.IsWary) || (true==mi.IsAlien()) )
                  continue;
               else if (true == mi.IsControlled)
                  townPeoples.Add(mi);
               else
                  uncontrolledPeoples.Add(mi);
            }
         }
         uncontrolledPeoples = uncontrolledPeoples.Shuffle();
         //------------------------------------
         IMapPoint mpCenterControlled = Territory.GetCenterPointControlled(stacks);
         IMapPoint mpCenterAlien = Territory.GetCenterPointAlien(stacks);
         //------------------------------------
         foreach (IMapItem candidate in uncontrolledPeoples)
         {
            IMetricAlienMoves uncontrolledMoveMetric = new MetricAlienMoves();
            IMapItems closeMapItems = Territory.GetMapItemsOfInterest(stacks, Territories.theTerritories, candidate); // MapItems this mapitem can move to
            closeMapItems = closeMapItems.Shuffle();
            foreach (IMapItem closeMapItem in closeMapItems)
            {
               if ((true == closeMapItem.IsKnockedout) || (true == closeMapItem.IsTiedUp) || (true == closeMapItem.IsKilled) || ("Zebulon" == closeMapItem.Name) || (true == closeMapItem.IsStunned) || (true == closeMapItem.IsWary) || (true == closeMapItem.IsControlled) )
                  continue;
               if ((true == closeMapItem.IsMovingThisTurn) && (true == candidate.IsMovingThisTurn))
                  continue;
               if (closeMapItem.TerritoryCurrent.ToString() == candidate.TerritoryCurrent.ToString())
                  continue;
               IStack? candidateStack = stacks.Find(candidate.TerritoryCurrent);
               int candidateStackCount = Territory.GetStackingCount(candidateStack, candidate);
               IStack? closeMapItemStack = stacks.Find(closeMapItem.TerritoryCurrent);
               int closeMapItemStackCount = Territory.GetStackingCount(closeMapItemStack, closeMapItem);
               if ((3 < candidateStackCount) && (3 < closeMapItemStackCount))
                  continue;
               //------------------------------------------
               IMetricAlienMove mamCandidateMove = new MetricAlienMove(candidate, closeMapItem.TerritoryCurrent);
               IMetricAlienMove mamCloseItemMove = new MetricAlienMove(closeMapItem, candidate.TerritoryCurrent);
               bool isCloseMapItemWithinRange = Territory.IsMapItemWithingRange(candidate, closeMapItem);
               //------------------------------------------
               if ((closeMapItemStackCount <= MAX_STACKING_COUNT) && (false == candidate.IsMovingThisTurn))
                  mamCandidateMove.SetTotalMetric(stacks, mpZebulon, mpCenterAlien, mpCenterControlled);
               //------------------------------------------
               if ((candidateStackCount <= MAX_STACKING_COUNT) && (false == closeMapItem.IsMovingThisTurn) && (true == isCloseMapItemWithinRange))
                  mamCloseItemMove.SetTotalMetric(stacks, mpZebulon, mpCenterAlien, mpCenterControlled);
               //------------------------------------------
               if (mamCloseItemMove.MetricTotal < mamCandidateMove.MetricTotal)
               {
                  IStack? stack = stacks.Find(mamCandidateMove.TargetTerritory);
                  if( null == stack )
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "-->" + closeMapItem.Name + " in " + mamCandidateMove.TargetTerritory.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "-->" + closeMapItem.Name + " stack=" + stack.ToString());
                  uncontrolledMoveMetric.Add(mamCandidateMove);
               }
               else
               {
                  IStack? stack = stacks.Find(mamCloseItemMove.TargetTerritory);
                  if (null == stack)
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "<--" + closeMapItem.Name + " in " + mamCloseItemMove.TargetTerritory.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): nnnnnnnnnnnnnnnnnnnnnnnn " + candidate.Name + "<--" + closeMapItem.Name + " stack=" + stack.ToString());
                  uncontrolledMoveMetric.Add(mamCloseItemMove);
               }
            }
            if (0 < uncontrolledMoveMetric.Count)
            {
               IMetricAlienMoves sortedMetrics = uncontrolledMoveMetric.Sort();
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): >>>>>>>>>>>>>>>>>>>>>>>> candidate=" + candidate.Name + " candidateMoves=" + candidateMoves.ToString());
               foreach (IMetricAlienMove mam in sortedMetrics)
               {
                  IStack? stack = stacks.Find(mam.TargetTerritory);
                  if (null == stack)
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): " + mam.ToString());
                  else
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): " + mam.ToString() + " stack=" + stack.ToString());
               }
               if (0 < sortedMetrics.Count)
               {
                  IMetricAlienMove? newMetric = sortedMetrics[0];
                  if (null == newMetric)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Add_UncontrolledMoves(): mam1=null");
                     return false;
                  }
                  if ( candidateMoves.Count < MAX_CANDIDATE_MOVES)
                  {
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): aaaaaaaaaaaaaaaaaaaaaaaa " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString()+")");
                     candidateMoves.Add(newMetric);
                     newMetric.MovingMapItem.IsMovingThisTurn = true;
                     stacks.Remove(newMetric.MovingMapItem); // reset stacks to show change
                     stacks.Add(newMetric.MovingMapItem, newMetric.TargetTerritory);
                  }
                  else
                  {
                     candidateMoves = candidateMoves.Sort();
                     IMetricAlienMove? lastMetric = candidateMoves[candidateMoves.Count - 1];
                     if( null == lastMetric )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Add_UncontrolledMoves(): lastMetric=null");
                        return false;
                     }
                     if(lastMetric.MetricTotal < newMetric.MetricTotal)
                     {
                        Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): rrrrrrrrrrrrrrrrrrrrrrrr " + lastMetric.MovingMapItem.Name + "->" + lastMetric.TargetTerritory.ToString() + "(" + lastMetric.MetricTotal.ToString() + ") WITH " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString() + ")");
                        lastMetric.MovingMapItem.IsMovingThisTurn = false;
                        stacks.Remove(lastMetric.MovingMapItem); // return the last moving mapitem to its original stack
                        stacks.Add(lastMetric.MovingMapItem, lastMetric.MovingMapItem.TerritoryStarting);
                        candidateMoves.Remove(lastMetric);
                        //--------------------------------
                        candidateMoves.UpdateMetrics(stacks);
                        //--------------------------------
                        candidateMoves.Add(newMetric);
                        newMetric.MovingMapItem.IsMovingThisTurn = true;
                        stacks.Remove(newMetric.MovingMapItem);
                        stacks.Add(newMetric.MovingMapItem, newMetric.TargetTerritory);
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "Add_UncontrolledMoves(): iiiiiiiiiiiiiiiiiiiiiiii " + lastMetric.MovingMapItem.Name + "->" + lastMetric.TargetTerritory.ToString() + "(" + lastMetric.MetricTotal.ToString() + ") AND " + newMetric.MovingMapItem.Name + "->" + newMetric.TargetTerritory.ToString() + "(" + newMetric.MetricTotal.ToString() + ")");
                     }
                  }
               }
            }
         }
         return true;
      }
      private bool AddMovesToStunned(IStacks stacks, IMapPoint mpZebulon, IMetricAlienMoves candidateMoves)
      {
         IMapItems aliens = new MapItems();
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolledPeoples = new MapItems();
         IMapItems stunned = new MapItems();
         foreach (IStack stack in stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsMovingThisTurn) || (true == mi.IsKnockedout) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || ("Zebulon" == mi.Name) || (true == mi.IsStunned) || (true == mi.IsWary))
                  continue;
               else if (true == mi.IsAlien())
                  aliens.Add(mi);
               else if (true == mi.IsControlled)
                  townPeoples.Add(mi);
               else if (true == mi.IsStunned)
                  stunned.Add(mi);
               else if (false == mi.IsTiedUp) // tied up uncontrolled people are ignored
                  uncontrolledPeoples.Add(mi);
            }
         }
         //------------------------------------
         IMapPoint mpCenterControlled = Territory.GetCenterPointControlled(stacks);
         IMapPoint mpCenterAlien = Territory.GetCenterPointAlien(stacks);
         return true;
      }

   }
}
