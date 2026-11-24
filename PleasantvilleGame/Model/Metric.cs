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
            else if ((true == mi.IsControlled) && (false == mi.IsStunned))
               townPeoples.Add(mi);
            else if ( (true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown) ) // dont count Zebulon
               aliens.Add(mi);
            else if ((false == mi.IsWary) && ("Zebulon" != mi.Name))
               uncontrolled.Add(mi);
         }
         switch (uncontrolled.Count)  
         {
            case 0:
               this.MetricLeaving = 100;
               break;
            case 1:
               switch (aliens.Count)
               {
                  case 0:
                     this.MetricLeaving = 70;
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
            else if ((true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown)) // dont count Zebulon
               aliens.Add(mi);
            else if ( (false == mi.IsWary) && ("Zebulon" != mi.Name) )
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
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 15; // when uncontrolled moves, it may help a little
                     break;
                  default:
                     this.MetricTakeover = 35; // when alien moves, that is generally good
                     break;
               }
               break;
            case 1:
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 20;
                     break;
                  case 1:
                     this.MetricTakeover = 100;
                     break;
                  default:
                     this.MetricTakeover = 60;
                     break;
               }
               break;
            case 2:
               switch (alienCount)
               {
                  case 0:
                     this.MetricTakeover = 55;
                     break;
                  case 1:
                     this.MetricTakeover = 25;
                     break;
                  default:
                     this.MetricTakeover = 85;
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
                     this.MetricTakeover = 40;
                     break;
                  default:
                     this.MetricTakeover = 90;
                     break;
               }
               break;
         }
         if (0 < townPeoples.Count)
            MetricTakeover -= 50;
      }
      public void SetMetricAlienCenter(IMapPoint center, ITerritory starting, ITerritory ending)
      {
         double xdiffStart = Math.Abs(center.X - starting.CenterPoint.X);
         double ydiffStart = Math.Abs(center.Y - starting.CenterPoint.Y);
         int rawDiffStart = (int)Math.Sqrt(xdiffStart * xdiffStart + ydiffStart * ydiffStart);
         //-------------------------------------------------------
         double xdiffEnd = Math.Abs(center.X - ending.CenterPoint.X);
         double ydiffEnd = Math.Abs(center.Y - ending.CenterPoint.Y);
         int rawDiffEnd = (int)Math.Sqrt(xdiffEnd * xdiffEnd + ydiffEnd * ydiffEnd);
         //-------------------------------------------------------
         int normalizedEnd = 50 - Math.Min(50, (int)(50 * rawDiffEnd / 500));
         int diff = Math.Max(0, (rawDiffEnd - rawDiffStart));
         int normalizedDiff = 50 - Math.Min(50, (int)(50 * diff / 300));
         MetricAlienCenter = normalizedEnd + normalizedDiff;
      }
      public void SetMetricTownCenter(IMapPoint center, ITerritory starting, ITerritory ending)
      {
         double xdiffStart = Math.Abs(center.X - starting.CenterPoint.X);
         double ydiffStart = Math.Abs(center.Y - starting.CenterPoint.Y);
         int rawDiffStart = (int)Math.Sqrt(xdiffStart * xdiffStart + ydiffStart * ydiffStart);
         //-------------------------------------------------------
         double xdiffEnd = Math.Abs(center.X - ending.CenterPoint.X);
         double ydiffEnd = Math.Abs(center.Y - ending.CenterPoint.Y);
         int rawDiffEnd = (int)Math.Sqrt(xdiffEnd * xdiffEnd + ydiffEnd * ydiffEnd);
         //-------------------------------------------------------
         int normalizedEnd = Math.Min(30, (int)(30 * rawDiffEnd / 500));
         int diff = Math.Max(0, (rawDiffEnd - rawDiffStart));
         int normalizedDiff = Math.Min( 70, (int)(70 * diff / 300));
         MetricTownCenter = normalizedEnd + normalizedDiff;
      }
      public void SetMetricGuard(IStack? stack, IStacks stacks, bool isPremove)
      {
         MetricGuard = 0;
      }
      public void SetTotalMetric(IStacks stacks, IMapPoint centerZebulon, IMapPoint centerAlien, IMapPoint centerControlled)
      {
         IStack? currentStack = stacks.Find(this.MovingMapItem.TerritoryStarting);
         SetLeavingMetric(currentStack);
         IStack? targetStack = stacks.Find(this.TargetTerritory);
         SetObservationMetric(targetStack, stacks, true);
         SetTakeoverMetric(targetStack, true);
         MetricZebulon = GetClosestDistanceMetric(centerZebulon, TargetTerritory);
         SetMetricAlienCenter(centerAlien, this.MovingMapItem.TerritoryStarting, TargetTerritory);
         SetMetricTownCenter(centerControlled, this.MovingMapItem.TerritoryStarting, TargetTerritory);
         SetMetricGuard(targetStack, stacks, true);
         MetricTotal = MetricDenyTown + MetricObs + MetricLeaving + MetricTakeover + MetricZebulon + MetricAlienCenter + MetricTownCenter + MetricGuard;
      }
      public void UpdateTotalMetric(IStacks stacks)
      {
         int prevLeaving = this.MetricLeaving;
         IStack? currentStack = stacks.Find(this.MovingMapItem.TerritoryStarting);
         SetLeavingMetric(currentStack);
         if (null != currentStack)
            Logger.Log(LogEnum.LE_SHOW_MIM, "Update_TotalMetric(): mi=" + this.MovingMapItem.Name + " current=" + currentStack.ToString() + " " + prevLeaving.ToString() + "->" + this.MetricLeaving.ToString());
         IStack? targetStack = stacks.Find(this.TargetTerritory);
         int prevObs = this.MetricObs;
         int prevTake = this.MetricTakeover;
         SetObservationMetric(targetStack, stacks, false);
         SetTakeoverMetric(targetStack, false);
         if (null != targetStack)
            Logger.Log(LogEnum.LE_SHOW_MIM, "Update_TotalMetric(): mi=" + this.MovingMapItem.Name + " target=" + targetStack.ToString() + " " + prevObs.ToString() + "->" + this.MetricObs.ToString() + " " + prevTake.ToString() + "->" + this.MetricTakeover.ToString());
         MetricTotal = MetricDenyTown + MetricObs + MetricLeaving + MetricTakeover + MetricZebulon + MetricAlienCenter + MetricTownCenter + MetricGuard;
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
      public IMetricAlienMoves Clone()
      {
         IMetricAlienMoves clone = new MetricAlienMoves();
         foreach (IMetricAlienMove metric in myList)
            clone.Add(metric);
         return clone;
      }
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
            IStack? currentStack = stacks.Find(mam.MovingMapItem.TerritoryCurrent);
            if( null != currentStack )
            {

            }
            mam.SetLeavingMetric(currentStack);
            IStack? targetStack = stacks.Find(mam.TargetTerritory);
            mam.SetTakeoverMetric(targetStack, false);
            mam.SetObservationMetric(targetStack, stacks, false);
         }
      }
      public override string ToString()
      {
         IMetricAlienMoves moves = this.Sort();
         StringBuilder sb = new StringBuilder();
         foreach (IMetricAlienMove move in moves)
         {
            sb.Append("<");
            sb.Append(move.MovingMapItem.Name);
            sb.Append("=>");
            sb.Append(move.TargetTerritory.ToString());
            sb.Append("(");
            sb.Append(move.MetricTotal.ToString());
            sb.Append(")>");
         }
         return sb.ToString();
      }
   }
}

