namespace PleasantvilleGame
{
    public interface IView
    {
        void UpdateView(ref IGameInstance gi, GameAction action);
    }
}
