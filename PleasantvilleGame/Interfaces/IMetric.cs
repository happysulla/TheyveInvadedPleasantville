using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   //=================================================================
   public interface IMetricAlienMove
   {
      IMapItem MovingMapItem { get; set; }
      ITerritory TargetTerritory { get; set; }
      int MetricTotal { get; set; }
      int MetricFixStacking { get; set; } // Fix stacking if overstacked with known aliens
      int MetricDenyTown { get; set; } // Deny Town Person access for Controlling
      int MetricObs { get; set; }
      int MetricZebulon { get; set; }
      int MetricAlienCenter { get; set; }
      int MetricTownCenter { get; set; }
      int MetricLeaving { get; set; }
      int MetricTakeover { get; set; }
      int MetricGuard { get; set; }
      void SetObservationMetric(IStack? stack, IStacks stacks, bool isPremove);
      void SetLeavingMetric(IStack? stack);
      void SetTakeoverMetric(IStack? stack, bool isPremove);
      void SetMetricAlienCenter(IMapPoint center, ITerritory starting, ITerritory ending);
      void SetMetricTownCenter(IMapPoint center, ITerritory starting, ITerritory ending);
      void SetMetricGuard(IStack? stack, IStacks stacks, bool isPremove);
      void SetTotalMetric(IStacks stacks, IMapPoint centerZebulon, IMapPoint centerAlien, IMapPoint centerControlled);
      void UpdateTotalMetric(IStacks stacks);
   }
   public interface IMetricAlienMoves : System.Collections.IEnumerable
   {
      int Count { get; }
      IMetricAlienMoves Clone();
      void Add(IMetricAlienMove metric);
      void Insert(int index, IMetricAlienMove metric);
      void Clear();
      bool Contains(IMetricAlienMove metric);
      int IndexOf(IMetricAlienMove metric);
      void Remove(IMetricAlienMove metric);
      IMetricAlienMove? RemoveAt(int index);
      IMetricAlienMove? this[int index] { get; set; }
      IMetricAlienMoves Shuffle();
      IMetricAlienMoves Sort();
      void UpdateMetrics(IStacks stacks);
   }
}
