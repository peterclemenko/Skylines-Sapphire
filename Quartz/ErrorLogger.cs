using System;
using UnityEngine;

namespace Quartz
{
    public static class ErrorLogger
    {
        private static bool _foundModTools;
        private static bool _lookedForModTools;

        public static void ResetSettings()
        {
            _lookedForModTools = false;
            _foundModTools = false;
        }

        private static bool FoundModTools
        {
            get
            {
                if (!_lookedForModTools)
                {
                    _foundModTools = GameObject.Find("ModTools") != null;
                    _lookedForModTools = true;
                }

                return _foundModTools;
            }
        }

        public static void LogError(string message)
        {
            if (!FoundModTools)
            {
                ExceptionDialog.Show(message);
            }

            Debug.LogError(message);
        }

        public static void LogErrorFormat(string message, params object[] args)
        {
            if (!FoundModTools)
            {
                ExceptionDialog.Show(String.Format(message, args));
            }

            Debug.LogErrorFormat(message, args);
        }

    }
}
