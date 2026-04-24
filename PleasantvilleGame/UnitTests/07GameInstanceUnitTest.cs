using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using Image=System.Windows.Controls.Image;

namespace PleasantvilleGame
{
   internal class GameInstanceUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMain = null;
      private Canvas? myCanvasHelper = null;
      //private IGameInstance? myGameInstanceSave = null;
      //private IGameInstance? myGameInstanceLoad = null;
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public GameInstanceUnitTest(DockPanel dp)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("07-Save Game");
         myHeaderNames.Add("07-Load Game");
         myHeaderNames.Add("07-Compare");
         myHeaderNames.Add("07-Finish");
         //------------------------------------
         myCommandNames.Add("Save Game");
         myCommandNames.Add("Load Game");
         myCommandNames.Add("Compare");
         myCommandNames.Add("Finish");
         //------------------------------------
         myDockPanelTop = dp; // top most dock panel that holds menu, statusbar, left dockpanel, and right dockpanel
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
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                        if (ui2 is Canvas)
                        {
                           myCanvasHelper = (Canvas)ui2;
                        }
                     }
                  }
               }
            }
         }
         if (null == myCanvasMain) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasHelper) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasHelper=null");
            CtorError = true;
            return;
         }
      }
      //--------------------------------------------------------------------
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
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
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasTank=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //----------------------------------------------------b-
         if (CommandName == myCommandNames[0])
         {
         }
         else if( CommandName == myCommandNames[1])
         {
          
            ++myIndexName;
         }
         else if (CommandName == myCommandNames[2])
         {
         

         }
         else if (CommandName == myCommandNames[3])
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
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
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
         ++gi.GameTurn; // moves to next unit test
         return true;
      }
      //--------------------------------------------------------------------
   }
}
