using System.IO;
using UnityEditor;
using UnityEngine;

namespace CardFramework.Presentation.Views {
    [CustomEditor(typeof(CardFaceGenerator))]
    public class CardFaceGeneratorEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Save Texture")) {
                SaveTextureMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SaveTextureMenu() {
            var generator = target as CardFaceGenerator;
            if (generator == null) return;

            var defaultFolder = Path.Combine(Application.dataPath, "Project/Art/Textures/Cards");
            string folder = RequestSaveFolder(defaultFolder);
            if (string.IsNullOrEmpty(folder)) return;

            generator.SaveTextureToFolder(folder);
            AssetDatabase.Refresh();
        }

        public static string RequestSaveFolder(string defaultFolder) {
            if (Application.isBatchMode || IsRunningInTestContext()) {
                return string.Empty;
            }

            return EditorUtility.SaveFolderPanel("Select folder to save card texture", defaultFolder, "");
        }

        private static bool IsRunningInTestContext() {
            var stackTrace = new System.Diagnostics.StackTrace();
            foreach (var frame in stackTrace.GetFrames() ?? System.Array.Empty<System.Diagnostics.StackFrame>()) {
                var method = frame?.GetMethod();
                var declaringType = method?.DeclaringType;
                if (declaringType == null) continue;

                var assemblyName = declaringType.Assembly.GetName().Name ?? string.Empty;
                if (assemblyName.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) >= 0) {
                    return true;
                }
            }

            return false;
        }
    }
}
