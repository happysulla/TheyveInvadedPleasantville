using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IMapItemTakeover
   {
      IMapItem Alien { get; set; }
      IMapItem Victim { get; set; }
      IMapItem? Observer { get; set; }
      bool IsConverted { get; set; }
      bool IsObserved { get; set; }
   }
   public interface IMapItemTakeovers : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItemTakeover to);
      IMapItemTakeover? RemoveAt(int index);
      void Insert(int index, IMapItemTakeover to);
      void Clear();
      bool Contains(IMapItemTakeover to);
      int IndexOf(IMapItemTakeover to);
      IMapItemTakeover? this[int index] { get; set; }
   }
}
