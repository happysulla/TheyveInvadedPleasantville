using System.Windows;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace PleasantvilleGame
{
   public partial class ReturnToActiveEventDialog : Window
   {
      public ReturnToActiveEventDialog()
      {
         InitializeComponent();
         Image imageTutorial = new Image() { Name = "Tutorial0", Width = 370, Height = 70, HorizontalAlignment=System.Windows.HorizontalAlignment.Center, Source = MapItem.theMapImages.GetBitmapImage("Tutorial0") };
         myGrid.Children.Add(imageTutorial);
         Grid.SetRow(imageTutorial, 1);
         Grid.SetColumn(imageTutorial, 0);
      }
      private void ButtonGoto_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
         Close();
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
