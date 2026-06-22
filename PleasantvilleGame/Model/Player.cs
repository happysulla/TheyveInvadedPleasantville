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
