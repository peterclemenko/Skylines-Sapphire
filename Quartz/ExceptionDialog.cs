using UnityEngine;

namespace Quartz
{
    public class ExceptionDialog : MonoBehaviour
    {

        private readonly Rect _windowRect = new Rect(Screen.width*0.5f-256.0f, Screen.height*0.5f-128.0f, 512.0f, 256.0f);
        public string Message;

        public static ExceptionDialog CurrentDialog;

        public static void Show(string message)
        {
            if (CurrentDialog != null)
            {
                return;
            }

			var go = new GameObject("QuartzExceptionDialog");
            CurrentDialog = go.AddComponent<ExceptionDialog>();
            CurrentDialog.Message = message;
        }

        void OnGUI()
        {
			GUI.Window(512621, _windowRect, DrawWindow, "Quartz skin exception!");
        }

        void DrawWindow(int index)
        {
			GUILayout.Label("Quartz has encountered an error while processing a skin.");
            GUILayout.Label("This is most likely a bug in the skin itself which you should report to its author.");
            GUILayout.Label("Copy/ paste the text below in your support issue.");

            GUILayout.TextArea(Message, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Close"))
            {
                Destroy(gameObject);
            }

            GUILayout.EndHorizontal();
        }

    }

}
