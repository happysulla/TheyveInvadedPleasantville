using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Brushes=System.Windows.Media.Brushes;
using Color=System.Windows.Media.Color;
using FontFamily=System.Windows.Media.FontFamily;
using Image=System.Windows.Controls.Image;
using Label=System.Windows.Controls.Label;
using MessageBox=System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace PleasantvilleGame
{
   public class TerritoryCreateUnitTest : IUnitTest
   {
      private static Double theEllipseDiameter = 20;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      //-----------------------------------------
      private string? myFileName = null;
      private DockPanel myDockPanelTop;
      private Canvas? myCanvas = null;
      private IGameInstance? myGameInstance = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private UIElement? myEllipseSelected = null;
      private Territory? myAnchorTerritory = null;
      private bool myIsDraggingMapItem = false;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ)
      {
         myIndexName = 0;
         myHeaderNames.Add("02-Delete File");
         myHeaderNames.Add("02-Switch Main Canvas");
         myHeaderNames.Add("02-Switch Tank");
         myHeaderNames.Add("02-Delete Territory");
         myHeaderNames.Add("02-New Territories");
         myHeaderNames.Add("02-Set CenterPoints");
         myHeaderNames.Add("02-Verify Territories");
         myHeaderNames.Add("02-Set Adjacents");
         myHeaderNames.Add("02-Set Paved Roads");
         myHeaderNames.Add("02-Set Unpaved Roads");
         myHeaderNames.Add("02-Final");
         //------------------------------------
         myCommandNames.Add("00-Delete File");
         myCommandNames.Add("01-Switch Main Image");
         myCommandNames.Add("02-Change Tank Mat");
         myCommandNames.Add("03-Delete Territory");
         myCommandNames.Add("04-Click Canvas to Add");
         myCommandNames.Add("05-Click Elispse to Move");
         myCommandNames.Add("06-Click Ellispe to Verify");
         myCommandNames.Add("07-Verify Adjacents");
         myCommandNames.Add("08-Verify Paved");
         myCommandNames.Add("09-Verify Unpaved");
         myCommandNames.Add("10-Cleanup");
         //------------------------------------
         myDockPanelTop = dp;
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
         //------------------------------------
         foreach (UIElement ui0 in myDockPanelTop.Children)
         {
            if (ui0 is StackPanel stackPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in stackPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                  }
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                     }
                  }
               }
            }
         }
         //----------------------------------
         if ( false == SetFileName())
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): SetFileName() returned false");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if( null == myGameInstance )
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myGameInstance=null");
            return false;
         }
         if (null == myFileName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myFileName=null");
            return false;
         }
         if (null == myCanvasImageViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasImageViewer=null");
            return false;
         }
         //-----------------------------------
         if (CommandName == myCommandNames[0])
         {
            System.IO.File.Delete(myFileName);  // delete old file
            Territories.theTerritories.Clear();
            if (false == NextTest(ref gi)) // automatically move next test
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1])  // Switch Main Image
         {
            if( false == DeleteEllipses() )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipses() returned false");
               return false;
            }
            if( false == CreateEllipses() )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[2])  // Switch Tank Mat
         {
            if (false == DeleteEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipses() returned false");
               return false;
            }
            //-------------------------------------
            if (false == CreateEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[3])  // Delete Territory
         {

         }
         else if (CommandName == myCommandNames[4])  // New Territory
         {

         }
         else if (CommandName == myCommandNames[5]) // Move territories
         {

         }
         else if (CommandName == myCommandNames[6]) // verify territories
         {

         }
         else if (CommandName == myCommandNames[7]) // set adjacents
         {
            if (false == ShowAdjacents(Territories.theTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): ShowAdjacents() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[8]) // set paved
         {
         }
         else if (CommandName == myCommandNames[9]) // set unpaved
         {
         }
         else 
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myGameInstance=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[0])
         {
            if (false == CreateEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[1]) // Switch Main Canvas Image
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2]) // Switch Tank Mat
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3]) // Click to Add
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4]) // Click Elispse to Move
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5]) // Click Elispse to Verify
         {
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6]) // Click Ellispe to Set Adjacents
         {
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[7]) // Click Ellispe to Set PavedRoads
         {
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[8]) // Click Ellispe to Set UnpavedRoads
         {
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else  // Verify Adjacents
         {
            if (false == Cleanup(ref gi))
            {
               Console.WriteLine("TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         //--------------------------------------------------
         if( false == DeleteEllipses())
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): DeleteEllipses() returned false");
            return false;
         }
         //--------------------------------------------------
         if ( false == CreateXml(Territories.theTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): CreateXml() returned false");
            return false;
         }
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------------------
      private bool SetFileName()
      {
         string? path = ConfigFileReader.theConfigDirectory;
         if (null == path)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path=null");
            return false;
         }
         System.IO.DirectoryInfo? dirInfo = Directory.GetParent(path);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path1 = dirInfo.FullName;
         if (null == path1)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path1=null");
            return false;
         }
         dirInfo = Directory.GetParent(path1);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path2 = dirInfo.FullName;
         if (null == path2)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path2=null");
            return false;
         }
         dirInfo = Directory.GetParent(path2);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path3 = dirInfo.FullName;
         if (null == path3)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path3=null");
            return false;
         }
         dirInfo = Directory.GetParent(path3);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path4 = dirInfo.FullName;
         if (null == path4)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path4=null");
            return false;
         }
         dirInfo = Directory.GetParent(path4);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         myFileName = dirInfo.FullName + "\\Config\\" + Territories.FILENAME;
         return true;
      }
      private bool CreateEllipse(ITerritory territory, IMapPoint mp)
      {
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Colors.Black };
         Ellipse aEllipse = new Ellipse
         {
            Name = territory.Name,
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipse, mp.X);
         Canvas.SetTop(aEllipse, mp.Y);
         myEllipses.Add(aEllipse);
         return true;
      }
      private bool CreateEllipses()
      {
         myEllipses.Clear();
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = System.Windows.Media.Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in Territories.theTerritories)
         {
            Ellipse aEllipse = new Ellipse () { Name = t.Name };
            aEllipse.Fill = aSolidColorBrush0;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = theEllipseDiameter;
            aEllipse.Height = theEllipseDiameter;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myEllipses.Add(aEllipse);
            //-------------------------
            Label aLabel = new Label() { Foreground = Brushes.Red, FontFamily = myFontFam, FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = t.Name };
            p.X -= theEllipseOffset;
            p.Y -= 2 * theEllipseOffset;
            Canvas.SetLeft(aLabel, p.X);
            Canvas.SetTop(aLabel, p.Y);
            //-------------------------
         }
         return true;
      }
      private bool DeleteEllipses()
      {
         myEllipses.Clear();
         return true;
      }
      private bool CreateXml(ITerritories territories)
      {
         if (null == myFileName)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): myFileName=null");
            return false;
         }
         try
         {
            System.IO.File.Delete(myFileName);           // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
            //-----------------------------------------------------
            System.Xml.XmlDocument aXmlDocument = new XmlDocument();
            aXmlDocument.LoadXml("<Territories></Territories>");
            if (null == aXmlDocument.DocumentElement)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
               return false;
            }
            GameLoadMgr loadMgr = new GameLoadMgr();
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (false == loadMgr.CreateXmlTerritories(aXmlDocument, territories))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories() returned false");
               return false;
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            //-----------------------------------------------------
            using (FileStream writer = new FileStream(myFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): exeption=\n" + e.Message);
            return false;
         }
         return true;
      }
      private bool ShowAdjacents(ITerritories territories)
      {
         myAnchorTerritory = null;
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         return true;
      }
      //--------------------------------------------------------------------
      void MouseLeftButtonDownDeleteTerritory(object sender, MouseButtonEventArgs e)
      {
         if( null == myCanvas )
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvas);
         //--------------------------------------------
         Ellipse? selectedEllipse = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ellipse.IsMouseOver)
               {
                  selectedEllipse = ellipse;
                  break;
               }
            }
         }
         if( null == selectedEllipse)
         {
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (true == ellipse.IsMouseOver)
                  {
                     selectedEllipse = ellipse;
                     break;
                  }
               }
            }
         }
         if (null == selectedEllipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): selectedEllipse=null");
            return;
         }
         ITerritory? t = Territories.theTerritories.Find(selectedEllipse.Name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): t=null for ellipse.Name=" + selectedEllipse.Name);
            return;
         }
         Territories.theTerritories.Remove(t);
         System.Windows.MessageBox.Show(selectedEllipse.Name);
         myEllipses.Remove(selectedEllipse);
         myCanvas.Children.Remove(selectedEllipse);
      }
      void MouseLeftButtonDownCreateTerritory(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvas);
         TerritoryCreateDialog dialog = new TerritoryCreateDialog(myCanvas); // Get the name from user
         dialog.myTextBoxName.Focus();
         if (true == dialog.ShowDialog())
         {
            Territory territory = new Territory() { CenterPoint = new MapPoint(p.X, p.Y) };
            territory.Name = dialog.myTextBoxName.Text;
            territory.CanvasName = TerritoryCreateDialog.theParentChecked;
            if( "Main" == territory.CanvasName)
               territory.Type = TerritoryCreateDialog.theTypeChecked;
            else
               territory.Type = TerritoryCreateDialog.theCardChecked;
            Territories.theTerritories.Add(territory);
            if ( false == CreateEllipse(territory, territory.CenterPoint))
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCreateTerritory(): CreateEllipse() returned false");
               return;
            }
         }
      }
      void MouseDownEllipseSetCenterPoint(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSetCenterPoint(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvas);
         Console.WriteLine("TerritoryUnitTest.MouseDown(): {0}", p.ToString());
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == myIsDraggingMapItem)
                  {
                     string showText = ellipse.Name + ":";
                     MessageBox.Show(showText);
                     this.myIsDraggingMapItem = true;
                     this.myEllipseSelected = ui;
                  }
               }
            }
         }
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == myIsDraggingMapItem)
                  {
                     MessageBox.Show(ellipse.Name);
                     this.myIsDraggingMapItem = true;
                     this.myEllipseSelected = ui;
                  }
               }
            }
         }
      }
      void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (true == myIsDraggingMapItem)
         {
            if (null != myEllipseSelected)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvas);
               Canvas.SetTop(myEllipseSelected, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myEllipseSelected, newPoint.X - theEllipseOffset);
            }
         }
         else if (true == myIsDraggingMapItem)
         {
            if (null != myEllipseSelected)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvas);
               Canvas.SetTop(myEllipseSelected, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myEllipseSelected, newPoint.X - theEllipseOffset);
            }
         }
      }
      void MouseUp(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point newPoint = new Point();
         if( true == myIsDraggingMapItem)
            newPoint = e.GetPosition(myCanvas);
         else
            newPoint = e.GetPosition(myCanvas);
         this.myIsDraggingMapItem = false;
         this.myIsDraggingMapItem = false;
         if (null != this.myEllipseSelected)
         {
            if (this.myEllipseSelected is Ellipse)
            {
               Ellipse? ellipse = (Ellipse)myEllipseSelected;
               if( null == ellipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): ellipse=null");
                  return;
               }
               string? name1 = ellipse.Name;
               if (null == name1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): name1=null");
                  return;
               }
               ITerritory? t = Territories.theTerritories.Find(name1);
               if( null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): t=null for name1=" + name1);
                  return;
               }
               TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(t);
               dialog.myButtonOk.Focus();
               if (true == dialog.ShowDialog())
               {
                  t.CanvasName = dialog.RadioOutputParent;
                  t.Type = dialog.RadioOutputType;
                  return;
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myEllipseSelected=null");
         }
      }
      void MouseDownEllipseVerify(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): myCanvasTank=null");
            return;
         }
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string? name = ellipse.Name;
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): name=null");
                     return;
                  }
                  ITerritory? t = Territories.theTerritories.Find(name);
                  if (null == t)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): t=null for name=" + name);
                     return;
                  }
                  TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(t);
                  dialog.myButtonOk.Focus();
                  if (true == dialog.ShowDialog())
                  {
                     t.CanvasName = dialog.RadioOutputParent;
                     t.Type = dialog.RadioOutputType;
                     return;
                  }
               }
            }
         }
         //-----------------------------------------------------------------------
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): myCanvas=null");
            return;
         }
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string? name = ellipse.Name;
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): name=null");
                     return;
                  }
                  foreach (Territory t in Territories.theTerritories)
                  {
                     string? name1 = t.Name;
                     if (name1 == name)
                     {
                        TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(t);
                        dialog.myButtonOk.Focus();
                        if (true == dialog.ShowDialog())
                        {
                           t.CanvasName = dialog.RadioOutputParent;
                           t.Type = dialog.RadioOutputType;
                           return;
                        }
                     }
                  }
               }
            }
         }
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvas=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory? selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territories.theTerritories)
                  {
                     if ( selectedEllipse.Name == t.Name )
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Name);
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Name);
                     sb.Append(" ");
                     sb.Append(selectedTerritory.Name);
                     sb.Append(" ");
                     Console.WriteLine("Anchoring {0} ", selectedTerritory.Name);
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Adjacents.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.Name != myAnchorTerritory.Name)
                  {
                     // If the matching territory is not the anchor territory, change its color.
                     selectedEllipse.Fill = aSolidColorBrush2;
                     // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Adjacents where s == selectedTerritory.Name select s;
                     if (0 == results.Count())
                     {
                        Console.WriteLine("Adding {0} ", selectedTerritory.Name);
                        myAnchorTerritory.Adjacents.Add(selectedTerritory.Name);
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
                     // to the adjacents ellipse. Clear the data so another one can be selected.
                     StringBuilder sb = new StringBuilder("Saving"); 
                     sb.Append(selectedEllipse.Name); 
                     sb.Append(" "); sb.Append(myAnchorTerritory.Name);
                     sb.Append(" "); sb.Append(selectedTerritory.Name); 
                     sb.Append(" ");
                     Console.WriteLine("Saving {0} ", selectedTerritory.Name);
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           ITerritory? t = Territories.theTerritories.Find(ellipse1.Name);
                           if (null == t)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): t=null for name=" + ellipse1.Name);
                              return;
                           }
                           if (0 == t.Adjacents.Count)
                              ellipse1.Fill = aSolidColorBrush0;
                           else
                              ellipse1.Fill = aSolidColorBrush1;
                           break;
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
      void MouseLeftButtonDownSetPaved(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetPaved(): myCanvas=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory? selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territories.theTerritories)
                  {
                     if (selectedEllipse.Name == t.Name)
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Name);
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Name);
                     sb.Append(" ");
                     sb.Append(selectedTerritory.Name);
                     sb.Append(" ");
                     Console.WriteLine("Anchoring {0} ", selectedTerritory.Name);
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.PavedRoads.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.Name != myAnchorTerritory.Name)
                  {
                     // If the matching territory is not the anchor territory, change its color.
                     selectedEllipse.Fill = aSolidColorBrush2;
                     // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.PavedRoads where s == selectedTerritory.Name select s;
                     if (0 == results.Count())
                     {
                        Console.WriteLine("Adding {0} ", selectedTerritory.Name);
                        myAnchorTerritory.PavedRoads.Add(selectedTerritory.Name);
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
                     // to the PavedRoad ellipse. Clear the data so another one can be selected.
                     StringBuilder sb = new StringBuilder("Saving");
                     sb.Append(selectedEllipse.Name);
                     sb.Append(" "); sb.Append(myAnchorTerritory.Name);
                     sb.Append(" "); sb.Append(selectedTerritory.Name);
                     sb.Append(" ");
                     Console.WriteLine("Saving {0} ", selectedTerritory.Name);
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territories.theTerritories)
                           {
                              if (ellipse1.Name == t.Name)
                              {
                                 if (0 == t.PavedRoads.Count)
                                    ellipse1.Fill = aSolidColorBrush0;
                                 else
                                    ellipse1.Fill = aSolidColorBrush1;
                                 break;
                              }
                           }
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvas.Children)
      }
   }
}

