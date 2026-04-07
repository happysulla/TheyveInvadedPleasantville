using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PleasantvilleGame
{
   public partial class TerritoryCreateDialog : Window
   {
      public static string theTypeChecked = "A";
      public static string theCardChecked = "1";
      public static string theParentChecked = "Main";
      private Canvas? myCanvasMain = null;
      private Canvas? myCanvasTank = null;
      public TerritoryCreateDialog(Canvas? main)
      {
         if (null == main)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): main=null");
            return;
         }
         myCanvasMain = main;
         //-----------------------------------------
         InitializeComponent();
         UpdateView();
      }
      //-----------------------------------------------------
      void UpdateView()
      {
    
      }
      //---------------CONTROLLER FUNCTIONS------------------
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
   }
}
