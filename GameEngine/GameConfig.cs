public enum GameType
{
    PlayerVsPlayer,
    PlayerVsComp
}

public class GameConfig
{
    private readonly int m_BoardSize;
    private readonly GameType m_GameType;

    public GameConfig(int i_BoardSize, GameType i_GameType)
    {
        m_BoardSize = i_BoardSize;
        m_GameType = i_GameType;
    }

    public int BoardSize
    {
        get
        {
            return m_BoardSize;
        }
    }

    public GameType GameType
    {
        get
        {
            return m_GameType;
        }
    }
}