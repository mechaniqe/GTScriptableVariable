﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace GTVariable.Editor
{

    public class ParameterizedGameEventEditor<ListenerType,GameEventType,EventType,ParameterType> : UnityEditor.Editor
        where EventType : UnityEngine.Events.UnityEvent<ParameterType>
        where GameEventType : ParameterizedGameEvent<IParameterizedListener<EventType, ParameterType>, EventType,ParameterType>
        where ListenerType : ParameterizedListener<GameEventType, EventType,ParameterType>
    {
        public ParameterType parameter;
        private List<ListenerType> gameEventListners = new List<ListenerType>();
        private List<bool> problems = new List<bool>();
        private GameEventType gameEvent;
        private int currentSelected = -1;
        private string currentSelectedName = "";
        private Vector2 responseScroll;
        private Vector2 subscirbersScroll;
        private GUIStyle errorStyle;

        private void OnEnable()
        {
            gameEvent = target as GameEventType;
            gameEventListners = FindGameEventListnerInScene();
            CheckForResponseProblems();
            errorStyle = new GUIStyle();
            errorStyle.padding = new RectOffset(0, 0, 3, 0);
            errorStyle.imagePosition = ImagePosition.ImageOnly;
        }

        public override void OnInspectorGUI()
        {
            DrawOptions();



            DrawSubscribers();
            EditorGUILayout.Space();
            if (currentSelected != -1 && currentSelected < gameEventListners.Count)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{gameEventListners[currentSelected].gameObject.name} - { currentSelectedName}");
                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = gameEventListners[currentSelected];
                }
                if (GUILayout.Button("Ping", EditorStyles.miniButtonMid))
                {
                    EditorGUIUtility.PingObject(gameEventListners[currentSelected]);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Response", EditorStyles.boldLabel);
                DrawResponse(gameEventListners[currentSelected].Response);
            }

        }


        private void DrawOptions()
        {
            EditorGUILayout.BeginHorizontal();

            bool enable = GUI.enabled;

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                gameEvent.Raise(parameter);
            }

            GUI.enabled = enable;

            if (GUILayout.Button("Find in scene"))
            {
                gameEventListners = FindGameEventListnerInScene();
                CheckForResponseProblems();
            }

            EditorGUILayout.EndHorizontal();
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("With:");
                DrawParameter();
            }
        }

        private void DrawResponse(EventType response)
        {

            var maxWidth = GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth) / 3 - 18);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target", maxWidth);
            EditorGUILayout.LabelField("Method name", maxWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            var eventCount = response.GetPersistentEventCount();

            responseScroll = EditorGUILayout.BeginScrollView(responseScroll, GUILayout.MaxHeight(200));
            for (int i = 0; i < eventCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var hasTarget = response.GetPersistentTarget(i) != null;

                if (hasTarget)
                {
                    EditorGUILayout.LabelField(response.GetPersistentTarget(i).name, EditorStyles.miniLabel, maxWidth);
                    EditorGUILayout.LabelField(response.GetPersistentMethodName(i), EditorStyles.miniLabel, maxWidth);
                    if (GUILayout.Button("Select", EditorStyles.miniButtonMid))
                    {
                        Selection.activeObject = response.GetPersistentTarget(i);
                    }
                    if (GUILayout.Button("Ping", EditorStyles.miniButtonMid))
                    {
                        EditorGUIUtility.PingObject(response.GetPersistentTarget(i));
                    }
                }
                else
                {
                    EditorGUI.HelpBox(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 50, EditorGUIUtility.singleLineHeight + 5), "NULL", MessageType.Error);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

       

        private void CheckForResponseProblems()
        {
            for (int i = 0; i < gameEventListners.Count; i++)
            {
                if (i >= problems.Count)
                {
                    problems.Add(CheckGameEventListener(gameEventListners[i]));
                }
                else
                {
                    problems[i] = CheckGameEventListener(gameEventListners[i]);
                }
            }
        }

        private bool CheckGameEventListener(ListenerType listener)
        {
            for (int i = 0; i < listener.Response.GetPersistentEventCount(); i++)
            {
                if (listener.Response.GetPersistentTarget(i) == null)
                    return true;
            }

            return false;
        }

        private List<ListenerType> FindGameEventListnerInScene()
        {
            return FindObjectsOfType<ListenerType>()
                    .Where(p => p.gameEvents.Where(c => c == target as GameEventType).ToArray().Length > 0).ToList();
        }


        private void DrawSubscribers()
        {
            GUILayout.Label("Subscirbers:", EditorStyles.boldLabel);
            subscirbersScroll = EditorGUILayout.BeginScrollView(subscirbersScroll);
            for (int i = 0; i < gameEventListners.Count; i++)
            {
                if (problems.Count <= i || problems[i] == false)
                {
                    DrawSubscriber(i);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.erroricon.sml"), errorStyle, GUILayout.MaxHeight(20), GUILayout.MaxWidth(30));
                    DrawSubscriber(i);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSubscriber(int id)
        {
            var name = gameEventListners[id].name;
            if (string.IsNullOrEmpty(name)) name = "[No name specify]";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(gameEventListners[id].gameObject.name, GUILayout.MaxWidth(120));
            if (GUILayout.Button(name))
            {
                currentSelected = id;
            }
            if (id == currentSelected) currentSelectedName = name;
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawParameter() { }
    }
}
