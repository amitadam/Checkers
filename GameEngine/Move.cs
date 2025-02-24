public struct Move
{
    private Coord m_Source;
    private Coord m_Destination;
    private readonly bool? m_IsJump;

    public bool IsJump
    {
        get
        {
            return m_IsJump ?? false;
        }
    }

    public Coord Source
    {
        get
        {
            return m_Source;
        }
    }

    public Coord Destination
    {
        get
        {
            return m_Destination;
        }
    }

    public Move(Coord i_Source, Coord i_Destination, bool? i_IsJump)
    {
        m_IsJump = i_IsJump;
        m_Source = i_Source;
        m_Destination = i_Destination;
    }

    public static bool CompareMoves(Move move1, Move move2)
    {
        bool result = true;

        if (move1.Source.X != move2.Source.X)
        {
            result = false;
        }

        if (move1.Source.Y != move2.Source.Y)
        {
            result = false;
        }

        if (move1.Destination.X != move2.Destination.X)
        {
            result = false;
        }

        if (move1.Destination.Y != move2.Destination.Y)
        {
            result = false;
        }

        return result;
    }
}