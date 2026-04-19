[System.Serializable]
public class GameResponse {
    public string status;
    public GameInfo data;
}

[System.Serializable]
public class GameInfo {
    public string roomCode;
    public string hostId;
    public string _id;
}