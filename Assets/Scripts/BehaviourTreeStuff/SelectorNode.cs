using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//goes through every node until one succeeds
public class SelectorNode : CompositeNode
    {

    public SelectorNode(params INode[] nodes) : base(nodes)
        {
        //inherits _nodes from parent class CompositeNode
        }

    public override IEnumerator<NodeResult> Tick()
        {
        NodeResult returnNodeResult = NodeResult.Failure;
        foreach (INode node in _nodes)
            {
            IEnumerator<NodeResult> result = node.Tick();
            while (result.MoveNext() && result.Current == NodeResult.Running) //zolang de node een movenext heeft en hij is running
                {
                yield return NodeResult.Running; //on running it pauses the coroutine
                }

            returnNodeResult = result.Current;
            if (result.Current == NodeResult.Failure)
                continue; //go to next node
            if (result.Current == NodeResult.Succes)
                break; //break out of the foreach loop
            }
        yield return returnNodeResult; //returns succes if one of the nodes was succesful, otherwise returns failure
        }
    }
