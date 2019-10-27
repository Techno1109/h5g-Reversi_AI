using Unity.Entities;
using Unity.Tiny.Core2D;

public struct GameState : IComponentData
{
    //�Q�[�����i�s���Ȃ̂��A�I�����Ă���̂��i�[���܂�
    public bool IsActive;

    //  �ǂ���̃^�[���Ȃ̂����i�[���܂� 0==�ҋ@���@1==���@2==��
    public int NowTurn;

    // �������������ǂ����i�[���܂�
    public bool GameEnd;

    //���҂��i�[���܂� 0=���m��@1=�� 2=��
    public int WinnetNum;

    //AI���ǂ���̐F�Ȃ̂��i�[���܂� 0=���m��@1=�� 2=��
    public int AIColor;

    //AI�̑ҋ@�J�E���^�[
    public float AiWaitCount;
}
