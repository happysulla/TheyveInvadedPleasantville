using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Control=System.Windows.Controls.Control;
using Label = System.Windows.Controls.Label;

namespace PleasantvilleGame
{
    class StatusBarViewer : IView
    {
        private StatusBar myStatusBar;
      private IGameInstance myGameInstance;
      private IGameEngine myGameEngine;
      private Canvas myCanvas;
      private Cursor? myTargetCursor;
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
                        if( false == GameEngine.theIsAlien )
                            item.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
      //--------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
        {
            StringBuilder sb1 = new StringBuilder("---------------StatusBarViewer::UpdateView() ==> action="); sb1.Append(action.ToString()); sb1.Append("  ==> NextAction="); sb1.Append(gi.NextAction);
            Logger.Log(LogEnum.LE_VIEW_UPDATE_STATUS_BAR, sb1.ToString());

            foreach (Control item in myStatusBar.Items)
            {
                if (item is Label)
                {
                    Label label = (Label)item;
                    if (label.Name == "myLabelInfluenceTotal")
                    {
                        label.Content = "Total Influence=" + gi.InfluenceCountTotal.ToString();
                    }
                    else if (label.Name == "myLabelInfluenceTownspeople")
                    {
                        label.Content = "Town's People Influence=" + gi.InfluenceCountTownspeople.ToString();
                    }
                    else if (label.Name == "myLabelInfluenceAlienKnown")
                    {
                        label.Content = "Alien Influence=" + gi.InfluenceCountAlienKnown.ToString();
                    }
                    else if (label.Name == "myLabelInfluenceAlien")
                    {
                        label.Content = "UnKnown Influence=" + gi.InfluenceCountAlienUnknown.ToString();
                    }
                    else if (label.Name == "myLabelGamePhase")
                    {
                        item.Visibility = Visibility.Visible;
                        label.Content = "Game Phase = " + gi.GamePhase;
                    }
                    else if (label.Name == "myLabelNextAction")
                    {
                        item.Visibility = Visibility.Visible;
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Next Action = ");
                        if ("Decides Where to Perform Combats" == gi.NextAction)
                        {
                            if (true == GameEngine.theIsAlien)
                                sb.Append("Alien ");
                            else
                                sb.Append("Townsperson ");
                        }
                        else if ("Ack Random Movement" == gi.NextAction)
                        {
                            if (true == GameEngine.theIsAlien)
                                sb.Append("Awaiting Alien ");
                            else
                                sb.Append("Awaiting Townsperson ");
                        }
                        else if ("Display Random Movement" == gi.NextAction)
                        {
                            if (true == GameEngine.theIsAlien)
                                sb.Append("Awaiting Alien ");
                            else
                                sb.Append("Awaiting Townsperson ");
                        }
                        sb.Append(gi.NextAction);
                        label.Content = sb.ToString();
                    }
                }
            }
        }
    }
}
