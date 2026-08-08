using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using Image=System.Windows.Controls.Image;
using Button=System.Windows.Controls.Button;

namespace PleasantvilleGame
{
   internal class TableMgrUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private IGameInstance? myGameInstance = null;
      private GameViewerWindow? myGameViewerWindow = null;
      private DockPanel? myDockPanelTop = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMain = null;
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TableMgrUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ, GameViewerWindow gvw)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("08-Create Town People");
         myHeaderNames.Add("08-Shuffle MapItems");
         myHeaderNames.Add("08-Finish");
         //------------------------------------
         myCommandNames.Add("00-Create Town Peope");
         myCommandNames.Add("01-Shuffle");
         myCommandNames.Add("02-Finish");
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == gvw)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): gvw=null");
            CtorError = true;
            return;
         }
         myGameViewerWindow = gvw;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
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
         if (null == myGameViewerWindow)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myGameViewerWindow=null");
            return false;
         }
         if (null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasMain=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //---------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            if( false == TableMgr.CreateTownspeople(myGameInstance))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): CreateTownspeople() returned false");
               return false;
            }
            string tName = "House_K";
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            //-----------------
            IMapItem? wife = myGameInstance.Stacks.FindMapItem("Wife"); // move wife to House_K
            if (null == wife)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find wife");
               return false;
            }
            myGameInstance.Stacks.Remove(wife); // remove from existing stack
            wife.TerritoryCurrent = wife.TerritoryStarting = t;
            wife.Location.X = t.CenterPoint.X;
            wife.Location.Y = t.CenterPoint.Y;
            myGameInstance.Stacks.Add(wife); // add to new stack
            //-----------------
            IMapItem? sheriff = myGameInstance.Stacks.FindMapItem("Sheriff");  // move sheriff to House_K
            if (null == sheriff)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find sheriff" );
               return false;
            }
            myGameInstance.Stacks.Remove(sheriff); // remove from existing stack
            sheriff.TerritoryCurrent = sheriff.TerritoryStarting = t;
            sheriff.Location.X = t.CenterPoint.X;
            sheriff.Location.Y = t.CenterPoint.Y;
            myGameInstance.Stacks.Add(sheriff); // add to new stack
            ++myIndexName;
         } 
         //-----------------------------------------
         else if (CommandName == myCommandNames[1])
         {
            string tName = "House_K";
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? stack = myGameInstance.Stacks.Find(t);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find stack=null for House_K");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_UNIT_TEST, "Command(): stack=\n" + myGameInstance.Stacks.ToString());
            IMapItems shuffleMapItems = stack.MapItems.Shuffle();
            stack.MapItems = shuffleMapItems;
            Logger.Log(LogEnum.LE_SHOW_UNIT_TEST, "Command(): stack=\n" + myGameInstance.Stacks.ToString());
         }
         //-----------------------------------------
         else 
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvasMain=null");
            return false;
         }
         //--------------------------------------
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
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasMain=null");
            return false;
         }
         CleanCanvas(gi, myCanvasMain);
         ++gi.GameTurn; // Move to next unit test
         return true;
      }
      //--------------------------------------------------------------------
      private void CleanCanvas(IGameInstance gi, Canvas canvas)
      {
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in canvas.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Map")) 
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
            if (ui is Button button)
            {
               if (true == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
                  continue;
            }
         }
         foreach (UIElement ui1 in elements)
            canvas.Children.Remove(ui1);
      }
   }
}
