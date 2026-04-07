using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Xps.Serialization;

namespace PleasantvilleGame
{
   [Serializable]
   public struct BloodSpot
   {
      public int mySize;      // diameter  of blood spot
      public double myLeft;   // left of where blood spot exists on canvas
      public double myTop;    // top of where blood spot exists on canvas
      public BloodSpot(int range, Random r)
      {
         mySize = r.Next(5) + 3;
         myLeft = r.Next(0, range - mySize);
         myTop = r.Next(0, range - mySize);
      }
      public BloodSpot(int size, double left, double top)
      {
         mySize = size;
         myLeft = left;
         myTop = top;
      }
   }
   public interface IMapItem
   {
      string Name { get; set; }
      string TopImageName { get; set; }
      string BottomImageName { get; set; }
      string OverlayImageName { get; set; }
      List<BloodSpot> WoundSpots { get; }
      double Zoom { get; set; }
      bool IsAnimated { get; set; }
      bool IsMoved { get; set; }              
      bool IsKilled { get; set; }
      int Count { get; set; }
      //----------------------------------------
      IMapPoint Location { get; set; }       // top left corner of MapItem
      ITerritory TerritoryCurrent { get; set; }
      ITerritory TerritoryStarting { get; set; }
      //----------------------------------------
      int Combat { get; set; }
      int Influence { get; set; }
      int Movement { get; set; }
      int MovementUsed { get; set; }
      //----------------------------------------
      bool IsConscious { get; set; }
      bool IsAlienUnknown { get; set; }
      bool IsAlienKnown { get; set; }
      bool IsControlled { get; set; }
      bool IsImplantHeld { get; set; }
      bool IsInterrogated { get; set; }
      bool IsSkeptical { get; set; }
      bool IsStunned { get; set; }
      bool IsSurrendered { get; set; }
      bool IsTiedUp { get; set; }
      bool IsWary { get; set; }
      bool IsMoveStoppedThisTurn { get; set; }
      bool IsMoveAllowedToResetThisTurn { get; set; }
      bool IsConversedThisTurn { get; set; }
      bool IsInfluencedThisTurn { get; set; }
      bool IsCombatThisTurn { get; set; }
      bool IsInterrogatedThisTurn { get; set; }
      bool IsImplantRemovalThisTurn { get; set; }
      bool IsTakeoverThisTurn { get; set; }
      //----------------------------------------
      void Copy(IMapItem mi);
      void Sync(IMapItem mi); // synchronize most of the data but not all
      void SetBloodSpots(int percent);
   }
   //==========================================
   public interface IMapItems : System.Collections.IEnumerable
   {
      static IMapItems Townspersons = new MapItems();
      int Count { get; }
      void Add(IMapItem mi);
      void Insert(int index, IMapItem mi);
      void Clear();
      bool Contains(IMapItem mi);
      int IndexOf(IMapItem mi);
      void Remove(IMapItem miName);
      void Reverse();
      IMapItem? Remove(string miName);
      IMapItem? RemoveAt(int index);
      IMapItem? Find(string miName);
      IMapItem? this[int index] { get; set; }
      IMapItems Shuffle();
      IMapItems Sort();
      IMapItems SortOnCombat();
      void Rotate(int numOfRotates);
   }
}
