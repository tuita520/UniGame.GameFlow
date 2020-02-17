﻿using UniGreenModules.UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.Nodes.Runtime.Nodes;

namespace UniGreenModules.UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.Examples.ContextNodes.ContextNodes
{
    using UniCore.Runtime.ProfilerTools;
    using UniCore.Runtime.Rx.Extensions;
    using UniNodeSystem.Runtime.Core;
    using UniRx;

    [CreateNodeMenu("Examples/ContextNodes/FloatContex","FloatContex")]
    public class FloatContexResourceNode : ContextNode
    {
        public float currentIntSummValue;
        public float lastIntValue;
        
        protected override void OnExecute()
        {
            Receive<float>().
                Do(x => GameLog.Log($"{ItemName} : VALUE {x}")).
                Do(x => lastIntValue        =  x).
                Do(x => currentIntSummValue += x).
                Subscribe().
                AddTo(LifeTime);
        }
    }
}