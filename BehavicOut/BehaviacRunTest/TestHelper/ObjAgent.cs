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
}