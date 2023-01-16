namespace SGame.InGame.GameLogic;
using behaviac;

public class TestAgent : Agent
{
    private int TestGameStatus { get; set; } = 0;

    public EBTStatus TestRunning(int p0)
    {
        if (TestGameStatus >= p0)
        {
            TestGameStatus = 3;
            return EBTStatus.BT_SUCCESS;
        }

        TestGameStatus++;
        Console.Out.WriteLine($"now Running On {TestGameStatus}");
        return EBTStatus.BT_RUNNING;
    }


    public int TestGetValue()
    {
        return TestGameStatus;
    }
}