using behaviac;

public class ObjAgent
{
    public bool IsInValidVint3(VInt3 testVar)
    {
        return false;
    }

    public void LogMessage(string seq)
    {
        Console.Out.WriteLine(seq);
    }

    public EBTStatus IsOffline()
    {
        return EBTStatus.BT_FAILURE;
    }

    public EBTStatus PlayAgeAction(string balabala)
    {
        return EBTStatus.BT_RUNNING;
    }

    public int GetIntVariable(string empty)
    {
        return 1;
    }


    public SkillSlotType GetTargetSkillSlotType(uint pTargetHero, int i, List<AISearchSkillType> aiSearchSkillTypes, List<AISkillTypeTag> aiSkillTypeTags, List<AISkillTypeTag> list, int i1)
    {
        throw new NotImplementedException();
    }

    public void GetEnemyActorInRange(int i, List<AISearcherType> aiSearcherTypes, List<int> testIntList, List<int> ints)
    {
        throw new NotImplementedException();
    }
}