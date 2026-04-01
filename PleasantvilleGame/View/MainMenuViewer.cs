using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PleasantvilleGame
{
    class MainMenuViewer:IView
    {
        #region Fields ( myMainMenu, myMenuItemNextAction )
        private Canvas myCanvas;
        private Menu myMainMenu;
        private bool myIsAlien = false;
        private MenuItem myMenuItemGamePhase = null;
        private MenuItem myMenuItemNextAction = null;
        private MenuItem myMenuItemDisplay = null;
        private IGameEngine myGameEngine = null;
        private IGameInstance myGameInstance = null;
        private bool myIsZebulonTerritoriesVisible = false;

        private bool myIsTownspersonStarted = false;
        private bool myIsAlienStarted = false;
        private bool myIsTownspersonAcked = false;
        private bool myIsAlienAcked = false;

        #endregion

        #region Properties( none )
        #endregion

        #region Constructor (IGameEngine ge, IGameInstance gi, Canvas c, Menu mi )
        public MainMenuViewer(IGameEngine ge, IGameInstance gi, Canvas c, Menu mi, bool isAlien)
        {
            myGameEngine = ge;
            myGameInstance = gi;
            myMainMenu = mi;
            myCanvas = c;
            myIsAlien = isAlien;
            

            foreach (Control item in myMainMenu.Items)
            {
                if (item is MenuItem)
                {
                    MenuItem menuItem = (MenuItem)item;
                    if (menuItem.Name == "myMenuItemGamePhase")
                    {
                        myMenuItemGamePhase = menuItem;
                        myMenuItemGamePhase.Header = "_Game Actions";
                        myMenuItemGamePhase.InputGestureText = "Ctrl+G";

                        foreach (Control item1 in menuItem.Items)
                        {
                            MenuItem menuItem1 = (MenuItem)item1;
                            if (menuItem1.Name == "myMenuItemNextAction")
                            {
                                myMenuItemNextAction = menuItem1;
                                myMenuItemNextAction.Click += MenuItemNextAction_Click;
                                myMenuItemNextAction.Header = "_Start";
                                myMenuItemNextAction.InputGestureText = "Ctrl+P";
                            }
                            else if (menuItem1.Name == "myMenuItemDisplay")
                            {
                                myMenuItemDisplay = menuItem1;
                                myMenuItemDisplay.Click += MenuItemDisplay_Click;
                                myMenuItemDisplay.Header = "_Display Possible Zebulon Locatons";
                                myMenuItemDisplay.InputGestureText = "Ctrl+D";
                            }

                        } // end if (item is MenuItem)

                    } // end if (item is MenuItem)

                } // end foreach (Control item in myMainMenu.Items)

            } // end foreach (Control item in myMainMenu.Items)

        } // end MainMenuViewer()
        #endregion

        #region UpdateCanvasShowZebulonLocations()
        public void UpdateCanvasShowZebulonLocations()
        {
            IGameInstance gi = myGameInstance;
            SolidColorBrush aSolidColorBrush1 = new SolidColorBrush();
            aSolidColorBrush1.Color = Color.FromArgb(0, 0, 1, 0);

            if (false == myIsZebulonTerritoriesVisible)
            {
                myIsZebulonTerritoriesVisible = true;

                SolidColorBrush aSolidColorBrush2 = new SolidColorBrush();
                aSolidColorBrush2.Color = Colors.Black;

                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Polygon)
                    {
                        Polygon p = (Polygon)ui;
                        ITerritory t = gi.ZebulonTerritories.Find(p.Tag.ToString());
                        if (null == t)
                            p.Fill = aSolidColorBrush1;
                        else
                            p.Fill = aSolidColorBrush2;
                    }
                }
            }
            else
            {
                myIsZebulonTerritoriesVisible = false;
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Polygon)
                    {
                        Polygon p = (Polygon)ui;
                        p.Fill = aSolidColorBrush1;
                    }
                }
            }
        }
        #endregion

        #region UpdateView(ref IGameInstance gi, GameAction action)
        public void UpdateView(ref IGameInstance gi, GameAction action)
        {
            myGameInstance = gi;
            myMenuItemGamePhase.Header = gi.GameTurn;
            StringBuilder sb = new StringBuilder("-----------------MainMenuViewer::UpdateView() => action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
            Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, sb.ToString());
            switch (action)
            {
                #region AlienStart
                case GameAction.AlienStart:
                    if (true == myIsAlien)
                    {
                        myIsAlienStarted = true;
                        myMenuItemNextAction.Header = "_Display Random Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+D";
                        if (GamePhase.RandomMovement == gi.GamePhase)
                            myMenuItemNextAction.IsEnabled = true;
                        else
                            myMenuItemNextAction.IsEnabled = false;
                    }
                    else
                    {
                        if( true == myIsTownspersonStarted )
                            myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region TownspersonStart
                case GameAction.TownspersonStart:
                    if (false == myIsAlien)
                    {
                        myIsTownspersonStarted = true;
                        myMenuItemNextAction.Header = "_Display Random Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+D";
                        if (GamePhase.RandomMovement == gi.GamePhase)
                            myMenuItemNextAction.IsEnabled = true;
                        else
                            myMenuItemNextAction.IsEnabled = false;
                    }
                    else
                    {
                        if (true == myIsAlienStarted)
                            myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region AlienDisplaysRandomMovement
                case GameAction.AlienDisplaysRandomMovement:
                    if (true == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Random Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+D";
                    }
                    break;
                #endregion

                #region TownspersonDisplaysRandomMovement
                case GameAction.TownspersonDisplaysRandomMovement:
                    if (false == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Random Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+D";
                    }
                    break;
                #endregion

                #region AlienAcksRandomMovement
                case GameAction.AlienAcksRandomMovement:
                    if (true == myIsAlien)
                    {
                        myIsAlienAcked = true;
                        myMenuItemNextAction.Header = "_Complete Alien Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        if (GamePhase.AlienMovement == gi.GamePhase)
                            myMenuItemNextAction.IsEnabled = true;
                        else
                            myMenuItemNextAction.IsEnabled = false;
                    }
                    else if( true == myIsTownspersonAcked )
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+A";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    break;
                #endregion

                #region TownspersonAcksRandomMovement
                case GameAction.TownspersonAcksRandomMovement:
                    if (false == myIsAlien)
                    {
                        myIsTownspersonAcked = true;
                        myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+A";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    else if( true == myIsAlienAcked ) // alien player
                    {
                        myMenuItemNextAction.Header = "_Complete Alien Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region ResetMovement
                case GameAction.ResetMovement:
                    break;
                #endregion

                #region AlienMovement
                case GameAction.AlienMovement:
                    break;
                #endregion

                #region AlienCompletesMovement
                case GameAction.AlienCompletesMovement:
                    //Console.WriteLine("RESET");
                    myIsTownspersonAcked = false;
                    myIsAlienAcked = false;
                    if (true == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Townspeople Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+A";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    else
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region TownspersonAcksAlienMovement
                case GameAction.TownspersonAcksAlienMovement:
                    if (false == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Complete Townspeople Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region TownpersonProposesMovement
                case GameAction.TownpersonProposesMovement:
                    myMenuItemNextAction.IsEnabled = false;
                    break;
                #endregion

                #region AlienTimeoutOnMovement
                case GameAction.AlienTimeoutOnMovement:
                    myMenuItemNextAction.IsEnabled = true;
                    break;
                #endregion

                #region TownpersonMovement
                case GameAction.TownpersonMovement:
                    myMenuItemNextAction.IsEnabled = true;
                    break;
                #endregion

                #region AlienModifiesTownspersonMovement
                case GameAction.AlienModifiesTownspersonMovement:
                    myMenuItemNextAction.IsEnabled = true;
                    break;
                #endregion

                #region TownspersonCompletesMovement
                case GameAction.TownpersonCompletesMovement:
                    if (true == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Townspeople Movement";
                        myMenuItemNextAction.InputGestureText = "Ctrl+A";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    else
                    {
                        myMenuItemNextAction.Header = "_Complete Conversations";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    break;
                #endregion

                #region AlienAcksTownspersonMovement
                case GameAction.AlienAcksTownspersonMovement:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.TownspersonMovement == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Conversations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Influences == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.Conversations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Conversations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Influences == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Influencing";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    break;
                #endregion

                #region TownspersonPerformsConversation
                case GameAction.TownspersonPerformsConversation:
                    break;
                #endregion

                #region TownspersonCompletesConversations
                case GameAction.TownspersonCompletesConversations:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.Influences == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.Influences == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Influencing";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    break;
                #endregion

                #region TownspersonPerformsInfluencing
                case GameAction.TownspersonPerformsInfluencing:
                    break;
                #endregion

                #region TownspersonCompletesInfluencing
                case GameAction.TownspersonCompletesInfluencing:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    break;
                #endregion

                #region TownspersonInitiateCombat
                case GameAction.TownspersonInitiateCombat:
                    break;
                #endregion

                #region TownspersonPerformCombat
                case GameAction.TownspersonPerformCombat:
                    break;
                #endregion

                #region TownspersonCompletesCombat
                case GameAction.TownspersonCompletesCombat:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    break;
                #endregion

                #region AlienInitiateCombat
                case GameAction.AlienInitiateCombat:
                    break;
                #endregion

                #region AlienPerformCombat
                case GameAction.AlienPerformCombat:
                    break;
                #endregion

                #region AlienCompletesCombat
                case GameAction.AlienCompletesCombat:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.Combat == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Combats";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.Iterrogations == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Iterogations";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else 
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                    }
                    break;
                #endregion

                #region TownspersonIterrogates
                case GameAction.TownspersonIterrogates:
                    if (true == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Iterogations";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    else
                    {
                        myMenuItemNextAction.Header = "_Complete Iterogations";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    break;
                #endregion

                #region TownspersonCompletesIterogations
                case GameAction.TownspersonCompletesIterogations:
                    if (true == myIsAlien)
                    {
                        myMenuItemNextAction.Header = "_Acknowledge Iterogations";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = true;
                    }
                    else
                    {
                        myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                        myMenuItemNextAction.InputGestureText = "Ctrl+C";
                        myMenuItemNextAction.IsEnabled = false;
                    }
                    break;
                #endregion

                #region AlienAcksIterogations
                case GameAction.AlienAcksIterogations:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.ImplantRemoval == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                    }
                    break;
                #endregion

                #region TownspersonRemovesImplant
                case GameAction.TownspersonRemovesImplant:
                    break;
                #endregion

                #region TownspersonCompletesRemoval
                case GameAction.TownspersonCompletesRemoval:
                    if (true == myIsAlien)
                    {
                        if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Complete Takeovers";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (GamePhase.AlienTakeover == gi.GamePhase)
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = false;
                        }
                        else
                        {
                            myMenuItemNextAction.Header = "_Display Random Movement";
                            myMenuItemNextAction.InputGestureText = "Ctrl+C";
                            myMenuItemNextAction.IsEnabled = true;
                        }
                    }
                    break;
                #endregion

                #region AlienTakeover
                case GameAction.AlienTakeover:
                    break;
                #endregion

                #region AlienCompletesTakeovers
                case GameAction.AlienCompletesTakeovers:
                    myMenuItemNextAction.Header = "_Display Random Movement";
                    myMenuItemNextAction.InputGestureText = "Ctrl+C";
                    myMenuItemNextAction.IsEnabled = true;
                    break;
                #endregion

                #region ShowAlien
                case GameAction.ShowAlien:
                    break;
                #endregion

                #region ShowEndGame
                case GameAction.ShowEndGame:
                    myMenuItemNextAction.Header = "_Exit Game";
                    myMenuItemNextAction.InputGestureText = "Ctrl+E";
                    myMenuItemNextAction.IsEnabled = true;
                    break;
                #endregion

                #region default
                default:
                    Console.WriteLine("ERROR<<<<<MainMenuViewer::UpdateView() reached default with action={0} NextAction={1}", action.ToString(), gi.NextAction);
                    break;
                #endregion
            }
        }
        #endregion

        #region MenuItemNextAction_Click
        private void MenuItemNextAction_Click(object sender, RoutedEventArgs e)
        {
            IGameInstance gi = myGameInstance;

            // Game State follows the state pattern.  Each game state represents
            // a different object.  The initial game state is Setup.

            GameAction action = GameAction.TownspersonDisplaysRandomMovement;

            switch (myMenuItemNextAction.Header.ToString())
            {
                case "_Start":
                    if (true == myIsAlien)
                        action = GameAction.AlienStart;
                    else
                        action = GameAction.TownspersonStart;
                    break;

                case "_Display Random Movement":
                    if( true == myIsAlien )
                       action = GameAction.AlienDisplaysRandomMovement;
                    else
                       action = GameAction.TownspersonDisplaysRandomMovement;
                    break;

                case "_Acknowledge Random Movement":
                    if (true == myIsAlien)
                        action = GameAction.AlienAcksRandomMovement;
                    else
                        action = GameAction.TownspersonAcksRandomMovement;
                    break;

                case "_Complete Alien Movement":
                    action = GameAction.AlienCompletesMovement;
                    break;

                case "_Acknowledge Alien Movement":
                    action = GameAction.TownspersonAcksAlienMovement;
                    break;

                case "_Complete Townspeople Movement":
                    action = GameAction.TownpersonCompletesMovement;
                    break;

                case "_Acknowledge Townspeople Movement":
                    action = GameAction.AlienAcksTownspersonMovement;
                    break;

                case "_Complete Conversations":
                    action = GameAction.TownspersonCompletesConversations;
                    break;

                case "_Complete Influencing":
                    action = GameAction.TownspersonCompletesInfluencing;
                    break;

                case "_Complete Combats":
                    if (true == myIsAlien)
                        action = GameAction.AlienCompletesCombat;
                    else
                        action = GameAction.TownspersonCompletesCombat;
                    break;

                case "_Complete Iterogations":
                    action = GameAction.TownspersonCompletesIterogations;
                    break;

                case "_Acknowledge Iterogations":
                    action = GameAction.AlienAcksIterogations;
                    break;

                case "_Complete Takeovers":
                    action = GameAction.AlienCompletesTakeovers;
                    break;

                case "_Exit Game":
                    action = GameAction.ExitGame;
                    break;

                default:
                    Console.WriteLine("{0}", myMenuItemNextAction.Header.ToString());
                    break;
            }

            myGameEngine.PerformNextAction(ref gi, action);
            
        }
        #endregion

        #region MenuItemDisplay_Click
        public void MenuItemDisplay_Click(object sender, RoutedEventArgs e)
        {
            UpdateCanvasShowZebulonLocations();
        }
        #endregion

    }
}
