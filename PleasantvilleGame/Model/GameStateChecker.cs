using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PleasantvilleGame
{
   public class GameStateChecker
   {
      public GameStateChecker() { }
      static public bool CheckForConversations(IGameInstance gi, out bool isConversation)
      {
         isConversation = false;
         foreach (Stack stack in gi.Stacks)
         {
            IMapItems townspeopleControlled = new MapItems();
            IMapItems townspeopleUncontrolled = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
                  continue;

               if (true == mi.IsControlled)
               {
                  townspeopleControlled.Add(mi);
               }
               else
               {
                  if ((false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
                     townspeopleUncontrolled.Add(mi);
               }
            }
            if ((0 != townspeopleControlled.Count) && (0 != townspeopleUncontrolled.Count))
            {
               isConversation = true;
               return true;
            }
         }
         return true;
      }
      static public bool CheckForInfluence(IGameInstance gi, out bool isInfluence)
      {
         isInfluence = false;
         foreach (Stack stack in gi.Stacks)
         {
            IMapItems townspeopleControlled = new MapItems();
            IMapItems townspeopleUncontrolled = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsInfluencedThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
               {
                  townspeopleControlled.Add(mi);
               }
               else
               {
                  if ((false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
                     townspeopleUncontrolled.Add(mi);
               }
            }
            if ((0 != townspeopleControlled.Count) && (0 != townspeopleUncontrolled.Count))
            {
               isInfluence = true;
               return true;
            }
         }
         return true;
      }
      static public bool CheckForTownspersonCombats(IGameInstance gi, out bool isTownspersonCombat)
      {
         isTownspersonCombat = false;
         IMapItemCombat? previousCombat = gi.MapItemCombat; // If the previous combat had retreats, do not assume combats are completed
         if (null == previousCombat)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckForTownspersonCombats(): previousCombat=null");
            return false;
         }
         if (true == previousCombat.IsAnyRetreat)          // until the player explicitly indicates it with menu command.  This allows them to see the retreats. 
         {
            Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, "CheckForTownspersonCombats(): previousCombat.IsAnyRetreat=true");
            isTownspersonCombat = true;
            return true;
         }
         //-----------------------------------------------------------
         // The townspeople can fight aliens or uncontrolled townspeople.
         // They might want to fight uncontrolled townspeople if they suspect 
         // they are aliens.  However, if they are not aliens, the uncontrolled
         // townspeople immediately lose the battle and could be killed.
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            IMapItems controlled = new MapItems();
            IMapItems uncontrolled = new MapItems();
            IMapItems aliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsCombatThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
                  controlled.Add(mi);
               else if (true == mi.IsAlienKnown)
                  aliens.Add(mi);
               else
                  uncontrolled.Add(mi);
            }
            //-----------------------------------------------------------
            if ((0 != controlled.Count) && ((0 != aliens.Count) || (0 != uncontrolled.Count)))
            {
               StringBuilder sb = new StringBuilder("CheckForTownspersonCombats(): t="); sb.Append(stack.Territory.ToString());
               sb.Append(" controlled.Count="); sb.Append(controlled.Count.ToString());
               sb.Append(" aliens.Count="); sb.Append(aliens.Count.ToString());
               sb.Append(" uncontrolled.Count="); sb.Append(uncontrolled.Count.ToString());
               Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, sb.ToString());
               isTownspersonCombat = true;
               return true;
            }
         }
         return true;
      }
      static public bool CheckForAlienCombats(IGameInstance gi, out bool isAlienCombat)
      {
         isAlienCombat = false;
         if (null == gi.MapItemCombat)
         {
            Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, "CheckForTownspersonCombats(): gi.MapItemCombat=true");
            return false;                                  // to see the retreats. 
         }
         IMapItemCombat? previousCombat = gi.MapItemCombat; // If the previous combat had retreats, do not assume combats are completed
         if (true == previousCombat.IsAnyRetreat)           // until the player explicitly indicates it with menu command.  This allows them to be seen.
         {
            Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, "CheckForTownspersonCombats(): previousCombat.IsAnyRetreat=true");
            isAlienCombat = true;
            return true;                                  // to see the retreats. 
         }                             // to see the retreats. 
         //--------------------------------------------------
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            IMapItems controlled = new MapItems();
            IMapItems uncontrolled = new MapItems();
            IMapItems aliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsCombatThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
                  controlled.Add(mi);
               else if (true == mi.IsAlienKnown)
                  aliens.Add(mi);
               else
                  uncontrolled.Add(mi);
            }

            if ((0 != controlled.Count) && ((0 != aliens.Count) || (0 != uncontrolled.Count)))
            {
               StringBuilder sb = new StringBuilder("CheckFor_AlienCombats(): t="); sb.Append(stack.Territory.ToString());
               sb.Append(" controlled.Count="); sb.Append(controlled.Count.ToString());
               sb.Append(" aliens.Count="); sb.Append(aliens.Count.ToString());
               sb.Append(" uncontrolled.Count="); sb.Append(uncontrolled.Count.ToString());
               Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, sb.ToString());
               return true;
            }

            if (0 != aliens.Count)
            {
               bool isAnyMapItemsWary = false;
               foreach (IMapItem mi2 in uncontrolled)
               {
                  if (true == mi2.IsWary)
                     isAnyMapItemsWary = true;
               }
               if (true == isAnyMapItemsWary)
               {
                  StringBuilder sb = new StringBuilder("CheckFor_AlienCombats(): t="); sb.Append(stack.Territory.ToString());
                  sb.Append(" controlled.Count="); sb.Append(controlled.Count.ToString());
                  sb.Append(" aliens.Count="); sb.Append(aliens.Count.ToString());
                  sb.Append(" uncontrolled.Count="); sb.Append(uncontrolled.Count.ToString());
                  sb.Append(" isAnyMapItemsWary=true");
                  Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, sb.ToString());
                  isAlienCombat = true;
                  return true;
               }
            }
         }
         return true;
      }
      static public bool CheckForIterogations(IGameInstance gi)
      {
         gi.NumIterogationsThisTurn = 0;
         IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
         if (null == zebulon)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckForIterogations(): ERROR: GameState::CheckForIterogations() - unable to find Zebulon");
            return false;
         }
         if (true == zebulon.IsAlienKnown)  // If Zebulon is already on the map board, no need to iterogate
            return false;
         IMapItems controlled = new MapItems();
         IMapItems surrenderedAliens = new MapItems();
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;

            controlled.Clear();
            surrenderedAliens.Clear();

            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsInterrogatedThisTurn) || (true == mi.IsInterrogated) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned))
                  continue;

               if (true == mi.IsControlled)
               {
                  if (false == mi.IsTiedUp)    // Must not be stunned to interogate
                     controlled.Add(mi);
               }
               else
               {
                  if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsSurrendered) || (true == mi.IsTiedUp)))
                  {
                     surrenderedAliens.Add(mi);
                     mi.IsInterrogated = true;
                     mi.IsInterrogatedThisTurn = true;
                  }
               }
            }

            if (((0 != controlled.Count) && (0 != surrenderedAliens.Count)))
               gi.NumIterogationsThisTurn += surrenderedAliens.Count * 4;
         }

         if (0 < gi.NumIterogationsThisTurn)
            return true;
         return false;
      }
      static public bool CheckForImplantRemoval(IGameInstance gi)
      {
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;

            IMapItems controlled = new MapItems();
            IMapItems aliens = new MapItems();

            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsImplantRemovalThisTurn) || (true == mi.IsKilled))
                  continue;

               if ((true == mi.IsControlled) && (true == mi.IsConscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  controlled.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsConscious)))
                  aliens.Add(mi);
            }

            if (((0 != controlled.Count) && (0 != aliens.Count)))
               return true;
         }

         return false;
      }
      static public bool CheckForAlienTakeovers(IGameInstance gi)
      {
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            IMapItems possibleVictums = new MapItems();
            IMapItems knownAliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               // Unconscious or dead cannot be taken over

               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsSurrendered) || ("Zebulon" == mi.Name))
                  continue;

               if ((true == mi.IsControlled) || (true == mi.IsWary))
               {
                  if ((true == mi.IsStunned) || (true == mi.IsTiedUp))
                     possibleVictums.Add(mi);
               }
               else
               {
                  if (true == mi.IsAlienKnown)
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp))
                        knownAliens.Add(mi);
                  }
                  else
                  {
                     possibleVictums.Add(mi);
                  }
               }

            }

            if (1 < possibleVictums.Count) // If any stack has two or more counters that are not controlled, return true       
               return true;

            if ((1 == possibleVictums.Count) && (0 < knownAliens.Count)) // If any stack has at least one possible victum with a known alien, return true   
               return true;
         }

         return false;
      }
      static public bool CheckForEndOfGame(IGameInstance gi)
      {
         StringBuilder sb;
         gi.NumIterogationsThisTurn = 0;
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               mi.IsMoveStoppedThisTurn = false;
               mi.IsMoveAllowedToResetThisTurn = true;
               mi.IsConversedThisTurn = false;
               mi.IsInfluencedThisTurn = false;
               mi.IsCombatThisTurn = false;
               mi.IsInterrogatedThisTurn = false;
               mi.IsImplantRemovalThisTurn = false;
               mi.IsTakeoverThisTurn = false;
               mi.MovementUsed = 0;
               mi.IsMoved = false;
               mi.TerritoryStarting = mi.TerritoryCurrent;
            }
         }
         //--------------------------------------------------------
         foreach (Stack stack in gi.Stacks) //  Tied Up MapItems - Tied up players are freed if a friendly counter is in the same hex at the end of the turn.
         {
            IMapItems alienTiedUpPersons = new MapItems();
            IMapItems controlledTiedUpPersons = new MapItems();
            bool isFriendlyAlienHelping = false;
            bool isFriendlyControlledHelping = false;
            //-------------------------------------------------------------------------------
            StringBuilder sb1 = new StringBuilder("CheckForEndOfGame(): Tied Up Units in t=\n"); sb1.Append(stack.Territory.ToString());
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsSurrendered) || (true == mi.IsKilled))
                  continue;
               if (true == mi.IsTiedUp) // Cound be stunned or unconscious
               {
                  if (true == mi.IsAlienKnown)
                     alienTiedUpPersons.Add(mi);
                  else if (true == mi.IsControlled)
                     controlledTiedUpPersons.Add(mi);
                  sb1.Append(" ");
                  sb1.Append(mi.Name);
               }
               if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned))
               {
                  if (true == mi.IsAlienKnown)
                  {
                     isFriendlyAlienHelping = true;
                     sb1.Append(" FRIENDLY ALIEN=");
                  }
                  else if (true == mi.IsControlled)
                  {
                     isFriendlyControlledHelping = true;
                     sb1.Append(" FRIENDLY TP=");
                  }
                  sb1.Append(mi.Name);
               }
            }
            //-------------------------------------------------------------------------------
            if (true == isFriendlyAlienHelping)
            {
               foreach (IMapItem alien in alienTiedUpPersons) // known aliens tied up
               {
                  alien.IsTiedUp = false;
                  sb1.Append(" untied alien=");
                  sb1.Append(alien.Name);
                  if ((true == alien.IsConscious) && (false == alien.IsStunned))
                  {
                     gi.InfluenceCountTotal += alien.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(alien.Name); sb.Append(" ++++ to Total "); sb.Append(alien.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

                     gi.InfluenceCountAlienKnown += alien.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(alien.Name); sb.Append(" ++++ to known "); sb.Append(alien.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
               }
            }
            //-------------------------------------------------------------------------------
            if (true == isFriendlyControlledHelping)
            {
               foreach (IMapItem controlled in controlledTiedUpPersons)
               {
                  controlled.IsTiedUp = false;
                  sb1.Append(" untied TP=");
                  sb1.Append(controlled.Name);
                  if ((true == controlled.IsConscious) && (false == controlled.IsStunned))
                  {
                     gi.InfluenceCountTotal += controlled.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie "); sb.Append(controlled.Name); sb.Append(" ++++ to Total "); sb.Append(controlled.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                     gi.InfluenceCountTownspeople += controlled.Influence;
                     sb = new StringBuilder("CheckForEndOfGame(): untie"); sb.Append(controlled.Name); sb.Append(" ++++ to TP "); sb.Append(controlled.Influence.ToString());
                     sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                     sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                     sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                     sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                     Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                  }
               }
            }
            Logger.Log(LogEnum.LE_GAMESTATE_CHECKER_TIED_UP, sb1.ToString());
         } // end foreach (Stack stack in stacks)
         //-----------------------------------------------------------
         foreach (IMapItem mi1 in gi.PersonsStunned)          // Unstunned - For each person who was stunned returns to the game
         {
            mi1.IsStunned = false;
            if (false == mi1.IsTiedUp)
            {
               gi.InfluenceCountTotal += mi1.Influence;
               sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi1.Name); sb.Append(" ++++ to TOTAL "); sb.Append(mi1.Influence.ToString());
               sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

               if (true == mi1.IsAlienUnknown)
               {
                  gi.InfluenceCountAlienUnknown += mi1.Influence;
                  sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi1.Name); sb.Append(" ++++ to unknown "); sb.Append(mi1.Influence.ToString());
                  sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               else if (true == mi1.IsAlienKnown)
               {
                  gi.InfluenceCountAlienKnown += mi1.Influence;
                  sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi1.Name); sb.Append(" ++++ to known "); sb.Append(mi1.Influence.ToString());
                  sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               else if (true == mi1.IsControlled)
               {
                  gi.InfluenceCountTownspeople += mi1.Influence;
                  sb = new StringBuilder("CheckForEndOfGame() : unstunned "); sb.Append(mi1.Name); sb.Append(" ++++ to TP "); sb.Append(mi1.Influence.ToString());
                  sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

            }
         }
         gi.PersonsStunned.Clear();
         //-----------------------------------------------------------
         foreach (IMapItem mi in gi.PersonsKnockedOut) // Knocked Out Map Items - For each person who was not recently knocked out, it converts to a stunned counter.
         {
            mi.IsConscious = true;
            mi.IsStunned = true;
         }
         gi.PersonsKnockedOut.Clear();
         //-----------------------------------------------------------
         if (false == GameStateChecker.IsInfluenceCheck(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame(): returned error");
            return false;
         }
         IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon"); // If Zebulon is dead, game is over.
         if( null == zebulon)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame(): ERROR: GameState::CheckForEndOfGame() - unable to find Zebulon");
            return false;
         }
         if (true == zebulon.IsKilled)
            return true;
         if (((gi.InfluenceCountAlienUnknown <= 0) && (gi.InfluenceCountAlienKnown <= 0)) || (gi.InfluenceCountTownspeople <= 0))  // If either the Alien or Townscontrolled influcence reaches zero, game over
            return true;
         gi.GameTurn++;
         if (12 < gi.GameTurn) // Determine turn number.  If reach 12, game is over.
            return true;
         return false;
      }
      static public bool CheckForRandomMoves(IGameInstance gi)
      {
         gi.IsAlienDisplayedRandomMovement = false;
         gi.IsControlledDisplayedRandomMovement = false;
         gi.IsAlienAckedRandomMovement = false;
         gi.IsControlledAckedRandomMovement = false;
         gi.Takeover = null;
         gi.MapItemCombat.IsAnyRetreat = false;
         foreach (IMapItemMove mim in gi.MapItemMoves)
         {
            IMapItem mi = mim.MapItem;
            mi.TerritoryCurrent = mim.NewTerritory;
            mi.TerritoryStarting = mim.NewTerritory;
            mi.IsMoved = false;
            mi.MovementUsed = 0;
         }
         gi.MapItemMoves.Clear();

         // Check if anybody can move

         foreach (IMapItem mi in gi.Persons)
         {
            if ((true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsSurrendered)
                || (true == mi.IsStunned) || (true == mi.IsTiedUp)
                || (true == mi.IsControlled) || (true == mi.IsAlienKnown))
               continue;

            return true; // If any mapitem can move randomly
         }

         return false;
      }
      public static bool IsInfluenceCheck(IGameInstance gi)
      {
         int totalInfluence = 0;

         int cogentInfluence = 0;
         int knownInfluence = 0;
         int unknownInfluence = 0;
         int controlledInfluence = 0;
         int uncontrolledInfluence = 0;

         int incapacitatedInfluence = 0;
         int tiedUpInfluence = 0;
         int stunnedInfluence = 0;
         int unconsciousInfluence = 0;
         int surrenderedInfluence = 0;
         int killedInfluence = 0;
         int errorInfluence = 0;

         foreach (IMapItem mi in gi.Persons)
         {
            totalInfluence += mi.Influence;

            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               cogentInfluence += mi.Influence;
               if (true == mi.IsControlled)
                  controlledInfluence += mi.Influence;
               if (true == mi.IsAlienKnown)
                  knownInfluence += mi.Influence;
               if (true == mi.IsAlienUnknown)
                  unknownInfluence += mi.Influence;
               if ((false == mi.IsControlled) && (false == mi.IsAlienKnown) && (false == mi.IsAlienUnknown))
                  uncontrolledInfluence += mi.Influence;
            }
            else
            {
               incapacitatedInfluence += mi.Influence;
               if (true == mi.IsKilled)
                  killedInfluence += mi.Influence;
               else if (true == mi.IsSurrendered)
                  surrenderedInfluence += mi.Influence;
               else if (false == mi.IsConscious)
                  unconsciousInfluence += mi.Influence;
               else if (true == mi.IsStunned)
                  stunnedInfluence += mi.Influence;
               else if (true == mi.IsTiedUp)
                  tiedUpInfluence += mi.Influence;
               else
                  errorInfluence += mi.Influence;
            }
         }

         if ((337 != totalInfluence) ||
             (totalInfluence != (cogentInfluence + incapacitatedInfluence)) ||
             (cogentInfluence != (controlledInfluence + knownInfluence + unknownInfluence + uncontrolledInfluence)) ||
             (0 != errorInfluence))
         {
            StringBuilder sb = new StringBuilder("Is_InfluenceCheck(): Influence Not Adding Up: ");
            sb.Append("\n T="); sb.Append(totalInfluence.ToString());
            sb.Append("\n cap="); sb.Append(cogentInfluence.ToString());
            sb.Append("\n kn="); sb.Append(knownInfluence.ToString());
            sb.Append("\n unk="); sb.Append(unknownInfluence.ToString());
            sb.Append("\n tp="); sb.Append(controlledInfluence.ToString());
            sb.Append("\n uc="); sb.Append(uncontrolledInfluence.ToString());

            sb.Append("\n incap="); sb.Append(incapacitatedInfluence.ToString());
            sb.Append("\n tu="); sb.Append(tiedUpInfluence.ToString());
            sb.Append("\n st="); sb.Append(stunnedInfluence.ToString());
            sb.Append("\n unc="); sb.Append(unconsciousInfluence.ToString());
            sb.Append("\n sur="); sb.Append(surrenderedInfluence.ToString());
            sb.Append("\n kia="); sb.Append(killedInfluence.ToString());
            sb.Append("\n err="); sb.Append(errorInfluence.ToString());

            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return false;
         }
         return true;
      }

   }
}
