using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PleasantvilleGame
{
   public partial class JoinGameDialog : Window
   {
      public string ServerAddress { get; private set; } = "http://127.0.0.1:50051";
      public string SessionId { get; private set; } = string.Empty;
      public string JoinCode { get; private set; } = string.Empty;
      //-------------------------------------------
      public JoinGameDialog()
      {
         InitializeComponent();
      }
      //-------------------------------------------
      private void Ok_Click(object sender, RoutedEventArgs e)
      {
         string serverAddress = ServerAddressTextBox.Text.Trim();
         string sessionId = SessionIdTextBox.Text.Trim();
         string joinCode = JoinCodeTextBox.Text.Trim();
         //-------------------------------------------
         if (true == string.IsNullOrWhiteSpace(serverAddress))
         {
            MessageBox.Show(this, "Enter the host address.", "Join Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }
         if (true == string.IsNullOrWhiteSpace(sessionId))
         {
            MessageBox.Show(this, "Enter the session id from the host.", "Join Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }
         if (true == string.IsNullOrWhiteSpace(joinCode))
         {
            MessageBox.Show(this, "Enter the join code from the host.", "Join Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }
         //-------------------------------------------
         ServerAddress = serverAddress;
         SessionId = sessionId;
         JoinCode = joinCode;
         DialogResult = true;
      }
   }
}
