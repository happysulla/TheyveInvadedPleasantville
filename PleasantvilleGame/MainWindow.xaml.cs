using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace PleasantvilleGame
{
   //---------------------------------------------------------------------
   [Serializable]
   [StructLayout(LayoutKind.Sequential)]
   public struct POINT  // used in WindowPlacement structure
   {
      public int X;
      public int Y;
      public POINT(int x, int y)
      {
         X = x;
         Y = y;
      }
   }
   //-------------------------------------------
   [Serializable]
   [StructLayout(LayoutKind.Sequential)]
   public struct RECT // used in WindowPlacement structure
   {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
      public RECT(int left, int top, int right, int bottom)
      {
         Left = left;
         Top = top;
         Right = right;
         Bottom = bottom;
      }
   }
   //-------------------------------------------
   [Serializable]
   [StructLayout(LayoutKind.Sequential)]
   public struct WindowPlacement // used to save window position between sessions
   {
      public int length;
      public int flags;
      public int showCmd;
      public POINT minPosition;
      public POINT maxPosition;
      public RECT normalPosition;
      public bool IsZero()
      {
         if (0 != length)
            return false;
         if (0 != flags)
            return false;
         if (0 != minPosition.X)
            return false;
         if (0 != minPosition.Y)
            return false;
         if (0 != maxPosition.X)
            return false;
         if (0 != maxPosition.Y)
            return false;
         return true;
      }
   }
   //===========================================================================
   public partial class MainWindow : Window
   {
      public static string theAssemblyDirectory = "";
      private IGameEngine? myGameEngine = null;
      private GameViewerWindow? myGameViewerWindow = null;
      public MainWindow()
      {
         InitializeComponent();
         try
         {
            //--------------------------------------------
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Logger.theLogDirectory = appDataDir + @"\Pleasantville\Logs\";
            if (false == Directory.Exists(Logger.theLogDirectory)) // create directory if does not exists
               Directory.CreateDirectory(Logger.theLogDirectory);
            if (false == Logger.SetInitial()) // setup logger
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): SetInitial() returned false");
               Application.Current.Shutdown();
               return;
            }
            //--------------------------------------------
            Assembly assem = Assembly.GetExecutingAssembly();
            string codeBase = assem.Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            String? assemDir = System.IO.Path.GetDirectoryName(path);
            if (null == assemDir)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            theAssemblyDirectory = assemDir;
            MapImage.theImageDirectory = theAssemblyDirectory + @"\Images\";
            ConfigFileReader.theConfigDirectory = theAssemblyDirectory + @"\Config\";
            //--------------------------------------------
            GameLoadMgr.theGamesDirectory = appDataDir + @"\Pleasantville\Games\";
            if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
            //--------------------------------------------
            GameFeats.theGameFeatDirectory = appDataDir + @"\Pleasantville\GameFeats\";
            if (false == Directory.Exists(GameFeats.theGameFeatDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameFeats.theGameFeatDirectory);
            //--------------------------------------------
            GameStatistics.theGameStatisticsDirectory = appDataDir + @"\Pleasantville\GameStats\";
            if (false == Directory.Exists(GameStatistics.theGameStatisticsDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameStatistics.theGameStatisticsDirectory);
            //--------------------------------------------
            Utilities.InitializeRandomNumGenerators();
            //--------------------------------------------
            IGameInstance gi = new GameInstance();
            if (true == gi.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            //--------------------------------------------
            myGameEngine = new GameEngine(this);
            myGameViewerWindow = new GameViewerWindow(myGameEngine, gi); // Start the main view
            if (true == myGameViewerWindow.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameViewerWindow() ctor error");
               Application.Current.Shutdown();
               return;
            }
            //--------------------------------------------
            string iconFilename = MapImage.theImageDirectory + "PattonsBest.ico";
            Uri iconUri = new Uri(iconFilename, UriKind.Absolute);
            this.Icon = BitmapFrame.Create(iconUri);
            //--------------------------------------------
            myGameViewerWindow.Icon = this.Icon;
            myGameViewerWindow.Show(); // Finished initializing so show the window
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MainWindow() e=" + e.ToString());
            Application.Current.Shutdown();
            return;
         }
      }
      //-----------------------------------------------------------------------
      public void UpdateViews(IGameInstance gi, GameAction action)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateViews() myGameEngine=null");
            return;
         }
         foreach (IView v in myGameEngine.Views)
         {
            if (null == v)
               Logger.Log(LogEnum.LE_ERROR, "UpdateViews(): v=null");
            else
               v.UpdateView(ref gi, action);
         }
      }
   }
}