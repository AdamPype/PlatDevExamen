using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//goes through every node until one fails
public class SequenceNode : CompositeNode
    {

    public SequenceNode(params INode[] nodes) : base(nodes)
        {
        //inherits _nodes from parent class CompositeNode
        }

    public override IEnumerator<NodeResult> Tick()
        {
        NodeResult returnNodeResult = NodeResult.Succes;

        foreach (INode node in _nodes)
            {
            IEnumerator<NodeResult> result = node.Tick();

            while (result.MoveNext() && result.Current == NodeResult.Running)
                {
                yield return NodeResult.Running;
                }

            //the opposite as selectornode
            returnNodeResult = result.Current;
            if (result.Current == NodeResult.Succes)
                continue;
            if (result.Current == NodeResult.Failure)
                break;

            }

        yield return returnNodeResult;
        }
    }
