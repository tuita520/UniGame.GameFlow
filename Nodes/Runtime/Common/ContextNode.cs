﻿namespace UniGame.UniNodes.Nodes.Runtime.Common
{
    using System;
    using System.Collections.Generic;
    using NodeSystem.Runtime.Attributes;
    using NodeSystem.Runtime.Core;
    using NodeSystem.Runtime.Interfaces;
    using UniModules.UniCore.Runtime.Rx.Extensions;
    using UniModules.UniGame.Core.Runtime.Interfaces;
    using UniModules.UniGame.Core.Runtime.Interfaces.Rx;
    using UniRx;

    [Serializable]
    [HideNode]
    public class ContextNode : UniNode,
        IReadonlyRecycleReactiveProperty<IContext>, 
        IMessageBroker
    {
        private SContextNode contextNode;
        

        public IDisposable Subscribe(IObserver<IContext> observer) => 
            contextNode.Subscribe(observer);

        public IContext Value => contextNode.Value;
        
        public bool HasValue => contextNode.HasValue;

        public void Complete() => contextNode.Complete();
        
        public void Publish<T>(T message) => contextNode.Publish(message);

        public IObservable<T> Receive<T>() => contextNode.Receive<T>();
        
        public IReadOnlyReactiveProperty<IContext> Source => contextNode.Source;
        
        protected override IProxyNode CreateInnerNode()
        {  
            contextNode = new SContextNode(id, nodeName, ports);
            return contextNode;
        }
        
        protected override void UpdateCommands(List<ILifeTimeCommand> nodeCommands)
        {
            base.UpdateCommands(nodeCommands);
                        
            Source.Where(x => x!=null).
                Do(OnContextActivate).
                Subscribe().
                AddTo(LifeTime);
        }

        protected virtual void OnContextActivate(IContext context) { }
    }
}
