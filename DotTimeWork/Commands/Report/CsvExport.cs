using DotTimeWork.Helper;
using DotTimeWork.TimeTracker;

namespace DotTimeWork.Commands.Report
{
    internal class CsvExport
    {
        private const string SEPARATOR = ";";
        private readonly ITaskTimeTracker _taskTimeTracker;

        public CsvExport(ITaskTimeTracker taskTimeTracker)
        {
            _taskTimeTracker = taskTimeTracker;
        }

        public bool ExecuteCsvReport(string outputFile)
        {
            try
            {
                using (var writer = new StreamWriter(outputFile))
                {
                    GenerateTableTasks(writer);
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Error generating CSV report: {ex.Message}");
                return false;
            }
        }


        private void GenerateTableTasks(StreamWriter writer)
        {
            DateTime dateTime = DateTime.Now;
            // Aufgaben in die Tabelle einfügen
            writer.WriteLine("State"+SEPARATOR+"Name"+SEPARATOR+"Started"+SEPARATOR+"Ended"+SEPARATOR+"Working Time"+SEPARATOR+"Focus Working"+SEPARATOR+"Developer");
            foreach (var task in _taskTimeTracker.GetAllRunningTasks())
            {
                writer.WriteLine("Active"+SEPARATOR+task.Name+SEPARATOR+task.Started+SEPARATOR+"N/A"+SEPARATOR + TimeHelper.GetWorkingTimeHumanReadable(dateTime - task.Started)+SEPARATOR+task.FocusWorkTime+SEPARATOR+task.Developer);
            }
            foreach (var task in _taskTimeTracker.GetAllFinishedTasks())
            {
                writer.WriteLine("Done" + SEPARATOR + task.Name + SEPARATOR + task.Started + SEPARATOR + task.Finished + SEPARATOR + TimeHelper.GetWorkingTimeHumanReadable(task.Finished - task.Started) + SEPARATOR + task.FocusWorkTime + SEPARATOR + task.Developer);
            }
        }
    }
}
