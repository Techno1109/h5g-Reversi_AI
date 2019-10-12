using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.Core;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Tiny.Text;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(BoardMan))]
public class HudMan : ComponentSystem
{
    EntityQueryDesc GameEndWindowDesc;
    EntityQuery     GameEndWindowEntity;

    EntityQueryDesc WinnerTextDesc;
    EntityQuery WinnerTextEntity;

    EntityQueryDesc WinnerBgDesc;
    EntityQuery WinnerBgEntity;

    EntityQueryDesc NowTurnTextDesc;
    EntityQuery NowTurnTextEntity;

    EntityQueryDesc NowTurnBgDesc;
    EntityQuery NowTurnBgEntity;

    EntityQueryDesc ReplayButtonDesc;
    EntityQuery ReplayButtonEntity;

    EntityQueryDesc WhiteCountTextDesc;
    EntityQuery WhiteCountTextEntity;

    EntityQueryDesc BlackCountTextDesc;
    EntityQuery BlackCountTextEntity;

    protected override void OnCreate()
    {
        /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

        GameEndWindowDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(GameEndWindowTag)},
        };

        WinnerTextDesc = new EntityQueryDesc()
        {
            All= new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(WinnerTexts), typeof(WinnerTextTag) },
        };

        WinnerBgDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(WinnerBgTag) },
        };

        NowTurnTextDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(NowTurnTag) ,typeof(TurnTexts)},
        };

        NowTurnBgDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(NowTurnBg) },
        };


       ReplayButtonDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(PointerInteraction), typeof(Button), typeof(ReplayButton) },
        };

        WhiteCountTextDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(TextString), typeof(WhiteTag),typeof(Text2DStyle) },
        };

        BlackCountTextDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(TextString), typeof(BlackTag), typeof(Text2DStyle) },
        };

        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        GameEndWindowEntity = GetEntityQuery(GameEndWindowDesc);
        WinnerTextEntity = GetEntityQuery(WinnerTextDesc);
        WinnerBgEntity = GetEntityQuery(WinnerBgDesc);
        NowTurnTextEntity = GetEntityQuery(NowTurnTextDesc);
        NowTurnBgEntity = GetEntityQuery(NowTurnBgDesc);
        ReplayButtonEntity = GetEntityQuery(ReplayButtonDesc);
        WhiteCountTextEntity = GetEntityQuery(WhiteCountTextDesc);
        BlackCountTextEntity = GetEntityQuery(BlackCountTextDesc);
    }

    protected override void OnUpdate()
    {
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
        Color White = new Color(0.9705882f, 0.9705882f, 0.9705882f);
        Color Black = new Color(0.1960784f, 0.1960784f, 0.1960784f);

        //バックグラウンドの色と文字を変更
        Entities.With(NowTurnBgEntity).ForEach((ref Sprite2DRenderer Sprite2D) =>
        {
            Sprite2D.color = G_State.NowTurn == 1 ? Black : White;
        });

        Entities.With(NowTurnTextEntity).ForEach((ref Sprite2DRenderer Sprite2D,ref TurnTexts Texts) =>
        {
            Sprite2D.sprite = G_State.NowTurn == 1 ? Texts.Black : Texts.White;
            Sprite2D.color = G_State.NowTurn == 1 ? White : Black;
        });

        if (G_State.GameEnd == true)
        {
            bool PushFlag = false;

            Entities.With(ReplayButtonEntity).ForEach((ref PointerInteraction GridClickData) =>
            {
                PushFlag = GridClickData.clicked;
            });

            Entities.With(GameEndWindowEntity).ForEach((ref RectTransform RectT) =>
            {
                //描画位置
                if (PushFlag == false)
                {
                    RectT.anchoredPosition = new float2(0, 0);
                }
                else
                {
                    RectT.anchoredPosition = new float2(0, 1000);
                    EntityManager.World.GetExistingSystem<IniBoard>().InitBoard();
                }

            });

            Entities.With(WinnerTextEntity).ForEach((ref Sprite2DRenderer Sprite2D, ref WinnerTexts WinnerText) =>
            {
                Sprite2D.sprite = G_State.WinnetNum == 1 ? WinnerText.Black : WinnerText.White;
                Sprite2D.color = G_State.WinnetNum == 1? White : Black;
            });


            Entities.With(WinnerBgEntity).ForEach((ref Sprite2DRenderer Sprite2D) =>
            {
                Sprite2D.color = G_State.WinnetNum == 1 ? Black : White;
            });
        }
            DrawCount(B_State.WhiteCount, B_State.BlackCount);
    }

    public void DrawCount(int WhiteCount,int BlackCount)
    {

        Entities.With(WhiteCountTextEntity).ForEach((Entity TargetEntity, ref Text2DStyle TextStyle) =>
        {
            EntityManager.SetBufferFromString<TextString>(TargetEntity, WhiteCount.ToString());
        });

        Entities.With(BlackCountTextEntity).ForEach((Entity TargetEntity, ref Text2DStyle TextStyle) =>
        {
            EntityManager.SetBufferFromString<TextString>(TargetEntity, BlackCount.ToString());
            TextStyle.color = new Color(0.9705882f, 0.9705882f, 0.9705882f); 
        });
    }
}
