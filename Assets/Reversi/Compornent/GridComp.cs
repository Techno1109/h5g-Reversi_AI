using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 GridNum;

    //0���󔒁@1�����@2���� 3=�ݒu�\
    public int GridState;

    //���̃O���b�h�̕]���l��ݒ肵�܂��A������΍����قǗD�悵�Ď��ׂ���ł��B
    public int Priority;

    //���̃O���b�h�ɐݒu���邱�Ƃ��m�肵���ꍇ��True�ɂȂ�܂��B
    //���t���[���e�O���b�h�̃`�F�b�N�I�����False�ɕύX����܂��B
    public bool PutFlag;
}
