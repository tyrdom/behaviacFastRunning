// See https://aka.ms/new-console-template for more information

using behaviac;

internal static class Program
{
    private static void Main()
    {
        
            
        var basicNodes = new WrapperAI_NewTest_TestNode(new ObjAgent());
        const int tickNum = 20;
        for (var i = 0; i < tickNum; i++)
        {
            basicNodes.Tick();
        }
    }
}