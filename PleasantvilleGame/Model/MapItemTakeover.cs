using PleasantvilleGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows.Documents;
using MessageBox=System.Windows.MessageBox;

namespace PleasantvilleGame
{
   [Serializable]
   class MapItemTakeover : IMapItemTakeover
   {
      public IMapItem Alien { get; set; } = new MapItem();
      public IMapItem Victim { get; set; } = new MapItem();
      public IMapItem? Observer { get; set; } = null;
      public bool IsConverted { get; set; } = false;
      public bool IsObserved { get; set; } = false;
      //--------------------------------------------------------
      public MapItemTakeover() { }
      public MapItemTakeover(IMapItemTakeover takeover)
      { 
      
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Alien=");
         sb.Append(Alien.ToString());
         sb.Append(" Victim=");
         sb.Append(Victim.ToString());
         sb.Append(" Observer=");
         if( null == Observer )
            sb.Append("null");
         else
            sb.Append(Observer.ToString());
         sb.Append(" IsConverted=");
         sb.Append(IsConverted.ToString());
         sb.Append(" IsObserved=");
         sb.Append(IsObserved.ToString());
         return sb.ToString();
      }
   }
   //==========================================================
   [Serializable]
   public class MapItemTakeovers : IEnumerable, IMapItemTakeovers
{
      private ArrayList myList;
      public MapItemTakeovers() { myList = new ArrayList(); }
      public void Add(IMapItemTakeover to) { myList.Add(to); }
      public IMapItemTakeover? RemoveAt(int index)
      {
         Object? o = myList[index];
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemTakeovers.RemoveAt(): null object at index " + index.ToString());
            return null;
         }
         IMapItemTakeover to = (IMapItemTakeover)o;
         myList.RemoveAt(index);
         return to;
      }
      public void Insert(int index, IMapItemTakeover to) { myList.Insert(index, to); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemTakeover to) { return myList.Contains(to); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemTakeover to) { return myList.IndexOf(to); }
      public IMapItemTakeover? this[int index]
      {
         get 
         {
            Object? o = myList[index];
            if (null == o)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemTakeovers.this(): null object at index " + index.ToString());
               return null;
            }
            IMapItemTakeover to = (IMapItemTakeover)o;
            return to; 
         }
         set { myList[index] = value; }
      }
   }
}
