﻿namespace UniGreenModules.UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.Nodes.Runtime.Nodes
{
    using System;
    using System.Collections.Generic;
    using NodeSystem.Runtime.Attributes;
    using NodeSystem.Runtime.Nodes;
    using UniCore.Runtime.Interfaces;
    using UniNodeSystem.Nodes.Commands;
    using UniNodeSystem.Runtime;

    [HideNode]
    [Serializable]
    public class TypeBridgeNode<TData> : UniNode
    {
        protected override void UpdateCommands(List<ILifeTimeCommand> nodeCommands)
        {
            base.UpdateCommands(nodeCommands);
            
            var portCommand = new ConnectedFormatedPairCommand();
            portCommand.Initialize(this,typeof(TData).Name,false);
            nodeCommands.Add(portCommand);

            var contextPortAction = new PortDataBridgeActionCommand<TData>(
                OnDataUpdated,
                portCommand.InputPort, 
                portCommand.OutputPort);
            nodeCommands.Add(contextPortAction);
        }

        /// <summary>
        /// process port value context data
        /// </summary>
        /// <param name="data">new data item from input port</param>
        /// <param name="source">source of data</param>
        /// <param name="target">target data output</param>
        protected virtual void OnDataUpdated(TData data,IContext source,IContext target)
        {
            target.Publish(data);
        }
    }
}