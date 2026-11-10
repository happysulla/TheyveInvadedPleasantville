using Microsoft.AspNetCore.Mvc;
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
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;


namespace PleasantvilleGame
{
   public class PlayerUnitTest : IUnitTest
   {
      private static Double theEllipseDiameter = 20;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      private IGameInstance? myGameInstance = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private Canvas? myCanvasMain = null;
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
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
            Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest.Command(): myCanvas=null");
            return false;
         }
         //=========================================================
         if (CommandName == myCommandNames[0])
         {
            gi.PlayerAlien = new PlayerAlienComputer();
            gi.PlayerTown = new PlayerTownHuman();
            if (false == TableMgr.CreateTownspeople(myGameInstance)) // Create in known locations
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): CreateTownspeople() returned false");
               return false;
            }
            //--------------------------------------------
            string startingHq = StartingHqMgr.GetStartingHqTerritory();
            ITerritory? tZebutonStart = Territories.theTerritories.Find(startingHq);
            if (null == tZebutonStart)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): zTerritory=null");
               return false;
            }
            gi.Zebulon.TerritoryCurrent = gi.Zebulon.TerritoryStarting = tZebutonStart;
            gi.Zebulon.Location = Territory.GetRandomPointBoundingRect(tZebutonStart, gi.Zebulon.Zoom * Utilities.theMapItemOffset);
            gi.Zebulon.IsAlienKnown = true;
            gi.Stacks.Add(gi.Zebulon);
            //--------------------------------------------
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
         //=========================================================
         else if (CommandName == myCommandNames[1])
         {
            DeleteEllipses();
            IMapItem? mi1 = gi.Stacks.FindMapItem(TableMgr.HOTEL_OWNER);
            if (null == mi1)
            {
               Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest.Command(): mi1=null ");
               return false;
            }
            if (false == CreateEllipse(mi1.TerritoryCurrent, mi1.TerritoryCurrent.CenterPoint, Colors.Black))
            {
               Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest.Command(): Create_Ellipse() returned false for mi1");
               return false;
            }
            IMapItem? mi2 = gi.Stacks.FindMapItem(TableMgr.MINSTER);
            if (null == mi2)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): mi1=null ");
               return false;
            }
            if (false == CreateEllipse(mi1.TerritoryCurrent, mi2.TerritoryCurrent.CenterPoint, Colors.Red))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): Create_Ellipse() returned false for mi1");
               return false;
            }
            ITerritories territoriesToDisplay = Territory.GetTerritoriesOfInterest(Territories.theTerritories, mi2, mi1, false);
            if (false == CreateEllipsesForDisplay(territoriesToDisplay))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipsesForDisplay() returned false");
               return false;
            }
            //-------------------------
            Logger.Log(LogEnum.LE_SHOW_MIM_CLEAR, "PlayerUnitTest.Command(): gi.MapItemMoves.Clear()");
            gi.MapItemMoves.Clear();
            PlayerAlienComputerMoveMgr moveMgr = new PlayerAlienComputerMoveMgr();
            if (false == moveMgr.CreateMoves(gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "PlayerUnitTest.Command():  CreateMoves() returned false");
               return false;
            }
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
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
      private bool CreateEllipse(ITerritory territory, IMapPoint mp, System.Windows.Media.Color color)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Creat_eEllipse(): myCanvasMain=null");
            return false;
         }
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = color };
         Ellipse aEllipse = new Ellipse
         {
            Name = territory.ToString(),
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         myCanvasMain.Children.Add(aEllipse);
         Canvas.SetLeft(aEllipse, mp.X);
         Canvas.SetTop(aEllipse, mp.Y);
         Canvas.SetZIndex(aEllipse, 99999);
         return true;
      }
      private bool CreateEllipsesForDisplay(ITerritories territories)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_EllipsesForDisplay(): myCanvasMain=null");
            return false;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = System.Windows.Media.Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in territories)
         {
            Ellipse aEllipse = new Ellipse() { Name = t.ToString() };
            aEllipse.Fill = aSolidColorBrush0;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = theEllipseDiameter;
            aEllipse.Height = theEllipseDiameter;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            myCanvasMain.Children.Add(aEllipse);
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            Canvas.SetZIndex(aEllipse, 10000);
            //-------------------------
            Label aLabel = new Label() { Foreground = Brushes.Red, FontFamily = myFontFam, FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = t.ToString() };
            p.X -= theEllipseOffset;
            p.Y -= 2 * theEllipseOffset;
            myCanvasMain.Children.Add(aLabel);
            Canvas.SetLeft(aLabel, p.X);
            Canvas.SetTop(aLabel, p.Y);
            Canvas.SetZIndex(aLabel, 10000);
         }
         return true;
      }
      private bool DeleteEllipses()
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipses(): myCanvasMain=null");
            return false;
         }
         List<UIElement> results1 = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results1.Add(ui);
            if (ui is Label)
               results1.Add(ui);
         }
         foreach (UIElement ui1 in results1)
            myCanvasMain.Children.Remove(ui1);
         return true;
      }
   }
}
