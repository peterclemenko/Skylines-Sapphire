using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Quartz
{
    public class FileWatcher 
    {

        public delegate void OnFileChanged(string path);

        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        private readonly string _rootPath;

        private readonly List<string> _changedFiles = new List<string>();
        private readonly object _changedFilesLock = new object();

        public FileWatcher(string rootPath)
        {
            _rootPath = rootPath;
        }

        public void WatchFile(string relativeFilePath)
        {
            var filePath = Path.Combine(_rootPath, relativeFilePath);

            try
            {
                var path = Path.GetDirectoryName(filePath);
                Debug.LogWarning("adding watcher for path " + path);

                var watcher = new FileSystemWatcher
	                              {
		                              Path = path,
		                              NotifyFilter = NotifyFilters.Attributes |
		                                             NotifyFilters.CreationTime |
		                                             NotifyFilters.FileName |
		                                             NotifyFilters.LastAccess |
		                                             NotifyFilters.LastWrite |
		                                             NotifyFilters.Size |
		                                             NotifyFilters.Security,
		                              Filter = Path.GetFileName(filePath)
	                              };

	            watcher.Changed += (sender, args) =>
                {   
                    lock (_changedFilesLock)
                    {
                        _changedFiles.Add(args.FullPath);
                    } 
                };

                watcher.EnableRaisingEvents = true;
                _watchers.Add(filePath, watcher);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to add watcher for file \"" + filePath + "\": " + ex);
            }
        }

        public void Dispose()
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _watchers.Clear();
        }

        public bool CheckForAnyChanges(bool discardChanged = true)
        {
            bool result;

            lock (_changedFilesLock)
            {
                result = _changedFiles.Any();
                if (discardChanged)
                {
                    _changedFiles.Clear();
                }
            }

            return result;
        }

    }

}
