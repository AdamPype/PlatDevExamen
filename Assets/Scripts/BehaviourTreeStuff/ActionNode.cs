using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//executes given action
public class ActionNode : INode
    {
    public delegate IEnumerator<NodeResult> Action();
    public readonly Action _action;

    //constructor
    public ActionNode(Action action)
        {
        _action = action;
        }

    public IEnumerator<NodeResult> Tick()
        {
        return _action();
        }
    }
