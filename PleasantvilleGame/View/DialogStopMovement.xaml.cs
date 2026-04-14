using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
namespace PleasantvilleGame
{
   public partial class DialogStopMovement : Window
   {
      public bool myIsCtorError = false;
      private IGameInstance? myGameInstance = null;
      private IMapItem? myMovingTownsperson = null;
      private IMapItems myKnownAliens = new MapItems();
      private bool myIsMoveStopped = false;
      public bool IsMoveStopped { get { return myIsMoveStopped; } }
      //-----------------------------------------------------------
      public DialogStopMovement(IGameInstance gi, IMapItem movingTownsperson, IMapItems knownAliens)
      {
         InitializeComponent();
         myGameInstance = gi;
         myMovingTownsperson = movingTownsperson;
         foreach (IMapItem mi in knownAliens)
            myKnownAliens.Add(mi);
         if( false == UpdateView() )
         {
            Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:DialogStopMovement() - UpdateView() returned false");
            myIsCtorError = true;
         }
      }
      public bool UpdateView()
      {
         if( null == myMovingTownsperson )
         {
            Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - myMovingTownsperson is null");
            return false;
         }  
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
         MapItem.SetButtonContent(myButton1, myMovingTownsperson, true);
         IMapItem? alien0 = null;
         IMapItem? alien1 = null;
         IMapItem? alien2 = null;
         switch (myKnownAliens.Count)
         {
            case 1:
               Canvas.SetLeft(myButton1, 12 + 50);
               Canvas.SetLeft(myRectangle1, 12 + 50);
               Canvas.SetLeft(myLabelArrow, 77 + 50);
               Canvas.SetLeft(myButton2, 149 + 50);
               Canvas.SetLeft(myRectangle2, 149 + 50);
               myButton2.Visibility = Visibility.Visible;
               alien0 = myKnownAliens[0];
               if (null == alien0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien0 is null");
                  return false;
               }
               MapItem.SetButtonContent(myButton2, alien0, true);
               break;
            case 2:
               Canvas.SetLeft(myButton1, 12 + 25);
               Canvas.SetLeft(myRectangle1, 12 + 25);
               Canvas.SetLeft(myLabelArrow, 77 + 25);
               Canvas.SetLeft(myButton2, 149 + 25);
               Canvas.SetLeft(myRectangle2, 149 + 25);
               Canvas.SetLeft(myButton3, 216 + 25);
               Canvas.SetLeft(myRectangle3, 216 + 25);
               myButton2.Visibility = Visibility.Visible;
               myButton3.Visibility = Visibility.Visible;
               alien0 = myKnownAliens[0];
               if (null == alien0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien0 is null");
                  return false;
               }
               alien1 = myKnownAliens[1];
               if (null == alien1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien1 is null");
                  return false;
               }
               MapItem.SetButtonContent(myButton2, alien0, true);
               MapItem.SetButtonContent(myButton3, alien1, true);
               break;
            case 3:
               myButton2.Visibility = Visibility.Visible;
               myButton3.Visibility = Visibility.Visible;
               myButton4.Visibility = Visibility.Visible;
               alien0 = myKnownAliens[0];
               if (null == alien0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien0 is null");
                  return false;
               }
               alien1 = myKnownAliens[1];
               if (null == alien1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien1 is null");
                  return false;
               }
               alien2 = myKnownAliens[2];
               if (null == alien2)
               {
                  Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - alien2 is null");
                  return false;
               }
               MapItem.SetButtonContent(myButton2, alien0, true);
               MapItem.SetButtonContent(myButton3, alien1, true);
               MapItem.SetButtonContent(myButton4, alien2, true);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:UpdateView() - reached default");
               return false;
         }
         return true;
      }
      //----------------HELPER FUNCTIONS------------------
      private bool StopMove(IMapItem? alien)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:StopMove() - myGameInstance is null");
            return false;
         }
         if ( null == alien)
         {
            Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:StopMove() - alien is null");
            return false;
         }
         alien.IsMoveStoppedThisTurn = true;
         myIsMoveStopped = true;
         if (null == myMovingTownsperson)
         {
            Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:StopMove() - myMovingTownsperson is null");
            return false;
         }
         myMovingTownsperson.IsMoveStoppedThisTurn = true;
         myMovingTownsperson.IsMoveAllowedToResetThisTurn = false;
         if (false == alien.IsAlienKnown)
         {
            myGameInstance.AddKnownAlien(alien);
         }
         myGameInstance.MapItemMoves.Clear();
         return true;
      }
      //----------------CONTROLLER FUNCTIONS------------------
      private void myButtonOk_Click(object sender, RoutedEventArgs e)
      {
         myButton1.Content = "Closing";
         myButton2.Content = "Closing";
         myButton3.Content = "Closing";
         myButton4.Content = "Closing";
         Close();
      }
      private void myButton2_Click(object sender, RoutedEventArgs e)
      {
         if (Visibility.Hidden == myRectangle2.Visibility)
         {
            myRectangle2.Visibility = Visibility.Visible;
            if (myKnownAliens.Count < 1)
               return;
            IMapItem? alien = myKnownAliens[0];
            if( null == alien)
            {
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - myKnownAliens[0] is null");
               return;
            }
            if (false == StopMove(alien))
              Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - StopMove() returned false");
         }
      }
      private void myButton3_Click(object sender, RoutedEventArgs e)
      {
         if (Visibility.Hidden == myRectangle3.Visibility)
         {
            myRectangle3.Visibility = Visibility.Visible;
            if (myKnownAliens.Count < 2)
               return;
            IMapItem? alien = myKnownAliens[1];
            if (null == alien)
            {
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - myKnownAliens[1] is null");
               return;
            }
            if (false == StopMove(alien))
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - StopMove() returned false");
         }

      }
      private void myButton4_Click(object sender, RoutedEventArgs e)
      {
         if (Visibility.Hidden == myRectangle4.Visibility)
         {
            myRectangle4.Visibility = Visibility.Visible;
            if (myKnownAliens.Count < 3)
               return;
            IMapItem? alien = myKnownAliens[2];
            if (null == alien)
            {
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - myKnownAliens[2] is null");
               return;
            }
            if (false == StopMove(alien))
               Logger.Log(LogEnum.LE_ERROR, "DialogStopMovement:myButton2_Click() - StopMove() returned false");
         }
      }
   }
}
