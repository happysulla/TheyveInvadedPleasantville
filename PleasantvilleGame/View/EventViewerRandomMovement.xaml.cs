
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
using CheckBox = System.Windows.Controls.CheckBox;
using System.Diagnostics.SymbolStore;

namespace PleasantvilleGame
{
   public partial class EventViewerRandomMovement : System.Windows.Controls.UserControl
   {
      public delegate bool EndPerformRandomMovement();
      private const int STARTING_ASSIGNED_ROW = 6;
      public enum E063Enum
      {
         PREPARE,
         END
      };
      public bool CtorError { get; } = false;
      private EndPerformRandomMovement? myCallback = null;
      private E063Enum myState = E063Enum.PREPARE;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll = Utilities.NO_RESULT;
         public string myBuildingName;
         public bool myIsBlockedFromMove = false;
         public GridRow(IMapItem mi, string bName)
         {
            myMapItem = mi;
            myBuildingName = bName;
         }
      };
      private GridRow[] myGridRows = new GridRow[4];
      private int myMaxRowCount = 0;
      private bool myIsPossibleBlock = false;
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      private string myDieRollResult="";
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private readonly Thickness myMarginRight = new Thickness(5, 0, 0, 0);
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
         if( 4 != myGameInstance.RandomMoves.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): invalid state myGameInstance.RandomMoves.Count=" + myGameInstance.RandomMoves.Count.ToString());
            return false;
         }
         //--------------------------------------------------
         myIsPossibleBlock = false;
         myGridRows = new GridRow[4];
         int numPeopleMoved = 0;
         foreach (RandomMoveData rmd in myGameInstance.RandomMoves)
         {
            IMapItem? mi = myGameInstance.Townspeople.Find(rmd.myMapItemName);
            if( null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): unable to find map item for name=" + rmd.myMapItemName);
               return false;
            }
            string buildingName = GetBuildingName(rmd.myBuildingName);
            if( "ERROR" == buildingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "Perform_RandomMovement(): GetBuildingName() returned ERROR for kvp.Value=" + rmd.myBuildingName);
               return false;
            }
            myGridRows[numPeopleMoved] = new GridRow(mi, buildingName);
            numPeopleMoved++;
            if ((false == GameEngine.theIsAlien) && (true == mi.IsControlled))
               myIsPossibleBlock = true;
            else if ((true == GameEngine.theIsAlien) && ((true == mi.IsAlienKnown) || (true == mi.IsAlienUnknown)))
               myIsPossibleBlock = true;
         }
         myMaxRowCount = numPeopleMoved;
         myCallback = callback;
         myState = E063Enum.PREPARE;
         myIsRollInProgress = false;
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
         if (E063Enum.END == myState)
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
         if (E063Enum.END == myState)
         {
            if( null == myGameInstance )
            {
               Logger.Log(LogEnum.LE_ERROR, "Update_EndState(): myGameInstance=null");
               return false;
            }
            foreach (GridRow gr in myGridRows)
            {
               if (null == gr.myMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Update_EndState(): null map item in grid row");
                  return false;
               }
               if (true == gr.myIsBlockedFromMove )
               {

               }
            }
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "Update_EndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "Update_EndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         if( true == myIsPossibleBlock )
            myTextBlockInstructions.Inlines.Add(new Run("Check the box to block a controlled person from moving. Click the image to continue."));
         else
            myTextBlockInstructions.Inlines.Add(new Run("No blocks possible. Click the image to continue."));
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Add(img23);
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //-----------------------------
            CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            if ( (true == mi.IsTiedUp) || (true == mi.IsUnconscious) || (true == mi.IsKilled))
               cb.IsChecked = true;
            else
               cb.IsChecked = false;
            myGrid.Children.Add(cb);
            Grid.SetRow(cb, rowNum);
            Grid.SetColumn(cb, 1);
            //-----------------------------
            string dest = myGridRows[i].myBuildingName;
            if ((true == mi.IsTiedUp) || (true == mi.IsUnconscious) || (true == mi.IsKilled))
               dest = "NA";
            Label labelforBuildingName = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dest };
            myGrid.Children.Add(labelforBuildingName);
            Grid.SetRow(labelforBuildingName, rowNum);
            Grid.SetColumn(labelforBuildingName, 2);
            //-----------------------------
            CheckBox cb1 = new CheckBox() { FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            if ((true == mi.IsTiedUp) || (true == mi.IsUnconscious) || (true == mi.IsKilled))
            {
               Label labelForBlock = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelForBlock);
               Grid.SetRow(labelForBlock, rowNum);
               Grid.SetColumn(labelForBlock, 3);
            }
            else if ((false == GameEngine.theIsAlien) && (true == mi.IsControlled))
            {
               cb1.IsEnabled = true;
               cb1.IsChecked = myGridRows[i].myIsBlockedFromMove;
               cb1.Checked += CheckBox_Checked;
               cb1.Unchecked += CheckBox_Unchecked;
            }
            else if ((true == GameEngine.theIsAlien) && ((true == mi.IsAlienKnown) || (true == mi.IsAlienUnknown)))
            {
               cb1.IsEnabled = true;
               cb1.IsChecked = myGridRows[i].myIsBlockedFromMove;
               cb1.Checked += CheckBox_Checked;
               cb1.Unchecked += CheckBox_Unchecked;
            }
            else
            {
               cb1.IsEnabled = false;
               cb1.IsChecked = false;
            }
            myGrid.Children.Add(cb1);
            Grid.SetRow(cb1, rowNum);
            Grid.SetColumn(cb1, 3);
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private string GetBuildingName(string tName)
      {
         if (true == tName.Contains("House"))
         {
            string modifiedTName = tName.Replace("_", " ");
            return modifiedTName;
         }
         else
         {
            int arraySize = TableMgr.theBuildingSizes.GetLength(0);
            for (int i = 0; i < arraySize; i++)
            {
               string matchingName = Utilities.RemoveSpaces(TableMgr.theBuildingSizes[i, 0]);
               if (true == tName.Contains(matchingName))
                  return TableMgr.theBuildingSizes[i, 0];
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetBuildingName(): unable to find building name for territory=" + tName);
         return "ERROR";
      }
      private bool SetTerritory(IMapItem mi, ITerritory newT)
      {
         mi.TerritoryCurrent = newT;
         double offset = mi.Zoom * Utilities.theMapItemOffset;
         mi.Location.X = newT.CenterPoint.X - offset;
         mi.Location.Y = newT.CenterPoint.Y - offset;
         return true;
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = 1.1 * Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = 1.1 * Utilities.ZOOM * Utilities.theMapItemSize;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         b.BorderThickness = new Thickness(1);
         b.Margin = new Thickness(2);
         MapItem.SetButtonContent(b, mi); // This sets the image as the button's content
         return b;
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
                        if ("Continue" == img.Name)
                        {
                           myState = E063Enum.END;
                        }
                        else if ("DieRoll" == img.Name)
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
            else if (ui is Image img1) // next check all images within the Grid Rows
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
      private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         int row = Grid.GetRow(cb);
         if (row < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): invalid row=" + row.ToString());
            return;
         }
         int i = row - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsBlockedFromMove = false;
      }
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         int row = Grid.GetRow(cb);
         if (row < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): invalid row=" + row.ToString());
            return;
         }
         int i = row - STARTING_ASSIGNED_ROW;
         myGridRows[i].myIsBlockedFromMove = true;
      }
   }
}
