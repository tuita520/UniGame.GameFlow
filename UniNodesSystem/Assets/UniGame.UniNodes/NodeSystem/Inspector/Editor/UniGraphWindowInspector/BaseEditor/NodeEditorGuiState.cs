﻿namespace UniGreenModules.UniNodeSystem.Inspector.Editor.BaseEditor
{
    using System.Collections.Generic;
    using UnityEngine;

    public struct NodeEditorGuiState
    {
        public Event        Event;
        public Vector2      MousePosition;
        public List<Object> PreSelection;
        public EventType    EventType;
    }
}