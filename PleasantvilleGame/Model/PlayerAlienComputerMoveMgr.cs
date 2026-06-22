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
      public bool MoveUnknownAliens(IGameInstance gi, IMapItems unknownAliens, ref IMapItemMoves alienMoves )
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
               IMetricObservation metric = new MetricObservation(mi);
               metric.Value = metric.GetObservationMetric(gi);
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
            //-----------------------------------------
            unknownAlien.IsMovingThisTurn = true;         // do not allow alien to move other than to the victim
            metricVictim.Target.IsMovingThisTurn = true; // do not allow victim to move
            if( unknownAlien.TerritoryCurrent.ToString() != metricVictim.Target.TerritoryCurrent.ToString()) // If in same territory, do not move
            {
               IMapItemMove? mim = null;
               IMetricObservation metricAlien = new MetricObservation(unknownAlien); // Is it better to move unknown to victum or victum to unknown
               if (metricAlien.Value < metricVictim.Value)
                  mim = gi.CreateMapItemMove(unknownAlien, metricAlien.Target.TerritoryCurrent);
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
      public bool IntersectAlienUncontrolled(IGameInstance gi, IMapItems unknownAliens, IMapItems uncontrolledPeoples, ref IMapItemMoves alienMoves)
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
      public bool MoveUncontrolled(IGameInstance gi, IMapItems uncontrolledPeoples, ref IMapItemMoves alienMoves)
      {
         IMetricObservations uncontrolledMetrics = new MetricObservations();
         foreach (IMapItem uncontrolled in uncontrolledPeoples)
         {
            foreach (IMapItem mi in uncontrolledPeoples)
            {
               if ((true == mi.IsControlled) || (true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown)) // do not include controlled MapItems as candidates
                  continue;
               IMetricObservation metric = new MetricObservation(mi);
               metric.Value = metric.GetObservationMetric(gi);
               uncontrolledMetrics.Add(metric);
            }
         }
         IMetricObservations sortedUncontrolledMetrics = uncontrolledMetrics.Sort();
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
            int randomNum = Utilities.RandomGenerator.Next(closeMapItems.Count);
            IMapItem? movingMi = closeMapItems[randomNum];
            if (null == movingMi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): movingMi=null for randUm=" + randomNum.ToString() + " in closeMapItems=" + closeMapItems.ToString());
               return false;
            }
            if (movingMi.TerritoryCurrent.ToString() != anchor.TerritoryCurrent.ToString()) // If in same territory, do not move
               continue;
            if (true == movingMi.IsMovingThisTurn) // already moving or targeted  so skip moving
               continue;
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
