using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using Point = System.Windows.Point;
using CheckBox = System.Windows.Controls.CheckBox;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;
using Rectangle = System.Windows.Shapes.Rectangle;
using Brush = System.Windows.Media.Brush;
using Brushes=System.Windows.Media.Brushes;
using Application=System.Windows.Application;
using Color=System.Windows.Media.Color;
using MessageBox=System.Windows.MessageBox;

namespace PleasantvilleGame
{
   public partial class GameViewerWindow : Window, IView
   {
      //--------------------------------------------------------------
      public bool CtorError { get; } = false;
      private IGameEngine? myGameEngine = null;
      private IGameInstance? myGameInstance = null;
      private List<Button> myButtons = new List<Button>();
      private List<Brush> myBrushes = new List<Brush>();
      private List<Rectangle> myRectangles = new List<Rectangle>();
      //--------------------------------------------------------------
      private int myBrushIndex = 0;
      private DoubleCollection myDashArray = new DoubleCollection();
      private SolidColorBrush mySolidColorBrushClear = new SolidColorBrush();
      private SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();
      private SolidColorBrush mySolidColorBrushGray = new SolidColorBrush();          // Conversations
      private SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush();         // INfluences
      private SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();           // Combat
      private SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();        // Interogations
      private SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();     // Implant Removal
      private SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();        // Takeovers
      //--------------------------------------------------------------
      private MapItems myMovingMapItems = new MapItems();         // A list to track which MapItems have moved this turn
      private Button? myMovingButton = null;                      // The manually selected button that will be moved
      private ContextMenu myContextMenuCanvas = new ContextMenu();
      //--------------------------------------------------------------
      private bool myIsFlagSetForAlienMoveCountExceeded = false;  // Alien only allowed to move 5 counters
      private bool myIsFlagSetForMoveReset = false;               // Players cannot reset counter when selected
      private bool myIsFlagSetForOverstack = false;               // MapItem cannot move into hex due to overstack
      private bool myIsFlagSetForMaxMove = false;                 // MapItem cannot move into hex due to overstack
      //--------------------------------------------------------------
      //private Rectangle? myMovingRectangle = null;                // Rentangle that is moving with button
      private Rectangle myRectangleSelection = new Rectangle();   // Player has manually selected this button
      //--------------------------------------------------------------
      private Storyboard? myStoryboard = null;
      private System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
      private const int ANIMATE_SPEED = 3;
      protected bool myIsAlienAbleToStopMove = false;             // The Alien player is allowed to stop Townspeople from moving if in the same hex
      //--------------------------------------------------------------
      private ITerritories myTerritoriesCombatForAlien = new Territories();
      private ITerritories myTerritoriesCombatForTownsperson = new Territories();
      private bool myIsCombatInitiatedForAlien = false;
      private bool myIsCombatInitiatedForTownsperson = false;
      private bool myIsTakeOverInOneRegion = false;                 // These two state variable are used in Takeover phase.  It is used to indicate
      private bool myIsTakeOverPromptNeededToFoolOpponent = false;  // if Townsperson would learn information if the Takeover phase is skipped due to no possible takeovers.
      private bool myConversationsCompleted = false;
      private bool myInfluencesCompleted = false;
      private bool myAlienCombatCompleted = false;
      private bool myTownspeopleCombatCompleted = false;
      private bool myInterogationsCompleted = false;
      private bool myImplateRemovalsCompleted = false;
      private bool myTakeoversCompleted = false;
      //--------------------------------------------------------------
      private IMapItems myLeftMapItemsInActionPanel = new MapItems();
      private IMapItems myLeftMapItemsInActionPanelSelected = new MapItems();
      private IMapItems myRightMapItemsInActionPanel = new MapItems();
      private IMapItems myRightMapItemsInActionPanelSelected = new MapItems();
      public bool IsAlien { set; get; } = false;
      //==============================================================
      public GameViewerWindow(IGameEngine ge, IGameInstance gi, bool isServer=false, bool isAlien=false)
      {
         InitializeComponent();
         myGameEngine = ge;
         myGameInstance = gi;
         IsAlien = isAlien;
         if (true == IsAlien)
            myTextBoxEntry.Foreground = Constants.theAlienControlledBrush;
         else
            myTextBoxEntry.Foreground = Constants.theTownControlledBrush;

         //myTimer.Interval = ANIMATE_SPEED * 1000 + 1000;
         //myTimer.Tick += new EventHandler(this.TimerElasped);

         if (true == isAlien)
            this.BorderBrush = Constants.theAlienControlledBrush;
         else
            this.BorderBrush = Constants.theTownControlledBrush;

         StringBuilder sb55 = new StringBuilder();
         if (true == isServer)
            sb55.Append("SERVER: ");
         else
            sb55.Append("CLIENT: ");
         if (true == isAlien)
            sb55.Append("Pleasantville For Aliens");
         else
            sb55.Append("Pleasantville For Humans");
         this.Title = sb55.ToString();

         myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownCanvas;  // Connect up the Event Handler for mouse down
         myCanvas.MouseRightButtonDown += this.MouseRightButtonDownCanvas;  // Connect up the Event Handler for mouse down

         // Implement the Model View Controller (MVC) pattern by registering views with
         // the game engine such that when the model data is changed, the views are updated.

         ge.RegisterForUpdates(this);
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, isAlien);
         ge.RegisterForUpdates(sbv);
         MainMenuViewer mmv = new MainMenuViewer(myGameEngine, gi, myCanvas, myMainMenu, isAlien);
         ge.RegisterForUpdates(mmv);

         //----------------------------------------------------------
         // Create the territories and the regions marking the territories.
         // Keep a list of Territories used in the game.  All the information 
         // of Territories is static and does not change.
         foreach (ITerritory t in Territories.theTerritories)
         {
            if (0 < t.Points.Count)
            {
               Polygon aPolygon = new Polygon();
               aPolygon.Fill = mySolidColorBrushClear;
               aPolygon.Tag = Utilities.RemoveSpaces(t.ToString());
               aPolygon.Name = t.Name + t.Sector.ToString();
               myCanvas.RegisterName(aPolygon.Name, aPolygon);
               List<Point> points = new List<Point>();
               foreach (IMapPoint mp in t.Points)
                  points.Add(new Point(mp.X, mp.Y));
               PointCollection pointCollection = new PointCollection(points);
               aPolygon.Points = pointCollection;
               myCanvas.Children.Add(aPolygon);
            }
         }
         //------------------------------------------
         // Setup Context Menu for Buttons
         myContextMenuCanvas.Loaded += this.ContextMenuLoaded;
         MenuItem mi1 = new MenuItem();
         mi1.Header = "_Return to Starting point";
         mi1.InputGestureText = "Ctrl+S";
         mi1.Click += this.ContextMenuClickReturnToStart;
         myContextMenuCanvas.Items.Add(mi1);
         MenuItem mi2 = new MenuItem();
         mi2.Header = "_Rotate Stack";
         mi2.InputGestureText = "Ctrl+R";
         mi2.Click += this.ContextMenuClickRotate;
         myContextMenuCanvas.Items.Add(mi2);

         if (true == IsAlien)
         {
            MenuItem mi3 = new MenuItem();
            mi3.Header = "_Expose";
            mi3.InputGestureText = "Ctrl+E";
            mi3.Click += this.ContextMenuClickExposeAlien;
            myContextMenuCanvas.Items.Add(mi3);
            MenuItem mi4 = new MenuItem();
            mi4.Header = "_Stop Townsperson Move";
            mi4.InputGestureText = "Ctrl+S";
            mi4.Click += this.ContextMenuClickStopMove;
            myContextMenuCanvas.Items.Add(mi4);
         }

         //--------------------------------------------------
         // Create the buttons based on People

         foreach (IMapItem person in gi.Persons)
         {
            Button b = new Button();
            if (person.Name == "Zebulon")
            {
               if (false == IsAlien)
                  b.Visibility = Visibility.Hidden;
            }
            b.ContextMenu = myContextMenuCanvas;
            Canvas.SetLeft(b, person.Location.X - Utilities.theMapItemOffset);
            Canvas.SetTop(b, person.Location.Y - Utilities.theMapItemOffset);
            b.Click += this.ClickMapItem;
            b.MouseDoubleClick += this.MouseDoubleClickMapItem;
            b.Name = person.Name;
            b.Height = 50.0;
            b.Width = 50.0;
            b.IsEnabled = true;
            MapItem.SetButtonContent(b, person, IsAlien);
            myButtons.Add(b);
            myCanvas.Children.Add(b);
         }

         //-----------------------------------------------
         // Create standard color brushes

         Constants.theTownControlledBrush.Color = Color.FromArgb(0xFF, 0x33, 0xAA, 0x33);
         Constants.theAlienControlledBrush.Color = Color.FromArgb(0xFF, 0xFF, 0xD5, 0x00);  // 0xFFFFD500
         Constants.theSkepticalBrush.Color = Color.FromArgb(0xFF, 0xF2, 0xDE, 0x9B);
         Constants.theWaryBrush.Color = Color.FromArgb(0xFF, 0x87, 0xE5, 0x87);
         mySolidColorBrushClear.Color = Color.FromArgb(0, 0, 1, 0);
         mySolidColorBrushBlack.Color = Colors.Black;
         mySolidColorBrushGray.Color = Colors.Ivory;
         mySolidColorBrushGreen.Color = Colors.Green;
         mySolidColorBrushRed.Color = Colors.Red;
         mySolidColorBrushOrange.Color = Colors.Orange;
         mySolidColorBrushPurple.Color = Colors.Purple;
         mySolidColorBrushRosyBrown.Color = Colors.RosyBrown;

         //------------------------------------------------
         // Create a container of brushes for painting paths.
         // The first brush is the alien color.
         // The second brush is the townspeople color.

         myBrushes.Add(Brushes.Green);
         myBrushes.Add(Brushes.Blue);
         myBrushes.Add(Brushes.Purple);
         myBrushes.Add(Brushes.Yellow);
         myBrushes.Add(Brushes.Red);
         myBrushes.Add(Brushes.Orange);

         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines

         //-----------------------------------------------------------------
         // Create a Bounding Rectangles to indicate when a MapItem is moved

         for (int i = 0; i < 6; ++i)
         {
            Rectangle r = new Rectangle();
            r.Stroke = myBrushes[i];
            r.StrokeThickness = 2.0;
            r.StrokeDashArray = myDashArray;
            r.Width = 50;
            r.Height = 50;
            r.Visibility = Visibility.Hidden;
            myRectangles.Add(r);
            myCanvas.Children.Add(r);
         }

         // Create a Bounding Rectangle to indicate when a MapItem is selected to be moved by mouse pointer

         myRectangleSelection.Stroke = Brushes.Red;
         myRectangleSelection.StrokeThickness = 3.0;
         myRectangleSelection.Width = 50;
         myRectangleSelection.Height = 50;
         myRectangleSelection.Visibility = Visibility.Hidden;
         myCanvas.Children.Add(myRectangleSelection);
         Canvas.SetZIndex(myRectangleSelection, 1000);

         UpdateCanvas(gi); // Update the canvase based on data in the GameInstance
         ClearActionPanel();

      }
      //-------------INTERFACE FUNCTIONS---------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if( null == myGameEngine )
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::UpdateView() myGameEngine is null");
            return;
         }  
         if (true == IsAlien)
         {
            StringBuilder sb = new StringBuilder("---------------   ALIEN GameViewerWindow::UpdateView() ==> action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
            Logger.Log(LogEnum.LE_VIEW_UPDATE_WINDOW, sb.ToString());
         }
         else
         {
            StringBuilder sb = new StringBuilder("---------------   TP   GameViewerWindow::UpdateView() ==> action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
            Logger.Log(LogEnum.LE_VIEW_UPDATE_WINDOW, sb.ToString());
         }

         myGameInstance = gi;
         GameAction outAction = GameAction.Error;
         switch (action) // Perform acton based on the current next action.
         {
            case GameAction.AlienStart:
               break;
            case GameAction.TownspersonStart:
               break;
            case GameAction.AlienDisplaysRandomMovement:
               if (true == IsAlien)
               {
                  myStoryboard = null;
                  UpdateViewMovement(gi);
                  ClearActionPanel();
               }
               break;
            case GameAction.TownspersonDisplaysRandomMovement:
               if (false == IsAlien)
               {
                  myStoryboard = null;
                  UpdateViewMovement(gi);
                  ClearActionPanel();
               }
               break;
            case GameAction.AlienAcksRandomMovement:
               if (true == IsAlien)
               {
                  UpdateCanvas(gi);
                  myMovingMapItems.Clear();
                  myMovingButton = null;
                  myIsFlagSetForAlienMoveCountExceeded = false;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
                  myIsCombatInitiatedForAlien = false;
                  myIsCombatInitiatedForTownsperson = false;
                  myConversationsCompleted = false;
                  myInfluencesCompleted = false;
                  myAlienCombatCompleted = false;
                  myTownspeopleCombatCompleted = false;
                  myInterogationsCompleted = false;
                  myImplateRemovalsCompleted = false;
                  myTakeoversCompleted = false;
                  myTimer.Interval = ANIMATE_SPEED * 1000 + 3000;  // reset timer
               }
               break;
            case GameAction.TownspersonAcksRandomMovement:
               if (false == IsAlien)
               {
                  UpdateCanvas(gi);
                  myMovingMapItems.Clear();
                  myMovingButton = null;
                  myIsFlagSetForAlienMoveCountExceeded = false;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
                  myIsCombatInitiatedForAlien = false;
                  myIsCombatInitiatedForTownsperson = false;
                  myConversationsCompleted = false;
                  myInfluencesCompleted = false;
                  myAlienCombatCompleted = false;
                  myTownspeopleCombatCompleted = false;
                  myInterogationsCompleted = false;
                  myImplateRemovalsCompleted = false;
                  myTakeoversCompleted = false;
               }
               break;
            case GameAction.ResetMovement:
               UpdateCanvas(gi, true);  // unhide the pologon line shown
               break;
            case GameAction.AlienMovement:
               UpdateViewMovement(gi);
               ClearActionPanel();
               break;
            case GameAction.AlienCompletesMovement:
               if (true == IsAlien)
               {
                  UpdateCanvas(gi);
                  myMovingButton = null;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
               break;
            case GameAction.TownspersonAcksAlienMovement:
               UpdateCanvas(gi);
               myMovingMapItems.Clear();
               myMovingButton = null;
               myIsFlagSetForAlienMoveCountExceeded = false;
               myMovingRectangle = null;
               myRectangleSelection.Visibility = Visibility.Hidden;
               break;
            case GameAction.TownpersonProposesMovement:
               if (true == IsAlien)
               {
                  myIsAlienAbleToStopMove = true;
                  if (false == IsMoveStoppedByAlienBeforeStarted(gi))
                  {
                     UpdateViewMovement(gi);
                     myTimer.Start(); // give the Alien time to look at move
                  }
                  else
                  {
                     outAction = GameAction.AlienModifiesTownspersonMovement;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               else
               {
                  myMovingButton = null;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
               break;
            case GameAction.TownpersonMovement:
               myIsAlienAbleToStopMove = false;
               myTimer.Stop();
               UpdateViewMovement(gi);
               break;
            case GameAction.AlienTimeoutOnMovement:
               if (false == IsAlien)
               {
                  myIsAlienAbleToStopMove = false;
                  UpdateViewMovement(gi);
               }
               break;
            case GameAction.AlienModifiesTownspersonMovement:
               myIsAlienAbleToStopMove = false;
               UpdateCanvas(gi, true);
               UpdateViewMovement(gi);
               break;
            case GameAction.TownpersonCompletesMovement:
               if (false == IsAlien)
               {
                  UpdateCanvas(gi);
                  myMovingButton = null;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
               break;
            case GameAction.AlienAcksTownspersonMovement:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               myMovingMapItems.Clear();
               myMovingButton = null;
               myIsFlagSetForAlienMoveCountExceeded = false;
               myMovingRectangle = null;
               myRectangleSelection.Visibility = Visibility.Hidden;
               break;
            case GameAction.TownspersonPerformsConversation:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonCompletesConversations:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonPerformsInfluencing:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonCompletesInfluencing:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.AlienInitiateCombat:
               if ((true == myIsCombatInitiatedForAlien) && (true == IsAlien))
               {
                  UpdateCanvas(gi);
                  Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():AlienInitiateCombat: ALIEN Performs Combat");
                  outAction = GameAction.AlienPerformCombat;
                  myGameEngine.PerformAction(ref gi, ref outAction);
               }
               break;
            case GameAction.TownspersonNackCombatSelection:
               if (true == IsAlien)
               {
                  UpdateCanvas(gi);
                  UpdateViewState(gi);
                  myIsCombatInitiatedForAlien = false;
                  Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():TownspersonNackCombatSelection: ALIEN myIsCombatInitiatedForAlien=false");
               }
               break;
            case GameAction.AlienPerformCombat:
               UpdateCanvas(gi);
               if( null == gi.MapItemCombat )
               { 
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienPerformCombat gi.MapItemCombat=null");
               }
               else
               {
                   if (null != gi.MapItemCombat.Territory)
                   {
                      if ((0 != gi.MapItemCombat.Attackers.Count) && (0 != gi.MapItemCombat.Defenders.Count))
                         DisplayCombatResults(gi);
                   }
               }
               break;
            case GameAction.TownspersonInitiateCombat:
               if ((true == myIsCombatInitiatedForTownsperson) && (false == IsAlien))
               {
                  UpdateCanvas(gi);
                  Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():TownspersonInitiateCombat: TP PERFORMS COMBAT");
                  outAction = GameAction.TownspersonPerformCombat;
                  myGameEngine.PerformAction(ref gi, ref outAction);
               }
               break;
            case GameAction.AlienNackCombatSelection:
               if (false == IsAlien)
               {
                  UpdateCanvas(gi);
                  UpdateViewState(gi);
                  myIsCombatInitiatedForTownsperson = false;
                  Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():AlienNackCombatSelection: TP    myIsCombatInitiatedForTownsperson=false");
               }
               break;
            case GameAction.TownspersonPerformCombat:
               UpdateCanvas(gi);
               if (null != gi.MapItemCombat.Territory)
               {
                  if ((0 != gi.MapItemCombat.Attackers.Count) && (0 != gi.MapItemCombat.Defenders.Count))
                  {
                     DisplayCombatResults(gi);
                  }
               }
               break;
            case GameAction.TownspersonCompletesCombat:
               myRectangleSelection.Visibility = Visibility.Hidden;
               if (true == IsAlien)
               {
                  myMovingButton = null;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
               UpdateCanvas(gi);
               UpdateViewState(gi);
               myIsCombatInitiatedForTownsperson = false;
               StringBuilder sb2 = new StringBuilder("UpdateView():TownspersonCompletesCombat: "); sb2.Append(IsAlien.ToString()); sb2.Append("myIsCombatInitiatedForTownsperson=false");
               Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb2.ToString());
               break;
            case GameAction.AlienCompletesCombat:
               if (false == IsAlien)
               {
                  myMovingButton = null;
                  myMovingRectangle = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;

               }
               UpdateCanvas(gi);
               UpdateViewState(gi);
               myIsCombatInitiatedForAlien = false;
               StringBuilder sb3 = new StringBuilder("UpdateView():AlienCompletesCombat: "); sb3.Append(IsAlien.ToString()); sb3.Append("myIsCombatInitiatedForAlien=false");
               Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb3.ToString());
               break;
            case GameAction.TownspersonIterrogates:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonCompletesIterogations:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.AlienAcksIterogations:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonRemovesImplant:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.TownspersonCompletesRemoval:
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.AlienTakeover:
               UpdateCanvas(gi);
               if (null == gi.Takeover)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienTakeover gi.Takeover=null");
               }
               else
               {
                  if (null == gi.Takeover.Alien)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienTakeover gi.Takeover.Alien=null");
                  }
                  else
                  {
                     if (true == IsAlien)
                     {
                        myTextBoxResults.Text = gi.Takeover.Observations;
                        UpdateActionPanelButtons(gi);
                     }
                     else
                     {
                        if ("Nobody Noticed" != gi.Takeover.Observations)
                           PerformTakeoverObserved(gi);
                     }
                  }
               }
               UpdateViewState(gi);
               break;
            case GameAction.AlienCompletesTakeovers:
               UpdateCanvas(gi);
               myIsTakeOverInOneRegion = false;
               myIsTakeOverPromptNeededToFoolOpponent = false;
               break;
            case GameAction.ShowEndGame:
               ClearActionPanel();
               foreach (IMapItem mi in gi.Persons)         // SHow all the aliens
               {
                  if (true == mi.IsAlienUnknown)
                  {
                     if (false == gi.AddKnownAlien(mi))
                        Logger.Log(LogEnum.LE_ERROR, "UpdateView() returned error");
                  }
               }

               bool isAlienWin = true;                    // Determine who won.
               IMapItem zebulon = gi.Persons.Find("Zebulon");
               zebulon.IsAlienKnown = true;
               if (true == zebulon.IsKilled)
                  isAlienWin = false;

               double controlledRatio = ((double)gi.InfluenceCountTownspeople) / ((double)gi.InfluenceCountTotal);
               if (0.499999 < controlledRatio)
                  isAlienWin = false;

               int alienInflunence = gi.InfluenceCountAlienUnknown + gi.InfluenceCountAlienKnown;
               if (0 == alienInflunence)
                  isAlienWin = false;

               myLabelWinner.Visibility = Visibility.Visible;  // Report the winner
               if (true == isAlienWin)
               {
                  myLabelWinner.Content = "Aliens Win!!!!";
                  myLabelWinner.Foreground = Brushes.Orange;
               }
               else
               {
                  myLabelWinner.Content = "Towns People Win!!!!";
                  myLabelWinner.Foreground = Constants.theTownControlledBrush;
               }
               UpdateCanvas(gi);
               UpdateViewState(gi);
               break;
            case GameAction.ShowAlien:
               foreach (Stack stack in gi.Stacks)
               {
                  foreach (IMapItem mi in stack.MapItems)
                  {
                     if (true == mi.IsAlienKnown)
                     {
                        Button b = myButtons.Find(mi.Name);
                        if (null != b)
                           MapItem.SetButtonContent(b, mi, IsAlien);
                        IMapItem anotherMi = gi.Persons.Find(mi.Name);
                        if (null != anotherMi)
                           anotherMi.IsAlienKnown = true;
                     }
                  }
               }
               UpdateViewState(gi);
               break;
            default:
               Console.WriteLine("ERROR: GameViewerWindow::UpdateView() reached default next action={0}", action.ToString());
               break;
         }
      }
      //-------------UPDATE HELPER FUNCTIONS---------------------------------
      public void ClearActionPanel()
      {
         const int button1Left = 30;
         const int button2Left = 97;

         Canvas.SetLeft(myButton1, button1Left);
         Canvas.SetLeft(myRectangle1, button1Left);
         Canvas.SetLeft(myLabelButton1, button1Left);
         Canvas.SetLeft(myButton2, button2Left);
         Canvas.SetLeft(myRectangle2, button2Left);
         Canvas.SetLeft(myLabelButton2, button2Left);

         myLabelHeading.Visibility = Visibility.Hidden;
         myLabelLeftTop.Visibility = Visibility.Hidden;
         myLabelRightTop.Visibility = Visibility.Hidden;
         myLabelArrow.Visibility = Visibility.Hidden;
         myLabelButton1.Visibility = Visibility.Hidden;
         myLabelButton2.Visibility = Visibility.Hidden;
         myLabelButton3.Visibility = Visibility.Hidden;
         myLabelButton4.Visibility = Visibility.Hidden;
         myLabelButton5.Visibility = Visibility.Hidden;
         myLabelButton6.Visibility = Visibility.Hidden;

         Logger.Log(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL_CLEAR, "ClearActionPanel() myTextBoxResults.Clear()");
         myTextBoxResults.Clear();
         myTextBoxResults.Visibility = Visibility.Hidden;

         myButton1.Visibility = Visibility.Hidden;
         myButton2.Visibility = Visibility.Hidden;
         myButton3.Visibility = Visibility.Hidden;
         myButton4.Visibility = Visibility.Hidden;
         myButton5.Visibility = Visibility.Hidden;
         myButton6.Visibility = Visibility.Hidden;
         myButtonOk.Visibility = Visibility.Hidden;
         myButtonIgnore.Visibility = Visibility.Hidden;

         myRectangle1.Visibility = Visibility.Hidden;
         myRectangle2.Visibility = Visibility.Hidden;
         myRectangle3.Visibility = Visibility.Hidden;
         myRectangle4.Visibility = Visibility.Hidden;
         myRectangle5.Visibility = Visibility.Hidden;
         myRectangle6.Visibility = Visibility.Hidden;

         myLeftMapItemsInActionPanel.Clear();
         myRightMapItemsInActionPanel.Clear();
         myLeftMapItemsInActionPanelSelected.Clear();
         myRightMapItemsInActionPanelSelected.Clear();
      }
      public void UpdateActionPanel(IGameInstance gi, bool isOkButtonDisplayed)
      {
         const int button1Left = 169;
         const int button2Left = 97;

         myButton1.IsEnabled = true;
         myButton2.IsEnabled = true;
         myButton3.IsEnabled = true;
         myButton4.IsEnabled = true;
         myButton5.IsEnabled = true;
         myButton6.IsEnabled = true;

         String sb = "UpdateActionPanel() isOkButtonDisplayed=" + isOkButtonDisplayed.ToString();
         Logger.Log(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL, sb);
         switch (myLeftMapItemsInActionPanel.Count)
         {
            case 0:
               return;

            case 1:
               MapItem.SetButtonContent(myButton1, myLeftMapItemsInActionPanel[0], false);
               myButton1.Visibility = Visibility.Visible;
               Canvas.SetLeft(myButton1, button1Left);
               Canvas.SetLeft(myRectangle1, button1Left);
               Canvas.SetLeft(myLabelButton1, button1Left);
               myRectangle1.Visibility = Visibility.Visible;
               IMapItem mi = myLeftMapItemsInActionPanel[0];
               myLeftMapItemsInActionPanelSelected.Add(mi);
               break;
            case 2:
               MapItem.SetButtonContent(myButton1, myLeftMapItemsInActionPanel[0], false);
               MapItem.SetButtonContent(myButton2, myLeftMapItemsInActionPanel[1], false);
               myButton1.Visibility = Visibility.Visible;
               myButton2.Visibility = Visibility.Visible;
               myLabelLeftTop.Visibility = Visibility.Visible;
               Canvas.SetLeft(myButton1, button2Left);
               Canvas.SetLeft(myRectangle1, button2Left);
               Canvas.SetLeft(myLabelButton1, button2Left);
               Canvas.SetLeft(myButton2, button1Left);
               Canvas.SetLeft(myRectangle2, button1Left);
               Canvas.SetLeft(myLabelButton2, button1Left);
               break;
            default:
               myLeftMapItemsInActionPanel = myLeftMapItemsInActionPanel.Sort();
               myButton1.Visibility = Visibility.Visible;
               myButton2.Visibility = Visibility.Visible;
               myButton3.Visibility = Visibility.Visible;
               myLabelLeftTop.Visibility = Visibility.Visible;
               break;
         }

         switch (myRightMapItemsInActionPanel.Count)
         {
            case 0:
               break;
            case 1:
               MapItem.SetButtonContent(myButton4, myRightMapItemsInActionPanel[0], false);
               myButton4.Visibility = Visibility.Visible;

               myRectangle4.Visibility = Visibility.Visible;
               IMapItem mi = myRightMapItemsInActionPanel[0];
               myRightMapItemsInActionPanelSelected.Add(mi);
               break;
            case 2:
               myButton4.Visibility = Visibility.Visible;
               myButton5.Visibility = Visibility.Visible;
               myLabelRightTop.Visibility = Visibility.Visible;
               break;
            default:
               myRightMapItemsInActionPanel = myRightMapItemsInActionPanel.Sort();
               myButton4.Visibility = Visibility.Visible;
               myButton5.Visibility = Visibility.Visible;
               myButton6.Visibility = Visibility.Visible;
               myLabelRightTop.Visibility = Visibility.Visible;
               break;
         }

         if (true == isOkButtonDisplayed)
         {
            myButtonOk.Visibility = Visibility.Visible;
            myButtonIgnore.Visibility = Visibility.Visible;
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;

         UpdateActionPanelButtons(gi);
      }
      public void UpdateActionPanelButtons(IGameInstance gi)
      {
         if (Visibility.Visible == myButton1.Visibility)
         {
            if (0 < myLeftMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myLeftMapItemsInActionPanel[0].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton1, mi, IsAlien);
            }
         }
         if (Visibility.Visible == myButton2.Visibility)
         {
            if (1 < myLeftMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myLeftMapItemsInActionPanel[1].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton2, mi, IsAlien);
            }
         }
         if (Visibility.Visible == myButton3.Visibility)
         {
            if (2 < myLeftMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myLeftMapItemsInActionPanel[2].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton3, mi, IsAlien);
            }
         }

         if (Visibility.Visible == myButton4.Visibility)
         {
            if (0 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myRightMapItemsInActionPanel[0].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton4, mi, IsAlien);
            }
         }
         if (Visibility.Visible == myButton5.Visibility)
         {
            if (1 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myRightMapItemsInActionPanel[1].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton5, mi, IsAlien);
            }
         }
         if (Visibility.Visible == myButton6.Visibility)
         {
            if (2 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem mi = gi.Persons.Find(myRightMapItemsInActionPanel[2].Name);
               if (null != mi)
                  MapItem.SetButtonContent(myButton6, mi, IsAlien);
            }
         }
      }
      private void UpdateViewMovement(IGameInstance gi)
      {
         try
         {
            // First return the counter and button back to its original position

            foreach (IMapItemMove mim in gi.MapItemMoves)
            {
               IMapItem mi = mim.MapItem;
               int counterCount1 = 0;
               foreach (IMapItem mi1 in gi.Persons)
               {
                  if ((mi1.TerritoryCurrent.Name == mi.TerritoryStarting.Name) && (mi1.TerritoryCurrent.Sector == mi.TerritoryStarting.Sector))
                     ++counterCount1;
               }

               mi.TerritoryCurrent = mi.TerritoryStarting;
               mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount1 * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount1 * 3));

               Button b = myButtons.Find(mi.Name);
               if (null != b)
               {
                  MapItem.SetButtonContent(b, mi, IsAlien);
                  b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                  b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                  Canvas.SetLeft(b, mi.Location.X);
                  Canvas.SetTop(b, mi.Location.Y);
                  Canvas.SetZIndex(b, counterCount1);
               }
            }

            // Move it 

            foreach (IMapItemMove mim2 in gi.MapItemMoves)
            {
               if( null == mim2.NewTerritory )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateViewMovement() mim2.NewTerritory is null");
                  return ;
               }
               IMapItem mi = mim2.MapItem;
               int counterCount2 = 0;
               foreach (IMapItem mi2 in gi.Persons)
               {
                  if ((mi2.TerritoryCurrent.Name == mim2.NewTerritory.Name) && (mi2.TerritoryCurrent.Sector == mim2.NewTerritory.Sector))
                     ++counterCount2;
               }

               IMapItem alreadyMovedMapItem = myMovingMapItems.Find(mi.Name);
               if (null == alreadyMovedMapItem)
               {
                  ++myBrushIndex;
                  if (myBrushes.Count <= myBrushIndex)
                     myBrushIndex = 0;
                  myMovingMapItems.Add(mi);

                  // Reset flags

                  myIsFlagSetForAlienMoveCountExceeded = false;
                  myIsFlagSetForMoveReset = false;
                  myIsFlagSetForOverstack = false;
                  myIsFlagSetForMaxMove = false;

                  StringBuilder sb1 = new StringBuilder("UpdateViewMovement():");
                  if (true == IsAlien)
                     sb1.Append(" ALIEN sees moving ");
                  else
                     sb1.Append("  TP sees moving ");
                  sb1.Append(mi.Name);
                  Logger.Log(LogEnum.LE_SHOW_MIM_MOVING_COUNT, sb1.ToString());
               }

               MovePathDisplay(mim2);
               MovePathAnimate(mim2, gi.Persons);

               // Reset to its final position

               mi.TerritoryCurrent = mim2.NewTerritory;
               mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount2 * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount2 * 3));

               if (mi.Movement <= mi.MovementUsed)
               {
                  myMovingButton = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("UpdateViewMovement() - EXCEPTION THROWN e={0}", e.ToString());
         }
      }
      private void UpdateViewState(IGameInstance gi)
      {
         if( null == myGameEngine )
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::UpdateViewState() myGameEngine is null");
            return;
         }  
         myStoryboard = null;  // turn off flashing
         GameAction outAction = GameAction.Error;  
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
               if (false == DisplayConversations(gi))
               {
                  if (true != IsAlien || false != myConversationsCompleted)
                  {
                     break;
                  }
                  myConversationsCompleted = true;
                  outAction = GameAction.TownspersonCompletesConversations;
                  myGameEngine.PerformAction(ref gi, ref outAction);
                  break;
               }
               break;

            case GamePhase.Influences:
               if (false == DisplayInfluences(gi))
               {
                  if ((true == IsAlien) && (false == myInfluencesCompleted))
                  {
                     myInfluencesCompleted = true;
                     outAction = GameAction.TownspersonCompletesInfluencing;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               break;

            case GamePhase.Combat:
               if (false == DisplayCombats(gi))
               {
                  if (true == IsAlien)
                  {
                     if (false == myAlienCombatCompleted)
                     {
                        myAlienCombatCompleted = true;
                        outAction = GameAction.AlienCompletesCombat;
                        myGameEngine.PerformAction(ref gi, ref outAction);
                     }
                  }
                  else if (false == myTownspeopleCombatCompleted)
                  {
                     if (false == myTownspeopleCombatCompleted)
                     {
                        myTownspeopleCombatCompleted = true;
                        outAction = GameAction.TownspersonCompletesCombat;
                        myGameEngine.PerformAction(ref gi, ref outAction);
                     }
                  }
               }
               break;

            case GamePhase.Iterrogations:
               if (false == DisplayIterogations(gi))
               {
                  if ((false == IsAlien) && (false == myInterogationsCompleted))
                  {
                     myInterogationsCompleted = true;
                     outAction = GameAction.TownspersonCompletesIterogations;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               break;

            case GamePhase.ImplantRemoval:
               if (false == DisplayImplantRemovals(gi))
               {
                  if ((false == IsAlien) && (false == myImplateRemovalsCompleted))
                  {
                     myImplateRemovalsCompleted = true;
                     outAction = GameAction.TownspersonCompletesRemoval;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               break;

            case GamePhase.AlienTakeover:
               if (true == IsAlien)
               {
                  if (false == DisplayTakeovers(gi))
                  {
                     if (false == myTakeoversCompleted)
                     {
                        myTakeoversCompleted = true;
                        outAction = GameAction.AlienCompletesTakeovers;
                        myGameEngine.PerformAction(ref gi, ref outAction);
                     }
                  }
               }
               break;

            default:
               break;

         }  // end switch
      }
      private void UpdateCanvas(IGameInstance gi, bool isOnlyLastLegRemoved = false)
      {
         // Clean the Canvas of all marks

         List<UIElement> lines = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               Canvas.SetZIndex(p1, 0);
               ITerritory t = gi.ZebulonTerritories.Find(p1.Tag.ToString());
               switch (gi.GamePhase)
               {
                  case GamePhase.Iterrogations:
                     if (null == t)
                        p1.Fill = mySolidColorBrushClear;
                     else
                        p1.Fill = mySolidColorBrushBlack;
                     break;

                  case GamePhase.Conversations:
                  case GamePhase.Influences:
                  case GamePhase.Combat:
                  case GamePhase.AlienTakeover:
                     break;

                  default:
                     p1.Fill = mySolidColorBrushClear;
                     myStoryboard = null;  // turn off flashing
                     break;

               } // end switch

            } // end if us

            if (ui is Polyline)
               lines.Add(ui);

         } // end foreach

         if (false == isOnlyLastLegRemoved)
         {
            foreach (UIElement line in lines)
               myCanvas.Children.Remove(line);
         }
         else
         {
            if (0 < lines.Count)
               myCanvas.Children.Remove(lines.Last());
         }

         foreach (Rectangle r in myRectangles)
            r.Visibility = Visibility.Hidden;

         IMapItem zebulon = gi.Persons.Find("Zebulon");
         if (true == zebulon.IsAlienKnown)
         {
            Button b = myButtons.Find("Zebulon");
            if (null != b)
            {
               b.Visibility = Visibility.Visible;
               Canvas.SetZIndex(b, 100000);
            }
         }
         foreach (Stack stack in gi.Stacks) // Update the Canvas with new MapItem locations
         {
            int counterCount = 0;
            foreach (IMapItem mi in stack.MapItems)
            {
               Button b = myButtons.Find(mi.Name);
               if (null != b)
               {
                  b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                  b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset

                  if (true == mi.IsKilled)
                  {
                     if (Visibility.Visible == b.Visibility)
                     {
                        b.Visibility = Visibility.Hidden;
                        Logger.Log(LogEnum.LE_MOVE_KIA_RESULTS, mi.Name + " is hidden");
                     }
                  }
                  else
                  {
                     MapItem.SetButtonContent(b, mi, IsAlien);
                     mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount * 3));
                     ++counterCount;
                     Canvas.SetLeft(b, mi.Location.X);
                     Canvas.SetTop(b, mi.Location.Y);
                     Canvas.SetZIndex(b, counterCount);
                  }
               }
            }
         }

      }
      //-------------HELPER FUNCTIONS---------------------------------
      private bool IsMoveStoppedByAlienBeforeStarted(IGameInstance gi)
      {
         //if (0 == gi.MapItemMoves.Count)
         //   return false;
         //IMapItemMove mim = gi.MapItemMoves[0];
         ////List<Stack> stacks = new List<Stack>();
         ////stacks.AssignPeople(gi.Persons, IsAlien);
         //IEnumerable<Stack> results = from stack in gi.Stacks
         //                             where stack.Territory.Name == mim.OldTerritory.Name
         //                             where stack.Territory.Sector == mim.OldTerritory.Sector
         //                             select stack;
         //if (0 == results.Count())
         //   return false;
         //Stack s = results.First();

         //IMapItems aliens = new MapItems();
         //foreach (IMapItem mi in s.MapItems)
         //{
         //   if ((false == mi.IsWary) && ("Zebulon" != mi.Name) && (true != mi.IsStunned) && (true != mi.IsTiedUp) &&
         //       (true != mi.IsSurrendered) && (true != mi.IsKilled) && (false == mi.IsMoveStoppedThisTurn))
         //   {
         //      if ((true == mi.IsAlienKnown) || (true == mi.IsAlienUnknown))
         //         aliens.Add(mi);
         //   }
         //}
         //if (1 < s.MapItems.Count)
         //   myTimer.Interval = ANIMATE_SPEED * 1000 + 3000;
         //else
         //   myTimer.Interval = ANIMATE_SPEED * 1000 + 5000;
         //if (0 == aliens.Count)
         //   return false;
         //DialogStopMovement dlg = new DialogStopMovement(gi, mim.MapItem, aliens);
         //dlg.ShowDialog();
         //return dlg.IsMoveStopped;
         return false;
      }
      private void MovePathDisplay(IMapItemMove mim)
      {
         //if (null == mim.NewTerritory)
         //   return;
         //PointCollection aPointCollection = new PointCollection();
         //double offset = myMovingMapItems.Count % 6;
         //if (0 == myMovingMapItems.Count % 2)
         //   offset = -offset;
         //double xPostion = mim.OldTerritory.CenterPoint.X + offset;
         //double yPostion = mim.OldTerritory.CenterPoint.Y + offset;
         //Point newPoint = new Point(xPostion, yPostion);
         //aPointCollection.Add(newPoint);
         //foreach (ITerritory t in mim.BestPath.Territories)
         //{
         //   xPostion = t.CenterPoint.X + offset;
         //   yPostion = t.CenterPoint.Y + offset;
         //   newPoint = new Point(xPostion, yPostion);
         //   aPointCollection.Add(newPoint);
         //}

         //Polyline aPolyline = new Polyline();
         //aPolyline.Stroke = myBrushes[myBrushIndex];
         //aPolyline.StrokeThickness = 2;
         //aPolyline.StrokeEndLineCap = PenLineCap.Triangle;
         //aPolyline.Points = aPointCollection;
         //aPolyline.StrokeDashArray = myDashArray;
         //myCanvas.Children.Add(aPolyline);

         //myMovingRectangle = myRectangles[myBrushIndex];
         //Canvas.SetLeft(myMovingRectangle, mim.MapItem.Location.X);
         //Canvas.SetTop(myMovingRectangle, mim.MapItem.Location.Y);
         //myMovingRectangle.Visibility = Visibility.Visible;
      }
      private void MovePathAnimate(IMapItemMove mim, IMapItems persons)
      {
         //if (null == mim.NewTerritory)
         //   return;

         //persons.Remove(mim.MapItem.Name);  // These two step remove from middle of list 
         //persons.Add(mim.MapItem);          // and add to end so that it shows up on top.
         //IStack stack = 
         //List<Stack> stacks = new List<Stack>();
         //stacks.AssignPeople(persons, IsAlien);
         //IEnumerable<Stack> results = from stack in stacks
         //                             where stack.Territory.Name == mim.NewTerritory.Name
         //                             where stack.Territory.Sector == mim.NewTerritory.Sector
         //                             select stack;

         //int stackCount = 0;
         //if (0 != results.Count())
         //{
         //   Stack s = results.First();
         //   stackCount = s.MapItems.Count;
         //}

         //Button b = myButtons.Find(mim.MapItem.Name);
         //if (null == b)
         //   return;

         //try
         //{
         //   Canvas.SetZIndex(b, 100 + myMovingMapItems.Count); // Move the button to the top of the Canvas
         //   if (null != myMovingRectangle)
         //      Canvas.SetZIndex(myMovingRectangle, 110 + myMovingMapItems.Count); // Move the Rectangle

         //   PathFigure aPathFigure = new PathFigure();
         //   aPathFigure.StartPoint = new Point(mim.MapItem.Location.X, mim.MapItem.Location.Y);

         //   int lastItemIndex = mim.BestPath.Territories.Count - 1;
         //   for (int i = 0; i < lastItemIndex; i++)
         //   {
         //      ITerritory t = mim.BestPath.Territories[i];
         //      Point newPoint = new Point(t.CenterPoint.X - Utilities.theXOffset, t.CenterPoint.Y - Utilities.theYOffset);
         //      LineSegment lineSegment = new LineSegment(newPoint, false);
         //      aPathFigure.Segments.Add(lineSegment);
         //   }

         //   ITerritory newTerritory = mim.BestPath.Territories[lastItemIndex];

         //   // Add the last line segment

         //   Point newPoint2 = new Point(newTerritory.CenterPoint.X + (3 * stackCount) - Utilities.theXOffset, newTerritory.CenterPoint.Y + (3 * stackCount) - Utilities.theYOffset);
         //   LineSegment lineSegment2 = new LineSegment(newPoint2, false);
         //   aPathFigure.Segments.Add(lineSegment2);

         //   // Animiate the map item along the line segment

         //   PathGeometry aPathGeo = new PathGeometry();
         //   aPathGeo.Figures.Add(aPathFigure);
         //   aPathGeo.Freeze();

         //   DoubleAnimationUsingPath xAnimiation = new DoubleAnimationUsingPath();
         //   xAnimiation.PathGeometry = aPathGeo;
         //   xAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_SPEED);
         //   xAnimiation.Source = PathAnimationSource.X;

         //   DoubleAnimationUsingPath yAnimiation = new DoubleAnimationUsingPath();
         //   yAnimiation.PathGeometry = aPathGeo;
         //   yAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_SPEED);
         //   yAnimiation.Source = PathAnimationSource.Y;

         //   b.RenderTransform = new TranslateTransform();
         //   b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
         //   b.BeginAnimation(Canvas.TopProperty, yAnimiation);

         //   // Draw a bounding rectangle around the button and move it.

         //   if (null == myMovingRectangle)
         //   {
         //      Console.WriteLine("GameViewerWindow.MovePathAnimate() myMovingRectangle=null");
         //   }
         //   else
         //   {
         //      myMovingRectangle.RenderTransform = new TranslateTransform();
         //      myMovingRectangle.BeginAnimation(Canvas.LeftProperty, xAnimiation);
         //      myMovingRectangle.BeginAnimation(Canvas.TopProperty, yAnimiation);
         //   }

         //   if (null == myRectangleSelection)
         //   {
         //      Console.WriteLine("GameViewerWindow.MovePathAnimate() myRectangleSelection=null");
         //   }
         //   else
         //   {
         //      myRectangleSelection.RenderTransform = new TranslateTransform();
         //      myRectangleSelection.BeginAnimation(Canvas.LeftProperty, xAnimiation);
         //      myRectangleSelection.BeginAnimation(Canvas.TopProperty, yAnimiation);
         //   }

         //}
         //catch (Exception e)
         //{
         //   b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         //   b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         //   myRectangleSelection.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         //   myRectangleSelection.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         //   myMovingRectangle.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         //   myMovingRectangle.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         //   Console.WriteLine("MovePathAnimate() - EXCEPTION THROWN e={0}", e.ToString());
         //}
      }
      private bool DisplayConversations(IGameInstance gi)
      {
         //// Clear any previous flashing regions
         //myStoryboard = new Storyboard();
         //foreach (UIElement ui in myCanvas.Children)
         //{
         //   if (ui is Polygon)
         //   {
         //      Polygon p1 = (Polygon)ui;
         //      p1.Fill = mySolidColorBrushClear;
         //   }
         //}
         //// Display flashing regions where conversations can happen.
         //// Iterate through the stacks looking for multiple counters per stack.
         //// List<Stack> stacks = new List<Stack>();
         //stacks.AssignPeople(gi.Persons);
         //foreach (Stack stack in stacks)
         //{
         //   if (stack.MapItems.Count < 2)
         //      continue;
         //   // In each stack, get the count in the stack of the number of aliens 
         //   // and controlled townspeople
         //   IMapItems townspeopleControlled = new MapItems();
         //   IMapItems townspeopleUncontrolled = new MapItems();
         //   foreach (MapItem mi in stack.MapItems)
         //   {
         //      if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
         //         continue;
         //      if (true == mi.IsControlled)
         //      {
         //         townspeopleControlled.Add(mi);
         //      }
         //      else
         //      {
         //         if ((false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
         //            townspeopleUncontrolled.Add(mi);
         //      }
         //   }
         //   if ((0 == townspeopleControlled.Count) || (0 == townspeopleUncontrolled.Count))
         //      continue;
         //   // Turn the region red
         //   String targetName = townspeopleControlled[0].TerritoryCurrent.Name + townspeopleControlled[0].TerritoryCurrent.Sector.ToString();
         //   foreach (UIElement ui in myCanvas.Children)
         //   {
         //      if (ui is Polygon)
         //      {
         //         Polygon p1 = (Polygon)ui;
         //         if (p1.Name == targetName)
         //         {
         //            p1.Fill = mySolidColorBrushGray;
         //            Canvas.SetZIndex(p1, 1000);
         //            break;
         //         }
         //      }
         //   }
         //   // Perform animiation on the region
         //   DoubleAnimation anim = new DoubleAnimation();
         //   anim.From = 0.7;
         //   anim.To = 0.2;
         //   anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
         //   anim.AutoReverse = true;
         //   anim.RepeatBehavior = RepeatBehavior.Forever;
         //   myStoryboard.Children.Add(anim);
         //   Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
         //   Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         //} // end foreach (Stack stack in stacks)
         //if (0 == myStoryboard.Children.Count)
         //   return false;
         //myStoryboard.Begin(this);
         return true;
      }
      private void DisplayConversation(IGameInstance gi, ITerritory selectedTerritory)
      {
         //ClearActionPanel();
         ////----------------------------------------------------------------------
         //// If passed-in territory is not null, user has selected this region.
         //// Show a dialog of the conversation results.
         //if (null == selectedTerritory)
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "DisplayConversation() selectedTerritory=null");
         //   return;
         //}
         //List<Stack> stacks = new List<Stack>();
         //stacks.AssignPeople(gi.Persons);
         //IMapItems peopleInStack = stacks.FindPeople(selectedTerritory);
         //if (null != peopleInStack)
         //{
         //   myLeftMapItemsInActionPanel.Clear();
         //   myRightMapItemsInActionPanel.Clear();
         //   foreach (IMapItem mi in peopleInStack)
         //   {
         //      if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
         //         continue;

         //      if (true == mi.IsControlled)
         //      {
         //         myLeftMapItemsInActionPanel.Add(mi);
         //      }
         //      else
         //      {
         //         if ((false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
         //            myRightMapItemsInActionPanel.Add(mi);
         //      }
         //   }
         //   if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
         //   {
         //      UpdateActionPanel(gi, !IsAlien);

         //      myLabelHeading.Visibility = Visibility.Visible;
         //      myLabelArrow.Visibility = Visibility.Visible;
         //      myTextBoxResults.Visibility = Visibility.Visible;

         //      myLabelHeading.Content = "Conversing... \"Hello.  Are you an alien?\"";
         //      myLabelLeftTop.Content = "Choose a person who is talking:";
         //      myLabelRightTop.Content = "Choose a person being talked to:";
         //   }
         //}
      }
      private void PerformConversation(IGameInstance gi, bool isIgnoreResults)
      {
         // First get the influence factor of the townsperson talking.
         // Create a die roll modifier based-on this value.
         String sbstart = "PerformConversation() -isIgnoreResults=" + isIgnoreResults.ToString();
         Logger.Log(LogEnum.LE_SHOW_CONVERSATIONS, sbstart);
         if ((0 == myLeftMapItemsInActionPanelSelected.Count) || (0 == myRightMapItemsInActionPanelSelected.Count))
         {
            StringBuilder sb = new StringBuilder("PerformConversation(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }
         IMapItem leftMapItem = gi.Persons.Find(myLeftMapItemsInActionPanelSelected[0].Name);
         IMapItem rightMapItem = gi.Persons.Find(myRightMapItemsInActionPanelSelected[0].Name);
         leftMapItem.IsConversedThisTurn = true;
         if (false == isIgnoreResults)
         {
            int dieRollModifier = 0;
            if (15 < rightMapItem.Influence)
               dieRollModifier = 3;
            else if (10 < rightMapItem.Influence)
               dieRollModifier = 2;
            else if (5 < rightMapItem.Influence)
               dieRollModifier = 1;
            //------------------------------------------------
            int die1 = Utilities.RandomGenerator.Next(6) + 1;
            int die2 = Utilities.RandomGenerator.Next(6) + 1;
            int finalValue = die1 + die2 + dieRollModifier;
            int needRoll = 9 - dieRollModifier;
            //------------------------------------------------
            StringBuilder resultString = new StringBuilder("Modifier: +");
            resultString.Append(dieRollModifier.ToString());
            resultString.Append("\nNeed: ");
            resultString.Append(needRoll.ToString());
            resultString.Append("+");
            resultString.Append("\nRoll: ");
            resultString.Append(die1.ToString());
            resultString.Append(" + ");
            resultString.Append(die2.ToString());
            resultString.Append(" = ");
            resultString.Append(finalValue.ToString());
            resultString.Append("\n");
            resultString.Append(rightMapItem.Name);
            //------------------------------------------------
            if (8 < finalValue)
            {
               if (true == rightMapItem.IsAlienUnknown)
               {
                  gi.AddKnownAlien(rightMapItem);
                  resultString.Append(" is an Alien!!!!!!");
               }
               else
               {
                  resultString.Append(" says \"No not me!\"");
               }
            }
            else
            {
               resultString.Append(" says \"Really?  Have you been drinking?\"");
            }
            myTextBoxResults.Text = resultString.ToString();
         }
         GameAction outAction = GameAction.TownspersonPerformsConversation;
         myGameEngine.PerformAction(ref gi, ref outAction);
         if (true == isIgnoreResults)
            ClearActionPanel();
         else
            UpdateActionPanelButtons(gi);
      }
      private bool DisplayInfluences(IGameInstance gi)
      {
         myStoryboard = new Storyboard(); // Clear any previous flashing regions
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               p1.Fill = mySolidColorBrushClear;
            }
         }
         // Display flashing regions where conversations can happen.
         // Iterate through the stacks looking for multiple counters per stack.
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            // In each stack, get the count in the stack of the number of aliens 
            // and controlled townspeople
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
            if ((0 == townspeopleControlled.Count) || (0 == townspeopleUncontrolled.Count))
               continue;
            // Turn the region red
            String targetName = townspeopleControlled[0].TerritoryCurrent.Name + townspeopleControlled[0].TerritoryCurrent.Sector.ToString();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Polygon)
               {
                  Polygon p1 = (Polygon)ui;
                  if (p1.Name == targetName)
                  {
                     p1.Fill = mySolidColorBrushGreen;
                     Canvas.SetZIndex(p1, 1000);
                     break;
                  }
               }
            }
            //---------------------------------------------------
            DoubleAnimation anim = new DoubleAnimation(); // Perform animiation on the region
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;
            //---------------------------------------------------
            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)
         if (0 == myStoryboard.Children.Count)
            return false;
         myStoryboard.Begin(this);
         return true;
      }
      private void DisplayInfluence(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();
         if (null == selectedTerritory) 
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() selectedTerritory=null");
            return;
         }
         IStack stack = gi.Stacks.Find(selectedTerritory);
         if( null == stack )
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() stack=null");
            return;
         }
         if (null != stack.MapItems)
         {
            myLeftMapItemsInActionPanel.Clear();
            myRightMapItemsInActionPanel.Clear();
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsInfluencedThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;

               if (true == mi.IsControlled)
               {
                  myLeftMapItemsInActionPanel.Add(mi);
               }
               else
               {
                  if ((false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
                     myRightMapItemsInActionPanel.Add(mi);
               }
            }
            //----------------------------------------------------------------------
            if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
            {
               UpdateActionPanel(gi, !IsAlien);
               //----------------------------------------------------------------------
               for (int i = 0; i < myLeftMapItemsInActionPanel.Count; ++i)
               {
                  IMapItem leftMi = myLeftMapItemsInActionPanel[i];
                  if (true == leftMi.IsImplantHeld)
                  {
                     switch (i)
                     {
                        case 0: myLabelButton1.Visibility = Visibility.Visible; myLabelButton1.Content = "Has Implant"; break;
                        case 1: myLabelButton2.Visibility = Visibility.Visible; myLabelButton2.Content = "Has Implant"; break;
                        case 2: myLabelButton3.Visibility = Visibility.Visible; myLabelButton3.Content = "Has Implant"; break;
                        default: break;
                     }
                  }
               }
               //----------------------------------------------------------------------
               for (int i = 0; i < myRightMapItemsInActionPanel.Count; ++i)
               {
                  IMapItem rightMi = myRightMapItemsInActionPanel[i];
                  if (true == rightMi.IsSkeptical)
                  {
                     switch (i)
                     {
                        case 0: myLabelButton4.Visibility = Visibility.Visible; myLabelButton4.Content = "Skeptical"; break;
                        case 1: myLabelButton5.Visibility = Visibility.Visible; myLabelButton5.Content = "Skeptical"; break;
                        case 2: myLabelButton6.Visibility = Visibility.Visible; myLabelButton6.Content = "Skeptical"; break;
                        default: break;
                     }
                  }
                  if (true == rightMi.IsWary)
                  {
                     switch (i)
                     {
                        case 0: myLabelButton4.Visibility = Visibility.Visible; myLabelButton4.Content = "Wary"; break;
                        case 1: myLabelButton5.Visibility = Visibility.Visible; myLabelButton5.Content = "Wary"; break;
                        case 2: myLabelButton6.Visibility = Visibility.Visible; myLabelButton6.Content = "Wary"; break;
                        default: break;
                     }
                  }
               }
               //----------------------------------------------------------------------
               myLabelHeading.Visibility = Visibility.Visible;
               myLabelArrow.Visibility = Visibility.Visible;
               myTextBoxResults.Visibility = Visibility.Visible;
               myLabelHeading.Content = "Influencing... \"Please help me fight the aliens.\"";
               myLabelLeftTop.Content = "First, choose one or more persons:";
               myLabelRightTop.Content = "Last, choose a person being influenced:";
            }
         }
      }
      private void PerformInfluence(IGameInstance gi, bool isIgnoreResults)
      {
         if ((0 == myLeftMapItemsInActionPanelSelected.Count) || (0 == myRightMapItemsInActionPanelSelected.Count))
         {
            StringBuilder sb = new StringBuilder("PerformInfluence(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }

         double totalInfluence = 0;
         bool isImplantHeld = false;
         foreach (IMapItem mi in myLeftMapItemsInActionPanelSelected)
         {
            IMapItem leftMapItem = gi.Persons.Find(myLeftMapItemsInActionPanelSelected[0].Name);
            leftMapItem.IsInfluencedThisTurn = true;
            totalInfluence += (double)mi.Influence;
            if (true == mi.IsImplantHeld)
               isImplantHeld = true;
         }

         IMapItem rightMapItem = gi.Persons.Find(myRightMapItemsInActionPanelSelected[0].Name);
         rightMapItem.IsInfluencedThisTurn = true;

         if (false == isIgnoreResults)
         {
            StringBuilder displayResults = new StringBuilder("Odds: ");
            double odds = totalInfluence / ((double)rightMapItem.Influence);

            int dieThreshold = -99;
            if (3.999 < odds)
            {
               dieThreshold = 3;
               displayResults.Append("4-1");
            }
            else if (2.999 < odds)
            {
               dieThreshold = 4;
               displayResults.Append("3-1");
            }
            else if (1.999 < odds)
            {
               dieThreshold = 5;
               displayResults.Append("2-1");
            }
            else if (1.499 < odds)
            {
               dieThreshold = 6;
               displayResults.Append("3-2");
            }
            else if (0.999 < odds)
            {
               dieThreshold = 7;
               displayResults.Append("1-1");
            }
            else if (0.666 < odds)
            {
               dieThreshold = 8;
               displayResults.Append("2-3");
            }
            else if (0.499 < odds)
            {
               dieThreshold = 9;
               displayResults.Append("1-2");
            }
            else
            {
               dieThreshold = 10;
               displayResults.Append("1-3");
            }

            // Perform die roll modifier.
            // Subtact one if a controlled person holds evidence of an implant.
            // Check if MapItem is skeptical.  If both skeptical and wary,
            // cancel them out with no die roll modifier.
            // If not skeptical, check if wary.  This adds to the die roll.

            int dieRollModifier = 0;
            if (true == isImplantHeld)
               --dieRollModifier;
            if (true == rightMapItem.IsSkeptical)
               ++dieRollModifier;
            if (true == rightMapItem.IsWary)
               --dieRollModifier;

            dieThreshold += dieRollModifier;

            int die1 = Utilities.RandomGenerator.Next(6) + 1;
            int die2 = Utilities.RandomGenerator.Next(6) + 1;
            int sum = die1 + die2;

            displayResults.Append("\nModifier: ");
            if (0 <= dieRollModifier)
               displayResults.Append("+");
            displayResults.Append(dieRollModifier.ToString());

            displayResults.Append("\nNeed: ");
            displayResults.Append(dieThreshold.ToString());
            displayResults.Append("+");

            displayResults.Append("\nRoll: ");
            displayResults.Append(die1.ToString());
            displayResults.Append(" + ");
            displayResults.Append(die2.ToString());
            displayResults.Append(" = ");
            displayResults.Append(sum.ToString());
            displayResults.Append("\n");
            displayResults.Append(rightMapItem.Name);

            if (dieThreshold <= sum)
            {
               // Check for alien.  If alien, let user know it is discovered.
               // Else, make the townsperson controlled.

               if (true == rightMapItem.IsAlienUnknown)
               {
                  if (false == gi.AddKnownAlien(rightMapItem))
                     Logger.Log(LogEnum.LE_ERROR, "CheckForConversion() returned error");
                  displayResults.Append(" is an Alien!!!!!!");
               }
               else
               {
                  if (false == gi.AddTownperson(rightMapItem))
                     Logger.Log(LogEnum.LE_ERROR, "CheckForConversion() returned error");
                  displayResults.Append(" says \"You are right.  Let's go get 'em!\"");
               }
            }
            else
            {
               if (false == rightMapItem.IsWary)  // wary people cannot become skeptical
               {
                  rightMapItem.IsSkeptical = true;
                  displayResults.Append(" says \"Are you crazy?  That is absurd!\"");
               }
               else
               {
                  displayResults.Append(" says \"Hmmmm.  It seems so unlikely.\"");
               }
            }

            myTextBoxResults.Text = displayResults.ToString();

         } // if( true == isIgnoreResults)
         GameAction outAction = GameAction.TownspersonPerformsInfluencing;
         myGameEngine.PerformAction(ref gi, ref outAction);
         if (true == isIgnoreResults)
            ClearActionPanel();
         else
            UpdateActionPanelButtons(gi);

      }
      private bool DisplayCombats(IGameInstance gi)
      {
         // This method collects all possible combats in the respective containers 
         // <myTerritoriesCombatForAlien> or <myTerritoriesCombatForTownsperson>.  It turns 
         // the spaces red and causes them to flash as an indication to the user that they 
         // can be selected.  This function returns true if there are any possible combats
         // or retreats from previous combats.
         //----------------------------------------------------------------------
         myStoryboard = new Storyboard();
         foreach (UIElement ui in myCanvas.Children) // Clear any previous flashing regions
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               p1.Fill = mySolidColorBrushClear;
            }
         }
         if (true == IsAlien)
            myTerritoriesCombatForAlien.Clear();
         else
            myTerritoriesCombatForTownsperson.Clear();
         //----------------------------------------------------------------------
         foreach (Stack stack in gi.Stacks) // Display flashing regions where conversations can happen. Iterate through the stacks looking for multiple counters per stack.
         {
            if (stack.MapItems.Count < 2)
               continue;
            // In each stack, get the count in the stack of the number of aliens, 
            // uncontrolled, and controlled townspeople.
            IMapItems controlled = new MapItems();
            IMapItems uncontrolled = new MapItems();
            IMapItems aliens = new MapItems();
            IMapItems unknownAliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsCombatThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
                  continue;
               if (true == mi.IsControlled)
                  controlled.Add(mi);
               else if (true == mi.IsAlienKnown)
                  aliens.Add(mi);
               else if (true == mi.IsAlienUnknown)
                  unknownAliens.Add(mi);
               else
                  uncontrolled.Add(mi);
            }
            // Based on counts, determine if a battle is possible.
            // !!!!!REMEMBER!!!!!  unknown aliens will not trigger a combat against wary people
            // unless the MapItem is exposed.  
            ITerritory combatTerritory = null;
            if (true == IsAlien)
            {
               if (0 == aliens.Count)
                  continue;
               combatTerritory = aliens[0].TerritoryCurrent;

               if ((0 == controlled.Count) && (0 == uncontrolled.Count)) // If nobody to attack, skip
                  continue;

               if (0 == controlled.Count) // Alien can only attack uncontrolled counters that are wary
               {
                  bool isAnyMapItemsWary = false;
                  foreach (IMapItem mi1 in uncontrolled)
                  {
                     if (true == mi1.IsWary)
                        isAnyMapItemsWary = true;
                  }
                  if (false == isAnyMapItemsWary)
                     continue;
               }
            }
            else // Townspeople attacks
            {
               if (0 == controlled.Count)
                  continue;
               combatTerritory = controlled[0].TerritoryCurrent;
               if ((0 == aliens.Count) && ((0 == uncontrolled.Count) && (0 == unknownAliens.Count)))
                  continue;
            }

            if (true == IsAlien)
               myTerritoriesCombatForAlien.Add(combatTerritory);
            else
               myTerritoriesCombatForTownsperson.Add(combatTerritory);

            // Turn the region red

            String targetName = combatTerritory.Name + combatTerritory.Sector.ToString();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Polygon)
               {
                  Polygon p1 = (Polygon)ui;
                  if (p1.Name == targetName)
                  {
                     p1.Fill = mySolidColorBrushRed;
                     Canvas.SetZIndex(p1, 1000);
                     break;
                  }
               }
            }

            // Perform animiation on the region

            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;

            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)

         if (0 == myStoryboard.Children.Count)
         {
            if (true == gi.MapItemCombat.IsAnyRetreat) // If the previous combat had retreats, do not assume combats are completed
               return true;                           // until the player explicitly indicates it with menu command.  This allows them
                                                      // to see the retreats.
            return false;
         }
         myStoryboard.Begin(this);
         return true;
      }
      private void DisplayCombat(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();

         // If passed-in territory is not null, user has selected this region.
         // Show a dialog of the conversation results.

         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayTakover() selectedTerritory=null");
            return;
         }

         // Only handle this mouse click if the selected territory is one where combat
         // can occur.

         if (true == IsAlien)
         {
            if (null == myTerritoriesCombatForAlien.Find(selectedTerritory.ToString()))
               return;
         }
         else
         {
            if (null == myTerritoriesCombatForTownsperson.Find(selectedTerritory.ToString()))
               return;
         }

         IMapItems aliens = new MapItems();
         IMapItems controlled = new MapItems();
         IMapItems uncontrolled = new MapItems();
         IMapItems wary = new MapItems();

         foreach (MapItem mi in gi.Persons)
         {
            if ((selectedTerritory.Name == mi.TerritoryCurrent.Name) && (selectedTerritory.Sector == mi.TerritoryCurrent.Sector))
            {
               if ((false == mi.IsConscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || (true == mi.IsSurrendered))
                  continue;

               if (true == mi.IsAlienKnown)
               {
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
            if ((0 == aliens.Count) || (0 == wary.Count))
               return;
         }

         // Setup the action pane.

         if (true == IsAlien)
         {
            foreach (IMapItem mi in aliens)
               myLeftMapItemsInActionPanel.Add(mi);

            foreach (IMapItem mi in controlled)
               myRightMapItemsInActionPanel.Add(mi);

            if (0 == myRightMapItemsInActionPanel.Count)
            {
               foreach (IMapItem mi in wary)
                  myRightMapItemsInActionPanel.Add(mi);
            }
         }
         else
         {
            foreach (IMapItem mi in controlled)
               myLeftMapItemsInActionPanel.Add(mi);

            foreach (IMapItem mi in aliens)
               myRightMapItemsInActionPanel.Add(mi);

            if (0 == myRightMapItemsInActionPanel.Count)
            {
               foreach (IMapItem mi in uncontrolled)
                  myRightMapItemsInActionPanel.Add(mi);
            }
         }

         if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
         {
            UpdateActionPanel(gi, true);

            myLabelHeading.Visibility = Visibility.Visible;
            myLabelArrow.Visibility = Visibility.Visible;
            myTextBoxResults.Visibility = Visibility.Visible;
            myLabelLeftTop.Visibility = Visibility.Visible;
            myLabelRightTop.Visibility = Visibility.Visible;

            myLabelHeading.Content = "Combat... \"Let's Rumble!!!\"";
            myLabelLeftTop.Content = "All of these are attacking:";
            myLabelRightTop.Content = "All of these are defending:";
         }

      }
      private void PerformCombat(IGameInstance gi, bool isIgnoreResults)
      {
         if (true == isIgnoreResults)
         {
            ClearActionPanel();
            return;
         }

         if ((0 == myLeftMapItemsInActionPanel.Count) || (0 == myRightMapItemsInActionPanel.Count))
         {
            StringBuilder sb = new StringBuilder("PerformInfluence(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }

         // Only initiate combat if there is not an outstanding combat happening.

         if ((false == myIsCombatInitiatedForAlien) && (false == myIsCombatInitiatedForTownsperson))
         {
            IMapItem leftMapItem = gi.Persons.Find(myLeftMapItemsInActionPanel[0].Name);
            gi.MapItemCombat.Territory = leftMapItem.TerritoryCurrent;
            if (true == IsAlien)
            {
               Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "MouseLeftButtonDownCanvas():Combat: ALIEN myIsCombatInitiatedForAlien=true");
               myIsCombatInitiatedForAlien = true;
               GameAction outAction = GameAction.AlienInitiateCombat;
               myGameEngine.PerformAction(ref gi, ref outAction);
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "MouseLeftButtonDownCanvas():Combat TP    myIsCombatInitiatedForTownsperson=true");
               myIsCombatInitiatedForTownsperson = true;
               GameAction outAction = GameAction.TownspersonInitiateCombat;
               myGameEngine.PerformAction(ref gi, ref outAction);
            }
            return;
         }

      }
      private void DisplayCombatResults(IGameInstance gi)
      {
         ClearActionPanel();

         IMapItemCombat battle = gi.MapItemCombat;
         if (null == battle)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayCombatResults() battle=null");
            return;
         }

         int totalCombatForAttacker = 0;
         int numAttackers = 0;
         IMapItems leftMapItems = new MapItems();
         foreach (IMapItem mi in battle.Attackers)
            leftMapItems.Add(mi);
         leftMapItems = leftMapItems.SortOnCombat();

         foreach (IMapItem mi in leftMapItems)
         {
            myLeftMapItemsInActionPanel.Add(mi);
            myLeftMapItemsInActionPanelSelected.Add(mi);
            totalCombatForAttacker += mi.Combat;
            if (3 <= ++numAttackers)
               break;
         }

         int totalCombatForDefender = 0;
         int numDefenders = 0;
         IMapItems rightMapItems = new MapItems();
         foreach (IMapItem mi in battle.Defenders)
            rightMapItems.Add(mi);
         rightMapItems = rightMapItems.SortOnCombat();
         foreach (IMapItem mi in rightMapItems)
         {
            myRightMapItemsInActionPanel.Add(mi);
            myRightMapItemsInActionPanelSelected.Add(mi);
            totalCombatForDefender += mi.Combat;
            if (3 <= ++numDefenders)
               break;
         }

         if ((0 == myLeftMapItemsInActionPanel.Count) || (0 == myRightMapItemsInActionPanel.Count))
         {
            StringBuilder sb = new StringBuilder("DisplayCombatResults(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }

         UpdateActionPanel(gi, true);

         myLabelHeading.Visibility = Visibility.Visible;
         myLabelArrow.Visibility = Visibility.Visible;

         Logger.Log(LogEnum.LE_SHOW_COMBAT_THREAD, "DisplayCombatResults() myTextBoxResults.Visibility = Visibility.Visible");

         myLabelHeading.Content = "Combat Results";
         myLabelLeftTop.Content = "Attackers:";
         myLabelRightTop.Content = "Defenders:";

         StringBuilder displayResults = new StringBuilder();
         displayResults.Append("Total Attacker Combat Factors=");
         displayResults.Append(totalCombatForAttacker.ToString());
         displayResults.Append("\nTotal Defender Combat Factors=");
         displayResults.Append(totalCombatForDefender.ToString());

         int differenceInCombat = totalCombatForAttacker - totalCombatForDefender;
         displayResults.Append("\nDifference: ");
         displayResults.Append(differenceInCombat.ToString());

         int die1 = battle.DieRoll1;
         int die2 = battle.DieRoll2;
         displayResults.Append("\nRoll: ");
         displayResults.Append(die1.ToString());
         displayResults.Append(" + ");
         displayResults.Append(die2.ToString());
         displayResults.Append(" => ");
         displayResults.Append(battle.Result.ToString());

         Logger.Log(LogEnum.LE_SHOW_COMBAT_THREAD, "DisplayCombatResults() myTextBoxResults.Text=displayResults");
         myTextBoxResults.Text = displayResults.ToString();
         myLabelLeftTop.Visibility = Visibility.Visible;
         myLabelRightTop.Visibility = Visibility.Visible;
         myTextBoxResults.Visibility = Visibility.Visible;
         Logger.Log(LogEnum.LE_SHOW_COMBAT_THREAD, "DisplayCombatResults() myTextBoxResults.Text=" + myTextBoxResults.Text);
         UpdateActionPanelButtons(gi);
         Logger.Log(LogEnum.LE_SHOW_COMBAT_THREAD, "DisplayCombatResults() myTextBoxResults.Text=" + myTextBoxResults.Text);
      }
      private void PerformCombatRetreat(IGameInstance gi, bool isIgnoreResults)
      {
         if (null != gi.MapItemCombat.Territory)
            UpdateViewMovement(gi); // Show retreats
         UpdateViewState(gi);
         myIsCombatInitiatedForTownsperson = false;
         StringBuilder sb1 = new StringBuilder("UpdateView():TownspersonPerformCombat: "); sb1.Append(IsAlien.ToString()); sb1.Append("myIsCombatInitiatedForTownsperson=false");
         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb1.ToString());

         if (true == isIgnoreResults)
            ClearActionPanel();
      }
      private bool DisplayIterogations(IGameInstance gi)
      {
         myStoryboard = new Storyboard();
         foreach (UIElement ui in myCanvas.Children) // Clear any previous flashing regions
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               ITerritory t = gi.ZebulonTerritories.Find(p1.Tag.ToString());
               if (null == t)
                  p1.Fill = mySolidColorBrushClear;
               else
                  p1.Fill = mySolidColorBrushBlack;
            }
         }
         //--------------------------------------------------------------
         foreach (Stack stack in gi.Stacks) // Display flashing regions where conversations can happen. Iterate through the stacks looking for multiple counters per stack.
         {
            if (stack.MapItems.Count < 2)
               continue;
            // In each stack, get the count in the stack of the number of aliens and controlled townspeople
            IMapItems townspeopleControlled = new MapItems();
            IMapItems surrenderedAliens = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if ((true == mi.IsInterrogatedThisTurn) || (true == mi.IsInterrogated) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsStunned))
                  continue;

               if (true == mi.IsControlled)
               {
                  if (false == mi.IsTiedUp)
                     townspeopleControlled.Add(mi);
               }
               else
               {
                  if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsSurrendered) || (true == mi.IsTiedUp)))
                     surrenderedAliens.Add(mi);
               }
            }
            if ((0 == townspeopleControlled.Count) || (0 == surrenderedAliens.Count))
               continue;
            //--------------------------------------------------------------
            String targetName = townspeopleControlled[0].TerritoryCurrent.Name + townspeopleControlled[0].TerritoryCurrent.Sector.ToString();
            foreach (UIElement ui in myCanvas.Children) // Turn the region red
            {
               if (ui is Polygon)
               {
                  Polygon p1 = (Polygon)ui;
                  if (p1.Name == targetName)
                  {
                     p1.Fill = mySolidColorBrushPurple;
                     Canvas.SetZIndex(p1, 1000);
                     break;
                  }
               }
            }
            //--------------------------------------------------------------
            DoubleAnimation anim = new DoubleAnimation(); // Perform animiation on the region
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         } // end foreach (Stack stack in stacks)
           //--------------------------------------------------------------
         if (0 < myStoryboard.Children.Count)
            myStoryboard.Begin(this);
         if (0 < gi.NumIterogationsThisTurn)
            return true;
         return false;
      }
      private bool DisplayImplantRemovals(IGameInstance gi)
      {
         myStoryboard = new Storyboard();
         foreach (UIElement ui in myCanvas.Children) // Clear any previous flashing regions
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               p1.Fill = mySolidColorBrushClear;
            }
         }
         //-------------------------------------------------------------- 
         foreach (Stack stack in gi.Stacks) // Display flashing regions where conversations can happen. Iterate through the stacks looking for multiple counters per stack.
         {
            if (stack.MapItems.Count < 2)
               continue;
            // In each stack, get the count in the stack of the number of aliens 
            // and controlled townspeople
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
            if ((0 == controlled.Count) || (0 == aliens.Count))
               continue;
            //-------------------------------------------------------------- 
            String targetName = controlled[0].TerritoryCurrent.Name + controlled[0].TerritoryCurrent.Sector.ToString();  // Turn the region red
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Polygon)
               {
                  Polygon p1 = (Polygon)ui;
                  if (p1.Name == targetName)
                  {
                     p1.Fill = mySolidColorBrushRosyBrown;
                     Canvas.SetZIndex(p1, 1000);
                     break;
                  }
               }
            }
            //-------------------------------------------------------------- 
            DoubleAnimation anim = new DoubleAnimation(); // Perform animiation on the region
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         } 
         //-------------------------------------------------------------- 
         if (0 == myStoryboard.Children.Count)
            return false;
         myStoryboard.Begin(this);
         return true;
      }
      private void DisplayImplantRemoval(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();

         
         // Show a dialog of the conversation results.

         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayImplantRemoval() selectedTerritory=null");
            return;
         }
         IStack? stack = gi.Stacks.Find(selectedTerritory);
         if( null == stack )
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayImplantRemoval() stack=null");
            return;
         }
         if (null != stack.MapItems)
         {
            myLeftMapItemsInActionPanel.Clear();
            myRightMapItemsInActionPanel.Clear();
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsImplantRemovalThisTurn) || (true == mi.IsKilled))
                  continue;

               if ((true == mi.IsControlled) && (true == mi.IsConscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  myLeftMapItemsInActionPanel.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsConscious)))
                  myRightMapItemsInActionPanel.Add(mi);
            }

            if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
            {
               UpdateActionPanel(gi, !IsAlien);
               myLabelHeading.Visibility = Visibility.Visible;
               myLabelArrow.Visibility = Visibility.Visible;
               myTextBoxResults.Visibility = Visibility.Visible;
               myLabelHeading.Content = "Remove Implant to Hold Evidence of Alien Takeover";
               myLabelLeftTop.Content = "Choose a person who is removing implant:";
               myLabelRightTop.Content = "Choose a person to have implant removed:";
            }
         }
      }
      private void PerformImplantRemoval(IGameInstance gi, bool isIgnoreResults)
      {
         if ((0 == myLeftMapItemsInActionPanelSelected.Count) || (0 == myRightMapItemsInActionPanelSelected.Count))
         {
            StringBuilder sb = new StringBuilder("PerformImplantRemoval(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }

         IMapItem leftMapItem = gi.Persons.Find(myLeftMapItemsInActionPanelSelected[0].Name);
         leftMapItem.IsImplantRemovalThisTurn = true;

         IMapItem rightMapItem = gi.Persons.Find(myRightMapItemsInActionPanelSelected[0].Name);
         rightMapItem.IsImplantRemovalThisTurn = true;

         if (false == isIgnoreResults)
         {
            int die1 = Utilities.RandomGenerator.Next(6) + 1;
            int die2 = Utilities.RandomGenerator.Next(6) + 1;
            int sum = die1 + die2;

            StringBuilder displayResults = new StringBuilder("Roll: ");
            displayResults.Append(die1.ToString());
            displayResults.Append(" + ");
            displayResults.Append(die2.ToString());
            displayResults.Append(" = ");
            displayResults.Append(sum.ToString());

            switch (sum)
            {
               case 2: // Implant Explodes
               case 3:
                  displayResults.Append("\nImplant Explodes!!");
                  rightMapItem.IsKilled = true;           // Kill the townsperson counter
                  leftMapItem.IsKilled = true;                       // Kill the Alien counter
                  break;

               case 4: // Implant is too tighly attached
               case 5:
               case 6:
                  displayResults.Append("\nImplant is too tighly attached. Try again next turn.");
                  break;

               case 7: // Implant is removed but disintegrates
               case 8:
               case 9:
               case 10:
                  displayResults.Append("\nImplant is removed but disintegrates.");
                  if (false == gi.AddTownperson(rightMapItem))
                     Logger.Log(LogEnum.LE_ERROR, "CheckForImplantRemoval() returned error");
                  break;

               case 11: // Implant usuable
               case 12:
                  displayResults.Append("\nImplant is removed intact! You now have evidence.");
                  if (false == gi.AddTownperson(rightMapItem))
                     Logger.Log(LogEnum.LE_ERROR, "CheckForImplantRemoval() returned error");
                  leftMapItem.IsImplantHeld = true;
                  break;

               default:
                  break;
            }

            myTextBoxResults.Text = displayResults.ToString();
         }

         if (true == isIgnoreResults)
            ClearActionPanel();
         else
            UpdateActionPanelButtons(gi);
      }
      private bool DisplayTakeovers(IGameInstance gi, ITerritory? selectedTerritory = null)
      {

         if (false == IsAlien)
         {
            myStoryboard = null; // turn off any flashing spaces
            return false;
         }

         //----------------------------------------------------------------------
         // Clear any previous flashing regions

         myStoryboard = new Storyboard();

         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               p1.Fill = mySolidColorBrushClear;
            }
         }

         //----------------------------------------------------------------------
         // Display flashing regions where takovers can happen.
         // Iterate through the stacks looking for multiple counters per stack.;
         foreach (Stack stack in gi.Stacks)
         {
            if (stack.MapItems.Count < 2)
               continue;
            // In each stack, get the count in the stack of the number of aliens 
            // and controlled townspeople
            IMapItems townspeopleControlled = new MapItems();
            IMapItems possibleVictums = new MapItems();
            IMapItems knownAliens = new MapItems();
            IMapItems unknownAliens = new MapItems();
            IMapItems uncontrolled = new MapItems();
            foreach (MapItem mi in stack.MapItems)
            {
               if (stack.MapItems.Count < 2)
                  continue;
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
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp)) // stunned or tied-up aliens cannot takeover
                        knownAliens.Add(mi);
                  }
                  else if (true == mi.IsAlienUnknown)
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp)) // stunned or tied-up aliens cannot takeover
                        unknownAliens.Add(mi);
                  }
                  else // uncontrolled 
                  {
                     possibleVictums.Add(mi);
                  }
               }
            }
            int countOfUnknown = possibleVictums.Count + unknownAliens.Count;
            int countOfAliens = knownAliens.Count + unknownAliens.Count;
            if (1 < countOfUnknown) // If any stack has two or more counters that are not controlled, return true       
               myIsTakeOverPromptNeededToFoolOpponent = true;
            if ((1 == countOfUnknown) && (0 < knownAliens.Count)) // If any stack has at least one possible victum with a known alien, return true   
               myIsTakeOverPromptNeededToFoolOpponent = true;
            if ((0 == possibleVictums.Count) || (0 == countOfAliens))
               continue;
            myIsTakeOverInOneRegion = true;
            // Turn the region orange
            String targetName = possibleVictums[0].TerritoryCurrent.Name + possibleVictums[0].TerritoryCurrent.Sector.ToString();
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Polygon)
               {
                  Polygon p1 = (Polygon)ui;
                  if (p1.Name == targetName)
                  {
                     p1.Fill = mySolidColorBrushOrange;
                     Canvas.SetZIndex(p1, 1000);
                     break;
                  }
               }
            }

            // Perform animiation on the region

            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;

            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)

         if (0 == myStoryboard.Children.Count)
         {
            if ((true == myIsTakeOverPromptNeededToFoolOpponent) && (false == myIsTakeOverInOneRegion))
               return true;
            else
               return false;
         }

         myStoryboard.Begin(this);
         return true;
      }
      private void DisplayTakover(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();
         // If passed-in territory is not null, user has selected this region.
         // Show a dialog of the conversation results.
         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayTakover() selectedTerritory=null");
            return;
         }
         gi.Takeover = null;
         IStack? stack = gi.Stacks.Find(selectedTerritory);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayImplantRemoval() stack=null");
            return;
         }
         if (null != stack.MapItems)
         {
            myLeftMapItemsInActionPanel.Clear();
            myRightMapItemsInActionPanel.Clear();

            foreach (IMapItem mi in stack.MapItems)
            {
               if (stack.MapItems.Count < 2)
                  continue;

               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (false == mi.IsConscious) || (true == mi.IsSurrendered) || ("Zebulon" == mi.Name))
                  continue;

               if ((true == mi.IsControlled) || (true == mi.IsWary))
               {
                  if ((true == mi.IsStunned) || (true == mi.IsTiedUp))
                     myRightMapItemsInActionPanel.Add(mi);
               }
               else
               {
                  if ((true == mi.IsAlienKnown) || (true == mi.IsAlienUnknown))
                  {
                     if ((false == mi.IsStunned) && (false == mi.IsTiedUp))
                        myLeftMapItemsInActionPanel.Add(mi);
                  }
                  else // uncontrolled
                  {
                     myRightMapItemsInActionPanel.Add(mi);
                  }
               }
            }
            if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
            {
               UpdateActionPanel(gi, IsAlien);
               myLabelHeading.Visibility = Visibility.Visible;
               myLabelArrow.Visibility = Visibility.Visible;
               myTextBoxResults.Visibility = Visibility.Visible;
               myLabelHeading.Content = "Takeover... \"You will be assimulated.\"";
               myLabelLeftTop.Content = "Choose an alien who is assimulating:";
               myLabelRightTop.Content = "Choose a person being assimulated:";
            }
         }
      }
      private void PerformTakeover(IGameInstance gi, bool isIgnoreResults)
      {
         if ((0 == myLeftMapItemsInActionPanelSelected.Count) || (0 == myRightMapItemsInActionPanelSelected.Count))
         {
            StringBuilder sb = new StringBuilder("PerformTakeover(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }

         IMapItem alien = gi.Persons.Find(myLeftMapItemsInActionPanelSelected[0].Name);
         alien.IsTakeoverThisTurn = true;

         IMapItem victum = gi.Persons.Find(myRightMapItemsInActionPanelSelected[0].Name);
         victum.IsTakeoverThisTurn = true;

         if (false == isIgnoreResults)
            gi.Takeover = new MapItemTakeover(alien, victum);
         GameAction outAction = GameAction.AlienTakeover;
         myGameEngine.PerformAction(ref gi, ref outAction);
         if (true == isIgnoreResults)
            ClearActionPanel();
      }
      private void PerformTakeoverObserved(IGameInstance gi)
      {
         ClearActionPanel();
         myLeftMapItemsInActionPanel.Add(gi.Takeover.Alien);
         myRightMapItemsInActionPanel.Add(gi.Takeover.Uncontrolled);
         UpdateActionPanel(gi, !IsAlien);

         myLabelHeading.Visibility = Visibility.Visible;
         myLabelArrow.Visibility = Visibility.Visible;
         myTextBoxResults.Visibility = Visibility.Visible;

         myLabelHeading.Content = "Takover... \"You will be assimulated.\"";
         myLabelLeftTop.Content = "Alien who is assimulating:";
         myLabelRightTop.Content = "Person being assimulated:";

         myTextBoxResults.Text = gi.Takeover.Observations;
         UpdateActionPanelButtons(gi);
      }
      //-------------CONTROLLER FUNCTIONS---------------------------------
      private void myTextBoxEntryTextChanged(object sender, TextChangedEventArgs e)
      {
         if (null != myGameEngine)
         {
            string entry = myTextBoxEntry.Text;  // Do not do anything unless a carriage return happens
            int length = entry.Count();
            if (0 == length)
               return;

            if ('\n' == entry[length - 1])
            {
               myTextBoxEntry.Text = "";
               StringBuilder sb = new StringBuilder("You say: ");
               sb.Append(entry);
               myTextBoxDisplay.AppendText(sb.ToString());
               myTextBoxDisplay.ScrollToEnd();
               //myGameEngine.SendText(entry);
            }
         }
      }
      private void ClickMapItem(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         if (sender is Button)
         {
            Button selectedButton = (Button)sender;
            IMapItem selectedMapItem = gi.Persons.Find(selectedButton.Name);
            if (null == selectedMapItem)
            {
               Console.WriteLine("Selected Map Item {0} no longer found", selectedButton.Name);
               return;
            }

            if (true == selectedMapItem.IsKilled)  // If killed, do nothing
            {
               this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
               return;
            }

            switch (gi.GamePhase)
            {
               case GamePhase.AlienMovement:
                  if (null == myMovingButton)
                  {
                     if (false == IsAlien)
                     {
                        this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                        return;  // do nothing
                     }
                     if (4 < myMovingMapItems.Count)
                     {
                        this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                        if (true == myIsFlagSetForAlienMoveCountExceeded)
                           MessageBox.Show("Alien only allowed to move five people");
                        else
                           myIsFlagSetForAlienMoveCountExceeded = true;
                        return;  // do nothing
                     }
                     if (("Zebulon" == selectedMapItem.Name) || ("Alien Performs Movement" != gi.NextAction)
                         || (false == selectedMapItem.IsConscious) || (true == selectedMapItem.IsControlled) || (true == selectedMapItem.IsKilled)
                         || (true == selectedMapItem.IsSurrendered) || (true == selectedMapItem.IsStunned) || (true == selectedMapItem.IsTiedUp) || (true == selectedMapItem.IsWary))
                     {
                        this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                        return;  // do nothing
                     }
                  }
                  break;
               case GamePhase.TownspersonMovement:
                  if (null == myMovingButton)
                  {
                     if ((("Townsperson Selects Counter to Move" != gi.NextAction) || (true == IsAlien)
                         || (false == selectedMapItem.IsConscious) || (false == selectedMapItem.IsControlled) || (true == selectedMapItem.IsKilled)
                         || (true == selectedMapItem.IsStunned) || (true == selectedMapItem.IsTiedUp) || (true == myIsAlienAbleToStopMove)))
                     {
                        this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                        return;  // do nothing
                     }
                  }
                  break;

               default:
                  MapItemCommonAction(selectedMapItem.TerritoryCurrent);
                  return;
            }

            // There is already a moving button.  Do not do any actions until
            // the alien player responds or there is a timeout on the alien response.
            // When that happens, myIsAlienAbleToStopMove=false.

            if (true == myIsAlienAbleToStopMove)
               return;

            // This section of code only applies if attempting to move a MapItem, i.e.,
            // either the AlienMovement Phase or the TownspersonMOvementPhase.

            if (null == myMovingButton)
            {
               if (true == selectedMapItem.IsMoveStoppedThisTurn)
               {
                  MessageBox.Show("Not allowed to Move This Turn");
                  this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                  return;
               }
               if (selectedMapItem.Movement <= selectedMapItem.MovementUsed)
               {

                  if (true == myIsFlagSetForMaxMove)
                  {
                     MessageBox.Show("Already Reached Maximum Movement");
                     myIsFlagSetForMaxMove = false;
                  }
                  myIsFlagSetForMaxMove = true;
                  this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                  return;
               }

               myRectangleSelection.BeginAnimation(Canvas.LeftProperty, null);
               myRectangleSelection.BeginAnimation(Canvas.TopProperty, null);
               Canvas.SetLeft(myRectangleSelection, selectedMapItem.Location.X);
               Canvas.SetTop(myRectangleSelection, selectedMapItem.Location.Y);
               myRectangleSelection.Visibility = Visibility.Visible;             // highlight the moving button with a rectangle
               myMovingButton = selectedButton;
            }
            else
            {
               //*******************************************************
               // case: MapItem already selected to move.  

               if (selectedButton.Name == myMovingButton.Name) // clicking the moving button again causes it to be unhighlighted
               {
                  myMovingButton = null;
                  myRectangleSelection.Visibility = Visibility.Hidden;
               }
               else
               {
                  MapItemMoveManually(selectedMapItem.TerritoryCurrent, myMovingButton);
               }
            }
         }
      }
      private void MouseLeftButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         Point p = e.GetPosition(myCanvas);  // not used but useful info

         // There is already a moving button.  Do not do any actions until
         // the alien player responds or there is a timeout on the alien response.
         // When that happens, myIsAlienAbleToStopMove=false.

         if (true == myIsAlienAbleToStopMove)
            return;

         #region Get the selected territory
         //--------------------------------------------------
         // Get the selected territory

         ITerritory selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
            {
               Polygon aPolygon = (Polygon)ui;
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territories.theTerritories)
                  {
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }

            } // end if (ui is Polygon)

            if (null != selectedTerritory)
               break;

         }  // end foreach (UIElement ui in myCanvas.Children)

         if (null == selectedTerritory)  // If no territory is selected, return
            return;

         #endregion

         #region switch(gi.GamePhase)
         switch (gi.GamePhase)
         {
            #region GamePhase.AlienMovement
            case GamePhase.AlienMovement:
               if ((true == IsAlien) && (null != myMovingButton))
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               else
                  RotateStack(selectedTerritory);
               break;
            #endregion

            #region GamePhase.TownspersonMovement
            case GamePhase.TownspersonMovement:
               if ((false == IsAlien) && (null != myMovingButton))
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               else
                  RotateStack(selectedTerritory);
               break;
            #endregion

            default:
               MapItemCommonAction(selectedTerritory);
               break;
         }
         #endregion

      }
      private void MouseRightButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {

         Point p = e.GetPosition(myCanvas);  // not used but useful info

         #region Get the selected territory
         //--------------------------------------------------
         // Get the selected territory

         ITerritory selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
            {
               Polygon aPolygon = (Polygon)ui;
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territories.theTerritories)
                  {
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }

            } // end if (ui is Polygon)

            if (null != selectedTerritory)
               break;

         }  // end foreach (UIElement ui in myCanvas.Children)

         if (null == selectedTerritory)  // If no territory is selected, return
            return;

         #endregion

         this.RotateStack(selectedTerritory);

      }
      private void MouseDoubleClickMapItem(object sender, RoutedEventArgs e)
      {
         // There is already a moving button.  Do not do any actions until
         // the alien player responds or there is a timeout on the alien response.
         // When that happens, myIsAlienAbleToStopMove=false.
         if (true == myIsAlienAbleToStopMove)
            return;
         if (sender is Button)
         {
            Button selectedButton = (Button)sender;
            MapItemReturnToStart(selectedButton);
         }
      }
      private void ContextMenuLoaded(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         if (sender is ContextMenu)
         {
            ContextMenu cm = (ContextMenu)sender;
            // Gray out all menu items as default
            for (int i = 0; i < cm.Items.Count; ++i)
            {
               if (cm.Items[i] is MenuItem)
               {
                  MenuItem menuItem = (MenuItem)cm.Items[i];
                  menuItem.IsEnabled = false;
               }
            }
            if (cm.PlacementTarget is Button)
            {
               Button b = (Button)cm.PlacementTarget;
               IMapItem mi = gi.Persons.Find(b.Name);
               // Gray out the "Retun to Starting Point" menu item
               if ((0 < cm.Items.Count) && (true == mi.IsMoveAllowedToResetThisTurn))
               {
                  if (cm.Items[0] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[0];
                     if ((true == IsAlien) && (GamePhase.AlienMovement == gi.GamePhase) && (true == mi.IsMoved))
                        menuItem.IsEnabled = true;
                     else if ((false == IsAlien) && (GamePhase.TownspersonMovement == gi.GamePhase) && (true == mi.IsMoved))
                        menuItem.IsEnabled = true;
                  }
               }
               // Gray out the "Rotate Stack" menu item
               //if (1 < cm.Items.Count)
               //{
               //   if (cm.Items[1] is MenuItem)
               //   {
               //      MenuItem menuItem = (MenuItem)cm.Items[1];
               //      List<Stack> stacks = new List<Stack>();
               //      stacks.AssignPeople(gi.Persons, IsAlien);
               //      IMapItems mapItems = stacks.FindPeople(mi.TerritoryCurrent);
               //      if (null != mapItems)
               //      {
               //         if (1 < mapItems.Count)
               //            menuItem.IsEnabled = true;
               //      }
               //   }
               //}
               // Gray out the "Expose" menu item
               if (2 < cm.Items.Count)
               {
                  if (cm.Items[2] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[2];
                     if ((true == mi.IsAlienUnknown) && (false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
                        menuItem.IsEnabled = true;
                  }
               }
               // Gray out the "Stop Movement" menu item
               if (3 < cm.Items.Count)
               {
                  if (cm.Items[3] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[3];
                     menuItem.IsEnabled = IsAlienAbleToStopMove(gi, mi);
                  }
               }
            }
         }
      }
      private void ContextMenuClickReturnToStart(object sender, RoutedEventArgs e)
      {
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  MapItemReturnToStart(b);
               }
            }
         }
      }
      private void ContextMenuClickRotate(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  IMapItem selectedMapItem = gi.Persons.Find(b.Name);
                  if (null == selectedMapItem)
                  {
                     Console.WriteLine("GameViewerWindow::MouseDoubleClickMapItem() selectedMapItem = null for {0}", b.Name);
                     return;
                  }
                  this.RotateStack(selectedMapItem.TerritoryCurrent);
               }
            }
         }
      }
      private void ContextMenuClickExposeAlien(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  IMapItem selectedMapItem = gi.Persons.Find(b.Name);
                  if (null == selectedMapItem)
                  {
                     Console.WriteLine("GameViewerWindow::MouseDoubleClickMapItem() selectedMapItem = null for {0}", b.Name);
                     return;
                  }
                  if (true == selectedMapItem.IsAlienUnknown)
                  {
                     if (false == gi.AddKnownAlien(selectedMapItem))
                        Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickExposeAlien(): returned error");
                     GameAction outAction = GameAction.ShowAlien;
                     myGameEngine.PerformAction(ref gi, ref outAction); // Inform the user to return back
                  }
               }
            }
         }
      }
      private void ContextMenuClickStopMove(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         myTimer.Stop();
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  IMapItem selectedMapItem = gi.Persons.Find(b.Name);
                  if (null != selectedMapItem)
                  {
                     if (((true == selectedMapItem.IsAlienUnknown) || (true == selectedMapItem.IsAlienKnown)) && (true == myIsAlienAbleToStopMove) && (false == selectedMapItem.IsMoveStoppedThisTurn))
                     {
                        if (false == gi.AddKnownAlien(selectedMapItem))
                           Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove(): returned error");
                        selectedMapItem.IsMoveStoppedThisTurn = true;

                        // Reset the moving MapItem

                        if (0 < gi.MapItemMoves.Count)
                        {
                           IMapItemMove mim = gi.MapItemMoves[0];
                           IMapItem movingMi = gi.Persons.Find(mim.MapItem.Name);
                           movingMi.TerritoryCurrent = movingMi.TerritoryStarting;
                           movingMi.IsMoveStoppedThisTurn = true;
                           movingMi.MovementUsed -= mim.BestPath.Territories.Count;
                           if (movingMi.MovementUsed <= 0)
                           {
                              movingMi.MovementUsed = 0;
                              movingMi.IsMoved = false;
                           }

                           // Change to modified MapItemMove

                           IMapItemMove modifiedMove = new MapItemMove(Territories.theTerritories, movingMi, selectedMapItem.TerritoryCurrent);
                           gi.MapItemMoves[0] = modifiedMove;
                           movingMi.MovementUsed = movingMi.Movement; // ensure cannot move further
                           GameAction outAction = GameAction.AlienModifiesTownspersonMovement;
                           myGameEngine.PerformAction(ref gi, ref outAction);
                           UpdateCanvas(gi, true);
                        }
                     }
                  }
               }
            }
         }
      }
      private void TimerElasped(object sender, EventArgs e)
      {
         IGameInstance gi = myGameInstance;
         Logger.Log(LogEnum.LE_TIMER_ELAPED, "TimerElasped() called");
         if (true == myIsAlienAbleToStopMove)
         {
            myIsAlienAbleToStopMove = false;
            Logger.Log(LogEnum.LE_TIMER_ELAPED, "TimerElasped():  Reset State myIsAlienAbleToStopMove=false");
            myTimer.Stop();
            GameAction outAction = GameAction.AlienTimeoutOnMovement;
            myGameEngine.PerformAction(ref gi, ref outAction);
         }
      }
      private void myButton1_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle2.Visibility = Visibility.Hidden;
               myRectangle3.Visibility = Visibility.Hidden;
               myLeftMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myLeftMapItemsInActionPanel[0];
         if (Visibility.Hidden == myRectangle1.Visibility) // if selected, deselect it
         {
            myRectangle1.Visibility = Visibility.Visible;
            myLeftMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle1.Visibility = Visibility.Hidden;
            myLeftMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton2_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle1.Visibility = Visibility.Hidden;
               myRectangle3.Visibility = Visibility.Hidden;
               myLeftMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myLeftMapItemsInActionPanel[1];
         if (Visibility.Hidden == myRectangle2.Visibility) // if selected, deselect it
         {
            myRectangle2.Visibility = Visibility.Visible;
            myLeftMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle2.Visibility = Visibility.Hidden;
            myLeftMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton3_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle1.Visibility = Visibility.Hidden;
               myRectangle2.Visibility = Visibility.Hidden;
               myLeftMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myLeftMapItemsInActionPanel[2];
         if (Visibility.Hidden == myRectangle3.Visibility) // if selected, deselect it
         {
            myRectangle3.Visibility = Visibility.Visible;
            myLeftMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle3.Visibility = Visibility.Hidden;
            myLeftMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton4_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle5.Visibility = Visibility.Hidden;
               myRectangle6.Visibility = Visibility.Hidden;
               myRightMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myRightMapItemsInActionPanel[0];
         if (Visibility.Hidden == myRectangle4.Visibility) // if selected, deselect it
         {
            myRectangle4.Visibility = Visibility.Visible;
            myRightMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle4.Visibility = Visibility.Hidden;
            myRightMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton5_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle4.Visibility = Visibility.Hidden;
               myRectangle6.Visibility = Visibility.Hidden;
               myRightMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myRightMapItemsInActionPanel[1];
         if (Visibility.Hidden == myRectangle5.Visibility) // if selected, deselect it
         {
            myRectangle5.Visibility = Visibility.Visible;
            myRightMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle5.Visibility = Visibility.Hidden;
            myRightMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton6_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
            case GamePhase.ImplantRemoval:
            case GamePhase.AlienTakeover:
               myRectangle4.Visibility = Visibility.Hidden;
               myRectangle5.Visibility = Visibility.Hidden;
               myRightMapItemsInActionPanelSelected.Clear();
               break;
            default:
               break;
         }

         IMapItem mi = myRightMapItemsInActionPanel[2];
         if (Visibility.Hidden == myRectangle6.Visibility) // if selected, deselect it
         {
            myRectangle6.Visibility = Visibility.Visible;
            myRightMapItemsInActionPanelSelected.Add(mi);
         }
         else
         {
            myRectangle6.Visibility = Visibility.Hidden;
            myRightMapItemsInActionPanelSelected.Remove(mi.Name);
         }

         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButtonOk_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         myButtonOk.Visibility = Visibility.Hidden;
         myButtonIgnore.Visibility = Visibility.Hidden;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations: PerformConversation(gi, false); break;
            case GamePhase.ImplantRemoval: PerformImplantRemoval(gi, false); break;
            case GamePhase.AlienTakeover: PerformTakeover(gi, false); break;
            case GamePhase.Influences: PerformInfluence(gi, false); break;
            case GamePhase.Combat:
               {
                  if ("" == myTextBoxResults.Text)
                     PerformCombat(gi, false);
                  else
                     PerformCombatRetreat(gi, false);
                  break;
               }
            default: break;
         }
         UpdateViewState(gi);
      }
      private void myButtonIgnoreClick(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = myGameInstance;
         myButtonOk.Visibility = Visibility.Hidden;
         myButtonIgnore.Visibility = Visibility.Hidden;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations: PerformConversation(gi, true); break;
            case GamePhase.ImplantRemoval: PerformImplantRemoval(gi, true); break;
            case GamePhase.AlienTakeover: PerformTakeover(gi, true); break;
            case GamePhase.Influences: PerformInfluence(gi, true); break;
            case GamePhase.Combat:
               {
                  if ("" == myTextBoxResults.Text)
                     PerformCombat(gi, true);
                  else
                     PerformCombatRetreat(gi, true);
                  break;
               }
            default: break;
         }
         UpdateViewState(gi);
      }
      private void GameViewerWindowClosed(object sender, EventArgs e)
      {
         Application app = Application.Current;
         app.Shutdown();
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private void MapItemCommonAction(ITerritory selectedTerritory)
      {
         IGameInstance gi = myGameInstance;
         myStoryboard = null;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
               if (false == IsAlien)
                  DisplayConversation(gi, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.Influences:
               if (false == IsAlien)
                  DisplayInfluence(gi, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.Combat:
               DisplayCombat(gi, selectedTerritory);
               return;
            case GamePhase.Iterrogations:
               if (false == IsAlien)
               {
                  if ((true == selectedTerritory.IsBuilding()) && (null != gi.ZebulonTerritories.Find(selectedTerritory.ToString())) && (0 < gi.NumIterogationsThisTurn))
                  {
                     --gi.NumIterogationsThisTurn;
                     gi.ZebulonTerritories.Remove(selectedTerritory);
                     IMapItem zebulon = myGameInstance.Persons.Find("Zebulon");
                     if ((zebulon.TerritoryCurrent.Name == selectedTerritory.Name) && (zebulon.TerritoryCurrent.Sector == selectedTerritory.Sector))
                     {
                        zebulon.IsAlienKnown = true;
                        gi.NumIterogationsThisTurn = 0;
                     }
                     StringBuilder sb = new StringBuilder("MouseLeftButtonDownCanvas(): "); sb.Append(gi.NumIterogationsThisTurn.ToString()); sb.Append("). picked "); sb.Append(selectedTerritory.ToString());
                     Logger.Log(LogEnum.LE_SHOW_ITEROGATIONS, sb.ToString());
                     GameAction outAction = GameAction.TownspersonIterrogates;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               return;
            case GamePhase.ImplantRemoval:
               if (false == IsAlien)
                  DisplayImplantRemoval(gi, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.AlienTakeover:
               if (true == IsAlien)
                  DisplayTakover(gi, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            default:
               this.RotateStack(selectedTerritory); // rotate the stack
               break;
         }
      }
      private void MapItemMoveManually(ITerritory selectedTerritory, Button selectedButton)
      {
         IGameInstance gi = myGameInstance;

         // MapItem already selected to move.  Moving it to a known space

         if ((null != selectedTerritory) && (null != selectedButton))
         {
            IMapItem movingMapItem = gi.Persons.Find(selectedButton.Name);
            if ((selectedTerritory.Name == movingMapItem.TerritoryCurrent.Name) && (selectedTerritory.Sector == movingMapItem.TerritoryCurrent.Sector))
            {
               this.RotateStack(selectedTerritory); // rotate the stack
            }
            else if (movingMapItem.Movement <= movingMapItem.MovementUsed) // already used up movement
            {
               this.RotateStack(selectedTerritory); // rotate the stack
            }
            else
            {
               int movementLeftToUse = movingMapItem.Movement - movingMapItem.MovementUsed;
               if (movementLeftToUse < 1)
               {
                  MessageBox.Show("No movement left for this person, Choose another person to move.");
                  return;
               }

               movingMapItem.TerritoryStarting = movingMapItem.TerritoryCurrent;
               MapItemMove mim = new MapItemMove(Territories.theTerritories, movingMapItem, selectedTerritory);
               if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
               {
                  if (true == myIsFlagSetForOverstack)
                     MessageBox.Show("Unable to take this path due to overstacking restrictions. Choose another endpoint.");
                  myIsFlagSetForOverstack = true;
                  return;
               }
               gi.MapItemMoves.Clear();
               gi.MapItemMoves.Add(mim);
               if (GamePhase.AlienMovement == gi.GamePhase)
               {
                  GameAction outAction = GameAction.AlienMovement;
                  myGameEngine.PerformAction(ref gi, ref outAction);
               }
               else if (GamePhase.TownspersonMovement == gi.GamePhase)
               {
                  myIsAlienAbleToStopMove = true; // The townsperson cannot move any more MapItems until a response is received from teh Alien player.
                  GameAction outAction = GameAction.TownpersonProposesMovement;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
            }
         }
      }
      private bool IsAlienAbleToStopMove(IGameInstance gi, IMapItem mi)
      {
         if (("Zebulon" != mi.Name) && (true != mi.IsStunned) && (true != mi.IsTiedUp) && (true != mi.IsSurrendered)
          && (true != mi.IsStunned) && (true != mi.IsKilled) && ((true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown))
          && (false == mi.IsMoveStoppedThisTurn) && (GamePhase.TownspersonMovement == gi.GamePhase))
         {
            if (0 < gi.MapItemMoves.Count)
            {
               IMapItemMove mim = gi.MapItemMoves[0];

               IMapItem movingMI = gi.Persons.Find(mim.MapItem.Name);
               if ((mi.TerritoryCurrent.Name == mim.OldTerritory.Name) && (mi.TerritoryCurrent.Sector == mim.OldTerritory.Sector))
               {
                  if ((true == movingMI.IsControlled) && (false == movingMI.IsStunned) && (false == movingMI.IsTiedUp)
                     && (false == movingMI.IsSurrendered) && (false == movingMI.IsStunned) && (false == movingMI.IsKilled))
                  {
                     return true;
                  }
               }
               else
               {
                  foreach (ITerritory t in mim.BestPath.Territories)
                  {
                     if ((mi.TerritoryCurrent.Name == t.Name) && (mi.TerritoryCurrent.Sector == t.Sector))
                     {
                        if ((true == movingMI.IsControlled) && (false == movingMI.IsStunned) && (false == movingMI.IsTiedUp)
                             && (false == movingMI.IsSurrendered) && (false == movingMI.IsStunned) && (false == movingMI.IsKilled)
                             && (false == movingMI.IsMoveStoppedThisTurn))
                        {
                           return true;
                        }
                     }
                  } // end foreach
               }

            } // end if (0 < gi.MapItemMoves.Count)

         } // end if (("Zebulon" != mi.Name) ...

         return false;
      }
      private void MapItemReturnToStart(Button selectedButton)
      {
         IGameInstance gi = myGameInstance;
         IMapItem selectedMapItem = gi.Persons.Find(selectedButton.Name);
         if (null == selectedMapItem)
         {
            Console.WriteLine("GameViewerWindow::MapItemReturnToStart(): selectedMapItem=null for {0}", selectedButton.Name);
            return;
         }
         if (false == selectedMapItem.IsMoveAllowedToResetThisTurn) // if not allowed to reset, do nothing
         {
            if ((true == myIsFlagSetForMoveReset) && (true == IsAlien) && (GamePhase.AlienMovement == gi.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            if ((true == myIsFlagSetForMoveReset) && (false == IsAlien) && (GamePhase.TownspersonMovement == gi.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            myIsFlagSetForMoveReset = true;
            this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
            return;  // do nothing
         }
         switch (gi.GamePhase)
         {
            case GamePhase.AlienMovement:
               if ((true == selectedMapItem.IsControlled) || (false == IsAlien))
               {
                  this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                  return;  // do nothing
               }
               break;

            case GamePhase.TownspersonMovement:
               if ((false == selectedMapItem.IsControlled) || (true == IsAlien))
               {
                  this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
                  return;  // do nothing
               }
               break;

            default:
               this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
               return;

         } // end switch

         StringBuilder sb = new StringBuilder("MapItemReturnToStart(): t="); sb.Append(selectedMapItem.TerritoryCurrent.ToString()); sb.Append(" st="); sb.Append(selectedMapItem.TerritoryStarting.ToString());
         Logger.Log(LogEnum.LE_MIM_RETURN_TO_START, sb.ToString());

         if (selectedMapItem.TerritoryCurrent != selectedMapItem.TerritoryStarting)
         {
            // Turn off all animation

            foreach (Rectangle r in myRectangles)
            {
               r.BeginAnimation(Canvas.LeftProperty, null);
               r.BeginAnimation(Canvas.TopProperty, null);
            }
            myRectangleSelection.BeginAnimation(Canvas.LeftProperty, null);
            myRectangleSelection.BeginAnimation(Canvas.TopProperty, null);
            selectedButton.BeginAnimation(Canvas.LeftProperty, null);
            selectedButton.BeginAnimation(Canvas.TopProperty, null);

            myRectangleSelection.Visibility = Visibility.Hidden;
            myMovingButton = null;

            if (0 < gi.MapItemMoves.Count)
            {
               IMapItemMove mim = gi.MapItemMoves[0];
               IMapItem previousMovingMi1 = gi.Persons.Find(mim.MapItem.Name);
               if (null != previousMovingMi1)
               {
                  previousMovingMi1.TerritoryCurrent = previousMovingMi1.TerritoryStarting;
                  previousMovingMi1.MovementUsed -= mim.BestPath.Territories.Count;
                  if (previousMovingMi1.MovementUsed <= 0)
                  {
                     previousMovingMi1.MovementUsed = 0;
                     previousMovingMi1.IsMoved = false;

                     IMapItem alreadyMovedMapItem = myMovingMapItems.Find(previousMovingMi1.Name);
                     if (null != alreadyMovedMapItem)
                     {
                        StringBuilder sb1 = new StringBuilder("MapItemReturnToStart(): n="); sb1.Append(previousMovingMi1.Name); sb1.Append(" st="); sb1.Append(previousMovingMi1.TerritoryStarting.ToString());
                        Logger.Log(LogEnum.LE_SHOW_MIM_MOVING_COUNT, sb1.ToString());
                        myMovingMapItems.Remove(previousMovingMi1.Name);
                     }
                  }
               }

               gi.MapItemMoves.Clear();
               GameAction outAction = GameAction.ResetMovement;
               myGameEngine.PerformAction(ref gi, ref outAction); // Inform the user to return back
            }
         }
      }
      private void RotateStack(ITerritory selectedTerritory)
      {
         IGameInstance gi = myGameInstance;
         myRectangleSelection.Visibility = Visibility.Hidden;
         myMovingButton = null;

         // Find the right stack that matches the selected terriroty
         IStack? stack = myGameInstance.Stacks.Find(selectedTerritory);
         if (null == stack)
            return;
         IMapItems deadPeopleInStack = new MapItems();
         IMapItems alivePeopleInStack = new MapItems();
         foreach (IMapItem mi in stack.MapItems)
         {
            if (true == mi.IsKilled)
               deadPeopleInStack.Add(mi);
            else
               alivePeopleInStack.Add(mi);
         }
         if (0 == alivePeopleInStack.Count) // if there are no alive people, nothing to rotate
            return;
         foreach (IMapItem deadPerson in deadPeopleInStack)   // Remove all dead people
         {
            IMapItem person = gi.Persons.Find(deadPerson.Name);
            gi.Persons.Remove(person);
         }
         Rectangle bottomRect = null;
         IMapItem bottomMi = gi.Persons.Find(alivePeopleInStack[0].Name); // Remove the bottom MapItem, Bounding Rectable, and button.
         if (null == bottomMi)
            return;
         Button bottomButton = myButtons.Find(bottomMi.Name);
         if (null == bottomButton)
            return;
         alivePeopleInStack.Remove(bottomMi.Name);
         gi.Persons.Remove(bottomMi.Name);
         foreach (Rectangle r in myRectangles)
         {
            if ((Canvas.GetLeft(r) == Canvas.GetLeft(bottomButton)) && (Canvas.GetTop(r) == Canvas.GetTop(bottomButton)))
            {
               bottomRect = r;
               break;
            }
         }
         int counterCount = 0; // Shift the remaining MapItems in the stack
         foreach (IMapItem mi in alivePeopleInStack)
         {
            Button b = myButtons.Find(mi.Name);
            if (null != b)
            {
               mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount * 3));
               foreach (Rectangle r in myRectangles)
               {
                  if (r.Visibility == Visibility.Visible)
                  {
                     if ((Canvas.GetLeft(r) == Canvas.GetLeft(b)) && (Canvas.GetTop(r) == Canvas.GetTop(b)))
                     {
                        Canvas.SetLeft(r, mi.Location.X);
                        Canvas.SetTop(r, mi.Location.Y);
                        Canvas.SetZIndex(r, counterCount * 10 + 1);
                        r.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                        r.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                        break;
                     }
                  }
               }
               Canvas.SetLeft(b, mi.Location.X);
               Canvas.SetTop(b, mi.Location.Y);
               Canvas.SetZIndex(b, counterCount * 10);
               b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
               b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
               MapItem.SetButtonContent(b, mi, IsAlien);
               ++counterCount;
            }
         }
         gi.Persons.Add(bottomMi); // Add back the bottom items to the top
         bottomMi.Location = new MapPoint(bottomMi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount * 3), bottomMi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount * 3));
         Canvas.SetLeft(bottomButton, bottomMi.Location.X);
         Canvas.SetTop(bottomButton, bottomMi.Location.Y);
         Canvas.SetZIndex(bottomButton, counterCount * 10);
         bottomButton.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         bottomButton.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         MapItem.SetButtonContent(bottomButton, bottomMi, IsAlien);
         if (null != bottomRect)
         {
            Canvas.SetLeft(bottomRect, bottomMi.Location.X);
            Canvas.SetTop(bottomRect, bottomMi.Location.Y);
            Canvas.SetZIndex(bottomRect, 1000);
            bottomRect.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            bottomRect.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         }

         // Add dead people back into the stack

         foreach (IMapItem deadPerson in deadPeopleInStack)
         {
            IMapItem person = gi.Persons.Find(deadPerson.Name);
            gi.Persons.Add(person);
         }
      }
   }
   //============================================================================
   public static class MyGameViewerWindowExtensions
   {
      public static Button? Find(this IList<Button> list, string name)
      {
         IEnumerable<Button> results = from button in list
                                       where button.Name == name
                                       select button;
         if (0 < results.Count())
            return results.First();
         else
            return null;
      }
   }
}
