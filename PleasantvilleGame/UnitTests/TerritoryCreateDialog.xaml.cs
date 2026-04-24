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
      public static string theTypeChecked = "A";
      public static string theParentChecked = "Main";
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
         foreach (UIElement ui in myStackPanelTerritory.Children)
         {
            if (ui is RadioButton rb)
               rb.Checked += RadioButtonType_Checked;
         }
         myRadioButtonMain.Checked += RadioButtonParent_Checked;
         myRadioButtonHelper.Checked += RadioButtonParent_Checked;
      }
      //-----------------------------------------------------
      void UpdateView()
      {
         myRadioButtonMain.IsChecked = true;
         foreach (UIElement ui in myStackPanelTerritory.Children)
         {
            if (ui is RadioButton rb)
            {
               if (rb.Content is string)
               {
                  rb.Visibility = Visibility.Visible;
                  string s = (string)rb.Content;
                  if (theTypeChecked == s)
                     rb.IsChecked = true;
               }
            }
         }
      }
      //---------------CONTROLLER FUNCTIONS------------------
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
      private void RadioButtonType_Checked(object sender, RoutedEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): myCanvasMain=null");
            return;
         }
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
            return;
         }
         if (null == radioButton.Content)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
            return;
         }
         string content = (string)radioButton.Content;
         theTypeChecked = content;
         switch (content)
         {
            case "A":
            case "B":
            case "C":
            case "D":
            case "E":
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): reached default content=" + content);
               break;
         }
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
                  theParentChecked = output;
            }
         }
         UpdateView();
         e.Handled = true;
      }
   }
}
