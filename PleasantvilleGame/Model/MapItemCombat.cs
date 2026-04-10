using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using MessageBox=System.Windows.MessageBox;

namespace PleasantvilleGame
{
   [Serializable]
   class MapItemCombat : IMapItemCombat
   {
      private IMapItems myAttackers = new MapItems();
      public IMapItems Attackers
      {
         get { return myAttackers; }
         set { myAttackers = value; }
      }
      private IMapItems myDefenders = new MapItems();
      public IMapItems Defenders
      {
         get { return myDefenders; }
         set { myDefenders = value; }
      }
      private ITerritory myTerritory = null;
      public ITerritory Territory
      {
         get { return myTerritory; }
         set { myTerritory = value; }
      }
      private CombatResult myResult = CombatResult.AttackerWins;
      public CombatResult Result
      {
         get { return myResult; }
         set { myResult = value; }
      }
      private int myDieRoll1 = 0;
      public int DieRoll1
      {
         get { return myDieRoll1; }
         set { myDieRoll1 = value; }
      }
      private int myDieRoll2 = 0;
      public int DieRoll2
      {
         get { return myDieRoll2; }
         set { myDieRoll2 = value; }
      }
      private bool myIsAnyRetreat = false;
      public bool IsAnyRetreat
      {
         get { return myIsAnyRetreat; }
         set { myIsAnyRetreat = value; }
      }
      //--------------------------------------------------------
      public MapItemCombat() { }
      public MapItemCombat(ITerritory t)
      {
         myTerritory = t;
      }
      public MapItemCombat(IMapItemCombat combat)
      {
         if (null != combat.Attackers)
         {
            myAttackers.Clear();
            foreach (IMapItem mi1 in combat.Attackers)
               myAttackers.Add(mi1);
         }
         if (null != combat.Defenders)
         {
            myDefenders.Clear();
            foreach (IMapItem mi2 in combat.Defenders)
               myDefenders.Add(mi2);
         }
         myTerritory = combat.Territory;
         myResult = combat.Result;
         myDieRoll1 = combat.DieRoll1;
         myDieRoll2 = combat.DieRoll2;
         myIsAnyRetreat = combat.IsAnyRetreat;
      }
   }
   //==========================================================
   [Serializable]
   public class MapItemCombats : IEnumerable, IMapItemCombats
   {
      private ArrayList myList;
      public MapItemCombats() { myList = new ArrayList(); }
      public void Add(IMapItemCombat cr) { myList.Add(cr); }
      public IMapItemCombat RemoveAt(int index)
      {
         IMapItemCombat cr = (IMapItemCombat)myList[index];
         myList.RemoveAt(index);
         return cr;
      }
      public void Insert(int index, IMapItemCombat cr) { myList.Insert(index, cr); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemCombat cr) { return myList.Contains(cr); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemCombat cr) { return myList.IndexOf(cr); }
      public IMapItemCombat this[int index]
      {
         get { return (IMapItemCombat)(myList[index]); }
         set { myList[index] = value; }
      }
   }
}
