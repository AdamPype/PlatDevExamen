using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//this is a parent class for sequence and selector node, because all these classes have the following in common:
// - they all require an array of nodes as constructor argument
public abstract class CompositeNode : INode
    {
    protected INode[] _nodes;

    public CompositeNode(params INode[] nodes)
        {
        _nodes = nodes;
        }

    public abstract IEnumerator<NodeResult> Tick();
    }