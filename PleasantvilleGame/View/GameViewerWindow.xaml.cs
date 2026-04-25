using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs=System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace PleasantvilleGame
{
   public partial class GameViewerWindow : Window, IView
   {
      //--------------------------------------------------------------
      private const int ANIMATE_SPEED = 3;
      public bool CtorError { set; get; } = false;
      private static Mutex theSaveSettingsMutex = new Mutex();
      #region Win32 API declarations to set and get window placement
      [DllImport("user32.dll")]
      private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);
      [DllImport("user32.dll")]
      private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);
      private const int SwShownormal = 1;
      private const int SwShowminimized = 2;
      #endregion
      //--------------------------------------------------------------
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30.0;
      private const Double ELLIPSE_DIAMETER = 40.0;
      private const Double ELLIPSE_RADIUS = ELLIPSE_DIAMETER / 2.0;
      private Double theOldXAfterAnimation = 0.0;
      private Double theOldYAfterAnimation = 0.0;
      //--------------------------------------------------------------
      private IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      private IDieRoller? myDieRoller = null;
      private EventViewer? myEventViewer = null;
      private MainMenuViewer? myMainMenuViewer = null;
      //--------------------------------------------------------------
      private List<Button> myButtons = new List<Button>();
      private bool myIsFlagSetForAlienMoveCountExceeded = false;  // Alien only allowed to move 5 counters
      private bool myIsFlagSetForMoveReset = false;               // Players cannot reset counter when selected
      private bool myIsFlagSetForOverstack = false;               // MapItem cannot move into hex due to overstack
      private bool myIsFlagSetForMaxMove = false;                 // MapItem cannot move into hex due to overstack
      private bool myIsAlienAbleToStopMove = false;             // The Alien player is allowed to stop Townspeople from moving if in the same hex
      //--------------------------------------------------------------
      private List<Brush> myBrushes = new List<Brush>();
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
      private List<Rectangle> myRectangles = new List<Rectangle>();
      private Rectangle? myMovingRectangle = null;                // Rentangle that is moving with button
      private MapItems myMovingMapItems = new MapItems();         // A list to track which MapItems have moved this turn
      private Button? myMovingButton = null;                      // The manually selected button that will be moved
      private Rectangle myRectangleSelection = new Rectangle();   // Player has manually selected this button
      //--------------------------------------------------------------
      private readonly SplashDialog mySplashScreen;
      private ContextMenu myContextMenuCanvas = new ContextMenu();
      //--------------------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahofma");
      private Storyboard? myStoryboard = null;
#pragma warning disable CA1416 // Validate platform compatibility
      private System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
#pragma warning restore CA1416 // Validate platform compatibility
      private TextBlock myTextBoxMarquee; // Displayed at end to show Statistics of games
      private Double mySpeedRatioMarquee = 1.0;
      private Storyboard myStoryboardMarquee = new Storyboard();    // Show Statistics Marquee at end of game 
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
      //==============================================================
      public GameViewerWindow(IGameEngine ge, IGameInstance gi)
      {
         myGameEngine = ge;
         myGameInstance = gi;
         mySplashScreen = new SplashDialog(); // show splash screen waiting for finish initializing
         mySplashScreen.Show();
         InitializeComponent();
         //---------------------------------------------------------------
         NameScope.SetNameScope(this, new NameScope()); // TextBox Marquee is end game condtion - display Game Statistics
         myTextBoxMarquee = new TextBlock() { Foreground = Brushes.Red, FontFamily = myFontFam, FontSize = 24 };
         myTextBoxMarquee.MouseLeftButtonDown += MouseLeftButtonDownMarquee;
         myTextBoxMarquee.MouseLeftButtonUp += MouseLeftButtonUpMarquee;
         myTextBoxMarquee.MouseRightButtonDown += MouseRightButtonDownMarquee;
         this.RegisterName("tbMarquee", myTextBoxMarquee);
         //---------------------------------------------------------------
         myMainMenuViewer = new MainMenuViewer(ge, gi, myMainMenu);
         if (false == AddHotKeys(myMainMenuViewer))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): AddHotKeys() returned false");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         if (false == String.IsNullOrEmpty(Properties.Settings.Default.GameDirectoryName))
            GameLoadMgr.theGamesDirectory = Properties.Settings.Default.GameDirectoryName; // remember the game directory name
         //---------------------------------------------------------------
         if (false == DeserializeOptions(Properties.Settings.Default.GameOptions, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): DeserializeOptions() returned false");
            CtorError = true;
            return;
         }
         myMainMenuViewer.NewGameOptions = gi.Options;
         Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "GameViewerWindow(): Options=" + gi.Options.ToString());
         //---------------------------------------------------------------
         if (false == DeserializeGameFeats(GameEngine.theInGameFeats))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): DeserializeGameFeats() returned false");
            CtorError = true;
            return;
         }
         GameEngine.theStartingFeats = GameEngine.theInGameFeats.Clone(); // need to know difference between starting feats and feats that happen in this game
         GameEngine.theStartingFeats.SetGameFeatThreshold();
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "GameViewerWindow():\n  feats=" + GameEngine.theInGameFeats.ToString());
         //---------------------------------------------------------------
         if (false == DeserializeGameStatistics(GameEngine.theAlienSoloStatistics, "stat0"))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Deserialize_GameStatistics(theAlienSoloStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "GameViewerWindow():\n  theAlienSoloStatistics stats=" + GameEngine.theAlienSoloStatistics.ToString());
         if (false == DeserializeGameStatistics(GameEngine.theTownsSoloStatistics, "stat1"))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Deserialize_GameStatistics(theTownsSoloStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "GameViewerWindow():\n  theTownsSoloStatistics stats=" + GameEngine.theTownsSoloStatistics.ToString());
         if (false == DeserializeGameStatistics(GameEngine.theAlienVersusStatistics, "stat2"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowStatsAdds(): Deserialize_GameStatistics(theAlienVersusStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "GameViewerWindow():\n  theTownsVersusStatistics stats=" + GameEngine.theTownsVersusStatistics.ToString());
         if (false == DeserializeGameStatistics(GameEngine.theTownsVersusStatistics, "stat3"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowStatsAdds(): Deserialize_GameStatistics(theTownsVersusStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "GameViewerWindow():\n  theTownsVersusStatistics stats=" + GameEngine.theTownsVersusStatistics.ToString());
         //---------------------------------------------------------------
         Utilities.ZoomCanvas = Properties.Settings.Default.ZoomCanvas;
         myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // Constructor - revert to save zoom
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvasMain);
         //---------------------------------------------------------------
         SetDisplayIconForUninstall(); // This is specialized code to add to Windows Registry the icon for uninstall
         //-----------------------------------------------
         this.BorderBrush = Constants.theNeutralBrush;
         mySolidColorBrushClear.Color = Color.FromArgb(0, 0, 1, 0);
         myBrushes.Add(Brushes.Green);  // Create a container of brushes for painting paths.
         myBrushes.Add(Brushes.Blue);
         myBrushes.Add(Brushes.Purple);
         myBrushes.Add(Brushes.Yellow);
         myBrushes.Add(Brushes.Red);
         myBrushes.Add(Brushes.Orange);
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);
         //---------------------------------------------------------------
         myDieRoller = new DieRoller(myCanvasMain, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //----------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, Territories.theTerritories, myDieRoller);
         if (true == myEventViewer.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myEventViewer.CtorError=true");
            CtorError = true;
            return;
         }
         CanvasImageViewer civ = new CanvasImageViewer(myCanvasMain, myDieRoller);
         if (true == civ.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): civ.CtorError=true");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         //if (true == GameEngine.theIsAlien)
         //   myTextBoxEntry.Foreground = Constants.theAlienControlledBrush;
         //else
         //   myTextBoxEntry.Foreground = Constants.theTownControlledBrush;
         //----------------------------------------------------------
#pragma warning disable CA1416 // Validate platform compatibility
         myTimer.Interval = ANIMATE_SPEED * 1000 + 1000;
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
         myTimer.Tick += new EventHandler(TimerElasped);
#pragma warning restore CA1416 // Validate platform compatibility
         //----------------------------------------------------------
         StringBuilder sb55 = new StringBuilder();
         if (true == GameEngine.theIsServer)
            sb55.Append("SERVER: ");
         else
            sb55.Append("CLIENT: ");
         if (true == GameEngine.theIsAlien)
            sb55.Append("Pleasantville For Aliens");
         else
            sb55.Append("Pleasantville For Humans");
         this.Title = sb55.ToString();
         myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownCanvas;
         myCanvasMain.MouseRightButtonDown += this.MouseRightButtonDownCanvas;
         //----------------------------------------------------------
         //foreach (ITerritory t in Territories.theTerritories) // Create the regions associated with the territories. All the information of Territories is static and does not change.
         //{
         //   if (null == t)
         //   {
         //      Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): null territory in Territories.theTerritories");
         //      CtorError = true;
         //      return;
         //   }
         //   string? tagName = t.ToString();
         //   if (null == tagName)
         //   {
         //      Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): tagName=null for t=" + t.Name);
         //      CtorError = true;
         //      return;
         //   }
         //   if (0 < t.Points.Count)
         //   {
         //      Polygon aPolygon = new Polygon();
         //      aPolygon.Fill = mySolidColorBrushClear;
         //      aPolygon.Tag = Utilities.RemoveSpaces(tagName);
         //      aPolygon.Name = t.Name + t.Subname.ToString();
         //      myCanvasMain.RegisterName(aPolygon.Name, aPolygon);
         //      List<Point> points = new List<Point>();
         //      foreach (IMapPoint mp in t.Points)
         //         points.Add(new Point(mp.X, mp.Y));
         //      PointCollection pointCollection = new PointCollection(points);
         //      aPolygon.Points = pointCollection;
         //      myCanvasMain.Children.Add(aPolygon);
         //   }
         //}
         //------------------------------------------
         //myContextMenuCanvas.Loaded += this.ContextMenuLoaded;  // Setup Context Menu for Buttons
         //MenuItem mi1 = new MenuItem();
         //mi1.Header = "_Return to Starting point";
         //mi1.InputGestureText = "Ctrl+S";
         //mi1.Click += this.ContextMenuClickReturnToStart;
         //myContextMenuCanvas.Items.Add(mi1);
         //MenuItem mi2 = new MenuItem();
         //mi2.Header = "_Rotate Stack";
         //mi2.InputGestureText = "Ctrl+R";
         //mi2.Click += this.ContextMenuClickRotate;
         //myContextMenuCanvas.Items.Add(mi2);
         ////------------------------------------------
         //if (true == GameEngine.theIsAlien)
         //{
         //   MenuItem mi3 = new MenuItem();
         //   mi3.Header = "_Expose";
         //   mi3.InputGestureText = "Ctrl+E";
         //   mi3.Click += this.ContextMenuClickExposeAlien;
         //   myContextMenuCanvas.Items.Add(mi3);
         //   MenuItem mi4 = new MenuItem();
         //   mi4.Header = "_Stop Townsperson Move";
         //   mi4.InputGestureText = "Ctrl+S";
         //   mi4.Click += this.ContextMenuClickStopMove;
         //   myContextMenuCanvas.Items.Add(mi4);
         //}
         ////-----------------------------------------------
         //foreach (IStack stack in gi.Stacks) // Create the buttons based on People
         //{
         //   foreach (IMapItem person in stack.MapItems) 
         //   {
         //      Button b = new Button();
         //      if (person.Name == "Zebulon")
         //      {
         //         if (false == GameEngine.theIsAlien)
         //            b.Visibility = Visibility.Hidden;
         //      }
         //      b.ContextMenu = myContextMenuCanvas;
         //      Canvas.SetLeft(b, person.Location.X - Utilities.theMapItemOffset);
         //      Canvas.SetTop(b, person.Location.Y - Utilities.theMapItemOffset);
         //      b.Click += this.ClickMapItem;
         //      b.MouseDoubleClick += this.MouseDoubleClickMapItem;
         //      b.Name = person.Name;
         //      b.Height = 50.0;
         //      b.Width = 50.0;
         //      b.IsEnabled = true;
         //      MapItem.SetButtonContent(b, person, GameEngine.theIsAlien);
         //      myButtons.Add(b);
         //      myCanvasMain.Children.Add(b);
         //   }
         //}
         ////------------------------------------------------
         //for (int i = 0; i < 6; ++i) // Create a Bounding Rectangles to indicate when a MapItem is moved
         //{
         //   Rectangle r = new Rectangle();
         //   r.Stroke = myBrushes[i];
         //   r.StrokeThickness = 2.0;
         //   r.StrokeDashArray = myDashArray;
         //   r.Width = 50;
         //   r.Height = 50;
         //   r.Visibility = Visibility.Hidden;
         //   myRectangles.Add(r);
         //   myCanvasMain.Children.Add(r);
         //}
         //myRectangleSelection.Stroke = Brushes.Red; // Create a Bounding Rectangle to indicate when a MapItem is selected to be moved by mouse pointer
         //myRectangleSelection.StrokeThickness = 3.0;
         //myRectangleSelection.Width = 50;
         //myRectangleSelection.Height = 50;
         //myRectangleSelection.Visibility = Visibility.Hidden;
         //myCanvasMain.Children.Add(myRectangleSelection);
         //Canvas.SetZIndex(myRectangleSelection, 1000);
         //ClearActionPanel();
         //----------------------------------------------------------
         ge.RegisterForUpdates(civ); // Implement the Model View Controller (MVC) pattern by registering views with  the game engine such that when the model data is changed, the views are updated.
         ge.RegisterForUpdates(myMainMenuViewer);
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(myEventViewer); // needs to be last so UploadGameView
         ge.RegisterForUpdates(this);
         Logger.Log(LogEnum.LE_GAME_INIT, "GameViewerWindow(): \nzoomCanvas=" + Properties.Settings.Default.ZoomCanvas.ToString() + "\nwp=" + Properties.Settings.Default.WindowPlacement + "\noptions=" + Properties.Settings.Default.GameOptions);
#if UT1
         if (false == ge.CreateUnitTests(gi, myDockPanelTop, this, myEventViewer, myDieRoller, civ))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Create_UnitTests() returned false");
            CtorError = true;
            return;
         }
         gi.GamePhase = GamePhase.UnitTest;
#endif
      }
      //-----------------------SUPPORTING FUNCTIONS--------------------
      private bool AddHotKeys(MainMenuViewer mmv)
      {
         try
         {
            //RoutedCommand command = new RoutedCommand();
            //KeyGesture keyGesture = new KeyGesture(Key.N, ModifierKeys.Control);
            //InputBindings.Add(new KeyBinding(command, keyGesture));
            //CommandBindings.Add(new CommandBinding(command, mmv.MenuItemNew_Click));
            ////------------------------------------------------
            //command = new RoutedCommand();
            //keyGesture = new KeyGesture(Key.O, ModifierKeys.Control);
            //InputBindings.Add(new KeyBinding(command, keyGesture));
            //CommandBindings.Add(new CommandBinding(command, mmv.MenuItemFileOpen_Click));
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddHotKeys(): ex=" + ex.ToString());
            return false;
         }
         return true;
      }
      private void SetDisplayIconForUninstall()
      {
#if !DEBUG // Only do this for release version
         if (true == Properties.Settings.Default.theIsFirstRun) // only do once - must set it in registry
         {
            try
            {
               string iconSourcePath = System.IO.Path.Combine(MapImage.theImageDirectory, "Pleasantville.ico");
               var myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
               string[] mySubKeyNames = myUninstallKey.GetSubKeyNames();
               for (int i = 0; i < mySubKeyNames.Length; i++)
               {
                  RegistryKey aKey = myUninstallKey.OpenSubKey(mySubKeyNames[i], true);
                  // ClickOnce(Publish)
                  // Publish -> Settings -> Options 
                  // Publish Options -> Description -> Product Name (is your DisplayName)
                  string displayName = (string)aKey.GetValue("DisplayName");
                  if (true == displayName.Contains("Pattons Best"))
                  {
                     Logger.Log(LogEnum.LE_GAME_INIT, "SetDisplayIconForUninstall(): iconSourcePath=" + iconSourcePath);
                     aKey.SetValue("DisplayIcon", iconSourcePath);
                     break;
                  }
               }
               Properties.Settings.Default.theIsFirstRun = false;
               Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): e=" + ex.ToString());
            }
         }
#endif
      }
      private void CloseSplashScreen() // callback function that removes splash screen when dice are loaded
      {
         GameAction outAction = GameAction.RemoveSplashScreen;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private bool SaveDefaultsToSettings(bool isWindowPlacementSaved = true)
      {
         theSaveSettingsMutex.WaitOne();
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         try
         {                                                                              
            if (true == isWindowPlacementSaved)
            {
               WindowPlacement wp; // Persist window placement details to application settings
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == GetWindowPlacement(hwnd, out wp))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): GetWindowPlacement() returned false");
                  return false;
               }
               string sWinPlace = Utilities.Serialize<WindowPlacement>(wp);
               Properties.Settings.Default.WindowPlacement = sWinPlace;
            }
            //-------------------------------------------
            Properties.Settings.Default.ZoomCanvas = Utilities.ZoomCanvas;
            //-------------------------------------------
            //Properties.Settings.Default.ScrollViewerHeight = myScrollViewerMain.Height;
            //Properties.Settings.Default.ScrollViewerWidth = myScrollViewerMain.Width;
            //-------------------------------------------
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "Save_DefaultsToSettings(): Options=" + myGameInstance.Options.ToString());
            string? sOptions = SerializeOptions(myGameInstance.Options);
            if (null == sOptions)
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeOptions() returned false");
               return false;
            }
            Properties.Settings.Default.GameOptions = sOptions;
            //-------------------------------------------
            Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Save_DefaultsToSettings():\n  SAVING feats=" + GameEngine.theInGameFeats.ToString());
            if (false == SerializeGameFeats(GameEngine.theInGameFeats))
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): Serialize_GameFeats() returned false");
               return false;
            }
            //-------------------------------------------
            if (false == SerializeGameStatistics(GameEngine.theAlienSoloStatistics, "stat0"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics() returned false");
               return false;
            }
            if (false == SerializeGameStatistics(GameEngine.theTownsSoloStatistics, "stat1"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics(theTownsSoloStatistics) returned false");
               return false;
            }
            if (false == SerializeGameStatistics(GameEngine.theAlienVersusStatistics, "stat2"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics(theAlienVersusStatistics) returned false");
               return false;
            }
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveDefaultsToSettings(): ex=" + ex.ToString());
            return false;
         }
         finally
         {
            Properties.Settings.Default.Save();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            theSaveSettingsMutex.ReleaseMutex();
         }
      }
      private string? SerializeOptions(Options options)
      {
         //--------------------------------                                                                                              //--------------------------------                                                                                  //--------------------------------
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Options></Options>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): root is null");
            return null;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", options.Count.ToString());
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");
            if (null == optionElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): CreateElement(Option) returned null");
               return null;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = root.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): AppendChild(optionNode) returned null");
               return null;
            }
         }
         //--------------------------------
         return aXmlDocument.OuterXml;
      }
      private bool SerializeGameFeats(GameFeats feats)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameFeats></GameFeats>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): aXmlDocument.DocumentElement=null");
            return false;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): root is null");
            return false;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", feats.Count.ToString());
         //--------------------------------
         foreach (GameFeat feat in feats)
         {
            XmlElement? featElem = aXmlDocument.CreateElement("Feat");
            if (null == featElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): CreateElement(Feat) returned null");
               return false;
            }
            featElem.SetAttribute("Key", feat.Key);
            featElem.SetAttribute("Value", feat.Value.ToString());
            XmlNode? featNode = root.AppendChild(featElem);
            if (null == featNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): AppendChild(featNode) returned null");
               return false;
            }
         }
         //-----------------------------------------
         if (null == aXmlDocument)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameTo_File(): aXmlDocument=null");
            return false;
         }
         //-----------------------------------------
         try
         {
            if (false == Directory.Exists(GameFeats.theGameFeatDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameFeats.theGameFeatDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): path=" + GameFeats.theGameFeatDirectory + "\n e=" + e.ToString());
            return false;
         }
         string filename = GameFeats.theGameFeatDirectory + "feats.xml";
         if (File.Exists(filename))
            File.Delete(filename);
         FileStream? writer = null;
         //-----------------------------------------
         try
         {
            writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);// For XmlWriter, it uses the stream that was created: writer.
            aXmlDocument.Save(xmlWriter);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): path=" + GameFeats.theGameFeatDirectory + "\n e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
         finally
         {
            if (writer != null)
               writer.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         return true;
      }
      private bool SerializeGameStatistics(GameStatistics statistics, string filename)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameStatistics> </GameStatistics>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): aXmlDocument.DocumentElement=null");
            return false;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): root is null");
            return false;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", statistics.Count.ToString());
         //-----------------------------------------
         foreach (GameStatistic statistic in statistics)
         {
            XmlElement? statisticElem = aXmlDocument.CreateElement("GameStatistic");
            if (null == statisticElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): CreateElement(GameStatistic) returned null");
               return false;
            }
            statisticElem.SetAttribute("Key", statistic.Key);
            statisticElem.SetAttribute("Value", statistic.Value.ToString());
            XmlNode? statisticNode = root.AppendChild(statisticElem);
            if (null == statisticNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): AppendChild(statisticNode) returned null");
               return false;
            }
         }
         //-----------------------------------------
         string filenameFull = GameStatistics.theGameStatisticsDirectory + filename + ".xml";
         if (File.Exists(filenameFull))
            File.Delete(filenameFull);
         FileStream? writer = null;
         //-----------------------------------------
         try
         {
            writer = new FileStream(filenameFull, FileMode.OpenOrCreate, FileAccess.Write);
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);// For XmlWriter, it uses the stream that was created: writer.
            aXmlDocument.Save(xmlWriter);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): path=" + GameStatistics.theGameStatisticsDirectory + "\n e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
         finally
         {
            if (writer != null)
               writer.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         //--------------------------------
         return true;
      }
      private bool DeserializeOptions(String sXml, Options options)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         //-----------------------------------------------
         options.Clear();
         if (true == String.IsNullOrEmpty(sXml))
         {
            Logger.Log(LogEnum.LE_GAME_INIT, "Deserialize_Options(): String.IsNullOrEmpty(sXml) returned true - first time thru");
            options.SetOriginalGameOptions();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return true;
         }
         //-----------------------------------------------
         try // XML serializer does not work for Interfaces
         {
            StringReader stringreader = new StringReader(sXml);
            XmlReader reader = XmlReader.Create(stringreader);
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "Options")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Options != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): IsStartElement(Option) returned false");
                  return false;
               }
               if (reader.Name != "Option")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Option != " + reader.Name);
                  return false;
               }
               string? name = reader.GetAttribute("Name");
               if (name == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Name=null");
                  return false;
               }
               string? sEnabled = reader.GetAttribute("IsEnabled");
               if (sEnabled == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): IsEnabled=null");
                  return false;
               }
               bool isEnabled = bool.Parse(sEnabled);
               Option option = new Option(name, isEnabled);
               options.Add(option);
            }
            if (0 < count)
               reader.Read(); // get past </Options>
         }
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\ndirException=" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nfileException=" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nioException=" + ioException.ToString());
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nex=" + ex.ToString());
         }
         finally
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == options.Count)
               options.SetOriginalGameOptions();
         }
         return true;
      }
      private bool DeserializeGameFeats(GameFeats feats)
      {
         feats.Clear();
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlTextReader? reader = null;
         try
         {
            string filename = GameFeats.theGameFeatDirectory + "feats.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): reader=null");
               return false;
            }
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "GameFeats")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Options != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): IsStartElement(Feat) returned false");
                  return false;
               }
               if (reader.Name != "Feat")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Feat != " + reader.Name);
                  return false;
               }
               string? key = reader.GetAttribute("Key");
               if (key == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Key=null");
                  return false;
               }
               string? sValue = reader.GetAttribute("Value");
               if (sValue == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): sValue=null");
                  return false;
               }
               int value = Convert.ToInt32(sValue);
               GameFeat feat = new GameFeat(key, value);
               feats.Add(feat);
            }
            if (0 < count)
               reader.Read(); // get past </GameFeats>
         }
         //==========================================
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): dirException=" + dirException.ToString());
            return false;
         }
         catch (FileNotFoundException)
         {
            // expected on first run
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ioException=" + ioException.ToString());
            return false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ex=" + ex.ToString());
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == feats.Count)
            {
               feats.SetOriginalGameFeats();
            }
            else
            {
               foreach (string sKey in GameFeats.theDefaults) // ensure that if any new options are added, they show up in list
               {
                  bool isMatchFound = false;
                  foreach (GameFeat feat in feats)
                  {
                     if (sKey == feat.Key)
                     {
                        isMatchFound = true;
                        break;
                     }
                  }
                  if (false == isMatchFound)
                     feats.Add(new GameFeat(sKey));
               }

            }
            feats.SetGameFeatThreshold(); // always set game feat thresholds to a known value on startup
         }
         return true;
      }
      private bool DeserializeGameStatistics(GameStatistics statistics, string filename)
      {
         statistics.Clear();
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlTextReader? reader = null;
         try
         {
            string qualifiedFilename = GameStatistics.theGameStatisticsDirectory + filename + ".xml";
            reader = new XmlTextReader(qualifiedFilename) { WhitespaceHandling = WhitespaceHandling.None };
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "GameStatistics")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): GameStatistics != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): IsStartElement(Feat) returned false");
                  return false;
               }
               if (reader.Name != "GameStatistic")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): GameStatistic != " + reader.Name);
                  return false;
               }
               string? key = reader.GetAttribute("Key");
               if (key == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): Key=null");
                  return false;
               }
               string? sValue = reader.GetAttribute("Value");
               if (sValue == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): sValue=null");
                  return false;
               }
               int value = Convert.ToInt32(sValue);
               GameStatistic stat = new GameStatistic(key, value);
               statistics.Add(stat);
            }
            if (0 < count)
               reader.Read(); // get past </GameFeats>
         }
         //==========================================
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): dirException=" + dirException.ToString());
            return false;
         }
         catch (FileNotFoundException)
         {
            // expected on first run
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ioException=" + ioException.ToString());
            return false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ex=" + ex.ToString());
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == statistics.Count)
               statistics.SetOriginalGameStatistics();
         }
         return true;
      }
      //-------------INTERFACE FUNCTIONS---------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) || (GameAction.RemoveSplashScreen == action))
         {
            if (false == UpdateViewForNewGame(ref gi, action)) // This calls PerformAction() to get to proper event
               Logger.Log(LogEnum.LE_ERROR, "Update_View(): UpdateViewForNewGame() returned false");
            return;
         }
         ////-------------------------------------------------------
         //if (true == GameEngine.theIsAlien)
         //{
         //   StringBuilder sb = new StringBuilder("---------------   ALIEN GameViewerWindow::UpdateView() ==> action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
         //   Logger.Log(LogEnum.LE_VIEW_UPDATE_WINDOW, sb.ToString());
         //}
         //else
         //{
         //   StringBuilder sb = new StringBuilder("---------------   TP   GameViewerWindow::UpdateView() ==> action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
         //   Logger.Log(LogEnum.LE_VIEW_UPDATE_WINDOW, sb.ToString());
         //}
         //myGameInstance = gi;
         //GameAction outAction = GameAction.Error;
         //switch (action) // Perform acton based on the current next action.
         //{
         //   case GameAction.RemoveSplashScreen:
         //   case GameAction.UpdateNewGameEnd:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
         //         return;
         //      }
         //      mySplashScreen.Close();
         //      //myScrollViewerMain.UpdateLayout();
         //      break;
         //   case GameAction.AlienStart:
         //      break;
         //   case GameAction.TownspersonStart:
         //      break;
         //   case GameAction.AlienDisplaysRandomMovement:
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         myStoryboard = null;
         //         UpdateViewMovement(gi);
         //         ClearActionPanel();
         //      }
         //      break;
         //   case GameAction.TownspersonDisplaysRandomMovement:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         myStoryboard = null;
         //         UpdateViewMovement(gi);
         //         ClearActionPanel();
         //      }
         //      break;
         //   case GameAction.AlienAcksRandomMovement:
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienAcksRandomMovement: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         myMovingMapItems.Clear();
         //         myMovingButton = null;
         //         myIsFlagSetForAlienMoveCountExceeded = false;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //         myIsCombatInitiatedForAlien = false;
         //         myIsCombatInitiatedForTownsperson = false;
         //         myConversationsCompleted = false;
         //         myInfluencesCompleted = false;
         //         myAlienCombatCompleted = false;
         //         myTownspeopleCombatCompleted = false;
         //         myInterogationsCompleted = false;
         //         myImplateRemovalsCompleted = false;
         //         myTakeoversCompleted = false;
         //         myTimer.Interval = ANIMATE_SPEED * 1000 + 3000;  // reset timer
         //      }
         //      break;
         //   case GameAction.TownspersonAcksRandomMovement:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonAcksRandomMovement: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         myMovingMapItems.Clear();
         //         myMovingButton = null;
         //         myIsFlagSetForAlienMoveCountExceeded = false;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //         myIsCombatInitiatedForAlien = false;
         //         myIsCombatInitiatedForTownsperson = false;
         //         myConversationsCompleted = false;
         //         myInfluencesCompleted = false;
         //         myAlienCombatCompleted = false;
         //         myTownspeopleCombatCompleted = false;
         //         myInterogationsCompleted = false;
         //         myImplateRemovalsCompleted = false;
         //         myTakeoversCompleted = false;
         //      }
         //      break;
         //   case GameAction.ResetMovement:
         //      if (false == UpdateCanvasMain(gi, action, true))  // unhide the pologon line shown
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): ResetMovement: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      break;
         //   case GameAction.AlienMovement:
         //      UpdateViewMovement(gi);
         //      ClearActionPanel();
         //      break;
         //   case GameAction.AlienCompletesMovement:
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienCompletesMovement: UpdateCaUpdate_CanvasMainnvasMain() returned false");
         //            return;
         //         }
         //         myMovingButton = null;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //      }
         //      break;
         //   case GameAction.TownspersonAcksAlienMovement:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonAcksAlienMovement: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      myMovingMapItems.Clear();
         //      myMovingButton = null;
         //      myIsFlagSetForAlienMoveCountExceeded = false;
         //      myMovingRectangle = null;
         //      myRectangleSelection.Visibility = Visibility.Hidden;
         //      break;
         //   case GameAction.TownpersonProposesMovement:
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         myIsAlienAbleToStopMove = true;
         //         if (false == IsMoveStoppedByAlienBeforeStarted(gi))
         //         {
         //            UpdateViewMovement(gi);
         //            myTimer.Start(); // give the Alien time to look at move
         //         }
         //         else
         //         {
         //            outAction = GameAction.AlienModifiesTownspersonMovement;
         //            myGameEngine.PerformAction(ref gi, ref outAction);
         //         }
         //      }
         //      else
         //      {
         //         myMovingButton = null;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //      }
         //      break;
         //   case GameAction.TownpersonMovement:
         //      myIsAlienAbleToStopMove = false;
         //      myTimer.Stop();
         //      UpdateViewMovement(gi);
         //      break;
         //   case GameAction.AlienTimeoutOnMovement:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         myIsAlienAbleToStopMove = false;
         //         UpdateViewMovement(gi);
         //      }
         //      break;
         //   case GameAction.AlienModifiesTownspersonMovement:
         //      myIsAlienAbleToStopMove = false;
         //      if (false == UpdateCanvasMain(gi, action, true))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienModifiesTownspersonMovement: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewMovement(gi);
         //      break;
         //   case GameAction.TownpersonCompletesMovement:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownpersonCompletesMovement: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         myMovingButton = null;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //      }
         //      break;
         //   case GameAction.AlienAcksTownspersonMovement:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienAcksTownspersonMovement: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      myMovingMapItems.Clear();
         //      myMovingButton = null;
         //      myIsFlagSetForAlienMoveCountExceeded = false;
         //      myMovingRectangle = null;
         //      myRectangleSelection.Visibility = Visibility.Hidden;
         //      break;
         //   case GameAction.TownspersonPerformsConversation:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonPerformsConversation: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonCompletesConversations:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonCompletesConversations: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonPerformsInfluencing:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonPerformsInfluencing: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonCompletesInfluencing:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonCompletesInfluencing: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.AlienInitiateCombat:
         //      if ((true == myIsCombatInitiatedForAlien) && (true == GameEngine.theIsAlien))
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienInitiateCombat: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():AlienInitiateCombat: ALIEN Performs Combat");
         //         outAction = GameAction.AlienPerformCombat;
         //         myGameEngine.PerformAction(ref gi, ref outAction);
         //      }
         //      break;
         //   case GameAction.TownspersonNackCombatSelection:
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonNackCombatSelection: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         UpdateViewState(gi);
         //         myIsCombatInitiatedForAlien = false;
         //         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():TownspersonNackCombatSelection: ALIEN myIsCombatInitiatedForAlien=false");
         //      }
         //      break;
         //   case GameAction.AlienPerformCombat:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienPerformCombat: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      if (null == gi.MapItemCombat)
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienPerformCombat gi.MapItemCombat=null");
         //      }
         //      else
         //      {
         //         if (null != gi.MapItemCombat.Territory)
         //         {
         //            if ((0 != gi.MapItemCombat.Attackers.Count) && (0 != gi.MapItemCombat.Defenders.Count))
         //               DisplayCombatResults(gi);
         //         }
         //      }
         //      break;
         //   case GameAction.TownspersonInitiateCombat:
         //      if ((true == myIsCombatInitiatedForTownsperson) && (false == GameEngine.theIsAlien))
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonInitiateCombat: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():TownspersonInitiateCombat: TP PERFORMS COMBAT");
         //         outAction = GameAction.TownspersonPerformCombat;
         //         myGameEngine.PerformAction(ref gi, ref outAction);
         //      }
         //      break;
         //   case GameAction.AlienNackCombatSelection:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         if (false == UpdateCanvasMain(gi, action))
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienNackCombatSelection: Update_CanvasMain() returned false");
         //            return;
         //         }
         //         UpdateViewState(gi);
         //         myIsCombatInitiatedForTownsperson = false;
         //         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, "UpdateView():AlienNackCombatSelection: TP    myIsCombatInitiatedForTownsperson=false");
         //      }
         //      break;
         //   case GameAction.TownspersonPerformCombat:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonPerformCombat: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      if (null == gi.MapItemCombat)
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "UpdateView():TownspersonPerformCombat gi.MapItemCombat=null");
         //         return;
         //      }
         //      else
         //      {
         //         if ((0 != gi.MapItemCombat.Attackers.Count) && (0 != gi.MapItemCombat.Defenders.Count))
         //         {
         //            DisplayCombatResults(gi);
         //         }
         //      }
         //      break;
         //   case GameAction.TownspersonCompletesCombat:
         //      myRectangleSelection.Visibility = Visibility.Hidden;
         //      if (true == GameEngine.theIsAlien)
         //      {
         //         myMovingButton = null;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //      }
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonCompletesCombat: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      myIsCombatInitiatedForTownsperson = false;
         //      StringBuilder sb2 = new StringBuilder("UpdateView():TownspersonCompletesCombat: "); sb2.Append(GameEngine.theIsAlien.ToString()); sb2.Append("myIsCombatInitiatedForTownsperson=false");
         //      Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb2.ToString());
         //      break;
         //   case GameAction.AlienCompletesCombat:
         //      if (false == GameEngine.theIsAlien)
         //      {
         //         myMovingButton = null;
         //         myMovingRectangle = null;
         //         myRectangleSelection.Visibility = Visibility.Hidden;
         //      }
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienCompletesCombat: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      myIsCombatInitiatedForAlien = false;
         //      StringBuilder sb3 = new StringBuilder("UpdateView():AlienCompletesCombat: "); sb3.Append(GameEngine.theIsAlien.ToString()); sb3.Append("myIsCombatInitiatedForAlien=false");
         //      Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb3.ToString());
         //      break;
         //   case GameAction.TownspersonIterrogates:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonIterrogates: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonCompletesIterogations:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonCompletesIterogations: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.AlienAcksIterogations:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienAcksIterogations: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonRemovesImplant:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonRemovesImplant: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.TownspersonCompletesRemoval:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): TownspersonCompletesRemoval: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.AlienTakeover:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienTakeover: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      if (null == gi.Takeover)
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienTakeover gi.Takeover=null");
         //      }
         //      else
         //      {
         //         if (null == gi.Takeover.Alien)
         //         {
         //            Logger.Log(LogEnum.LE_ERROR, "UpdateView():AlienTakeover gi.Takeover.Alien=null");
         //         }
         //         else
         //         {
         //            if (true == GameEngine.theIsAlien)
         //            {
         //               myTextBoxResults.Text = gi.Takeover.Observations;
         //               UpdateActionPanelButtons(gi);
         //            }
         //            else
         //            {
         //               if ("Nobody Noticed" != gi.Takeover.Observations)
         //                  PerformTakeoverObserved(gi);
         //            }
         //         }
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.AlienCompletesTakeovers:
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): AlienCompletesTakeovers: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      myIsTakeOverInOneRegion = false;
         //      myIsTakeOverPromptNeededToFoolOpponent = false;
         //      break;
         //   case GameAction.ShowEndGame:
         //      ClearActionPanel();
         //      foreach (IStack stack in gi.Stacks)         // Show all the stacks
         //      {
         //         foreach (IMapItem mi in stack.MapItems)
         //         {
         //            if (true == mi.IsAlienUnknown)
         //            {
         //               if (false == gi.AddKnownAlien(mi))
         //                  Logger.Log(LogEnum.LE_ERROR, "UpdateView() returned error");
         //            }
         //         }
         //      }
         //      //-------------------------------------------------------
         //      bool isAlienWin = true;
         //      IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
         //      if (null == zebulon)
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "UpdateView() could not find Zebulon in gi.Stacks");
         //         break;
         //      }
         //      zebulon.IsAlienKnown = true;
         //      //if (true == zebulon.IsKilled)
         //      //   isAlienWin = false;

         //      //double controlledRatio = ((double)gi.InfluenceCountTownspeople) / ((double)gi.InfluenceCountTotal);
         //      //if (0.499999 < controlledRatio)
         //      //   isAlienWin = false;

         //      //int alienInflunence = gi.InfluenceCountAlienUnknown + gi.InfluenceCountAlienKnown;
         //      //if (0 == alienInflunence)
         //      //   isAlienWin = false;

         //      //myLabelWinner.Visibility = Visibility.Visible;  // Report the winner
         //      //if (true == isAlienWin)
         //      //{
         //      //   myLabelWinner.Content = "Aliens Win!!!!";
         //      //   myLabelWinner.Foreground = Brushes.Orange;
         //      //}
         //      //else
         //      //{
         //      //   myLabelWinner.Content = "Towns People Win!!!!";
         //      //   myLabelWinner.Foreground = Constants.theTownControlledBrush;
         //      //}
         //      if (false == UpdateCanvasMain(gi, action))
         //      {
         //         Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow:UpdateView(): ShowEndGame: Update_CanvasMain() returned false");
         //         return;
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   case GameAction.ShowAlien:
         //      foreach (Stack stack in gi.Stacks)
         //      {
         //         foreach (IMapItem mi in stack.MapItems)
         //         {
         //            if (true == mi.IsAlienKnown)
         //            {
         //               Button? b = myButtons.Find(mi.Name);
         //               if (null != b)
         //                  MapItem.SetButtonContent(b, mi, GameEngine.theIsAlien);
         //            }
         //         }
         //      }
         //      UpdateViewState(gi);
         //      break;
         //   default:
         //      Console.WriteLine("ERROR: GameViewerWindow::UpdateView() reached default next action={0}", action.ToString());
         //      break;
         //}
      }
      private bool UpdateViewForNewGame(ref IGameInstance gi, GameAction action) // GameAction.UpdateLoadingGame  GameAction.UpdateNewGame
      {
         Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateViewForNewGame(): Clearing action=" + action.ToString());
         myGameInstance = gi;
         myButtons.Clear();
         UpdateCanvasMainClear(myButtons, gi.Stacks, action, false);
         myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // UploadNewGame - Return to previous saved zoom level
         ////----------------------------------
         GameAction nextAction = GameAction.Error;
         if (GameAction.UpdateLoadingGame == action)
         {
            IGameCommand? cmd = gi.GameCommands.GetLast();
            if (null == cmd)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): cmd=null");
               return false;
            }
            nextAction = cmd.Action;
            gi.GamePhase = cmd.Phase;
            gi.DieRollAction = cmd.ActionDieRoll;
            gi.EventDisplayed = gi.EventActive = cmd.EventActive;
         }
         else if (GameAction.UpdateNewGame == action)
         {
            nextAction = GameAction.UpdateNewGameEnd;
         }
         else if (GameAction.RemoveSplashScreen == action)
         {
            mySplashScreen.Close();
            nextAction = GameAction.UpdateNewGameEnd;
         }
         //----------------------------------
         if (false == UpdateCanvasMain(gi, action))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): UpdateCanvasMain() returned error ");
            return false;
         }
         myGameEngine.PerformAction(ref gi, ref nextAction, Utilities.NO_RESULT);
         return true;
      }
      //-------------GameViewerWindow---------------------------------
      private void ContentRenderedGameViewerWindow(object sender, EventArgs e)
      {
         double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight; // 50=titlebar;
         myDockPanelInside.Height = mapPanelHeight;
         myDockPanelControls.Height = mapPanelHeight;
         //-----------------------------------------------------
         myScrollViewerTextBlock.Height = mapPanelHeight - myCanvasHelper.ActualHeight - 5;
         myTextBlockDisplay.Height = mapPanelHeight - myCanvasHelper.ActualHeight;
         //-----------------------------------------------------
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         myScrollViewerMain.Width = mapPanelWidth;
         myScrollViewerMain.Height = mapPanelHeight;
      }
      private void SizeChangedGameViewerWindow(object sender, SizeChangedEventArgs e)
      {
         double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight; // 50=titlebar
         myDockPanelInside.Height = mapPanelHeight;
         myDockPanelControls.Height = mapPanelHeight;
         //-----------------------------------------------------
         myScrollViewerTextBlock.Height = mapPanelHeight - myCanvasHelper.ActualHeight - 5;
         myTextBlockDisplay.Height = mapPanelHeight - myCanvasHelper.ActualHeight;
         //-----------------------------------------------------
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         myScrollViewerMain.Width = mapPanelWidth;
         myScrollViewerMain.Height = mapPanelHeight;
         Logger.Log(LogEnum.LE_SHOW_SCREEN_SIZE, " SizeChangedGameViewerWindow(): mapPanelHeight=" + mapPanelHeight.ToString("F2") + " mapPanelWidth=" + mapPanelWidth.ToString("F2"));
      }
      private void ClosedGameViewerWindow(object sender, EventArgs e)
      {
         System.Windows.Application app = System.Windows.Application.Current;
         app.Shutdown();
      }
      protected override void OnSourceInitialized(EventArgs e)
      {
         base.OnSourceInitialized(e);
         try
         {
            // Load window placement details for previous application session from application settings
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement places the window onto a visible monitor.
            if (false == String.IsNullOrEmpty(Properties.Settings.Default.WindowPlacement))
            {
               WindowPlacement wp = Utilities.Deserialize<WindowPlacement>(Properties.Settings.Default.WindowPlacement);
               wp.length = Marshal.SizeOf(typeof(WindowPlacement));
               wp.flags = 0;
               wp.showCmd = (wp.showCmd == SwShowminimized ? SwShownormal : wp.showCmd);
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == SetWindowPlacement(hwnd, ref wp))
                  Logger.Log(LogEnum.LE_ERROR, "SetWindowPlacement() returned false");
            }
            //if (0.0 != Properties.Settings.Default.ScrollViewerHeight)
            //   myScrollViewerMain.Height = Properties.Settings.Default.ScrollViewerHeight;
            //if (0.0 != Properties.Settings.Default.ScrollViewerWidth)
            //   myScrollViewerMain.Width = Properties.Settings.Default.ScrollViewerWidth;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "OnSourceInitialized() e=" + ex.ToString());
         }
         return;
      }
      protected override void OnClosing(CancelEventArgs e) //  // WARNING - Not fired when Application.SessionEnding is fired
      {
         base.OnClosing(e);
         if( false == SaveDefaultsToSettings())
            Logger.Log(LogEnum.LE_ERROR, "OnClosing() SaveDefaultsToSettings() returned false");
      }
      //-------------UPDATE HELPER FUNCTIONS---------------------------------
      private bool UpdateCanvasMain(IGameInstance gi, GameAction action, bool isOnlyLastLineRemoved = false)
      {
         //UpdateCanvasMainClear(myButtons, gi.Stacks, action, isOnlyLastLineRemoved);
         //---------------------------------------------------------------
         //IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
         //if (null == zebulon)
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "Update_CanvasMain(): could not find Zebulon in gi.Stacks");
         //   return false;
         //}
         //if (true == zebulon.IsAlienKnown)
         //{
         //   Button? b = myButtons.Find("Zebulon");
         //   if (null == zebulon)
         //   {
         //      Logger.Log(LogEnum.LE_ERROR, "Update_CanvasMain(): could not find Zebulon in myButtons");
         //      return false;
         //   }
         //   if (null != b)
         //   {
         //      b.Visibility = Visibility.Visible;
         //      Canvas.SetZIndex(b, 100000);
         //   }
         //}
         //---------------------------------------------------------------
         //foreach (Stack stack in gi.Stacks) // Update the Canvas with new MapItem locations
         //{
         //   int counterCount = 0;
         //   foreach (IMapItem mi in stack.MapItems)
         //   {
         //      Button? b = myButtons.Find(mi.Name);
         //      if (null != b)
         //      {
         //         b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
         //         b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         //         if (true == mi.IsKilled)
         //         {
         //            if (Visibility.Visible == b.Visibility)
         //            {
         //               b.Visibility = Visibility.Hidden;
         //               Logger.Log(LogEnum.LE_MOVE_KIA_RESULTS, mi.Name + " is hidden");
         //            }
         //         }
         //         else
         //         {
         //            MapItem.SetButtonContent(b, mi, GameEngine.theIsAlien);
         //            mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount * 3));
         //            ++counterCount;
         //            Canvas.SetLeft(b, mi.Location.X);
         //            Canvas.SetTop(b, mi.Location.Y);
         //            Canvas.SetZIndex(b, counterCount);
         //         }
         //      }
         //   }
         //}
         return true;
      }
      private void UpdateCanvasMainClear(List<Button> buttons, IStacks stacks, GameAction action, bool isOnlyLastLineRemoved)
      {
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Update_CanvasMainClear(): " + stacks.ToString());
         List<UIElement> lines = new List<UIElement>();  
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
         {
            if (ui is Button button)
            {
               if (true == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
                  continue;
               IMapItem? mi = stacks.FindMapItem(button.Name);
               if (null == mi) // If Button does not have corresponding MapItem, remove button.
               {
                  elements.Add(ui);
                  buttons.Remove(button);
                  IStack? stack = stacks.Find(button.Name);
                  if (null == stack)
                  {
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "Update_CanvasMainClear(): mi=" + button.Name + " does not belong to " + stacks.ToString());
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "Update_CanvasMainClear(): Remove mi=" + button.Name + " from stack=" + stack.ToString());
                     stack.MapItems.Remove(button.Name);
                  }
               }
               else
               {
                  MapItem.SetButtonContent(button, mi, true, true);
               }
            }
            else if (ui is Ellipse ellipse)
            {
               if ("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            else if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               elements.Add(ui);
            }
            else if (ui is Polygon polygon)
               elements.Add(ui);
            else if(ui is Label label)  // A Game Feat Label
               elements.Add(ui);
            else if(ui is Rectangle rect)
               elements.Add(ui);
            else if (ui is TextBlock tb)
               elements.Add(ui);
            else if (ui is Polyline polyline)
               elements.Add(ui);                  // Adding to lines instead of elements
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         ////---------------------------------------------------------------
         //if (false == isOnlyLastLineRemoved)
         //{
         //   foreach (UIElement line in lines)
         //      myCanvasMain.Children.Remove(line);
         //}
         //else
         //{
         //   if (0 < lines.Count)
         //      myCanvasMain.Children.Remove(lines.Last());
         //}
         ////---------------------------------------------------------------
         //foreach (Rectangle r in myRectangles)
         //   r.Visibility = Visibility.Hidden;
      }
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
               IMapItem? leftMapItem = myLeftMapItemsInActionPanel[0];
               if (null == leftMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton1, leftMapItem, false);
               myButton1.Visibility = Visibility.Visible;
               Canvas.SetLeft(myButton1, button1Left);
               Canvas.SetLeft(myRectangle1, button1Left);
               Canvas.SetLeft(myLabelButton1, button1Left);
               myRectangle1.Visibility = Visibility.Visible;
               myLeftMapItemsInActionPanelSelected.Add(leftMapItem);
               break;
            case 2:
               IMapItem? leftMapItem0 = myLeftMapItemsInActionPanel[0];
               if (null == leftMapItem0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               IMapItem? leftMapItem1 = myLeftMapItemsInActionPanel[1];
               if (null == leftMapItem1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem1 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton1, leftMapItem0, false);
               MapItem.SetButtonContent(myButton2, leftMapItem1, false);
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
               IMapItem? rightMapItem = myRightMapItemsInActionPanel[0];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton4, rightMapItem, false);
               myButton4.Visibility = Visibility.Visible;
               myRectangle4.Visibility = Visibility.Visible;
               myRightMapItemsInActionPanelSelected.Add(rightMapItem);
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
               IMapItem? leftMapItem0 = myLeftMapItemsInActionPanel[0];
               if (null == leftMapItem0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton1, leftMapItem0, GameEngine.theIsAlien);
            }
         }
         if (Visibility.Visible == myButton2.Visibility)
         {
            if (1 < myLeftMapItemsInActionPanel.Count)
            {
               IMapItem? leftMapItem1 = myLeftMapItemsInActionPanel[0];
               if (null == leftMapItem1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton2, leftMapItem1, GameEngine.theIsAlien);
            }
         }
         if (Visibility.Visible == myButton3.Visibility)
         {
            if (2 < myLeftMapItemsInActionPanel.Count)
            {
               IMapItem? leftMapItem2 = myLeftMapItemsInActionPanel[2];
               if (null == leftMapItem2)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton3, leftMapItem2, GameEngine.theIsAlien);
            }
         }

         if (Visibility.Visible == myButton4.Visibility)
         {
            if (0 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem? rightMapItem = myRightMapItemsInActionPanel[0];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton4, rightMapItem, GameEngine.theIsAlien);
            }
         }
         if (Visibility.Visible == myButton5.Visibility)
         {
            if (1 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem? rightMapItem = myRightMapItemsInActionPanel[1];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton5, rightMapItem, GameEngine.theIsAlien);
            }
         }
         if (Visibility.Visible == myButton6.Visibility)
         {
            if (2 < myRightMapItemsInActionPanel.Count)
            {
               IMapItem? rightMapItem = myRightMapItemsInActionPanel[2];
               if (null == rightMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateActionPanel() leftMapItem0 is null");
                  return;
               }
               MapItem.SetButtonContent(myButton6, rightMapItem, GameEngine.theIsAlien);
            }
         }
      }
      private bool UpdateViewMovement(IGameInstance gi)
      {
         try
         {
            foreach (IMapItemMove mim in gi.MapItemMoves)  // First return the counter and button back to its original position
            {
               IMapItem mi = mim.MapItem;
               int counterCount1 = 0;
               foreach (IMapItem mi1 in gi.Persons)
               {
                  if ((mi1.TerritoryCurrent.Name == mi.TerritoryStarting.Name) && (mi1.TerritoryCurrent.Subname == mi.TerritoryStarting.Subname))
                     ++counterCount1;
               }
               mi.TerritoryCurrent = mi.TerritoryStarting;
               mi.Location = new MapPoint(mi.TerritoryCurrent.CenterPoint.X - Utilities.theMapItemOffset + (counterCount1 * 3), mi.TerritoryCurrent.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount1 * 3));
               Button? b = myButtons.Find(mi.Name);
               if (null != b)
               {
                  MapItem.SetButtonContent(b, mi, GameEngine.theIsAlien);
                  b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                  b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                  Canvas.SetLeft(b, mi.Location.X);
                  Canvas.SetTop(b, mi.Location.Y);
                  Canvas.SetZIndex(b, counterCount1);
               }
            }
            //-----------------------------------------------
            foreach (IMapItemMove mim2 in gi.MapItemMoves) // Move it 
            {
               if (null == mim2.NewTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateViewMovement() mim2.NewTerritory is null");
                  return false;
               }
               IMapItem mi = mim2.MapItem;
               int counterCount2 = 0;
               foreach (IMapItem mi2 in gi.Persons)
               {
                  if ((mi2.TerritoryCurrent.Name == mim2.NewTerritory.Name) && (mi2.TerritoryCurrent.Subname == mim2.NewTerritory.Subname))
                     ++counterCount2;
               }
               //-----------------------------------------------
               IMapItem? alreadyMovedMapItem = myMovingMapItems.Find(mi.Name);
               if (null == alreadyMovedMapItem)
               {
                  ++myBrushIndex;
                  if (myBrushes.Count <= myBrushIndex)
                     myBrushIndex = 0;
                  myMovingMapItems.Add(mi);
                  myIsFlagSetForAlienMoveCountExceeded = false;
                  myIsFlagSetForMoveReset = false;
                  myIsFlagSetForOverstack = false;
                  myIsFlagSetForMaxMove = false;
                  StringBuilder sb1 = new StringBuilder("UpdateViewMovement():");
                  if (true == GameEngine.theIsAlien)
                     sb1.Append(" ALIEN sees moving ");
                  else
                     sb1.Append("  TP sees moving ");
                  sb1.Append(mi.Name);
                  Logger.Log(LogEnum.LE_SHOW_MIM_MOVING_COUNT, sb1.ToString());
               }
               MovePathDisplay(mim2);
               MovePathAnimate(mim2, gi.Persons);
               mi.TerritoryCurrent = mim2.NewTerritory; // Reset to its final position
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateViewMovement:  EXCEPTION THROWN e=\n{0}" + e.ToString());
            return false;
         }
         return true;
      }
      private void UpdateViewState(IGameInstance gi)
      {
         myStoryboard = null;  // turn off flashing
         GameAction outAction = GameAction.Error;
         switch (gi.GamePhase)
         {
            case GamePhase.Conversations:
               if (false == DisplayConversations(gi))
               {
                  if (true != GameEngine.theIsAlien || false != myConversationsCompleted)
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
                  if ((true == GameEngine.theIsAlien) && (false == myInfluencesCompleted))
                  {
                     myInfluencesCompleted = true;
                     outAction = GameAction.TownspersonCompletesInfluencing;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               break;
            case GamePhase.Combat:
               bool isRetreatNeedAck;
               if (false == DisplayCombats(gi, out isRetreatNeedAck))
               {
                  Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::UpdateViewState() DisplayCombats() returned falsel");
                  return;
               }
               if (false == isRetreatNeedAck)
               {
                  if (true == GameEngine.theIsAlien)
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
               bool isAckIterrogationsNeeded;
               if (false == DisplayIterogations(gi, out isAckIterrogationsNeeded))
               {
                  Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::UpdateViewState() DisplayIterogations() returned false");
                  return;
               }
               if (false == isAckIterrogationsNeeded)
               {
                  if ((false == GameEngine.theIsAlien) && (false == myInterogationsCompleted))
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
                  if ((false == GameEngine.theIsAlien) && (false == myImplateRemovalsCompleted))
                  {
                     myImplateRemovalsCompleted = true;
                     outAction = GameAction.TownspersonCompletesRemoval;
                     myGameEngine.PerformAction(ref gi, ref outAction);
                  }
               }
               break;
            case GamePhase.AlienTakeover:
               if (true == GameEngine.theIsAlien)
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
      //-------------HELPER FUNCTIONS---------------------------------
      private bool IsMoveStoppedByAlienBeforeStarted(IGameInstance gi)
      {
         //if (0 == gi.MapItemMoves.Count)
         //   return false;
         //IMapItemMove mim = gi.MapItemMoves[0];
         ////List<Stack> stacks = new List<Stack>();
         ////stacks.AssignPeople(gi.Persons, GameEngine.theIsAlien);
         //IEnumerable<Stack> results = from stack in gi.Stacks
         //                             where stack.Territory.Name == mim.OldTerritory.Name
         //                             where stack.Territory.Subname == mim.OldTerritory.Subname
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
         //myCanvasMain.Children.Add(aPolyline);

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
         //stacks.AssignPeople(persons, GameEngine.theIsAlien);
         //IEnumerable<Stack> results = from stack in stacks
         //                             where stack.Territory.Name == mim.NewTerritory.Name
         //                             where stack.Territory.Subname == mim.NewTerritory.Subname
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
         //foreach (UIElement ui in myCanvasMain.Children)
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
         //   String targetName = townspeopleControlled[0].TerritoryCurrent.Name + townspeopleControlled[0].TerritoryCurrent.Subname.ToString();
         //   foreach (UIElement ui in myCanvasMain.Children)
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
         //      UpdateActionPanel(gi, !GameEngine.theIsAlien);

         //      myLabelHeading.Visibility = Visibility.Visible;
         //      myLabelArrow.Visibility = Visibility.Visible;
         //      myTextBoxResults.Visibility = Visibility.Visible;

         //      myLabelHeading.Content = "Conversing... \"Hello.  Are you an alien?\"";
         //      myLabelLeftTop.Content = "Choose a person who is talking:";
         //      myLabelRightTop.Content = "Choose a person being talked to:";
         //   }
         //}
      }
      private bool PerformConversation(IGameInstance gi, bool isIgnoreResults)
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
            return false;
         }
         //-------------------------------------------------------------
         IMapItem? selectedLeft = myLeftMapItemsInActionPanelSelected[0];
         if (null == selectedLeft)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformConversation() myLeftMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         IMapItem? leftMapItem = gi.Stacks.FindMapItem(selectedLeft.Name);
         if (null == leftMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformConversation() leftMapItem=null");
            return false;
         }
         IMapItem? selectedRight = myRightMapItemsInActionPanelSelected[0];
         if (null == selectedRight)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformConversation() myLeftMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         if (null == myRightMapItemsInActionPanelSelected[0])
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformConversation() myRightMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         IMapItem? rightMapItem = gi.Stacks.FindMapItem(selectedRight.Name);
         if (null == rightMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformConversation() rightMapItem=null");
            return false;
         }
         //-------------------------------------------------------------
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
         return true;
      }
      private bool DisplayInfluences(IGameInstance gi)
      {
         myStoryboard = new Storyboard(); // Clear any previous flashing regions
         foreach (UIElement ui in myCanvasMain.Children)
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
            //---------------------------------------------------
            IMapItem? controlled = townspeopleControlled[0];
            if (null == controlled)
            {
               Logger.Log(LogEnum.LE_ERROR, "DisplayInfluences() townspeopleControlled[0]=null");
               continue;
            }
            String targetName = controlled.TerritoryCurrent.Name + controlled.TerritoryCurrent.Subname.ToString(); // Turn the region red
            foreach (UIElement ui in myCanvasMain.Children)
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
      private bool DisplayInfluence(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();
         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() selectedTerritory=null");
            return false;
         }
         IStack? stack = gi.Stacks.Find(selectedTerritory);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() stack=null");
            return false;
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
               UpdateActionPanel(gi, !GameEngine.theIsAlien);
               //----------------------------------------------------------------------
               for (int i = 0; i < myLeftMapItemsInActionPanel.Count; ++i)
               {
                  IMapItem? leftMi = myLeftMapItemsInActionPanel[i];
                  if (null == leftMi)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() myLeftMapItemsInActionPanel[" + i + "]=null");
                     continue;
                  }
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
                  IMapItem? rightMi = myRightMapItemsInActionPanel[i];
                  if (null == rightMi)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() myRightMapItemsInActionPanel[" + i + "]=null");
                     return false;
                  }
                  if (true == rightMi.IsSkeptical)
                  {
                     switch (i)
                     {
                        case 0: myLabelButton4.Visibility = Visibility.Visible; myLabelButton4.Content = "Skeptical"; break;
                        case 1: myLabelButton5.Visibility = Visibility.Visible; myLabelButton5.Content = "Skeptical"; break;
                        case 2: myLabelButton6.Visibility = Visibility.Visible; myLabelButton6.Content = "Skeptical"; break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() reached default i=" + i.ToString());
                           return false;
                     }
                  }
                  if (true == rightMi.IsWary)
                  {
                     switch (i)
                     {
                        case 0: myLabelButton4.Visibility = Visibility.Visible; myLabelButton4.Content = "Wary"; break;
                        case 1: myLabelButton5.Visibility = Visibility.Visible; myLabelButton5.Content = "Wary"; break;
                        case 2: myLabelButton6.Visibility = Visibility.Visible; myLabelButton6.Content = "Wary"; break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "DisplayInfluence() reached default i=" + i.ToString());
                           return false;
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
         return true;
      }
      private bool PerformInfluence(IGameInstance gi, bool isIgnoreResults)
      {
         if ((0 == myLeftMapItemsInActionPanelSelected.Count) || (0 == myRightMapItemsInActionPanelSelected.Count))
         {
            StringBuilder sb = new StringBuilder("Perform_Influence(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return false;
         }
         //-----------------------------------------------------------------------------
         double totalInfluence = 0;
         bool isImplantHeld = false;
         foreach (IMapItem mi in myLeftMapItemsInActionPanelSelected)
         {
            IMapItem? leftMi = myLeftMapItemsInActionPanelSelected[0];
            if (null == leftMi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_Influence(): myLeftMapItemsInActionPanelSelected[0]=null");
               return false;
            }
            leftMi.IsInfluencedThisTurn = true;
            totalInfluence += (double)mi.Influence;
            if (true == mi.IsImplantHeld)
               isImplantHeld = true;
         }
         //-----------------------------------------------------------------------------
         IMapItem? rightMapItem = myRightMapItemsInActionPanelSelected[0];
         if (null == rightMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_Influence(): rightMapItem=null");
            return false;
         }
         rightMapItem.IsInfluencedThisTurn = true;
         //-----------------------------------------------------------------------------
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
            //-----------------------------------------------------------------------------
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
            //-----------------------------------------------------------------------------
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
            if (dieThreshold <= sum) // Check for alien.  If alien, let user know it is discovered. Else, make the townsperson controlled.
            {
               if (true == rightMapItem.IsAlienUnknown)
               {
                  if (false == gi.AddKnownAlien(rightMapItem))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Perform_Influence(): AddKnownAlien() returned error");
                     return false;
                  }
                  displayResults.Append(" is an Alien!!!!!!");
               }
               else
               {
                  if (false == gi.AddTownperson(rightMapItem))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Perform_Influence(): AddTownperson() returned error");
                     return false;
                  }
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
         }
         //-----------------------------------------------------------------------------
         GameAction outAction = GameAction.TownspersonPerformsInfluencing;
         myGameEngine.PerformAction(ref gi, ref outAction);
         if (true == isIgnoreResults)
            ClearActionPanel();
         else
            UpdateActionPanelButtons(gi);
         return true;

      }
      private bool DisplayCombats(IGameInstance gi, out bool isRetreatNeedAck)
      {
         // This method collects all possible combats in the respective containers 
         // <myTerritoriesCombatForAlien> or <myTerritoriesCombatForTownsperson>.  It turns 
         // the spaces red and causes them to flash as an indication to the user that they 
         // can be selected.  This function returns true if there are any possible combats
         // or retreats from previous combats.
         //----------------------------------------------------------------------
         isRetreatNeedAck = false;
         myStoryboard = new Storyboard();
         foreach (UIElement ui in myCanvasMain.Children) // Clear any previous flashing regions
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               p1.Fill = mySolidColorBrushClear;
            }
         }
         if (true == GameEngine.theIsAlien)
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
            ITerritory? combatTerritory = null;
            if (true == GameEngine.theIsAlien)
            {
               if (0 == aliens.Count)
                  continue;
               IMapItem? alien = aliens[0];
               if (null == alien)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Display_Combats() aliens[0]=null");
                  return false;
               }
               combatTerritory = alien.TerritoryCurrent;
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
               IMapItem? controlledMapItem = controlled[0];
               if (null == controlledMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Display_Combats() controlled[0]=null");
                  return false;
               }
               combatTerritory = controlledMapItem.TerritoryCurrent;
               if ((0 == aliens.Count) && ((0 == uncontrolled.Count) && (0 == unknownAliens.Count)))
                  continue;
            }
            //------------------------------------------------------------------------------
            if (true == GameEngine.theIsAlien)
               myTerritoriesCombatForAlien.Add(combatTerritory);
            else
               myTerritoriesCombatForTownsperson.Add(combatTerritory);
            //------------------------------------------------------------------------------
            String targetName = combatTerritory.Name + combatTerritory.Subname.ToString(); // Turn the region red
            foreach (UIElement ui in myCanvasMain.Children)
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
            //------------------------------------------------------------------------------
            DoubleAnimation anim = new DoubleAnimation();  // Perform animiation on the region
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         }
         //------------------------------------------------------------------------------
         if (0 == myStoryboard.Children.Count)
         {
            if (null == gi.MapItemCombat)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::Display_Combats() gi.MapItemCombat is null");
               return false;
            }
            if (true == gi.MapItemCombat.IsAnyRetreat) // If the previous combat had retreats, do not assume combats are completed until the player explicitly indicates it with menu command. This allows them to see the retreats.
               isRetreatNeedAck = true;
            return true;
         }
         myStoryboard.Begin(this);
         return true;
      }
      private void DisplayCombat(IGameInstance gi, ITerritory selectedTerritory)
      {
         ClearActionPanel();
         if (null == selectedTerritory)  // If passed-in territory is not null, user has selected this region. Show a dialog of the conversation results.
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayTakover() selectedTerritory=null");
            return;
         }
         if (true == GameEngine.theIsAlien) // Only handle this mouse click if the selected territory is one where combat can occur.
         {
            if (null == myTerritoriesCombatForAlien.Find(selectedTerritory.Name))
               return;
         }
         else
         {
            if (null == myTerritoriesCombatForTownsperson.Find(selectedTerritory.Name))
               return;
         }
         //-------------------------------------------------------------------
         IMapItems aliens = new MapItems();
         IMapItems controlled = new MapItems();
         IMapItems uncontrolled = new MapItems();
         IMapItems wary = new MapItems();
         foreach (MapItem mi in gi.Persons)
         {
            if ((selectedTerritory.Name == mi.TerritoryCurrent.Name) && (selectedTerritory.Subname == mi.TerritoryCurrent.Subname))
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
         //-------------------------------------------------------------------
         if (0 == controlled.Count) // If there is no combat, return from this method
         {
            if ((0 == aliens.Count) || (0 == wary.Count))
               return;
         }
         if (true == GameEngine.theIsAlien) // Setup the action pane.
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
         //-------------------------------------------------------------------
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
      private bool PerformCombat(IGameInstance gi, bool isIgnoreResults)
      {
         if( null == myGameEngine )
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::Perform_Combat(): myGameEngine is null");
            return false;
         }
         if (true == isIgnoreResults)
         {
            ClearActionPanel();
            return true;
         }
         if ((0 == myLeftMapItemsInActionPanel.Count) || (0 == myRightMapItemsInActionPanel.Count))
         {
            StringBuilder sb = new StringBuilder("GameViewerWindow::Perform_Combat(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            sb.Append(" myLeftSelected=");
            sb.Append(myLeftMapItemsInActionPanelSelected.Count.ToString());
            sb.Append(" myRightSelected=");
            sb.Append(myRightMapItemsInActionPanelSelected.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return false;
         }
         //-----------------------------------------------------------------------------
         if ((false == myIsCombatInitiatedForAlien) && (false == myIsCombatInitiatedForTownsperson)) // Only initiate combat if there is not an outstanding combat happening.
         {

            IMapItem? leftMapItem = myLeftMapItemsInActionPanel[0];
            if( null == leftMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::Perform_Combat(): myLeftMapItemsInActionPanel[0]=null");
               return false;
            }
            if( null == gi.MapItemCombat)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::Perform_Combat(): gi.MapItemCombat=null");
               return false;
            }
            gi.MapItemCombat.Territory = leftMapItem.TerritoryCurrent;
            if (true == GameEngine.theIsAlien)
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
         }
         return true;
      }
      private void DisplayCombatResults(IGameInstance gi)
      {
         ClearActionPanel();
         if (null == gi.MapItemCombat)
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayCombatResults() gi.MapItemCombat=null");
            return;
         }
         int totalCombatForAttacker = 0;
         int numAttackers = 0;
         IMapItems leftMapItems = new MapItems();
         foreach (IMapItem mi in gi.MapItemCombat.Attackers)
            leftMapItems.Add(mi);
         leftMapItems = leftMapItems.SortOnCombat();
         //-----------------------------------------------------------------------------
         foreach (IMapItem mi in leftMapItems)
         {
            myLeftMapItemsInActionPanel.Add(mi);
            myLeftMapItemsInActionPanelSelected.Add(mi);
            totalCombatForAttacker += mi.Combat;
            if (3 <= ++numAttackers)
               break;
         }
         //-----------------------------------------------------------------------------
         int totalCombatForDefender = 0;
         int numDefenders = 0;
         IMapItems rightMapItems = new MapItems();
         foreach (IMapItem mi in gi.MapItemCombat.Defenders)
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
         //-----------------------------------------------------------------------------
         if ((0 == myLeftMapItemsInActionPanel.Count) || (0 == myRightMapItemsInActionPanel.Count))
         {
            StringBuilder sb = new StringBuilder("DisplayCombatResults(): myLeft=");
            sb.Append(myLeftMapItemsInActionPanel.Count.ToString());
            sb.Append(" myRight=");
            sb.Append(myRightMapItemsInActionPanel.Count.ToString());
            Logger.Log(LogEnum.LE_ERROR, sb.ToString());
            return;
         }
         //-----------------------------------------------------------------------------
         UpdateActionPanel(gi, true);
         myLabelHeading.Visibility = Visibility.Visible;
         myLabelArrow.Visibility = Visibility.Visible;
         Logger.Log(LogEnum.LE_SHOW_COMBAT_THREAD, "DisplayCombatResults() myTextBoxResults.Visibility = Visibility.Visible");
         myLabelHeading.Content = "Combat Results";
         myLabelLeftTop.Content = "Attackers:";
         myLabelRightTop.Content = "Defenders:";
         //-----------------------------------------------------------------------------
         StringBuilder displayResults = new StringBuilder();
         displayResults.Append("Total Attacker Combat Factors=");
         displayResults.Append(totalCombatForAttacker.ToString());
         displayResults.Append("\nTotal Defender Combat Factors=");
         displayResults.Append(totalCombatForDefender.ToString());
         int differenceInCombat = totalCombatForAttacker - totalCombatForDefender;
         displayResults.Append("\nDifference: ");
         displayResults.Append(differenceInCombat.ToString());
         //-----------------------------------------------------------------------------
         int die1 = gi.MapItemCombat.DieRoll1;
         int die2 = gi.MapItemCombat.DieRoll2;
         displayResults.Append("\nRoll: ");
         displayResults.Append(die1.ToString());
         displayResults.Append(" + ");
         displayResults.Append(die2.ToString());
         displayResults.Append(" => ");
         displayResults.Append(gi.MapItemCombat.Result.ToString());
         //-----------------------------------------------------------------------------
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
         if( null == gi.MapItemCombat)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombatRetreat() gi.MapItemCombat=null");
            return;
         }
         if (null != gi.MapItemCombat.Territory)
            UpdateViewMovement(gi); // Show retreats
         UpdateViewState(gi);
         myIsCombatInitiatedForTownsperson = false;
         StringBuilder sb1 = new StringBuilder("UpdateView():TownspersonPerformCombat: "); 
         sb1.Append(GameEngine.theIsAlien.ToString()); 
         sb1.Append("myIsCombatInitiatedForTownsperson=false");
         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb1.ToString());
         if (true == isIgnoreResults)
            ClearActionPanel();
      }
      private bool DisplayIterogations(IGameInstance gi, out bool isInterrogations)
      {
         isInterrogations = false;
         myStoryboard = new Storyboard();
         foreach (UIElement ui in myCanvasMain.Children) // Clear any previous flashing regions
         {
            if (ui is Polygon)
            {
               Polygon p1 = (Polygon)ui;
               if( null == p1.Tag)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DisplayIterogations() Polygon.Tag=null");
                  return false;
               }
               string tagString = (string)p1.Tag;
               if( null == tagString)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DisplayIterogations() Polygon.Tag.ToString()=null");
                  return false;
               }  
               ITerritory? t = gi.ZebulonTerritories.Find(tagString);
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
            IMapItem? controlled = townspeopleControlled[0];
            if( null == controlled)
            {
               Logger.Log(LogEnum.LE_ERROR, "DisplayIterogations() townspeopleControlled[0]=null");
               return false;
            }  
            String targetName = controlled.TerritoryCurrent.Name + controlled.TerritoryCurrent.Subname.ToString();
            foreach (UIElement ui in myCanvasMain.Children) // Turn the region red
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
         foreach (UIElement ui in myCanvasMain.Children) // Clear any previous flashing regions
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
            IMapItems controlledMapItems = new MapItems();
            IMapItems aliens = new MapItems();
            foreach (MapItem mi in stack.MapItems) // In each stack, get the count in the stack of the number of aliens  and controlled townspeople
            {
               if ((true == mi.IsImplantRemovalThisTurn) || (true == mi.IsKilled))
                  continue;
               if ((true == mi.IsControlled) && (true == mi.IsConscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  controlledMapItems.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsConscious)))
                  aliens.Add(mi);
            }
            if ((0 == controlledMapItems.Count) || (0 == aliens.Count))
               continue;
            //-------------------------------------------------------------- 
            IMapItem? controlledMapItem = controlledMapItems[0];
            if( null == controlledMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "DisplayImplantRemovals() controlledMapItems[0]=null");
               return false;
            }
            String targetName = controlledMapItem.TerritoryCurrent.Name + controlledMapItem.TerritoryCurrent.Subname.ToString();  // Turn the region red
            foreach (UIElement ui in myCanvasMain.Children)
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
         if (null == selectedTerritory)  // Show a dialog of the conversation results.
         {
            Logger.Log(LogEnum.LE_ERROR, "DisplayImplantRemoval() selectedTerritory=null");
            return;
         }
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
               if ((true == mi.IsImplantRemovalThisTurn) || (true == mi.IsKilled))
                  continue;

               if ((true == mi.IsControlled) && (true == mi.IsConscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  myLeftMapItemsInActionPanel.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsConscious)))
                  myRightMapItemsInActionPanel.Add(mi);
            }

            if ((0 != myLeftMapItemsInActionPanel.Count) && (0 != myRightMapItemsInActionPanel.Count))
            {
               UpdateActionPanel(gi, !GameEngine.theIsAlien);
               myLabelHeading.Visibility = Visibility.Visible;
               myLabelArrow.Visibility = Visibility.Visible;
               myTextBoxResults.Visibility = Visibility.Visible;
               myLabelHeading.Content = "Remove Implant to Hold Evidence of Alien Takeover";
               myLabelLeftTop.Content = "Choose a person who is removing implant:";
               myLabelRightTop.Content = "Choose a person to have implant removed:";
            }
         }
      }
      private bool PerformImplantRemoval(IGameInstance gi, bool isIgnoreResults)
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
            return false;
         }
         //-----------------------------------------------------------------------------
         IMapItem? leftMapItem = myLeftMapItemsInActionPanelSelected[0];
         if( null == leftMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::PerformImplantRemoval(): myLeftMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         leftMapItem.IsImplantRemovalThisTurn = true;
         //-----------------------------------------------------------------------------
         IMapItem? rightMapItem = myRightMapItemsInActionPanelSelected[0];
         if (null == rightMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::PerformImplantRemoval(): myRightMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         rightMapItem.IsImplantRemovalThisTurn = true;
         //-----------------------------------------------------------------------------
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
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckForImplantRemoval() returned error");
                     return false;
                  }
                  break;
               case 11: // Implant usuable
               case 12:
                  displayResults.Append("\nImplant is removed intact! You now have evidence.");
                  if (false == gi.AddTownperson(rightMapItem))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CheckForImplantRemoval() returned error");
                     return false;
                  }
                  leftMapItem.IsImplantHeld = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "CheckForImplantRemoval() reached default dr=" + sum.ToString());
                  return false;
            }
            myTextBoxResults.Text = displayResults.ToString();
         }
         //-----------------------------------------------------------------------------
         if (true == isIgnoreResults)
            ClearActionPanel();
         else
            UpdateActionPanelButtons(gi);
         return true;
      }
      private bool DisplayTakeovers(IGameInstance gi, ITerritory? selectedTerritory = null)
      {
         if (false == GameEngine.theIsAlien)
         {
            myStoryboard = null; // turn off any flashing spaces
            return false;
         }
         //----------------------------------------------------------------------
         myStoryboard = new Storyboard(); // Clear any previous flashing regions
         foreach (UIElement ui in myCanvasMain.Children)
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
            //-----------------------------------------------------------
            IMapItem? possibleVictum = possibleVictums[0];
            if( null == possibleVictum)
            {
               Logger.Log(LogEnum.LE_ERROR, "DisplayTakeovers() possibleVictums[0]=null");
               return false;
            }  
            String targetName = possibleVictum.TerritoryCurrent.Name + possibleVictum.TerritoryCurrent.Subname.ToString();  // Turn the region orange
            foreach (UIElement ui in myCanvasMain.Children)
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
            //-----------------------------------------------------------
            DoubleAnimation anim = new DoubleAnimation();  // Perform animiation on the region
            anim.From = 0.7;
            anim.To = 0.2;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.6));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)
           //-------------------------------------------------------------------------------------------------
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
         if (null == selectedTerritory) // If passed-in territory is not null, user has selected this region. Show a dialog of the conversation results.
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
               UpdateActionPanel(gi, GameEngine.theIsAlien);
               myLabelHeading.Visibility = Visibility.Visible;
               myLabelArrow.Visibility = Visibility.Visible;
               myTextBoxResults.Visibility = Visibility.Visible;
               myLabelHeading.Content = "Takeover... \"You will be assimulated.\"";
               myLabelLeftTop.Content = "Choose an alien who is assimulating:";
               myLabelRightTop.Content = "Choose a person being assimulated:";
            }
         }
      }
      private bool PerformTakeover(IGameInstance gi, bool isIgnoreResults)
      {
         if( null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::PerformTakeover(): myGameEngine is null");
            return false;
         }
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
            return false;
         }
         //-----------------------------------------------------------------------------
         IMapItem? alien = myLeftMapItemsInActionPanelSelected[0];
         if( null == alien)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::PerformTakeover(): myLeftMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         alien.IsTakeoverThisTurn = true;
         IMapItem? victum = myLeftMapItemsInActionPanelSelected[0];
         if( null == victum)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow::PerformTakeover(): myRightMapItemsInActionPanelSelected[0]=null");
            return false;
         }
         victum.IsTakeoverThisTurn = true;
         //-----------------------------------------------------------------------------
         if (false == isIgnoreResults)
            gi.Takeover = new MapItemTakeover(alien, victum);
         if (true == isIgnoreResults)
            ClearActionPanel();
         //-----------------------------------------------------------------------------
         GameAction outAction = GameAction.AlienTakeover;
         myGameEngine.PerformAction(ref gi, ref outAction);
         return true;
      }
      private bool PerformTakeoverObserved(IGameInstance gi)
      {
         ClearActionPanel();
         if( null == gi.Takeover)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTakeoverObserved() gi.Takeover=null");
            return false;
         }
         myLeftMapItemsInActionPanel.Add(gi.Takeover.Alien);
         myRightMapItemsInActionPanel.Add(gi.Takeover.Uncontrolled);
         UpdateActionPanel(gi, !GameEngine.theIsAlien);

         myLabelHeading.Visibility = Visibility.Visible;
         myLabelArrow.Visibility = Visibility.Visible;
         myTextBoxResults.Visibility = Visibility.Visible;

         myLabelHeading.Content = "Takover... \"You will be assimulated.\"";
         myLabelLeftTop.Content = "Alien who is assimulating:";
         myLabelRightTop.Content = "Person being assimulated:";

         myTextBoxResults.Text = gi.Takeover.Observations;
         UpdateActionPanelButtons(gi);
         return true;
      }
      //-------------CONTROLLER FUNCTIONS---------------------------------
      private void MouseLeftButtonDownMarquee(object sender, MouseEventArgs e)
      {
         myStoryboardMarquee.Pause(this);
      }
      private void MouseLeftButtonUpMarquee(object send, MouseEventArgs e)
      {
         myStoryboardMarquee.Resume(this);
      }
      private void MouseRightButtonDownMarquee(object send, MouseEventArgs e)
      {
         if (2.5 < mySpeedRatioMarquee)
            mySpeedRatioMarquee = 0.25;
         else if ((1.8 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 2.2))
            mySpeedRatioMarquee = 3.0;
         else if ((0.8 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 1.2))
            mySpeedRatioMarquee = 2.0;
         else if ((0.3 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 0.6))
            mySpeedRatioMarquee = 1.0;
         else
            mySpeedRatioMarquee = 0.5;
         myStoryboardMarquee.SetSpeedRatio(this, mySpeedRatioMarquee);
      }
      private void myTextBoxEntryTextChanged(object sender, TextChangedEventArgs e)
      {
         //if (null != myGameEngine)
         //{
         //   string entry = myTextBoxEntry.Text;  // Do not do anything unless a carriage return happens
         //   int length = entry.Count();
         //   if (0 == length)
         //      return;
         //   if ('\n' == entry[length - 1])
         //   {
         //      myTextBoxEntry.Text = "";
         //      StringBuilder sb = new StringBuilder("You say: ");
         //      sb.Append(entry);
         //      myTextBoxDisplay.AppendText(sb.ToString());
         //      myTextBoxDisplay.ScrollToEnd();
         //      //myGameEngine.SendText(entry);
         //   }
         //}
      }
      private void ClickMapItem(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickMapItem() myGameInstance=null");
            return;
         }
         IGameInstance gi = myGameInstance;
         if (sender is Button)
         {
            Button selectedButton = (Button)sender;
            IMapItem? selectedMapItem = gi.Stacks.FindMapItem(selectedButton.Name);
            if (null == selectedMapItem)
            {
               Console.WriteLine("Selected Map Item {0} no longer found", selectedButton.Name);
               return;
            }
            if (true == selectedMapItem.IsKilled)  // If killed, do nothing
            {
               if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                  Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
               return;
            }
            //--------------------------------------
            switch (gi.GamePhase)
            {
               case GamePhase.AlienMovement:
                  if (null == myMovingButton)
                  {
                     if (false == GameEngine.theIsAlien)
                     {
                        if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                           Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
                        return;  // do nothing
                     }
                     if (4 < myMovingMapItems.Count)
                     {
                        if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
                           return;
                        }
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
                        if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                           Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
                        return;
                     }
                  }
                  break;
               case GamePhase.TownspersonMovement:
                  if (null == myMovingButton)
                  {
                     if ((("Townsperson Selects Counter to Move" != gi.NextAction) || (true == GameEngine.theIsAlien)
                         || (false == selectedMapItem.IsConscious) || (false == selectedMapItem.IsControlled) || (true == selectedMapItem.IsKilled)
                         || (true == selectedMapItem.IsStunned) || (true == selectedMapItem.IsTiedUp) || (true == myIsAlienAbleToStopMove)))
                     {
                        if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                           Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
                        return;
                     }
                  }
                  break;
               default:
                  MapItemCommonAction(selectedMapItem.TerritoryCurrent);
                  return;
            }
            //--------------------------------------
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
                  if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                     Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
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
                  if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                     Logger.Log(LogEnum.LE_ERROR, "ClickMapItem(): RotateStack() returned false");
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
               if (selectedButton.Name == myMovingButton.Name) // case: MapItem already selected to move -- clicking the moving button again causes it to be unhighlighted
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
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickMapItem() myGameInstance=null");
            return;
         }
         IGameInstance gi = myGameInstance;
         Point p = e.GetPosition(myCanvasMain);  // not used but useful info
         // There is already a moving button.  Do not do any actions until
         // the alien player responds or there is a timeout on the alien response.
         // When that happens, myIsAlienAbleToStopMove=false.
         if (true == myIsAlienAbleToStopMove)
            return;
         //--------------------------------------------------
         // Get the selected territory
         ITerritory? selectedTerritory = null;
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Polygon)
            {
               Polygon aPolygon = (Polygon)ui;
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territories.theTerritories)
                  {
                     if( null == t )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCanvas() t=null in Territories.theTerritories");
                        continue;
                     }
                     string? tName  = t.ToString();
                     if( true == string.IsNullOrEmpty(tName))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCanvas() t.ToString() is null or empty for territory in Territories.theTerritories");
                        continue;
                     }
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(tName))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }
            } 
            if (null != selectedTerritory)
               break;
         }  
         if (null == selectedTerritory)  // If no territory is selected, return
            return;
         //-----------------------------------------------------
         switch (gi.GamePhase)
         {
            case GamePhase.AlienMovement:
               if ((true == GameEngine.theIsAlien) && (null != myMovingButton))
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               else
                  RotateStack(selectedTerritory);
               break;
            case GamePhase.TownspersonMovement:
               if ((false == GameEngine.theIsAlien) && (null != myMovingButton))
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               else
                  RotateStack(selectedTerritory);
               break;
            default:
               MapItemCommonAction(selectedTerritory);
               break;
         }
      }
      private void MouseRightButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {

         Point p = e.GetPosition(myCanvasMain);  // not used but useful info
         //--------------------------------------------------
         ITerritory? selectedTerritory = null;  // Get the selected territory
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Polygon)
            {
               Polygon aPolygon = (Polygon)ui;
               if (true == aPolygon.IsMouseOver)
               {
                  foreach (ITerritory t in Territories.theTerritories)
                  {
                     if (null == t)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCanvas() t=null in Territories.theTerritories");
                        continue;
                     }
                     string? tName = t.ToString();
                     if (true == string.IsNullOrEmpty(tName))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCanvas() t.ToString() is null or empty for territory in Territories.theTerritories");
                        continue;
                     }
                     if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(tName))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
               }
            } 
            if (null != selectedTerritory)
               break;
         }  
         if (null == selectedTerritory)  // If no territory is selected, return
            return;
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
            if( false == MapItemReturnToStart(selectedButton))
               Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded() MapItemReturnToStart() returned error");
         }
      }
      private void ContextMenuLoaded(object sender, RoutedEventArgs e)
      {
         if( null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded() myGameInstance=null");
            return;
         }
         //--------------------------------------------------
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
               IMapItem? mi = myGameInstance.Stacks.FindMapItem(b.Name);
               if( null == mi )
               {
                  Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded(): myGameInstance.Stacks.FindMapItem() returned null for name=" + b.Name);
                  return;
               }
               // Gray out the "Retun to Starting Point" menu item
               if ((0 < cm.Items.Count) && (true == mi.IsMoveAllowedToResetThisTurn))
               {
                  if (cm.Items[0] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[0];
                     if ((true == GameEngine.theIsAlien) && (GamePhase.AlienMovement == myGameInstance.GamePhase) && (true == mi.IsMoved))
                        menuItem.IsEnabled = true;
                     else if ((false == GameEngine.theIsAlien) && (GamePhase.TownspersonMovement == myGameInstance.GamePhase) && (true == mi.IsMoved))
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
               //      stacks.AssignPeople(gi.Persons, GameEngine.theIsAlien);
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
                     bool isMenuEnabled;
                     if( false == IsAlienAbleToStopMove(myGameInstance, mi, out isMenuEnabled))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded(): IsAlienAbleToStopMove() returned false");
                        return;
                     }
                     menuItem.IsEnabled = isMenuEnabled;
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
                  if (false == MapItemReturnToStart(b))
                     Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickReturnToStart(): MapItemReturnToStart() returned error");
               }
            }
         }
      }
      private void ContextMenuClickRotate(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded(): myGameInstance=null");
            return;
         }
         //--------------------------------------------------
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(b.Name);
                  if (null == selectedMapItem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickRotate() selectedMapItem=null for name=" + b.Name);
                     return;
                  }
                  this.RotateStack(selectedMapItem.TerritoryCurrent);
               }
            }
         }
      }
      private void ContextMenuClickExposeAlien(object sender, RoutedEventArgs e)
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
                  IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(b.Name);
                  if (null == selectedMapItem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickExposeAlien() selectedMapItem=null for name=" + b.Name);
                     return;
                  }
                  if (true == selectedMapItem.IsAlienUnknown)
                  {
                     if (false == myGameInstance.AddKnownAlien(selectedMapItem))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickExposeAlien(): returned error");
                        return;
                     }
                     GameAction outAction = GameAction.ShowAlien;
                     myGameEngine.PerformAction(ref myGameInstance, ref outAction); // Inform the user to return back
                  }
               }
            }
         }
      }
      private void ContextMenuClickStopMove(object sender, RoutedEventArgs e)
      {
#pragma warning disable CA1416 // Validate platform compatibility
         myTimer.Stop();
#pragma warning restore CA1416 // Validate platform compatibility
         if (sender is MenuItem)
         {
            MenuItem mi = (MenuItem)sender;
            if (mi.Parent is ContextMenu)
            {
               ContextMenu cm = (ContextMenu)mi.Parent;
               if (cm.PlacementTarget is Button)
               {
                  Button b = (Button)cm.PlacementTarget;
                  IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(b.Name);
                  if (null != selectedMapItem)
                  {
                     if (((true == selectedMapItem.IsAlienUnknown) || (true == selectedMapItem.IsAlienKnown)) && (true == myIsAlienAbleToStopMove) && (false == selectedMapItem.IsMoveStoppedThisTurn))
                     {
                        if (false == myGameInstance.AddKnownAlien(selectedMapItem))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove(): returned error");
                           return;
                        }
                        selectedMapItem.IsMoveStoppedThisTurn = true;
                        if (0 < myGameInstance.MapItemMoves.Count) // Reset the moving MapItem
                        {
                           IMapItemMove? mim = myGameInstance.MapItemMoves[0];
                           if( null == mim)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove() myGameInstance.MapItemMoves[0]=null");
                              return;
                           }
                           if (null == mim.MapItem)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove() mim.MapItem=null");
                              return;
                           }
                           if (null == mim.BestPath)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove() mim.BestPath=null");
                              return;
                           }
                           mim.MapItem.TerritoryCurrent = mim.MapItem.TerritoryStarting;
                           mim.MapItem.IsMoveStoppedThisTurn = true;
                           mim.MapItem.MovementUsed -= mim.BestPath.Territories.Count;
                           if (mim.MapItem.MovementUsed <= 0)
                           {
                              mim.MapItem.MovementUsed = 0;
                              mim.MapItem.IsMoved = false;
                           }
                           //--------------------------------
                           IMapItemMove modifiedMove = new MapItemMove(Territories.theTerritories, mim.MapItem, selectedMapItem.TerritoryCurrent); // Change to modified MapItemMove
                           myGameInstance.MapItemMoves[0] = modifiedMove;
                           mim.MapItem.MovementUsed = mim.MapItem.Movement; // ensure cannot move further
                           if( false == UpdateCanvasMain(myGameInstance, GameAction.AlienStopsTownspersonMovement, true))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ContextMenuClickStopMove() Update_CanvasMain() returned false");
                              return;
                           }
                           //--------------------------------
                           GameAction outAction = GameAction.AlienModifiesTownspersonMovement;
                           myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                        }
                     }
                  }
               }
            }
         }
      }
      private void TimerElasped(object? sender, EventArgs e)
      {
         Logger.Log(LogEnum.LE_TIMER_ELAPED, "TimerElasped() called");
         if (true == myIsAlienAbleToStopMove)
         {
            myIsAlienAbleToStopMove = false;
            Logger.Log(LogEnum.LE_TIMER_ELAPED, "TimerElasped():  Reset State myIsAlienAbleToStopMove=false");
#pragma warning disable CA1416 // Validate platform compatibility
            myTimer.Stop();
#pragma warning restore CA1416 // Validate platform compatibility
            //-------------------------------
            GameAction outAction = GameAction.AlienTimeoutOnMovement;
            myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         }
      }
      private void myButton1_Click(object sender, RoutedEventArgs e)
      {
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myLeftMapItemsInActionPanel[0];
         if( null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton1_Click() myLeftMapItemsInActionPanel[0]=null");
            return;
         }  
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
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myLeftMapItemsInActionPanel[1];
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton1_Click() myLeftMapItemsInActionPanel[0]=null");
            return;
         }
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
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myLeftMapItemsInActionPanel[2];
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton3_Click() myLeftMapItemsInActionPanel[0]=null");
            return;
         }
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
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myRightMapItemsInActionPanel[0];
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton3_Click() myLeftMapItemsInActionPanel[0]=null");
            return;
         }
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
         //-----------------------------------------------------------  
         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton5_Click(object sender, RoutedEventArgs e)
      {
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myRightMapItemsInActionPanel[1];
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton3_Click() myLeftMapItemsInActionPanel[0]=null");
            return;
         }
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
         //-----------------------------------------------------------  
         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButton6_Click(object sender, RoutedEventArgs e)
      {
         switch (myGameInstance.GamePhase)
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
         //-----------------------------------------------------------  
         IMapItem? mi = myRightMapItemsInActionPanel[2];
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton6_Click() myRightMapItemsInActionPanel[2]=null");
            return;
         }
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
         //-----------------------------------------------------------  
         if ((0 < myLeftMapItemsInActionPanelSelected.Count) && (0 < myRightMapItemsInActionPanelSelected.Count))
            myButtonOk.IsEnabled = true;
         else
            myButtonOk.IsEnabled = false;
      }
      private void myButtonOk_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButton6_Click() myGameInstance=null");
            return;
         }
         myButtonOk.Visibility = Visibility.Hidden;
         myButtonIgnore.Visibility = Visibility.Hidden;
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.Conversations: PerformConversation(myGameInstance, false); break;
            case GamePhase.ImplantRemoval: PerformImplantRemoval(myGameInstance, false); break;
            case GamePhase.AlienTakeover: PerformTakeover(myGameInstance, false); break;
            case GamePhase.Influences: PerformInfluence(myGameInstance, false); break;
            case GamePhase.Combat:
               {
                  if ("" == myTextBoxResults.Text)
                     PerformCombat(myGameInstance, false);
                  else
                     PerformCombatRetreat(myGameInstance, false);
                  break;
               }
            default: break;
         }
         UpdateViewState(myGameInstance);
      }
      private void myButtonIgnoreClick(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "myButtonIgnoreClick() myGameInstance=null");
            return;
         }
         myButtonOk.Visibility = Visibility.Hidden;
         myButtonIgnore.Visibility = Visibility.Hidden;
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.Conversations: PerformConversation(myGameInstance, true); break;
            case GamePhase.ImplantRemoval: PerformImplantRemoval(myGameInstance, true); break;
            case GamePhase.AlienTakeover: PerformTakeover(myGameInstance, true); break;
            case GamePhase.Influences: PerformInfluence(myGameInstance, true); break;
            case GamePhase.Combat:
               {
                  if ("" == myTextBoxResults.Text)
                     PerformCombat(myGameInstance, true);
                  else
                     PerformCombatRetreat(myGameInstance, true);
                  break;
               }
            default: break;
         }
         UpdateViewState(myGameInstance);
      }
      private void GameViewerWindowClosed(object sender, EventArgs e)
      {
         Application app = Application.Current;
         app.Shutdown();
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private void MapItemCommonAction(ITerritory selectedTerritory)
      {
         //----------------------------------------
         myStoryboard = null;
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.Conversations:
               if (false == GameEngine.theIsAlien)
                  DisplayConversation(myGameInstance, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.Influences:
               if (false == GameEngine.theIsAlien)
                  DisplayInfluence(myGameInstance, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.Combat:
               DisplayCombat(myGameInstance, selectedTerritory);
               return;
            case GamePhase.Iterrogations:
               if (false == GameEngine.theIsAlien)
               {
                  if ((true == selectedTerritory.IsBuilding()) && (null != myGameInstance.ZebulonTerritories.Find(selectedTerritory.Name)) && (0 < myGameInstance.NumIterogationsThisTurn))
                  {
                     --myGameInstance.NumIterogationsThisTurn;
                     myGameInstance.ZebulonTerritories.Remove(selectedTerritory);
                     IMapItem? zebulon = myGameInstance.Stacks.FindMapItem("Zebulon");
                     if( null == zebulon)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MapItemCommonAction() myGameInstance.Stacks.FindMapItem(\"Zebulon\") returned null");
                        return;
                     }
                     if ((zebulon.TerritoryCurrent.Name == selectedTerritory.Name) && (zebulon.TerritoryCurrent.Subname == selectedTerritory.Subname))
                     {
                        zebulon.IsAlienKnown = true;
                        myGameInstance.NumIterogationsThisTurn = 0;
                     }
                     StringBuilder sb = new StringBuilder("MouseLeftButtonDownCanvas(): "); 
                     sb.Append(myGameInstance.NumIterogationsThisTurn.ToString()); 
                     sb.Append("). picked "); 
                     sb.Append(selectedTerritory.ToString());
                     Logger.Log(LogEnum.LE_SHOW_ITEROGATIONS, sb.ToString());
                     //------------------------------------------------------
                     GameAction outAction = GameAction.TownspersonIterrogates;
                     myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  }
               }
               return;
            case GamePhase.ImplantRemoval:
               if (false == GameEngine.theIsAlien)
                  DisplayImplantRemoval(myGameInstance, selectedTerritory);
               else
                  RotateStack(selectedTerritory);
               return;
            case GamePhase.AlienTakeover:
               if (true == GameEngine.theIsAlien)
                  DisplayTakover(myGameInstance, selectedTerritory);
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
         //----------------------------------------
         if ((null != selectedTerritory) && (null != selectedButton))  // MapItem already selected to move.  Moving it to a known space
         {
            IMapItem? movingMapItem = myGameInstance.Stacks.FindMapItem(selectedButton.Name);
            if (null == movingMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() myGameInstance.Stacks.FindMapItem() returned null for name=" + selectedButton.Name);
               return;
            }
            if ((selectedTerritory.Name == movingMapItem.TerritoryCurrent.Name) && (selectedTerritory.Subname == movingMapItem.TerritoryCurrent.Subname))
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
               MapItemMove? mim = new MapItemMove(Territories.theTerritories, movingMapItem, selectedTerritory);
               if(null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() new MapItemMove() returned null");
                  return;
               }
               if (null == mim.BestPath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() new MapItemMove() mim.BestPath=null");
                  return;
               }
               if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
               {
                  if (true == myIsFlagSetForOverstack)
                     MessageBox.Show("Unable to take this path due to overstacking restrictions. Choose another endpoint.");
                  myIsFlagSetForOverstack = true;
                  return;
               }
               myGameInstance.MapItemMoves.Clear();
               myGameInstance.MapItemMoves.Add(mim);
               if (GamePhase.AlienMovement == myGameInstance.GamePhase)
               {
                  GameAction outAction = GameAction.AlienMovement;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               else if (GamePhase.TownspersonMovement == myGameInstance.GamePhase)
               {
                  myIsAlienAbleToStopMove = true; // The townsperson cannot move any more MapItems until a response is received from teh Alien player.
                  GameAction outAction = GameAction.TownpersonProposesMovement;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
            }
         }
      }
      private bool IsAlienAbleToStopMove(IGameInstance gi, IMapItem mi, out bool isAlienAbleToStopMove)
      {
         isAlienAbleToStopMove=false;
         if (("Zebulon" != mi.Name) && (true != mi.IsStunned) && (true != mi.IsTiedUp) && (true != mi.IsSurrendered)
          && (true != mi.IsStunned) && (true != mi.IsKilled) && ((true == mi.IsAlienUnknown) || (true == mi.IsAlienKnown))
          && (false == mi.IsMoveStoppedThisTurn) && (GamePhase.TownspersonMovement == gi.GamePhase))
         {
            if (0 < gi.MapItemMoves.Count)
            {
               IMapItemMove? mim = gi.MapItemMoves[0];
               if( null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsAlienAbleToStopMove() gi.MapItemMoves[0]=null");
                  return false;
               }
               if (null == mim.OldTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsAlienAbleToStopMove() mim.OldTerritory=null");
                  return false;
               }
               if (null == mim.BestPath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsAlienAbleToStopMove() mim.BestPath=null");
                  return false;
               }
               IMapItem? movingMI = gi.Stacks.FindMapItem(mim.MapItem.Name);
               if( null == movingMI)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsAlienAbleToStopMove() gi.Stacks.FindMapItem() returned null for name=" + mim.MapItem.Name);
                  return false;
               }  
               if ((mi.TerritoryCurrent.Name == mim.OldTerritory.Name) && (mi.TerritoryCurrent.Subname == mim.OldTerritory.Subname))
               {
                  if ((true == movingMI.IsControlled) && (false == movingMI.IsStunned) && (false == movingMI.IsTiedUp)
                     && (false == movingMI.IsSurrendered) && (false == movingMI.IsStunned) && (false == movingMI.IsKilled))
                  {
                     isAlienAbleToStopMove = true;
                     return true;
                  }
               }
               else
               {
                  foreach (ITerritory t in mim.BestPath.Territories)
                  {
                     if ((mi.TerritoryCurrent.Name == t.Name) && (mi.TerritoryCurrent.Subname == t.Subname))
                     {
                        if ((true == movingMI.IsControlled) && (false == movingMI.IsStunned) && (false == movingMI.IsTiedUp)
                             && (false == movingMI.IsSurrendered) && (false == movingMI.IsStunned) && (false == movingMI.IsKilled)
                             && (false == movingMI.IsMoveStoppedThisTurn))
                        {
                           isAlienAbleToStopMove = true;
                           return true;
                        }
                     }
                  } 
               }
            } 
         } 
         return true;
      }
      private bool MapItemReturnToStart(Button selectedButton)
      {
         IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(selectedButton.Name);
         if (null == selectedMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): myGameInstance.Stacks.FindMapItem() returned null for name=" + selectedButton.Name);
            return false;
         }
         if (false == selectedMapItem.IsMoveAllowedToResetThisTurn) // if not allowed to reset, do nothing
         {
            if ((true == myIsFlagSetForMoveReset) && (true == GameEngine.theIsAlien) && (GamePhase.AlienMovement == myGameInstance.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            if ((true == myIsFlagSetForMoveReset) && (false == GameEngine.theIsAlien) && (GamePhase.TownspersonMovement == myGameInstance.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            myIsFlagSetForMoveReset = true;
            this.RotateStack(selectedMapItem.TerritoryCurrent); // rotate the stack
            return true;  // do nothing
         }
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.AlienMovement:
               if ((true == selectedMapItem.IsControlled) || (false == GameEngine.theIsAlien))
               {
                  if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): RotateStack() returned error");
                     return false;
                  }
                  return true;  // do nothing
               }
               break;
            case GamePhase.TownspersonMovement:
               if ((false == selectedMapItem.IsControlled) || (true == GameEngine.theIsAlien))
               {
                  if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): RotateStack() returned error");
                     return false;
                  }
                  return true;  // do nothing
               }
               break;
            default:
               if (false == this.RotateStack(selectedMapItem.TerritoryCurrent))
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): RotateStack() returned error");
                  return false;
               }
               return true;  // do nothing
         } // end switch
         //--------------------------------------------------
         StringBuilder sb = new StringBuilder("MapItem_ReturnToStart(): t="); sb.Append(selectedMapItem.TerritoryCurrent.ToString()); sb.Append(" st="); sb.Append(selectedMapItem.TerritoryStarting.ToString());
         Logger.Log(LogEnum.LE_MIM_RETURN_TO_START, sb.ToString());
         if (selectedMapItem.TerritoryCurrent != selectedMapItem.TerritoryStarting)
         {
            foreach (Rectangle r in myRectangles) // Turn off all animation
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
            if (0 < myGameInstance.MapItemMoves.Count)
            {
               IMapItemMove? mim = myGameInstance.MapItemMoves[0];
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): gi.MapItemMoves[0] = null");
                  return false;
               }
               if (null == mim.BestPath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart():  mim.BestPath = null");
                  return false;
               }
               IMapItem? previousMovingMi1 = myGameInstance.Stacks.FindMapItem(mim.MapItem.Name);
               if (null != previousMovingMi1)
               {
                  previousMovingMi1.TerritoryCurrent = previousMovingMi1.TerritoryStarting;
                  previousMovingMi1.MovementUsed -= mim.BestPath.Territories.Count;
                  if (previousMovingMi1.MovementUsed <= 0)
                  {
                     previousMovingMi1.MovementUsed = 0;
                     previousMovingMi1.IsMoved = false;

                     IMapItem? alreadyMovedMapItem = myMovingMapItems.Find(previousMovingMi1.Name);
                     if (null != alreadyMovedMapItem)
                     {
                        StringBuilder sb1 = new StringBuilder("MapItem_ReturnToStart(): n="); sb1.Append(previousMovingMi1.Name); sb1.Append(" st="); sb1.Append(previousMovingMi1.TerritoryStarting.ToString());
                        Logger.Log(LogEnum.LE_SHOW_MIM_MOVING_COUNT, sb1.ToString());
                        myMovingMapItems.Remove(previousMovingMi1.Name);
                     }
                  }
               }
               myGameInstance.MapItemMoves.Clear();
               GameAction outAction = GameAction.ResetMovement;
               myGameEngine.PerformAction(ref myGameInstance, ref outAction); // Inform the user to return back
            }
         }
         return true;
      }
      private bool RotateStack(ITerritory selectedTerritory)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "RotateStack(): myGameInstance=null");
            return false;
         }
         IGameInstance gi = myGameInstance;
         myRectangleSelection.Visibility = Visibility.Hidden;
         myMovingButton = null;
         IStack? stack = myGameInstance.Stacks.Find(selectedTerritory); // Find the right stack that matches the selected terriroty
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "RotateStack(): myGameInstance.Stacks.Find() returned false for name=" + selectedTerritory.Name);
            return false;
         }
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
         {
            return true;
         }
         IMapItem? bottomAlivePerson = alivePeopleInStack[0];
         if ( null == bottomAlivePerson)
         {
            Logger.Log(LogEnum.LE_ERROR, "RotateStack(): alivePeopleInStack[0]=null");
            return false;
         }
         //--------------------------------------------------I 
         foreach (IMapItem deadPerson in deadPeopleInStack)   // Remove all dead people
            stack.MapItems.Remove(deadPerson);
         Rectangle? bottomRect = null;
         IMapItem? bottomMi = gi.Stacks.FindMapItem(bottomAlivePerson.Name); // Remove the bottom MapItem, Bounding Rectable, and button.
         if (null == bottomMi)
            return true;
         Button? bottomButton = myButtons.Find(bottomMi.Name);
         if (null == bottomButton)
            return true;
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
            Button? b = myButtons.Find(mi.Name);
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
               MapItem.SetButtonContent(b, mi, GameEngine.theIsAlien);
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
         MapItem.SetButtonContent(bottomButton, bottomMi, GameEngine.theIsAlien);
         if (null != bottomRect)
         {
            Canvas.SetLeft(bottomRect, bottomMi.Location.X);
            Canvas.SetTop(bottomRect, bottomMi.Location.Y);
            Canvas.SetZIndex(bottomRect, 1000);
            bottomRect.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            bottomRect.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
         }
         foreach (IMapItem deadPerson in deadPeopleInStack) // Add dead people back into the stack
            stack.MapItems.Add(deadPerson);
         return true;
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
