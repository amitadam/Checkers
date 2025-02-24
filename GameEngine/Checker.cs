public struct Checker
{
    private readonly eColors m_Color;
    private bool m_IsKing;

    public Checker(eColors Color)
    {
        m_Color = Color;
        m_IsKing = false;
    }

    public eColors Color
    {
        get
        {
            return m_Color;
        }
    }

    public void SetAsKing()
    {
        m_IsKing = true;
    }

    public bool IsKing()
    {
        return m_IsKing;
    }
}