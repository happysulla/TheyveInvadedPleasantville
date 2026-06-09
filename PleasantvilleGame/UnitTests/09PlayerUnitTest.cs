using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;
using Brushes = System.Windows.Media.Brushes;


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
         myHeaderNames.Add("09-Player");
         myHeaderNames.Add("09-GetAlienCount");
         myHeaderNames.Add("09-GetTownCount");
         myHeaderNames.Add("09-Player");
         myHeaderNames.Add("09-Player");
         myCommandNames.Add("Player");
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         if (null == dp)
         {
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest(): dp=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         myCanvasImageViewer = civ;
         myDockPanelTop = dp;
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (null == myCanvasHelper)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasHelper=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //-----------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            gi.PlayerAlien = new PlayerAlienComputer();
            gi.PlayerTown = new PlayerTownHuman();
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
   }
}
