using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(BoardMan))]
public class PlayerInput : ComponentSystem
{
    EntityQueryDesc GirdEntityDesc;
    EntityQuery GridEntity;

    EntityQueryDesc CanvasDesc;
    EntityQuery CanvasEntity;

    protected override void OnCreate()
    {
        /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(PointerInteraction), typeof(Button), typeof(GridComp) },
        };

        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        GridEntity = GetEntityQuery(GirdEntityDesc);
    }

    protected override void OnUpdate()
    {
        //そもそもシングルトンが生成されているかどうかチェック
        if (HasSingleton<GameState>() == false)
        {
            return;
        }

        if (HasSingleton<BoardState>() == false)
        {
            return;
        }
        var G_State = GetSingleton<GameState>();
        var B_State = GetSingleton<BoardState>();

        if (G_State.IsActive == false)
        {
            return;
        }


        if (G_State.GameEnd == true)
        {
            return;
        }


        //盤面がリセットされていなければスキップ
        if (B_State.InitBoard == false)
        {
            return;
        }

        //プレイヤーのターン中以外はスキップ
        if (G_State.AIColor == G_State.NowTurn || G_State.IsActive == false)
        {
            return;
        }


        //設置するグリッドが確定した際にTrueになります
        bool PutConfirm = false;

        //プレイヤー側の入力受付
        Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData, ref GridComp GridData) =>
        {
            if (PutConfirm == false && GridClickData.clicked == true)
            {
                if (GridData.GridState == 3)
                {
                    //設置フラグを立てます
                    GridData.PutFlag = true;
                    PutConfirm = true;
                }
            }
        });

    }
}
