﻿namespace UniGreenModules.UniGameFlow.UniNodesSystem.Assets.UniGame.UniNodes.NodeSystem.Runtime.Nodes
{
    using System;
    using System.Collections.Generic;
    using Attributes;
    using UniCore.Runtime.DataFlow;
    using UniCore.Runtime.DataFlow.Interfaces;
    using UniCore.Runtime.Extension;
    using UniCore.Runtime.Interfaces;
    using UniCore.Runtime.ProfilerTools;
    using UniNodeSystem.Runtime.Commands;
    using UniNodeSystem.Runtime.Core;
    using UniNodeSystem.Runtime.Extensions;
    using UniNodeSystem.Runtime.Interfaces;
    using UnityEngine;

    [HideNode]
    [Serializable]
    public abstract class UniNode : UniBaseNode, IUniNode
    {       
        #region inspector fields

        [SerializeField]
        public List<SerializedNodeCommand> nodeSavedCommands = new List<SerializedNodeCommand>();

        #endregion
        
        #region private fields

        [NonSerialized] public List<ILifeTimeCommand> commands = 
            new List<ILifeTimeCommand>();

        [NonSerialized] public List<IPortValue> portValues = 
            new List<IPortValue>();
        
        [NonSerialized] private LifeTimeDefinition lifeTimeDefinition = 
            new LifeTimeDefinition();

        [NonSerialized] private Dictionary<string, IPortValue> portValuesMap = 
            new Dictionary<string, IPortValue>();

        [NonSerialized] private bool isInitialized;

        [NonSerialized] private bool isActive = false;

        #endregion

        #region public properties

        /// <summary>
        /// Is node currently active
        /// </summary>
        public bool IsActive => isActive;

        public IReadOnlyList<IPortValue> PortValues => portValues;

        public ILifeTime LifeTime => lifeTimeDefinition.LifeTime;

        public string ItemName => name;

        #endregion

        #region public methods

        public void Initialize()
        {
            //check initialization status
            if (isInitialized)
                return;

            isInitialized = true;
            
            //custom node initialization
            OnInitialize();

            //initialize all node commandss
            InitializeCommands();
            
            //remove deleted ports
            Ports.RemoveItems(this.IsPortRemoved, RemoveInstancePort);
        }

        /// <summary>
        /// stop execution
        /// </summary>
        public void Exit()
        {
            isActive = false;
            lifeTimeDefinition.Terminate();
        }

        /// <summary>
        /// start node execution
        /// </summary>
        public void Execute()
        {
            //node already active
            if (isActive) {
                return;
            }

            //mark as active
            isActive = true;
            
            //restart lifetime
            lifeTimeDefinition.Release();

            //initialize
            Initialize();

            //cleanup ports on exit
            LifeTime.AddCleanUpAction(CleanUpPorts);

            //execute all node commands
            commands.ForEach(x => x.Execute(LifeTime));
            
            //user defined logic
            OnExecute();
        }

        /// <summary>
        /// stop node execution
        /// </summary>
        public void Release() => Exit();

        #region Node Ports operations

        public override object GetValue(NodePort port) => GetPortValue(port);

        public IPortValue GetPortValue(INodePort port) => GetPortValue(port.fieldName);

        public IPortValue GetPortValue(string portName)
        {
            portValuesMap.TryGetValue(portName, out var value);
            return value;
        }

        public bool AddPortValue(IPortValue portValue)
        {
            if (portValue == null) {
                GameLog.LogErrorFormat("Try add NULL port value to {0}", this);
                return false;
            }

            if (portValuesMap.ContainsKey(portValue.ItemName)) {
                return false;
            }

            portValuesMap[portValue.ItemName] = portValue;
            portValues.Add(portValue);

            return true;
        }
        
        #endregion

        #endregion

        /// <summary>
        /// Call once on node initialization
        /// </summary>
        protected virtual void OnInitialize(){}

        /// <summary>
        /// base logic realization
        /// </summary>
        protected virtual void OnExecute(){}

        /// <summary>
        /// update active list commands
        /// add all supported node commands here
        /// </summary>
        protected virtual void UpdateCommands(List<ILifeTimeCommand> nodeCommands){}
        
        /// <summary>
        /// cleanup all ports values
        /// </summary>
        private void CleanUpPorts()
        {
            for (var i = 0; i < PortValues.Count; i++) {
                var portValue = PortValues[i];
                portValue.CleanUp();
            }
        }

        private void InitializeCommands()
        {
            commands.Clear();
            
            //register all backed commands to main list
            for (var i = 0; i < nodeSavedCommands.Count; i++) {
                var command = nodeSavedCommands[i];
                if(command!=null) 
                    commands.Add(command.Create(this));
            }
            
            //register node commands
            UpdateCommands(commands);
        }
        
        private void OnDestroy() => Exit();
        
#region inspector call
        
        protected virtual void OnValidate()
        {
            if(Application.isPlaying == false)
                Initialize();
        }
        
#endregion
    }
}