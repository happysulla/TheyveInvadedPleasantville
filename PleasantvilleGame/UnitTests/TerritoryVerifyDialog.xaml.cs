using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using RadioButton=System.Windows.Controls.RadioButton;

namespace PleasantvilleGame
{
   public partial class TerritoryVerifyDialog : Window
   {
      public static string theLastEnteredName = "";
      public static string theLastEnteredSubname = "";
      public static string theLastEnteredCanvasName = "Main";
      public TerritoryVerifyDialog(ITerritory t)
      {
         InitializeComponent();
         myTextBoxName.Text = t.Name;
         myTextBoxSubname.Text = t.Subname;

      }
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
   }
}
