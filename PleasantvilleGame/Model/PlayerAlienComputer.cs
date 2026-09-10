using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace PleasantvilleGame
{
   internal enum AlienStrategyEnum
   {
      DEFEND_ZEBULON,
      SURROUND_ZEBULON,
      FIENT_ZEBULON,
      KEEP_HIDDEN,
      ATTACK_TOWNSPEOPLE,
      MAX_TAKEOVER
   }
   internal class Behavior
   {
      public AlienStrategyEnum StrategyPrimary { set; get; } = AlienStrategyEnum.KEEP_HIDDEN;
      public AlienStrategyEnum StrategySecondary { set; get; } = AlienStrategyEnum.MAX_TAKEOVER;
      public int Risky { set; get; }
      public int Stealthy { set; get; }
   }

   internal class TakeoverMetric
   {
      public IMapItems myKnownAliens = new MapItems();
      public IMapItems myUnknownAliens = new MapItems();
      public IMapItems myUncontrolleds = new MapItems();
      public IMapItems myControlledInRanges = new MapItems();
   }
   //===============================================================
   public class PlayerAlienComputer : PlayerBase, IPlayerAlien
   {
      public ITerritory ZebulonLocation { set; get; } = new Territory();
      private Behavior myBehavior = new Behavior();
      //---------------------------------------------------------------
      public PlayerAlienComputer() : base(true)
      {
         myBehavior.StrategyPrimary = AlienStrategyEnum.KEEP_HIDDEN;
      }
      //===============================================================
      public override bool GetNextState(IGameInstance gi, ref GameAction action)
      {
         string key = gi.EventActive;
         switch (key)
         {
            case "e002":
               gi.EventActive = gi.EventDisplayed = "e003t";
               gi.DieRollAction = GameAction.DieRollActionNone;
               if (false == GetStartingAlienCounters(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): GetStartingAlienCounters() returned false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.GetNextState(): unhandled key=" + key);
               return false;
         }
         return true;
      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.ChooseStartingHqArea(): not implemented");
         return false;
      }
      public bool GetStartingAlienCounters(IGameInstance gi)
      {
         string startingTownplayer = gi.StartingTownspeople[0];
         if (true == String.IsNullOrEmpty(startingTownplayer))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_StartingAliens():  gi.PlayerTown.StartingTownspeople[0] is empty");
            return false;
         }
         //---------------------------------
         for (int i = 0; i < 2; i++)
         {
            string startingAlien = "ERROR";
            int count = 1000;
            while (0 < count--)
            {
               int die1 = Utilities.RandomGenerator.Next(0, 5);
               int die2 = Utilities.RandomGenerator.Next(0, 6);
               startingAlien = TableMgr.GetTownspersonName(die1, die2);
               if ("ERROR" == startingAlien)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): first TableMgr.Get_TownspersonName() returned ERROR for die1=" + die1.ToString() + " die2=" + die2.ToString());
                  return false;
               }
               if (true == startingTownplayer.Contains(startingAlien))
                  continue;
               if (gi.StartingTownspeople[1] == startingAlien)
                  continue;
               break;
            }
            if (count < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_StartingAlien(): never found aliens");
               return false;
            }
            //---------------------------------
            Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Get_StartingAlien(): Added name=" + startingAlien);
            if (true == String.IsNullOrEmpty(gi.StartingTownspeople[1]))
               gi.StartingTownspeople[1] = startingAlien;
            else
               gi.StartingTownspeople[2] = startingAlien;
         }
         return true;
      }
      public bool BlockRandomMoves(IGameInstance gi)
      {
         gi.IsAlienAckedRandomMovement = true; // computer does not need to see random moves
         // Determine if alien wants to block any movement.
         // If early in game, do not want to block and expose
         // If strategy is PROTECT_ZEBULON and exposed, then block
         // If strategy is to DEFEND_ZEUBLON, block no matter what
         // If late in game, and winning on influence, maybe block to keep winning position.
         // If blocking, remove from Random Moves.
         gi.EventDisplayed = gi.EventActive = "e006t";          // Set next state.
         return true;
      }
      public bool PerformAlienMoves(IGameInstance gi)
      {
         Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "Perform_AlienMoves(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         // Choose 5 counters to be moved.
         // Need to get the aliens to comingle with other uncontrolled townspeople
         // If strategy is FIENT_ZEBULON, move away from ZEBULON
         // If strategy is MAX_TAKEOVER, get aliens to other uncontrolled locations 
         // Create a metric for each Uncontrolled Townsperson:
         //    --- Isolated from Observations
         //    --- Away from Controlled Townspeople
         //    --- Closer to Zebulon
         //    --- Closer to Alien Center
         //    --- Greater Influence
         //    --- Greater Combat
         // Identify who Aliens move to or what uncontrolled move to Aliens
         // Should move away observing units?
         // Pick remaining uncontrolled townspeople to move.
         //    --- Move away from Town controlled units
         //    --- Add deception on what is being taken over
         // NOTES:
         //    --- Do not move an uncontrolled away from an Alien Takeover
         //    --- If an alien cannot get to an area, consider moving the uncontrolled to it
         //    --- Move units away from controlled townsperson
         IMapItemMoves alienMoves = new MapItemMoves();
         IMapItems knownAliens = new MapItems();
         IMapItems unknownAliens = new MapItems();
         IMapItems townControlledPeoples = new MapItems();
         IMapItems uncontrolledPeoples = new MapItems();
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               mi.IsMovingThisTurn = false;
               if (true == mi.IsAlienKnown)
                  knownAliens.Add(mi);
               else if (true == mi.IsAlienUnknown)
                  unknownAliens.Add(mi);
               else if (true == mi.IsControlled)
                  townControlledPeoples.Add(mi);
               else if ((false == mi.IsWary) && (false == mi.IsKnockedout) && (false == mi.IsKilled) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  uncontrolledPeoples.Add(mi);
            }
         }
         Logger.Log(LogEnum.LE_SHOW_UNCONTROLLED, "Perform_AlienMoves(): MoveUnknownAliens() uncontrolledPeoples=" + uncontrolledPeoples.ToString());
         //----------------------------------------------------------------
         PlayerAlienComputerMoveMgr moveMgr = new PlayerAlienComputerMoveMgr();
         if (false == moveMgr.MoveUnknownAliens(gi, unknownAliens, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): MoveUnknownAliens() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMoves(): mims=" + alienMoves.ToString());
         if (false == moveMgr.IntersectAlienUncontrolled(gi, unknownAliens, uncontrolledPeoples, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): IntersectAlienUncontrolled() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMoves(): mims=" + alienMoves.ToString());
         if (false == moveMgr.MoveUncontrolled(gi, uncontrolledPeoples, alienMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_AlienMoves(): MoveUncontrolled() returned error");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM, "Perform_AlienMoves(): mims=" + alienMoves.ToString());
         //----------------------------------------------------------------
         gi.MapItemMoves = alienMoves.Shuffle();
         return true;
      }
      public bool BlockTownMove(ref IMapItemMove mim)
      {
         return true;
      }
      public bool ShowPossibleTakeover(IGameInstance gi, IStack stack)
      {
         IMapItems possibleVictims = new MapItems();
         IMapItems knownAliens = new MapItems();
         IMapItems unknownAliens = new MapItems();
         IMapItems stuns = new MapItems();
         foreach (MapItem mi in stack.MapItems)
         {
            if ((true == mi.IsKilled) || (true == mi.IsKnockedout) || (true == mi.IsSurrendered) )  // Unconscious, dead, or surrendered cannot partipate
               continue;
            if (true == mi.IsControlled)
            {
               if (true == mi.IsStunned)
                  stuns.Add(mi);
            }
            else if (true == mi.IsAlienKnown)
            {
               if (false == mi.IsTiedUp)
                  knownAliens.Add(mi);
            }
            else if (true == mi.IsAlienUnknown)
            {
               if (false == mi.IsTiedUp)
                  unknownAliens.Add(mi);
            }
            else if (true == mi.IsWary)
            {
               if (true == mi.IsStunned)
                  possibleVictims.Add(mi);
            }
            else // conscious, unwary, uncontrolled townspeople
            {
               possibleVictims.Add(mi);
            }
         }
         possibleVictims = possibleVictims.Sort();
         stuns = stuns.Sort();
         unknownAliens = unknownAliens.Sort();
         //----------------------------------------
         int alienCount = knownAliens.Count + unknownAliens.Count;
         int possibleVictimCount = stuns.Count + possibleVictims.Count;
         if ( (0==possibleVictimCount) && (1 == alienCount)) // if no victims, then need to have two aliens
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 1-v=" + possibleVictims.ToString() + " ua=" + unknownAliens.ToString() + " ka=" + knownAliens.ToString() + " in t=" + stack.Territory.ToString() + " stacks=\n" + gi.Stacks.ToString());
            return false;
         }
         if ((possibleVictimCount < 2) && (0 == alienCount)) // if no aliens, then need to have two victims
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 2-v=" + possibleVictims.ToString() + " ua=" + unknownAliens.ToString() + " ka=" + knownAliens.ToString() + " in t=" + stack.Territory.ToString() + " stacks=\n" + gi.Stacks.ToString());
            return false;
         }
         //----------------------------------------
         IMapItem? leftMapItem = null;
         IMapItem? rightMapItem = null;
         if (0 == possibleVictimCount) // no possible takeover, but Townsperson does not know - so need to show - can have up to three known and three unknown
         {
            if( ( 0 < knownAliens.Count ) && (unknownAliens.Count <= knownAliens.Count )) 
            {
               int unknownCount = 0;
               foreach(IMapItem knownAlien in knownAliens )
               {
                  if (unknownCount == unknownAliens.Count)
                     break;
                  leftMapItem = knownAlien;
                  rightMapItem = unknownAliens[unknownCount];
                  if( null == rightMapItem )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for unknownCount=" + unknownCount.ToString());
                     return false;
                  }
                  unknownCount++;
                  gi.AlienTakeovers[leftMapItem] = rightMapItem;
                  Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 1-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
               }
            }
            else if ((0 < knownAliens.Count) && (knownAliens.Count < unknownAliens.Count))
            {
               int knownCount = 0;
               foreach (IMapItem unknownAlien in unknownAliens)
               {
                  if (knownCount == knownAliens.Count)
                     break;
                  leftMapItem = unknownAlien;
                  rightMapItem = knownAliens[knownCount];
                  if (null == rightMapItem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for knownCount=" + knownCount.ToString());
                     return false;
                  }
                  knownCount++;
                  gi.AlienTakeovers[leftMapItem] = rightMapItem;
                  Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 2-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
               }
            }
            else // can only have up to three unknown aliens in space - choose two random ones
            {
               int r1 = Utilities.RandomGenerator.Next(unknownAliens.Count);
               leftMapItem = unknownAliens[r1];
               if (null == leftMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 1-leftMapItem=null for r1=" + r1.ToString());
                  return false;
               }
               int r2 = r1;
               while (r2 == r1)
                  r2 = Utilities.RandomGenerator.Next(unknownAliens.Count);
               rightMapItem = unknownAliens[r2];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 1-rightMapItem=null for r2=" + r2.ToString());
                  return false;
               }
               gi.AlienTakeovers[leftMapItem] = rightMapItem;
               Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 3-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
            }
         }
         else // possible victims
         {
            if (0 < knownAliens.Count) // show known aliens
            {
               int victimCount = 0;
               foreach (IMapItem knownAlien in knownAliens)
               {
                  leftMapItem = knownAlien;
                  if (victimCount == possibleVictims.Count)
                     break;
                  if (victimCount < stuns.Count)
                  {
                     rightMapItem = stuns[victimCount];
                     if (null == rightMapItem)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for stuns=" + victimCount.ToString());
                        return false;
                     }
                     victimCount++;
                     gi.AlienTakeovers[leftMapItem] = rightMapItem;
                     Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 4-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
                  }
                  else 
                  {
                     int indexOffset = victimCount - stuns.Count;
                     rightMapItem = possibleVictims[indexOffset];
                     if (null == rightMapItem)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for indexOffset=" + indexOffset.ToString());
                        return false;
                     }
                     victimCount++;
                     gi.AlienTakeovers[leftMapItem] = rightMapItem;
                     Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 5-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
                  }
               }
            }
            else if (0 < unknownAliens.Count) // shown unknown aliens
            {
               int victimCount = 0;
               foreach (IMapItem unknownAlien in unknownAliens)
               {
                  leftMapItem = unknownAlien;
                  if (victimCount == possibleVictims.Count)
                     break;
                  if (victimCount < stuns.Count)
                  {
                     rightMapItem = stuns[victimCount];
                     if (null == rightMapItem)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for stuns=" + victimCount.ToString());
                        return false;
                     }
                     victimCount++;
                     gi.AlienTakeovers[leftMapItem] = rightMapItem;
                     Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover():Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
                     gi.AddKnownAlien(leftMapItem); // unknown becomes known when taking over a Stunned TP
                     Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Show_PossibleTakeover(): 6-AddKnownAlien() unknown becomes known when taking over stunned -- a=" + leftMapItem.ToString() );
                  }
                  else
                  {
                     int indexOffset = victimCount - stuns.Count;
                     rightMapItem = possibleVictims[indexOffset];
                     if (null == rightMapItem)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): rightMapItem=null for indexOffset=" + indexOffset.ToString());
                        return false;
                     }
                     victimCount++;
                     gi.AlienTakeovers[leftMapItem] = rightMapItem;
                     Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 7-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
                  }
               }
            }
            else // no aliens and no stuns counters in the space
            {
               int r1 = Utilities.RandomGenerator.Next(possibleVictims.Count);
               leftMapItem = possibleVictims[r1];
               if (null == leftMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 2-leftMapItem=null for r1=" + r1.ToString());
                  return false;
               }
               int r2 = r1;
               while (r2 == r1)
                  r2 = Utilities.RandomGenerator.Next(possibleVictims.Count);
               rightMapItem = possibleVictims[r2];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Show_PossibleTakeover(): 2-rightMapItem=null for r2=" + r2.ToString());
                  return false;
               }
               gi.AlienTakeovers[leftMapItem] = rightMapItem;
               Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Show_PossibleTakeover(): 8-Adding leftMapItem=" + leftMapItem.Name + " rightMapItem=" + rightMapItem.Name + " in t=" + stack.Territory.ToString());
            }
         }
         return true;
      }
      private List<TakeoverMetric> GetTakeoverMetrics(IGameInstance gi)
      {
         List<TakeoverMetric> metrics = new List<TakeoverMetric>();
         foreach (IStack stack in gi.Stacks)
         {
            if (0 == stack.MapItems.Count)
               continue;
            TakeoverMetric metric = new TakeoverMetric();
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsAlienKnown)
                  metric.myKnownAliens.Add(mi);
               else if (true == mi.IsAlienUnknown)
                  metric.myKnownAliens.Add(mi);
               else if ((false == mi.IsAlienKnown) && (false == mi.IsAlienUnknown) && (false == mi.IsControlled))
                  metric.myUncontrolleds.Add(mi);
            }
            metrics.Add(metric); // possible takeover
         }
         //--------------------------------------------
         foreach (TakeoverMetric metric in metrics)
         {

         }
         return metrics;
      }
   }
}
