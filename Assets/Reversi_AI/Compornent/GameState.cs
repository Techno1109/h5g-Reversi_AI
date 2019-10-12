using Unity.Entities;
using Unity.Tiny.Core2D;

public struct GameState : IComponentData
{
    //ƒQ[ƒ€‚ªis’†‚È‚Ì‚©AI—¹‚µ‚Ä‚¢‚é‚Ì‚©Ši”[‚µ‚Ü‚·
    public bool IsActive;

    //  ‚Ç‚¿‚ç‚Ìƒ^[ƒ“‚È‚Ì‚©‚ğŠi”[‚µ‚Ü‚·
    public int NowTurn;

    // Œˆ’…‚ª‚Â‚¢‚½‚©‚Ç‚¤‚©Ši”[‚µ‚Ü‚·
    public bool GameEnd;

    //ŸÒ‚ğŠi”[‚µ‚Ü‚· 0=–¢Šm’è@1=• 2=”’
    public int WinnetNum;

    //AI‚ª‚Ç‚¿‚ç‚ÌF‚È‚Ì‚©Ši”[‚µ‚Ü‚· 0=–¢Šm’è@1=• 2=”’
    public int AIColor;
}
