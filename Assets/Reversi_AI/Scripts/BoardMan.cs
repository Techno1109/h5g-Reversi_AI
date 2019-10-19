using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(IniBoard))]
public class BoardMan : ComponentSystem
{
    const int BoardSize = 8;


    EntityQueryDesc GirdEntityDesc;
    EntityQuery GridEntity;

    EntityQueryDesc CanvasDesc;
    EntityQuery CanvasEntity;

    protected override void OnCreate()
    {
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(PointerInteraction) ,typeof(Button),typeof(GridComp)},
        };

        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
        GridEntity = GetEntityQuery(GirdEntityDesc);
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

        if (G_State.IsActive==false)
        {
            return;
        }


        if (G_State.GameEnd == true)
        {
            return;
        }

        if ( ! (GridEntity.CalculateLength() > 0) )
        {
            return;
        }

        NativeArray<Entity> GridDatas = new NativeArray<Entity>(0, Allocator.Temp);

        GetGirdArray(ref GridDatas);

        if (B_State.InitBoard == false)
        {
            CheckCanPut_AllGrid(ref G_State, ref GridDatas);
            B_State.InitBoard = true;
            SetSingleton<BoardState>(B_State);
        }

        //AI�̃^�[�����͑�����󂯕t���܂���B
        if(G_State.AIColor!=G_State.NowTurn)
        {
            //�ݒu����O���b�h���m�肵���ۂ�True�ɂȂ�܂�
            bool PutConfirm = false;

            //�v���C���[���̓��͎�t
            //����ʃV�X�e���Ƃ��ēƗ������܂�
            Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData, ref GridComp GridData) =>
            {
                if (PutConfirm==false && GridClickData.clicked == true)
                {

                    if (CheckGridData(GridData.GridNum, ref GridDatas))
                    {
                        return;
                    }


                    if (GridData.GridState == 3)
                    {
                        //�ݒu�t���O�𗧂Ă܂�
                        GridData.PutFlag = true;
                        PutConfirm = true;
                    }
                }
            });

        }

        Entities.With(GridEntity).ForEach((Entity EntityData, ref GridComp GridData) =>
        {
            if (GridData.PutFlag == true)
            {
                SetGridData(GridData.GridNum, G_State.NowTurn, ref GridDatas);

                Reverse(GridData.GridNum, G_State.NowTurn, ref GridDatas);

                G_State.NowTurn = G_State.NowTurn == 1 ? 2 : 1;

                if (!CheckCanPut_AllGrid(ref G_State, ref GridDatas))
                {
                    G_State.NowTurn = G_State.NowTurn == 1 ? 2 : 1;
                    CheckCanPut_AllGrid(ref G_State, ref GridDatas);
                }

                //�����I����Ƀt���O��False�ɕύX���܂�
                GridData.PutFlag = false;

                SetSingleton<GameState>(G_State);

                PawnCounter();
            }
        });

        GridDatas.Dispose();
    }

    //Entity���e���W�ɑΉ����������ԂɊi�[���Ȃ����܂�
    public bool  GetGirdArray(ref NativeArray<Entity> ReturnEntities)
    {
        NativeArray<Entity> EntitiesArray = new NativeArray<Entity>(BoardSize * BoardSize, Allocator.Temp);

        for (int i = 0; i < BoardSize * BoardSize; ++i)
        {
            EntitiesArray[i] = Entity.Null;
        }


        Entities.With(GridEntity).ForEach((Entity EntityData, ref GridComp GridData) =>
        {
            if (GridData.GridNum.x < BoardSize && GridData.GridNum.y < BoardSize)
            {
                EntitiesArray[GridData.GridNum.x + (GridData.GridNum.y * BoardSize)] = EntityData;
            }
        });

        ReturnEntities = EntitiesArray;

        EntitiesArray.Dispose();
        return true;
    }

    //�w����W��Entity���擾���܂�
    public Entity GetGridEntity(int2 CheckPos, ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < 0 && CheckPos.y < 0)
        {
            return Entity.Null;
        }

        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            return Entities[CheckPos.x + (CheckPos.y * BoardSize)];
        }

        return Entity.Null;
    }

    //�w�肵���O���b�h�̃f�[�^���擾���܂�
    public int GetGridData(int2 CheckPos, ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < 0 || CheckPos.y < 0)
        {
            return -1;
        }

        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[CheckPos.x + (CheckPos.y * BoardSize)]);
            return GridData.GridState;
        }

        return -1;
    }

    //�����Ă������W�Ƀf�[�^�����������܂��B
    public void SetGridData(int2 SetPos,int SetStatus,ref NativeArray<Entity> Entities)
    {
        if (SetPos.x < 0 || SetPos.y < 0)
        {
            return;
        }

        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[SetPos.x + (SetPos.y * BoardSize)]);
            GridData.GridState = SetStatus;
            EntityManager.SetComponentData(Entities[SetPos.x + (SetPos.y * BoardSize)], GridData);
        }
    }

    //���łɃf�[�^���Z�b�g����Ă���̂��m�F���܂�
    //True�̏ꍇ�͒u����Ă���
    //False�̏ꍇ�͒u����Ă��Ȃ�
    public bool CheckGridData(int2 SetPos,ref NativeArray<Entity> Entities)
    {
        if(SetPos.x < 0 || SetPos.y < 0)
        {
            return true;
        }
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[SetPos.x + (SetPos.y * BoardSize)]);
            if(GridData.GridState==0 || GridData.GridState==3)
            {
                return false;
            }
        }

        return true;
    }

    //�N���b�N���ꂽ�ꏊ�ɋ��ݒu�ł��邩�ǂ����Ԃ��܂�
    public bool CheckCanPut(int2 SetPos,int SetState ,ref NativeArray<Entity> Entities)
    {
        //�P�����ł��ݒu�ł����OK
        //��
        if(CheckPinch(SetPos,new int2(0,1), SetState, 0, ref Entities))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(0, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(-1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E
        if (CheckPinch(SetPos, new int2(1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, -1), SetState, 0, ref Entities))
        {
            return true;
        }

        return false;
    }

    //�e��O���b�h���ݒu�\���ǂ������m�F����
    public bool CheckCanPut_AllGrid(ref GameState State, ref NativeArray<Entity> GridDatas)
    {
        bool CanPut = false;
        for(int x=0;x<BoardSize;x++)
        {
            for(int y = 0; y < BoardSize; y++)
            {
                int2 TargetGrid = new int2(x, y);

                if (CheckGridData(TargetGrid,ref GridDatas))
                {
                    continue;
                }


                if (CheckCanPut(TargetGrid, State.NowTurn, ref GridDatas))
                {
                    //�ݒu�\�̏ꍇ�A�}�X�ڂ̏�Ԃ�ݒu�\�}�X�Ƃ��Đݒ肷��
                    CanPut |= true;
                    SetGridData(TargetGrid, 3, ref GridDatas);
                }
                else
                {
                    SetGridData(TargetGrid, 0, ref GridDatas);
                }
            }
        }
        RefreshBoardColor();
        //�ǂ����ɐݒu�ł��鎞�_�Ŏ��̃^�[���͗L���ƍl������B
        return CanPut;
    }

    //���񂾋�𔽓]���܂�
    public void Reverse(int2 SetPos, int SetState, ref NativeArray<Entity> Entities)
    {
        //��
        CheckReverseState(SetPos, new int2(0, 1), SetState, 0, ref Entities);

        //��
        CheckReverseState(SetPos, new int2(0, -1), SetState, 0, ref Entities);

        //��
        CheckReverseState(SetPos, new int2(-1, 0), SetState, 0, ref Entities);

        //�E
        CheckReverseState(SetPos, new int2(1, 0), SetState, 0, ref Entities);

        //�E��
        CheckReverseState(SetPos, new int2(1, 1), SetState, 0, ref Entities);

        //�E��
        CheckReverseState(SetPos, new int2(1, -1), SetState, 0, ref Entities);

        //����
        CheckReverseState(SetPos, new int2(-1, 1), SetState, 0, ref Entities);

        //����
        CheckReverseState(SetPos, new int2(-1, -1), SetState, 0, ref Entities);
    }

    //�w��x�N�g�������ɋ��߂Ă���̂��`�F�b�N����
    public bool CheckPinch(int2 CheckPos,int2 CheckVector,int BaseState , int Count,ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < 0 || CheckPos.y < 0)
        {
            return false;
        }

        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        if(GetGridData(CheckPos+CheckVector,ref Entities) == BaseState)
        {
            if(Count>0)
            {
                return true;
            }

            return false;
        }

        int TargetGridState = GetGridData(CheckPos + CheckVector, ref Entities);
        if (TargetGridState == 0 || TargetGridState == -1 || TargetGridState == 3)
        {
            return false;
        }

        return CheckPinch(CheckPos+CheckVector, CheckVector,BaseState,++Count,ref Entities);
    }

    //����߂Ă��邩�`�F�b�N���āA���߂Ă����甽�]������
    public bool CheckReverseState(int2 CheckPos, int2 CheckVector, int BaseState, int Count, ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < 0 && CheckPos.y < 0)
        {
            return false;
        }

        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        if (GetGridData(CheckPos + CheckVector, ref Entities) == BaseState)
        {
            if (Count > 0)
            {
                SetGridData(CheckPos, BaseState, ref Entities);
                return true;
            }

            return false;
        }

        int TargetGridState = GetGridData(CheckPos + CheckVector, ref Entities);
        if (TargetGridState == 0 || TargetGridState == -1 || TargetGridState == 3)
        {
            return false;
        }

        if (CheckReverseState(CheckPos + CheckVector, CheckVector, BaseState, ++Count, ref Entities))
        {
            SetGridData(CheckPos,BaseState,ref Entities);
            return true;
        }

        return false;
    }

    //��̐����J�E���g���܂�
    public void PawnCounter()
    {
        int White = 0;
        int Black = 0;
        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridState==1)
            {
                Black+=1;
            }

            if (GridData.GridState == 2)
            {
                White += 1;
            }
        });

        var Config = GetSingleton<BoardState>();
        Config.WhiteCount = White;
        Config.BlackCount = Black;
        SetSingleton<BoardState>(Config);

        if(White+Black>=BoardSize*BoardSize)
        {
            var State = GetSingleton<GameState>();
            State.IsActive = false;
            State.GameEnd = true;
            State.WinnetNum = Black > White ? 1 : 2;
            SetSingleton<GameState>(State);
        }
    }

    //�Ֆʂ̃f�[�^��ǂݎ��A�F���ĕ`�悵�܂��B
    public bool RefreshBoardColor()
    {
        //Board���̂̐F
        Color TableColor_1 = new Color(0.4366531f, 0.853f, 0.423941f);
        Color TableColor_2 = new Color(0.3252917f, 0.6886792f, 0.3151032f);

        //��̐F
        Color White = new Color(0.9705882f, 0.9705882f, 0.9705882f);
        Color Black = new Color(0.1960784f, 0.1960784f, 0.1960784f);

        Color CanPut = new Color(0.874f, 0.2264659f, 0.140714f);

        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            //�Ֆʂɋ�����Ȃ���΂��̃O���b�h�ɓK����Board�̐F��ݒ�
            if (GridData.GridState==0)
            {
                int ColBaseNum = GridData.GridNum.y % 2 == 0 ? 0 : 1;

                Sprite2D.color = (ColBaseNum + GridData.GridNum.x) % 2 > 0 ? TableColor_1 : TableColor_2;
                return;
            }

            //�ݒu�\�}�X�̏ꍇ
            if(GridData.GridState==3)
            {
                Sprite2D.color = CanPut;
                return;
            }

            //�����łȂ��ꍇ�A��̐F��`��

            Sprite2D.color = GridData.GridState == 1 ? Black : White;
        });

        return true;
    }
}
