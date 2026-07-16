using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Windows.ApplicationModel.Chat;

namespace PleasantvilleGame
{
   public class MetricObservation : IMetricObservation
   {
      public IMapItem? Alien { get; set; } = null;
      public IMapItem Target { get; set; }
      public int Value { get; set; }
      public MetricObservation(IGameInstance gi, IMapItem target)
      {
         Target = target;
         Value = GetObservationMetric(gi);
      }
      public MetricObservation(IGameInstance gi, IMapItem target, IMapItem alien)
      {
         Alien = alien;
         Target = target;
         Value = GetObservationMetric(gi);
      }
      private int GetObservationMetric(IGameInstance gi)
      {
         double pTotal = 1.0; // probability of observation = 1 - (1-p0)(1-p1)(1-p3).... where p is the probability of observation from that hex
         foreach (var kvp in Target.TerritoryCurrent.Observations)
         {
            IStack? stack = gi.Stacks.Find(kvp.Key); 
            if (null == stack) // might not be a stack in this Territory
               continue;
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null != Alien)  // do not count the target and alien
               {
                  if (this.Alien.Name == mi.Name)
                     continue;
               }
               if (this.Target.Name == mi.Name)   // do not count the target 
                  continue;
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
         sb.Append("<");
         sb.Append(this.Target.Name);
         sb.Append(",");
         sb.Append(this.Value.ToString());
         sb.Append(">");
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
      public IMetricObservation? Find(ITerritory t)
      {
         foreach (IMetricObservation metric in myList)
         {
            if (metric.Target.ToString() == t.ToString())
               return metric;
         }
         return null;
      }
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
      public ITerritory Territory { get; set; }
      public int Value { get; set; }
      public MetricAlienMove(ITerritory t)
      {
         Territory = t;
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
            if (metric.Territory.ToString() == t.ToString()) return metric;
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
            sb.Append(m.Territory.ToString());
            sb.Append(">");
         }
         return sb.ToString();
      }
   }
   //===============================================================
}

