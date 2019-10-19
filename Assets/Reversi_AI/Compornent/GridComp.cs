using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 GridNum;

    //0＝空白　1＝黒　2＝白 3=設置可能
    public int GridState;

    //このグリッドの評価値を設定します、高ければ高いほど優先して取るべき駒です。
    public int Priority;

    //このグリッドに設置することが確定した場合にTrueになります。
    //毎フレーム各グリッドのチェック終了後にFalseに変更されます。
    public bool PutFlag;
}
