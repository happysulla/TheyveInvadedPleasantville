
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
   public partial class EventViewerAlienTakeovers : System.Windows.Controls.UserControl
   {
      public delegate bool EndAlienTakeovers();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int MAX_ROW_COUNT = 25;
      private const int NO_OBSERVER = 1000;
      public enum E091Enum
      {
         ROLL_FOR_OBSERVE,
         ROLL_FOR_OBSERVE_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndAlienTakeovers? myCallback = null;
      private E091Enum myState = E091Enum.ROLL_FOR_OBSERVE;
      private bool myIsRollInProgress = false;
      private int myMaxRowNum = 0;
      private int myRollResultRowNum = 0;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem1;
         public IMapItem myMapItem2;
         public IMapItem? myObserver;
         public double myProbability;
         public int myDieRoll = Utilities.NO_RESULT;
         public bool myIsResult = false;
         public GridRow(IMapItem? observer, IMapItem mi1, IMapItem mi2, double probability )
         {
            myObserver = observer;
            myMapItem1 = mi1;
            myMapItem2 = mi2;
            myProbability = probability;
         }
      };
      private GridRow[] myGridRows = new GridRow[MAX_ROW_COUNT];
      private int myMaxRowCount = 0;
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
      public EventViewerAlienTakeovers(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
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
      public bool ConsumateAlienTakeovers(EndAlienTakeovers callback)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myCallback = callback;
         myState = E091Enum.ROLL_FOR_OBSERVE;
         myIsRollInProgress = false;
         int gridRowNum = 0;
         foreach(KeyValuePair<IMapItem,IMapItem> kvp in myGameInstance.AlienTakeovers) 
         {
            IMapItem leftMapItem = kvp.Key;
            IMapItem rightMapItem = kvp.Value;
            ITerritory t = leftMapItem.TerritoryCurrent;
            IStack? stack = myGameInstance.Stacks.Find(t);
            if( null == stack )
            {
               Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): stack=null for t=" + t.ToString());
               return false;
            }
            //-----------------------------------------
            int randNum = Utilities.RandomGenerator.Next(2); // randomize which unit is displayed to user on left hand side
            if( 0 == randNum )
            {
               IMapItem temp = leftMapItem;
               leftMapItem = rightMapItem;
               rightMapItem = temp;
            }
            //-----------------------------------------
            bool isObservation = false;
            foreach(KeyValuePair<String, double> kvp1 in t.Observations) // look thru all territories that can observe this takeover
            {
               ITerritory? t1 = Territories.theTerritories.Find(kvp1.Key); // kvp1.Key=Territory_Name, kvp1.Value=Observe_Probability
               if( null == t1 )
               {
                  Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): t1=null for " + kvp1.Key);
                  return false;
               }
               IStack? stackObs = myGameInstance.Stacks.Find(t1); // could be nobody in this observation territory
               if (null == stackObs)
                  continue;
               foreach(IMapItem mi in stackObs.MapItems )
               {
                  if ((true == mi.Name.Contains(leftMapItem.Name)) || (true == mi.Name.Contains(rightMapItem.Name)))
                     continue;
                  if (true == mi.IsAlienKnown) // known aliens do not observer. Unknown aliens need to be listed so that town person does not suspect them as alien, but they will not find anything
                     continue;
                  if ((true == mi.IsKilled) || (true == mi.IsUnconscious)) // stuned, killed people cannot observe
                     continue;
                  myGridRows[gridRowNum] = new GridRow(mi, leftMapItem, rightMapItem, kvp1.Value);
                  gridRowNum++;
                  isObservation = true;
               }
            }
            if (false == isObservation) // If there is no observations, indicate to user that zero probability of detection
            {
               myGridRows[gridRowNum] = new GridRow(null, leftMapItem, rightMapItem, 0.0);
               myGridRows[gridRowNum].myDieRoll = NO_OBSERVER;
               gridRowNum++;
            }
            Logger.Log(LogEnum.LE_SHOW_TAKEOVERS, "Consumate_AlienTakeovers(): Adding Key=" + leftMapItem.Name + " Value=" + rightMapItem.Name + " w/ obs?=" + isObservation.ToString());
         }
         myMaxRowCount = gridRowNum;
         //--------------------------------------------------
         myState = E091Enum.ROLL_FOR_OBSERVE_SHOW;
         foreach (GridRow gr in myGridRows)
         {
            if (gr.myDieRoll < 0)
               myState = E091Enum.ROLL_FOR_OBSERVE;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Consumate_AlienTakeovers(): UpdateGrid() return false");
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
         if (E091Enum.END == myState)
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
         if (E091Enum.END == myState)
         {
            if (null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "EventViewerAlienTakeovers.UpdateEndState(): myGameInstance=null");
               return false;
            }
            foreach (GridRow gr1 in myGridRows)
            {
               if (NO_OBSERVER == gr1.myDieRoll)
               {
                  if (false == PerformObservation(gr1))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EventViewerAlienTakeovers.UpdateEndState()(): Perform_Observation(() returned false");
                     return false;
                  }
               }
            }
            //-----------------------------------------------
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
            case E091Enum.ROLL_FOR_OBSERVE:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die to roll for observation"));
               break;
            case E091Enum.ROLL_FOR_OBSERVE_SHOW:
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
            case E091Enum.ROLL_FOR_OBSERVE:
               Rectangle r = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r);
               break;
            case E091Enum.ROLL_FOR_OBSERVE_SHOW:
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
            IMapItem? observer = myGridRows[i].myObserver;
            if( null == observer )
            {
               Label labelForObserver= new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelForObserver);
               Grid.SetRow(labelForObserver, rowNum);
               Grid.SetColumn(labelForObserver, 0);
               Label labelForRoll1 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelForRoll1);
               Grid.SetRow(labelForRoll1, rowNum);
               Grid.SetColumn(labelForRoll1, 4);
            }
            else
            {
               Button b0 = CreateButton(observer);
               myGrid.Children.Add(b0);
               Grid.SetRow(b0, rowNum);
               Grid.SetColumn(b0, 0);
               if (Utilities.NO_RESULT < myGridRows[i].myDieRoll)
               {
                  string result = "NA";
                  if (true == myGridRows[i].myIsResult)
                     result = myGridRows[i].myDieRoll.ToString();
                  Label labelForRoll = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = result };
                  myGrid.Children.Add(labelForRoll);
                  Grid.SetRow(labelForRoll, rowNum);
                  Grid.SetColumn(labelForRoll, 4);
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
                  Grid.SetColumn(img, 4);
               }
            }
            //-----------------------------
            Button b1 = CreateButton(myGridRows[i].myMapItem1);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 1);
            //-----------------------------
            Button b2 = CreateButton(myGridRows[i].myMapItem2);
            myGrid.Children.Add(b2);
            Grid.SetRow(b2, rowNum);
            Grid.SetColumn(b2, 2);
            //-----------------------------
            int prob = (int)(myGridRows[i].myProbability * 100.0);
            string sProb = prob.ToString() + "%";
            Label labelForProb = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sProb };
            myGrid.Children.Add(labelForProb);
            Grid.SetRow(labelForProb, rowNum);
            Grid.SetColumn(labelForProb, 3);
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
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement.ShowDieResults(): myGameInstance=null");
            return;
         }
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         IMapItem? observer = myGridRows[i].myObserver;
         if (null == observer)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myObserver=null");
            return;
         }
         if ((true == observer.IsAlienKnown) || (true == observer.IsAlienUnknown))
         {
            myGridRows[i].myDieRoll = NO_OBSERVER;
         }
         else
         {
            myGridRows[i].myDieRoll = dieRoll;
            switch (dieRoll)
            {
               case 1:
                  if (0.15 < myGridRows[i].myProbability)
                     myGridRows[i].myIsResult = true;
                  break;
               case 2:
                  if (0.32 < myGridRows[i].myProbability)
                     myGridRows[i].myIsResult = true;
                  break;
               case 3:
                  if (0.49 < myGridRows[i].myProbability)
                     myGridRows[i].myIsResult = true;
                  break;
               case 4:
                  if (0.65 < myGridRows[i].myProbability)
                     myGridRows[i].myIsResult = true;
                  break;
               case 5:
               case 6:
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): invalid die roll=" + myGridRows[i].myDieRoll.ToString());
                  return;
            }
            if (true == myGridRows[i].myIsResult)
            {
               if (false == PerformObservation(myGridRows[i]))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): PerformObservation(() returned false");
                  return;
               }
            }
         }
         //-------------------------------
         myState = E091Enum.ROLL_FOR_OBSERVE_SHOW;
         foreach (GridRow gr1 in myGridRows)
         {
            if (gr1.myDieRoll < 0)
               myState = E091Enum.ROLL_FOR_OBSERVE;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "EventViewerRandomMovement.ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public bool PerformObservation(GridRow gr)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Perform_Observation(): myGameInstance=null");
            return false;
         }
         if ((true == gr.myMapItem1.IsAlienUnknown) && (true == gr.myMapItem2.IsUncontrolled())) // Alien can be in either of these positions.
         {
            if (true == gr.myIsResult) // true when observed doing takeover
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 1-OBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem1.Name + " taking over " + gr.myMapItem2.Name);
               myGameInstance.AddKnownAlien(gr.myMapItem1); // PerformObservation()
               myGameInstance.AddKnownAlien(gr.myMapItem2); // PerformObservation()
               if (null != gr.myObserver)
                  gr.myObserver.IsWary = true;
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 1-UNOBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem1.Name + " taking over " + gr.myMapItem2.Name);
               myGameInstance.AddUnknownAlien(gr.myMapItem2);
            }
         }
         else if ((true == gr.myMapItem2.IsAlienUnknown) && (true == gr.myMapItem1.IsUncontrolled())) // Alien can be in either of these positions.
         {
            if (true == gr.myIsResult) // true when observed doing takeover
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 2-OBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem2.Name + " taking over " + gr.myMapItem1.Name);
               myGameInstance.AddKnownAlien(gr.myMapItem1);  // PerformObservation()
               myGameInstance.AddKnownAlien(gr.myMapItem2);  // PerformObservation()
               if (null != gr.myObserver)
                  gr.myObserver.IsWary = true;
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 2-UNOBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem2.Name + " taking over " + gr.myMapItem1.Name);
               myGameInstance.AddUnknownAlien(gr.myMapItem1);
            }
         }
         else if ((true == gr.myMapItem1.IsAlienKnown) && (true == gr.myMapItem2.IsUncontrolled())) // Alien can be in either of these positions.
         {
            if (true == gr.myIsResult) // true when observed doing takeover
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 1-OBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem1.Name + " taking over " + gr.myMapItem2.Name);
               myGameInstance.AddKnownAlien(gr.myMapItem2); // PerformObservation()
               if (null != gr.myObserver)
                  gr.myObserver.IsWary = true;
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 1-UNOBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem1.Name + " taking over " + gr.myMapItem2.Name);
               myGameInstance.AddUnknownAlien(gr.myMapItem2);
            }
         }
         else if ((true == gr.myMapItem2.IsAlienKnown) && (true == gr.myMapItem1.IsUncontrolled())) // Alien can be in either of these positions.
         {
            if (true == gr.myIsResult) // true when observed doing takeover
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 2-OBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem2.Name + " taking over " + gr.myMapItem1.Name);
               myGameInstance.AddKnownAlien(gr.myMapItem1);  // PerformObservation()
               if (null != gr.myObserver)
                  gr.myObserver.IsWary = true;
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_ALIEN_ADD, "Perform_Observation(): 2-UNOBSERVED - AddingKnownAlien() rightMapItem=" + gr.myMapItem2.Name + " taking over " + gr.myMapItem1.Name);
               myGameInstance.AddUnknownAlien(gr.myMapItem1);
            }
         }
         return true;
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
                           myState = E091Enum.END;
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
