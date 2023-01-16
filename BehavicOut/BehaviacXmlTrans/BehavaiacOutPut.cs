using System.Numerics;

namespace BehaviacXmlTrans;

public enum BtStatus : byte
{
    Success,
    Fail,
    Running
}

public class BehavaiacOutPut
{
    private int nowRunningNode { get; set; } = -1;
    
    
    Vector3 p_hostPos = Vector3.Zero;
    int p_selfID = 0;
    Vector3 p_selfPos = Vector3.Zero;
    int p_hostID = 0;
    private int node11RunTime { get; set; } = 0;
    private int node11RunMaxTime { get; set; } = 40;

    public bool ABTFile()
    {
        
        switch (nowRunningNode)
        {
            case 11:
                goto node11Start;
            case 7:
                goto Node7Start;
        }
        //root
        //node11 Loop
        node11Start:
        var node11Result = false;
        node11RunTime++;
        {
            
        }
        node11End:
        if (node11RunTime < node11RunMaxTime)
        {
            nowRunningNode = 11;
            return node11Result;
        }

        //Node1 seq
        Node1Start:
        var node1Result = true;

        //Node2 Assignment
        p_selfID = GetMyObjID();

        //Node3 Action
        Node3Start:
        var node3Result = false;
        SyncHorizonState2Target(p_hostID);
        node3Result = true;
        if (!node3Result)
        {
            goto Node1End;
        }

        //Node4 seq
        var node4reslut = true;
        {
            //Node6 ifElse
            //Node7 Cond
            var node7Result = true;
            if (node7Result)
                //node8 
            {
            }
            else
            {
            }

            goto Node4End;
        }
        Node4End:
        if (!node4reslut)
        {
            goto Node1End;
        }

        Console.Out.WriteLine("....");
        //Node7 loop
        Node7Start:
        var node5Reslut = false;
        while (true)
        {
        }
        //seq

        Node1End:
        return node1Result;
    }

    private void SyncHorizonState2Target(int pHostId)
    {
        throw new NotImplementedException();
    }

    public bool FuncB { get; set; }

    public bool FuncA { get; set; }

    private int GetMyObjID()
    {
        throw new NotImplementedException();
    }
    
    
}