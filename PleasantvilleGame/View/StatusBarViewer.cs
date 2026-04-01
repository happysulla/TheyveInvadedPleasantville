using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using PleasantvilleRemote;

namespace Pleasantville
{
    class StatusBarViewer : IView
    {
        #region Fields ( myStatusBar )
        private StatusBar myStatusBar;
        private bool myIsAlien = false;
        #endregion

        #region Properties( none )
        #endregion

        #region Constructor 
        public StatusBarViewer( StatusBar sb, bool isAlien )
        {
            myStatusBar = sb;
            myIsAlien = isAlien;

            foreach (Control item in myStatusBar.Items)
            {
                if (item is Label)
                {
                    Label label = (Label)item;
                    if (label.Name == "myLabelInfluenceAlien")
                    {
                        if( false == isAlien )
                            item.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
        #endregion

        #region Updateview(ref IGameInstance gi, GameAction action)
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
                            if (true == myIsAlien)
                                sb.Append("Alien ");
                            else
                                sb.Append("Townsperson ");
                        }
                        else if ("Ack Random Movement" == gi.NextAction)
                        {
                            if (true == myIsAlien)
                                sb.Append("Awaiting Alien ");
                            else
                                sb.Append("Awaiting Townsperson ");
                        }
                        else if ("Display Random Movement" == gi.NextAction)
                        {
                            if (true == myIsAlien)
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
        #endregion
    }
}
