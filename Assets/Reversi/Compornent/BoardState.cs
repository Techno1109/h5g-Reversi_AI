using Unity.Entities;

public struct BoardState : IComponentData
{
    //このコンポーネントで盤面の状態を格納できるようにします
    public bool EmitBoard;

    public bool InitBoard;

    //盤面上に設置されている白駒の数
    public int WhiteCount;

    //盤面上に設置されている黒駒の数
    public int BlackCount;
}
