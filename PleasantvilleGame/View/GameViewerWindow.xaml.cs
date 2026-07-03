using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
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
      private const double RO = 1.5;  // rangtanble offset
      private const int MAX_RECTANGLES = 30; // There are thirty townspeople who can move
      private const int ANIMATE_TIME_SEC = 4; // For moving mapitems
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
      private int myZIndexLastUsed = 1000;
      #endregion
      //--------------------------------------------------------------
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30.0;
      private const Double ELLIPSE_DIAMETER = 40.0;
      private const Double ELLIPSE_RADIUS = ELLIPSE_DIAMETER / 2.0;
      //--------------------------------------------------------------
      private IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      private IDieRoller? myDieRoller = null;
      private EventViewer? myEventViewer = null;
      private MainMenuViewer? myMainMenuViewer = null;
      //--------------------------------------------------------------
      private ContextMenu myContextMenuButton = new ContextMenu();
      private Button? myDraggedButton = null;
      private List<Button> myButtons = new List<Button>();
      private List<Polygon> myPolygons = new List<Polygon>();
      //--------------------------------------------------------------
      private bool myIsFlagSetForAlienMoveCountExceeded = false;  // Alien only allowed to move 5 counters
      private bool myIsFlagSetForMoveReset = false;               // Players cannot reset counter when selected
      private bool myIsFlagSetForOverstack = false;               // MapItem cannot move into hex due to overstack
      private bool myIsFlagSetForMaxMove = false;                 // MapItem cannot move into hex due to overstack
      private bool myIsAlienAbleToStopMove = false;               // The Alien player is allowed to stop Townspeople from moving if in the same hex
      //--------------------------------------------------------------
      private List<Brush> myBrushes = new List<Brush>();
      private int myBrushIndex = 0;
      private DoubleCollection myDashArray = new DoubleCollection();
      private SolidColorBrush mySolidColorBrushClear = new SolidColorBrush();
      private SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();
      private SolidColorBrush mySolidColorBrushGray = new SolidColorBrush();          // Conversations
      private SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush();         // Influences
      private SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();           // Combat
      private SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();        // Interogations
      private SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();     // Implant Removal
      private SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();        // Takeovers
      //--------------------------------------------------------------
      private Dictionary<IMapItem, Rectangle> myRectangleMaps = new Dictionary<IMapItem, Rectangle>();
      private Rectangle? myMovingRectangle = null;                // Rentangle that is moving with button
      private MapItems myMovingMapItems = new MapItems();         // A list to track which MapItems have moved this turn
      private Button? myMovingButton = null;                      // The manually selected button that will be moved
      //--------------------------------------------------------------
      private readonly SplashDialog mySplashScreen;
      //--------------------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahofma");
      private Storyboard? myStoryboardFlashing = null;
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
         SetDisplayIconForUninstall();   // This is specialized code to add to Windows Registry icon for uninstall
         CreateContentMenuForButtons();
         //-----------------------------------------------
         this.BorderBrush = Constants.theNeutralBrush;
         mySolidColorBrushClear.Color = Color.FromArgb(0, 0, 1, 0);
         myBrushes.Add(Brushes.Green);  // Create a container of brushes for painting paths.
         myBrushes.Add(Brushes.Blue);
         myBrushes.Add(Brushes.Purple);
         myBrushes.Add(Brushes.Violet);
         myBrushes.Add(Brushes.Red);
         myBrushes.Add(Brushes.DeepPink);
         myBrushes.Add(Constants.theAlienControlledBrush);
         myBrushes.Add(Constants.theTownControlledBrush);
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
         if (true == GameEngine.theIsAlien)
            myTextBoxOpponent.Foreground = Constants.theAlienControlledBrush;
         else
            myTextBoxOpponent.Foreground = Constants.theTownControlledBrush;
         //----------------------------------------------------------
#pragma warning disable CA1416 // Validate platform compatibility
         myTimer.Interval = ANIMATE_SPEED * 1000 + 1000;
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
         myTimer.Tick += new EventHandler(TimerElasped);
#pragma warning restore CA1416 // Validate platform compatibility
         //----------------------------------------------------------
         UpdateWindowTitle();
         myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownCanvas;
         myCanvasMain.MouseRightButtonDown += this.MouseRightButtonDownCanvas;
         this.PreviewMouseMove += MouseMoveGameViewerWindow;
         //----------------------------------------------------------
         UpdateActionPanelClear();
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
      private void CreateContentMenuForButtons()
      {
         MenuItem mi0 = new MenuItem();
         mi0.Header = "_Scatter Stack";
         mi0.InputGestureText = "Ctrl+S";
         mi0.Click += this.ContextMenuClickScatter;
         myContextMenuButton.Items.Add(mi0);
         MenuItem mi1 = new MenuItem();
         mi1.Header = "_Rotate Stack";
         mi1.InputGestureText = "Ctrl+R";
         mi1.Click += this.ContextMenuClickRotate;
         myContextMenuButton.Items.Add(mi1);
         MenuItem mi2 = new MenuItem();
         mi2.Header = "_Return to Starting point";
         mi2.InputGestureText = "Shift+S";
         mi2.Click += this.ContextMenuClickReturnToStart;
         myContextMenuButton.Items.Add(mi2);
         if (true == GameEngine.theIsAlien)
         {
            MenuItem mi3 = new MenuItem();
            mi3.Header = "_Stop Townsperson Move";
            mi3.InputGestureText = "Ctrl+Shift+P";
            mi3.Click += this.ContextMenuClickStopMove;
            myContextMenuButton.Items.Add(mi3);
         }
         myContextMenuButton.Loaded += this.ContextMenuLoadedButton;
      }
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
         //--------------------------------                                                                            //--------------------------------
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
      private void UpdateWindowTitle()
      {
         StringBuilder sb55 = new StringBuilder();
         if (true == GameEngine.theIsHost)
            sb55.Append("SERVER: ");
         else
            sb55.Append("CLIENT: ");
         if (true == GameEngine.theIsAlien)
            sb55.Append("Pleasantville For Aliens");
         else
            sb55.Append("Pleasantville For Humans");
         this.Title = sb55.ToString();
      }
      //-------------INTERFACE FUNCTIONS---------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) || (GameAction.RemoveSplashScreen == action))
         {
            UpdateActionPanelClear();
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
         myGameInstance = gi;
         GameAction outAction = GameAction.Error;
         switch (action) // Perform acton based on the current next action.
         {
            case GameAction.GameSetupHostGame:
               UpdateWindowTitle();
               break;
            case GameAction.GameSetupJoinGame:
               UpdateWindowTitle();
               break;
            case GameAction.GameSetupPlayAlien:
               UpdateWindowTitle();
               break;
            case GameAction.GameSetupPlayTownsperson:
               UpdateWindowTitle();
               break;
            case GameAction.RandomMovementStartTowns:
               UpdateActionPanelClear();
               if (false == UpdateCanvasMain(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
                  return;
               }
               myRectangleMaps.Clear();
               int index = 0;
               foreach (Button b in myButtons)
               {
                  foreach(RandomMoveData rmd in myGameInstance.RandomMoves)
                  {
                     if( true == b.Name.Contains(rmd.myName))
                     {
                        IMapItem? mi = gi.Stacks.FindMapItem(rmd.myName);
                        if ( mi == null )
                        {
                           Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() mi=null for rmd.myName=" + rmd.myName);
                           return;
                        }
                        Rectangle r = new Rectangle() { Width=b.Width + 2, Height=b.Height + 2, Visibility=Visibility.Visible, Stroke = myBrushes[index], StrokeThickness=3.0, StrokeDashArray=myDashArray  };
                        myRectangleMaps[mi] = r;
                        index++;
                        myCanvasMain.Children.Add(r);
                        double left = Canvas.GetLeft(b) - RO;
                        double top = Canvas.GetTop(b) - RO;
                        Canvas.SetLeft(r, left);
                        Canvas.SetTop(r, top);
                        Canvas.SetZIndex(r, myZIndexLastUsed);
                        break;
                     }
                  }
               }
               break;
            case GameAction.RandomMovementTownsShow:
               if( false == UpdateCanvasMovement(gi, action, gi.Stacks, myButtons))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMovement() returned error ");
                  return;
               }
               myRectangleMaps.Clear();
               break;
            case GameAction.RandomMovementTownAck:

               if (false == UpdateCanvasMain(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
                  return;
               }
               int index1 = 0;
               foreach (Button b in myButtons)
               {
                  foreach (IMapItemMove mim in myGameInstance.MapItemMoves)
                  {
                     if (true == b.Name.Contains(mim.MapItem.Name))
                     {
                        Rectangle r = new Rectangle() { Width = b.Width + 2, Height = b.Height + 2, Visibility = Visibility.Visible, Stroke = myBrushes[6], StrokeThickness = 3.0, StrokeDashArray = myDashArray };
                        myRectangleMaps[mim.MapItem] = r;
                        index1++;
                        myCanvasMain.Children.Add(r);
                        double left = Canvas.GetLeft(b) - RO;
                        double top = Canvas.GetTop(b) - RO;
                        Canvas.SetLeft(r, left);
                        Canvas.SetTop(r, top);
                        Canvas.SetZIndex(r, myZIndexLastUsed);
                        break;
                     }
                  }
               }
               break;
            case GameAction.AlienMovementTownsShow:
               if (false == UpdateCanvasMovement(gi, action, gi.Stacks, myButtons))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMovement() returned error ");
                  return;
               }
               break;
            case GameAction.AlienMovementTownsAck:
               myRectangleMaps.Clear();
               if (false == UpdateCanvasMain(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
                  return;
               }
               if( false == UpdateTownMovementTownPerforms(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_TownMovementTownPerforms() returned error ");
                  return;
               }
               break;
            case GameAction.TownMovementTownPerforms:
               if (false == UpdateCanvasMain(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
                  return;
               }
               if (false == UpdateCanvasMovement(gi, action, gi.Stacks, myButtons))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMovement() returned error ");
                  return;
               }
               break;
            default:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
               break;
         }
      }
      private bool UpdateViewForNewGame(ref IGameInstance gi, GameAction action) // GameAction.UpdateLoadingGame  GameAction.UpdateNewGame
      {
         Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateViewForNewGame(): Clearing action=" + action.ToString());
         myGameInstance = gi;
         myButtons.Clear();
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
         UpdateCanvasMainClear(myButtons, gi.Stacks, action);
         //--------------------------------------------------------------
         //IMapItem? zebulon = gi.Stacks.FindMapItem("Zebulon");
         //if (null == zebulon)
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain_MapItems(): could not find Zebulon in gi.Stacks");
         //   return false;
         //}
         //if (true == zebulon.IsAlienKnown)
         //{
         //   Button? b = myButtons.Find("Zebulon");
         //   if (null == zebulon)
         //   {
         //      Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain_MapItems(): could not find Zebulon in myButtons");
         //      return false;
         //   }
         //   if (null != b)
         //   {
         //      b.Visibility = Visibility.Visible;
         //      Canvas.SetZIndex(b, 100000);
         //   }
         //}
         //---------------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMain_MapItems(): " + gi.Stacks.ToString());
         foreach (IStack stack in gi.Stacks)
         {
            ITerritory t = stack.Territory;
            double count = 0;
            foreach (IMapItem mi in stack.MapItems)
            {
               double offset = (count * 3.0) + (mi.Zoom * Utilities.theMapItemOffset);
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain_MapItems(): mi=null");
                  return false;
               }
               //---------------------------------------------
               Button? b = myButtons.Find(mi.Name);
               if (null != b)
               {
                  b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                  b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                  Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMain_MapItems(): Updating mi=" + mi.Name + " X=" + mi.Location.X.ToString("F2") + " Y=" + mi.Location.Y.ToString("F2"));
                  if (true == stack.IsStacked)
                  {
                     Canvas.SetLeft(b, t.CenterPoint.X - offset);
                     Canvas.SetTop(b, t.CenterPoint.Y - offset);
                  }
                  else
                  {
                     Canvas.SetLeft(b, mi.Location.X);
                     Canvas.SetTop(b, mi.Location.Y);
                  }
                  Canvas.SetZIndex(b, myZIndexLastUsed + (int)count);
               }
               else
               {
                  Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "UpdateCanvasMain_MapItems(): Adding Button for mi=" + mi.Name + " X=" + mi.Location.X.ToString("F2") + " Y=" + mi.Location.Y.ToString("F2") + " in stack@" + stack.ToString());
                  System.Windows.Controls.Button newButton = new Button { Name = mi.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(1), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
                  MapItem.SetButtonContent(newButton, mi, true, true); // This sets the image as the button's content
                  myButtons.Add(newButton);
                  if (true == stack.IsStacked)
                  {
                     Canvas.SetLeft(newButton, t.CenterPoint.X - offset);
                     Canvas.SetTop(newButton, t.CenterPoint.Y - offset);
                  }
                  else
                  {
                     Canvas.SetLeft(newButton, mi.Location.X);
                     Canvas.SetTop(newButton, mi.Location.Y);
                  }
                  Canvas.SetZIndex(newButton, myZIndexLastUsed++);
                  myCanvasMain.Children.Add(newButton);
                  newButton.ContextMenu = myContextMenuButton;
                  newButton.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDownMapItem;
                  newButton.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUpMapItem;
               }
               if (true == myRectangleMaps.ContainsKey(mi))
               {
                  Rectangle r = myRectangleMaps[mi];
                  if (false == myCanvasMain.Children.Contains(r))
                     myCanvasMain.Children.Add(r);
                  Canvas.SetLeft(r, mi.Location.X);
                  Canvas.SetTop(r, mi.Location.Y);
                  Canvas.SetZIndex(r, myZIndexLastUsed++);
               }
               count++;
            }
         }
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMain_MapItems(): action=" + action.ToString() + " stacks=" + gi.Stacks.ToString());
         return true;
      }
      private void UpdateCanvasMainClear(List<Button> buttons, IStacks stacks, GameAction action)
      {
         if (GamePhase.UnitTest == myGameInstance.GamePhase)
            return;
         myStoryboardFlashing = null;
         //-------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "Update_CanvasMainClear(): Clearing action=" + action.ToString() + " stacks=" + stacks.ToString());
         List<UIElement> elementRemovals = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
         {
            if (ui is Button button)
            {
               if (true == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
                  continue;
               IMapItem? mi = stacks.FindMapItem(button.Name);
               if (null == mi) // If Button does not have corresponding MapItem, remove button.
               {
                  elementRemovals.Add(ui);
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
                  elementRemovals.Add(ui);
            }
            else if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               elementRemovals.Add(ui);
            }
            else if (ui is Rectangle rectangle) 
               elementRemovals.Add(ui);
            else if (ui is Label label)  // A Game Feat Label
               elementRemovals.Add(ui);
            else if (ui is TextBlock tb)
               elementRemovals.Add(ui);
            else if (ui is Polyline polyline)
               elementRemovals.Add(ui);
            //else if (ui is Polygon polygon)
            //   elements.Add(ui);
         }
         foreach (UIElement ui1 in elementRemovals)
            myCanvasMain.Children.Remove(ui1);
      }
      private void UpdateActionPanel(IGameInstance gi, bool isOkButtonDisplayed)
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
      private void UpdateActionPanelClear()
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

         Logger.Log(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL_CLEAR, "UpdateActionPanelClear() myTextBoxResults.Clear()");
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
      private void UpdateActionPanelButtons(IGameInstance gi)
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
      private void UpdateViewState(IGameInstance gi)
      {
         myStoryboardFlashing = null;  // turn off flashing
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
      private bool UpdateCanvasMovement(IGameInstance gi, GameAction action, IStacks stacks, List<Button> buttons)
      {
         try
         {
            int count = 0;
            foreach (IMapItemMove mim in gi.MapItemMoves)
            {
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim=null");
                  return false;
               }
               if (null == mim.BestPath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim.BestPath=null");
                  return false;
               }
               if (null == mim.OldTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim.OldTerritory=null");
                  return false;
               }
               if (null == mim.NewTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim.NewTerritory=null");
                  return false;
               }
               IMapItem mi = mim.MapItem;
               IMapPoint endPoint = Territory.GetRandomPoint(mim.NewTerritory, mi.Zoom * Utilities.theMapItemOffset);
               if (false == MovePathDisplay(mim, count, endPoint))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): Move_PathDisplay() returned false t=" + mim.OldTerritory.ToString());
                  gi.MapItemMoves.Clear();
                  return false;
               }
               if (false == MovePathAnimate(gi, mim, buttons, count, endPoint))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): Move_PathAnimate() returned false t=" + mim.OldTerritory.ToString());
                  gi.MapItemMoves.Clear();
                  return false;
               }
               //------------------------------------------
               stacks.Remove(mi); // remove from existing stack
               Logger.Log(LogEnum.LE_SHOW_MIM, "UpdateCanvasMovement(): a=" + action.ToString() + " mi=" + mim.MapItem.Name + " t=" + mim.MapItem.TerritoryCurrent.ToString() + "==>" + mim.NewTerritory.ToString());
               mi.TerritoryCurrent = mi.TerritoryStarting = mim.NewTerritory;
               mi.Location.X = endPoint.X;
               mi.Location.Y = endPoint.Y;
               mi.MovementUsed += mim.BestPath.Territories.Count;
               if ( (GamePhase.TownspersonMovement == gi.GamePhase) && (mi.Movement <= mi.MovementUsed))
               {
                  Rectangle r = myRectangleMaps[mi];
                  r.Stroke = Constants.theTownControlledBrush;
               }
               stacks.Add(mi); // add to new stack
               count++;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement():  EXCEPTION THROWN e=\n" + e.ToString());
            return false;
         }
         return true;
      }
      private bool MovePathDisplay(IMapItemMove mim, int mapItemCount, IMapPoint endPoint)
      {
         if (null == mim.OldTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathDisplay(): mim.OldTerritory=null");
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathDisplay(): mim.NewTerritory=null");
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathDisplay(): mim.BestPath=null");
            return false;
         }
         //-----------------------------------------
         double offset = 0.0;
         if (0 < mapItemCount)
         {
            if (0 == mapItemCount % 2)
               offset = mapItemCount - 1;
            else
               offset = -mapItemCount;
         }
         offset *= 3.0;
         //-----------------------------------------
         PointCollection aPointCollection = new PointCollection();
         double xStart = mim.MapItem.Location.X + mim.MapItem.Zoom * Utilities.theMapItemOffset;  // get top left point of MapItem
         double yStart = mim.MapItem.Location.Y + mim.MapItem.Zoom * Utilities.theMapItemOffset;
         System.Windows.Point newPoint = new System.Windows.Point(xStart, yStart);
         aPointCollection.Add(newPoint);
         int lastItemIndex = mim.BestPath.Territories.Count - 1;
         for (int i = 0; i < lastItemIndex; i++)
         {
            ITerritory t = mim.BestPath.Territories[i];
            double xPostion = t.CenterPoint.X + offset;
            double yPostion = t.CenterPoint.Y + offset;
            newPoint = new System.Windows.Point(xPostion, yPostion);
            aPointCollection.Add(newPoint);
         }
         //-----------------------------------------
         double centerPointOfMapItem = offset - 1.0 + mim.MapItem.Zoom * Utilities.theMapItemOffset;
         System.Windows.Point lastPointMapItem = new System.Windows.Point(endPoint.X + centerPointOfMapItem, endPoint.Y + centerPointOfMapItem);
         aPointCollection.Add(lastPointMapItem);
         //-----------------------------------------
         Polyline aPolyline = new Polyline();
         if( true == myRectangleMaps.ContainsKey(mim.MapItem))
            aPolyline.Stroke = myRectangleMaps[mim.MapItem].Stroke;
         else
            aPolyline.Stroke = myBrushes[myBrushIndex];
         aPolyline.StrokeThickness = 3;
         aPolyline.StrokeEndLineCap = PenLineCap.Triangle;
         aPolyline.Points = aPointCollection;
         aPolyline.StrokeDashArray = myDashArray;
         myCanvasMain.Children.Add(aPolyline);
         return true;
      }
      private bool MovePathAnimate(IGameInstance gi, IMapItemMove mim, List<Button> buttons, int mapItemCount, IMapPoint endPoint)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathAnimate(): myGameInstance=null for n=" + mim.MapItem.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathAnimate(): mim.NewTerritory=null n=" + mim.MapItem.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathAnimate(): mim.BestPath=null for mi=" + mim.MapItem.Name);
            return false;
         }
         Button? b = buttons.Find(mim.MapItem.Name);
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "Move_PathAnimate(): b=null for mi=" + mim.MapItem.Name + " mim=" + mim.ToString());
            return false;
         }
         try
         {
            double offset = mim.MapItem.Zoom * Utilities.theMapItemOffset;
            Canvas.SetZIndex(b, myZIndexLastUsed++); // Move the button to the top of the Canvas
            double xStart = mim.MapItem.Location.X;  // get top left point of MapItem
            double yStart = mim.MapItem.Location.Y;
            if ((Math.Abs(endPoint.X - xStart) < 2) && (Math.Abs(endPoint.Y - yStart) < 2)) // if already at final location, skip animation or get runtime exception
               return true;
            //----------------------------------------------------
            PathFigure aPathFigureMapItem = new PathFigure() { StartPoint = new System.Windows.Point(xStart, yStart) };
            PathFigure aPathFigureRectangle = new PathFigure() { StartPoint = new System.Windows.Point(xStart - RO, yStart - RO) };
            int lastItemIndex = mim.BestPath.Territories.Count - 1;
            for (int i = 0; i < lastItemIndex; i++) // add intermediate movement points
            {
               ITerritory t = mim.BestPath.Territories[i];
               double x = t.CenterPoint.X - offset;
               double y = t.CenterPoint.Y - offset;
               System.Windows.Point newPointMapItem = new System.Windows.Point(x, y);
               LineSegment lineSegmentMapItem = new LineSegment(newPointMapItem, false);
               aPathFigureMapItem.Segments.Add(lineSegmentMapItem);
               System.Windows.Point newPointRectangle = new System.Windows.Point(x - RO, y - RO);
               LineSegment lineSegmentRectangle = new LineSegment(newPointRectangle, false);
               aPathFigureRectangle.Segments.Add(lineSegmentRectangle);
            }
            //----------------------------------------------------
            System.Windows.Point lastPointMapItem = new System.Windows.Point(endPoint.X, endPoint.Y);
            LineSegment lastLineSegmentMapItem = new LineSegment(lastPointMapItem, false);
            aPathFigureMapItem.Segments.Add(lastLineSegmentMapItem);
            PathGeometry aPathGeo = new PathGeometry(); // Animiate the map item along the line segment
            aPathGeo.Figures.Add(aPathFigureMapItem);
            aPathGeo.Freeze();
            DoubleAnimationUsingPath xAnimiation = new DoubleAnimationUsingPath();
            xAnimiation.PathGeometry = aPathGeo;
            xAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
            xAnimiation.Source = PathAnimationSource.X;
            DoubleAnimationUsingPath yAnimiation = new DoubleAnimationUsingPath();
            yAnimiation.PathGeometry = aPathGeo;
            yAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
            yAnimiation.Source = PathAnimationSource.Y;
            b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            b.BeginAnimation(Canvas.TopProperty, yAnimiation);
            //----------------------------------------------------
            if (true == myRectangleMaps.ContainsKey(mim.MapItem))
            {
               Rectangle r = myRectangleMaps[mim.MapItem];
               Canvas.SetZIndex(r, myZIndexLastUsed++); // Move the rectangle one higher
               System.Windows.Point lastPointRectangle = new System.Windows.Point(endPoint.X - 1.5, endPoint.Y - 1.5);
               LineSegment lastLineSegmenRectangle = new LineSegment(lastPointMapItem, false);
               aPathFigureRectangle.Segments.Add(lastLineSegmenRectangle);
               PathGeometry aPathGeoRectangle = new PathGeometry(); // Animiate the map item along the line segment
               aPathGeoRectangle.Figures.Add(aPathFigureRectangle);
               aPathGeoRectangle.Freeze();
               DoubleAnimationUsingPath xAnimiationR = new DoubleAnimationUsingPath();
               xAnimiationR.PathGeometry = aPathGeoRectangle;
               xAnimiationR.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
               xAnimiationR.Source = PathAnimationSource.X;
               DoubleAnimationUsingPath yAnimiationR = new DoubleAnimationUsingPath();
               yAnimiationR.PathGeometry = aPathGeoRectangle;
               yAnimiationR.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
               yAnimiationR.Source = PathAnimationSource.Y;
               r.BeginAnimation(Canvas.LeftProperty, xAnimiationR);
               r.BeginAnimation(Canvas.TopProperty, yAnimiationR);
            }
            return true;
         }
         catch (Exception e)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            Logger.Log(LogEnum.LE_ERROR, "Move_PathAnimate():  EXCEPTION THROWN e=\n" + e.ToString());
            return false;
         }
      }
      private bool UpdateTownMovementTownPerforms(IGameInstance gi, GameAction action)
      {
         foreach (ITerritory t in Territories.theTerritories) // Create the regions associated with the territories. All the information of Territories is static and does not change.
         {
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): null territory in Territories.theTerritories");
               return false;
            }
            if (0 < t.Points.Count)
            {
               List<Point> points = new List<Point>();
               foreach (IMapPoint mp in t.Points)
                  points.Add(new Point(mp.X, mp.Y));
               PointCollection pointCollection = new PointCollection(points);
               Polygon aPolygon = new Polygon() { Name = t.ToString(), Fill = mySolidColorBrushClear, Points = pointCollection };
               myCanvasMain.RegisterName(t.ToString(), aPolygon);
               myCanvasMain.Children.Add(aPolygon);
               myPolygons.Add(aPolygon);
               aPolygon.MouseDown += MouseDownPolygon;
            }
         }
         //-------------------------------------------
         if (false == GameEngine.theIsAlien)
         {
            myRectangleMaps.Clear();
            int index2 = 0;
            foreach (IStack stack in gi.Stacks) // add the event handler for the button click for controlled townspeople only
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if ( (true == mi.IsControlled) && (false == mi.IsTiedUp) && (false == mi.IsUnconscious) && (false == mi.IsStunned) && (mi.MovementUsed < mi.Movement))
                  {
                     foreach (Button b in myButtons)
                     {
                        if (mi.Name == b.Name)
                        {
                           b.Click += ClickButtonMapItem;
                           mi.MovementUsed = 0;
                           Rectangle r = new Rectangle() { Width = b.Width + 2, Height = b.Height + 2, Visibility = Visibility.Visible, Stroke = myBrushes[4], StrokeThickness = 3.0, StrokeDashArray = myDashArray };
                           myRectangleMaps[mi] = r;
                           index2++;
                           myCanvasMain.Children.Add(r);
                           double left = Canvas.GetLeft(b) - RO;
                           double top = Canvas.GetTop(b) - RO;
                           Canvas.SetLeft(r, left);
                           Canvas.SetTop(r, top);
                           Canvas.SetZIndex(r, myZIndexLastUsed);
                        }
                     }
                  }
               }
            }
         }
         return true;
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
      private bool DisplayConversations(IGameInstance gi)
      {
         //// Clear any previous flashing regions
         //myStoryboardFlashing = new Storyboard();
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
         //      if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
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
         //   myStoryboardFlashing.Children.Add(anim);
         //   Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
         //   Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         //} // end foreach (Stack stack in stacks)
         //if (0 == myStoryboardFlashing.Children.Count)
         //   return false;
         //myStoryboardFlashing.Begin(this);
         return true;
      }
      private void DisplayConversation(IGameInstance gi, ITerritory selectedTerritory)
      {
         //UpdateActionPanelClear();
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
         //      if ((true == mi.IsConversedThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsWary))
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
            UpdateActionPanelClear();
         else
            UpdateActionPanelButtons(gi);
         return true;
      }
      private bool DisplayInfluences(IGameInstance gi)
      {
         myStoryboardFlashing = new Storyboard(); // Clear any previous flashing regions
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
               if ((true == mi.IsInfluencedThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
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
                     Canvas.SetZIndex(p1, myZIndexLastUsed);
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
            myStoryboardFlashing.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)
         if (0 == myStoryboardFlashing.Children.Count)
            return false;
         myStoryboardFlashing.Begin(this);
         return true;
      }
      private bool DisplayInfluence(IGameInstance gi, ITerritory selectedTerritory)
      {
         UpdateActionPanelClear();
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
               if ((true == mi.IsInfluencedThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
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
            UpdateActionPanelClear();
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
         myStoryboardFlashing = new Storyboard();
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
               if ((true == mi.IsCombatThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp))
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
                     Canvas.SetZIndex(p1, myZIndexLastUsed);
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
            myStoryboardFlashing.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         }
         //------------------------------------------------------------------------------
         if (0 == myStoryboardFlashing.Children.Count)
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
         myStoryboardFlashing.Begin(this);
         return true;
      }
      private void DisplayCombat(IGameInstance gi, ITerritory selectedTerritory)
      {
         UpdateActionPanelClear();
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
         foreach (MapItem mi in gi.Townspeople)
         {
            if ((selectedTerritory.Name == mi.TerritoryCurrent.Name) && (selectedTerritory.Subname == mi.TerritoryCurrent.Subname))
            {
               if ((false == mi.IsUnconscious) || (true == mi.IsStunned) || (true == mi.IsTiedUp) || (true == mi.IsKilled) || (true == mi.IsSurrendered))
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
            UpdateActionPanelClear();
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
         UpdateActionPanelClear();
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
         //if (null != gi.MapItemCombat.Territory)
         //   UpdateViewMovement(gi); // Show retreats
         UpdateViewState(gi);
         myIsCombatInitiatedForTownsperson = false;
         StringBuilder sb1 = new StringBuilder("UpdateView():TownspersonPerformCombat: "); 
         sb1.Append(GameEngine.theIsAlien.ToString()); 
         sb1.Append("myIsCombatInitiatedForTownsperson=false");
         Logger.Log(LogEnum.LE_SHOW_COMBAT_STATE, sb1.ToString());
         if (true == isIgnoreResults)
            UpdateActionPanelClear();
      }
      private bool DisplayIterogations(IGameInstance gi, out bool isInterrogations)
      {
         isInterrogations = false;
         myStoryboardFlashing = new Storyboard();
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
               if ((true == mi.IsInterrogatedThisTurn) || (true == mi.IsInterrogated) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsStunned))
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
                     Canvas.SetZIndex(p1, myZIndexLastUsed);
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
            myStoryboardFlashing.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         } // end foreach (Stack stack in stacks)
           //--------------------------------------------------------------
         if (0 < myStoryboardFlashing.Children.Count)
            myStoryboardFlashing.Begin(this);
         if (0 < gi.NumIterogationsThisTurn)
            return true;
         return false;
      }
      private bool DisplayImplantRemovals(IGameInstance gi)
      {
         myStoryboardFlashing = new Storyboard();
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
               if ((true == mi.IsControlled) && (true == mi.IsUnconscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  controlledMapItems.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsUnconscious)))
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
                     Canvas.SetZIndex(p1, myZIndexLastUsed);
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
            myStoryboardFlashing.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select
         }
         //-------------------------------------------------------------- 
         if (0 == myStoryboardFlashing.Children.Count)
            return false;
         myStoryboardFlashing.Begin(this);
         return true;
      }
      private void DisplayImplantRemoval(IGameInstance gi, ITerritory selectedTerritory)
      {
         UpdateActionPanelClear();
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

               if ((true == mi.IsControlled) && (true == mi.IsUnconscious) && (false == mi.IsTiedUp) && (false == mi.IsStunned))
                  myLeftMapItemsInActionPanel.Add(mi);
               else if ((true == mi.IsAlienKnown) && ("Zebulon" != mi.Name) && ((true == mi.IsTiedUp) || (true == mi.IsSurrendered) || (false == mi.IsUnconscious)))
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
            UpdateActionPanelClear();
         else
            UpdateActionPanelButtons(gi);
         return true;
      }
      private bool DisplayTakeovers(IGameInstance gi, ITerritory? selectedTerritory = null)
      {
         if (false == GameEngine.theIsAlien)
         {
            myStoryboardFlashing = null; // turn off any flashing spaces
            return false;
         }
         //----------------------------------------------------------------------
         myStoryboardFlashing = new Storyboard(); // Clear any previous flashing regions
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
               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsSurrendered) || ("Zebulon" == mi.Name))
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
                     Canvas.SetZIndex(p1, myZIndexLastUsed);
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
            myStoryboardFlashing.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetName(anim, targetName); // Start flashing the region where the user can select

         } // end foreach (Stack stack in stacks)
           //-------------------------------------------------------------------------------------------------
         if (0 == myStoryboardFlashing.Children.Count)
         {
            if ((true == myIsTakeOverPromptNeededToFoolOpponent) && (false == myIsTakeOverInOneRegion))
               return true;
            else
               return false;
         }
         myStoryboardFlashing.Begin(this);
         return true;
      }
      private void DisplayTakover(IGameInstance gi, ITerritory selectedTerritory)
      {
         UpdateActionPanelClear();
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

               if ((true == mi.IsTakeoverThisTurn) || (true == mi.IsKilled) || (false == mi.IsUnconscious) || (true == mi.IsSurrendered) || ("Zebulon" == mi.Name))
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
            UpdateActionPanelClear();
         //-----------------------------------------------------------------------------
         GameAction outAction = GameAction.AlienTakeover;
         myGameEngine.PerformAction(ref gi, ref outAction);
         return true;
      }
      private bool PerformTakeoverObserved(IGameInstance gi)
      {
         UpdateActionPanelClear();
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
      //---------------
      private void TextBoxEntryTextChanged(object sender, TextChangedEventArgs e)
      {
         //if (null != myGameEngine)
         //{
         //   string entry = myTextBoxOpponent.Text;  // Do not do anything unless a carriage return happens
         //   int length = entry.Count();
         //   if (0 == length)
         //      return;
         //   if ('\n' == entry[length - 1])
         //   {
         //      myTextBoxOpponent.Text = "";
         //      StringBuilder sb = new StringBuilder("You say: ");
         //      sb.Append(entry);
         //      myTextBoxDisplay.AppendText(sb.ToString());
         //      myTextBoxDisplay.ScrollToEnd();
         //      //myGameEngine.SendText(entry);
         //   }
         //}
      }
      private void PreviewMouseLeftButtonDownMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (e.LeftButton == MouseButtonState.Pressed)
         {
            Button? button = sender as Button;
            if (null == button)
            {
               Logger.Log(LogEnum.LE_ERROR, "PreviewMouseLeftButtonDown_MapItem(): button=null");
               return;
            }
            Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "PreviewMouseLeftButtonDown_MapItem(): selected button.Name=" + button.Name);
            myDraggedButton = button;
         }
      }
      private void PreviewMouseLeftButtonUpMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (null == myDraggedButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "PreviewMouseLeftButtonUp_MapItem(): myDraggedButton=null");
            return;
         }
         Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "PreviewMouseLeftButtonUp_MapItem(): unselecting button.Name=" + myDraggedButton.Name);
         IMapItem? mi = myGameInstance.Stacks.FindMapItem(myDraggedButton.Name);
         if( null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "PreviewMouseLeftButtonUp_MapItem(): unable to find mi=" + myDraggedButton.Name);
            return;
         }
         string? tName = mi.TerritoryCurrent.ToString();
         if (null == tName)
         {
            Logger.Log(LogEnum.LE_ERROR, "PreviewMouseLeftButtonUp_MapItem(): mi.TerritoryCurrent.ToString()=null");
            return;
         }
         IStack? stack = myGameInstance.Stacks.Find(tName);
         if ( null == stack )
         {
            Logger.Log(LogEnum.LE_ERROR, "PreviewMouseLeftButtonUp_MapItem(): unable to find stack=" + tName);
            return;
         }
         stack.IsStacked = false;
         myDraggedButton = null;
      }
      private void MouseMoveGameViewerWindow(object sender, MouseEventArgs e)
      {
         if (null == myDraggedButton)
         {
            base.OnMouseMove(e);
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseMove_GameViewerWindow(): myGameInstance=null");
            return;
         }
         //-----------------------------------
         IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(myDraggedButton.Name); // selectedMapItem is the new target
         if (null == selectedMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseMove_GameViewerWindow(): selectedMapItem=null for button.Name=" + myDraggedButton.Name);
            return;
         }
         //-----------------------------------
         System.Windows.Point newPoint = e.GetPosition(myCanvasMain);
         if (true == Territory.IsPointInPolygon(selectedMapItem.TerritoryCurrent, newPoint))
         {
            Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "MouseMove_GameViewerWindow(): button.Name=" + myDraggedButton.Name + " moving to p=(" + newPoint.X.ToString("###") + "," + newPoint.Y.ToString("###") + ")");
            double offset = selectedMapItem.Zoom * Utilities.theMapItemOffset;
            selectedMapItem.Location.X = newPoint.X - offset;
            selectedMapItem.Location.Y = newPoint.Y - offset;
            Canvas.SetLeft(myDraggedButton, newPoint.X - offset);
            Canvas.SetTop(myDraggedButton, newPoint.Y - offset);
            Canvas.SetZIndex(myDraggedButton, myZIndexLastUsed++);
            if (true == myRectangleMaps.ContainsKey(selectedMapItem))
            {
               Rectangle r = myRectangleMaps[selectedMapItem];
               Canvas.SetLeft(r, selectedMapItem.Location.X);
               Canvas.SetTop(r, selectedMapItem.Location.Y);
               Canvas.SetZIndex(r, myZIndexLastUsed++);
            }
         }
         e.Handled = true;
      }
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButton_MapItem() myGameInstance=null");
            return;
         }
         Button? selectedButton = sender as Button;
         if (null == selectedButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButton_MapItem(): selectedButton=null");
            return;
         }
         IMapItem? selectedMapItem = myGameInstance.Stacks.FindMapItem(selectedButton.Name);
         if (null == selectedMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButton_MapItem() Did not find MapItem associated with selectedButton=" + selectedButton.Name);
            return;
         }
         //------------------------------------------------------------
         myGameInstance.SelectedStack = myGameInstance.Stacks.Find(selectedMapItem.TerritoryCurrent);
         if (null == myGameInstance.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemCommonAction(): stack=null for t=" + selectedMapItem.TerritoryCurrent.ToString());
            return;
         }
         GameAction outAction = GameAction.UpdateRotateStack;
         //------------------------------------------------------------
         if (true == selectedMapItem.IsKilled)  // If killed, do nothing
         {
            myGameEngine.PerformAction(ref myGameInstance, ref outAction);
            return;
         }
         //------------------------------------------------------------
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.AlienMovement:
               Logger.Log(LogEnum.LE_ERROR, "ClickButton_MapItem(): AlienMovement not implemented");
               return;
            case GamePhase.TownspersonMovement:
               if (null == myGameInstance.SelectedMapItems.Find(selectedMapItem.Name))
               {
                  Logger.Log(LogEnum.LE_SHOW_TOWN_MOVE, "ClickButton_MapItem(): Adding to SelectedMapItems mi=" + selectedMapItem.Name);
                  myGameInstance.SelectedMapItems.Add(selectedMapItem);
               }
               else
               {
                  Logger.Log(LogEnum.LE_SHOW_TOWN_MOVE, "ClickButton_MapItem(): REmoving to SelectedMapItems mi=" + selectedMapItem.Name);
                  myGameInstance.SelectedMapItems.Remove(selectedMapItem.Name);
               }
               break;
            default:
               if (false == MapItemCommonAction(selectedMapItem.TerritoryCurrent))
                  Logger.Log(LogEnum.LE_ERROR, "ClickButton_MapItem(): MapItem_CommonAction() returned false");
               break;
         }
         e.Handled = true;
      }
      private void DoubleClickMapItem(object sender, RoutedEventArgs e)
      {
         // There is already a moving button.  Do not do any actions until
         // the alien player responds or there is a timeout on the alien response.
         // When that happens, myIsAlienAbleToStopMove=false.
         if (true == myIsAlienAbleToStopMove)
            return;
         if (sender is Button)
         {
            Button selectedButton = (Button)sender;
            if (false == MapItemReturnToStart(selectedButton))
               Logger.Log(LogEnum.LE_ERROR, "MouseDoubleClickMapItem() MapItemReturnToStart() returned error");
         }
      }
      private void MouseDownPolygon(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_Polygon() myGameInstance=null");
            return;
         }
         Polygon p = (Polygon)sender;
         if(null == p )
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_Polygon() polygon=null");
            return;
         }
         myGameInstance.SelectedTerritory = Territories.theTerritories.Find(p.Name);
         if (null == myGameInstance.SelectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_Polygon() myGameInstance.SelectedTerritory=null for p.Name=" + p.Name);
            return;
         }
         GameAction outAction = GameAction.Error;
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.AlienMovement:
                break;
            case GamePhase.TownspersonMovement:
               Logger.Log(LogEnum.LE_SHOW_TOWN_MOVE, "MouseDown_Polygon(): gi.SelectedMapItems.Count=" + myGameInstance.SelectedMapItems.Count.ToString() + " p.Name=" + p.Name);

               if (0 == myGameInstance.SelectedMapItems.Count) // if no selected mapitems, do nothing
                  return;
               IMapItem? mi = myGameInstance.SelectedMapItems[0];
               if( null == mi )
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseDown_Polygon() mi=null");
                  return;
               }
               if (mi.TerritoryCurrent.ToString() == p.Name) // if clicking in same territory at unit, do nothing.
                  return;
               outAction = GameAction.TownMovementTownPerforms;
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            default:
               return;
         }
         e.Handled = true;
      }
      private void MouseLeftButtonDownCanvas(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDown_Canvas() myGameInstance=null");
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
                     if (aPolygon.Name == Utilities.RemoveSpaces(tName))
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
               {
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               }
               else
               {
                  GameAction outAction = GameAction.UpdateRotateStack;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               break;
            case GamePhase.TownspersonMovement:
               if ((false == GameEngine.theIsAlien) && (null != myMovingButton))
               {
                  MapItemMoveManually(selectedTerritory, myMovingButton);
               }
               else
               {
                  GameAction outAction = GameAction.UpdateRotateStack;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
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
         GameAction outAction = GameAction.UpdateRotateStack;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ContextMenuLoadedButton(object sender, RoutedEventArgs e)
      {
         if( null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoaded_Button() myGameInstance=null");
            return;
         }
         //--------------------------------------------------
         if (sender is ContextMenu)
         {
            ContextMenu cm = (ContextMenu)sender;
            for (int i = 0; i < cm.Items.Count; ++i) // Gray out all menu items as default
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
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ContextMenu_Loaded(): myGameInstance.Stacks.FindMapItem() returned null for name=" + b.Name);
                  return;
               }
               if (null == mi.TerritoryCurrent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ContextMenu_Loaded(): mi.TerritoryCurrent=null for mi=" + mi.Name);
                  return;
               }
               string? tName = mi.TerritoryCurrent.ToString();
               if (null == tName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ContextMenu_Loaded(): mi.TerritoryCurrent.ToString() for mi=" + mi.Name);
                  return;
               }
               myGameInstance.SelectedStack = myGameInstance.Stacks.Find(tName);
               if (null == myGameInstance.SelectedStack)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ContextMenu_Loaded(): unable to find stack for tName=" + tName + " for mi=" + mi.Name);
                  return;
               }
               //-----------------------------------
               if ((0 < cm.Items.Count) ) // Set either Scatter or Stack
               {
                  if (cm.Items[0] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[0];
                     menuItem.IsEnabled = true;
                     if(true == myGameInstance.SelectedStack.IsStacked)
                        menuItem.Header = "_Scatter";
                     else
                        menuItem.Header = "_Stack";
                  }
               }
               //-----------------------------------
               if (1 < cm.Items.Count) // Gray out the "Rotate Stack" menu item
               {
                  if (cm.Items[1] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[1];
                     if ((1 < myGameInstance.SelectedStack.MapItems.Count) && (true == myGameInstance.SelectedStack.IsStacked) )
                        menuItem.IsEnabled = true;
                  }
               }
               //-----------------------------------
               if ((2 < cm.Items.Count) && (true == mi.IsMoveAllowedToResetThisTurn)) // Gray out the "Retun to Starting Point" menu item
               {
                  if (cm.Items[2] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[0];
                     if ((true == GameEngine.theIsAlien) && (GamePhase.AlienMovement == myGameInstance.GamePhase) && (true == mi.IsMoved))
                        menuItem.IsEnabled = true;
                     else if ((false == GameEngine.theIsAlien) && (GamePhase.TownspersonMovement == myGameInstance.GamePhase) && (true == mi.IsMoved))
                        menuItem.IsEnabled = true;
                  }
               }
               //-----------------------------------
               // Gray out the "Expose" menu item
               if (3 < cm.Items.Count)
               {
                  if (cm.Items[2] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[2];
                     if ((true == mi.IsAlienUnknown) && (false == mi.IsAlienKnown) && ("Zebulon" != mi.Name))
                        menuItem.IsEnabled = true;
                  }
               }
               //-----------------------------------
               if (4 < cm.Items.Count)  // Gray out the "Stop Movement" menu item
               {
                  if (cm.Items[3] is MenuItem)
                  {
                     MenuItem menuItem = (MenuItem)cm.Items[3];
                     bool isMenuEnabled;
                     if (false == IsAlienAbleToStopMove(myGameInstance, mi, out isMenuEnabled))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ContextMenuLoadedButton(): IsAlienAbleToStopMove() returned false");
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
         GameAction outAction = GameAction.UpdateRotateStack;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ContextMenuClickScatter(object sender, RoutedEventArgs e)
      {
         GameAction outAction = GameAction.UpdateScatterStack;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
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
      private void MouseEnterMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         Button b = (Button)sender;
         //if (1 < myGameInstance.PartyMembers.Count)
         //{
         //   myPartyDisplayDialog = new PartyDisplayDialog(myGameInstance, myCanvas, b);
         //   Logger.Log(LogEnum.LE_VIEW_DIALOG_PARTY, "MouseEnterMapItem(): Showing due to 1 < partyCount=" + myGameInstance.PartyMembers.Count.ToString());
         //   myPartyDisplayDialog.Show();
         //}
      }
      private void MouseLeaveMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         //if (null != myPartyDisplayDialog)
         //   myPartyDisplayDialog.Close();
         //myPartyDisplayDialog = null;
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private bool MapItemCommonAction(ITerritory selectedTerritory)
      {
         myGameInstance.SelectedStack = myGameInstance.Stacks.Find(selectedTerritory);
         if( null == myGameInstance.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemCommonAction(): stack=null for t=" + selectedTerritory.ToString());
            return false;
         }
         GameAction outAction = GameAction.UpdateRotateStack;
         myStoryboardFlashing = null;
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.Conversations:
               if (false == GameEngine.theIsAlien)
               {
                  DisplayConversation(myGameInstance, selectedTerritory);
               }
               else
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               break;
            case GamePhase.Influences:
               if (false == GameEngine.theIsAlien)
               {
                  DisplayInfluence(myGameInstance, selectedTerritory);
               }
               else
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               break;
            case GamePhase.Combat:
               DisplayCombat(myGameInstance, selectedTerritory);
               break;
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
                        Logger.Log(LogEnum.LE_ERROR, "MapItem_CommonAction() myGameInstance.Stacks.FindMapItem(\"Zebulon\") returned null");
                        return false;
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
                     myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  }
               }
               break;
            case GamePhase.ImplantRemoval:
               if (false == GameEngine.theIsAlien)
               {
                  DisplayImplantRemoval(myGameInstance, selectedTerritory);
               }
               else
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               break;
            case GamePhase.AlienTakeover:
               if (true == GameEngine.theIsAlien)
               {
                  DisplayTakover(myGameInstance, selectedTerritory);
               }
               else
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               break;
            default:
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
         }
         return true;
      }
      private bool MapItemMoveManually(ITerritory selectedTerritory, Button selectedButton)
      {
         myGameInstance.SelectedStack = myGameInstance.Stacks.Find(selectedTerritory);
         if (null == myGameInstance.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemCommonAction(): stack=null for t=" + selectedTerritory.ToString());
            return false;
         }
         GameAction outAction = GameAction.UpdateRotateStack;
         //----------------------------------------
         if ((null != selectedTerritory) && (null != selectedButton))  // MapItem already selected to move.  Moving it to a known space
         {
            IMapItem? movingMapItem = myGameInstance.Stacks.FindMapItem(selectedButton.Name);
            if (null == movingMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() myGameInstance.Stacks.FindMapItem() returned null for name=" + selectedButton.Name);
               return false;
            }
            if (selectedTerritory.ToString() == movingMapItem.TerritoryCurrent.ToString()) // Rotatate Stack
            {
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
            }
            else
            {
               int movementLeftToUse = movingMapItem.Movement - movingMapItem.MovementUsed;
               if (movementLeftToUse < 1)
               {
                  MessageBox.Show("No movement left for this person, Choose another person to move.");
                  return true;
               }

               movingMapItem.TerritoryStarting = movingMapItem.TerritoryCurrent;
               MapItemMove? mim = new MapItemMove(Territories.theTerritories, movingMapItem, selectedTerritory);
               if(null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() new MapItemMove() returned null");
                  return false;
               }
               if (null == mim.BestPath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItemMoveManually() new MapItemMove() mim.BestPath=null");
                  return false;
               }
               if ((0 == mim.BestPath.Territories.Count) || (null == mim.NewTerritory))
               {
                  if (true == myIsFlagSetForOverstack)
                     MessageBox.Show("Unable to take this path due to overstacking restrictions. Choose another endpoint.");
                  myIsFlagSetForOverstack = true;
                  return true;
               }
               myGameInstance.MapItemMoves.Clear();
               myGameInstance.MapItemMoves.Add(mim);
               if (GamePhase.AlienMovement == myGameInstance.GamePhase)
               {
                  outAction = GameAction.AlienMovement;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               else if (GamePhase.TownspersonMovement == myGameInstance.GamePhase)
               {
                  myIsAlienAbleToStopMove = true; // The townsperson cannot move any more MapItems until a response is received from the Alien player.
                  outAction = GameAction.TownMovementTownPerforms;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
            }
         }
         return true;
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
         //------------------------------------------------------------
         myGameInstance.SelectedStack = myGameInstance.Stacks.Find(selectedMapItem.TerritoryCurrent);
         if (null == myGameInstance.SelectedStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem_ReturnToStart(): stack=null for t=" + selectedMapItem.TerritoryCurrent.ToString());
            return false;
         }
         GameAction outAction = GameAction.UpdateRotateStack;
         //------------------------------------------------------------
         if (false == selectedMapItem.IsMoveAllowedToResetThisTurn) // if not allowed to reset, do nothing
         {
            if ((true == myIsFlagSetForMoveReset) && (true == GameEngine.theIsAlien) && (GamePhase.AlienMovement == myGameInstance.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            if ((true == myIsFlagSetForMoveReset) && (false == GameEngine.theIsAlien) && (GamePhase.TownspersonMovement == myGameInstance.GamePhase))
               MessageBox.Show("Reset Not Allowed");
            myIsFlagSetForMoveReset = true;

            myGameEngine.PerformAction(ref myGameInstance, ref outAction);
            return true;  // do nothing
         }
         switch (myGameInstance.GamePhase)
         {
            case GamePhase.AlienMovement:
               if ((true == selectedMapItem.IsControlled) || (false == GameEngine.theIsAlien))
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  return true;  // do nothing
               }
               break;
            case GamePhase.TownspersonMovement:
               if ((false == selectedMapItem.IsControlled) || (true == GameEngine.theIsAlien))
               {
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  return true;  // do nothing
               }
               break;
            default:
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               return true;  // do nothing
         } // end switch
         //--------------------------------------------------
         StringBuilder sb = new StringBuilder("MapItem_ReturnToStart(): t="); sb.Append(selectedMapItem.TerritoryCurrent.ToString()); sb.Append(" st="); sb.Append(selectedMapItem.TerritoryStarting.ToString());
         Logger.Log(LogEnum.LE_MIM_RETURN_TO_START, sb.ToString());
         if (selectedMapItem.TerritoryCurrent != selectedMapItem.TerritoryStarting)
         {
            foreach (var kvp in myRectangleMaps) // Turn off all animation for rectangles
            {
               kvp.Value.BeginAnimation(Canvas.LeftProperty, null);
               kvp.Value.BeginAnimation(Canvas.TopProperty, null);
            }
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
               outAction = GameAction.ResetMovement;
               myGameEngine.PerformAction(ref myGameInstance, ref outAction); // Inform the user to return back
            }
         }
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
