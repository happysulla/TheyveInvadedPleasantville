
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using Point = System.Windows.Point;
using CheckBox = System.Windows.Controls.CheckBox;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;
using Rectangle = System.Windows.Shapes.Rectangle;
using Brush = System.Windows.Media.Brush;

namespace PleasantvilleGame
{
   public class CanvasImageViewer : IView
   {
      public bool CtorError { get; } = false;
      private Canvas? myCanvas = null;
      private IDieRoller? myDieRoller = null;
      private System.Windows.Input.Cursor? myTargetCursor = null;
      //-------------------------------------------------
      public CanvasImageViewer(Canvas? c, IDieRoller? dr)
      {
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): dr=null");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
      }
      //-------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myCanvas=null");
            return;
         }
         switch (action)
         {
            case GameAction.UpdateStatusBar:
               if (null != myTargetCursor) // increase/decrease size of cursor when zoom in or out
               {
                  myTargetCursor.Dispose();
                  double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
                  System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFire"), Width = sizeCursor, Height = sizeCursor };
                  myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
                  this.myCanvas.Cursor = myTargetCursor;
               }
               break;
            case GameAction.UpdateNewGame:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = System.Windows.Input.Cursors.Arrow; // get rid of the canvas cursor
               break;
            case GameAction.UpdateNewGameEnd:
               ShowGameMap(myCanvas);
               break;
            case GameAction.UpdateLoadingGame:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = System.Windows.Input.Cursors.Arrow; // get rid of the canvas cursor
               break;
            case GameAction.UpdateUndo:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = System.Windows.Input.Cursors.Arrow; // get rid of the canvas cursor
               break;
            case GameAction.EndGameWin:
               ShowEndGameSuccess(myCanvas);
               break;
            case GameAction.EndGameLost:
               ShowEndGameFail(myCanvas);
               break;
            default:
               break;
         }
      }
      //-------------------------------------------------
      public void CleanCanvas(Canvas c)
      {
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in c.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Rectangle rectangle)
                 elements.Add(ui);
            if (ui is Ellipse ellipse)
            {
               if ("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            if (ui is System.Windows.Controls.Label label)  // A Game Feat Label
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Die"))
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            c.Children.Remove(ui1);
      }
      public void ShowEndGameSuccess(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         int randomNum = Utilities.RandomGenerator.Next(10);
         bmi1.BeginInit();
         if (6 < randomNum)
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess.gif", UriKind.Absolute);
         else
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess2.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      public void ShowEndGameFail(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         bmi1.BeginInit();
         bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameFail.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowGameMap(Canvas c)
      {
         double width = Math.Min(c.Width, c.Height*0.86);

         Image img = new Image() { Name = "CanvasMain", Width = width, Height = c.Height, Source = MapItem.theMapImages.GetBitmapImage("PleasantvilleMap") };
         c.Children.Add(img);
         double x = (c.ActualWidth - img.Width) * 0.5;
         double y = (c.ActualHeight - img.Height) * 0.5;
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
      }
   }
}
