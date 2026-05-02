
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Point = System.Windows.Point;

namespace PleasantvilleGame
{
   public partial class TableDialog : Window
   {
      private static double HEADER_PLUS_SCROLL_HEIGHT = 50;
      public static double SCROLL_WIDTH = 20;
      private static SolidColorBrush theBrushOrange = new SolidColorBrush(Colors.Orange);
      private static SolidColorBrush theBrushBlue = new SolidColorBrush(Colors.LightBlue);
      private static SolidColorBrush theBrushGreen = new SolidColorBrush(Colors.LightGreen);
      private static SolidColorBrush theBrushTan = new SolidColorBrush(Colors.AntiqueWhite);
      public bool CtorError { get; } = false;
      private string myKey = "";
      public string Key { get => myKey; }
      private FlowDocument? myFlowDocumentContent = null;
      public FlowDocument? FlowDocumentContent { get => myFlowDocumentContent; }
      public TableDialog(string key, StringReader sr)
      {
         InitializeComponent();
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myFlowDocumentContent = (FlowDocument)XamlReader.Load(xr);
            myFlowDocumentScrollViewer.Document = myFlowDocumentContent;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, " e=" + e.ToString() + " sr.content=\n" + sr.ToString());
            CtorError = true;
            return;
         }
      }
      //-------------------------------------------------------------------------
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void TableDialog_Loaded(object sender, RoutedEventArgs e)
      {
         double heightOfConcern = 0.0;
         double widthOfConcern = 0.0;
         this.myFlowDocumentScrollViewer.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         switch (Key)
         {
            case "Alien Loss":
               this.Title = "Alien Loss";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Combat Results":
               this.Title = "Combat Results";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Implant Removal":
               this.Title = "Implant Removal";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Influence Results":
               this.Title = "Implant Removal";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Conversation Modifiers":
               this.Title = "Modifiers to Conversation";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Influence Modifiers":
               this.Title = "Modifiers to Influence Attempts";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Target Building":
               this.Title = "Target Building";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Town Loss":
               this.Title = "Town Loss";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Town Player Starting":
               this.Title = "Town Player Starting";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Townspeople":
               this.Title = "Townspeople";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TableDialog_Loaded(): reached default key=" + Key);
               break;
         }
         myFlowDocumentScrollViewer.MinHeight = myFlowDocumentScrollViewer.Height = heightOfConcern;
         myFlowDocumentScrollViewer.MinWidth = myFlowDocumentScrollViewer.Width = widthOfConcern;
         myScrollViewer.Height = heightOfConcern;
         myScrollViewer.Width = widthOfConcern;
         this.Height = Math.Min((heightOfConcern + HEADER_PLUS_SCROLL_HEIGHT) * Utilities.ZoomCanvas, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.Width = Math.Min((widthOfConcern + SCROLL_WIDTH) * Utilities.ZoomCanvas, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.MinHeight = heightOfConcern * 0.5;
         this.MinWidth = widthOfConcern * 0.5;
         this.MaxHeight = Math.Min(3 * heightOfConcern, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.MaxWidth = Math.Min(3 * widthOfConcern, System.Windows.SystemParameters.PrimaryScreenWidth);
      }
      private void TableDialog_ContentRendered(object sender, EventArgs e)
      {
      }
      private void TableDialog_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         myScrollViewer.Height = this.ActualHeight - HEADER_PLUS_SCROLL_HEIGHT;
         myScrollViewer.Width = this.ActualWidth - SCROLL_WIDTH;
      }
   }
}
