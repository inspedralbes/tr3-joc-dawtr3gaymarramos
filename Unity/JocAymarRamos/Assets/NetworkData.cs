using System;

[Serializable]
public class SincroEnemic {
    public string room { get; set; }
    public string enemyName { get; set; }
    public float x { get; set; }
    public float y { get; set; }
}

[Serializable]
public class PlayerStateData {
    public string room { get; set; }
    public bool isHost { get; set; }
}

[Serializable]
public class PlayerDamageData {
    public string room { get; set; }
    public bool isHost { get; set; }
    public int damage { get; set; }
}
