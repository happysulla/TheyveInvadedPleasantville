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
      private ITerritory myTerritory = new Territory();
      public ITerritory Territory
      {
         get { return myTerritory; }
         set { myTerritory = value; }
      }
      private CombatResult myResult = CombatResult.Error;
      public CombatResult Result
      {
         get { return myResult; }
         set { myResult = value; }
      }
      private int myDieRoll = Utilities.NO_RESULT;
      public int DieRoll
      {
         get { return myDieRoll; }
         set { myDieRoll = value; }
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
         myDieRoll = combat.DieRoll;
      }
      public void Clear()
      {
         myAttackers.Clear();
         myDefenders.Clear();
         myTerritory = new Territory();
         myResult = CombatResult.Error;
         myDieRoll = Utilities.NO_RESULT;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Attackers=");
         int totalAttackCombat = 0;
         foreach (IMapItem mi in myAttackers)
         {
            sb.Append(mi.Name);
            sb.Append("(");
            sb.Append(mi.Combat.ToString());
            sb.Append(") ");
            totalAttackCombat += mi.Combat;
         }
         sb.Append("Defenders=");
         int totalDefendCombat = 0;
         foreach (IMapItem mi in myDefenders)
         {
            sb.Append(mi.Name);
            sb.Append("(");
            sb.Append(mi.Combat.ToString());
            sb.Append(") ");
            totalDefendCombat += mi.Combat;
         }
         sb.Append("in ");
         sb.Append(myTerritory.ToString());
         sb.Append(" odds=(");
         sb.Append(totalAttackCombat.ToString());
         sb.Append("vs");
         sb.Append(totalDefendCombat.ToString());
         sb.Append(") Result=");
         sb.Append(myResult.ToString());
         return sb.ToString();
      }
   }
   //==========================================================
   [Serializable]
   public class MapItemCombats : IEnumerable, IMapItemCombats
   {
      private ArrayList myList;
      public MapItemCombats() { myList = new ArrayList(); }
      public void Add(IMapItemCombat cr) { myList.Add(cr); }
      public IMapItemCombat? RemoveAt(int index)
      {
         Object? o = myList[index];
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR,"MapItemCombats.RemoveAt(): null object at index " + index.ToString());
            return null;
         }
         IMapItemCombat cr = (IMapItemCombat)o;
         myList.RemoveAt(index);
         return cr;
      }
      public void Insert(int index, IMapItemCombat cr) { myList.Insert(index, cr); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemCombat cr) { return myList.Contains(cr); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemCombat cr) { return myList.IndexOf(cr); }
      public IMapItemCombat? this[int index]
      {
         get 
         {
            Object? o = myList[index];
            if (null == o)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemCombats.RemoveAt(): null object at index " + index.ToString());
               return null;
            }
            IMapItemCombat cr = (IMapItemCombat)o;
            return cr; 
         }
         set { myList[index] = value; }
      }
   }
}
