
using Google.Protobuf.WellKnownTypes;
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
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using FontFamily = System.Windows.Media.FontFamily;  
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace PleasantvilleGame
{
   public partial class EventViewerCombatResolve : System.Windows.Controls.UserControl
   {
      public delegate bool EndCombatResolve();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int MAX_ROW_COUNT = 10;
      public enum E11Enum
      {
         ROLL_FOR_COMBAT,
         ROLL_FOR_COMBAT_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndCombatResolve? myCallback = null;
      private E11Enum myState = E11Enum.ROLL_FOR_COMBAT;
      private bool myIsRollInProgress = false;
      private int myMaxRowNum = 0;
      private int myRollResultRowNum = 0;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll = Utilities.NO_RESULT;
         public string myResult = "ERROR";
         public GridRow(IMapItem mapItem)
         {
            myMapItem = mapItem;
         }
      };
      private GridRow[] myGridRows = new GridRow[MAX_ROW_COUNT];
      private int myMaxRowCount = 0;
      private bool myIsTownAttacker = false;
      private bool myIsAlienAttacker = false;
      private bool myIsTownDefender = false;
      private bool myIsAlienDefender = false;
      private bool myIsUncontrolledDefender = false;
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
      public EventViewerCombatResolve(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer_CombatResolve(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool ResolveCombat(EndCombatResolve callback)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myDieRoller=null");
            return false;
         }
         if (null == myGameInstance.MapItemCombat)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myGameInstance.MapItemCombat=null");
            return false;
         }
         myCallback = callback;
         myState = E11Enum.ROLL_FOR_COMBAT;
         myIsRollInProgress = false;
         //--------------------------------------------------
         if (0 == myGameInstance.MapItemCombat.Attackers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myGameInstance.MapItemCombat.Attackers.Count=0");
            return false;
         }
         IMapItem? firstAttacker = myGameInstance.MapItemCombat.Attackers[0];
         if (null == firstAttacker)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): firstAttacker=null");
            return false;
         }
         myIsTownAttacker = false;
         myIsAlienAttacker = false;
         if (true == firstAttacker.IsControlled)
            myIsTownAttacker = true;
         else if (true == firstAttacker.IsAlienKnown)
            myIsAlienAttacker = true; 
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): Reached Default firstAttacker=" + firstAttacker.ToString());
            return false;
         }
         //--------------------------------------------------
         if (0 == myGameInstance.MapItemCombat.Defenders.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): myGameInstance.MapItemCombat.Defenders.Count=0");
            return false;
         }
         IMapItem? firstDefender = myGameInstance.MapItemCombat.Defenders[0];
         if (null == firstDefender)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): firstDefender=null");
            return false;
         }
         myIsTownDefender = false;
         myIsAlienDefender = false;
         myIsUncontrolledDefender = false;
         if (true == firstDefender.IsControlled)
         {
            myIsTownDefender = true;
            myButtonLossTable.Content = "Town Loss";
         }
         else if (true == firstDefender.IsAlienKnown)
         {
            myIsAlienDefender = true;
            myButtonLossTable.Content = "Alien Loss";
         }
         else if (true == firstDefender.IsUncontrolled())
         {
            myIsUncontrolledDefender = true;
            myButtonLossTable.Content = "Town Loss";
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): Reached Default mi=" + firstDefender.ToString());
            return false;
         }
         //--------------------------------------------------
         IMapItems mapItems;
         if( CombatResult.AttackerWins == myGameInstance.MapItemCombat.Result)
         {
            mapItems = myGameInstance.MapItemCombat.Defenders;

         }
         else if (CombatResult.DefenderWins == myGameInstance.MapItemCombat.Result)
         {
            mapItems = myGameInstance.MapItemCombat.Attackers;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): invalid state myGameInstance.MapItemCombat.Result=" + myGameInstance.MapItemCombat);
            return false;
         }
         //--------------------------------------------------
         int gridRowNum = 0;
         foreach (IMapItem mi in mapItems)
         {
            myGridRows[gridRowNum] = new GridRow(mi);
            gridRowNum++;
         }
         myMaxRowCount = gridRowNum;
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_Combat(): UpdateGrid() return false");
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
         if (E11Enum.END == myState)
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
         if (E11Enum.END == myState)
         {
            if( null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "Update_EndState(): myGameInstance=null");
               return false;
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
         switch (myState)
         {
            case E11Enum.ROLL_FOR_COMBAT:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die to roll for result"));
               break;
            case E11Enum.ROLL_FOR_COMBAT_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click the image to continue."));
               break;
            default:
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch(myState)
         {
            case E11Enum.ROLL_FOR_COMBAT:
               Rectangle r = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r);
               break;
            case E11Enum.ROLL_FOR_COMBAT_SHOW:
               System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img23);
               break;
            default:
               return false;
         }
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
            //------------------------------------------------------------
            if (Utilities.NO_RESULT < myGridRows[i].myDieRoll)
            {
               Label labelForRoll = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
               myGrid.Children.Add(labelForRoll);
               Grid.SetRow(labelForRoll, rowNum);
               Grid.SetColumn(labelForRoll, 1);
               Label labelForResult = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult };
               myGrid.Children.Add(labelForResult);
               Grid.SetRow(labelForResult, rowNum);
               Grid.SetColumn(labelForResult, 2);
               if( (true == mi.IsKilled) || (true == myIsTownDefender) || (true == myIsUncontrolledDefender))
               {
                  Label labelForTiedUp = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA"};
                  myGrid.Children.Add(labelForTiedUp);
                  Grid.SetRow(labelForTiedUp, rowNum);
                  Grid.SetColumn(labelForTiedUp, 3);
               }
               else
               {
                  CheckBox cb = new CheckBox() { IsEnabled = true, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                  myGrid.Children.Add(cb);
                  Grid.SetRow(cb, rowNum);
                  Grid.SetColumn(cb, 3);
                  if( true == mi.IsTiedUp )
                  {
                     cb.Unchecked += CheckBox_Unchecked;
                     cb.IsChecked = true;
                  }
                  else
                  {
                     cb.Checked += CheckBox_Checked;
                     cb.IsChecked = false;
                  }
               }
            }
            else
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "dieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 1);
            }
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = 2.0 * mi.Zoom * Utilities.theMapItemSize;
         b.Height = 2.0 * mi.Zoom * Utilities.theMapItemSize;
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
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatResolve.ShowDieResults(): myGameInstance=null");
            return;
         }
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         myGridRows[i].myDieRoll = dieRoll;
         //-------------------------------
         if(true == myIsAlienDefender)
         {
            if( dieRoll < 5 )
            {
               myGridRows[i].myResult = "KIA";
               myGridRows[i].myMapItem.IsKilled = true;
            }
            else if (dieRoll < 7 )
            {
               myGridRows[i].myResult = "K.O.";
               myGridRows[i].myMapItem.IsUnconscious = true;
               myGridRows[i].myMapItem.IsTiedUp = true;
               Logger.Log(LogEnum.LE_GAMESTATE_TIED_UP, "ShowDieResults(): mi=" + myGridRows[i].myMapItem.ToString() + " ++TIED and KO");
            }
            else
            {
               myGridRows[i].myResult = "Hands-Up";
               myGridRows[i].myMapItem.IsSurrendered = true;
               myGridRows[i].myMapItem.IsTiedUp = true;
               Logger.Log(LogEnum.LE_GAMESTATE_TIED_UP, "ShowDieResults(): mi=" + myGridRows[i].myMapItem.ToString() + " ++TIED and gives up");
            }
         }
         else if ( (true == myIsTownDefender) || (true == myIsUncontrolledDefender) )
         {
            if (dieRoll < 5)
            {
               myGridRows[i].myResult = "KIA";
               myGridRows[i].myMapItem.IsKilled = true;
            }
            else if (dieRoll < 7)
            {
               myGridRows[i].myResult = "K.O.";
               myGridRows[i].myMapItem.IsUnconscious = true;
            }
            else
            {
               myGridRows[i].myResult = "Stunned";
               myGridRows[i].myMapItem.IsStunned = true;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatResolve.ShowDieResults(): Reached Default for mi=" + myGridRows[i].myMapItem.ToString());
            return;
         }
         Logger.Log(LogEnum.LE_SHOW_COMBATS, "EventViewerCombatResolve.ShowDieResults(): dr=" + dieRoll.ToString() + " result=" + myGridRows[i].myResult + " for mi=" + myGridRows[i].myMapItem.ToString());
         //-------------------------------
         myState = E11Enum.ROLL_FOR_COMBAT_SHOW;
         foreach(GridRow gr in myGridRows)
         {
            if( gr.myDieRoll < 0 )
               myState = E11Enum.ROLL_FOR_COMBAT;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatResolve.ShowDieResults(): UpdateGrid() return false");
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
                           myState = E11Enum.END;
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
                     myRollResultRowNum = Grid.GetRow(img1);
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDice(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         int rowNum = Grid.GetRow(cb);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         if( i < 0 )
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): i=" + i.ToString());
            return;
         }
         myGridRows[i].myMapItem.IsTiedUp = true;
         Logger.Log(LogEnum.LE_GAMESTATE_TIED_UP, "CheckBox_Checked(): mi=" + myGridRows[i].myMapItem.ToString() + " ++TIED");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): UpdateGrid() return false");
      }
      private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         int rowNum = Grid.GetRow(cb);
         int i = rowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): i=" + i.ToString());
            return;
         }
         myGridRows[i].myMapItem.IsTiedUp = false;
         Logger.Log(LogEnum.LE_GAMESTATE_TIED_UP, "CheckBox_Unchecked(): mi=" + myGridRows[i].myMapItem.ToString() + " --TIED");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): UpdateGrid() return false");
      }
   }
}
