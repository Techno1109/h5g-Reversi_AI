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
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(PointerInteraction), typeof(Button), typeof(GridComp) },
        };

        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
        GridEntity = GetEntityQuery(GirdEntityDesc);
    }


    protected override void OnUpdate()
    {
        //���������V���O���g������������Ă��邩�ǂ����`�F�b�N
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


        //�Ֆʂ����Z�b�g����Ă��Ȃ���΃X�L�b�v
        if (B_State.InitBoard == false)
        {
            return;
        }

        //AI�̃^�[�����ȊO�̓X�L�b�v
        if (G_State.AIColor != G_State.NowTurn || G_State.IsActive == false)
        {
            return;
        }

        //���������Ŋe�Ֆʂ̕]���l�̎擾�A����є��f�������Ă����܂��B
        int2 PutPos = new int2(0, 0);
        int TopPriorityPoint = -999999;

        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridState == 3)
            {
                NativeArray<GridComp> GridDatas = new NativeArray<GridComp>(0, Allocator.Temp);

                //���݂̔Ֆʂ��擾
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

        ////��ԕ]���l�����������f�[�^�̃O���b�h�f�[�^�ƈ�v����ꏊ�ɐݒu�t���O�𗧂Ă܂�
        Entities.With(GridEntity).ForEach((ref GridComp GridData) =>
        {
            if (GridData.GridNum.x == PutPos.x &&
                GridData.GridNum.y == PutPos.y)
            {
                GridData.PutFlag = true;
            }
        });
    }




    //GridComp���e���W�ɑΉ����������ԂɊi�[����NativeArray��Ԃ��܂�
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

    //�w����W��GridComp���擾���܂�
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

    //�w�肵���O���b�h�̏�Ԃ��擾���܂�
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

    //�����Ă������W�Ƀf�[�^�����������܂��B
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

    //���łɃf�[�^���Z�b�g����Ă���̂��m�F���܂�
    //True�̏ꍇ�͒u����Ă���
    //False�̏ꍇ�͒u����Ă��Ȃ�
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

    //�N���b�N���ꂽ�ꏊ�ɋ��ݒu�ł��邩�ǂ����Ԃ��܂�
    public bool CheckCanPut(int2 SetPos, int SetState, ref NativeArray<GridComp> GridDatas)
    {
        //�P�����ł��ݒu�ł����OK
        //��
        if (CheckPinch(SetPos, new int2(0, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(0, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(-1, 0), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //�E
        if (CheckPinch(SetPos, new int2(1, 0), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, 1), SetState, 0, ref GridDatas))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, -1), SetState, 0, ref GridDatas))
        {
            return true;
        }

        return false;
    }

    //�e��O���b�h���ݒu�\���ǂ������m�F����
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
        //�ǂ����ɐݒu�ł��鎞�_�Ŏ��̃^�[���͗L���ƍl������B
        return CanPut;
    }

    //���񂾋�𔽓]���܂�
    public void Reverse(int2 SetPos, int SetState, ref NativeArray<GridComp> GridDatas)
    {
        //��
        CheckReverseState(SetPos, new int2(0, 1), SetState, 0, ref GridDatas);

        //��
        CheckReverseState(SetPos, new int2(0, -1), SetState, 0, ref GridDatas);

        //��
        CheckReverseState(SetPos, new int2(-1, 0), SetState, 0, ref GridDatas);

        //�E
        CheckReverseState(SetPos, new int2(1, 0), SetState, 0, ref GridDatas);

        //�E��
        CheckReverseState(SetPos, new int2(1, 1), SetState, 0, ref GridDatas);

        //�E��
        CheckReverseState(SetPos, new int2(1, -1), SetState, 0, ref GridDatas);

        //����
        CheckReverseState(SetPos, new int2(-1, 1), SetState, 0, ref GridDatas);

        //����
        CheckReverseState(SetPos, new int2(-1, -1), SetState, 0, ref GridDatas);
    }

    //�w��x�N�g�������ɋ��߂Ă���̂��`�F�b�N����
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

    //����߂Ă��邩�`�F�b�N���āA���߂Ă����甽�]������
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

    //�]���l�̍��v���擾���܂�
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