using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
namespace PleasantvilleGame
{
    public partial class DialogStopMovement : Window
    {
        #region Fields (myGameInstance, Controlled, Uncontrolled )
        private IGameInstance myGameInstance = null;
        private IMapItem myMovingTownsperson = null;
        private IMapItems myKnownAliens =  new MapItems();
        #endregion

        #region Properties ( None )
        /// <summary>
        /// This property indicates if alien stopped movement.
        /// </summary>
        private bool myIsMoveStopped = false;
        public bool IsMoveStopped
        {
            get { return myIsMoveStopped; }
        }
        #endregion

        #region Constructor
        public DialogStopMovement(IGameInstance gi, IMapItem movingTownsperson, IMapItems knownAliens)
        {
            InitializeComponent();
            myGameInstance = gi;
            if (null == movingTownsperson)
                return;

            myMovingTownsperson = gi.Persons.Find( movingTownsperson.Name );
            foreach (IMapItem mi in knownAliens)
            {
                IMapItem alien = gi.Persons.Find(mi.Name);
                myKnownAliens.Add(alien);
            }
            
            UpdateView();
        }
        #endregion

        #region void UpdateView()
        public void UpdateView()
        {
            myButton1.Content = "Default";
            myButton2.Content = "Default";
            myButton3.Content = "Default";
            myButton4.Content = "Default";
            myButton1.Visibility = Visibility.Visible;
            myButton2.Visibility = Visibility.Hidden;
            myButton3.Visibility = Visibility.Hidden;
            myButton4.Visibility = Visibility.Hidden;
            Canvas.SetLeft(myButton1, 12);
            Canvas.SetLeft(myButton2, 149);
            Canvas.SetLeft(myButton3, 216);
            Canvas.SetLeft(myButton4, 280);

            myRectangle1.Visibility = Visibility.Visible;
            myRectangle2.Visibility = Visibility.Hidden;
            myRectangle3.Visibility = Visibility.Hidden;
            myRectangle4.Visibility = Visibility.Hidden;
            Canvas.SetLeft(myRectangle1, 12);
            Canvas.SetLeft(myRectangle2, 149);
            Canvas.SetLeft(myRectangle3, 216);
            Canvas.SetLeft(myRectangle4, 280);

            myLabelAlienSelected.Visibility = Visibility.Visible;
            Canvas.SetLeft(myLabelArrow, 77);
            MapItem.SetButtonContent(myButton1, myMovingTownsperson);

            switch (myKnownAliens.Count)
            {
                case 1:
                    Canvas.SetLeft(myButton1, 12+50);
                    Canvas.SetLeft(myRectangle1, 12 + 50);
                    Canvas.SetLeft(myLabelArrow, 77+50);
                    Canvas.SetLeft(myButton2, 149+50);
                    Canvas.SetLeft(myRectangle2, 149 + 50);
                    myButton2.Visibility = Visibility.Visible;
                    MapItem.SetButtonContent(myButton2, myKnownAliens[0]);
                    break;
                case 2:
                    Canvas.SetLeft(myButton1, 12+25);
                    Canvas.SetLeft(myRectangle1, 12 + 25);
                    Canvas.SetLeft(myLabelArrow, 77+25);
                    Canvas.SetLeft(myButton2, 149+25);
                    Canvas.SetLeft(myRectangle2, 149 + 25);
                    Canvas.SetLeft(myButton3, 216+25);
                    Canvas.SetLeft(myRectangle3, 216 + 25);
                    myButton2.Visibility = Visibility.Visible;
                    myButton3.Visibility = Visibility.Visible;
                    MapItem.SetButtonContent(myButton2, myKnownAliens[0]);
                    MapItem.SetButtonContent(myButton3, myKnownAliens[1]);
                    break;
                case 3:
                    myButton2.Visibility = Visibility.Visible;
                    myButton3.Visibility = Visibility.Visible;
                    myButton4.Visibility = Visibility.Visible;
                    MapItem.SetButtonContent(myButton2, myKnownAliens[0]);
                    MapItem.SetButtonContent(myButton3, myKnownAliens[1]);
                    MapItem.SetButtonContent(myButton4, myKnownAliens[2]);
                    break;
                default:
                    Console.WriteLine("DialogStopMovement:UpdateView() ERROR - reached default");
                    break;
            }
        }
        #endregion

        #region myButtonOk_Click()
        private void myButtonOk_Click(object sender, RoutedEventArgs e)
        {
            myButton1.Content = "Closing";
            myButton2.Content = "Closing";
            myButton3.Content = "Closing";
            myButton4.Content = "Closing";
            Close();
        }
        #endregion

        #region void StopMove(IMapItem alien)
        private void StopMove(IMapItem alien)
        {
            myIsMoveStopped = true;

            alien.IsMoveStoppedThisTurn = true;
            myMovingTownsperson.IsMoveStoppedThisTurn = true;
            myMovingTownsperson.IsMoveAllowedToResetThisTurn = false;

            if (false == alien.IsAlienKnown)
                myGameInstance.AddKnownAlien(alien);
            myGameInstance.MapItemMoves.Clear();
        }
        #endregion

        private void myButton2_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility.Hidden == myRectangle2.Visibility)
            {
                myRectangle2.Visibility = Visibility.Visible;
                if (myKnownAliens.Count < 1)
                    return;
                StopMove( myKnownAliens[0]);
            }
            
        }
        private void myButton3_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility.Hidden == myRectangle3.Visibility)
            {
                myRectangle3.Visibility = Visibility.Visible;
                if (myKnownAliens.Count < 2)
                    return;
                StopMove(myKnownAliens[1]);
            }

        }
        private void myButton4_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility.Hidden == myRectangle4.Visibility)
            {
                myRectangle4.Visibility = Visibility.Visible;
                if (myKnownAliens.Count < 3)
                    return;
                StopMove(myKnownAliens[2]);
            }
        }
    }
}
