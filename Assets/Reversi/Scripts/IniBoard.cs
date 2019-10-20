using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.UILayout;
using Unity.Tiny.Core2D;
using Unity.Tiny.UIControls;
using Unity.Mathematics;
using Unity.Collections;


public class IniBoard : ComponentSystem
{

    EntityQueryDesc GridEntityDesc;
    EntityQuery GridEntity;

    const float GridSize = 42.5f;
    const float GridStartPos_X = 20;
    const float GridStartPos_Y = -80;
    const int BoardSize = 8;

    protected override void OnCreate()
    {
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GridEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform),typeof(Sprite2DRenderer),typeof(GridComp) },
        };


        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
        GridEntity = GetEntityQuery(GridEntityDesc);
    }


    protected override void OnUpdate()
    {
        if (HasSingleton<BoardState>() == false)
        {
            return;
        }
        if (HasSingleton<GameState>() == false)
        {
            return;
        }
        if (!((GridEntity.CalculateLength() > 0)))
        {
            return;
        }
        var Config = GetSingleton<BoardState>();
        if (GetSingleton<BoardState>().EmitBoard == false)
        {
            InitBoard();
        }
    }

    //�Ֆʂ��쐬���܂�
    public void InitBoard()
    {
        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            GridData.GridState = 0;
            if ( (GridData.GridNum.y == 3 || GridData.GridNum.y == 4) && (GridData.GridNum.x == 3 || GridData.GridNum.x == 4))
            {
                if (GridData.GridNum.x == 3)
                {
                    GridData.GridState = 2;
                }
                if (GridData.GridNum.x == 4)
                {
                    GridData.GridState = 1;
                }

                if (GridData.GridNum.y == 4)
                {
                    GridData.GridState = GridData.GridState == 1 ? 2 : 1;
                }
            }
        });

        var Config = GetSingleton<BoardState>();
        Config.EmitBoard = true;
        Config.WhiteCount = 2;
        Config.BlackCount = 2;
        Config.InitBoard = false;
        SetSingleton<BoardState>(Config);

        var State = GetSingleton<GameState>();
        State.IsActive = true;
        State.NowTurn = 1;
        State.GameEnd = false;
        State.WinnetNum = 0;
        SetSingleton<GameState>(State);

        EntityManager.World.GetExistingSystem<BoardMan>().RefreshBoardColor();
    }
}
