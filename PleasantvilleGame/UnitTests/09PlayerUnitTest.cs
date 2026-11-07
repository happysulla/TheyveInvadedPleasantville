using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Windows.Controls.Image;


namespace PleasantvilleGame
{
   public class PlayerUnitTest : IUnitTest
   {
      private IGameInstance? myGameInstance = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private Canvas? myCanvasMain = null;
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasHelper = null;
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------------------
      public PlayerUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ, GameViewerWindow gvw)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("09-Centers");
         myHeaderNames.Add("09-Create Moves1");
         myHeaderNames.Add("09-Create Moves2");
         myHeaderNames.Add("09-Player");
         myHeaderNames.Add("09-Player");

         myCommandNames.Add("00-Show Centers");
         myCommandNames.Add("01-Move Away From Controlled");
         myCommandNames.Add("02-Move Away From Controlled");
         myCommandNames.Add("03-InitializePlayer");
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
         //------------------------------------
         if (null == dp)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): dp=null");
            CtorError = true;
            return;
         }
         myDockPanelTop = dp;
         //------------------------------------
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewerCanvas = (ScrollViewer)ui1;
                     if (myScrollViewerCanvas.Content is Canvas)
                        myCanvasMain = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvasMain) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myGameInstance=null");
            return false;
         }
         if (null == myCanvasImageViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasImageViewer=null");
            return false;
         }
         if (null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         //-----------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            gi.PlayerAlien = new PlayerAlienComputer();
            gi.PlayerTown = new PlayerTownHuman();
            if (false == TableMgr.CreateTownspeople(myGameInstance)) // Create in known locations
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): CreateTownspeople() returned false");
               return false;
            }
            IMapItem? mi = gi.Stacks.FindMapItem(TableMgr.WIFE); 
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): unable to find mi=" + TableMgr.WIFE);
               return false;
            }
            gi.AddUnknownAlien(mi);
            mi = gi.Stacks.FindMapItem(TableMgr.SHERIFF); 
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): unable to find mi=" + TableMgr.SHERIFF);
               return false;
            }
            gi.AddUnknownAlien(mi);
            mi = gi.Stacks.FindMapItem(TableMgr.MINSTER);
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): unable to find mi=" + TableMgr.MINSTER);
               return false;
            }
            gi.AddControlled(mi);
            IMapPoint mpCenterControlled = Territory.GetCenterPointControlled(gi);
            CreateEllipse(mpCenterControlled, Utilities.theTownControlledBrush);
            IMapPoint mpCenterAlien = Territory.GetCenterPointAlien(gi);
            CreateEllipse(mpCenterAlien, Utilities.theAlienControlledBrush);
            myIndexName++;
         }
         else if (CommandName == myCommandNames[1])
         {
            Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "Perform_AlienMoves(): gi.MapItemMoves.Clear()");
            gi.MapItemMoves.Clear();
            PlayerAlienComputerMoveMgr moveMgr = new PlayerAlienComputerMoveMgr();
            if (false == moveMgr.CreateMoves(gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "PlayerAlienComputer.Perform_AlienMoves() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[2])
         {

         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvas=null");
            return false;
         }
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               elements.Add(ui);
            }
            else if (ui is Polygon polygon)
               elements.Add(ui);
            else if (ui is Polyline polyline)
               elements.Add(ui);
            else if (ui is Ellipse ellipse)
               elements.Add(ui);
            else if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         //--------------------------------------------------
         ++gi.GameTurn;
         System.Windows.Application.Current.Shutdown();
         return true;
      }
      //--------------------------------------------------------------------
      private void CreateEllipse(IMapPoint mp, System.Windows.Media.Brush brush)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvas=null");
            return;
         }
         Ellipse aEllipse = new Ellipse
         {
            Fill = brush,
            StrokeThickness = 1,
            Stroke = brush,
            Width = 20,
            Height = 20
         };
         Canvas.SetLeft(aEllipse, mp.X-10);
         Canvas.SetTop(aEllipse, mp.Y-10);
         Canvas.SetZIndex(aEllipse, 10000);
         myCanvasMain.Children.Add(aEllipse);
      }
   }
}
