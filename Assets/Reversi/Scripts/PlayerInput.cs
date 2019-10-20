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

        //�v���C���[�̃^�[�����ȊO�̓X�L�b�v
        if (G_State.AIColor == G_State.NowTurn || G_State.IsActive == false)
        {
            return;
        }


        //�ݒu����O���b�h���m�肵���ۂ�True�ɂȂ�܂�
        bool PutConfirm = false;

        //�v���C���[���̓��͎�t
        Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData, ref GridComp GridData) =>
        {
            if (PutConfirm == false && GridClickData.clicked == true)
            {
                if (GridData.GridState == 3)
                {
                    //�ݒu�t���O�𗧂Ă܂�
                    GridData.PutFlag = true;
                    PutConfirm = true;
                }
            }
        });

    }
}
