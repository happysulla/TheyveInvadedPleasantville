using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Control = System.Windows.Controls.Control;

namespace PleasantvilleGame
{
   class MainMenuViewer : IView
   {
      public Options? NewGameOptions { get; set; } = null;  // These options take affect when new game menu item is selected
      private readonly IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      private readonly Menu myMainMenu;                     // Top level menu items: File | View | Options | Help
      private readonly MenuItem myMenuItemTopLevel1 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel2 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel3 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel4 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel21 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel22 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel23 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel31 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel36 = new MenuItem();
      private bool myIsZebulonTerritoriesVisible = false;
      private bool myIsTownspersonStarted = false;
      private bool myIsAlienStarted = false;
      private bool myIsTownspersonAcked = false;
      private bool myIsAlienAcked = false;
      //=======================================================
      public MainMenuViewer(IGameEngine ge, IGameInstance gi, Menu mi)
      {
         myGameEngine = ge;
         myGameInstance = gi;
         myMainMenu = mi;
         //------------------------------------------
         foreach (Control item in myMainMenu.Items) // Initialize all the menu items
         {
            if (item is MenuItem menuItem)
            {
               if (menuItem.Name == "myMenuItemTopLevel1")
               {
                  myMenuItemTopLevel1 = menuItem;
                  myMenuItemTopLevel1.Header = "_File";
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel2")
               {
                  myMenuItemTopLevel2 = menuItem;
                  myMenuItemTopLevel2.Header = "_Edit";
                  myMenuItemTopLevel2.Visibility = Visibility.Visible;
                  myMenuItemTopLevel21.Header = "_Undo";
                  myMenuItemTopLevel21.InputGestureText = "Ctrl+Z";
                  myMenuItemTopLevel21.IsEnabled = false;
                  myMenuItemTopLevel21.Click += MenuItemEditUndo_Click;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel21);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel3")
               {
                  myMenuItemTopLevel3 = menuItem;
                  myMenuItemTopLevel3.Header = "_View";
                  myMenuItemTopLevel3.Visibility = Visibility.Visible;
                  MenuItem subItem35 = new MenuItem();
                  subItem35.Header = "_Game Feats...";
                  subItem35.InputGestureText = "Ctrl+Shift+G";
                  subItem35.Click += MenuItemViewFeats_Click;
                  myMenuItemTopLevel3.Items.Add(subItem35);
                  MenuItem subItem37 = new MenuItem();
                  subItem37.Header = "_Other Games...";
                  subItem37.InputGestureText = "Ctrl+Shift+O";
                  subItem37.Click += MenuItemViewOtherGames_Click;
                  myMenuItemTopLevel3.Items.Add(subItem37);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel4")
               {
                  myMenuItemTopLevel4 = menuItem;
                  myMenuItemTopLevel4.Header = "_Help";
                  myMenuItemTopLevel4.Visibility = Visibility.Visible;
                  MenuItem subItem41 = new MenuItem();
                  subItem41.Header = "_Rules...";
                  subItem41.InputGestureText = "F1";
                  subItem41.Click += MenuItemHelpRules_Click;
                  myMenuItemTopLevel4.Items.Add(subItem41);
                  MenuItem subItem42 = new MenuItem();
                  subItem42.Header = "_Events...";
                  subItem42.InputGestureText = "F2";
                  subItem42.Click += MenuItemHelpEvents_Click;
                  myMenuItemTopLevel4.Items.Add(subItem42);
                  MenuItem subItem43 = new MenuItem();
                  subItem43.Header = "_Tables...";
                  subItem43.InputGestureText = "F3";
                  subItem43.Click += MenuItemHelpTables_Click;
                  myMenuItemTopLevel4.Items.Add(subItem43);
                  MenuItem subItem44 = new MenuItem();
                  subItem44.Header = "_Icons...";
                  subItem44.InputGestureText = "F4";
                  subItem44.Click += MenuItemHelpIcons_Click;
                  myMenuItemTopLevel4.Items.Add(subItem44);
                  MenuItem subItem46 = new MenuItem();
                  subItem46.Header = "Report Error...";
                  subItem46.InputGestureText = "F6";
                  subItem46.Click += MenuItemHelpReportError_Click;
                  myMenuItemTopLevel4.Items.Add(subItem46);
                  MenuItem subItem47 = new MenuItem();
                  subItem47.Header = "_About...";
                  subItem47.InputGestureText = "Ctrl+A";
                  subItem47.Click += MenuItemHelpAbout_Click;
                  myMenuItemTopLevel4.Items.Add(subItem47);
               }
            } // end foreach (Control item in myMainMenu.Items) 
         } // end foreach (Control item in myMainMenu.Items)
         //foreach (Control item in myMainMenu.Items)
         //{
         //   if (item is MenuItem)
         //   {
         //      MenuItem menuItem = (MenuItem)item;
         //      if (menuItem.Name == "myMenuItemGamePhase")
         //      {
         //         myMenuItemGamePhase = menuItem;
         //         myMenuItemGamePhase.Header = "_Game Actions";
         //         myMenuItemGamePhase.InputGestureText = "Ctrl+G";

         //         foreach (Control item1 in menuItem.Items)
         //         {
         //            MenuItem menuItem1 = (MenuItem)item1;
         //            if (menuItem1.Name == "myMenuItemNextAction")
         //            {
         //               myMenuItemNextAction = menuItem1;
         //               myMenuItemNextAction.Click += MenuItemNextAction_Click;
         //               myMenuItemNextAction.Header = "_Start";
         //               myMenuItemNextAction.InputGestureText = "Ctrl+P";
         //            }
         //            else if (menuItem1.Name == "myMenuItemDisplay")
         //            {
         //               myMenuItemDisplay = menuItem1;
         //               myMenuItemDisplay.Click += MenuItemDisplay_Click;
         //               myMenuItemDisplay.Header = "_Display Possible Zebulon Locatons";
         //               myMenuItemDisplay.InputGestureText = "Ctrl+D";
         //            }
         //         } // end if (item is MenuItem)
         //      } // end if (item is MenuItem)
         //   } // end foreach (Control item in myMainMenu.Items)
         //}
         //
#if UT1
         myMenuItemTopLevel1.Width = 300;
         myMenuItemTopLevel2.Visibility = Visibility.Hidden;
         myMenuItemTopLevel3.Visibility = Visibility.Hidden;
         myMenuItemTopLevel4.Visibility = Visibility.Hidden;
         MenuItem subItem1 = new MenuItem();
         subItem1.Click += MenuItemCommand_Click;
         myMenuItemTopLevel1.Items.Add(subItem1);
         MenuItem subItem2 = new MenuItem();
         subItem2.Header = "_NextTest";
         subItem2.Click += MenuItemNextTest_Click;
         myMenuItemTopLevel1.Items.Add(subItem2);
         MenuItem subItem3 = new MenuItem();
         subItem3.Header = "_Cleanup";
         subItem3.Click += MenuItemCleanup_Click;
         myMenuItemTopLevel1.Items.Add(subItem3);
#else
         MenuItem subItem1 = new MenuItem();
         subItem1.Header = "_New";
         subItem1.InputGestureText = "Ctrl+N";
         subItem1.Click += MenuItemNew_Click;
         myMenuItemTopLevel1.Items.Add(subItem1);
         MenuItem subItem2 = new MenuItem();
         subItem2.Header = "_Open...";
         subItem2.InputGestureText = "Ctrl+O";
         subItem2.Click += MenuItemFileOpen_Click;
         myMenuItemTopLevel1.Items.Add(subItem2);
         MenuItem subItem3 = new MenuItem();
         subItem3.Header = "_Close";
         subItem3.InputGestureText = "Ctrl+C";
         subItem3.Click += MenuItemClose_Click;
         myMenuItemTopLevel1.Items.Add(subItem3);
         MenuItem subItem4 = new MenuItem();
         subItem4.Header = "_Save As...";
         subItem4.InputGestureText = "Ctrl+S";
         subItem4.Click += MenuItemSaveAs_Click;
         myMenuItemTopLevel1.Items.Add(subItem4);
         MenuItem subItem5 = new MenuItem();
         subItem5.Header = "_Options...";
         subItem5.InputGestureText = "Ctrl+Shift+O";
         subItem5.Click += MenuItemFileOptions_Click;
         myMenuItemTopLevel1.Items.Add(subItem5);
#endif
      }
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         //myGameInstance = gi;
         //if (null == myMenuItemGamePhase)
         //{
         //   Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, "MainMenuViewer::Update_View() => myMenuItemGamePhase is null");
         //   return;
         //}
         //myMenuItemGamePhase.Header = gi.GameTurn;
         StringBuilder sb = new StringBuilder("-----------------MainMenuViewer::Update_View() => action="); sb.Append(action.ToString()); sb.Append("  ==> NextAction="); sb.Append(gi.NextAction);
         Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, sb.ToString());
         switch (action)
         {
            //case GameAction.AlienStart:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myIsAlienStarted = true;
            //      myMenuItemNextAction.Header = "_Display Random Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+D";
            //      if (GamePhase.RandomMovement == gi.GamePhase)
            //         myMenuItemNextAction.IsEnabled = true;
            //      else
            //         myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else
            //   {
            //      if (true == myIsTownspersonStarted)
            //         myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.TownspersonStart:
            //   if (false == GameEngine.theIsAlien)
            //   {
            //      myIsTownspersonStarted = true;
            //      myMenuItemNextAction.Header = "_Display Random Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+D";
            //      if (GamePhase.RandomMovement == gi.GamePhase)
            //         myMenuItemNextAction.IsEnabled = true;
            //      else
            //         myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else
            //   {
            //      if (true == myIsAlienStarted)
            //         myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.AlienDisplaysRandomMovement:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Random Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+D";
            //   }
            //   break;
            //case GameAction.TownspersonDisplaysRandomMovement:
            //   if (false == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Random Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+D";
            //   }
            //   break;
            //case GameAction.AlienAcksRandomMovement:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myIsAlienAcked = true;
            //      myMenuItemNextAction.Header = "_Complete Alien Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      if (GamePhase.AlienMovement == gi.GamePhase)
            //         myMenuItemNextAction.IsEnabled = true;
            //      else
            //         myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else if (true == myIsTownspersonAcked)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+A";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   break;
            //case GameAction.TownspersonAcksRandomMovement:
            //   if (false == GameEngine.theIsAlien)
            //   {
            //      myIsTownspersonAcked = true;
            //      myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+A";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else if (true == myIsAlienAcked) // alien player
            //   {
            //      myMenuItemNextAction.Header = "_Complete Alien Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.ResetMovement:
            //   break;
            //case GameAction.AlienMovement:
            //   break;
            //case GameAction.AlienCompletesMovement:
            //   //Console.WriteLine("RESET");
            //   myIsTownspersonAcked = false;
            //   myIsAlienAcked = false;
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Townspeople Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+A";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Alien Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.TownspersonAcksAlienMovement:
            //   if (false == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Complete Townspeople Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.TownpersonProposesMovement:
            //   myMenuItemNextAction.IsEnabled = false;
            //   break;
            //case GameAction.AlienTimeoutOnMovement:
            //   myMenuItemNextAction.IsEnabled = true;
            //   break;
            //case GameAction.TownpersonMovement:
            //   myMenuItemNextAction.IsEnabled = true;
            //   break;
            //case GameAction.AlienModifiesTownspersonMovement:
            //   myMenuItemNextAction.IsEnabled = true;
            //   break;
            //case GameAction.TownpersonCompletesMovement:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Townspeople Movement";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+A";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   else
            //   {
            //      myMenuItemNextAction.Header = "_Complete Conversations";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   break;
            //case GameAction.AlienAcksTownspersonMovement:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.TownspersonMovement == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Conversations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Influences == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.Conversations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Conversations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Influences == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Influencing";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   break;
            //case GameAction.TownspersonPerformsConversation:
            //   break;
            //case GameAction.TownspersonCompletesConversations:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.Influences == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.Influences == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Influencing";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   break;
            //case GameAction.TownspersonPerformsInfluencing:
            //   break;
            //case GameAction.TownspersonCompletesInfluencing:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   break;
            //case GameAction.TownspersonInitiateCombat:
            //   break;
            //case GameAction.TownspersonPerformCombat:
            //   break;
            //case GameAction.TownspersonCompletesCombat:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   break;
            //case GameAction.AlienInitiateCombat:
            //   break;
            //case GameAction.AlienPerformCombat:
            //   break;
            //case GameAction.AlienCompletesCombat:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.Combat == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Combats";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.Iterrogations == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Iterogations";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //   }
            //   break;
            //case GameAction.TownspersonIterrogates:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Iterogations";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   else
            //   {
            //      myMenuItemNextAction.Header = "_Complete Iterogations";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   break;
            //case GameAction.TownspersonCompletesIterogations:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      myMenuItemNextAction.Header = "_Acknowledge Iterogations";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = true;
            //   }
            //   else
            //   {
            //      myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //      myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //      myMenuItemNextAction.IsEnabled = false;
            //   }
            //   break;
            //case GameAction.AlienAcksIterogations:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.ImplantRemoval == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Implant Removal Phase";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //   }
            //   break;
            //case GameAction.TownspersonRemovesImplant:
            //   break;
            //case GameAction.TownspersonCompletesRemoval:
            //   if (true == GameEngine.theIsAlien)
            //   {
            //      if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Complete Takeovers";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   else
            //   {
            //      if (GamePhase.AlienTakeover == gi.GamePhase)
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = false;
            //      }
            //      else
            //      {
            //         myMenuItemNextAction.Header = "_Display Random Movement";
            //         myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //         myMenuItemNextAction.IsEnabled = true;
            //      }
            //   }
            //   break;
            //case GameAction.AlienTakeover:
            //   break;
            //case GameAction.AlienCompletesTakeovers:
            //   myMenuItemNextAction.Header = "_Display Random Movement";
            //   myMenuItemNextAction.InputGestureText = "Ctrl+C";
            //   myMenuItemNextAction.IsEnabled = true;
            //   break;
            //case GameAction.ShowAlien:
            //   break;
            //case GameAction.ShowEndGame:
            //   myMenuItemNextAction.Header = "_Exit Game";
            //   myMenuItemNextAction.InputGestureText = "Ctrl+E";
            //   myMenuItemNextAction.IsEnabled = true;
            //   break;
            default:
               return;
         }
      }
      private void ApplyOptionsToCurrentGame(Options newOptions, Options currentOptions)
      {
         string name = "SkipTutorial0";
         Option? currentOption = currentOptions.Find(name);
         if (null == currentOption)
         {
            currentOption = new Option(name, false);
            currentOptions.Add(currentOption);
         }
         Option? newOption = newOptions.Find(name);
         if (null == newOption)
         {
            newOption = new Option(name, false);
            newOptions.Add(newOption);
         }
         currentOption.IsEnabled = newOption.IsEnabled;
         //-----------------------------------------
         currentOptions.Clear();
         foreach (Option option in newOptions)
         {
            Option newbie = new Option(option.Name, option.IsEnabled);
            currentOptions.Add(newbie);
         }
      }
      public bool UpdateCanvasShowZebulonLocations()
      {
         //SolidColorBrush aSolidColorBrush1 = new SolidColorBrush();
         //aSolidColorBrush1.Color = Color.FromArgb(0, 0, 1, 0);
         //if (false == myIsZebulonTerritoriesVisible)
         //{
         //   myIsZebulonTerritoriesVisible = true;

         //   SolidColorBrush aSolidColorBrush2 = new SolidColorBrush();
         //   aSolidColorBrush2.Color = Colors.Black;

         //   foreach (UIElement ui in myCanvas.Children)
         //   {
         //      if (ui is Polygon)
         //      {
         //         Polygon? p = (Polygon)ui;
         //         if( null == p )
         //         {
         //            Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, "MainMenuViewer::UpdateCanvas_ShowZebulonLocations() => polygon is null");
         //            continue;
         //         }
         //         ITerritory? t = myGameInstance.ZebulonTerritories.Find(p.Name);
         //         if (null == t)
         //            p.Fill = aSolidColorBrush1;
         //         else
         //            p.Fill = aSolidColorBrush2;
         //      }
         //   }
         //}
         //else
         //{
         //   myIsZebulonTerritoriesVisible = false;
         //   foreach (UIElement ui in myCanvas.Children)
         //   {
         //      if (ui is Polygon)
         //      {
         //         Polygon p = (Polygon)ui;
         //         p.Fill = aSolidColorBrush1;
         //      }
         //   }
         //}
         return true;
      }
      //----------------------------------------------------------
      public void MenuItemNew_Click(object sender, RoutedEventArgs e)
      {
         if (null == NewGameOptions)
            myGameInstance = new GameInstance();
         else
            myGameInstance = new GameInstance(this.NewGameOptions);
         if (true == myGameInstance.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemNew_Click(): myGameInstance.CtorError = true");
            return;
         }
         GameAction action = GameAction.UpdateNewGame;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemClose_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemClose_Click(): myGameInstance=null");
         }
         else
         {
            GameAction action = GameAction.EndGameClose;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGameFromFile();
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         if (false == loadMgr.SaveGameAsToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): GameLoadMgr.SaveGameAs() returned false");
      }
      public void MenuItemFileOptions_Click(object sender, RoutedEventArgs e)
      {
         ShowOptionsSelectionDialog dialog = new ShowOptionsSelectionDialog(myGameInstance.Options); // Set Options in Game
         if (true == dialog.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemFileOptions_Click(): OptionSelectionDialog CtorError=true");
            return;
         }
         if (true == dialog.ShowDialog())
         {
            this.NewGameOptions = dialog.NewOptions;
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "MenuItemFileOptions_Click(): new=" + this.NewGameOptions.ToString());
            ApplyOptionsToCurrentGame(this.NewGameOptions, myGameInstance.Options);
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "MenuItemFileOptions_Click(): current=" + myGameInstance.Options.ToString());
            GameAction action = GameAction.UpdateGameOptions;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemEditUndo_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateUndo;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemEditUndo_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         if (null == myGameInstance.UndoCmd)
            e.CanExecute = false;
         else
            e.CanExecute = true;
      }
      public void MenuItemEditRecoverCheckpoint_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGame("CheckpointLastDay.pbg");
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemEditRecoverCheckpoint_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         try
         {
            if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
            string filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastDay.pbg";
            if (true == File.Exists(filepath))
               e.CanExecute = true;
            else
               e.CanExecute = false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + GameLoadMgr.theGamesDirectory + " ex=" + ex.ToString());
            e.CanExecute = false;
         }
      }
      public void MenuItemEditRecoverRound_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGame("CheckpointLastRound.pbg");
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemEditRecoverRound_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         try
         {
            if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
            string filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastRound.pbg";
            if (true == File.Exists(filepath))
               e.CanExecute = true;
            else
               e.CanExecute = false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + GameLoadMgr.theGamesDirectory + " ex=" + ex.ToString());
            e.CanExecute = false;
         }
      }
      public void MenuItemViewFeats_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowGameFeatsDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewOtherGames_Click(object sender, RoutedEventArgs e)
      {
         ShowOtherGamesDialog dialog = new ShowOtherGamesDialog();
         dialog.Show();
      }
      public void MenuItemHelpRules_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowRuleListingDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpEvents_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowEventListingDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpTables_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowTableListing;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpIcons_Click(object sender, RoutedEventArgs e)
      {
         ShowIconDisplayDialog dialog = new ShowIconDisplayDialog();
         dialog.Show();
      }
      public void MenuItemHelpReportError_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowReportErrorDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpAbout_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowAboutDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      //----------------------------------------------------------
      public void MenuItemCommand_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestCommand;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemNextTest_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestNext;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemCleanup_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestCleanup;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      //----------------------------------------------------------
      private void MenuItemNextAction_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MainMenuViewer::MenuItemNextAction_Click(): myGameInstance is null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "MainMenuViewer::MenuItemNextAction_Click(): myGameEngine is null");
            return;
         }
         GameAction action = GameAction.TownspersonDisplaysRandomMovement; // Game State follows the state pattern.  Each game state represents a different object.  The initial game state is Setup.
         //switch (myMenuItemNextAction.Header.ToString())
         //{
         //   case "_Start":
         //      if (true == GameEngine.theIsAlien)
         //         action = GameAction.AlienStart;
         //      else
         //         action = GameAction.TownspersonStart;
         //      break;
         //   case "_Display Random Movement":
         //      if (true == GameEngine.theIsAlien)
         //         action = GameAction.AlienDisplaysRandomMovement;
         //      else
         //         action = GameAction.TownspersonDisplaysRandomMovement;
         //      break;
         //   case "_Acknowledge Random Movement":
         //      if (true == GameEngine.theIsAlien)
         //         action = GameAction.AlienAcksRandomMovement;
         //      else
         //         action = GameAction.TownspersonAcksRandomMovement;
         //      break;
         //   case "_Complete Alien Movement":
         //      action = GameAction.AlienCompletesMovement;
         //      break;
         //   case "_Acknowledge Alien Movement":
         //      action = GameAction.TownspersonAcksAlienMovement;
         //      break;
         //   case "_Complete Townspeople Movement":
         //      action = GameAction.TownpersonCompletesMovement;
         //      break;
         //   case "_Acknowledge Townspeople Movement":
         //      action = GameAction.AlienAcksTownspersonMovement;
         //      break;
         //   case "_Complete Conversations":
         //      action = GameAction.TownspersonCompletesConversations;
         //      break;
         //   case "_Complete Influencing":
         //      action = GameAction.TownspersonCompletesInfluencing;
         //      break;
         //   case "_Complete Combats":
         //      if (true == GameEngine.theIsAlien)
         //         action = GameAction.AlienCompletesCombat;
         //      else
         //         action = GameAction.TownspersonCompletesCombat;
         //      break;
         //   case "_Complete Iterogations":
         //      action = GameAction.TownspersonCompletesIterogations;
         //      break;
         //   case "_Acknowledge Iterogations":
         //      action = GameAction.AlienAcksIterogations;
         //      break;
         //   case "_Complete Takeovers":
         //      action = GameAction.AlienCompletesTakeovers;
         //      break;
         //   case "_Exit Game":
         //      action = GameAction.ExitGame;
         //      break;
         //   default:
         //      Console.WriteLine("{0}", myMenuItemNextAction.Header.ToString());
         //      break;
         //}
         GameAction outAction = action;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      public void MenuItemDisplay_Click(object sender, RoutedEventArgs e)
      {
         if (false == UpdateCanvasShowZebulonLocations())
            Logger.Log(LogEnum.LE_ERROR, "MainMenuViewer::MenuItemDisplay_Click(): UpdateCanvas_ShowZebulonLocations() returned false");
      }
   }
}
