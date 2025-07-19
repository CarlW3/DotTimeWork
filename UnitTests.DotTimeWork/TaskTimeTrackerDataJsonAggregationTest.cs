using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using Moq;
using System.Text.Json;

namespace UnitTests.DotTimeWork
{
    public class TaskTimeTrackerDataJsonAggregationTest : IDisposable
    {
        private readonly string _testDirectory;
        private readonly TaskTimeTrackerDataJson _dataProvider;
        private readonly Mock<IInputAndOutputService> _mockInputOutputService;
        private readonly Mock<IDeveloperConfigController> _mockDeveloperController;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        public TaskTimeTrackerDataJsonAggregationTest()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DotTimeWorkTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            _mockInputOutputService = new Mock<IInputAndOutputService>();
            _mockDeveloperController = new Mock<IDeveloperConfigController>();
            
            _dataProvider = new TaskTimeTrackerDataJson(_mockInputOutputService.Object, _mockDeveloperController.Object);
            _dataProvider.SetStoragePath(_testDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void LoadAggregatedTaskData_SingleDeveloper_ReturnsCorrectTask()
        {
            // Arrange
            var developer = "Alice";
            SetupCurrentDeveloper(developer);
            
            var task = CreateTestTask("Task1", developer, 60);
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "task1", task } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            Assert.Single(result);
            Assert.Contains("task1", result.Keys);
            Assert.Equal("Task1", result["task1"].Name);
            Assert.Equal(60, result["task1"].DeveloperWorkTimes[developer]);
        }

        [Fact]
        public void LoadAggregatedTaskData_MultipleDevelopers_MergesTasksCorrectly()
        {
            // Arrange
            var developer1 = "Alice";
            var developer2 = "Bob";
            SetupCurrentDeveloper(developer1);

            var task1 = CreateTestTask("SharedTask", developer1, 60);
            var task2 = CreateTestTask("SharedTask", developer2, 45);
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "sharedtask", task1 } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "sharedtask", task2 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            Assert.Single(result);
            Assert.Contains("sharedtask", result.Keys);
            
            var mergedTask = result["sharedtask"];
            Assert.Equal("SharedTask", mergedTask.Name);
            Assert.Equal(2, mergedTask.DeveloperWorkTimes.Count);
            Assert.Equal(60, mergedTask.DeveloperWorkTimes[developer1]);
            Assert.Equal(45, mergedTask.DeveloperWorkTimes[developer2]);
            Assert.Equal(105, mergedTask.GetTotalWorkTime());
        }

        [Fact]
        public void LoadAggregatedTaskData_MultipleDevelopers_MergesCommentsCorrectly()
        {
            // Arrange
            var developer1 = "Alice";
            var developer2 = "Bob";
            SetupCurrentDeveloper(developer1);

            var task1 = CreateTestTask("TaskWithComments", developer1, 60);
            task1.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-2), Developer = developer1, Comment = "Started work" });
            
            var task2 = CreateTestTask("TaskWithComments", developer2, 45);
            task2.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-1), Developer = developer2, Comment = "Continued work" });
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "taskwithcomments", task1 } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "taskwithcomments", task2 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            var mergedTask = result["taskwithcomments"];
            Assert.Equal(2, mergedTask.Comments.Count);
            Assert.Contains(mergedTask.Comments, c => c.Developer == developer1 && c.Comment == "Started work");
            Assert.Contains(mergedTask.Comments, c => c.Developer == developer2 && c.Comment == "Continued work");
        }

        [Fact]
        public void LoadAggregatedTaskData_SameDeveloperMultipleWorkSessions_SumsWorkTime()
        {
            // Arrange
            var developer = "Alice";
            SetupCurrentDeveloper(developer);

            var task1 = CreateTestTask("TaskWithMultipleSessions", developer, 60);
            var task2 = CreateTestTask("TaskWithMultipleSessions", developer, 30);
            task2.DeveloperWorkTimes[developer] = 30; // Second session
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "taskwithmultiplesessions", task1 } });
            
            // Simulate second session by updating the same task
            task1.AddOrUpdateWorkTime(developer, 30);
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "taskwithmultiplesessions", task1 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            var task = result["taskwithmultiplesessions"];
            Assert.Equal(90, task.DeveloperWorkTimes[developer]);
        }

        [Fact]
        public void LoadAggregatedTaskData_EmptyFiles_IgnoresEmptyFiles()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            // Create empty file
            File.WriteAllText(Path.Combine(_testDirectory, "taskStartTimeData.alice.json"), "");
            
            var task = CreateTestTask("ValidTask", "Bob", 60);
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "validtask", task } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            Assert.Single(result);
            Assert.Contains("validtask", result.Keys);
        }

        [Fact]
        public void LoadAggregatedTaskData_InvalidJsonFiles_IgnoresInvalidFiles()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            // Create invalid JSON file
            File.WriteAllText(Path.Combine(_testDirectory, "taskStartTimeData.alice.json"), "{ invalid json");
            
            var task = CreateTestTask("ValidTask", "Bob", 60);
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "validtask", task } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            Assert.Single(result);
            Assert.Contains("validtask", result.Keys);
        }

        [Fact]
        public void AddTask_AlwaysSavesToCurrentDeveloper()
        {
            // Arrange
            var currentDeveloper = "Alice";
            SetupCurrentDeveloper(currentDeveloper);
            
            var task = CreateTestTask("NewTask", currentDeveloper, 0);

            // Act
            _dataProvider.AddTask(task);

            // Assert
            var filePath = Path.Combine(_testDirectory, "taskStartTimeData.alice.json");
            Assert.True(File.Exists(filePath));
            
            var savedTasks = LoadTasksFromFile(filePath);
            Assert.Contains("newtask", savedTasks.Keys);
            Assert.Equal(currentDeveloper, savedTasks["newtask"].CreatedBy);
        }

        [Fact]
        public void AddTask_TaskAlreadyExists_DoesNotAdd()
        {
            // Arrange
            var developer = "Alice";
            SetupCurrentDeveloper(developer);
            
            var existingTask = CreateTestTask("ExistingTask", developer, 60);
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "existingtask", existingTask } });
            
            var newTask = CreateTestTask("ExistingTask", developer, 30);

            // Act
            _dataProvider.AddTask(newTask);

            // Assert
            var result = _dataProvider.RunningTasks;
            Assert.Single(result);
            Assert.Equal(60, result["existingtask"].DeveloperWorkTimes[developer]); // Should remain unchanged
        }

        [Fact]
        public void AddFocusTimeForTask_UpdatesWorkTimeAndSavesToCurrentDeveloper()
        {
            // Arrange
            var developer = "Alice";
            SetupCurrentDeveloper(developer);
            
            var task = CreateTestTask("FocusTask", developer, 30);
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "focustask", task } });

            // Act
            _dataProvider.AddFocusTimeForTask("FocusTask", 25, developer);

            // Assert
            var result = _dataProvider.RunningTasks;
            Assert.Equal(55, result["focustask"].DeveloperWorkTimes[developer]);
            
            // Verify it's saved to current developer's file
            var filePath = Path.Combine(_testDirectory, "taskStartTimeData.alice.json");
            var savedTasks = LoadTasksFromFile(filePath);
            Assert.Equal(55, savedTasks["focustask"].DeveloperWorkTimes[developer]);
        }

        [Fact]
        public void SetTaskAsFinished_MovesTaskFromRunningToFinished()
        {
            // Arrange
            var developer = "Alice";
            SetupCurrentDeveloper(developer);
            
            var task = CreateTestTask("TaskToFinish", developer, 60);
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "tasktofinish", task } });

            // Get the actual task instance from the running tasks (after aggregation)
            var runningTask = _dataProvider.GetRunningTaskById("TaskToFinish");
            Assert.NotNull(runningTask);

            // Act
            var success = _dataProvider.SetTaskAsFinished(runningTask);

            // Assert
            Assert.True(success);
            
            // Should be removed from running tasks
            var runningTasks = _dataProvider.RunningTasks;
            Assert.DoesNotContain("tasktofinish", runningTasks.Keys);
            
            // Should be added to finished tasks
            var finishedTasks = _dataProvider.FinishedTasks;
            Assert.Contains("tasktofinish", finishedTasks.Keys);
            Assert.True(finishedTasks["tasktofinish"].Finished != default);
        }

        [Fact]
        public void GetAllRunningTasksForAllDevelopers_ReturnsAggregatedList()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            var task1 = CreateTestTask("Task1", "Alice", 60);
            var task2 = CreateTestTask("Task2", "Bob", 45);
            var sharedTask1 = CreateTestTask("SharedTask", "Alice", 30);
            var sharedTask2 = CreateTestTask("SharedTask", "Bob", 25);
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> 
            { 
                { "task1", task1 },
                { "sharedtask", sharedTask1 }
            });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> 
            { 
                { "task2", task2 },
                { "sharedtask", sharedTask2 }
            });

            // Act
            var result = _dataProvider.GetAllRunningTasksForAllDevelopers();

            // Assert
            Assert.Equal(3, result.Count); // Task1, Task2, and merged SharedTask
            
            var sharedTask = result.First(t => t.Name == "SharedTask");
            Assert.Equal(2, sharedTask.DeveloperWorkTimes.Count);
            Assert.Equal(55, sharedTask.GetTotalWorkTime());
        }

        // Helper methods

        private void SetupCurrentDeveloper(string developerName)
        {
            var config = new DeveloperConfig { Name = developerName };
            _mockDeveloperController.Setup(x => x.CurrentDeveloperConfig).Returns(config);
        }

        private TaskData CreateTestTask(string name, string developer, int workMinutes)
        {
            var task = new TaskData
            {
                Name = name,
                Description = $"Test task {name}",
                Started = DateTime.Now.AddHours(-2),
                CreatedBy = developer
            };
            
            if (workMinutes > 0)
            {
                task.AddOrUpdateWorkTime(developer, workMinutes);
            }
            
            return task;
        }

        private void SaveTaskToFile(string fileName, Dictionary<string, TaskData> tasks)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            var json = JsonSerializer.Serialize(tasks, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private Dictionary<string, TaskData> LoadTasksFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new Dictionary<string, TaskData>();

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json, JsonOptions) 
                   ?? new Dictionary<string, TaskData>();
        }
    }
}
