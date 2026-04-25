using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RadioButton = System.Windows.Controls.RadioButton;

namespace PleasantvilleGame
{
   public partial class TerritoryCreateDialog : Window
   {
      public static string theLastEnteredName = "";
      public static string theLastEnteredSubname = "";
      public static string theLastEnteredCanvasName = "Main";
      private Canvas? myCanvasMain = null;
      public TerritoryCreateDialog(Canvas? c)
      {
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): c=null");
            return;
         }
         myCanvasMain = c;
         //-----------------------------------------
         InitializeComponent();
         UpdateView();
         myRadioButtonMain.Checked += RadioButtonParent_Checked;
         myRadioButtonHelper.Checked += RadioButtonParent_Checked;
         myTextBoxName.Text = theLastEnteredName;
         myTextBoxSubname.Text = theLastEnteredSubname;
      }
      //-----------------------------------------------------
      void UpdateView()
      {

      }
      //---------------CONTROLLER FUNCTIONS------------------
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         theLastEnteredName = myTextBoxName.Text;
         theLastEnteredSubname = myTextBoxSubname.Text;
         this.DialogResult = true;
      }
      private void RadioButtonParent_Checked(object sender, RoutedEventArgs e)
      {
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
         }
         else
         {
            if (true == radioButton.Content is String)
            {
               String output = (String)radioButton.Content;
               if (null != output)
                  theLastEnteredCanvasName = output;
            }
         }
         UpdateView();
         e.Handled = true;
      }
   }
}
