using System;

[Serializable]
public class SincroEnemic {
    public string room;
    public string enemyName;
    public float x;
    public float y;
}

[Serializable]
public class PlayerStateData {
    public string room;
    public bool isHost;
}

[Serializable]
public class PlayerDamageData {
    public string room;
    public bool isHost; // Para saber qué jugador ha sido golpeado
    public int damage;
}
