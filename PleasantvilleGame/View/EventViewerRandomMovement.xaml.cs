
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfAnimatedGif;
using FontFamily = System.Windows.Media.FontFamily;  
using Label = System.Windows.Controls.Label;
using Rectangle = System.Windows.Shapes.Rectangle;
using Image = System.Windows.Controls.Image;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace PleasantvilleGame
{
   public partial class EventViewerRandomMovement : System.Windows.Controls.UserControl
   {
      public delegate bool EndPerformRandomMovement();
      private const int STARTING_ASSIGNED_ROW = 6;
      public enum E162Enum
      {
         PREPARE,
         END
      };
      public bool CtorError { get; } = false;
      private EndPerformRandomMovement? myCallback = null;
      private E162Enum myState = E162Enum.PREPARE;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public string myReasonForNotMoving;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      private GridRow[] myGridRows = new GridRow[5];
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      private string myDieRollResult="";
      //---------------------------------------------------
      private readonly Thickness myMarginAssignPanel = new Thickness(20, 0, 0, 0);
      private readonly Thickness myMarginLeft = new Thickness(0, 0, 5, 0);
      private readonly Thickness myMarginRight = new Thickness(5, 0, 0, 0);
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerRandomMovement(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool PerformRandomMovement(EndPerformRandomMovement callback)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E162Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E162Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         return true;
      }
      private bool UpdateGridRows()
      {
         return true;
      }
      //------------------------------------------------------------------------------------
      private bool SetTerritory(IMapItem mi, ITerritory newT)
      {
         mi.TerritoryCurrent = newT;
         double offset = mi.Zoom * Utilities.theMapItemOffset;
         mi.Location.X = newT.CenterPoint.X - offset;
         mi.Location.Y = newT.CenterPoint.Y - offset;
         return true;
      }
      public bool CreateMovements(IGameInstance gi, int numPeopleToMove)
      {
         int numPeopleSkipped = 0;
         int numPeopleMoved = 0;
         int loopCount = 200;
         while ((numPeopleMoved < numPeopleToMove) && (0 < loopCount--))
         {
            int die1 = Utilities.RandomGenerator.Next(5);
            int die2 = Utilities.RandomGenerator.Next(6);
            string name = TableMgr.GetTownspersonName(die1, die2);
            if (true == String.IsNullOrEmpty(name))
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Movements(): TableMgr.GetTownspersonName() returned null");
               return false;
            }
            IMapItem? miMoving = gi.Townspeople.Find(name);
            if (null == miMoving)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Movements(): unable to find name=" + name);
               return false;
            }
            //------------------------------------------------------------
            // If the counter is moved or tied up or known to be alien controlled, do not move.
            if ((true == miMoving.IsMoved) || (true == miMoving.IsStunned) || (true == miMoving.IsTiedUp) || (false == miMoving.IsUnconscious) || (false == miMoving.IsKilled))
            {
               ++numPeopleSkipped;
               StringBuilder sb = new StringBuilder("Create_Movements(): skipped=");
               sb.Append(numPeopleSkipped.ToString());
               sb.Append(" mi=");
               sb.Append(miMoving.Name);
               sb.Append(" m?=");
               sb.Append(miMoving.IsMoved.ToString());
               sb.Append(" c?=");
               sb.Append(miMoving.IsControlled.ToString());
               sb.Append(" k?=");
               sb.Append(miMoving.IsAlienKnown.ToString());
               sb.Append(" stun?=");
               sb.Append(miMoving.IsStunned.ToString());
               sb.Append(" surr?=");
               sb.Append(miMoving.IsSurrendered.ToString());
               sb.Append(" tu?=");
               sb.Append(miMoving.IsTiedUp.ToString());
               sb.Append(" w?=");
               sb.Append(miMoving.IsWary.ToString());
               sb.Append(" con?=");
               sb.Append(miMoving.IsUnconscious.ToString());
               Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, sb.ToString());
               continue;
            }
            //------------------------------------------------------------
            die1 = Utilities.RandomGenerator.Next(5);
            die2 = Utilities.RandomGenerator.Next(6);
            string buildingName = TableMgr.GetTargetBuildingName(die1, die2); // Find the target building location.
            ITerritory? newTerritory = Territories.theTerritories.Find(buildingName);
            if (null == newTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_Movement(): newTerritory is null for buildingName=" + buildingName);
               return false;
            }
            //------------------------------------------------------------
            Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Perform_Movement(): mi=" + miMoving.Name + " entering t=" + newTerritory.Name);
            if (false == CreateMapItemMove(gi, miMoving, newTerritory))
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_Movement(): Create_MapItemMove() returned false");
               return false;
            }
            miMoving.IsMoved = true;
            //------------------------------------------------------------
            ++numPeopleMoved;  // Keep track of number of people moved
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Create_Movements(): moved miMoving=" + miMoving.Name + " numPeopleMoved=" + numPeopleMoved.ToString());
         }  // end while()
         if (0 < loopCount)
         {
            Logger.Log(LogEnum.LE_SHOW_RANDOM_MOVE, "Create_Movements(): invalid state loopCount=" + loopCount.ToString());
            return false;
         }
         return true;
      }
      protected bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_MapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_ADD, "Create_MapItemMove(): mi=" + mi.Name + " moving to t=" + newT.Name);
         gi.MapItemMoves.Insert(0, mim); // add at front
         return true;
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         if ( null == myGameInstance )
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement.ShowDieResults(): myGameInstance=null");
            return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement.ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //---------------------Controller Function--------------------------------------------
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == myRulesMgr.ShowRule(key))
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
         }
         else
         {
            if (false == myRulesMgr.ShowTable(key))
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowTable() returned false for key=" + key);
         }
      }
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameInstance=null");
            return;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCanvas=null");
            return;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myScrollViewer=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myRulesMgr=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myDieRoller=null");
            return;
         }
         //--------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        if ("DieRoll" == img.Name)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img.Visibility = Visibility.Hidden;
                           }
                           return;
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
   }
}
