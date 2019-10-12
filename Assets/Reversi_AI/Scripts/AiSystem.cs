using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(BoardMan))]
public class AiSystem : ComponentSystem
{
    const int BoardSize = 8;


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
        if(B_State.InitBoard==false)
        {
            return;
        }

        //AIのターン中以外はスキップ
        if (G_State.AIColor != G_State.NowTurn || G_State.AIColor==0)
        {
            return;
        }

        //稼働中のBoardManを取得
        var BManager= EntityManager.World.GetExistingSystem<BoardMan>();

        NativeArray<Entity> GridDatas = new NativeArray<Entity>(0, Allocator.Temp);

        //現在の盤面を取得
        BManager.GetGirdArray(ref GridDatas);


        //ここから先で各盤面の評価値の取得、および判断を書いていきます。
    }
}
