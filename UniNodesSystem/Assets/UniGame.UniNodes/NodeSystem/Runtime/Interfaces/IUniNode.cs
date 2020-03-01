namespace UniGame.UniNodes.NodeSystem.Runtime.Interfaces
{
    using System.Collections.Generic;
    using UniGreenModules.UniStateMachine.Runtime.Interfaces;

    public interface IUniNode : 
        INode,
        IState
    {
        IReadOnlyCollection<INodePort> PortValues { get; }

        bool AddPortValue(INodePort portValue);

    }
}