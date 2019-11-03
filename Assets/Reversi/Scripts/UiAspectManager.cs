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
            /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

            CanvasEntityDesc = new EntityQueryDesc()
            {
                All = new ComponentType[] { typeof(UICanvas), typeof(AspectManageTarget)},
            };

            /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
            //作成したクエリの結果を取得します。
            CanvasEntity = GetEntityQuery(CanvasEntityDesc);

        }

        protected override void OnUpdate()
        {
           AspectAdjustment();
        }

        //Int2で縦横のアスペクト比率を返します
        public float2 GetAspectRatio()
        {
            var DisplayData = World.TinyEnvironment().GetConfigData<DisplayInfo>();

            //ユークリッドの互除法により、アスペクト比率を求めます。
            float CommonDivisor = EuclideanAlgorithm(new int2(DisplayData.framebufferWidth, DisplayData.framebufferHeight));

            return new float2(DisplayData.framebufferWidth / CommonDivisor, DisplayData.framebufferHeight / CommonDivisor);
        }

        //AspectManageTargetがアタッチされているCanvasに対して、縦横の比率からサイズが小さい方にUIを合わせます。
        private void AspectAdjustment()
        {
            if (!(CanvasEntity.CalculateLength() > 0))
            {
                return;
            }

            //縦横、比率が小さい方に合わせます。
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

            //設定後、タグを削除します
            NativeArray<Entity> CanvasEntitys = CanvasEntity.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < CanvasEntitys.Length; i++)
            {
                EntityManager.RemoveComponent<AspectManageTarget>(CanvasEntitys[i]);
            }
            CanvasEntitys.Dispose();
        }

        //ユークリッドの互除法(最大公約数を求めます)
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
