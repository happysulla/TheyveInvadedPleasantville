
namespace PleasantvilleGame
{
    public partial class App : System.Windows.Application
    {
        public App()
        {
            // The initial multiplayer scaffold uses h2c for local/LAN development.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }
    }

}
