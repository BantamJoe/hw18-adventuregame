﻿using System;
using System.Collections;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class SetAnimBooleanNode
    {
        [Serializable]
        public class SetAnimBooleanNodeSettings
        {
            public string m_AnimParameterName;
            public bool   m_Value;
        }

        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            SetAnimBooleanNodeSettings settings = JsonUtility.FromJson<SetAnimBooleanNodeSettings>(currentNode.m_typeData);
            SceneManager.Instance.Character.Animator.SetBool(settings.m_AnimParameterName, settings.m_Value);
            yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Set Anim Boolean";

	        node.mainContainer.style.backgroundColor = Color.blue;

			node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            Port outputPort1 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort1.portName = "";
            outputPort1.userData = null;
            node.outputContainer.Add(outputPort1);

            SetAnimBooleanNodeSettings settings = JsonUtility.FromJson<SetAnimBooleanNodeSettings>(typeData);
            if (settings == null)
            {
                settings = new SetAnimBooleanNodeSettings();
            }

            var animParameterTextField = new TextField()
            {
                multiline = false,
                value = settings.m_AnimParameterName
            };
            node.mainContainer.Insert(1, animParameterTextField);

            var animParameterValueToggle = new Toggle()
            {
                value = settings.m_Value
            };
            node.mainContainer.Insert(2, animParameterValueToggle);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            SetAnimBooleanNodeSettings settings = new SetAnimBooleanNodeSettings();
            
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is TextField)
                {
                    settings.m_AnimParameterName = (ele as TextField).value;
                }
                else if (ele is Toggle)
                {
                    settings.m_Value = (ele as Toggle).value;
                }
            }

            return JsonUtility.ToJson(settings);
        }
#endif
    }
}