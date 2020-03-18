﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniGame.UniNodes.NodeSystem.Runtime.Attributes;
using UniGame.UniNodes.NodeSystem.Runtime.Core.Interfaces;
using UniGame.UniNodes.NodeSystem.Runtime.Interfaces;
using UniGreenModules.UniCore.Runtime.DataFlow;
using UniGreenModules.UniCore.Runtime.DataFlow.Interfaces;
using UniGreenModules.UniCore.Runtime.Interfaces;
using UniGreenModules.UniCore.Runtime.ObjectPool.Runtime;
using UniGreenModules.UniCore.Runtime.ObjectPool.Runtime.Extensions;
using UniGreenModules.UniCore.Runtime.ProfilerTools;
using UnityEngine;

namespace UniGame.UniNodes.NodeSystem.Runtime.Core.Nodes
{
    using UniCore.Runtime.ProfilerTools;

    [Serializable]
    public class SNode : SerializableNode, IProxyNode
    {
        #region inspector fields

        [HideNodeInspector]
        [SerializeReference]
        public List<ILifeTimeCommandSource> serializableCommands = new List<ILifeTimeCommandSource>();

        #endregion

        #region private fields
        
        private Action                         onInitialize;
        private Action<List<ILifeTimeCommand>> onCommandsInitialize;
        private Action                         onExecute;

        [NonSerialized] private bool isInitialized = false;

        private LifeTimeDefinition lifeTimeDefinition = new LifeTimeDefinition();

        private bool isActive = false;

        private ILifeTime lifeTime;

        private HashSet<INodePort> portValues;

        private List<ILifeTimeCommand> commands;

        #endregion


        #region constructor

        public SNode(){}

        public SNode(
            int id,
            string name,
            NodePortDictionary ports) : base(id, name, ports){}

        #endregion

        #region public properties

        /// <summary>
        /// Is node currently active
        /// </summary>
        public bool IsActive => isActive;

        public ILifeTime LifeTime => lifeTimeDefinition.LifeTime;

        public IReadOnlyCollection<INodePort> PortValues => portValues;

        #endregion

        #region public methods

        public void Bind(
            Action initializeAction = null,
            Action<List<ILifeTimeCommand>> initializeCommands = null,
            Action executeAction = null)
        {
            this.onInitialize         = initializeAction;
            this.onCommandsInitialize = initializeCommands;
            this.onExecute            = executeAction;
        }
        
        public sealed override void Initialize(IGraphData graphData)
        {
            base.Initialize(graphData);
            
            InitializeData(graphData);
            
            if (Application.isEditor && Application.isPlaying == false) {
                lifeTimeDefinition.Terminate();
            }

            if (Application.isPlaying && isInitialized)
                return;

            isInitialized = true;

            //initialize ports
            InitializePorts();
            //initialize all node commands
            InitializeCommands();
            //custom node initialization
            OnInitialize();
            //proxy outer initialization
            onInitialize?.Invoke();
            
            lifeTime.AddCleanUpAction(() => {
                isActive = false;
                this.onInitialize         = null;
                this.onCommandsInitialize = null;
                this.onExecute            = null;
            });
        }

        public bool AddPortValue(INodePort portValue)
        {
            if (portValue == null) {
                GameLog.LogErrorFormat("Try add NULL port value to {0}", this);
                return false;
            }

            portValues.Add(portValue);

            return true;
        }

        /// <summary>
        /// stop execution
        /// </summary>
        public void Exit() => lifeTimeDefinition.Terminate();

        /// <summary>
        /// start node execution
        /// </summary>
        public void Execute()
        {
            //node already active
            if (isActive) {
                return;
            }

            //initialize
            Initialize(graph);
            //mark as active
            isActive = true;
            //execute all node commands
            commands.ForEach(x => x.Execute(LifeTime));
            //user defined logic
            OnExecute();
            //proxy outer execution
            onExecute?.Invoke();
        }

        /// <summary>
        /// stop node execution
        /// </summary>
        public void Release() => Exit();

        
        #region inspector call

        public override void Validate()
        {
            var removedPorts = ClassPool.Spawn<List<NodePort>>();
            foreach (var portPair in ports) {
                var port = portPair.Value;
                if (port == null || string.IsNullOrEmpty(port.fieldName) || port.nodeId == 0) {
                    removedPorts.Add(port);
                    continue;
                }

                var value = PortValues.FirstOrDefault(x => x.ItemName == port.ItemName &&
                                                           x.Direction == port.Direction);
                if (value == null || string.IsNullOrEmpty(value.ItemName)) {
                    removedPorts.Add(port);
                }
            }

            removedPorts.ForEach(RemovePort);
            removedPorts.Despawn();

            for (int i = 0; i < Ports.Count; i++) {
                var port = Ports[i];
                port.nodeId = id;
                port.node   = this;
                port.Validate();
            }

            CleanUpSerializableCommands();
        }

        [Conditional("UNITY_EDITOR")]
        public void CleanUpSerializableCommands()
        {
            //remove all temp commands
            serializableCommands.RemoveAll(x => x == null || x.Validate() == false);
        }

        #endregion
        
        #endregion

        /// <summary>
        /// Call once on node initialization
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// base logic realization
        /// </summary>
        protected virtual void OnExecute()
        {
        }

        /// <summary>
        /// update active list commands
        /// add all supported node commands here
        /// </summary>
        protected virtual void UpdateCommands(List<ILifeTimeCommand> nodeCommands)
        {
        }

        #region private methods
        
        /// <summary>
        /// Initialize all node commands
        /// create port and bind them
        /// </summary>
        private void InitializeCommands()
        {
            commands.Clear();

            //register all backed commands to main list
            for (var i = 0; i < serializableCommands.Count; i++) {
                var command = serializableCommands[i];
                if (command != null)
                    commands.Add(command.Create(this));
            }

            //register node commands
            UpdateCommands(commands);
            
            //outer node commands
            onCommandsInitialize?.Invoke(commands);
        }

        /// <summary>
        /// initialize ports before execution
        /// </summary>
        private void InitializePorts()
        {
            //initialize ports
            for (var i = 0; i < Ports.Count; i++) {
                var nodePort = Ports[i];
                nodePort.Initialize(this);
                lifeTime.AddCleanUpAction(nodePort.Release);

                if (Application.isPlaying)
                    AddPortValue(nodePort);
            }
        }

        private void InitializeData(IGraphData graphData)
        {
            graph = graphData;
            //restart lifetime
            lifeTimeDefinition = lifeTimeDefinition ?? new LifeTimeDefinition();
            lifeTime           = lifeTimeDefinition.LifeTime;
            portValues         = portValues ?? new HashSet<INodePort>();
            commands           = commands ?? new List<ILifeTimeCommand>();
            
            portValues.Clear();
        }
        
        #endregion

    }
}