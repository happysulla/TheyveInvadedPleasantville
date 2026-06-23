using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienComputerMoveMgr
   {
      public IMapItems Targets { get; set; } = new MapItems();
      public PlayerAlienComputerMoveMgr(){}
      public bool MoveKnownAliens()
      {
         return true;
      }
      public bool MoveUnknownAliens(IGameInstance gi, IMapItems unknownAliens, IMapItemMoves alienMoves )
      {
         foreach (IMapItem unknownAlien in unknownAliens)
         {
            if (5 < alienMoves.Count) // only move five units
               break;
            IMapItems? closeMapItems = Territory.GetMapItemsWithinRange(gi, unknownAlien.TerritoryCurrent, unknownAlien.Movement);
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): GetMapItemsWithinRange() returned error");
               return false;
            }
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): GetMapItemsWithinRange() returned error");
               return false;
            }
            //-----------------------------------------
            IMetricObservations metrics = new MetricObservations();
            foreach (IMapItem mi in closeMapItems)
            {
               if ((true == mi.IsControlled) || (true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown)) // do not include controlled MapItems as candidates
                  continue;
               IMetricObservation metric = new MetricObservation(gi, mi);
               metrics.Add(metric);
            }
            IMetricObservations sortedMetrics = metrics.Sort();
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Perform_AlienMoves(): for unknownAlien=" + unknownAlien.Name + " sortedMetrics=" + sortedMetrics.ToString());
            if (0 == sortedMetrics.Count) // nobody to move to
               continue;
            //-----------------------------------------
            IMetricObservation? metricVictim = sortedMetrics[0];
            if (null == metricVictim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): metricVictim=null");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Perform_AlienMoves(): for unknownAlien=" + unknownAlien.Name + " metricVictim=" + metricVictim.ToString());
            //-----------------------------------------
            unknownAlien.IsMovingThisTurn = true;         // do not allow alien to move other than to the victim
            metricVictim.Target.IsMovingThisTurn = true;  // do not allow victim to move
            if( unknownAlien.TerritoryCurrent.ToString() != metricVictim.Target.TerritoryCurrent.ToString()) // If in same territory, do not move
            {
               IMapItemMove? mim = null;
               IMetricObservation metricAlien = new MetricObservation(gi, unknownAlien); // Is it better to move unknown to victum or victum to unknown
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Perform_AlienMoves(): for metricAlien=" + metricAlien.ToString());
               if (metricVictim.Value < metricAlien.Value) // higher probability of non-detection is path we want to go
                  mim = gi.CreateMapItemMove(metricVictim.Target, unknownAlien.TerritoryCurrent);
               else
                  mim = gi.CreateMapItemMove(unknownAlien, metricVictim.Target.TerritoryCurrent);
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): CreateMapItemMove() returned null");
                  return false;
               }
               alienMoves.Add(mim);
            }
         }
         return true;
      }
      public bool IntersectAlienUncontrolled(IGameInstance gi, IMapItems unknownAliens, IMapItems uncontrolledPeoples, IMapItemMoves alienMoves)
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
      public bool MoveUncontrolled(IGameInstance gi, IMapItems uncontrolledPeoples, IMapItemMoves alienMoves)
      {
         IMetricObservations uncontrolledMetrics = new MetricObservations();
         foreach (IMapItem uncontrolled in uncontrolledPeoples)
         {
            IMetricObservation metric = new MetricObservation(gi, uncontrolled);
            uncontrolledMetrics.Add(metric);
         }
         IMetricObservations sortedUncontrolledMetrics = uncontrolledMetrics.Sort();
         Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Perform_AlienMoves(): for sortedUncontrolledMetrics=" + sortedUncontrolledMetrics.ToString());
         //----------------------------------------------------------------
         foreach (IMetricObservation metric in sortedUncontrolledMetrics)
         {
            if (5 < alienMoves.Count) // only move five units
               break;
            IStack? stack = gi.Stacks.Find(metric.Target.TerritoryCurrent);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): stack=null for t=" + metric.Target.TerritoryCurrent.ToString());
               return false;
            }
            IMapItem? anchor = stack.MapItems[0];
            if (null == anchor)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): stack=null for t=" + metric.Target.TerritoryCurrent.ToString());
               return false;
            }

            IMapItems? closeMapItems = Territory.GetMapItemsWithinRange(gi, anchor.TerritoryCurrent, anchor.Movement);
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): GetMapItemsWithinRange() returned error");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS_METRIC, "Perform_AlienMoves(): for anchor=" + anchor.ToString() + " has closeMapItems=" + closeMapItems.ToString());
            if (closeMapItems.Count < 2)
               continue;
            //-----------------------------------------
            IMapItem? movingMi = null;
            int count = 100;
            while ( 0 < count-- )
            {
               int randomNum = Utilities.RandomGenerator.Next(closeMapItems.Count);
               movingMi = closeMapItems[randomNum];
               if (null == movingMi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): movingMi=null for randUm=" + randomNum.ToString() + " in closeMapItems=" + closeMapItems.ToString());
                  return false;
               }
               if (true == movingMi.IsMovingThisTurn) // already moving or targeted so skip moving
                  continue;
               if (movingMi.TerritoryCurrent.ToString() == anchor.TerritoryCurrent.ToString()) // If in same territory, do not move
                  continue;
               if (movingMi.Name != anchor.Name) // only use if different than anchor
                  break;
            }
            if (count < 0)
               continue;
            if (null == movingMi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): movingMi=null");
               return false;
            }
            anchor.IsMovingThisTurn = true;         // do not allow alien to move other than to the victim
            movingMi.IsMovingThisTurn = true; // do not allow victim to move
            //-----------------------------------------
            IMapItemMove? mim = gi.CreateMapItemMove(movingMi, anchor.TerritoryCurrent);
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): CreateMapItemMove() returned null");
               return false;
            }
            alienMoves.Add(mim);
         }
         return true;
      }
   }
}
