using Unity.Entities;

public struct BoardState : IComponentData
{
    //���̃R���|�[�l���g�ŔՖʂ̏�Ԃ��i�[�ł���悤�ɂ��܂�
    public bool EmitBoard;

    public bool InitBoard;

    //�Ֆʏ�ɐݒu����Ă��锒��̐�
    public int WhiteCount;

    //�Ֆʏ�ɐݒu����Ă��鍕��̐�
    public int BlackCount;
}
