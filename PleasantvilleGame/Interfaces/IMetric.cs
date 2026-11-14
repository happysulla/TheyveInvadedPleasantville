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
      int MetricDenyTown { get; set; } // Deny Town Person access for Controlling
      int MetricObs { get; set; }
      int MetricZebulon { get; set; }
      int MetricAlienCenter { get; set; }
      int MetricTownCenter { get; set; }
      int MetricTakeover { get; set; }
      int MetricGuard { get; set; }
      bool SetObservationMetric(IStacks stacks);
      void SetTownDenyMetric(IStack? stack);
      void SetTakeoverMetric(IStack? stack);
      bool SetTotalMetric(IStacks stacks, IMapPoint centerZebulon, IMapPoint centerAlien, IMapPoint centerControlled);
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
      IMetricAlienMoves Sort();
      bool UpdateMetrics(IStacks stacks);
   }
}
