using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using Point = System.Windows.Point;
using CheckBox = System.Windows.Controls.CheckBox;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;

namespace PleasantvilleGame
{
   public class EventViewer : IView
   {
      public bool CtorError { get; } = false;
      private IGameEngine? myGameEngine = null;
      private IGameInstance? myGameInstance = null;
      private ITerritories? myTerritories = null;
      //--------------------------------------------------------------------
      public RuleDialogViewer? myRulesMgr = null;
      private IDieRoller? myDieRoller = null;
      public int DieRoll { set; get; } = 0;
      //--------------------------------------------------------------------
      private ShowReportErrorDialog? myReportErrorDialog = null;
      private ShowAboutDialog? myDialogAbout = null;
      private RuleListingDialog? myDialogRuleListing = null;
      private RuleListingDialog? myDialogEventListing = null;
      private TableListingDialog? myDialogTableListing = null;
       //--------------------------------------------------------------------
      private ScrollViewer? myScrollViewerTextBlock;
      private Canvas? myCanvasMain = null;
      private TextBlock? myTextBlock = null;
      private int myNumSmokeAttacksThisRound = 0;
      //--------------------------------------------------------------------
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //--------------------------------------------------------------------
      public EventViewer(IGameEngine ge, IGameInstance gi, Canvas c, ScrollViewer sv, ITerritories territories, IDieRoller dr)
      {
         myDieRoller = dr;
         //--------------------------------------------------------
         if (null == ge)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------------
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvasMain = c;
         //--------------------------------------------------------
         if (null == territories)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): territories=null");
            CtorError = true;
            return;
         }
         myTerritories = territories;
         //--------------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewerTextBlock = sv;
         //--------------------------------------------------------
         if (myScrollViewerTextBlock.Content is TextBlock)
            myTextBlock = (TextBlock)myScrollViewerTextBlock.Content;  // Find the TextBox in the visual tree
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): myTextBlock=null");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         myRulesMgr = new RuleDialogViewer(myGameInstance, myGameEngine);
         if (true == myRulesMgr.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): RuleDialogViewer.CtorError=true");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         if (false == CreateEvents(gi)) // EventViewer()
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): Create_Events() returned false");
            CtorError = true;
            return;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): myRulesMgr.Events=null");
            CtorError = true;
            return;
         }
      }
      private bool CreateEvents(IGameInstance gi)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Events(): myRulesMgr=null");
            return false;
         }
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Events.txt";
            ConfigFileReader cfr = new ConfigFileReader(filename);
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Events(): cfr.CtorError=true");
               return false;
            }
            myRulesMgr.Events = cfr.Entries;
            if (0 == myRulesMgr.Events.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Events(): myRulesMgr.Events.Count=0");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_ROLL_STATE, "Create_Events(): resetting die rolls gi.DieResults.Count=" + gi.DieResults.Count.ToString());
            foreach (string key in myRulesMgr.Events.Keys) // For each event, create a dictionary entry. There can be no more than three die rolls per event
               gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Events(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myGameInstance=null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myGameEngine=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myRulesMgr=null");
            return;
         }
         if (null == myScrollViewerTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView() myScrollViewerTextBlock=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDieRoller=null");
            return;
         }
         gi.IsGridActive = true;
         switch (action)
         {
            case GameAction.UnitTestCommand:
            case GameAction.UnitTestNext:
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.ShowRuleListingDialog:
               if (null == myDialogRuleListing)
               {
                  myDialogRuleListing = new RuleListingDialog(myRulesMgr, false, CloseRuleListingDialog);
                  if (true == myDialogRuleListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                     return;
                  }
                  myDialogRuleListing.Show();
               }
               else
               {
                  myDialogRuleListing.WindowState = WindowState.Normal;
                  myDialogRuleListing.Activate();
               }
               break;
            case GameAction.ShowEventListingDialog:
               if (null == myDialogEventListing)
               {
                  myDialogEventListing = new RuleListingDialog(myRulesMgr, false, CloseEventListingDialog);
                  if (true == myDialogEventListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogEventListing CtorError=true");
                     return;
                  }
                  myDialogEventListing.Show();
               }
               else
               {
                  myDialogEventListing.WindowState = WindowState.Normal;
                  myDialogEventListing.Activate();
               }
               break;
            case GameAction.ShowTableListing:
               if (null == myDialogTableListing)
               {
                  myDialogTableListing = new TableListingDialog(myRulesMgr, CloseTableListingDialog);
                  if (true == myDialogTableListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogTableListing CtorError=true");
                     return;
                  }
                  myDialogTableListing.Show();
               }
               else
               {
                  myDialogTableListing.WindowState = WindowState.Normal;
                  myDialogTableListing.Activate();
               }
               break;
            case GameAction.ShowCombatCalendarDialog:
               if( false == myRulesMgr.ShowTable("Calendar"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): SmyRulesMgr.ShowTable(Calendar)=false");
                  return;
               }
               break;
            case GameAction.ShowReportErrorDialog:
               if (null == myReportErrorDialog)
               {
                  myReportErrorDialog = new ShowReportErrorDialog(CloseReportErrorDialog);
                  myReportErrorDialog.Show();
               }
               else
               {
                  myReportErrorDialog.WindowState = WindowState.Normal;
                  myReportErrorDialog.Activate();
               }
               break;
            case GameAction.ShowAboutDialog:
               if (null == myDialogAbout)
               {
                  myDialogAbout = new ShowAboutDialog(CloseShowAboutDialog);
                  if (true == myDialogAbout.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogAbout CtorError=true");
                     return;
                  }
                  myDialogAbout.Show();
               }
               else
               {
                  myDialogAbout.WindowState = WindowState.Normal;
                  myDialogAbout.Activate();
               }
               break;
            case GameAction.ShowGameFeatsDialog:
               ShowFeatDisplayDialog dialogShowFeats = new ShowFeatDisplayDialog(myRulesMgr);
               if (true == dialogShowFeats.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FeatDisplayDialog CtorError=true");
                  return;
               }
               if (true == dialogShowFeats.ShowDialog())
               {
               }
               break;
            case GameAction.EndGameLost:
            case GameAction.EndGameWin:
            default:
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
         }
      }
      public bool OpenEvent(IGameInstance gi, string key)
      {
         if (null == myScrollViewerTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myScrollViewerTextBlock=null");
            return false;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myTextBlock=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myRulesMgr=null");
            return false;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myRulesMgr.Events=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myDieRoller=null");
            return false;
         }
         //------------------------------------
         try
         {
            foreach (Inline inline in myTextBlock.Inlines) // Clean up resources from old link before adding new one
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                  {
                     b.Click -= Button_Click;
                  }
                  if (ui.Child is CheckBox cb)
                  {
                     cb.Checked -= CheckBox_Checked;
                     cb.Unchecked -= CheckBox_Unchecked;
                  }
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         try
         {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='573' Background='#FFB9EA9E' FontFamily='Georgia' FontSize='18' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' LineStackingStrategy='BlockLineHeight' Margin='0,0,0,0'>");
            sb.Append(myRulesMgr.Events[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlock = (TextBlock)XamlReader.Load(xr);
         }
         catch (System.Windows.Markup.XamlParseException e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         myScrollViewerTextBlock.Content = myTextBlock;
         myTextBlock.MouseDown += TextBlock_MouseDown;
         //--------------------------------------------------
         int dieNumIndex = 0;
         bool isModified = true;
         bool[] isDieShown = new bool[4] { true, false, false, false };
         int[]? eventDieRolls = null;
         try
         {
            eventDieRolls = gi.DieResults[key];
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): gi.DieResults[key] exception for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         while (dieNumIndex < 3 && true == isModified) // substitute die rolls that have occurred when multiple die rolls are in myTextBlock
         {
            int dieCount = 0;
            isModified = false;
            foreach (Inline inline in myTextBlock.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                  {
                     if (false == SetButtonState(gi, key, b))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): SetButtonState() returned false");
                        return false;
                     }
                  }
                  else if (ui.Child is CheckBox cb)
                  {
                     if (false == SetCheckboxState(gi, key, cb))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): SetCheckboxState() returned false");
                        return false;
                     }
                  }
                  else if (ui.Child is Image img)
                  {
                     string imageName = img.Name;
                     if (true == img.Name.Contains("Continue"))
                        imageName = "Continue";
                     else if (true == img.Name.Contains("Ambulance"))
                        imageName = "Ambulance";
                     else if( true == img.Name.Contains("DieRollWhite"))
                     {
                        imageName = "DieRollWhite";
                     }
                     else if (true == img.Name.Contains("DieRollBlue"))
                     {
                        imageName = "DieRollBlue";
                     }
                     else if (true == img.Name.Contains("DieRoll"))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): imageName=DieRoll for key=" + key);
                        return false;
                     }
                     string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
                     System.Windows.Media.Imaging.BitmapImage bitImage = new BitmapImage();
                     bitImage.BeginInit();
                     bitImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
                     bitImage.EndInit();
                     img.Source = bitImage;
                     ImageBehavior.SetAnimatedSource(img, img.Source);
                     if ((true == img.Name.Contains("DieRollWhite")) || (true == img.Name.Contains("DieRollBlue")))
                     {
                        //RoutedCommand command = new RoutedCommand();
                        //KeyGesture keyGesture = new KeyGesture(Key.Enter, ModifierKeys.None);
                        //myTextBlock.InputBindings.Add(new KeyBinding(command, keyGesture));
                        //myTextBlock.CommandBindings.Add(new CommandBinding(command, myTextBlock.MouseDown));
                        if (true == isDieShown[dieCount])
                        {
                           if (Utilities.NO_RESULT == eventDieRolls[dieNumIndex]) // if true, perform a one time insert b/c dieNumIndex increments by one
                           {

                           }
                           else
                           {
                              img.Visibility = Visibility.Hidden;
                              if (false == gi.IsMultipleSelectForDieResult)
                              {
                                 Run newInline = new Run(eventDieRolls[dieNumIndex].ToString());  // Insert the die roll number result
                                 myTextBlock.Inlines.InsertBefore(inline, newInline); // If modified, need to start again
                              }
                              else
                              {
                                 Button b1 = new Button() { Content = eventDieRolls[dieNumIndex].ToString(), FontFamily = myFontFam1, FontSize = 12, Height = 16, Width = 48 };
                                 myTextBlock.Inlines.InsertAfter(inline, new InlineUIContainer(b1));
                                 b1.Click += Button_Click;
                              }
                              isModified = true;
                              ++dieNumIndex;
                              isDieShown[dieCount] = false;
                              isDieShown[++dieCount] = true;
                              break;
                           }
                        }
                     }
                  }
               }
            }// end foreach
         } // end while
           //--------------------------------------------------
         myDieRoller.DieMutex.WaitOne();
         if( false == UpdateEventContent(gi, key))
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): UpdateEventContent() returned false");
            return false;
         }
         myDieRoller.DieMutex.ReleaseMutex();
         //--------------------------------------------------
         if (gi.EventDisplayed == gi.EventActive)
         {
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerActive;
            myTextBlock.Background = Utilities.theBrushScrollViewerActive;
         }
         else
         {
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerInActive;
            myTextBlock.Background = Utilities.theBrushScrollViewerInActive;
         }
            
         return true;
      }
      public bool ShowRule(string key)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule(): myRulesMgr=null");
            return false;
         }
         if (false == myRulesMgr.ShowRule(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowTable(string key)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable(): myRulesMgr=null");
            return false;
         }
         if (false == myRulesMgr.ShowTable(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowRegion(string key)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): myCanvasMain=null");
            return false;
         }
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): myTerritories=null");
            return false;
         }
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Polygon)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasMain.Children.Remove(ui1);
         //--------------------------------
         ITerritory? t = myTerritories.Find(key);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): Unable to find name=" + key);
            return false;
         }
         //--------------------------------
         if (false == SetThumbnailState(myCanvasMain, t))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): SetThumbnailState returned false name=" + key);
            return false;
         }
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
         myCanvasMain.Children.Add(aPolygon);
         return true;
      }
      //--------------------------------------------------------------------
      private bool UpdateEventContent(IGameInstance gi, string key)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myGameEngine=null");
            return false;
         }
         if ( null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myTextBlock=null");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_APPEND_EVENT, "UpdateEventContent(): k=" + key + " d0=" + gi.DieResults[key][0].ToString() + " d1=" + gi.DieResults[key][1].ToString() + " d2=" + gi.DieResults[key][2].ToString());
         //--------------------------------------------------------------------------
         int firstDieResult = gi.DieResults[key][0];
         switch (key)
         {
            case "e101":
               break;
            case "e102":
               break;
            case "e103":
               break;
            case "e104":
               break;
            case "e501":
               StringBuilder sbEndWon = new StringBuilder();
               sbEndWon.Append("Game ends on Turn#");
               sbEndWon.Append(gi.GameTurn.ToString());
               sbEndWon.Append(" due to '");
               sbEndWon.Append(gi.EndGameReason);
               sbEndWon.Append("'");
               myTextBlock.Inlines.Add(new Run(sbEndWon.ToString()));
               Image? imgEndGameWon = null;
               int randnumWon = Utilities.RandomGenerator.Next(10);
               switch (randnumWon)
               {
                  case 0:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 1:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                             "));
                     break;
                  case 2:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win4"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
                  case 3:
                  case 4:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win1"), Width = 300, Height = 160 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                             "));
                     break;
                  case 5:
                  case 6:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win2"), Width = 300, Height = 200 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
                  case 7:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win3"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
                  case 8:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win5"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
                  default:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Star"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
               }
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameWon));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               break;
            case "e502":
               StringBuilder sbEndLost = new StringBuilder();
               sbEndLost.Append("Game ends on Turn#");
               sbEndLost.Append(gi.GameTurn.ToString());
               sbEndLost.Append(" due to ");
               sbEndLost.Append(gi.EndGameReason);
               myTextBlock.Inlines.Add(new Run(sbEndLost.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image? imgEndGameLost = null;
               int randnumLost = Utilities.RandomGenerator.Next(9);
               switch (randnumLost)
               {
                  case 0:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 1:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Idiot"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 2:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("OBlood1"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 3:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("FarmerDead"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 4:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Skulls"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  case 5:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Loser1"), Width = 260, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                                         "));
                     break;
                  case 6:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Loser2"), Width = 300, Height = 230 };
                     myTextBlock.Inlines.Add(new Run("                                  "));
                     break;
                  default:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Frown"), Width = 300, Height = 300 };
                     myTextBlock.Inlines.Add(new Run("                           "));
                     break;
               }
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameLost));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               gi.Statistics.AddOne("NumOfScenariosLost");
               break;
            case "e503":
               Option optionSingleDayScenario = gi.Options.Find("SingleDayScenario");
               if ((true == optionSingleDayScenario.IsEnabled) || (gi.Day < 172))
               {
                  myTextBlock.Inlines.Add(new Run("Receive a Europe Campaign medal for participating in Europe Campaign."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imgEndGame1 = new Image { Name = "ExitGame", Source = MapItem.theMapImages.GetBitmapImage("DecorationEasternCampaign"), Width = 150, Height = 300 };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGame1));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run("Receive a Europe Campaign medal for participation and Victory medal since after Feb 1945."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imgEndGame2 = new Image { Name = "ExitGame", Source = MapItem.theMapImages.GetBitmapImage("DecorationVictoryMedal"), Width = 300, Height = 300 };
                  myTextBlock.Inlines.Add(new Run("                        "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGame2));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Select "));
               myTextBlock.Inlines.Add(new Run("'File | New'") { FontWeight = FontWeights.Bold });
               myTextBlock.Inlines.Add(new Run(" menu option to play again --or--"));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to exit the game."));
               break;
            default:
               break;
         }
         return true;
      }
      //--------------------------------------------------------------------
      private void ReplaceText(string keyword, string newString)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReplaceText(): myTextBlock=null");
            return;
         }
         TextRange text = new TextRange(myTextBlock.ContentStart, myTextBlock.ContentEnd);
         TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
         while (current != null)
         {
            string textInRun = current.GetTextInRun(LogicalDirection.Forward);
            if (!string.IsNullOrWhiteSpace(textInRun))
            {
               int index = textInRun.IndexOf(keyword);
               if (index != -1)
               {
                  TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                  TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
                  TextRange selection = new TextRange(selectionStart, selectionEnd);
                  selection.Text = newString;
               }
            }
            current = current.GetNextContextPosition(LogicalDirection.Forward);
         }
      }
      private bool SetThumbnailState(Canvas c, ITerritory t)
      {
         ScrollViewer scrollViewer = (ScrollViewer)c.Parent; // set thumbnails of scroll viewer to find the target hex
         if (null == scrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetThumbnailState(): scrollViewer=null");
            return false;
         }
         double percentHeight = (t.CenterPoint.Y / c.ActualHeight);
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double amountToScroll = percentToScroll * scrollViewer.ScrollableHeight;
         scrollViewer.ScrollToVerticalOffset(amountToScroll);
         //--------------------------------------------------------------------
         double percentWidth = (t.CenterPoint.X / c.ActualWidth);
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
         amountToScroll = percentToScroll * scrollViewer.ScrollableWidth;
         scrollViewer.ScrollToHorizontalOffset(amountToScroll);
         return true;
      }
      private bool SetButtonState(IGameInstance gi, string key, Button b)
      {
         string content = (string)b.Content;
         if (null == content)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): content=null for key=" + key);
            return false;
         }
          //---------------------------------------------------
         if ((key != gi.EventActive) && (false == content.StartsWith("e")))
         {
            b.IsEnabled = false;
            return true;
         }
         switch (key)
         {
            default:
               break;
         }
         b.Click += Button_Click;
         return true;
      }
      private bool SetCheckboxState(IGameInstance gi, string key, CheckBox cb)
      {
         Logger.Log(LogEnum.LE_VIEW_CONTROL_NAME, "Set_CheckboxState(): cb.Name=" + cb.Name + " for ae=" + gi.EventActive);
         //------------------------------------
         Option option = gi.Options.Find(cb.Name);
         if (true == option.IsEnabled)
            cb.IsChecked = true;
         //------------------------------------
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Set_CheckboxState(): myGameInstance=null");
            return false;
         }
         if (("e011" == key) || ("e011a" == key)) 
            cb.Visibility = Visibility.Hidden;
         else
            cb.Visibility = Visibility.Visible;
         //------------------------------------
         cb.Checked += CheckBox_Checked;
         cb.Unchecked += CheckBox_Unchecked;
         return true;
      }
      //--------------------------------------------------------------------
      public void CloseEventListingDialog()
      {
         myDialogEventListing = null;
      }
      public void CloseTableListingDialog()
      {
         myDialogTableListing = null;
      }
      public void CloseRuleListingDialog()
      {
         myDialogRuleListing = null;
      }
      public void CloseReportErrorDialog()
      {
         myReportErrorDialog = null;
      }
      public void CloseShowAboutDialog()
      {
         myDialogAbout = null;
      }
      //--------------------------------------------------------------------
      public void ShowDieResult(int dieRoll)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult(): myGameInstance=null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult(): myGameEngine=null");
            return;
         }
         myGameInstance.EventActive = myGameInstance.EventDisplayed; // As soon as you roll the die, the current event becomes the active event
         GameAction action = myGameInstance.DieRollAction;
         StringBuilder sb11 = new StringBuilder("      ######ShowDieResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(action.ToString());
         sb11.Append(" dr="); sb11.Append(dieRoll.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref action, dieRoll);
      }
      //--------------------------------------------------------------------
      private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myGameInstance=null");
            return;
         }
         if ( null == myCanvasMain )
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myCanvasMain=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myDieRoller=null");
            return;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myTextBlock=null");
            return;
         }
         //------------------------------------------------------------------------
         GameAction action = GameAction.Error;
         if (myGameInstance.EventActive != myGameInstance.EventDisplayed) // if an image is clicked, only take action if on active screen
         {
            ReturnToActiveEventDialog dialog = new ReturnToActiveEventDialog(); // Get the name from user
            dialog.Topmost = true;
            if (true == dialog.ShowDialog())
            {
               GameAction actionGoto = GameAction.UpdateEventViewerActive;
               myGameEngine.PerformAction(ref myGameInstance, ref actionGoto);
            }
            return;
         }
         //------------------------------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myTextBlock, p);  // Get the Point where the hit test occurrs
         foreach (Inline item in myTextBlock.Inlines)
         {
            if (item is InlineUIContainer ui)
            {
               if (ui.Child is Image)
               {
                  Image img = (Image)ui.Child;
                  if (result.VisualHit == img)
                  {
                     RollEndCallback rollEndCallback = ShowDieResult;
                     switch (img.Name)
                     {
                        case "DieRollWhite":
                           myDieRoller.RollMovingDie(myCanvasMain, rollEndCallback);
                           img.Visibility = Visibility.Hidden;
                           return;
                        case "DieRollBlue":
                           myDieRoller.RollMovingDice(myCanvasMain, rollEndCallback);
                           img.Visibility = Visibility.Hidden;
                           return;
                        case "ExitGame":
                           action = GameAction.EndGameExit;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "EndGameShowStats":
                           GameFeat changedFeat;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Reset_RoundSetState(): \n  Feats=" + GameEngine.theInGameFeats.ToString() + " \n SFeats=" + GameEngine.theStartingFeats.ToString());
                           if (false == GameEngine.theInGameFeats.GetFeatChange(GameEngine.theStartingFeats, out changedFeat)) // End Game Feats and Stats
                           {
                              Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "TextBlock_MouseDown(): Get_FeatChange() returned false");
                              return;
                           }
                           if( true == String.IsNullOrEmpty(changedFeat.Key))
                           {
                              action = GameAction.EndGameShowStats;
                           }
                           else
                           {
                              Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "TextBlock_MouseDown(): Change=" + changedFeat.ToString());
                              action = GameAction.EndGameShowFeats;
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        default:
                           break;// do nothing
                     }
                  }
               }
            }
         }
         //---------------------------------------------
         // Click anywhere to continue
         switch (myGameInstance.EventActive)
         {

            default:
               break;
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameInstance=null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameEngine=null");
            return;
         }
         GameAction action = GameAction.Error;
         Button b = (Button)sender;
         e.Handled = true;
         //----------------------------------------------------
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // event based click
         {
            myGameInstance.EventDisplayed = key;
            action = GameAction.UpdateEventViewerDisplay;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         else
         {
            if (false == Button_ClickShowOther(key, b.Name, out action))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): CloseEvent() return false for key=" + key);
               return;
            }
         }
      }
      private bool Button_ClickShowOther(string content, string name, out GameAction action)
      {
         action = GameAction.Error;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): myGameEngine=null");
            return false;
         }
         switch (content)
         {
            case "   -   ":
               switch (name)
               {
                  case "Button_ShowOptions":
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): reached default Unknown name=" + name);
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "   +   ":
               switch (name)
               {
                  case "Button_ShowOptions":
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): reached default Unknown name=" + name);
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            default:
               if (false == ShowTable(content))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowTable() returned false for key=" + content);
                  return false;
               }
               break;
         }
         return true;
      }
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): myGameInstance=null");
            return;
         }
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         Option option = myGameInstance.Options.Find(cb.Name);
         option.IsEnabled = true;
      }
      private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): myGameInstance=null");
            return;
         }
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         Option option = myGameInstance.Options.Find(cb.Name);
         option.IsEnabled = false;
      }
   }
}
