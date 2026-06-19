using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IMetricAlienMove
   {
      ITerritory Territory { get; set; }
      int Value { get; set; }
      int GetObservationMetric(IGameInstance gi);
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
      IMetricAlienMove? Find(ITerritory t);
      IMetricAlienMove? RemoveAt(int index);
      IMetricAlienMove? this[int index] { get; set; }
      IMetricAlienMoves Shuffle();
      public IMetricAlienMoves Sort();
   }
}
