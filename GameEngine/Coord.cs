public struct Coord
{
    private int m_X;
    private int m_Y;

    public Coord(int i_X, int i_Y)
    {
        m_X = i_X;
        m_Y = i_Y;
    }

    public int X
    {
        get { return m_X; }
    }

    public int Y
    {
        get { return m_Y; }
    }
}