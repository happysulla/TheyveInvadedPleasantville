using System.Text
using System.Windows;

namespace PleasantvilleGame
{
    public class GameStateChecker
    {
        #region constructor 
        public GameStateChecker()
        {
        }
        #endregion

        #region bool CheckForConversations(IGameInstance gi)
        static public bool CheckForConversations(IGameInstance gi)
        {
            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
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
                    return true;
            }

            return false;
        }
        #endregion

        #region bool CheckForInfluence(IGameInstance gi)
        static public bool CheckForInfluence(IGameInstance gi)
        {
            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
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
                    return true;
            }

            return false;
        }
        #endregion

        #region bool CheckForTownspersonCombats(IGameInstance gi)
        static public bool CheckForTownspersonCombats(IGameInstance gi)
        {
            IMapItemCombat previousCombat = gi.MapItemCombat; // If the previous combat had retreats, do not assume combats are completed
            if (true == previousCombat.IsAnyRetreat)          // until the player explicitly indicates it with menu command.  This allows them
            {
                Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, "CheckForTownspersonCombats(): previousCombat.IsAnyRetreat=true");
                return true;                                  // to see the retreats. 
            }

            // The townspeople can fight aliens or uncontrolled townspeople.
            // They might want to fight uncontrolled townspeople if they suspect 
            // they are aliens.  However, if they are not aliens, the uncontrolled
            // townspeople immediately lose the battle and could be killed.

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);

            foreach (Stack stack in stacks)
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
                    StringBuilder sb = new StringBuilder("CheckForTownspersonCombats(): t="); sb.Append(stack.Territory.ToString());
                    sb.Append(" controlled.Count="); sb.Append(controlled.Count.ToString());
                    sb.Append(" aliens.Count="); sb.Append(aliens.Count.ToString());
                    sb.Append(" uncontrolled.Count="); sb.Append(uncontrolled.Count.ToString());
                    Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, sb.ToString());
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region bool CheckForAlienCombats(IGameInstance gi)
        static public bool CheckForAlienCombats(IGameInstance gi)
        {
            IMapItemCombat previousCombat = gi.MapItemCombat; // If the previous combat had retreats, do not assume combats are completed
            if (true == previousCombat.IsAnyRetreat)          // until the player explicitly indicates it with menu command.  This allows them to be seen.
            {
                Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, "CheckForTownspersonCombats(): previousCombat.IsAnyRetreat=true");
                return true;                                  // to see the retreats. 
            }                             // to see the retreats. 

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);

            foreach (Stack stack in stacks)
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
                    StringBuilder sb = new StringBuilder("CheckForAlienCombats(): t="); sb.Append(stack.Territory.ToString());
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
                        StringBuilder sb = new StringBuilder("CheckForAlienCombats(): t="); sb.Append(stack.Territory.ToString());
                        sb.Append(" controlled.Count="); sb.Append(controlled.Count.ToString());
                        sb.Append(" aliens.Count="); sb.Append(aliens.Count.ToString());
                        sb.Append(" uncontrolled.Count="); sb.Append(uncontrolled.Count.ToString());
                        sb.Append(" isAnyMapItemsWary=true");
                        Logger.Log(LogEnum.LE_GAMESTATE_CHECKER, sb.ToString());
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region bool CheckForIterogations(IGameInstance gi)
        static public bool CheckForIterogations(IGameInstance gi)
        {
            gi.NumIterogationsThisTurn = 0;

            IMapItem zebulon = gi.Persons.Find("Zebulon");
            if (true == zebulon.IsAlienKnown)  // If Zebulon is already on the map board, no need to iterogate
                return false;

            IMapItems controlled = new MapItems();
            IMapItems surrenderedAliens = new MapItems();

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);

            foreach (Stack stack in stacks)
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
                   gi.NumIterogationsThisTurn += surrenderedAliens.Count*4;
            }

            if (0 < gi.NumIterogationsThisTurn)
                return true;
            return false;
        }
        #endregion

        #region bool CheckForImplantRemoval(IGameInstance gi)
        static public bool CheckForImplantRemoval(IGameInstance gi)
        {
            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
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
        #endregion

        #region bool CheckForAlienTakeovers(IGameInstance gi)
        static public bool CheckForAlienTakeovers(IGameInstance gi)
        {
            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
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

                    if ((true == mi.IsControlled) || (true == mi.IsWary) )
                    {
                        if ((true == mi.IsStunned) || (true == mi.IsTiedUp) )
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
        #endregion

        #region bool CheckForEndOfGame(IGameInstance gi)
        static public bool CheckForEndOfGame(IGameInstance gi)
        {
            StringBuilder sb = null;

            gi.NumIterogationsThisTurn = 0;

            // Cleanup the temporary state changes

            foreach (IMapItem mi in gi.Persons)
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
                mi.TerritoryStarting = mi.Territory;
            }

            //--------------------------------------------------------
            // Tied Up MapItems
            // Tied up players are freed if a friendly counter is in the same hex at the end of the turn.

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
            {
                IMapItems alienTiedUpPersons = new MapItems();
                IMapItems controlledTiedUpPersons = new MapItems();

                bool isFriendlyAlienHelping = false;
                bool isFriendlyControlledHelping = false;

                StringBuilder sb1 = new StringBuilder("CheckForEndOfGame(): Tied Up Units in t=\n"); sb1.Append(stack.Territory.ToString()); 
                foreach (MapItem person in stack.MapItems)
                {
                    IMapItem mi = gi.Persons.Find(person.Name);
                    if (null == mi)
                    {
                        Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame()1: ERROR: GameState::CheckForEndOfGame() - unalbe to find " + mi.Name);
                        return true;
                    }

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

                if (true == isFriendlyAlienHelping)
                {
                    foreach (IMapItem mi1 in alienTiedUpPersons) // known aliens tied up
                    {
                        IMapItem alien = gi.Persons.Find(mi1.Name);
                        if (null == mi1)
                        {
                            Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame()1: ERROR: GameState::CheckForEndOfGame() - unalbe to find " + mi1.Name);
                            return true;
                        }

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

                if (true == isFriendlyControlledHelping) 
                {
                    foreach (IMapItem mi2 in controlledTiedUpPersons)
                    {
                        IMapItem controlled = gi.Persons.Find(mi2.Name);
                        if (null == mi2)
                        {
                            Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame()1: ERROR: GameState::CheckForEndOfGame() - unalbe to find " + mi2.Name);
                            return true;
                        }

                        controlled.IsTiedUp = false;
                        sb1.Append(" untied TP=");
                        sb1.Append(controlled.Name);
                        if ((true == controlled.IsConscious) && (false == controlled.IsStunned) )
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
            // Unstunned 
            // For each person who was stunned returns to the game

            foreach (IMapItem mi in gi.PersonsStunned)
            {
                IMapItem mi1 = gi.Persons.Find(mi.Name);
                if (null == mi1)
                {
                    Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame()1: ERROR: GameState::CheckForEndOfGame() - unalbe to find " + mi.Name);
                    return true;
                }
                else
                {
                    mi1.IsStunned = false;
                    if (false == mi1.IsTiedUp)
                    {
                        gi.InfluenceCountTotal += mi1.Influence;
                        sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to TOTAL "); sb.Append(mi.Influence.ToString());
                        sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                        sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                        sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                        sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                        Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

                        if (true == mi1.IsAlienUnknown)
                        {
                            gi.InfluenceCountAlienUnknown += mi1.Influence;
                            sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to unknown "); sb.Append(mi.Influence.ToString());
                            sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                        }
                        else if (true == mi1.IsAlienKnown)
                        {
                            gi.InfluenceCountAlienKnown += mi1.Influence;
                            sb = new StringBuilder("CheckForEndOfGame(): unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to known "); sb.Append(mi.Influence.ToString());
                            sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                        }
                        else if (true == mi1.IsControlled)
                        {
                            gi.InfluenceCountTownspeople += mi1.Influence;
                            sb = new StringBuilder("CheckForEndOfGame() : unstunned "); sb.Append(mi.Name); sb.Append(" ++++ to TP "); sb.Append(mi.Influence.ToString());
                            sb.Append(" Tot="); sb.Append(gi.InfluenceCountTotal.ToString());
                            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
                        }

                    }
                }
            }
            gi.PersonsStunned.Clear();

            // -------------------------------------------------------
            // Knocked Out Map Items
            // For each person who was not recently knocked out, it converts to a stunned counter.

            foreach (IMapItem mi in gi.PersonsKnockedOut)
            {
                IMapItem mi1 = gi.Persons.Find(mi.Name);
                if (null != mi1)
                {
                    mi1.IsConscious = true;
                    mi1.IsStunned = true;  
                }
                else
                {
                    Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame()2: ERROR: GameState::CheckForEndOfGame() - unalbe to find " + mi.Name);
                    return true;
                }
            }
            gi.PersonsKnockedOut.Clear();

            if (false == Utilities.IsInfluenceCheck(gi))
            {
                MessageBox.Show("CheckForEndOfGame(): ERROR - Influence failure");
                Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfGame(): returned error");
            }

            // If Zebulon is dead, game is over.

            IMapItem zebulon = gi.Persons.Find("Zebulon");
            if (true == zebulon.IsKilled)
                return true;

            // If either the Alien or Townscontrolled influcence reaches zero,
            // the game is over.

            if (((gi.InfluenceCountAlienUnknown<=0) && (gi.InfluenceCountAlienKnown<=0)) || (gi.InfluenceCountTownspeople <= 0))
                return true;

            // Determine turn number.  If reach 12, game is over.

            int index = gi.GameTurn.IndexOf("#");
            string sGameTurn = gi.GameTurn.Substring(index + 1);
            int gameTurn = Int32.Parse(sGameTurn) + 1;
            if (12 < gameTurn)
                return true;
            gi.GameTurn = "Game Turn #" + gameTurn.ToString();

            return false;
        }
        #endregion

        #region bool CheckForRandomMoves(IGameInstance gi)
        static public bool CheckForRandomMoves(IGameInstance gi)
        {
            // Perform cleanup

            gi.IsAlienDisplayedRandomMovement = false;
            gi.IsControlledDisplayedRandomMovement = false;
            gi.IsAlienAckedRandomMovement = false;
            gi.IsControlledAckedRandomMovement = false;
            gi.Takeover = null;
            gi.MapItemCombat.IsAnyRetreat = false;
            foreach (IMapItemMove mim in gi.MapItemMoves)
            {
                IMapItem mi = mim.MapItem;
                mi.Territory = mim.NewTerritory;
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
                    || (true == mi.IsControlled) || (true == mi.IsAlienKnown) )
                    continue;

                return true; // If any mapitem can move randomly
            }

            return false;
        }
        #endregion
    }
    public abstract class GameState : IGameState
    {
        #region double GetDistance(ITerritory startT, ITerritory endT)
        public double GetDistance(ITerritory startT, ITerritory endT)
        {
            Point startPoint = new Point(startT.CenterPoint.X, startT.CenterPoint.Y);
            Point endPoint = new Point(endT.CenterPoint.X, endT.CenterPoint.Y);
            double xDelta = endPoint.X - startPoint.X;
            double yDelta = endPoint.Y - startPoint.Y;
            double distance = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
            return distance;
        }
        #endregion

        #region bool PerformMovement( IGameInstance gi, IMapItem mi )
        public bool PerformMovement(IGameInstance gi, IMapItem mi)
        {
            // Find the target building location.

            int r3 = GameEngine.RandomGenerator.Next(5);
            int r4 = GameEngine.RandomGenerator.Next(6);
            string building = Utilities.RemoveSpaces(Constants.targetBuildingTable[r3, r4]);

            // If moving to a build, randomly select a space from the building.
            // GetLength(0) gets the length of the array.

            int numOfSectorsInBuilding = 0;
            for (int i1 = 0; i1 < Constants.buildingSizes.GetLength(0); i1++)
            {
                string buildingToCompare = Utilities.RemoveSpaces(Constants.buildingSizes[i1, 0]);
                if (buildingToCompare == building)
                {
                    numOfSectorsInBuilding = Int32.Parse(Constants.buildingSizes[i1, 1]);
                    break;
                }
            }

            int selectedSector = GameEngine.RandomGenerator.Next(numOfSectorsInBuilding);
            ++selectedSector;

            ITerritory newTerritory = Territory.Find(building, selectedSector);
            if ((mi.Territory.Name == newTerritory.Name) && (mi.Territory.Sector == newTerritory.Sector))
            {
                return false;
            }
            mi.IsMoved = true;

            IMapItemMove mim = new MapItemMove(mi, newTerritory, gi.Persons);
            if (null != mim.NewTerritory)
            {
                gi.MapItemMoves.Add(mim);
                mi.Territory = mim.NewTerritory;
                int counterCount = 0;
                foreach (IMapItem mi1 in gi.Persons)
                {
                    if ((mi1.Territory.Name == mim.NewTerritory.Name) && (mi1.Territory.Sector == mim.NewTerritory.Sector))
                        ++counterCount;
                }
                mi.Location = new MapPoint(mi.Territory.CenterPoint.X - Utilities.theXOffset + (counterCount * 3), mi.Territory.CenterPoint.Y - Utilities.theYOffset + (counterCount * 3));
            }

            return true;
        }
        #endregion

        #region void PerformMovements( IGameInstance gi, int numPeopleToMove )
        public void PerformMovements(IGameInstance gi, int numPeopleToMove)
        {
            int numPeopleSkipped = 0;
            int numPeopleMoved = 0;
            int loopCount = 0;
            while ((numPeopleMoved < numPeopleToMove) && (++loopCount < 200))
            {
                int r1 = GameEngine.RandomGenerator.Next(5);
                int r2 = GameEngine.RandomGenerator.Next(6);
                string person = Utilities.RemoveSpaces(Constants.townsPersonTable[r1, r2]);
                IMapItem personMoving = gi.Persons.Find(person);
                if (null == personMoving)
                {
                    ++numPeopleSkipped;
                    //Console.WriteLine("PerformMovements(): {0} Unknown {1}", numPeopleSkipped.ToString(), person);
                    continue;
                }

                // If the counter is moved or tied up or known to be alien controlled, do not move.
                if ((true == personMoving.IsMoved) || (true == personMoving.IsControlled) || (true == personMoving.IsAlienKnown) ||
                    (true == personMoving.IsStunned) || (true == personMoving.IsSurrendered) ||
                    (true == personMoving.IsTiedUp) || (true == personMoving.IsWary) || (false == personMoving.IsConscious))
                {
                    ++numPeopleSkipped;
                    //                    Console.WriteLine("PerformMovements(): {0} Skipping Person {1}: {2},{3},{4},{5},{6},{7},{8},{9}",
                    //                                       numPeopleSkipped.ToString(),
                    //                                       personMoving.Name,
                    //                                       personMoving.IsMoved.ToString(),
                    //                                       personMoving.IsControlled.ToString(), personMoving.IsAlienKnown.ToString(),
                    //                                       personMoving.IsStunned.ToString(), personMoving.IsSurrendered.ToString(),
                    //                                       personMoving.IsTiedUp.ToString(), personMoving.IsWary.ToString(),
                    //                                       personMoving.IsConscious.ToString());
                    continue;
                }

                if (false == PerformMovement(gi, personMoving))
                {
                    ++numPeopleSkipped;
                    Console.WriteLine("PerformMovements(): {0} Same Building {1}", numPeopleSkipped.ToString(), person);
                    continue;
                }
                else
                {
                    ++numPeopleMoved;  // Keep track of number of people moved
                    //                    Console.WriteLine("PerformMovements(): {0} Moved {1}", numPeopleMoved.ToString(), person);
                }
            }  // end while()
        }
        #endregion

        #region IGameState GetGameState(GamePhase phase)
        static public IGameState GetGameState(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.AlienStart: return new GameStateSetup();
                case GamePhase.AlienMovement: return new GameStateAlienPlayerMovement();
                case GamePhase.AlienTakeover: return new GameStateAlienTakeover();
                case GamePhase.Combat: return new GameStateCombat();
                case GamePhase.Conversations: return new GameStateConversations();
                case GamePhase.ImplantRemoval: return new GameStateImplantRemoval();
                case GamePhase.Influences: return new GameStateInfluences();
                case GamePhase.Iterrogations: return new GameStateIterogations();
                case GamePhase.RandomMovement: return new GameStateRandomMovement();
                case GamePhase.ShowEndGame: return new GameStateEnded();
                case GamePhase.TownspersonMovement: return new GameStateTownPlayerMovement();
                case GamePhase.TownspersonStart: return new GameStateSetup();
                default: return new GameStateEnded();
            }
        }
        #endregion

        abstract public string PerformNextAction(IGameInstance gi, ref GameAction action);
    }
   //----------------------------------------------------------------
   #region GameStateStart
   class GameStateSetup : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region Alien Start
                case GameAction.AlienStart:
                    gi.IsAlienStarted = true;
                    if (true == gi.IsControlledStarted)
                    {
                        gi.GamePhase = GamePhase.RandomMovement;
                        gi.NextAction = "Display Random Movement";
                    }
                    else
                    {
                        gi.NextAction = "Awaiting Townsperson Start";
                    }
                    break;
                #endregion

                #region Townsperson Start
                case GameAction.TownspersonStart:
                    gi.IsControlledStarted = true;
                    if (true == gi.IsAlienStarted)
                    {
                        gi.GamePhase = GamePhase.RandomMovement;
                        gi.NextAction = "Display Random Movement";
                    }
                    else
                    {
                        gi.NextAction = "Awaiting Alien Start";
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateSetup.PerformNextAction() reached default with next action={0}", gi.NextAction);
                    break;
                #endregion
            }

            StringBuilder sb11 = new StringBuilder("\t\t\tGameState.PerformNextAction(gi,action) ==> action="); sb11.Append(action.ToString());
            sb11.Append(" IsAlienStarted="); sb11.Append(gi.IsAlienStarted); sb11.Append(" IsControlledStarted="); sb11.Append(gi.IsControlledStarted);
            sb11.Append(" GamePhase="); sb11.Append(gi.GamePhase.ToString()); sb11.Append(" NextAction="); sb11.Append(gi.NextAction);
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb11.ToString());

            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateRandomMovement
   class GameStateRandomMovement : GameState
    {
        #region void RecordIncapacitatedPeople(IGameInstance gi)
        public void RecordIncapacitatedPeople(IGameInstance gi)
        {
            // At the end of the turn, all stunned units become unstunned.
            // All knocked out people become stunned.

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (Stack stack in stacks)
            {
                foreach (MapItem mi in stack.MapItems)
                {
                    if (true == mi.IsStunned)
                        gi.PersonsStunned.Add(mi); // Keep a list of which MapItems are Stunned.

                    if (false == mi.IsConscious)
                        gi.PersonsKnockedOut.Add(mi); // Keep a list of which MapItems start the turn knocked out.
                }
            }
        }
        #endregion

        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                   break;
                #endregion

                #region AlienDisplaysRandomMovement
                case GameAction.AlienDisplaysRandomMovement:
                    gi.IsAlienDisplayedRandomMovement = true;
                    RecordIncapacitatedPeople(gi);
                    if (false == gi.IsControlledDisplayedRandomMovement)
                    {
                        PerformMovements(gi, 4);
                        gi.NextAction = "Awaiting Townsperson Display Random Movement";
                    }
                    else
                    {
                        if ( true == gi.IsControlledAckedRandomMovement )
                            gi.NextAction = "Awaiting Alien Ack Random Movement";
                        else
                            gi.NextAction = "Ack Random Movement";
                    }
                    break;
                #endregion

                #region TownspersonDisplaysRandomMovement
                case GameAction.TownspersonDisplaysRandomMovement:
                    gi.IsControlledDisplayedRandomMovement = true;
                    if (false == gi.IsAlienDisplayedRandomMovement)
                    {
                        PerformMovements(gi, 4);
                        gi.NextAction = "Awaiting Alien Display Random Movement";
                    }
                    else
                    {
                        if (true == gi.IsAlienAckedRandomMovement)
                            gi.NextAction = "Awaiting Townsperson Ack Random Movement";
                        else
                            gi.NextAction = "Ack Random Movement";
                    }
                    break;
                #endregion

                #region AlienAcksRandomMovement
                case GameAction.AlienAcksRandomMovement:
                    gi.IsAlienAckedRandomMovement = true;
                    if (true == gi.IsControlledAckedRandomMovement)
                    {
                        gi.IsAlienDisplayedRandomMovement = false;
                        gi.IsControlledDisplayedRandomMovement = false;
                        gi.IsAlienAckedRandomMovement = false;
                        gi.IsControlledAckedRandomMovement = false;

                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                        foreach (IMapItemMove mim in gi.MapItemMoves)
                        {
                            IMapItem mi = mim.MapItem;
                            mi.Territory = mim.NewTerritory;
                            mi.TerritoryStarting = mim.NewTerritory;
                            mi.IsMoved = false;
                            mi.MovementUsed = 0;
                        }
                        gi.MapItemMoves.Clear();
                        gi.MapItemCombat.IsAnyRetreat = false;
                        gi.Takeover = null;
                    }
                    else
                    {
                        if( false == gi.IsControlledDisplayedRandomMovement )
                           gi.NextAction = "Awaiting Townsperson Display Random Movement";
                        else
                           gi.NextAction = "Awaiting Townsperson Ack Random Movement";
                    }
                    break;
                #endregion

                #region TownspersonAcksRandomMovement
                case GameAction.TownspersonAcksRandomMovement:
                    gi.IsControlledAckedRandomMovement = true;
                    if (true == gi.IsAlienAckedRandomMovement)
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
                            mi.Territory = mim.NewTerritory;
                            mi.TerritoryStarting = mim.NewTerritory;
                            mi.IsMoved = false;
                            mi.MovementUsed = 0;
                        }
                        gi.MapItemMoves.Clear();

                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                        
                    }
                    else
                    {
                        if (false == gi.IsAlienDisplayedRandomMovement)
                            gi.NextAction = "Awaiting Alien Display Random Movement";
                        else
                            gi.NextAction = "Awaiting Alien Ack Random Movement";
                    }
                    break;
                #endregion

                #region default
                default:
                    Console.WriteLine("GameStateRandomMovement::PerformNextAction() reached default with action={0} NextAction={1}", action, gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateAlienPlayerMovement
   class GameStateAlienPlayerMovement : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region AlienMovement
                case GameAction.AlienMovement:
                    if (0 < gi.MapItemMoves.Count)
                    {
                        IMapItemMove mim1 = gi.MapItemMoves[0];
                        IMapItem movingMi1 = gi.Persons.Find(mim1.MapItem.Name);
                        movingMi1.MovementUsed += mim1.BestPath.Territories.Count;
                        movingMi1.IsMoved = true;
                    }
                    break;
                #endregion

                #region ResetMovement
                case GameAction.ResetMovement:
                    break;
                #endregion

                #region AlienCompletesMovement
                case GameAction.AlienCompletesMovement:
                    gi.NextAction = "Townsperson Acks Alien Movement";
                    break;
                #endregion

                #region TownspersonAcksAlienMovement
                case GameAction.TownspersonAcksAlienMovement:
                    foreach (IMapItem mi in gi.Persons)
                    {
                        mi.TerritoryStarting = mi.Territory;
                        mi.IsMoved = false;
                        mi.MovementUsed = 0;
                    }
                    gi.MapItemMoves.Clear();
                    gi.NextAction = "Townsperson Selects Counter to Move";
                    gi.GamePhase = GamePhase.TownspersonMovement;
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateAlienPlayerMovement::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateTownPlayerMovement
   class GameStateTownPlayerMovement : GameState
    {
        #region bool IsZebulonDiscovered(IGameInstance gi, IMapItemMove mim)
        private bool IsZebulonDiscovered(IGameInstance gi, IMapItemMove mim)
        {
            if (null != mim)
            {
                IMapItem movingMi = gi.Persons.Find(mim.MapItem.Name);
                if (null != movingMi)
                {
                    // Determine if Zebulon is along the path of this move.
                    // If so, back out to this new territory.

                    IMapItem zebulon = gi.Persons.Find("Zebulon");
                    foreach (ITerritory t in mim.BestPath.Territories)
                    {
                        if (false == zebulon.IsAlienKnown) // Determine if Zebulon is discovered
                        {
                            if ((t.Name == zebulon.Territory.Name) && (t.Sector == zebulon.Territory.Sector))
                            {
                                zebulon.IsAlienKnown = true;         // Zebulon is now exposed
                                movingMi.Territory = movingMi.TerritoryStarting; // Back out to the old Territory
                                movingMi.IsMoveAllowedToResetThisTurn = false;
                                movingMi.MovementUsed = 0;

                                IMapItemMove modifiedMove = new MapItemMove(movingMi, zebulon.Territory, gi.Persons);
                                gi.MapItemMoves[0] = modifiedMove;

                                movingMi.MovementUsed = movingMi.Movement;
                                movingMi.IsMoved = true;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region ResetMovement
                case GameAction.ResetMovement:
                    gi.PreviousMapItemMove = null;
                    break;
                #endregion

                #region TownpersonProposesMovement
                case GameAction.TownpersonProposesMovement:
                    // Based on the path taken by the moving MapItem, there 
                    // may be no capability for the Alien to stop the movement.  In that evnet,
                    // need to respond right away.  In that event, the returned action
                    // is TownspersonMovement.

                    if (0 < gi.MapItemMoves.Count)
                    {
                        IMapItemMove mim = gi.MapItemMoves[0];
                        IMapItem movingMi = gi.Persons.Find(mim.MapItem.Name);
                        foreach (IMapItem person in gi.Persons)
                        {

                            if ((false == person.IsWary) && (false == person.IsControlled) && (false == person.IsMoveStoppedThisTurn) && ("Zebulon" != person.Name)
                                && (false == person.IsStunned) && (false == person.IsTiedUp) && (false == person.IsSurrendered) && (false == person.IsKilled))
                            {

                                // Check if moving mapitem originates in territory controlled by alien

                                if ((person.Territory.Name == mim.OldTerritory.Name) && (person.Territory.Sector == mim.OldTerritory.Sector))
                                {
                                    Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Stop Move before started");
                                    gi.NextAction = "Alien May Elect to Stop Move if Possible";
                                    return returnStatus;
                                }

                                // Check if the moving mapitem moves along any path containing an alien

                                for (int i = 0; i < mim.BestPath.Territories.Count - 1; ++i) // Do not check the last territory moved into
                                {
                                    ITerritory t = mim.BestPath.Territories[i];
                                    if ((person.Territory.Name == t.Name) && (person.Territory.Sector == t.Sector))
                                    {
                                        Logger.Log(LogEnum.LE_TIMER_ELAPED, "Wait for Alien To Modify move");
                                        gi.NextAction = "Alien May Elect to Stop Move if Possible";
                                        return returnStatus;
                                    }
                                }
                            }
                        }

                        if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
                        {
                            action = GameAction.AlienModifiesTownspersonMovement;
                            gi.PreviousMapItemMove = null;
                        }
                        else
                        {
                            if( null != gi.PreviousMapItemMove )
                            {
                               foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                                  gi.ZebulonTerritories.Remove(t);
                            }

                            Logger.Log(LogEnum.LE_TIMER_ELAPED, "Continue Movement Without Waiting for Alien");
                            gi.NextAction = "Townsperson Selects Counter to Move";
                            action = GameAction.TownpersonMovement;
                            movingMi.MovementUsed += mim.BestPath.Territories.Count;
                            movingMi.IsMoved = true;
                            gi.PreviousMapItemMove = mim;
                        }
                    }
                    break;
                #endregion

                #region AlienTimeoutOnMovement
                case GameAction.AlienTimeoutOnMovement:
                    gi.NextAction = "Townsperson Selects Counter to Move";
                    if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
                    {
                        action = GameAction.AlienModifiesTownspersonMovement;
                        gi.PreviousMapItemMove = null;
                    }
                    else
                    {
                        if (null != gi.PreviousMapItemMove)
                        {
                            foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                                gi.ZebulonTerritories.Remove(t);
                        }
                        if (0 < gi.MapItemMoves.Count)
                        {
                            IMapItemMove mim1 = gi.MapItemMoves[0];
                            IMapItem movingMi1 = gi.Persons.Find(mim1.MapItem.Name);
                            movingMi1.IsMoved = true;
                            movingMi1.MovementUsed += mim1.BestPath.Territories.Count;
                            gi.PreviousMapItemMove = mim1;
                        }
                    }
                    break;
                #endregion   

                #region AlienModifiesTownspersonMovement
                case GameAction.AlienModifiesTownspersonMovement:
                    gi.NextAction = "Townsperson Selects Counter to Move";
                    if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
                    {
                        action = GameAction.AlienModifiesTownspersonMovement;
                        gi.PreviousMapItemMove = null;
                    }
                    else
                    {
                        if (0 < gi.MapItemMoves.Count)
                        {
                            IMapItemMove mim2 = gi.MapItemMoves[0];
                            IMapItem movingMi2 = gi.Persons.Find(mim2.MapItem.Name);
                            movingMi2.IsMoveAllowedToResetThisTurn = false;
                            movingMi2.MovementUsed += mim2.BestPath.Territories.Count;
                            movingMi2.IsMoved = true;
                            if (null != gi.PreviousMapItemMove)
                            {
                                foreach (ITerritory t in mim2.BestPath.Territories)
                                    gi.ZebulonTerritories.Remove(t);
                            }
                            gi.PreviousMapItemMove = null;
                        }
                    }
                    
                    break;
                #endregion

                #region TownpersonCompletesMovement
                case GameAction.TownpersonCompletesMovement:
                    if (true == IsZebulonDiscovered(gi, gi.PreviousMapItemMove))
                    {
                        gi.NextAction = "Townsperson Selects Counter to Move";
                        action = GameAction.AlienModifiesTownspersonMovement;
                    }
                    else if (null != gi.PreviousMapItemMove)
                    {
                        gi.NextAction = "Alien Acks Townspeople Movement";
                        foreach (ITerritory t in gi.PreviousMapItemMove.BestPath.Territories)
                            gi.ZebulonTerritories.Remove(t);
                    }
                    gi.PreviousMapItemMove = null;
                    break;
                #endregion
 
                #region AlienAcksTownspersonMovement
                case GameAction.AlienAcksTownspersonMovement:
                    foreach (IMapItem mi in gi.Persons)
                    {
                        mi.TerritoryStarting = mi.Territory;
                        mi.IsMoved = false;
                        mi.MovementUsed = 0;
                        mi.IsMoveStoppedThisTurn = false;
                    }
                    gi.MapItemMoves.Clear();

                    if (true == GameStateChecker.CheckForConversations(gi))
                    {
                        gi.NextAction = "Townsperson Select Flashing Space";
                        gi.GamePhase = GamePhase.Conversations;
                    }
                    else if (true == GameStateChecker.CheckForInfluence(gi))
                    {
                        gi.NextAction = "Townsperson Select Flashing Space";
                        gi.GamePhase = GamePhase.Influences;
                    }
                    else if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
                    {
                        gi.NextAction = "Each Player Select Flashing Space";
                        gi.GamePhase = GamePhase.Combat;
                    }
                    else if (true == GameStateChecker.CheckForIterogations(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                        gi.GamePhase = GamePhase.Iterrogations;
                    }
                    else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                    }
                    else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                    {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";        
                    }
                    else if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                        int index = gi.GameTurn.IndexOf("#");
                        string sGameTurn = gi.GameTurn.Substring(index + 1);
                        int gameTurn = Int32.Parse(sGameTurn) + 1;
                        gi.GameTurn = "Game Turn #" + gameTurn.ToString();
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateTownPlayerMovement::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateConversations
   class GameStateConversations : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region TownspersonPerformsConversation
                case GameAction.TownspersonPerformsConversation:
                    break;
                #endregion

                #region TownspersonCompletesConversations
                case GameAction.TownspersonCompletesConversations:
                    if (true == GameStateChecker.CheckForInfluence(gi))
                    {
                        gi.NextAction = "Townsperson Select Flashing Space";
                        gi.GamePhase = GamePhase.Influences;
                    }
                    else if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
                    {
                        gi.NextAction = "Decides Where to Perform Combats";
                        gi.GamePhase = GamePhase.Combat;
                    }
                    else if (true == GameStateChecker.CheckForIterogations(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                        gi.GamePhase = GamePhase.Iterrogations;
                    }
                    else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                    }
                    else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                    {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                    }
                    else if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Game Over";
                    }
                    else if (true == GameStateChecker.CheckForRandomMoves(gi))
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                    }
                    else
                    {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateConversations::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateInfluences
   class GameStateInfluences : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region TownspersonPerformsInfluencing
                case GameAction.TownspersonPerformsInfluencing:
                    break;
                #endregion

                #region TownspersonCompletesInfluencing
                case GameAction.TownspersonCompletesInfluencing:
                    if ((true == GameStateChecker.CheckForTownspersonCombats(gi)) || ((true == GameStateChecker.CheckForAlienCombats(gi))))
                    {
                        gi.NextAction = "Decides Where to Perform Combats";
                        gi.GamePhase = GamePhase.Combat;
                    }
                    else if (true == GameStateChecker.CheckForIterogations(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Interrogation";
                        gi.GamePhase = GamePhase.Iterrogations;
                    }
                    else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                    }
                    else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                    {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                    }
                    else if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if (true == GameStateChecker.CheckForRandomMoves(gi))
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                    }
                    else
                    {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateInfluences::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateCombat
   class GameStateCombat : GameState
    {
        static private CombatResult[,] theTable = new CombatResult[12, 5];

        #region Contructor
        public GameStateCombat()
        {
            theTable[0, 0] = CombatResult.DefenderWins;
            theTable[1, 0] = CombatResult.DefenderWins;
            theTable[2, 0] = CombatResult.DefenderWins;
            theTable[3, 0] = CombatResult.DefenderWins;
            theTable[4, 0] = CombatResult.DefenderFlees;
            theTable[5, 0] = CombatResult.AttackerFlees;
            theTable[6, 0] = CombatResult.DefenderFlees;
            theTable[7, 0] = CombatResult.AttackerWins;
            theTable[8, 0] = CombatResult.AttackerWins;
            theTable[9, 0] = CombatResult.AttackerWins;
            theTable[10, 0] = CombatResult.AttackerWins;

            theTable[0, 1] = CombatResult.AttackerFlees;
            theTable[1, 1] = CombatResult.AttackerWins;
            theTable[2, 1] = CombatResult.DefenderWins;
            theTable[3, 1] = CombatResult.DefenderWins;
            theTable[4, 1] = CombatResult.AttackerFlees;
            theTable[5, 1] = CombatResult.AttackerWins;
            theTable[6, 1] = CombatResult.DefenderFlees;
            theTable[7, 1] = CombatResult.AttackerWins;
            theTable[8, 1] = CombatResult.DefenderWins;
            theTable[9, 1] = CombatResult.AttackerWins;
            theTable[10, 1] = CombatResult.DefenderFlees;

            theTable[0, 2] = CombatResult.DefenderFlees;
            theTable[1, 2] = CombatResult.DefenderWins;
            theTable[2, 2] = CombatResult.AttackerFlees;
            theTable[3, 2] = CombatResult.AttackerWins;
            theTable[4, 2] = CombatResult.AttackerWins;
            theTable[5, 2] = CombatResult.AttackerWins;
            theTable[6, 2] = CombatResult.DefenderWins;
            theTable[7, 2] = CombatResult.DefenderFlees;
            theTable[8, 2] = CombatResult.AttackerWins;
            theTable[9, 2] = CombatResult.DefenderWins;
            theTable[10, 2] = CombatResult.AttackerWins;

            theTable[0, 3] = CombatResult.DefenderWins;
            theTable[1, 3] = CombatResult.DefenderWins;
            theTable[2, 3] = CombatResult.AttackerWins;
            theTable[3, 3] = CombatResult.DefenderFlees;
            theTable[4, 3] = CombatResult.AttackerWins;
            theTable[5, 3] = CombatResult.AttackerWins;
            theTable[6, 3] = CombatResult.AttackerWins;
            theTable[7, 3] = CombatResult.AttackerWins;
            theTable[8, 3] = CombatResult.DefenderWins;
            theTable[9, 3] = CombatResult.AttackerFlees;
            theTable[10, 3] = CombatResult.AttackerWins;

            theTable[0, 4] = CombatResult.AttackerWins;
            theTable[1, 4] = CombatResult.AttackerFlees;
            theTable[2, 4] = CombatResult.AttackerWins;
            theTable[3, 4] = CombatResult.AttackerWins;
            theTable[4, 4] = CombatResult.AttackerWins;
            theTable[5, 4] = CombatResult.AttackerWins;
            theTable[6, 4] = CombatResult.AttackerWins;
            theTable[7, 4] = CombatResult.DefenderFlees;
            theTable[8, 4] = CombatResult.AttackerWins;
            theTable[9, 4] = CombatResult.DefenderWins;
            theTable[10, 4] = CombatResult.DefenderWins;
        }
        #endregion

        #region PerformCombat( IGameInstance gi )
        public void PerformCombat(IGameInstance gi)
        {
            if (null == gi.MapItemCombat)
            {
                MessageBox.Show("No Combat");
                return;
            }

            IMapItemCombat combat = gi.MapItemCombat;
            if (null == combat.Territory)
            {
                MessageBox.Show("No combat territory");
                return;
            }
            combat.DieRoll1 = GameEngine.RandomGenerator.Next(6) + 1; // Assignment increases roll by one
            combat.DieRoll2 = GameEngine.RandomGenerator.Next(6) + 1; // Assignment increases roll by one
            int resultsRoll = combat.DieRoll1 + combat.DieRoll2 - 2;
            combat.IsAnyRetreat = false;  // assume no retreats until the results are known

            //-------------------------------------------------------------------------------
            // In each stack, get the count in the stack of the number of aliens 
            // and controlled townspeople

            IMapItems aliens = new MapItems();
            IMapItems controlled = new MapItems();
            IMapItems uncontrolled = new MapItems();
            IMapItems wary = new MapItems();

            foreach (MapItem mi in gi.Persons)
            {
                if ((combat.Territory.Name == mi.Territory.Name) && (combat.Territory.Sector == mi.Territory.Sector))
                {
                    if ((false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || (true == mi.IsSurrendered))
                        continue;

                    if (true == mi.IsAlienKnown)
                    {
                        aliens.Add(mi);
                    }
                    else if (true == mi.IsAlienUnknown)
                    {
                        if (false == gi.AddKnownAlien(mi)) // All aliens in this combat become exposed.
                            Logger.Log(LogEnum.LE_ERROR, "PerformCombat()1: returned error");
                        aliens.Add(mi);
                    }
                    else if (true == mi.IsControlled)
                    {
                        controlled.Add(mi);
                    }
                    else
                    {
                        if (true == mi.IsWary)
                            wary.Add(mi);
                        uncontrolled.Add(mi);
                    }
                }
            }

            // If there is no combat, return from this method

            if (0 == controlled.Count)
            {
                if ((0 == aliens.Count) || (0== wary.Count))
                  return;
            }

            // Determine the attack strength of the aliens.  
            // Limit it to top three counters.

            int alienAttackCombat = 0;
            int alienCount = 0;
            aliens = aliens.SortOnCombat();
            foreach (IMapItem alien in aliens)
            {
                alienAttackCombat += alien.Combat;
                StringBuilder sb = new StringBuilder("PerformCombat():"); sb.Append(alien.Name); sb.Append(" ++++ "); sb.Append(alien.Combat.ToString()); sb.Append(" to Alien="); sb.Append(alienAttackCombat.ToString());
                Logger.Log(LogEnum.LE_COMBAT_SUMS, sb.ToString());
                if (3 <= ++alienCount)
                    break;
            }

            // Determine the attack strength of the townspeople.  
            // Limit it to top three counters.

            int controlledAttackCombat = 0;
            int controlledCount = 0;
            controlled = controlled.SortOnCombat();
            foreach (IMapItem person in controlled)
            {
                controlledAttackCombat += person.Combat;
                StringBuilder sb = new StringBuilder("PerformCombat():"); sb.Append(person.Name); sb.Append(" ++++ "); sb.Append(person.Combat.ToString()); sb.Append(" to TP="); sb.Append(controlledAttackCombat.ToString());
                Logger.Log(LogEnum.LE_COMBAT_SUMS, sb.ToString());
                if (3 <= ++controlledCount)
                    break;
            }

            // Determine the attack strength of the wary townspeople.  
            // Limit it to top three counters.

            int waryAttackCombat = 0;
            int waryCount = 0;
            wary = wary.SortOnCombat();
            foreach (IMapItem person in wary)
            {
                waryAttackCombat += person.Combat;
                if (3 <= ++waryCount)
                    break;
            }

            int combatFactorDifference = 0;
            if ((0 < aliens.Count) && (0 < controlled.Count)) // A normal attack with known aliens in the same hex as controlled townspeople
            {
                // If there is no combat, ignore this stack

                if ((0 == alienCount) || (0 == controlledCount))
                    return;

                // Determine who is attackers and who are defenders based
                // on which side has the most Combat Factors.
            
                if (controlledAttackCombat < alienAttackCombat)
                {
                    combatFactorDifference = alienAttackCombat - controlledAttackCombat;
                    combat.Attackers = aliens;
                    combat.Defenders = controlled;
                }
                else
                {
                    combatFactorDifference = controlledAttackCombat - alienAttackCombat;
                    combat.Attackers = controlled;
                    combat.Defenders = aliens;
                }

                // Determine one index into the Combat Results Table.

                int tableFactor = 0;
                if (combatFactorDifference < 1)
                    tableFactor = 0;
                else if (combatFactorDifference < 4)
                    tableFactor = 1;
                else if (combatFactorDifference < 7)
                    tableFactor = 2;
                else if (combatFactorDifference < 10)
                    tableFactor = 3;
                else
                    tableFactor = 4;

                foreach (IMapItem alien in aliens) // A column shift occurs if any aliens went through an influence attempt this turn.
                {
                    if (true == alien.IsInfluencedThisTurn)
                    {
                        if (controlledAttackCombat < alienAttackCombat)  // aliens are attackers
                        {
                            if (0 == tableFactor)                        // shift column to right
                                tableFactor = 1;
                            else if (1 == tableFactor)
                                tableFactor = 2;
                            else if (2 == tableFactor)
                                tableFactor = 3;
                            else if (3 == tableFactor)
                                tableFactor = 4;
                        }
                        else                                             // aliens are defenders
                        {
                            if (1 == tableFactor)                        // shift column to left
                                tableFactor = 0;
                            else if (2 == tableFactor)
                                tableFactor = 1;
                            else if (3 == tableFactor)
                                tableFactor = 2;
                            else if (4 == tableFactor)
                                tableFactor = 3;
                        }
                        break;  // only one column shift occurs.
                    }
                }

                combat.Result = theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.
            }
            //***********************************************************************************************
            else if (0 < controlled.Count)  // Controlled townspeople attacking uncontrolled is automatic win
            {
                combat.Result = CombatResult.AttackerWins;
                combat.Attackers = controlled;
                combat.Defenders = uncontrolled;
            }
            //***********************************************************************************************
            else  // Alien townspeople attacking wary 
            {
                // If there is no combat, ignore this stack

                if ((0 == alienCount) || (0 == waryCount))
                    return;

                // Determine who is attackers and who are defenders based
                // on which side has the most Combat Factors.

                bool isAlienAttacker = false;

                if (waryAttackCombat < alienAttackCombat)
                {
                    combatFactorDifference = alienAttackCombat - waryAttackCombat;
                    combat.Attackers = aliens;
                    combat.Defenders = wary;
                    isAlienAttacker = true;
                }
                else
                {
                    combatFactorDifference = waryAttackCombat - alienAttackCombat;
                    combat.Attackers = wary;
                    combat.Defenders = aliens;
                }

                // Determine one index into the Combat Results Table.

                int tableFactor = 0;
                if (combatFactorDifference < 1)
                    tableFactor = 0;
                else if (combatFactorDifference < 4)
                    tableFactor = 1;
                else if (combatFactorDifference < 7)
                    tableFactor = 2;
                else if (combatFactorDifference < 10)
                    tableFactor = 3;
                else
                    tableFactor = 4;

                combat.Result = theTable[resultsRoll, tableFactor]; // The dice roll determines the other index into the Combat Results Table.
                
                // If Aliens lose to Wary people, the results is that the aliens immediately flee.

                if ((true == isAlienAttacker) && (CombatResult.DefenderWins == combat.Result))
                    combat.Result = CombatResult.DefenderFlees;
                if ((false == isAlienAttacker) && (CombatResult.AttackerWins == combat.Result))
                    combat.Result = CombatResult.AttackerFlees;
            }

            // Indicate who participated in the attack

            foreach (IMapItem defender in combat.Defenders)
                defender.IsCombatThisTurn = true;
            foreach (IMapItem attacker in combat.Attackers)
                attacker.IsCombatThisTurn = true;

            // Resolve the results

            switch (combat.Result)
            {
                case CombatResult.AttackerWins:
                    foreach (IMapItem defender in combat.Defenders)
                    {
                        PerformCombatResolveLoss(gi, defender);
                        if( true == defender.IsStunned ) // If the defender is stunned, they must retreat one territory
                            combat.IsAnyRetreat = true; 
                    }
                    break;

                case CombatResult.DefenderWins:
                    foreach (IMapItem attacker in combat.Attackers)
                    {
                        PerformCombatResolveLoss(gi, attacker);
                        if (true == attacker.IsStunned) // If the attacker is stunned, they must retreat one territory
                            combat.IsAnyRetreat = true;
                    }
                    break;

                case CombatResult.AttackerFlees:
                    combat.IsAnyRetreat = true;
                    foreach (IMapItem attacker in combat.Attackers)
                    {
                        if (attacker.Name == "Zebulon")
                        {
                            IMapItem zebulon = gi.Persons.Find("Zebulon");
                            zebulon.IsKilled = true;
                        }
                        attacker.TerritoryStarting = attacker.Territory;  // If there are any pending moves, make sure they are removed
                        if (false == PerformMovement(gi, attacker))
                            Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", attacker.Name);
                    }
                    break;

                case CombatResult.DefenderFlees:
                    combat.IsAnyRetreat = true;
                    foreach (IMapItem defender in combat.Defenders)
                    {
                        if (defender.Name == "Zebulon")
                        {
                            IMapItem zebulon = gi.Persons.Find("Zebulon");
                            zebulon.IsKilled = true;
                            return;
                        }
                        defender.TerritoryStarting = defender.Territory;  // If there are any pending moves, make sure they are removed
                       if (false == PerformMovement(gi, defender))
                            Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", defender.Name);
                    }
                    break;

                default:
                    Console.WriteLine("ERRROR - Reached default");
                    break;
            }
        } // end function
        #endregion

        #region void PerformCombatResolveLoss( IGameInstance gi, IMapItem mi)
        void PerformCombatResolveLoss(IGameInstance gi, IMapItem mi)
        {
            StringBuilder sb = null;
            if (mi.Name == "Zebulon")
            {
                IMapItem zebulon = gi.Persons.Find("Zebulon");
                zebulon.IsKilled = true;
                return;
            }

            // First perfom the actions that occur no matter what the result.
            // The influence factors are adjusted downward.

            gi.InfluenceCountTotal -= mi.Influence;
            sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from Total "); sb.Append(mi.Influence.ToString()); 
            sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
            sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
            sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
            sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
            Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());

            if (true == mi.IsAlienUnknown)
            {
                gi.InfluenceCountAlienUnknown -= mi.Influence;
                sb = new StringBuilder("PerformCombatResolveLoss(): "); sb.Append(mi.Name); sb.Append(" ---- from unknown "); sb.Append(mi.Influence.ToString()); 
                sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            else if (true == mi.IsAlienKnown)
            {
                gi.InfluenceCountAlienKnown -= mi.Influence;
                sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from known "); sb.Append(mi.Influence.ToString()); 
                sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            
            if( true == mi.IsControlled )
            {
                gi.InfluenceCountTownspeople -= mi.Influence;
                sb = new StringBuilder("PerformCombatResolveLoss():"); sb.Append(mi.Name); sb.Append(" ---- from TP "); sb.Append(mi.Influence.ToString()); 
                sb.Append(" T="); sb.Append(gi.InfluenceCountTotal.ToString());
                sb.Append(" Known="); sb.Append(gi.InfluenceCountAlienKnown.ToString());
                sb.Append(" UnKnown="); sb.Append(gi.InfluenceCountAlienUnknown.ToString());
                sb.Append(" TP="); sb.Append(gi.InfluenceCountTownspeople.ToString());
                Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }

            // Next see what the dice roll shows.

            int die1 = GameEngine.RandomGenerator.Next(6) + 1;
            int die2 = GameEngine.RandomGenerator.Next(6) + 1;
            int lossTableRoll = die1 + die2;

            if (lossTableRoll < 5)
            {
                mi.IsKilled = true;
             }
            else if (lossTableRoll < 7)
            {
                mi.IsConscious = false;
                if (true == mi.IsAlienKnown)
                    mi.IsTiedUp = true;
            }
            else
            {
                if (true == mi.IsAlienKnown)
                {
                    mi.IsSurrendered = true;
                    mi.IsTiedUp = true;
                }
                else
                {
                   mi.IsStunned = true;
                    int tempMovement = mi.Movement; // Set up to only retreat one space 
                    mi.Movement = 1;                // by setting the IMapItems movement to one.
                    mi.IsMoved = false;
                    mi.MovementUsed = 0;
                    mi.TerritoryStarting = mi.Territory;  // If there are any pending moves, make sure they are removed
                    if (false == PerformMovement(gi, mi))
                        Console.WriteLine("PerformCombatResolveLoss() No Retreat to same place for {0} ", mi.Name);
                    mi.Movement = tempMovement;     // return MapItem movement to original value
                }
            }
        }

        #endregion

        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region AlienInitiateCombat
                case GameAction.AlienInitiateCombat:
                    if( true == gi.IsAlienInitiatedCombat )
                        action = GameAction.TownspersonNackCombatSelection;
                    if ((false == gi.IsAlienInitiatedCombat) && (false == gi.IsControlledInitiatedCombat))
                    {
                        gi.IsAlienInitiatedCombat = true;
                        gi.MapItemMoves.Clear();    // Clear any previous retreats
                        gi.NextAction = "Alien Initiated Combat";
                    }
                    else
                    {
                        action = GameAction.TownspersonNackCombatSelection;
                    }
                    break;
                #endregion

                #region TownspersonInitiateCombat
                case GameAction.TownspersonInitiateCombat:
                    if( true == gi.IsControlledCombatCompleted )
                        action = GameAction.AlienNackCombatSelection;
                    if ((false == gi.IsAlienInitiatedCombat) && (false == gi.IsControlledInitiatedCombat))
                    {
                        gi.IsControlledInitiatedCombat = true;
                        gi.MapItemMoves.Clear();    // Clear any previous retreats
                        gi.NextAction = "Townsperson Initiated Combat";
                    }
                    else
                    {
                        action = GameAction.AlienNackCombatSelection;
                    }
                    break;
                #endregion

                #region AlienPerformCombat
                case GameAction.AlienPerformCombat:
                    PerformCombat(gi);
                    gi.IsAlienInitiatedCombat = false;
                    gi.NextAction = "Select Flashing Region to Initiate Combat";
                    IMapItem zebulon = gi.Persons.Find("Zebulon");
                    if (true == zebulon.IsKilled)
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if ((true == gi.IsControlledCombatCompleted) && ((false == GameStateChecker.CheckForAlienCombats(gi))))
                    {
                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                            gi.NextAction = "Townsperson chooses Black Building Space";
                            gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                            gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                            gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                            gi.GamePhase = GamePhase.AlienTakeover;
                            gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                            action = GameAction.ShowEndGame;
                            gi.GamePhase = GamePhase.ShowEndGame;
                            gi.NextAction = "End Game";
                            gi.GameTurn = "Completed";
                        }
                        else if (true == GameStateChecker.CheckForRandomMoves(gi))
                        {
                            gi.NextAction = "Display Random Movement";
                            gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                            gi.NextAction = "Alien Performs Movement";
                            gi.GamePhase = GamePhase.AlienMovement;
                        }
                    }
                    break;
                #endregion

                #region TownspersonPerformCombat
                case GameAction.TownspersonPerformCombat:
                    PerformCombat(gi);
                    gi.IsControlledInitiatedCombat = false;
                    IMapItem zebulon1 = gi.Persons.Find("Zebulon");
                    if (true == zebulon1.IsKilled)
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if ((true == gi.IsAlienCombatCompleted) && ((false == GameStateChecker.CheckForTownspersonCombats(gi))))
                    {
                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                            gi.NextAction = "Townsperson chooses Black Building Space";
                            gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                            gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                            gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                            gi.GamePhase = GamePhase.AlienTakeover;
                            gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                            action = GameAction.ShowEndGame;
                            gi.GamePhase = GamePhase.ShowEndGame;
                            gi.NextAction = "End Game";
                            gi.GameTurn = "Completed";
                        }
                        else if (true == GameStateChecker.CheckForRandomMoves(gi))
                        {
                            gi.NextAction = "Display Random Movement";
                            gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                            gi.NextAction = "Alien Performs Movement";
                            gi.GamePhase = GamePhase.AlienMovement;
                        }
                    }
                    break;
                #endregion

                #region TownspersonCompletesCombat
                case GameAction.TownspersonCompletesCombat:
                    gi.IsControlledCombatCompleted = true;
                    gi.IsControlledInitiatedCombat = false;
                    if (true == gi.IsAlienCombatCompleted)
                    {
                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        foreach (IMapItemMove mim in gi.MapItemMoves)
                        {
                            IMapItem mi = mim.MapItem;
                            mi.Territory = mim.NewTerritory;
                            mi.TerritoryStarting = mim.NewTerritory;
                            mi.IsMoved = false;
                            mi.MovementUsed = 0;
                        }
                        gi.MapItemMoves.Clear();

                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                            gi.NextAction = "Townsperson chooses Black Building Space";
                            gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                            gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                            gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                            gi.GamePhase = GamePhase.AlienTakeover;
                            gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                            action = GameAction.ShowEndGame;
                            gi.GamePhase = GamePhase.ShowEndGame;
                            gi.NextAction = "End Game";
                            gi.GameTurn = "Completed";
                        }
                        else if (true == GameStateChecker.CheckForRandomMoves(gi))
                        {
                            gi.NextAction = "Display Random Movement";
                            gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                            gi.NextAction = "Alien Performs Movement";
                            gi.GamePhase = GamePhase.AlienMovement;
                        }
                    }
                    else
                    {
                        gi.NextAction = "Awaiting Alien Complete Combat";
                    }
                    break;
                #endregion

                #region AlienCompletesCombat
                case GameAction.AlienCompletesCombat:
                    gi.IsAlienCombatCompleted = true;
                    gi.IsAlienInitiatedCombat = false;
                    if (true == gi.IsControlledCombatCompleted)
                    {
                        foreach (IMapItemMove mim in gi.MapItemMoves)
                        {
                            IMapItem mi = mim.MapItem;
                            mi.Territory = mim.NewTerritory;
                            mi.TerritoryStarting = mim.NewTerritory;
                            mi.IsMoved = false;
                            mi.MovementUsed = 0;
                        }
                        gi.MapItemMoves.Clear();

                        gi.IsAlienCombatCompleted = false;
                        gi.IsControlledCombatCompleted = false;
                        if (true == GameStateChecker.CheckForIterogations(gi))
                        {
                            gi.NextAction = "Townsperson chooses Black Building Space";
                            gi.GamePhase = GamePhase.Iterrogations;
                        }
                        else if (true == GameStateChecker.CheckForImplantRemoval(gi))
                        {
                            gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                            gi.GamePhase = GamePhase.ImplantRemoval;
                        }
                        else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                        {
                            gi.GamePhase = GamePhase.AlienTakeover;
                            gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                        }
                        else if (true == GameStateChecker.CheckForEndOfGame(gi))
                        {
                            action = GameAction.ShowEndGame;
                            gi.GamePhase = GamePhase.ShowEndGame;
                            gi.NextAction = "End Game";
                            gi.GameTurn = "Completed";
                        }
                        else if (true == GameStateChecker.CheckForRandomMoves(gi))
                        {
                            gi.NextAction = "Display Random Movement";
                            gi.GamePhase = GamePhase.RandomMovement;
                        }
                        else
                        {
                            gi.NextAction = "Alien Performs Movement";
                            gi.GamePhase = GamePhase.AlienMovement;
                        }
                    }
                    else
                    {
                        gi.NextAction = "Awaiting Townsperson Complete Combat";
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateCombat::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateIterogations
   class GameStateIterogations : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region TownspersonIterrogates
                case GameAction.TownspersonIterrogates:
                    if (0 == gi.NumIterogationsThisTurn)
                    {
                        gi.NextAction = "Alien Acknowledges Iterogations";
                        action = GameAction.TownspersonCompletesIterogations;
                    }
                    break;
                #endregion

                #region TownspersonCompletesIterogations
                case GameAction.TownspersonCompletesIterogations:
                    gi.NextAction = "Alien Acknowledges Iterogations";
                    break;
                #endregion

                #region AlienAcksIterogations
                case GameAction.AlienAcksIterogations:
                    if (true == GameStateChecker.CheckForImplantRemoval(gi))
                    {
                        gi.NextAction = "Townsperson chooses Flashing Space for Implant Removal";
                        gi.GamePhase = GamePhase.ImplantRemoval;
                    }
                    else if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                    {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                    }
                    else if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if (true == GameStateChecker.CheckForRandomMoves(gi))
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                    }
                    else
                    {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateIterogations::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateImplantRemoval
   class GameStateImplantRemoval : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region TownspersonCompletesInfluencing
                case GameAction.TownspersonCompletesInfluencing:
                    break;
                #endregion

                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region TownspersonCompletesRemoval
                case GameAction.TownspersonCompletesRemoval:
                    if (true == GameStateChecker.CheckForAlienTakeovers(gi))
                    {
                        gi.GamePhase = GamePhase.AlienTakeover;
                        gi.NextAction = "Alien Chooses Flashing Space for Takeover";
                    }
                    else if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if (true == GameStateChecker.CheckForRandomMoves(gi))
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                    }
                    else
                    {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateImplantRemoval::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   #region GameStateAlienTakeover
   class GameStateAlienTakeover : GameState
    {
        #region string PerformTakeover( IGameInstance gi )
        private string PerformTakeover(ref IGameInstance gi)
        {
            StringBuilder sb = new StringBuilder();

            if (null == gi.Takeover)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): takeover = null ");
                return "PerformTakeover() ERROR";
            }
            if (null == gi.Takeover.Alien)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): Alien = null ");
                return "PerformTakeover() ERROR";
            }
            if (null == gi.Takeover.Uncontrolled)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameStateAlienTakeover::PerformTakeover(): Uncontrolled = null ");
                return "PerformTakeover() ERROR";
            }

            // Determine if there are any observations.  If so, create a string to hold 
            // who and with what roll the observation happended.  

            List<Stack> stacks = new List<Stack>();
            stacks.AssignPeople(gi.Persons);
            foreach (String observation in gi.Takeover.Alien.Territory.Observations)
            {
                ITerritory obsTerritory = Territory.Find(observation);
                IMapItems people = stacks.FindPeople(obsTerritory);
                if (null != people)
                {
                    // Get distance between two territories

                    IMapPath path = MapItemMove.GetBestPath(gi.Takeover.Alien.Territory, obsTerritory, 3);
                    foreach (IMapItem person in people)
                    {
                        if (gi.Takeover.Uncontrolled.Name == person.Name)
                            continue;

                        if ((true == person.IsWary) || (true == person.IsAlienKnown) || (true == person.IsAlienUnknown) || (false == person.IsConscious) || (true == person.IsStunned) || (true == person.IsKilled))
                            continue;

                        int dieRoll = GameEngine.RandomGenerator.Next(6) + 1;
                        switch (path.Territories.Count)
                        {
                            case 0:
                                if (dieRoll < 5)
                                {
                                    person.IsWary = true;
                                    person.IsSkeptical = false;  // wary people are never skeptical
                                    sb.Append(person.Name);
                                    sb.Append(" observed with a die roll = ");
                                    sb.Append(dieRoll.ToString());
                                    sb.Append("\n");
                                }
                                break;

                            case 1:
                                if (dieRoll < 4)
                                {
                                    person.IsWary = true;
                                    person.IsSkeptical = false;  // wary people are never skeptical
                                    sb.Append(person.Name);
                                    sb.Append(" observed with a die roll = ");
                                    sb.Append(dieRoll.ToString());
                                    sb.Append("\n");
                                }
                                break;

                            case 2:
                                if (dieRoll < 3)
                                {
                                    person.IsWary = true;
                                    person.IsSkeptical = false;  // wary people are never skeptical
                                    sb.Append(person.Name);
                                    sb.Append(" observed with a die roll = ");
                                    sb.Append(dieRoll.ToString());
                                    sb.Append("\n");
                                }
                                break;

                            case 3:
                                if (dieRoll < 2)
                                {
                                    person.IsWary = true;
                                    person.IsSkeptical = false;  // wary people are never skeptical
                                    sb.Append(person.Name);
                                    sb.Append(" observed with a die roll = ");
                                    sb.Append(dieRoll.ToString());
                                    sb.Append("\n");
                                }
                                break;

                            default:
                                Logger.Log(LogEnum.LE_ERROR, "PerformTakeover(): reached default");
                                return "PerformTakeover() ERROR";

                        } // end switch

                    }  // end foreach (IMapItem person in people)

                }  // end else 

            }  //  end foreach (String observation in gi.Takeover.Alien.Territory.Observations)

            gi.Takeover.Observations = sb.ToString();
            if (0 == gi.Takeover.Observations.Count())
            {
                gi.Takeover.Observations = "Nobody Noticed";
                if ((true == gi.Takeover.Uncontrolled.IsControlled) || (true == gi.Takeover.Uncontrolled.IsWary))
                {
                    Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over controlled or wary ==> " + gi.Takeover.ToString());
                    if (false == gi.AddKnownAlien(gi.Takeover.Alien))
                    {
                        Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()1 returned error for " + gi.Takeover.Alien.Name);
                        return "PerformTakeover() ERROR";
                    }
                    if (false == gi.AddKnownAlien(gi.Takeover.Uncontrolled))
                    {
                        Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()2 returned error for " + gi.Takeover.Uncontrolled.Name);
                        return "PerformTakeover() ERROR";
                    }
                }
                else
                {
                    Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled without notice ==> " + gi.Takeover.ToString());
                    if (false == gi.AddUnknownAlien(gi.Takeover.Uncontrolled))
                    {
                        Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()3 returned error for " + gi.Takeover.Uncontrolled.Name);
                        return "PerformTakeover() ERROR";
                    }
                }
            }
            else
            {
                Logger.Log(LogEnum.LE_OBSERVATIONS, "PerformTakeover(): Taking over uncontrolled w/ observation ==> " + gi.Takeover.ToString());
                if (false == gi.AddKnownAlien(gi.Takeover.Alien))
                {
                    Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()4 returned error for " + gi.Takeover.Alien.Name);
                    return "PerformTakeover() ERROR";
                }
                if (false == gi.AddKnownAlien(gi.Takeover.Uncontrolled))
                {
                    Logger.Log(LogEnum.LE_ERROR, "PerformTakeover()5 returned error for " + gi.Takeover.Uncontrolled.Name);
                    return "PerformTakeover() ERROR";
                }
            }
            return "OK";

        } // end function
        #endregion

        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region AlienTakeover
                case GameAction.AlienTakeover:
                    returnStatus = PerformTakeover(ref gi);
                    break;
                #endregion

                #region AlienCompletesTakeovers
                case GameAction.AlienCompletesTakeovers:
                    if (true == GameStateChecker.CheckForEndOfGame(gi))
                    {
                        action = GameAction.ShowEndGame;
                        gi.GamePhase = GamePhase.ShowEndGame;
                        gi.NextAction = "End Game";
                        gi.GameTurn = "Completed";
                    }
                    else if (true == GameStateChecker.CheckForRandomMoves(gi))
                    {
                        gi.NextAction = "Display Random Movement";
                        gi.GamePhase = GamePhase.RandomMovement;
                    }
                    else
                    {
                        gi.NextAction = "Alien Performs Movement";
                        gi.GamePhase = GamePhase.AlienMovement;
                    }
                    break;
                #endregion

                #region default
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateAlienTakeover::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
            return returnStatus;
        }
    }
   #endregion
   //----------------------------------------------------------------
   class GameStateEnded : GameState
    {
        public override string PerformNextAction(IGameInstance gi, ref GameAction action)
        {
            String returnStatus = "OK";
            switch (action)
            {
                case GameAction.ShowAlien:
                    break;
                case GameAction.ShowEndGame:
                    break;
                case GameAction.ExitGame:
                    Application.Current.Shutdown();
                    break;
                default:
                    returnStatus = "Reached Default";
                    Console.WriteLine("GameStateEnded::PerformNextAction() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
            }
            return returnStatus;
        }
    }
}
