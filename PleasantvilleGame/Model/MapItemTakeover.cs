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
      public bool CtorError { get; } = false;
      public IMapItem Alien { get; set; } = new MapItem();
      public IMapItem Victim { get; set; } = new MapItem();
      public IMapItems Observers { get; set; } = new MapItems();
      //--------------------------------------------------------
      public MapItemTakeover() { }
      public MapItemTakeover(IGameInstance gi, IMapItem alien, IMapItem victim) 
      {
         Alien = alien;
         Victim = victim;
         ITerritory t = alien.TerritoryCurrent;
         foreach (KeyValuePair<String, double> kvp1 in t.Observations) // look thru all territories that can observe this takeover
         {
            ITerritory? t1 = Territories.theTerritories.Find(kvp1.Key); // kvp1.Key=Territory_Name, kvp1.Value=Observe_Probability
            if (null == t1)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemTakeover(): t1=null for " + kvp1.Key);
               CtorError = true;
               return;
            }
            IStack? stackObs = gi.Stacks.Find(t1); // could be nobody in this observation territory
            if (null == stackObs)
               continue;
            foreach (IMapItem possibleObserver in stackObs.MapItems)
            {
               if ((true == possibleObserver.Name.Contains(alien.Name)) || (true == possibleObserver.Name.Contains(victim.Name)))
                  continue;
               if (true == possibleObserver.IsAlienKnown) // known aliens do not observer. Unknown aliens need to be listed so that town person does not suspect them as alien, but they will not find anything
                  continue;
               if ((true == possibleObserver.IsKilled) || (true == possibleObserver.IsKnockedout)) // stunned or killed people cannot observe
                  continue;
               Logger.Log(LogEnum.LE_SHOW_OBSERVATIONS, "MapItemTakeover(): t1=" + t1.ToString() + " obs=" + possibleObserver.ToString() + " l=" + alien.ToString() + " r=" + victim.ToString());
               Observers.Add(possibleObserver);
            }
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Alien=");
         sb.Append(Alien.ToString());
         sb.Append(" Victim=");
         sb.Append(Victim.ToString());
         sb.Append(" Observers={");
         for (int i=0; i<Observers.Count; ++i)
         {
            IMapItem? o = Observers[i];
            if( null != o )
            {
               sb.Append(o.ToString());
               if( i != Observers.Count-1)
                  sb.Append(",");
            }
         }
         sb.Append("}");
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
