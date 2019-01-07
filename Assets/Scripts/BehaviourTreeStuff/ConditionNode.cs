using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//requires a condition, goes to the next node when it succeeds
public class ConditionNode : INode
    {
    public delegate bool Condition();
    private readonly Condition _condition;

    //constructor
    public ConditionNode(Condition condition)
        {
        _condition = condition;
        }

    public IEnumerator<NodeResult> Tick()
        {
        //returns succes or failure based on Condition()
        if (_condition())
            yield return NodeResult.Succes;
        else
            yield return NodeResult.Failure;
        }
    }
