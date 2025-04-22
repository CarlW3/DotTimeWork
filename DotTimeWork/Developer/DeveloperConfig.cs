using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTimeWork.Developer
{
    public class DeveloperConfig
    {
        public string Name { get; set; }
        public string E_Mail { get; set; }

        public int HoursPerDayWork { get; set; } = 8;

        public List<string> StartedTasks { get; set; } = new List<string>();

        public void AddStartedTask(string taskId)
        {
            StartedTasks.Add(taskId);
        }
    }
}
