using System;

[Serializable]
public class Level_v1
{
    public string levelCode, creatorUsername, description, levelName;

    public Level_v1(string levelCode, string creatorUsername, string description, string levelName)
    {
        this.levelCode = levelCode;
        this.creatorUsername = creatorUsername;
        this.description = description;
        this.levelName = levelName;
    }
}
