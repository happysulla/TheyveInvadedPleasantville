using System.Drawing.Drawing2D;
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
      private DockPanel myDockPanelTop;
      private Canvas? myCanvasMain = null;
      private Canvas? myCanvasHelper = null;
      private IGameInstance? myGameInstance = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private UIElement? myEllipseSelected = null;
      private ITerritory? myAnchorTerritory = null;
      private bool myIsDraggingMapItem = false;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------
      private string? myFileName = null;
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //-----------------------------------------
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ)
      {
         myIndexName = 0;
         myHeaderNames.Add("02-Delete File");
         myHeaderNames.Add("02-Delete Territory");
         myHeaderNames.Add("02-New Territories");
         myHeaderNames.Add("02-Set CenterPoints");
         myHeaderNames.Add("02-Verify Territories");
         myHeaderNames.Add("02-Set Adjacents");
         myHeaderNames.Add("02-Set Observations");
         myHeaderNames.Add("02-GetMapItemsWithinRange");
         myHeaderNames.Add("02-GetOverlappingTerritories");
         myHeaderNames.Add("02-Final");
         //------------------------------------
         myCommandNames.Add("00-Delete File");
         myCommandNames.Add("01-Delete Territory");
         myCommandNames.Add("02-Click Canvas to Add");
         myCommandNames.Add("03-Click Elispse to Move");
         myCommandNames.Add("04-Click Ellispe to Verify");
         myCommandNames.Add("05-Verify Adjacents");
         myCommandNames.Add("06-Verify Observations");
         myCommandNames.Add("07-Verify GetMapItemsWithinRange");
         myCommandNames.Add("08-Verify GetOverlappingTerritories");
         myCommandNames.Add("09-Cleanup");
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
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     ScrollViewer sv = (ScrollViewer)ui1;
                     if (sv.Content is Canvas)
                        myCanvasMain = (Canvas)sv.Content;  // Find the Canvas in the visual tree
                  }
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                        if (ui2 is Canvas)
                           myCanvasHelper = (Canvas)ui2;
                     }
                  }
               }
            }
         }
         if (null == myCanvasMain) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
         }
         if (null == myCanvasHelper) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasHelper=null");
            CtorError = true;
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
         else if (CommandName == myCommandNames[1])  // Delete Territory
         {

         }
         else if (CommandName == myCommandNames[2])  // New Territory
         {

         }
         else if (CommandName == myCommandNames[3]) // Move territories
         {

         }
         else if (CommandName == myCommandNames[4]) // verify territories
         {

         }
         else if (CommandName == myCommandNames[5]) // set adjacents
         {
            if (false == ShowAdjacents(Territories.theTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): ShowAdjacents() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[6]) // set observations
         {
            if (false == ShowObservations(Territories.theTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): ShowObservations() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[7]) // MapItemsWithinRange
         {
            string name = "GasPumps_0";
            ITerritory? t1 = Territories.theTerritories.Find(name);
            if( null == t1 )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): t1=null for ");
               return false;
            }
            IMapItems? closeMapItems = Territory.GetMapItemsWithinRange(gi, t1, 5); // Find mapitems that can be moved to the targets location
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): GetMapItemsWithinRange() returned error");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_UNIT_TEST, "TerritoryCreateUnitTest.Command(): closeMapItems=" + closeMapItems.ToString());
            IMapItem? bankPres = gi.Townspeople.Find("BankPresident");
            if( null == bankPres)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): bankPres=null");
               return false;
            }
            if( true == closeMapItems.Contains(bankPres))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): closeMapItems contains bankPres");
               return false;
            }
            //---------------------------------------------------
            IMapItem? stationAttend = gi.Townspeople.Find("Station");
            if (null == stationAttend)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): stationAttend=null");
               return false;
            }
            closeMapItems = Territory.GetMapItemsWithinRange(gi, stationAttend); // Find mapitems that stationAttend can move to
            if (null == closeMapItems)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): GetMapItemsWithinRange() returned error");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_UNIT_TEST, "TerritoryCreateUnitTest.Command(): closeMapItems=" + closeMapItems.ToString());
            if (false == closeMapItems.Contains(bankPres))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): closeMapItems DOES NOT contain bankPres");
               return false;
            }
         }
         else if (CommandName == myCommandNames[8]) // GetOverlappingTerritories
         {
            DeleteEllipses();
            int r1 = Utilities.RandomGenerator.Next(gi.Townspeople.Count);
            int r2 = Utilities.RandomGenerator.Next(gi.Townspeople.Count);
            while (r1 == r2)
               r2 = Utilities.RandomGenerator.Next(gi.Townspeople.Count);
            IMapItem? mi1 = gi.Townspeople[r1];
            if (null == mi1 )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): mi1=null ");
               return false;
            }
            if (false == CreateEllipse(mi1.TerritoryCurrent, mi1.TerritoryCurrent.CenterPoint))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipse() returned false for mi1");
               return false;
            }
            IMapItem? mi2 = gi.Townspeople[r2];
            if (null == mi2)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): mi2=null");
               return false;
            }
            if (false == CreateEllipse(mi2.TerritoryCurrent, mi2.TerritoryCurrent.CenterPoint))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipse() returned false for mi2");
               return false;
            }
            //----------------------------------------------------
            List<string>? tNames = Territory.GetOverlappingTerritories(gi, mi1, mi2);
            if( null == tNames)
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): tNames=null");
               return false;
            }
            //----------------------------------------------------
            ITerritories territoriesToDisplay = new Territories();
            foreach(string tName in tNames)
            {
               ITerritory? t = Territories.theTerritories.Find(tName);
               if( null == t )
               {
                  Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): t=null");
                  return false;
               }
               territoriesToDisplay.Add(t);
            }
            Logger.Log(LogEnum.LE_SHOW_UNIT_TEST, "TerritoryCreateUnitTest.Command(): mi1=" + mi1.Name + " mi2=" + mi2.Name + " overlap=" + territoriesToDisplay.ToString());
            //----------------------------------------------------
            if ( false == CreateEllipsesForDisplay(territoriesToDisplay))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipsesForDisplay() returned false");
               return false;
            }
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasMain=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownDeleteTerritory;
            if (false == CreateEllipsesForDisplay(Territories.theTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "Next_Test(): Create_EllipsesForDisplay() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[1])  // Click to Add
         {
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownDeleteTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
         }
         else if (HeaderName == myHeaderNames[2])  // Click Elispse to Move
         {
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseDownEllipseSetCenterPoint;
            myCanvasMain.MouseMove += MouseMove;
            myCanvasMain.MouseUp += MouseUp;
         }
         else if (HeaderName == myHeaderNames[3]) // Click Elispse to Verify
         {
            ++myIndexName;
            myCanvasMain.MouseMove -= MouseMove;
            myCanvasMain.MouseUp -= MouseUp;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseSetCenterPoint;
            myCanvasMain.MouseLeftButtonDown += this.MouseDownEllipseVerify;
         }
         else if (HeaderName == myHeaderNames[4]) // Click Ellispe to Set Adjacents
         {
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseVerify;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
         }
         else if (HeaderName == myHeaderNames[5]) // Click Ellispe to Set Observations
         {
            myAnchorTerritory = null;
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownSetObservations;
         }
         else if (HeaderName == myHeaderNames[6]) // start cleanup
         {
            if (false == DeleteEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "Cleanup(): DeleteEllipses() returned false");
               return false;
            }
            myAnchorTerritory = null;
            ++myIndexName;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownSetObservations;
            if( false == CreateTownspeople(gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Next_Test(): Create_Townspeople() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[7]) // start cleanup
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[8]) // start cleanup
         {
            ++myIndexName;
         }
         else  
         {
            if (false == Cleanup(ref gi))
            {
               System.Diagnostics.Debug.WriteLine("TerritoryCreateUnitTest.Command(): Cleanup() returned false");
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
         if( null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasMain=null");
            return false;
         }
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Colors.Black };
         Ellipse aEllipse = new Ellipse
         {
            Name = territory.ToString(),
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         myCanvasMain.Children.Add(aEllipse);
         Canvas.SetLeft(aEllipse, mp.X);
         Canvas.SetTop(aEllipse, mp.Y);
         Canvas.SetZIndex(aEllipse, 99999);
         myEllipses.Add(aEllipse);
         return true;
      }
      private bool CreateEllipsesForDisplay(ITerritories territories)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_EllipsesForDisplay(): myCanvasMain=null");
            return false;
         }
         myEllipses.Clear();
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = System.Windows.Media.Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in territories)
         {
            Ellipse aEllipse = new Ellipse () { Name = t.ToString() };
            aEllipse.Fill = aSolidColorBrush0;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = theEllipseDiameter;
            aEllipse.Height = theEllipseDiameter;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            myCanvasMain.Children.Add(aEllipse);
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myEllipses.Add(aEllipse);
            //-------------------------
            Label aLabel = new Label() { Foreground = Brushes.Red, FontFamily = myFontFam, FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = t.ToString() };
            p.X -= theEllipseOffset;
            p.Y -= 2 * theEllipseOffset;
            myCanvasMain.Children.Add(aLabel);
            Canvas.SetLeft(aLabel, p.X);
            Canvas.SetTop(aLabel, p.Y);
         }
         return true;
      }
      private bool DeleteEllipses()
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipses(): myCanvasMain=null");
            return false;
         }
         myEllipses.Clear();
         List<UIElement> results1 = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results1.Add(ui);
            if (ui is Label)
               results1.Add(ui);
         }
         foreach (UIElement ui1 in results1)
            myCanvasMain.Children.Remove(ui1);
         return true;
      }
      private bool CreateXml(ITerritories territories)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         if (null == myFileName)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): myFileName=null");
            return false;
         }
         try
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //-----------------------------------------------------
            System.IO.File.Delete(myFileName);           // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
            //-----------------------------------------------------
            System.Xml.XmlDocument aXmlDocument = new XmlDocument();
            aXmlDocument.LoadXml("<Territories></Territories>");
            if (null == aXmlDocument.DocumentElement)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               return false;
            }
            GameLoadMgr loadMgr = new GameLoadMgr();
            if (false == loadMgr.CreateXmlTerritories(aXmlDocument, territories))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories() returned false");
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               return false;
            }
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
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return false;
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return true;
      }
      private bool ShowAdjacents(ITerritories territories)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_Adjacents(): myCanvasMain=null");
            return false;
         }
         myAnchorTerritory = null;
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in territories)
         {
            StringBuilder sb1 = new StringBuilder("Ellipses=[");
            Ellipse? anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  sb1.Append(",");
                  sb1.Append(ellipse.Name);
                  if (anchorTerritory.ToString() == ellipse.Name)
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            sb1.Append("]");
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, "Show_Adjacents(): anchorEllipse=null for " + anchorTerritory.ToString() + " " + sb1.ToString() );
               return false;
            }
            if (0 < anchorTerritory.Adjacents.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            foreach (string s in anchorTerritory.Adjacents)  // At this point, the anchorEllipse and the anchorTerritory are found.
            {
               ITerritory? adjacentTerritory = null;
               foreach (ITerritory t in territories) // Find the River Territory corresponding to this name
               {
                  if (t.ToString() == s)
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
               if (null == adjacentTerritory)
               {
                  MessageBox.Show("Show_Adjacents(): Not Found s=" + s);
                  return false;
               }
               string? adjacentName = adjacentTerritory.ToString();
               if( null == adjacentName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Show_Adjacents(): adjacentName=null for " + anchorTerritory.Name);
                  return false;
               }
               Ellipse? adjacentEllipse = null; // Find the corresponding ellipse for this territory
               foreach (UIElement ui in myCanvasMain.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (adjacentName == ellipse.Name)
                     {
                        adjacentEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == adjacentEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, adjacentName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               //-------------------------------------------------
               bool isReturnFound = false;
               foreach (String s1 in adjacentTerritory.Adjacents) // Search the Adjacent Territory  List to make sure the anchor territory is in that list. It should be bi directional.
               {
                  string returnName = s1;
                  if (returnName == anchorTerritory.ToString())
                  {
                     isReturnFound = true; // Yes the adjacent River has a entry to return the River back to the anchor territory
                     break;
                  }
               }
               //-------------------------------------------------
               if (false == isReturnFound) // Anchor Property not found in the adjacent property territory.  This is an error condition.
               {
                  anchorEllipse.Fill = aSolidColorBrush3; // change color of two ellipses to signify error
                  adjacentEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("anchor=");
                  sb.Append(anchorTerritory.ToString());
                  sb.Append(" NOT in list for adjacent=");
                  sb.Append(adjacentName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      private bool ShowObservations(ITerritories territories)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_Observations(): myCanvasMain=null");
            return false;
         }
         myAnchorTerritory = null;
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in territories)
         {
            StringBuilder sb1 = new StringBuilder("Ellipses=[");
            Ellipse? anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  sb1.Append(",");
                  sb1.Append(ellipse.Name);
                  if (anchorTerritory.ToString() == ellipse.Name)
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            sb1.Append("]");
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, "Show_Observations(): anchorEllipse=null for " + anchorTerritory.ToString() + " " + sb1.ToString());
               return false;
            }
            if (0 < anchorTerritory.Observations.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            foreach (var kvp in anchorTerritory.Observations)  // At this point, the anchorEllipse and the anchorTerritory are found.
            {
               ITerritory? obsTerritory = null;
               foreach (ITerritory t in territories) 
               {
                  if (t.ToString() == kvp.Key)
                  {
                     obsTerritory = t;
                     break;
                  }
               }
               if (null == obsTerritory)
               {
                  MessageBox.Show("Show_Observations(): Not Found kvp.Key=" + kvp.Key);
                  return false;
               }
               string? obsName = obsTerritory.ToString();
               if (null == obsName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Show_Observations(): obsName=null for " + anchorTerritory.ToString());
                  return false;
               }
               Ellipse? adjacentEllipse = null; // Find the corresponding ellipse for this territory
               foreach (UIElement ui in myCanvasMain.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (obsName == ellipse.Name)
                     {
                        adjacentEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == adjacentEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, obsName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               //-------------------------------------------------
               bool isReturnFound = false;
               foreach (var kvp1 in obsTerritory.Observations) // Search the Adjacent Territory  List to make sure the anchor territory is in that list. It should be bi directional.
               {
                  if (kvp1.Key == anchorTerritory.ToString())
                  {
                     isReturnFound = true;
                     break;
                  }
               }
               //-------------------------------------------------
               if (false == isReturnFound) // Anchor Property not found in the observation property territory.  This is an error condition.
               {
                  anchorEllipse.Fill = aSolidColorBrush3; // change color of two ellipses to signify error
                  adjacentEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("anchor=");
                  sb.Append(anchorTerritory.ToString());
                  sb.Append(" NOT in list for observations=");
                  sb.Append(obsName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      private bool CreateTownspeople(IGameInstance gi)
      {
         gi.Townspeople.Clear();
         int maxNum = 4;
         int randomNum = Utilities.RandomGenerator.Next(maxNum);
         //------------------------------------
         string tName = "Bank_0";
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
            return false;
         }
         string name = "BankGuard";
         string miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem mi = new MapItem(miName, 0.8, name, t, 5, 10, 8);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         tName = "Bank_0";  // put BankPresident who only moves 4 in Bank_0 to see if it can move to GasPumps_0
         name = "BankPresident";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 19, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "BarAndGrill_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "BarAndGrillOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Tavern_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "BarTender";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Supermarket_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "CheckoutGirl";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 7, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Deputy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 11, 9);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "DocOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Doctor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 18, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "FireChief";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 12, 8);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "HotelOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "TownHall_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Judge";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "LawyersOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Lawyer";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 11, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Maid";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "MaitreD";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 4);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "GeneralStore_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Mayor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 16, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.Name + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Church_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Minister";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 20, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.Name + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "House_K";
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Paperboy";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 9, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Plumber";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 8);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "RepairShopOwner";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "SheriffFireDept_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Sheriff";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 6, 15, 10);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "GasPumps_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "StationAttendant";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 8, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Supermarket_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "SuperMarketManager";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "ClothingStore_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Tailor";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 11, 5);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "School_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Teacher";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 17, 4);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Bank_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Teller";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 3;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "Tavern_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "TownDrunk";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 3, 3, 8);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "VetOffice_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Vet";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 13, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 5;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "HotelAndRestaurant_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Waitress";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 9, 6);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 2;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "TrainStation_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "WarVeteran";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 12, 4);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 4;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "MachineShop_" + tNum.ToString();
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Welder";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 5, 10, 7);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         maxNum = 1;
         randomNum = Utilities.RandomGenerator.Next(maxNum);
         for (int i = 0; i < maxNum; i++)
         {
            int tNum = (randomNum + i) % maxNum;
            tName = "House_A";
            t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Townspeople(): unable to find tName=" + tName);
               return false;
            }
            IStack? tStack = gi.Stacks.Find(t);
            if (null == tStack) // if stack exists, then mapitem already exists at this location. Skip it.
               break;
         }
         name = "Wife";
         miName = name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         mi = new MapItem(miName, 0.8, name, t, 4, 8, 4);
         gi.Townspeople.Add(mi);
         gi.Stacks.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "Create_Townspeople(): miName=" + miName + " t=" + t.ToString() + " stacks=" + gi.Stacks.ToString());
         //------------------------------------
         return true;
      }

      //--------------------------------------------------------------------
      void MouseLeftButtonDownDeleteTerritory(object sender, MouseButtonEventArgs e)
      {
         if( null == myCanvasMain )
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         //--------------------------------------------
         Ellipse? selectedEllipse = null;
         foreach (UIElement ui in myCanvasMain.Children)
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
            foreach (UIElement ui in myCanvasMain.Children)
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
         myCanvasMain.Children.Remove(selectedEllipse);
      }
      void MouseLeftButtonDownCreateTerritory(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         TerritoryCreateDialog dialog = new TerritoryCreateDialog(myCanvasMain); // Get the name from user
         dialog.myTextBoxName.Focus();
         if (true == dialog.ShowDialog())
         {
            Territory territory = new Territory() { CenterPoint = new MapPoint(p.X, p.Y) };
            territory.Name = dialog.myTextBoxName.Text;
            territory.Subname = dialog.myTextBoxSubname.Text;
            territory.CanvasName = TerritoryCreateDialog.theLastEnteredCanvasName;
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSetCenterPoint(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         System.Diagnostics.Debug.WriteLine("TerritoryUnitTest.MouseDown(): p=" + p.ToString());
         foreach (UIElement ui in myCanvasMain.Children)
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
         foreach (UIElement ui in myCanvasMain.Children)
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
               System.Windows.Point newPoint = e.GetPosition(myCanvasMain);
               Canvas.SetTop(myEllipseSelected, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myEllipseSelected, newPoint.X - theEllipseOffset);
            }
         }
      }
      void MouseUp(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point newPoint = new Point();
         if( true == myIsDraggingMapItem)
            newPoint = e.GetPosition(myCanvasMain);
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
               ITerritory? t = Territories.theTerritories.Find(ellipse.Name);
               if( null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): t=null for eName=" + ellipse.Name);
                  return;
               }
               t.CenterPoint.X = newPoint.X;
               t.CenterPoint.Y = newPoint.Y;
               this.myEllipseSelected = null;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myEllipseSelected=null");
         }
      }
      void MouseDownEllipseVerify(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): myCanvasTank=null");
            return;
         }
         foreach (UIElement ui in myCanvasMain.Children)
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
                     t.Name = dialog.myTextBoxName.Text;
                     t.Subname = dialog.myTextBoxSubname.Text;
                     return;
                  }
               }
            }
         }
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvasMain=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         bool isEndMatch = false;
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  ITerritory? selectedTerritory = Territories.theTerritories.Find(selectedEllipse.Name);  // Find the corresponding Territory that user selected
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
                     System.Diagnostics.Debug.WriteLine("Anchoring selectedTerritory=" + selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Adjacents.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.ToString() == myAnchorTerritory.ToString())
                  {
                     StringBuilder sb = new StringBuilder("Saving");
                     sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving selectedTerritory=" + selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     isEndMatch = true;
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  else
                  {
                     selectedEllipse.Fill = aSolidColorBrush2; // If the matching territory is not the anchor territory, change its color.
                     string? tName = selectedTerritory.ToString();
                     if (null == tName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): tName=null for selectedTerritory=" + selectedTerritory);
                        return;
                     }
                     bool isMatch = false;
                     foreach (string s in myAnchorTerritory.Adjacents)
                     {
                        if (s == tName)
                        {
                           isMatch = true; break;
                        }
                     }
                     if (false == isMatch)
                     {
                        System.Diagnostics.Debug.WriteLine("Adding selectedTerritory=" + selectedTerritory.ToString());
                        myAnchorTerritory.Adjacents.Add(tName);
                     }
                  }
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvasMain.Children)
         //----------------------------------------------------------
         // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
         // to the adjacents ellipse. Clear the data so another one can be selected.
         if (true == isEndMatch)
         {
            foreach (UIElement ui1 in myCanvasMain.Children)
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
                     ellipse1.Fill = mySolidColorBrushWaterBlue;
                  else
                     ellipse1.Fill = aSolidColorBrush1;
               }
            }
         }
      }
      void MouseLeftButtonDownSetObservations(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvasMain=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         bool isEndMatch = false;
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  ITerritory? selectedTerritory = Territories.theTerritories.Find(selectedEllipse.Name);  // Find the corresponding Territory that user selected
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
                     System.Diagnostics.Debug.WriteLine("Anchoring selectedTerritory=" + selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     selectedEllipse.Fill = aSolidColorBrush3;
                     myAnchorTerritory.Observations.Clear();
                     return;
                  }
                  if (selectedTerritory.ToString() == myAnchorTerritory.ToString())
                  {
                     bool isBuilding = selectedTerritory.IsBuilding();
                     double prob = TableMgr.GetObservationChance(0, isBuilding);
                     if (TableMgr.FN_ERROR == prob)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetObservations(): TableMgr.GetObservationChance() returned error for selectedTerritory=" + selectedTerritory);
                        return;
                     }
                     string? tName = myAnchorTerritory.ToString();
                     if( null == tName )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetObservations(): TableMgr.GetObservationChance() tName=null myAnchorTerritory=" + selectedTerritory);
                        return;
                     }
                     myAnchorTerritory.Observations[tName] = prob; // The anchor territory has entry for itself in observations
                     System.Diagnostics.Debug.WriteLine("Adding observation tName=" + tName + " prob=" + prob.ToString());
                     StringBuilder sb = new StringBuilder("Saving");
                     sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" ");
                     System.Diagnostics.Debug.WriteLine("Saving selectedTerritory=" + selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     isEndMatch = true;
                  } 
                  else
                  {
                     selectedEllipse.Fill = aSolidColorBrush2; // If the matching territory is not the anchor territory, change its color.
                     string? tName = selectedTerritory.ToString();
                     if (null == tName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetObservations(): tName=null for selectedTerritory=" + selectedTerritory);
                        return;
                     }
                     bool isMatch = false;
                     foreach (var kvp in myAnchorTerritory.Observations)
                     {
                        if (kvp.Key == tName)
                        {
                           isMatch = true; 
                           break;
                        }
                     }
                     if (false == isMatch)
                     {
                        System.Diagnostics.Debug.WriteLine("Trying tName=" + tName);
                        IMapPath? mapPath = Territory.GetShortestRandomPath(Territories.theTerritories, myAnchorTerritory, selectedTerritory, 3);
                        if( null == mapPath )
                        {
                           Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetObservations(): mapPath=null from myAnchorTerritory=" + myAnchorTerritory.ToString() + " to tName=" + tName);
                           return;
                        }
                        bool isBuilding = selectedTerritory.IsBuilding();
                        int range = mapPath.Territories.Count;
                        double prob = TableMgr.GetObservationChance(range, isBuilding);
                        if( TableMgr.FN_ERROR == prob )
                        {
                           Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetObservations(): TableMgr.GetObservationChance() returned error for tName=" + tName);
                           return;
                        }
                        myAnchorTerritory.Observations[tName] = prob;
                        System.Diagnostics.Debug.WriteLine("Adding observation tName=" + tName + " prob=" + prob.ToString());
                        break;
                     }
                  }
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvasMain.Children)
         //----------------------------------------------------------
         // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
         // to the adjacents ellipse. Clear the data so another one can be selected.
         if (true == isEndMatch)
         {
            foreach (UIElement ui1 in myCanvasMain.Children)
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
                     ellipse1.Fill = mySolidColorBrushWaterBlue;
                  else
                     ellipse1.Fill = aSolidColorBrush1;
               }
            }
         }
      }
   }
}

