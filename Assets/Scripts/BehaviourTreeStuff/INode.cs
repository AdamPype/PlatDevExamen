using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface INode
    {
    IEnumerator<NodeResult> Tick();
    }
