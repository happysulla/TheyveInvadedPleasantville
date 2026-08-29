using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Control = System.Windows.Controls.Control;
using Label = System.Windows.Controls.Label;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;

namespace PleasantvilleGame
{
   class StatusBarViewer : IView
   {
      private StatusBar myStatusBar;
      private IGameInstance myGameInstance;
      private IGameEngine myGameEngine;
      private Canvas myCanvas;
      private Cursor? myTargetCursor;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private readonly Thickness myThickness = new Thickness(10, 0, 10, 0);
      //--------------------------------------------------------------
      public StatusBarViewer(StatusBar sb, IGameEngine ge, IGameInstance gi, Canvas c)
      {
         myStatusBar = sb;
         myGameInstance = gi;
         myGameEngine = ge;
         myCanvas = c;
         foreach (Control item in myStatusBar.Items)
         {
            if (item is Label)
            {
               Label label = (Label)item;
               if (label.Name == "myLabelInfluenceAlien")
               {
                  if ((GameType.MultiPlayerJoin == GameEngine.theGameType) || (GameType.SinglePlayerAlien == GameEngine.theGameType))
                     item.Visibility = Visibility.Hidden;
               }
            }
         }
      }
      //--------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         Logger.Log(LogEnum.LE_VIEW_UPDATE_STATUS_BAR, "---------------StatusBarViewer::UpdateView() ==> a=" + action.ToString());
         if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action))
         {
            myGameInstance = gi;
         }
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvas.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------

         switch (action)
         {
            case GameAction.UpdateNewGame:
            case GameAction.UpdateLoadingGame:
            case GameAction.UpdateUndo:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = Cursors.Arrow; // get rid of the canvas cursor
               break;
            default:
               break;
         }
         //--------------------------------------------
         myStatusBar.Items.Clear();
         System.Windows.Controls.Button buttonZoomIn = new System.Windows.Controls.Button { Content = " - ", FontFamily = myFontFam1, Width = 30, Height = 15 };
         buttonZoomIn.Click += ButtonZoomIn_Click;
         myStatusBar.Items.Add(buttonZoomIn);
         Label labelOr = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = "or" };
         myStatusBar.Items.Add(labelOr);
         System.Windows.Controls.Button buttonZoomOut = new System.Windows.Controls.Button { Content = " + ", FontFamily = myFontFam1, Width = 30, Height = 15 };
         buttonZoomOut.Click += ButtonZoomOut_Click;
         myStatusBar.Items.Add(buttonZoomOut);
         StringBuilder sbZ = new StringBuilder("Zoom=");
         sbZ.Append(Utilities.ZoomCanvas.ToString("#.##"));
         Label labelZoom = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbZ.ToString() };
         myStatusBar.Items.Add(labelZoom);
         //--------------------------------------------
         if( GamePhase.GameSetup != myGameInstance.GamePhase)
         {
            myStatusBar.Items.Add(new Separator());
            int uncontrolledInfluence = 0;
            int controlledInfluence = 0;
            int alienInfluence = 0;
            int unknownAlienInfluence = 0;
            foreach (IStack stack in myGameInstance.Stacks)
            {
               foreach (MapItem mi in stack.MapItems)
               {
                  if ((true == mi.IsKilled) || (true == mi.IsKnockedout) || (true == mi.IsStunned))
                     continue;
                  else if (true == mi.IsControlled)
                     controlledInfluence += mi.Influence;
                  else if (true == mi.IsAlienKnown)
                     alienInfluence += mi.Influence;
                  else
                  {
                     uncontrolledInfluence += mi.Influence;
                     if( true == mi.IsAlienUnknown)
                        unknownAlienInfluence += mi.Influence;
                  }
               }
            }
            int totalInfluence = controlledInfluence + alienInfluence + uncontrolledInfluence;
            StringBuilder sbInfluence = new StringBuilder("INFLUENCES: Total=");
            sbInfluence.Append(totalInfluence.ToString());
            sbInfluence.Append("   Uncontrolled=");
            sbInfluence.Append(uncontrolledInfluence.ToString());
            sbInfluence.Append("   Town=");
            sbInfluence.Append(controlledInfluence.ToString());
            sbInfluence.Append("   Known Alien=");
            sbInfluence.Append(alienInfluence.ToString());
            sbInfluence.Append("   Unknown Alien=");
            sbInfluence.Append(unknownAlienInfluence.ToString());
            Label labelUncontrolled = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbInfluence.ToString() };
            myStatusBar.Items.Add(labelUncontrolled);
         }

      }
      //--------------------------------------------------------------
      private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
      {
         if (Utilities.ZoomCanvas < 3.0)
         {
            Utilities.ZoomCanvas += 0.25;
            myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
            UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
         }
      }
      private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
      {
         if (0.25 < Utilities.ZoomCanvas)
         {
            Utilities.ZoomCanvas -= 0.25;
            myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
            UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
         }
      }
   }
}
