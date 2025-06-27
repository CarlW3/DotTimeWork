using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using Moq;
using System.Text.Json;

namespace UnitTests.DotTimeWork
{
    public class TaskTimeTrackerDataJsonPerformanceTest : IDisposable
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

        public TaskTimeTrackerDataJsonPerformanceTest()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DotTimeWorkPerfTests", Guid.NewGuid().ToString());
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
        public void LoadAggregatedTaskData_LargeNumberOfDevelopers_PerformanceTest()
        {
            // Arrange
            const int developerCount = 50;
            const int tasksPerDeveloper = 20;
            
            SetupCurrentDeveloper("TestDev");
            CreateLargeDataSet(developerCount, tasksPerDeveloper);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _dataProvider.RunningTasks;
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Loading took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
            Assert.True(result.Count > 0);
            
            // Verify some tasks were merged correctly
            var mergedTasks = result.Values.Where(t => t.DeveloperWorkTimes.Count > 1).ToList();
            Assert.True(mergedTasks.Count > 0, "Expected some tasks to be merged from multiple developers");
        }

        [Fact]
        public void LoadAggregatedTaskData_ManyTasksWithComments_HandlesCommentsEfficiently()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            const int taskCount = 10;
            const int commentsPerTask = 50;
            
            CreateTasksWithManyComments(taskCount, commentsPerTask);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _dataProvider.RunningTasks;
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Loading took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
            Assert.Equal(taskCount, result.Count);
            
            // Verify comments are preserved
            foreach (var task in result.Values)
            {
                Assert.True(task.Comments.Count >= commentsPerTask);
            }
        }

        [Fact]
        public void LoadAggregatedTaskData_CaseInsensitiveTaskNames_MergesCorrectly()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            var task1 = CreateTestTask("MyTask", "Alice", 30);
            var task2 = CreateTestTask("MYTASK", "Bob", 45);
            var task3 = CreateTestTask("mytask", "Charlie", 25);
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "mytask", task1 } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "MYTASK", task2 } });
            SaveTaskToFile("taskStartTimeData.charlie.json", new Dictionary<string, TaskData> { { "MyTask", task3 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            Assert.Single(result);
            var mergedTask = result.Values.First();
            Assert.Equal(3, mergedTask.DeveloperWorkTimes.Count);
            Assert.Equal(100, mergedTask.GetTotalWorkTime());
        }

        [Fact]
        public void LoadAggregatedTaskData_DuplicateComments_DeduplicatesCorrectly()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            var commonTimestamp = DateTime.Now;
            var task1 = CreateTestTask("TaskWithDuplicates", "Alice", 30);
            task1.Comments.Add(new TaskComment 
            { 
                Created = commonTimestamp, 
                Developer = "Alice", 
                Comment = "Same comment" 
            });
            
            var task2 = CreateTestTask("TaskWithDuplicates", "Bob", 45);
            task2.Comments.Add(new TaskComment 
            { 
                Created = commonTimestamp, 
                Developer = "Alice", 
                Comment = "Same comment" 
            }); // Exact duplicate
            task2.Comments.Add(new TaskComment 
            { 
                Created = commonTimestamp, 
                Developer = "Bob", 
                Comment = "Different comment" 
            });
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "taskwithduplicates", task1 } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "taskwithduplicates", task2 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            var mergedTask = result["taskwithduplicates"];
            Assert.Equal(2, mergedTask.Comments.Count); // Should deduplicate the identical comment
            Assert.Contains(mergedTask.Comments, c => c.Developer == "Alice" && c.Comment == "Same comment");
            Assert.Contains(mergedTask.Comments, c => c.Developer == "Bob" && c.Comment == "Different comment");
        }

        [Fact]
        public void LoadAggregatedTaskData_NoStoragePath_ReturnsEmptyDictionary()
        {
            // Arrange
            var dataProvider = new TaskTimeTrackerDataJson(_mockInputOutputService.Object, _mockDeveloperController.Object);
            // Don't set storage path

            // Act
            var result = dataProvider.RunningTasks;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void LoadAggregatedTaskData_NonExistentDirectory_ReturnsEmptyDictionary()
        {
            // Arrange
            var dataProvider = new TaskTimeTrackerDataJson(_mockInputOutputService.Object, _mockDeveloperController.Object);
            dataProvider.SetStoragePath(Path.Combine(_testDirectory, "NonExistent"));

            // Act
            var result = dataProvider.RunningTasks;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void TaskNormalization_DifferentCasing_NormalizesConsistently()
        {
            // Arrange & Act
            var normalized1 = TaskTimeTrackerDataJson.NormalizeTaskId("  My Task  ");
            var normalized2 = TaskTimeTrackerDataJson.NormalizeTaskId("MY TASK");
            var normalized3 = TaskTimeTrackerDataJson.NormalizeTaskId("my task");

            // Assert
            Assert.Equal("my task", normalized1);
            Assert.Equal("my task", normalized2);
            Assert.Equal("my task", normalized3);
        }

        [Fact]
        public void MergeTaskData_EarliestStartTime_UsesEarliestTime()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            var earlierTime = DateTime.Now.AddHours(-3);
            var laterTime = DateTime.Now.AddHours(-1);
            
            var task1 = CreateTestTask("TimeTask", "Alice", 30);
            task1.SetDeveloperStartTime("Alice", laterTime);
            task1.Created = laterTime;
            
            var task2 = CreateTestTask("TimeTask", "Bob", 45);
            task2.SetDeveloperStartTime("Bob", earlierTime);
            task2.Created = earlierTime;
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "timetask", task1 } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "timetask", task2 } });

            // Act
            var result = _dataProvider.RunningTasks;

            // Assert
            var mergedTask = result["timetask"];
            // Verify each developer's start time is preserved
            Assert.Equal(laterTime, mergedTask.GetDeveloperStartTime("Alice"));
            Assert.Equal(earlierTime, mergedTask.GetDeveloperStartTime("Bob"));
            // Verify Created property uses the earliest time
            Assert.Equal(earlierTime, mergedTask.Created);
        }

        [Fact]
        public void MergeTaskData_LatestFinishTime_UsesLatestTime()
        {
            // Arrange
            SetupCurrentDeveloper("Alice");
            
            var earlierTime = DateTime.Now.AddHours(-2);
            var laterTime = DateTime.Now.AddHours(-1);
            
            var task1 = CreateTestTask("FinishedTask", "Alice", 30);
            task1.Finished = earlierTime;
            
            var task2 = CreateTestTask("FinishedTask", "Bob", 45);
            task2.Finished = laterTime;
            
            SaveTaskToFile("taskFinishedTimeData.alice.json", new Dictionary<string, TaskData> { { "finishedtask", task1 } });
            SaveTaskToFile("taskFinishedTimeData.bob.json", new Dictionary<string, TaskData> { { "finishedtask", task2 } });

            // Act
            var result = _dataProvider.FinishedTasks;

            // Assert
            var mergedTask = result["finishedtask"];
            Assert.Equal(laterTime, mergedTask.Finished);
        }

        // Helper methods

        private void SetupCurrentDeveloper(string developerName)
        {
            var config = new DeveloperConfig { Name = developerName };
            _mockDeveloperController.Setup(x => x.CurrentDeveloperConfig).Returns(config);
        }

        private TaskData CreateTestTask(string name, string developer, int workMinutes)
        {
            var startTime = DateTime.Now.AddHours(-2);
            var task = new TaskData
            {
                Name = name,
                Description = $"Test task {name}",
                Created = startTime,
                CreatedBy = developer
            };
            
            // Set developer start time
            task.SetDeveloperStartTime(developer, startTime);
            
            if (workMinutes > 0)
            {
                task.AddOrUpdateWorkTime(developer, workMinutes);
            }
            
            return task;
        }

        private void CreateLargeDataSet(int developerCount, int tasksPerDeveloper)
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            
            for (int d = 0; d < developerCount; d++)
            {
                var devName = $"Developer{d:D3}";
                var tasks = new Dictionary<string, TaskData>();
                
                for (int t = 0; t < tasksPerDeveloper; t++)
                {
                    var taskName = $"Task{t:D3}";
                    var sharedTaskName = t % 5 == 0 ? "SharedTask" : taskName; // Every 5th task is shared
                    
                    var task = CreateTestTask(sharedTaskName, devName, random.Next(15, 120));
                    tasks[TaskTimeTrackerDataJson.NormalizeTaskId(sharedTaskName)] = task;
                }
                
                SaveTaskToFile($"taskStartTimeData.{devName.ToLower()}.json", tasks);
            }
        }

        private void CreateTasksWithManyComments(int taskCount, int commentsPerTask)
        {
            var tasks = new Dictionary<string, TaskData>();
            
            for (int i = 0; i < taskCount; i++)
            {
                var task = CreateTestTask($"TaskWithComments{i}", "Alice", 30);
                
                for (int c = 0; c < commentsPerTask; c++)
                {
                    task.Comments.Add(new TaskComment
                    {
                        Created = DateTime.Now.AddMinutes(-c),
                        Developer = "Alice",
                        Comment = $"Comment {c} for task {i}"
                    });
                }
                
                tasks[TaskTimeTrackerDataJson.NormalizeTaskId(task.Name)] = task;
            }
            
            SaveTaskToFile("taskStartTimeData.alice.json", tasks);
        }

        private void SaveTaskToFile(string fileName, Dictionary<string, TaskData> tasks)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            var json = JsonSerializer.Serialize(tasks, JsonOptions);
            File.WriteAllText(filePath, json);
        }
    }
}
