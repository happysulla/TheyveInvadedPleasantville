using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PleasantvilleGame
{
   public enum CombatResult
   {
      DefenderWins,
      AttackerWins, 
      DefenderFlees, 
      AttackerFlees, 
      Error
   };
   public interface IMapItemCombat
   {
      IMapItems Attackers { get; set; }
      IMapItems Defenders { get; set; }
      ITerritory Territory { get; set; }
      CombatResult Result { get; set; }
      int DieRoll1 { get; set; }
      int DieRoll2 { get; set; }
      bool IsAnyRetreat { get; set; }
   }
   public interface IMapItemCombats : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItemCombat cr);
      IMapItemCombat RemoveAt(int index);
      void Insert(int index, IMapItemCombat cr);
      void Clear();
      bool Contains(IMapItemCombat cr);
      int IndexOf(IMapItemCombat cr);
      IMapItemCombat this[int index] { get; set; }
   }
}
