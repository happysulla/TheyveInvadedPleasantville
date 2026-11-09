using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Windows.ApplicationModel.Chat;

namespace PleasantvilleGame
{
   public class MetricUtility
   {
      public static int GetClosestDistanceMetric(IMapPoint center, ITerritory t, bool isInverse=false)
      {
         double xdiff = Math.Abs(center.X - t.CenterPoint.X);
         double ydiff = Math.Abs(center.Y - t.CenterPoint.Y);
         int rawDiff = (int) Math.Sqrt(xdiff*xdiff + ydiff*ydiff);
         // Normalize from 0 to 100
         // normalizedValue = (value - min(distance)) / ( maxValue - min(distance) ) * 100;
         // min(distance) = 0
         // Assume Map Hieght = 950 pixels
         // Assume Map Width = height * 0.86 = 818
         // maxDistance(x) = sqrt( (mapWidth)^2 + (mapHeight)^2 ) = 1253
         int normalized = (int)(100.0 * rawDiff / 1253); // = 100 if distance is larget and 0 if distance is small
         if (false == isInverse)
            return 100 - normalized; // 0=>distance is large and 100=>distance is small
         else
            return normalized;       // 100=>distance is large and 0=>distance is small
      }
   }
   //===============================================================
   public class MetricObservation : IMetricObservation
   {
      public IMapItem? MapItem1 { get; set; } = null;
      public IMapItem Target { get; set; }
      public int Value { get; set; } = 0;
      public MetricObservation(IGameInstance gi, IMapItem target)
      {
         Target = target;
         Value = GetObservationMetric(gi);
      }
      public MetricObservation(IGameInstance gi, IMapItem target, IMapItem alien)
      {
         MapItem1 = alien;
         Target = target;
         Value = GetObservationMetric(gi);
      }
      public int GetObservationMetric(IGameInstance gi)
      {
         if (null == MapItem1)
            return 15; // would prefer to move to territory with other map items to fool townsperson
         double pTotal = 1.0; // probability of observation = 1 - (1-p0)(1-p1)(1-p3).... where p is the probability of observation from that hex
         foreach (var kvp in Target.TerritoryCurrent.Observations)
         {
            IStack? stack = gi.Stacks.Find(kvp.Key); 
            if (null == stack) // might not be a stack in this Territory
               continue;
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null != MapItem1)  // do not count the target and alien
               {
                  if (this.MapItem1.Name == mi.Name)
                     continue;
               }
               if (null != Target)  // do not count the target and alien
               {
                  if (this.Target.Name == mi.Name)
                     continue;
               }
               if ((true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown)) // Aliens do not observe
                  continue;
               pTotal *= (1 - kvp.Value);
            }
         }
         int probability = (int)((100.0) * (pTotal));  // probability of not being observed
         return probability;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         if (null != MapItem1)
         {
            sb.Append("mi1=");
            sb.Append(this.MapItem1.Name);
            sb.Append("->");
         }
         if (null != Target)
         {
            sb.Append(this.Target.Name);
            sb.Append("=>");
         }
         sb.Append(this.Value.ToString());;
         return sb.ToString();
      }
   }
   public class MetricObservations : IMetricObservations
   {
      private readonly ArrayList myList;
      public MetricObservations() { myList = new ArrayList(); }
      public int Count { get { return myList.Count; } }
      public void Add(IMetricObservation metric) { myList.Add(metric); }
      public void Insert(int index, IMetricObservation metric) { myList.Insert(index, metric); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMetricObservation metric) { return myList.Contains(metric); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMetricObservation metric) { return myList.IndexOf(metric); }
      public void Remove(IMetricObservation metric) { myList.Remove(metric); }
      public IMetricObservation? RemoveAt(int index)
      {
         if (index < 0 || index >= myList.Count) return null;
         IMetricObservation? metric = myList[index] as IMetricObservation;
         myList.RemoveAt(index);
         return metric;
      }
      public IMetricObservation? this[int index]
      {
         get
         {
            IMetricObservation? metric = myList[index] as IMetricObservation;
            return metric;
         }
         set { myList[index] = value; }
      }
      public IMetricObservations Shuffle()
      {
         var shuffled = new MetricObservations();
         var rnd = new Random();
         var list = myList.Cast<IMetricObservation>().ToList();
         while (list.Count > 0)
         {
            int index = rnd.Next(list.Count);
            shuffled.Add(list[index]);
            list.RemoveAt(index);
         }
         return shuffled;
      }
      public IMetricObservations Sort()
      {
         IMetricObservations sortedMetrics = new MetricObservations();
         foreach (Object o in myList)
         {
            IMetricObservation metric1 = (IMetricObservation)o;
            bool isInserted = false;
            int index = 0;
            foreach (IMetricObservation metric2 in sortedMetrics)
            {
               if (metric2.Value < metric1.Value)
               {
                  sortedMetrics.Insert(index, metric1);
                  isInserted = true;
                  break;
               }
               ++index;
            }
            if (false == isInserted) // If not inserted, add to end
               sortedMetrics.Add(metric1);
         }
         return sortedMetrics;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (Object o in myList)
         {
            IMetricObservation m = (IMetricObservation)o;
            sb.Append("[");
            sb.Append(m.ToString());
            sb.Append("]");
         }
         return sb.ToString();
      }
   }
   //===============================================================
   public class MetricAlienMove : IMetricAlienMove
   {
      public IMapItem MovingMapItem { get; set; }
      public ITerritory TargetTerritory { get; set; }
      public int Value { get; set; }
      public MetricAlienMove(IGameInstance gi, IMapItem movingMi, ITerritory targetT, IMapPoint centerAlien, IMapPoint centerControlled )
      {
         // Lower Observations Chance
         // Greater Influence + Combat if allowed to move to multiple counters
         // Closer to Zebulon
         // Closer to Alien Center
         // Away from Controlled Center
         MovingMapItem = movingMi;
         TargetTerritory = targetT;
         //---------------------------
         IMetricObservation? obsMetric = new MetricObservation(gi, movingMi); // return metric value = 100 ==> 100% of not being observed
         IStack? stack = gi.Stacks.Find(targetT);
         if( null != stack )
         {
            IMapItem? targetMi = stack.MapItems[0];
            if( null == targetMi )
            {
               Logger.Log(LogEnum.LE_ERROR, "MetricAlienMove(): targetMi=null");
               return;
            }
            if( true == movingMi.IsAlien())
               obsMetric = new MetricObservation(gi, movingMi, targetMi);
         }
         //---------------------------
         int zebulonMetric = (MetricUtility.GetClosestDistanceMetric(gi.Zebulon.TerritoryCurrent.CenterPoint, targetT));
         int centerAlienMetric = MetricUtility.GetClosestDistanceMetric(centerAlien, targetT);
         int centerControlledMetric = MetricUtility.GetClosestDistanceMetric(centerControlled, targetT, true);
         int totalMetric = obsMetric.Value + zebulonMetric + centerAlienMetric + centerControlledMetric;
         Logger.Log(LogEnum.LE_SHOW_ALIEN_MOVE_METRIC, "MetricAlienMove(): mi=" + movingMi.Name + " (t=" + totalMetric.ToString() + ")\t=(o=" + obsMetric.Value.ToString() + ")+(z=" + zebulonMetric.ToString() + ")+(a=" + centerAlienMetric.ToString() + ")+(c=" + centerControlledMetric.ToString() + ") to t=" + targetT.ToString());
      }
   }
   public class MetricAlienMoves : IMetricAlienMoves
   {
      private readonly ArrayList myList;
      public MetricAlienMoves() { myList = new ArrayList(); }
      public int Count { get { return myList.Count; } }
      public void Add(IMetricAlienMove metric) { myList.Add(metric); }
      public void Insert(int index, IMetricAlienMove metric) { myList.Insert(index, metric); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMetricAlienMove metric) { return myList.Contains(metric); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMetricAlienMove metric) { return myList.IndexOf(metric); }
      public void Remove(IMetricAlienMove metric) { myList.Remove(metric); }
      public IMetricAlienMove? Find(ITerritory t)
      {
         foreach (IMetricAlienMove metric in myList)
            if (metric.TargetTerritory.ToString() == t.ToString()) return metric;
         return null;
      }
      public IMetricAlienMove? RemoveAt(int index)
      {
         if (index < 0 || index >= myList.Count) return null;
         IMetricAlienMove? metric = myList[index] as IMetricAlienMove;
         myList.RemoveAt(index);
         return metric;
      }
      public IMetricAlienMove? this[int index] 
      {
         get
         {
            IMetricAlienMove? metric = myList[index] as IMetricAlienMove;
            return metric;
         }
         set { myList[index] = value; }
      }
      public IMetricAlienMoves Shuffle()
      {
         var shuffled = new MetricAlienMoves();
         var rnd = new Random();
         var list = myList.Cast<IMetricAlienMove>().ToList();
         while (list.Count > 0)
         {
            int index = rnd.Next(list.Count);
            shuffled.Add(list[index]);
            list.RemoveAt(index);
         }
         return shuffled;
      }
      public IMetricAlienMoves Sort()
      {
         IMetricAlienMoves sortedMetrics = new MetricAlienMoves();
         foreach (Object o in myList)
         {
            IMetricAlienMove metric1 = (IMetricAlienMove)o;
            bool isInserted = false;
            int index = 0;
            foreach (IMetricAlienMove metric2 in sortedMetrics)
            {
               if (metric2.Value < metric1.Value)
               {
                  sortedMetrics.Insert(index, metric1);
                  isInserted = true;
                  break;
               }
               ++index;
            }
            if (false == isInserted) // If not inserted, add to end
               sortedMetrics.Add(metric1);
         }
         return sortedMetrics;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (Object o in myList)
         {
            IMetricAlienMove m = (IMetricAlienMove)o;
            sb.Append("<");
            sb.Append(m.TargetTerritory.ToString());
            sb.Append(">");
         }
         return sb.ToString();
      }
   }
   //===============================================================
}

