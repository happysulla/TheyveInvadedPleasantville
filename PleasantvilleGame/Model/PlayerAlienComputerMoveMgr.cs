using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienComputerMoveMgr
   {
      public IMapItems Targets { get; set; } = new MapItems();
      public PlayerAlienComputerMoveMgr(){}
      public bool PerformAlienMovement(IGameInstance gi, Behavior behavior)
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
         // Create a metric for each Uncontrolled Townsperson:
         //    --- Takeover Stunned Controlled
         //    --- Free Tied-up Aliens 
         //    --- Isolated from Observations
         //    --- Away from Controlled Townspeople
         //    --- Closer to Zebulon
         //    --- Closer to Alien Center
         //    --- Greater Influence
         //    --- Greater Combat
         // Identify who Aliens move to or what uncontrolled move to Aliens
         // Should move away observing units?
         // Pick remaining uncontrolled townspeople to move.
         //    --- Move away from Town controlled units
         //    --- Add deception on what is being taken over
         // NOTES:
         //    --- Do not move an uncontrolled away from an Alien Takeover
         //    --- If an alien cannot get to an area, consider moving the uncontrolled to it
         //    --- Move units away from controlled townsperson
         //----------------------------------------------------------------
         IMapItemMoves alienMoves = new MapItemMoves();
         IMapItems knownAliens = new MapItems();
         IMapItems unknownAliens = new MapItems();
         IMapItems townControlledPeoples = new MapItems();
         IMapItems uncontrolledPeoples = new MapItems();
         IMapItems stunnedPeoples = new MapItems();
         IMapItems waryPeoples = new MapItems();
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               mi.IsMovingThisTurn = false;
               if ((false == mi.IsKnockedout) && (false == mi.IsKilled))
                  continue;
               else if ((true == mi.IsStunned) && (false == mi.IsAlien()))
                  stunnedPeoples.Add(mi);
               else if ((true == mi.IsWary) && (false == mi.IsAlien()))
                  waryPeoples.Add(mi);
               else if (true == mi.IsAlienKnown)
                  knownAliens.Add(mi);
               else if (true == mi.IsAlienUnknown)
                  unknownAliens.Add(mi);
               else if (true == mi.IsControlled)
                  townControlledPeoples.Add(mi);
               else if (false == mi.IsTiedUp) // tied up uncontrolled people are ignored
                  uncontrolledPeoples.Add(mi);
            }
         }
         //============================================
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
            }
         }
         //============================================
         Logger.Log(LogEnum.LE_SHOW_UNCONTROLLED, "Perform_AlienMovement(): MoveUnknownAliens() uncontrolledPeoples=" + uncontrolledPeoples.ToString());
         //-------------------------------------------------------

         //-------------------------------------------------------
         if (false == MoveUnknownAliens(gi, unknownAliens, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMovement(): MoveUnknownAliens() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMovement(): mims=" + alienMoves.ToString());
         if (false == IntersectAlienUncontrolled(gi, unknownAliens, uncontrolledPeoples, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMovement(): IntersectAlienUncontrolled() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMovement(): mims=" + alienMoves.ToString());
         if (false == MoveUncontrolled(gi, uncontrolledPeoples, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMovement(): MoveUncontrolled() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMovement(): mims=" + alienMoves.ToString());
         gi.MapItemMoves = alienMoves.Shuffle();
         return true;
      }
      private bool MoveKnownAliens()
      {
         return true;
      }
      private bool MoveUnknownAliens(IGameInstance gi, IMapItems unknownAliens, IMapItemMoves alienMoves )
      {
         foreach (IMapItem unknownAlien in unknownAliens)
         {
            if (4 < alienMoves.Count) // only move five units
               break;
            IMapItems? closeMapItems = Territory.GetMapItemsWithinRange(gi, unknownAlien.TerritoryCurrent, unknownAlien.Movement);
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): GetMapItemsWithinRange() returned error");
               return false;
            }
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): GetMapItemsWithinRange() returned error");
               return false;
            }
            //-----------------------------------------
            IMetricObservations metrics = new MetricObservations();
            foreach (IMapItem target in closeMapItems)
            {
               if ((true == target.IsControlled) || (true == target.IsAlienUnknown) || (true == target.IsAlienKnown) || (true == target.IsMovingThisTurn) || (true == target.IsWary)) // do not include controlled MapItems as candidates
                  continue;
               IMetricObservation metric = new MetricObservation(gi, target);
               metrics.Add(metric);
            }
            IMetricObservations sortedMetrics = metrics.Sort();
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_UnknownAliens(): for unknownAlien=" + unknownAlien.Name + " sortedMetrics=" + sortedMetrics.ToString());
            if (0 == sortedMetrics.Count) // nobody to move to
               continue;
            //-----------------------------------------
            IMetricObservation? metricVictim = sortedMetrics[0];
            if (null == metricVictim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): metricVictim=null");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_UnknownAliens(): for unknownAlien=" + unknownAlien.Name + " metricVictim=" + metricVictim.ToString());
            //-----------------------------------------
            if( unknownAlien.TerritoryCurrent.ToString() != metricVictim.Target.TerritoryCurrent.ToString()) // If in same territory, do not move
            {
               int alienStackingCount = Territory.GetStackingCountUncontrolled(gi, unknownAlien.TerritoryCurrent, alienMoves);
               int victimStackingCount = Territory.GetStackingCountUncontrolled(gi, metricVictim.Target.TerritoryCurrent, alienMoves);
               //-----------------------------------------
               IMapItemMove? mim = null;
               IMetricObservation metricAlien = new MetricObservation(gi, unknownAlien); // Is it better to move unknown to victum or victum to unknown
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_UnknownAliens(): for metricAlien=" + metricAlien.ToString());
               if (metricVictim.Value < metricAlien.Value) // prefer to move the victim to the unknown alien's territory
               {
                  if (alienStackingCount < 3)
                     mim = gi.CreateMapItemMove(metricVictim.Target, unknownAlien.TerritoryCurrent); // moving victim to unknown alien's territory
                  else if (victimStackingCount < 3)
                     mim = gi.CreateMapItemMove(unknownAlien, metricVictim.Target.TerritoryCurrent); // moving unknown alien to victim's territory
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE, "Move_UnknownAliens(): 2 - asc=" + alienStackingCount.ToString() + "(" + unknownAlien.TerritoryCurrent.ToString() + ") vsc=" + victimStackingCount.ToString() + "(" + metricVictim.Target.TerritoryCurrent.ToString() + ")");
                     continue; // cannot move either because of stacking limits
                  }
               }
               else // prefer to move the alien to the victim's territory
               {
                  if (victimStackingCount < 3)
                     mim = gi.CreateMapItemMove(unknownAlien, metricVictim.Target.TerritoryCurrent); // moving unknown alien to victim's territory
                  else if (alienStackingCount < 3)
                     mim = gi.CreateMapItemMove(metricVictim.Target, unknownAlien.TerritoryCurrent); // moving victim to unknown alien's territory
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE, "Move_UnknownAliens(): 2 - vsc=" + victimStackingCount.ToString() + "(" + metricVictim.Target.TerritoryCurrent.ToString() +  ") asc=" + alienStackingCount.ToString() + "(" + unknownAlien.TerritoryCurrent.ToString() + ")");
                     continue; // cannot move either because of stacking limits
                  }
   
               }
               //-----------------------------------------
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): CreateMapItemMove() returned mim=null");
                  return false;
               }
               if (null == mim.OldTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): CreateMapItemMove() returned mim.OldTerritory=null");
                  return false;
               }
               if (null == mim.NewTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Move_UnknownAliens(): CreateMapItemMove() returned mim.NewTerritory=null");
                  return false;
               }
               alienMoves.Add(mim);
               unknownAlien.IsMovingThisTurn = true;         // do not allow alien to move other than to the victim
               metricVictim.Target.IsMovingThisTurn = true;  // do not allow victim to move
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE, "Move_UnknownAliens(): Moving mi=" + mim.MapItem.Name + " from " + mim.OldTerritory.ToString() + " to " + mim.NewTerritory.ToString() + " asc=" + alienStackingCount.ToString() + " vsc=" + victimStackingCount.ToString());
            }
         }
         return true;
      }
      private bool IntersectAlienUncontrolled(IGameInstance gi, IMapItems unknownAliens, IMapItems uncontrolledPeoples, IMapItemMoves alienMoves)
      {
         foreach (IMapItem unknownAlien in unknownAliens)
         {
            if (4 < alienMoves.Count) // only move five units
               break;
            if (true == unknownAlien.IsMovingThisTurn) // already selected to move
               continue;
         }
         return true;
      }
      private bool MoveUncontrolled(IGameInstance gi, IMapItems uncontrolledPeoples, IMapItemMoves alienMoves)
      {
         IMetricObservations uncontrolledMetrics = new MetricObservations();
         foreach (IMapItem uncontrolled in uncontrolledPeoples) // need to find the best territory to move units to that will not be observed
         {
            IMetricObservation metric = new MetricObservation(gi, uncontrolled); // the metric provides the probability that this uncontrolled person will be observed in a takeover in this territory
            uncontrolledMetrics.Add(metric);
         }
         IMetricObservations sortedUncontrolledMetrics = uncontrolledMetrics.Sort(); // sort the metrics for all uncontrolled people - best locations to move uncontrolled people
         Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_Uncontrolled(): for sortedUncontrolledMetrics=" + sortedUncontrolledMetrics.ToString());
         //----------------------------------------------------------------
         foreach (IMetricObservation metric in sortedUncontrolledMetrics)
         {
            if (4 < alienMoves.Count) // only move five units
               break;
            if (true == metric.Target.IsMovingThisTurn) // already targeted or moving... skip this mapitem
            {
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_Uncontrolled(): target alreadying moving =" + metric.Target.ToString());
               continue;
            }
            //-----------------------------------------
            bool isAlienMovingHereAlready = false;
            foreach(IMapItemMove mim1 in alienMoves)
            {
               if( null == mim1.NewTerritory )
               {
                  Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): mim1.NewTerritory=null");
                  return false;
               }
               if (mim1.NewTerritory.ToString() == metric.Target.TerritoryCurrent.ToString())
               {
                  isAlienMovingHereAlready = true;
                  Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_Uncontrolled(): alien=" + mim1.MapItem.ToString() + " already there");
               }
            }
            if (true == isAlienMovingHereAlready)
               continue;
            //-----------------------------------------
            int uncontrolledStackingCount = Territory.GetStackingCountUncontrolled(gi, metric.Target.TerritoryCurrent, alienMoves);
            if( 3 <= uncontrolledStackingCount) // cannot move to this territory because of stacking limits
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE, "Move_Uncontrolled(): 1 - usc=" + uncontrolledStackingCount.ToString() + "(" + metric.Target.TerritoryCurrent.ToString() + ")");
               continue;
            }
            //-----------------------------------------
            IMapItems? closeMapItems = Territory.GetMapItemsWithinRange(gi, metric.Target.TerritoryCurrent, metric.Target.Movement); // Find mapitems that can be moved to the targets location
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): GetMapItemsWithinRange() returned error");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Move_Uncontrolled(): for target=" + metric.Target.ToString() + " has closeMapItems=" + closeMapItems.ToString());
            if (closeMapItems.Count < 1)
               continue;
            //-----------------------------------------
            IMapItem? movingMi = null;
            int count = 9;
            while ( 0 < count-- )
            {
               int randomNum = Utilities.RandomGenerator.Next(closeMapItems.Count);
               movingMi = closeMapItems[randomNum];
               if (null == movingMi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): movingMi=null for randUm=" + randomNum.ToString() + " in closeMapItems=" + closeMapItems.ToString());
                  return false;
               }
               if ((true == movingMi.IsWary) || (true == movingMi.IsKnockedout) || (true == movingMi.IsKilled) || (true == movingMi.IsTiedUp) || (true == movingMi.IsStunned) || (true == movingMi.IsControlled))
                  continue;
               if (true == movingMi.IsMovingThisTurn) // already moving or targeted so skip moving
                  continue;
               if (movingMi.TerritoryCurrent.ToString() == metric.Target.TerritoryCurrent.ToString()) // If in same territory, do not move
                  continue;
               if (movingMi.Name != metric.Target.Name) // only use if different than target
                  break;
            }
            if (count < 0)
               continue;
            if (null == movingMi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): movingMi=null");
               return false;
            }
            //-----------------------------------------
            IMapItemMove? mim = gi.CreateMapItemMove(movingMi, metric.Target.TerritoryCurrent);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): CreateMapItemMove() returned mim=null");
               return false;
            }
            if (null == mim.OldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): CreateMapItemMove() returned mim.OldTerritory=null");
               return false;
            }
            if (null == mim.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Move_Uncontrolled(): CreateMapItemMove() returned mim.NewTerritory=null");
               return false;
            }
            alienMoves.Add(mim);
            metric.Target.IsMovingThisTurn = true;         // do not allow target to move 
            movingMi.IsMovingThisTurn = true;              // do not allow victim to move
            Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE, "Move_Uncontrolled(): Moving mi=" + mim.MapItem.Name + " from " + mim.OldTerritory.ToString() + " to " + mim.NewTerritory.ToString() + " usc=" + uncontrolledStackingCount.ToString() );

         }
         return true;
      }
      private bool IsAlienInGroup(IMapItems mapItems)
      {
         foreach (IMapItem mi in mapItems)
         {
            if (true == mi.IsAlienKnown || true == mi.IsAlienUnknown)
               return true;
         }
         return false;
      }
   }
}
