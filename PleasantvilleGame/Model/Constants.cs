
namespace PleasantvilleGame
{
    public class Constants
    {
        public readonly static int STD_WAIT = 10;
        public readonly static string[,] townsPersonTable = new string[5, 6] 
        { 
         {"Mayor","Sheriff","Plumber","Lawyer","Hotel Owner","Judge"},
         {"Repair Shop Owner","Maitre D","Doctor","Teacher","Minister","Bank President"},
         {"Vet","Bar And Grill Owner","Teller","Station Attendant","Checkout Girl","PaperBoy"},
         {"Maid","Bank Guard","Town Drunk","Tailor","BarTender","War Veteran"},
         {"Waitress","Supermarket Manager","Fire Chief","Wife","Welder", "Deputy"},
        };

        public readonly static string[,] targetBuildingTable = new string[5, 6] 
        { 
         {"Tavern","Vet","Clothing Store","General Store","Gas Pumps","Supermarket"},
         {"School","Bank","Doc","VFW","Bar And Grill","Machine Shop"},
         {"Sheriff","Town Hall","Hotel","Church","Graveyard","Stock Pen"},
         {"Train Station","House","House","House","House","House"},
         {"House","House","House","House","Lawyer","House"},
        };

        public readonly static string[] townPlayerStartingTable = new string [6]
        {
          "Bank President",
          "Doctor",
          "Mayor",
          "Minister",
          "Teacher",
          "Sheriff"
        };

        public readonly static string[,] buildingSizes = new string[21, 2] 
        { 
         {"Tavern","3"},
         {"Vet","2"},
         {"Clothing Store","2"},
         {"General Store","4"},
         {"Gas Pumps","1"},
         {"SuperMarket","5"},
         {"School","4"},
         {"Bank","4"},
         {"Doc","2"},
         {"VFW","1"},
         {"Bar And Grill","2"},
         {"Machine Shop","4"},
         {"Sheriff","4"},
         {"Town Hall","3"},
         {"Hotel","5"},
         {"Church","5"},
         {"Graveyard","1"},
         {"Stock Pen","1"},
         {"Train Station","2"},
         {"House","10"},
         {"Lawyer","1"}
        };
    }
}