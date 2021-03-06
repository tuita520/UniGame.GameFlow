﻿namespace UniGame.UniNodes.NodeSystem.Runtime.Core
{
    using Interfaces;
    using Runtime.Interfaces;
    using UniModules.UniGame.Core.Runtime.Interfaces;

    public interface IGraphData : INamedItem, IUnique
    {
        int GetId();
        
        int UpdateId(int oldId);
        
        INode GetNode(int nodeId);

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"> The node to remove </param>
        IGraphData RemoveNode(INode node);

        IContext Context { get; }
    }
}