using System.Text;
using System.Windows;
using System.Xml;
using MessageBox=System.Windows.MessageBox;

namespace PleasantvilleGame
{ 
   public class GameInstance : IGameInstance
   {
      static public Logger Logger = new Logger();
      public bool CtorError { get; } = false;
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //------------------------------------------------
      public IGameCommands GameCommands { set; get; } = new GameCommands();
      public Options Options { get; set; } = new Options();
      public GameStatistics Statistics { get; set; } = new GameStatistics();
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //------------------------------------------------
      public bool IsMultipleSelectForDieResult { set; get; } = false;
      public bool IsGridActive { set; get; } = false;
      public IUndo? UndoCmd { set; get; } = null;
      //------------------------------------------------
      public Guid GameGuid { get; set; } = Guid.NewGuid();
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      //------------------------------------------------
      public int Day { get; set; } = 0;
      public int GameTurn { get; set; } = 0; 
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public string EndGameReason { set; get; } = "";
      //----------------------------------------------
      public ITerritories ZebulonTerritories { set; get; } = new Territories();
      public IMapItems Persons { set; get; } = new MapItems();
      public IMapItems PersonsStunned { set; get; } = new MapItems();
      public IMapItems PersonsKnockedOut { set; get; } = new MapItems();
      public IMapItemCombat? MapItemCombat { set; get; } = null;
      public IMapItemTakeover? Takeover { set; get; } = null;
      public IMapItemMove? PreviousMapItemMove { set; get; } = null;
      //---------------------------------------------------------------
      public string PlayerTurn { set; get; } = "Alien";
      public string NextAction { set; get; } = "";
      public int InfluenceCountTotal { set; get; } = 0;
      public int InfluenceCountTownspeople { set; get; } = 0;
      public int InfluenceCountAlienUnknown { set; get; } = 0;
      public int InfluenceCountAlienKnown { set; get; } = 0;
      public int NumIterogationsThisTurn { set; get; } = 0;
      public bool IsAlienStarted { set; get; } = false;
      public bool IsControlledStarted { set; get; } = false;
      public bool IsAlienDisplayedRandomMovement { set; get; } = false;
      public bool IsControlledDisplayedRandomMovement { set; get; } = false;
      public bool IsAlienAckedRandomMovement { set; get; } = false;
      public bool IsControlledAckedRandomMovement { set; get; } = false;
      public bool IsAlienInitiatedCombat { set; get; } = false;
      public bool IsControlledInitiatedCombat { set; get; } = false;
      public bool IsAlienCombatCompleted { set; get; } = false;
      public bool IsControlledCombatCompleted { set; get; } = false;
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {
         try
         {
            GameLoadMgr gameLoadMgr = new GameLoadMgr();
            string filename = ConfigFileReader.theConfigDirectory + Territories.FILENAME;
            XmlTextReader? reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): reader=null");
               return;
            }
            if (false == gameLoadMgr.ReadXmlTerritories(reader, Territories.theTerritories))
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned false for filename=" + filename);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() exception=\n" + e.ToString());
            return;
         }
         try
         {
            GameLoadMgr gameLoadMgr = new GameLoadMgr();
            string filename = ConfigFileReader.theConfigDirectory + "People.xml";
            XmlTextReader? reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): reader=null");
               return;
            }
            if (false == gameLoadMgr.ReadXmlTownspeople(reader, MapItems.theMapItems))
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadXmlTownspeople() returned false for filename=" + filename);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadXmlTownspeople() exception=\n" + e.ToString());
            return;
         }
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         Options = newGameOptions;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(GamePhase.ToString());
         sb.Append("]");
         return sb.ToString();
      }
      //---------------------------------------------------------------
      public bool AddUnknownAlien(IMapItem newAlien)
      {
         StringBuilder sb = new StringBuilder("AddUnknownAlien():"); 
         IMapItem? mi = this.Stacks.FindMapItem(newAlien.Name);
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): AddUnknownAlien() mi=null for name=" + newAlien.Name);
            return false;
         }
         if (false == mi.IsAlienUnknown)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb.Append(mi.Name); sb.Append(" ---- from TP "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }

               InfluenceCountAlienUnknown += mi.Influence;
               sb.Append(mi.Name); 
               sb.Append(" ++++ to Unknown "); 
               sb.Append(mi.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }
            mi.IsAlienUnknown = true;
            mi.IsWary = false;
            mi.IsControlled = false;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddUnknownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddKnownAlien(IMapItem newAlien)
      {
         StringBuilder sb = new StringBuilder("AddKnownAlien():"); ;
         IMapItem? mi = this.Stacks.FindMapItem(newAlien.Name);
         if (null == mi)
         {
            Console.WriteLine("GameInstance::AddKnownAlien() - ERROR mi = null ");
            return false;
         }
         if (false == mi.IsAlienKnown)  // Do not add if already known alien
         {
            // Stunned, tied-up, and surrendered townspeople can be taken over. 
            // However, influence probably not going to be added to Alien total until  condition expires.
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsSurrendered) && (false == mi.IsKilled))
            {
               if (true == mi.IsControlled)
               {
                  InfluenceCountTownspeople -= mi.Influence;
                  sb.Append(mi.Name); 
                  sb.Append(" ---- from TP "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               if (true == mi.IsAlienUnknown) // Determine if already an alien.  If already an alien, need to remove from that influence 
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  sb.Append(mi.Name); 
                  sb.Append(" ---- from Unknown "); 
                  sb.Append(mi.Influence.ToString());
                  sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
                  sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
                  sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
               }
               InfluenceCountAlienKnown += mi.Influence;
               sb =  sb.Append(mi.Name); 
               sb.Append(" ++++ to Known "); 
               sb.Append(mi.Influence.ToString());
               sb.Append(" T="); sb.Append(this.InfluenceCountTotal.ToString());
               sb.Append(" Known="); sb.Append(this.InfluenceCountAlienKnown.ToString());
               sb.Append(" UnKnown="); sb.Append(this.InfluenceCountAlienUnknown.ToString());
               sb.Append(" TP="); sb.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb.ToString());
            }

            mi.IsWary = false;
            mi.IsControlled = false;
            mi.IsAlienUnknown = false;
            mi.IsAlienKnown = true;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            MessageBox.Show("AddKnownAlien() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
      public bool AddTownperson(IMapItem mi)
      {
         //-----------------------------------------
         if (false == mi.IsControlled)
         {
            if ((false == mi.IsTiedUp) && (true == mi.IsConscious) && (false == mi.IsStunned) && (false == mi.IsKilled))
            {
               if ((true == mi.IsAlienKnown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienKnown -= mi.Influence;
                  StringBuilder sb0 = new StringBuilder("AddTownperson():"); sb0.Append(mi.Name); sb0.Append(" ---- from known "); sb0.Append(mi.Influence.ToString());
                  sb0.Append(" T="); sb0.Append(this.InfluenceCountTotal.ToString());
                  sb0.Append(" Known="); sb0.Append(this.InfluenceCountAlienKnown.ToString());
                  sb0.Append(" UnKnown="); sb0.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb0.Append(" TP="); sb0.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb0.ToString());
               }

               if ((true == mi.IsAlienUnknown) && (false == mi.IsSurrendered))
               {
                  InfluenceCountAlienUnknown -= mi.Influence;
                  StringBuilder sb1 = new StringBuilder("AddTownperson():"); sb1.Append(mi.Name); sb1.Append(" ---- from unknown "); sb1.Append(mi.Influence.ToString());
                  sb1.Append(" T="); sb1.Append(this.InfluenceCountTotal.ToString());
                  sb1.Append(" Known="); sb1.Append(this.InfluenceCountAlienKnown.ToString());
                  sb1.Append(" UnKnown="); sb1.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb1.Append(" TP="); sb1.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb1.ToString());
               }

               if (true == mi.IsSurrendered)
               {
                  InfluenceCountTotal += mi.Influence; // A surrendered alien that converts gets added back to total influence
                  StringBuilder sb2 = new StringBuilder("AddTownperson():"); sb2.Append(mi.Name); sb2.Append(" ++++ to total "); sb2.Append(mi.Influence.ToString());
                  sb2.Append(" T="); sb2.Append(this.InfluenceCountTotal.ToString());
                  sb2.Append(" Known="); sb2.Append(this.InfluenceCountAlienKnown.ToString());
                  sb2.Append(" UnKnown="); sb2.Append(this.InfluenceCountAlienUnknown.ToString());
                  sb2.Append(" TP="); sb2.Append(this.InfluenceCountTownspeople.ToString());
                  Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb2.ToString());
               }

               InfluenceCountTownspeople += mi.Influence;
               StringBuilder sb3 = new StringBuilder("AddTownperson():"); sb3.Append(mi.Name); sb3.Append(" ++++ to TP "); sb3.Append(mi.Influence.ToString());
               sb3.Append(" T="); sb3.Append(this.InfluenceCountTotal.ToString());
               sb3.Append(" Known="); sb3.Append(this.InfluenceCountAlienKnown.ToString());
               sb3.Append(" UnKnown="); sb3.Append(this.InfluenceCountAlienUnknown.ToString());
               sb3.Append(" TP="); sb3.Append(this.InfluenceCountTownspeople.ToString());
               Logger.Log(LogEnum.LE_INFLUENCE_CHANGE, sb3.ToString());
            }
            mi.IsAlienKnown = false;
            mi.IsControlled = true;
            mi.IsWary = false;
            mi.IsSurrendered = false;
            mi.IsAlienUnknown = false;
         }
         if (false == GameStateChecker.IsInfluenceCheck(this))
         {
            Logger.Log(LogEnum.LE_ERROR, "AddTownperson() ERROR - Influence failure for " + mi.Name);
            return false;
         }
         return true;
      }
   }
}

