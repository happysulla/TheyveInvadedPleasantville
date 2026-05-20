using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PleasantvilleGame
{
   public partial class HostGameDialog : Window
   {
      public string SessionName { get; private set; } = "Pleasantville Session";
      public int Port { get; private set; } = 50051;

      public HostGameDialog()
      {
         InitializeComponent();
      }

      private void Ok_Click(object sender, RoutedEventArgs e)
      {
         string sessionName = SessionNameTextBox.Text.Trim();
         if (string.IsNullOrWhiteSpace(sessionName))
         {
            MessageBox.Show(this, "Enter a session name.", "Host Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }

         if (!int.TryParse(PortTextBox.Text.Trim(), out int port) || port < 1024 || port > 65535)
         {
            MessageBox.Show(this, "Enter a valid TCP port between 1024 and 65535.", "Host Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }

         SessionName = sessionName;
         Port = port;
         DialogResult = true;
      }
   }
}
