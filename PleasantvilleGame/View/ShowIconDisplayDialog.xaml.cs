using System;
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
using System.Windows.Shapes;
using Image=System.Windows.Controls.Image;

namespace PleasantvilleGame
{
   public partial class ShowIconDisplayDialog : Window
   {
      public ShowIconDisplayDialog()
      {
         InitializeComponent();
         Thickness thickness = new Thickness(0, 8, 0, 0);
         int row = -1;
         int col = 0;
         Image img = new Image() { Height = 60, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Alien"), Margin = thickness};
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 60, Width = 60, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Implant") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() {Height=50, Width= 50, IsEnabled=false, Source = MapItem.theMapImages.GetBitmapImage("OKIA") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OKnockedOut") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OSkeptical") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         //--------------------------------------------
         row = -1;
         col = 2;
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OStunned") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OSurrendered") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OTiedUp") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("OWary") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 50, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DeadPerson") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
