using Microsoft.AspNetCore.Components.Web.Virtualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public abstract class PlayerBase : IPlayer
   {
      public String[] StartingTownspeople { get; set; } = new String[2];
      public List<string> BlockedRandomMoves { set; get; } = new List<string>(); // a townsperson name who is blocked by owner from moving in random movement
      public bool IsComputer { set; get; } = false;
      public PlayerBase(bool isComputer)
      {
         IsComputer = isComputer;
      }
      //===============================================================
      public virtual bool GetNextState(IGameInstance gi)
      {
         return true;
      }
      public virtual bool CreateRandomMoves(IGameInstance gi)
      {
         return true;
      }
      public virtual bool PerformRandomMoves(IGameInstance gi)
      {
         foreach (KeyValuePair<string, string> kvp in gi.RandomMoves)
         {
            string name = kvp.Key;
            string buildingName = kvp.Value;
            IMapItem? mi = gi.Townspeople.Find(name);
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): unable to find name=" + name);
               return false;
            }
            ITerritory? newTerritory = Territories.theTerritories.Find(buildingName);
            if (null == newTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): unable to find buildingName=" + buildingName);
               return false;
            }
            //-----------------------------------------
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Perform_RandomMoves(): mi=" + mi.Name + " entering t=" + newTerritory.Name);
            if (false == this.CreateMapItemMove(gi, mi, newTerritory, true))
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMoves(): Create_MapItemMove() returned false");
               return false;
            }
         }
         return true;
      }
      public virtual bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT, useRandomShortestPath);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.ToString());
         gi.MapItemMoves.Insert(0, mim); // add at front
         return true;
      }
      //--------------------------------------------------------------
      public virtual IMapItems? GetUncontrolledWithinRange(IGameInstance gi, string anchorName, int range, bool isUnknownAlso)
      {
         IMapItems mapItems = new MapItems();
         List<String>? tNames = Territory.GetTerritoriesWithinRange(gi, anchorName, range);
         if (null == tNames)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMapItemsWithinRange(): strings=null for anchorT=" + anchorName);
            return null;
         }
         foreach (string tName in tNames)
         {
            IStack? stack = gi.Stacks.Find(tName);
            if (null == stack) // If there is no stack in this territory, skip it
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
            {
               if ((false == mapItem.IsControlled) && (false == mapItem.IsKilled && false == mapItem.IsStunned))
               {
                  if (true == isUnknownAlso)
                  {
                     if (true == mapItem.IsAlienKnown || true == mapItem.IsAlienUnknown)
                        mapItems.Add(mapItem);
                  }
                  else
                  {
                     if (true == mapItem.IsAlienKnown)
                        mapItems.Add(mapItem);
                  }
               }
            }
         }
         return mapItems;
      }
      public virtual IMapItems? GetTownsWithinRange(IGameInstance gi, string anchorName, int range)
      {
         IMapItems mapItems = new MapItems();
         List<String>? tNames = Territory.GetTerritoriesWithinRange(gi, anchorName, range);
         if (null == tNames)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMapItemsWithinRange(): strings=null for anchorT=" + anchorName);
            return null;
         }
         foreach (string tName in tNames)
         {
            IStack? stack = gi.Stacks.Find(tName);
            if (null == stack) // If there is no stack in this territory, skip it
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
            {
               if ((true == mapItem.IsControlled) && (false == mapItem.IsKilled && false == mapItem.IsStunned))
                  mapItems.Add(mapItem);
            }
         }
         return mapItems;
      }
      public virtual IMapItems? GetAliensWithinRange(IGameInstance gi, string anchorName, int range, bool isUnknownAlso)
      {
         IMapItems mapItems = new MapItems();
         List<String>? tNames = Territory.GetTerritoriesWithinRange(gi, anchorName, range);
         if (null == tNames)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMapItemsWithinRange(): strings=null for anchorT=" + anchorName);
            return null;
         }
         foreach (string tName in tNames)
         {
            IStack? stack = gi.Stacks.Find(tName);
            if (null == stack) // If there is no stack in this territory, skip it
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
            {
               if ((false == mapItem.IsControlled) && (false == mapItem.IsKilled && false == mapItem.IsStunned))
               {
                  if (true == isUnknownAlso)
                  {
                     if (true == mapItem.IsAlienKnown || true == mapItem.IsAlienUnknown)
                        mapItems.Add(mapItem);
                  }
                  else
                  {
                     if (true == mapItem.IsAlienKnown)
                        mapItems.Add(mapItem);
                  }
               }
            }
         }
         return mapItems;
      }
      public virtual int GetUncontrolledCount(IGameInstance gi, IMapItems mapItems, bool isUnknownAlso)
      {
         int count = 0;
         foreach (IMapItem mi in mapItems)
         {
            if (false == mi.IsControlled && false == mi.IsKilled && false == mi.IsAlienKnown && (true == isUnknownAlso && false == mi.IsAlienUnknown))
               count++;
         }
         return count;
      }
      public virtual int GetTownCount(IGameInstance gi, IMapItems mapItems)
      {
         int count = 0;
         foreach (IMapItem mi in mapItems)
         {
            if (true == mi.IsControlled && false == mi.IsKilled)
               count++;
         }
         return count;
      }
      public virtual int GetAlienCount(IGameInstance gi, IMapItems mapItems, bool isUnknownAlso)
      {
         int count = 0;
         foreach (IMapItem mi in mapItems)
         {
            if (true == isUnknownAlso)
            {
               if (true == mi.IsAlienKnown || true == mi.IsAlienUnknown)
                  count++;
            }
            else
            {
               if (true == mi.IsAlienKnown)
                  count++;
            }
         }
         return count;
      }
      //--------------------------------------------------------------
      public virtual int GetKnownAlienCount(IGameInstance gi)
      {
         int count = 0;
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsAlienKnown && false == mi.IsKilled)
                  count++;
            }
         }
         return count;
      }
      public virtual int GetCombatTotal(IMapItems mapItems)
      {
         int combatTotal = 0;
         foreach (IMapItem mapItem in mapItems)
            combatTotal += mapItem.Combat;
         return combatTotal;
      }
      public virtual int GetInfluenceTotal(IMapItems mapItems)
      {
         int influenceTotal = 0;
         foreach (IMapItem mapItem in mapItems)
            influenceTotal += mapItem.Influence;
         return influenceTotal;
      }
      public virtual int GetInfluencePercent(IGameInstance gi)
      {
         int influence = 0;
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsAlienKnown || true == mi.IsAlienUnknown) && (false == mi.IsKilled))
                  influence += mi.Influence;
            }
         }
         return influence;
      }
      public virtual int GetInfluencePercentKnown(IGameInstance gi)
      {
         int influence = 0;
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsAlienKnown) && (false == mi.IsKilled))
                  influence += mi.Influence;
            }
         }
         return influence;
      }

   }
}
