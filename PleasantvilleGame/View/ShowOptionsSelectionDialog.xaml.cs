using System.Windows;
using System.Windows.Controls;
using CheckBox = System.Windows.Controls.CheckBox;
using RadioButton = System.Windows.Controls.RadioButton;

namespace PleasantvilleGame
{
   public partial class ShowOptionsSelectionDialog : Window
   {
      public bool CtorError { get; }
      public IGameInstance? myGameInstance = null;
      private Options myOptions { get; set; } = new Options();
      public Options NewOptions { get => myOptions; }
      //---------------------------------------------
      public ShowOptionsSelectionDialog(Options options)
      {
         Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "OptionSelectionDialog(): " + options.ToString());
         myOptions = new Options();
         foreach( Option o in options )
         {
            Option option = new Option(o.Name, o.IsEnabled);
            myOptions.Add(option);
         }
         InitializeComponent();
         //-----------------------------
         myCheckBoxAutoSetupTown.ToolTip = "Automatically setup town & alien starting players. Create townspeople and place on map.";
         //-----------------------------
         if (false == UpdateDisplay(myOptions))
         {
            Logger.Log(LogEnum.LE_ERROR, "OptionSelectionDialog(): UpdateDisplay() returned false");
            CtorError = true;
         }
      }
      //----------------------------------
      private bool UpdateDisplay(Options options)
      {

         return true;
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void StackPanelAutoSetup_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option;
         switch (cb.Name)
         {
            case "myCheckBoxAutoSetupTown": option = myOptions.Find("AutoSetupTown"); option.IsEnabled = !option.IsEnabled; break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): reached default name=" + cb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
