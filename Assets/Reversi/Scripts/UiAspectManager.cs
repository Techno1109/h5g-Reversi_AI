using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Mathematics;
using Unity.Collections;
namespace Liberapp.Ui.AspectManager
{


    public class UiAspectManager : ComponentSystem
    {
        EntityQueryDesc CanvasEntityDesc;
        EntityQuery CanvasEntity;

        protected override void OnCreate()
        {
            /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

            CanvasEntityDesc = new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(UICanvas), typeof(AspectManageTarget)},
            };

            /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
            //�쐬�����N�G���̌��ʂ��擾���܂��B
            CanvasEntity = GetEntityQuery(CanvasEntityDesc);

        }

        protected override void OnUpdate()
        {
           AspectAdjustment();
        }

        //Int2�ŏc���̃A�X�y�N�g�䗦��Ԃ��܂�
        public float2 GetAspectRatio()
        {
            var DisplayData = World.TinyEnvironment().GetConfigData<DisplayInfo>();

            //���[�N���b�h�̌ݏ��@�ɂ��A�A�X�y�N�g�䗦�����߂܂��B
            float CommonDivisor = EuclideanAlgorithm(new int2(DisplayData.framebufferWidth, DisplayData.framebufferHeight));

            return new float2(DisplayData.framebufferWidth / CommonDivisor, DisplayData.framebufferHeight / CommonDivisor);
        }

        //AspectManageTarget���A�^�b�`����Ă���Canvas�ɑ΂��āA�c���̔䗦����T�C�Y������������UI�����킹�܂��B
        private void AspectAdjustment()
        {
            if (!(CanvasEntity.CalculateLength() > 0))
            {
                return;
            }

            //�c���A�䗦�����������ɍ��킹�܂��B
            var DisplayData = World.TinyEnvironment().GetConfigData<DisplayInfo>();

            float MatchParam = 1;

            if(DisplayData.framebufferHeight >= DisplayData.framebufferWidth)
            {
                MatchParam = 0;
            }

            Entities.With(CanvasEntity).ForEach((Entity EntityData, ref UICanvas Canvas) =>
            {
                Canvas.matchWidthOrHeight = MatchParam;
            });

            //�ݒ��A�^�O���폜���܂�
            NativeArray<Entity> CanvasEntitys = CanvasEntity.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < CanvasEntitys.Length; i++)
            {
                EntityManager.RemoveComponent<AspectManageTarget>(CanvasEntitys[i]);
            }
            CanvasEntitys.Dispose();
        }

        //���[�N���b�h�̌ݏ��@(�ő���񐔂����߂܂�)
        private int EuclideanAlgorithm(int2 Data)
        {
            if(Data.y==0)
            {
                return Data.x;
            }

            return EuclideanAlgorithm(new int2(Data.y,Data.x%Data.y));
        }
    }
}
