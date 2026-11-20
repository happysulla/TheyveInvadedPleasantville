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
      public int MetricLeaving { get; set; } = 0;
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
         int normalized = Math.Min(100, (int)(100.0 * rawDiff / 1200)); // = 100 if distance is larget and 0 if distance is small
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
         int normalized = Math.Min(100, (int)(100.0 * rawDiff / 1000)); // = 100 if distance is larget and 0 if distance is small
         return normalized;       // 100=>distance is large and 0=>distance is small
      }
      //-----------------------------------------------------------
      public void SetLeavingMetric(IStack? currentStack)
      {
         if (null == currentStack)
            return;
         IMapItems aliens = new MapItems();
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolled = new MapItems();
         foreach (IMapItem mi in currentStack.MapItems)
         {
            if ((true == mi.IsKilled) || (true == mi.IsKnockedout) || (true == mi.IsSurrendered))  // Unconscious, dead, or surrendered cannot partipate
               continue;
            else if (true == mi.IsControlled && false == mi.IsStunned)
               townPeoples.Add(mi);
            else if (true == mi.IsAlien())
               aliens.Add(mi);
            else if (false == mi.IsWary)
               uncontrolled.Add(mi);
         }
         switch (uncontrolled.Count) // subtract if moving away from 
         {
            case 0:
               this.MetricLeaving = 100;
               break;
            case 1:
               switch (aliens.Count)
               {
                  case 0:
                     this.MetricLeaving = 100;
                     break;
                  default:
                     this.MetricLeaving = 0;
                     break;
               }
               break;
            case 2:
               switch (aliens.Count)
               {
                  case 0:
                     this.MetricLeaving = 20;
                     break;
                  case 1:
                     this.MetricLeaving = 10;
                     break;
                  default:
                     this.MetricLeaving = 0;
                     break;
               }
               break;
            default:
               switch (aliens.Count)
               {
                  case 0:
                     this.MetricLeaving = 50;
                     break;
                  case 1:
                     this.MetricLeaving = 10;
                     break;
                  default:
                     this.MetricLeaving = 0;
                     break;
               }
               break;
         }
         if (0 < townPeoples.Count)
            MetricTakeover = Math.Max(100, MetricTakeover+50);
      }
      public void SetObservationMetric(IStack? stack, IStacks stacks, bool isPremove)
      {
         int stackCount = 3;
         if (true == isPremove)
            stackCount = 2;
         double pTotal = 1.0; // probability of observation = 1 - (1-p0)(1-p1)(1-p3).... where p is the probability of observation from that hex
         foreach (var kvp in TargetTerritory.Observations)
         {
            IStack? stack1 = stacks.Find(kvp.Key);
            if (null == stack1) // might not be a stack in this Territory
               continue;
            //----------------------------
            if ((null != stack) && (stack.Territory.ToString() == stack1.Territory.ToString()))
            {
               if (stackCount == stack1.MapItems.Count)
               {
                  pTotal *= 0.33334;
               }
               continue;
            }
            foreach (IMapItem mi in stack1.MapItems)
            {
               if ((this.MovingMapItem.Name == mi.Name) || (true == mi.IsAlien()) || (true == mi.IsKnockedout) || (true == mi.IsKilled)) // aliens do not observer and cannot observe onself
                  continue;
               pTotal *= (1 - kvp.Value);
            }
         }
         MetricObs = (int)((100.0) * (pTotal));  // probability of not being observed
      }
      public void SetTakeoverMetric(IStack? targetStack, bool isPremove)
      {
         if (null == targetStack)
            return;
         IMapItems aliens = new MapItems();
         IMapItems townPeoples = new MapItems();
         IMapItems uncontrolled = new MapItems();
         foreach (IMapItem mi in targetStack.MapItems)
         {
            if ((true == mi.IsKilled) || (true == mi.IsKnockedout) || (true == mi.IsSurrendered))  // Unconscious, dead, or surrendered cannot partipate
               continue;
            else if (true == mi.IsControlled && false == mi.IsStunned)
               townPeoples.Add(mi);
            else if (true == mi.IsAlien())
               aliens.Add(mi);
            else if (false == mi.IsWary)
               uncontrolled.Add(mi);
         }
         int uncontrolledCount = uncontrolled.Count;
         int alienCount = aliens.Count;
         if (true == isPremove)
         {
            if (true == MovingMapItem.IsAlien())
               alienCount++;
            else
               uncontrolledCount++;
         }
         switch (uncontrolledCount)
         {
            case 0:
               this.MetricTakeover = 50;
               break;
            case 1:
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 50;
                     break;
                  case 1:
                     this.MetricTakeover = 100;
                     break;
                  default:
                     this.MetricTakeover = 70;
                     break;
               }
               break;
            case 2:
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 65;
                     break;
                  case 1:
                     this.MetricTakeover = 30;
                     break;
                  default:
                     this.MetricTakeover = 95;
                     break;
               }
               break;
            default:
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 20;
                     break;
                  case 1:
                     this.MetricTakeover = 50;
                     break;
                  default:
                     this.MetricTakeover = 100;
                     break;
               }
               break;
         }
         if (0 < townPeoples.Count)
            MetricTakeover -= 50;
      }
      public bool SetTotalMetric(IStacks stacks, IMapPoint centerZebulon, IMapPoint centerAlien, IMapPoint centerControlled)
      {
         IStack? targetStack = stacks.Find(this.TargetTerritory);
         IStack? currentStack = stacks.Find(this.MovingMapItem.TerritoryCurrent);
         SetLeavingMetric(currentStack);
         SetObservationMetric(targetStack, stacks, true);
         SetTakeoverMetric(targetStack, true);
         MetricZebulon = GetClosestDistanceMetric(centerZebulon, TargetTerritory);
         MetricAlienCenter = GetClosestDistanceMetric(centerAlien, TargetTerritory);
         MetricTownCenter = GetFarthestDistanceMetric(centerControlled, TargetTerritory);
         MetricTotal = MetricDenyTown + MetricObs + MetricLeaving + MetricTakeover + MetricZebulon + MetricAlienCenter + MetricTownCenter + MetricGuard;
         return true;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(this.MovingMapItem.Name);
         int count = 25 - this.MovingMapItem.Name.Count();
         for (int i = 0; i < count; i++)
            sb.Append(' ');
         sb.Append(this.MetricTotal.ToString());
         sb.Append("=(D");
         sb.Append(this.MetricDenyTown.ToString());
         sb.Append("+L");
         sb.Append(this.MetricLeaving.ToString());
         sb.Append("+O");
         sb.Append(this.MetricObs.ToString());
         sb.Append("+T");
         sb.Append(this.MetricTakeover.ToString());
         sb.Append("+Z");
         sb.Append(this.MetricZebulon.ToString());
         sb.Append("+A");
         sb.Append(this.MetricAlienCenter.ToString());
         sb.Append("+C");
         sb.Append(this.MetricTownCenter.ToString());
         sb.Append("+G");
         sb.Append(this.MetricGuard.ToString());
         sb.Append(")->(");
         sb.Append(this.MovingMapItem.TerritoryStarting.ToString());
         sb.Append("->");
         sb.Append(this.TargetTerritory.ToString());
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
      public void UpdateMetrics(IStacks stacks)
      {
         foreach (IMetricAlienMove mam in this)
         {
            IStack? stack = stacks.Find(mam.TargetTerritory);
            if (null == stack)
               return;
            mam.SetTakeoverMetric(stack, false);
            mam.SetObservationMetric(stack, stacks, false);
         }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (Object o in myList)
         {
            IMetricAlienMove m = (IMetricAlienMove)o;
            sb.Append("<");
            sb.Append(m.MovingMapItem.Name);
            sb.Append("=>");
            sb.Append(m.TargetTerritory.ToString());
            sb.Append("(");
            sb.Append(m.MetricTotal.ToString());
            sb.Append(")>");
         }
         return sb.ToString();
      }
   }
}

