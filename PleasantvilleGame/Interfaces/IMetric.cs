using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IMetricObservation
   {
      IMapItem TargetMapItem { get; set; }
      ITerritory TargetTerritory { get; set; }
      int Metric1 { get; set; }
      int Metric2 { get; set; }
   }
   public interface IMetricObservations : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMetricObservation metric);
      void Insert(int index, IMetricObservation metric);
      void Clear();
      bool Contains(IMetricObservation metric);
      int IndexOf(IMetricObservation metric);
      void Remove(IMetricObservation metric);
      IMetricObservation? RemoveAt(int index);
      IMetricObservation? this[int index] { get; set; }
      IMetricObservations Shuffle();
      public IMetricObservations Sort();
   }
   //=================================================================
   public interface IMetricAlienMove
   {
      IMapItem MovingMapItem { get; set; }
      ITerritory TargetTerritory { get; set; }
      int Value { get; set; }
   }
   public interface IMetricAlienMoves : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMetricAlienMove metric);
      void Insert(int index, IMetricAlienMove metric);
      void Clear();
      bool Contains(IMetricAlienMove metric);
      int IndexOf(IMetricAlienMove metric);
      void Remove(IMetricAlienMove metric);
      IMetricAlienMove? RemoveAt(int index);
      IMetricAlienMove? this[int index] { get; set; }
      IMetricAlienMoves Shuffle();
      public IMetricAlienMoves Sort();
   }
}
