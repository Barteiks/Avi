using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.Services
{
    public interface IPlatformPathService
    {
        string GetModelDirectory();
        string GetLogsDirectory();
    }
    
}
