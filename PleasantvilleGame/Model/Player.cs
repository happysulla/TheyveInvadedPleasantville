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
      public bool IsComputer { set; get; } = false;
      public PlayerBase(bool isComputer)
      {
         IsComputer = isComputer;
      }
      //===============================================================
      public virtual bool GetNextState(IGameInstance gi, ref GameAction action)
      {
         action = GameAction.Error;
         return false;
      }
      //--------------------------------------------------------------
      public virtual IMapItems? GetMapItemsWithinRange(IGameInstance gi, ITerritory t, int range)
      {
         string? tName = t.ToString();
         if( null == tName )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMapItemsWithinRange(): tName=null");
            return null;
         }
         List<string>? tNames = Territory.GetTerritoriesWithinRange(gi, tName, range);
         if (null == tNames)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMapItemsWithinRange(): tNames=null for anchorT=" + t.ToString());
            return null;
         }
         IMapItems mapItems = new MapItems();
         foreach (string territoryName in tNames)
         {
            IStack? stack = gi.Stacks.Find(territoryName);
            if (null == stack)
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
               mapItems.Add(mapItem);
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
      public virtual IMapItemMove? CreateMapItemMove(IMapItem mi, ITerritory newT, bool useRandomShortestPath = false)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT, useRandomShortestPath);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return null;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.ToString());
         return mim;
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
