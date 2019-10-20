using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(PlayerInput))]
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
        if (B_State.InitBoard == false)
        {
            return;
        }

        //AIのターン中以外はスキップ
        if (G_State.AIColor != G_State.NowTurn || G_State.IsActive == false)
        {
            return;
        }

        //ここから先で各盤面の評価値の取得、および判断を書いていきます。
        int2 PutPos = new int2(0, 0);
        int TopPriorityPoint = -999999;

        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridState == 3)
            {
                NativeArray<GridComp> GridDatas = new NativeArray<GridComp>(0, Allocator.Temp);

                //現在の盤面を取得
                GetGirdData(ref GridDatas);

                SetGridData(GridData.GridNum, G_State.AIColor, ref GridDatas);

                Reverse(GridData.GridNum, G_State.AIColor, ref GridDatas);

                int ThisPriority = GetTotalPriority(G_State.AIColor, ref GridDatas);
                if (ThisPriority> TopPriorityPoint)
                {
                    TopPriorityPoint = ThisPriority;
                    PutPos = GridData.GridNum;
                }

                GridDatas.Dispose();
            }
        });

        ////一番評価値が高かったデータのグリッドデータと一致する場所に設置フラグを立てます
        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridNum.x == PutPos.x &&
                GridData.GridNum.y == PutPos.y)
            {
                GridData.PutFlag = true;
            }
        });
    }




    //GridCompを各座標に対応させた順番に格納したNativeArrayを返します
    public bool GetGirdData(ref NativeArray<GridComp> ReturnGridDatas)
    {
        NativeArray<GridComp> GridDataArray = new NativeArray<GridComp>(BoardSize * BoardSize, Allocator.Temp);

        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridNum.x < BoardSize && GridData.GridNum.y < BoardSize)
            {
                GridDataArray[GridData.GridNum.x + (GridData.GridNum.y * BoardSize)] = GridData;
            }
        });

        ReturnGridDatas = GridDataArray;

        GridDataArray.Dispose();
        return true;
    }

    //指定座標のGridCompを取得します
    public GridComp GetGridData(int2 CheckPos, ref NativeArray<GridComp> GridDatas)
    {
        if (CheckPos.x < 0 && CheckPos.y < 0)
        {
            return new GridComp();
        }

        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            return GridDatas[CheckPos.x + (CheckPos.y * BoardSize)];
        }

        return new GridComp();
    }

    //指定したグリッドの状態を取得します
    public int GetGridState(int2 CheckPos, ref NativeArray<GridComp> GridDatas)
    {
        if (CheckPos.x < 0 || CheckPos.y < 0)
        {
            return -1;
        }

        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            return GridDatas[CheckPos.x + (CheckPos.y * BoardSize)].GridState;
        }

        return -1;
    }

    //送られてきた座標にデータを書き換えます。
    public void SetGridData(int2 SetPos, int SetStatus, ref NativeArray<GridComp> GridDatas)
    {
        if (SetPos.x < 0 || SetPos.y < 0)
        {
            return;
        }

        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp Tmp = GridDatas[SetPos.x + (SetPos.y * BoardSize)];
            Tmp.GridState = SetStatus;
            GridDatas[SetPos.x + (SetPos.y * BoardSize)] = Tmp;
        }
    }

    //すでにデータがセットされているのか確認します
    //Trueの場合は置かれている
    //Falseの場合は置かれていない
    public bool CheckGridData(int2 SetPos, ref NativeArray<GridComp> GridDatas)
    {
        if (SetPos.x < 0 || SetPos.y < 0)
        {
            return true;
        }
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp GridData = GridDatas[SetPos.x + (SetPos.y * BoardSize)];
            if (GridData.GridState == 0 || GridData.GridState == 3)
            {
                return false;
            }
        }

        return true;
    }

    //クリックされた場所に駒を設置できるかどうか返します
    public bool CheckCanPut(int2 SetPos, int SetState, ref NativeArray<GridComp> GridDatas)
    {
        //１方向でも設置できればOK
        //上
        if (CheckPinch(SetPos, new int2(0, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //下
        if (CheckPinch(SetPos, new int2(0, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //左
        if (CheckPinch(SetPos, new int2(-1, 0), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //右
        if (CheckPinch(SetPos, new int2(1, 0), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //右上
        if (CheckPinch(SetPos, new int2(1, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //右下
        if (CheckPinch(SetPos, new int2(1, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //左上
        if (CheckPinch(SetPos, new int2(-1, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //左下
        if (CheckPinch(SetPos, new int2(-1, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }

        return false;
    }

    //各種グリッドが設置可能かどうかを確認する
    public bool CheckCanPut_AllGrid(ref GameState State, ref NativeArray<GridComp> GridDatas)
    {
        bool CanPut = false;
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                int2 TargetGrid = new int2(x, y);

                if (CheckGridData(TargetGrid, ref GridDatas))
                {
                    continue;
                }


                if (CheckCanPut(TargetGrid, State.NowTurn, ref GridDatas))
                {
                    //設置可能の場合、マス目の状態を設置可能マスとして設定する
                    CanPut |= true;
                    SetGridData(TargetGrid, 3, ref GridDatas);
                }
                else
                {
                    SetGridData(TargetGrid, 0, ref GridDatas);
                }
            }
        }
        //どこかに設置できる時点で次のターンは有効と考えられる。
        return CanPut;
    }

    //挟んだ駒を反転します
    public void Reverse(int2 SetPos, int SetState, ref NativeArray<GridComp> GridDatas)
    {
        //上
        CheckReverseState(SetPos, new int2(0, 1), SetState, 0, ref GridDatas);

        //下
        CheckReverseState(SetPos, new int2(0, -1), SetState, 0, ref GridDatas);

        //左
        CheckReverseState(SetPos, new int2(-1, 0), SetState, 0, ref GridDatas);

        //右
        CheckReverseState(SetPos, new int2(1, 0), SetState, 0, ref GridDatas);

        //右上
        CheckReverseState(SetPos, new int2(1, 1), SetState, 0, ref GridDatas);

        //右下
        CheckReverseState(SetPos, new int2(1, -1), SetState, 0, ref GridDatas);

        //左上
        CheckReverseState(SetPos, new int2(-1, 1), SetState, 0, ref GridDatas);

        //左下
        CheckReverseState(SetPos, new int2(-1, -1), SetState, 0, ref GridDatas);
    }

    //指定ベクトル方向に挟めているのかチェックする
    public bool CheckPinch(int2 CheckPos, int2 CheckVector, int BaseState, int Count, ref NativeArray<GridComp> GridDatas)
    {
        if (CheckPos.x < 0 || CheckPos.y < 0)
        {
            return false;
        }

        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        int TargetGridState = GetGridState(CheckPos + CheckVector, ref GridDatas);

        if (TargetGridState == BaseState)
        {
            if (Count > 0)
            {
                return true;
            }

            return false;
        }

        if (TargetGridState == 0 || TargetGridState == -1 || TargetGridState == 3)
        {
            return false;
        }

        return CheckPinch(CheckPos + CheckVector, CheckVector, BaseState, ++Count, ref GridDatas);
    }

    //駒が挟めているかチェックして、挟めていたら反転させる
    public bool CheckReverseState(int2 CheckPos, int2 CheckVector, int BaseState, int Count, ref NativeArray<GridComp> GridDatas)
    {
        if (CheckPos.x < 0 && CheckPos.y < 0)
        {
            return false;
        }

        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        int TargetGridState = GetGridState(CheckPos + CheckVector, ref GridDatas);

        if (TargetGridState == BaseState)
        {
            if (Count > 0)
            {
                SetGridData(CheckPos, BaseState, ref GridDatas);
                return true;
            }

            return false;
        }

        if (TargetGridState == 0 || TargetGridState == -1 || TargetGridState == 3)
        {
            return false;
        }

        if (CheckReverseState(CheckPos + CheckVector, CheckVector, BaseState, ++Count, ref GridDatas))
        {
            SetGridData(CheckPos, BaseState, ref GridDatas);
            return true;
        }

        return false;
    }

    //評価値の合計を取得します
    public int GetTotalPriority(int TargetState, ref NativeArray<GridComp> GridDatas)
    {
        int Priority = 0;

        for(int i=0;i<GridDatas.Length;i++)
        {
            if (GridDatas[i].GridState==TargetState)
            {
                Priority += GridDatas[i].Priority;
            }
        }

        return Priority;
    }

  }