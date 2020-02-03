﻿namespace UniGreenModules.UniGameSystems.Examples.SimpleSystem
{
    using System.Diagnostics;
    using UniCore.Runtime.DataFlow.Interfaces;
    using UniCore.Runtime.Interfaces;
    using UniCore.Runtime.ProfilerTools;
    using UniGameSystem.Runtime;

    public class SimpleSystem1 : GameService
    {
        protected override IContext OnBind(IContext context, ILifeTime lifeTime = null)
        {
            context.Publish(this);
            isReady.Value = true;
            return context;
        }
        
    }
}
