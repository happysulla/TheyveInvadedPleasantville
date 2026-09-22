namespace PleasantvilleGame.Networking
{
   public static class MultiplayerSnapshotFactory
   {
      public static VisibleGameStateDataTranferObject CreateVisibleState(IGameInstance gameInstance, MultiplayerRole role)
      {
         VisibleGameStateDataTranferObject dto = new VisibleGameStateDataTranferObject
         {
            GameGuid = gameInstance.GameGuid.ToString(),
            EventActive = gameInstance.EventActive,
            EventDisplayed = gameInstance.EventDisplayed,
            GamePhase = gameInstance.GamePhase.ToString(),
            GameTurn = gameInstance.GameTurn,
            Day = gameInstance.Day,
         };
         return dto;
      }
   }
}
