using UnityEditor;
using UnityEngine;

namespace CardFramework.Presentation.Views {
    [CustomEditor(typeof(CardGamesRoomIntroData))]
    public class CardGamesRoomIntroDataEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset Data")) {
                var generator = target as CardGamesRoomIntroData;
                if (generator != null) {
                    generator.Reset();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
