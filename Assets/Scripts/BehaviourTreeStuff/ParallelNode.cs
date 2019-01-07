using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//goes through every node and gives a result based on a given policy
public class ParallelNode : CompositeNode
    {

    //we use a delegate so we can update the result in different ways
    public delegate ParallelNodePolicyAccumulator Policy();
    private Policy _policy;

    public ParallelNode(Policy policy, params INode[] nodes) : base(nodes)
        {
        _policy = policy;
        }

    public override IEnumerator<NodeResult> Tick()
        {

        ParallelNodePolicyAccumulator acc = _policy();
        NodeResult returnNodeResult = NodeResult.Failure;

        foreach (INode node in _nodes)
            {
            IEnumerator<NodeResult> result = node.Tick();

            while (result.MoveNext() && result.Current == NodeResult.Running)
                {
                yield return NodeResult.Running;
                }

            returnNodeResult = acc.Policy(result.Current); //the result will vary depending on the policy
            }

        yield return returnNodeResult;
        }
    }
