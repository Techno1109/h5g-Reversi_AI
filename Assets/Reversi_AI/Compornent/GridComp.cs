using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 GridNum;

    //0���󔒁@1�����@2���� 3=�ݒu�\
    public int GridState;

    //���̃O���b�h�̕]���l��ݒ肵�܂��A������΍����قǗD�悵�Ď��ׂ���ł��B
    public int Priority;
}
