namespace Models
{
    public class clsRoom
    {
        #region Propiedades
        public int PlayerCount { get { return Players.Count; } }

        public String Name { get; set; }

        public String CurrentTurn { get; set; }

        public Dictionary<string, string> Players { get; set; } = new Dictionary<string, string>(); // ConnectionId -> PlayerName
        #endregion

        #region Constructores
        public clsRoom() { }

        public clsRoom(string name, string ownerConnectionId, string ownerName)
        {
            Name = name;
            Players = new Dictionary<string, string> { { ownerConnectionId, ownerName } };
        }
        #endregion
    }
}
