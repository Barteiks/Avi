using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Avi.Functions
{
    public class FunctionsManager
    {
        private readonly TaskManager _taskManager;
        private CmdFunction _cmdFunction;
        public event Action<string>? OnCmdCommandOutput;
        public FunctionsManager(TaskManager taskManager)
        {
            _taskManager = taskManager;
        }
        public void loadFunctions()
        {
            _cmdFunction = new CmdFunction(_taskManager);
            _cmdFunction.OnCmdCommandOutput += message =>
            {
                OnCmdCommandOutput?.Invoke(message);
            };
        }
    }
}
