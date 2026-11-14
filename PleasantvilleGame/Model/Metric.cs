using PleasantvilleGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Shapes;
using Windows.ApplicationModel.Chat;

namespace PleasantvilleGame
{
   //===============================================================
   public class MetricAlienMove : IMetricAlienMove
   {
      public IMapItem MovingMapItem { get; set; }
      public ITerritory TargetTerritory { get; set; }
      public int MetricTotal { get; set; } = 0;
      public int MetricDenyTown { get; set; } = 0;
      public int MetricObs { get; set; } = 0;
      public int MetricTakeover { get; set; } = 0;
      public int MetricZebulon { get; set; } = 0;
      public int MetricAlienCenter { get; set; } = 0;
      public int MetricTownCenter { get; set; } = 0;
      public int MetricGuard { get; set; } = 0;
      //-----------------------------------------------------------
      public MetricAlienMove(IMapItem movingMi, ITerritory targetT)
      {
         MovingMapItem = movingMi;
         TargetTerritory = targetT;
      }
      //-----------------------------------------------------------
      public static int GetClosestDistanceMetric(IMapPoint center, ITerritory t)
      {
         double xdiff = Math.Abs(center.X - t.CenterPoint.X);
         double ydiff = Math.Abs(center.Y - t.CenterPoint.Y);
         int rawDiff = (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
         // Normalize from 0 to 100
         // normalizedValue = (value - min(distance)) / ( maxValue - min(distance) ) * 100;
         // min(distance) = 0
         // Assume Map Hieght = 950 pixels
         // Assume Map Width = height * 0.86 = 818
         // maxDistance(x) = sqrt( (mapWidth)^2 + (mapHeight)^2 ) = 1253
         int normalized = (int)(100.0 * rawDiff / 1253); // = 100 if distance is larget and 0 if distance is small
         return 100 - normalized; // 0=>distance is large and 100=>distance is small
      }
      public static int GetFarthestDistanceMetric(IMapPoint center, ITerritory t)
      {
         double xdiff = Math.Abs(center.X - t.CenterPoint.X);
         double ydiff = Math.Abs(center.Y - t.CenterPoint.Y);
         int rawDiff = (int)Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
         // Normalize from 0 to 100
         // normalizedValue = (value - min(distance)) / ( maxValue - min(distance) ) * 100;
         // min(distance) = 0
         // Assume Map Hieght = 950 pixels
         // Assume Map Width = height * 0.86 = 818
         // maxDistance(x) = sqrt( (mapWidth)^2 + (mapHeight)^2 ) = 1253
         int normalized = (int)(100.0 * rawDiff / 1253); // = 100 if distance is larget and 0 if distance is small
         return normalized;       // 100=>distance is large and 0=>distance is small
      }
      //-----------------------------------------------------------
      public bool SetObservationMetric(IStacks stacks)
      {
         double pTotal = 1.0; // probability of observation = 1 - (1-p0)(1-p1)(1-p3).... where p is the probability of observation from that hex
         foreach (var kvp in TargetTerritory.Observations)
         {
            IStack? stack = stacks.Find(kvp.Key);
            if (null == stack) // might not be a stack in this Territory
               continue;
            //----------------------------
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((this.MovingMapItem.Name == mi.Name) || (true == mi.IsAlien()) || (true == mi.IsKnockedout)) // aliens do not observer and cannot observe onself
                  continue;
               pTotal *= (1 - kvp.Value);
            }
         }
         MetricObs = (int)((100.0) * (pTotal));  // probability of not being observed
         return true;
      }
      public void SetTownDenyMetric(IStack? stack)
      {
         if( null == stack )
         {
            this.MetricDenyTown = 80;
            return;
         }
         //-------------------------
         bool isAlienInStack = false;
         foreach (IMapItem mi in stack.MapItems)
         {
            if ((true == mi.IsAlien()) && ("Zebulon" != mi.Name))
               isAlienInStack = true;
         }
         if (true == isAlienInStack)
            this.MetricDenyTown = 100;
         else
            this.MetricDenyTown = 80;
      }
      public void SetTakeoverMetric(IStack? stack)
      {
         if (null == stack)
            return;
         IMapItems aliens = new MapItems();
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolled = new MapItems();
         foreach (IMapItem mi in stack.MapItems)
         {
            if ((true == mi.IsKilled) || (true == mi.IsKnockedout) || (true == mi.IsSurrendered))  // Unconscious, dead, or surrendered cannot partipate
               continue;
            else if (true == mi.IsControlled && false == mi.IsStunned)
               townPeoples.Add(mi);
            else if (true == mi.IsAlien())
               aliens.Add(mi);
            else if( false == mi.IsWary )
               uncontrolled.Add(mi);
         }
         if( true == this.MovingMapItem.IsAlienUnknown)
         {
            switch (uncontrolled.Count)
            {
               case 0:
                  if( 0 < aliens.Count )
                     MetricTakeover = 50;
                  break;
               default:
                  MetricTakeover = 100;
                  break;
            }
         }
         else
         {
            switch (aliens.Count)
            {
               case 0:
                  if( 0 < uncontrolled.Count)
                     MetricTakeover = 50;
                  break;
               default:
                  MetricTakeover = 100;
                  break;
            }
         }
         if (0 < townPeoples.Count)
            MetricTakeover -= 50;
      }
      public bool SetTotalMetric(IStacks stacks, IMapPoint centerZebulon, IMapPoint centerAlien, IMapPoint centerControlled)
      {
         MetricZebulon = GetClosestDistanceMetric(centerZebulon, TargetTerritory);
         MetricAlienCenter = GetClosestDistanceMetric(centerAlien, TargetTerritory);
         MetricTownCenter = GetFarthestDistanceMetric(centerControlled, TargetTerritory);
         if (false == SetObservationMetric(stacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTotalMetric(): GetObservationMetric() returned error");
            return false;
         }
         IStack? stack = stacks.Find(this.TargetTerritory);
         SetTownDenyMetric(stack);
         SetTakeoverMetric(stack);
         MetricTotal = MetricObs + MetricTakeover + MetricZebulon + MetricAlienCenter + MetricTownCenter;
         return true;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(this.MovingMapItem.Name);
         sb.Append("->");
         sb.Append(this.TargetTerritory.ToString());
         sb.Append("=>");
         sb.Append(this.MetricTotal.ToString());
         sb.Append("=(");
         sb.Append(this.MetricDenyTown.ToString());
         sb.Append("+");
         sb.Append(this.MetricObs.ToString());
         sb.Append("+");
         sb.Append(this.MetricTakeover.ToString());
         sb.Append("+");
         sb.Append(this.MetricZebulon.ToString());
         sb.Append("+");
         sb.Append(this.MetricAlienCenter.ToString());
         sb.Append("+");
         sb.Append(this.MetricTownCenter.ToString());
         sb.Append("+");
         sb.Append(this.MetricGuard.ToString());
         sb.Append(")");
         return sb.ToString();
      }
   }
   //===============================================================
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
               if (metric2.MetricTotal < metric1.MetricTotal)
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
      public bool UpdateMetrics(IStacks stacks)
      {
         foreach (IMetricAlienMove mam in this)
         {
            if (false == mam.SetObservationMetric(stacks))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateMetrics(): SetObservationMetric() returned false");
               return false;
            }
            IStack? stack = stacks.Find(mam.TargetTerritory);
            if( null == stack )
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateMetrics(): stack=null");
               return false;
            }
            mam.SetTownDenyMetric(stack);
            mam.SetTakeoverMetric(stack);
         }
         return true;
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
}

