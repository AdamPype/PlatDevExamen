using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ParallelNodePolicyAccumulator
    {
    NodeResult Policy(NodeResult result);
    }

public class NSuccessIsSuccesAccumulator : ParallelNodePolicyAccumulator
    {
    public static ParallelNodePolicyAccumulator Factory()
        {
        return new NSuccessIsSuccesAccumulator(2);
        }

    private readonly int _n;
    private int _count = 0;

    public NSuccessIsSuccesAccumulator(int n = 1)
        {
        _n = n;
        }

    public NodeResult Policy(NodeResult result)
        {
        if (result == NodeResult.Succes)
            {
            _count++;
            }

        return (_count >= _n) ? NodeResult.Succes : NodeResult.Failure;
        }

    }
