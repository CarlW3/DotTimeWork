using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Developer;
using DotTimeWork.TimeTracker;
using Moq;
using System.Text.Json;

namespace UnitTests.DotTimeWork
{
    public class TaskTimeTrackerDataJsonCommentSeparationTest : IDisposable
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

        public TaskTimeTrackerDataJsonCommentSeparationTest()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DotTimeWorkCommentTests", Guid.NewGuid().ToString());
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
        public void SaveTask_OnlySavesCurrentDeveloperComments()
        {
            // Arrange
            var alice = "Alice";
            var bob = "Bob";
            SetupCurrentDeveloper(alice);

            // Create a task with comments from multiple developers
            var task = CreateTestTask("TaskWithMixedComments", alice, 60);
            task.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-2), Developer = alice, Comment = "Alice's comment" });
            task.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-1), Developer = bob, Comment = "Bob's comment" });
            task.Comments.Add(new TaskComment { Created = DateTime.Now, Developer = alice, Comment = "Another Alice comment" });

            // Act - Save the task (should only save Alice's comments since she's the current developer)
            _dataProvider.AddTask(task);

            // Assert - Check Alice's file
            var aliceFilePath = Path.Combine(_testDirectory, "taskStartTimeData.alice.json");
            Assert.True(File.Exists(aliceFilePath));
            
            var aliceTasks = LoadTasksFromFile(aliceFilePath);
            var savedTask = aliceTasks["taskwithmixedcomments"];
            
            // Should only have Alice's comments (2 comments)
            Assert.Equal(2, savedTask.Comments.Count);
            Assert.All(savedTask.Comments, comment => Assert.Equal(alice, comment.Developer));
            Assert.Contains(savedTask.Comments, c => c.Comment == "Alice's comment");
            Assert.Contains(savedTask.Comments, c => c.Comment == "Another Alice comment");
            Assert.DoesNotContain(savedTask.Comments, c => c.Comment == "Bob's comment");
        }

        [Fact]
        public void LoadAggregatedTasks_CombinesCommentsFromAllDevelopers()
        {
            // Arrange - Setup tasks in separate developer files
            var alice = "Alice";
            var bob = "Bob";
            
            // Alice's task with her comments
            var aliceTask = CreateTestTask("SharedTask", alice, 60);
            aliceTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-3), Developer = alice, Comment = "Alice started work" });
            aliceTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-1), Developer = alice, Comment = "Alice progress update" });
            
            // Bob's task with his comments  
            var bobTask = CreateTestTask("SharedTask", bob, 45);
            bobTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-2), Developer = bob, Comment = "Bob joined the task" });
            bobTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddMinutes(-30), Developer = bob, Comment = "Bob completed feature X" });
            
            // Save to separate files
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "sharedtask", aliceTask } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "sharedtask", bobTask } });
            
            SetupCurrentDeveloper(alice);

            // Act - Load aggregated tasks
            var aggregatedTasks = _dataProvider.RunningTasks;

            // Assert - Should have one task with all comments from both developers
            Assert.Single(aggregatedTasks);
            var mergedTask = aggregatedTasks["sharedtask"];
            
            Assert.Equal(4, mergedTask.Comments.Count); // 2 from Alice + 2 from Bob
            Assert.Equal(2, mergedTask.Comments.Count(c => c.Developer == alice));
            Assert.Equal(2, mergedTask.Comments.Count(c => c.Developer == bob));
            
            // Verify specific comments are present
            Assert.Contains(mergedTask.Comments, c => c.Developer == alice && c.Comment == "Alice started work");
            Assert.Contains(mergedTask.Comments, c => c.Developer == alice && c.Comment == "Alice progress update");
            Assert.Contains(mergedTask.Comments, c => c.Developer == bob && c.Comment == "Bob joined the task");
            Assert.Contains(mergedTask.Comments, c => c.Developer == bob && c.Comment == "Bob completed feature X");
        }

        [Fact]
        public void UpdateTask_OnlySavesCurrentDeveloperComments()
        {
            // Arrange
            var alice = "Alice";
            var bob = "Bob";
            
            // Create initial task files for both developers
            var aliceTask = CreateTestTask("UpdateTest", alice, 30);
            aliceTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-2), Developer = alice, Comment = "Original Alice comment" });
            
            var bobTask = CreateTestTask("UpdateTest", bob, 25);
            bobTask.Comments.Add(new TaskComment { Created = DateTime.Now.AddHours(-1), Developer = bob, Comment = "Original Bob comment" });
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "updatetest", aliceTask } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "updatetest", bobTask } });
            
            SetupCurrentDeveloper(alice);

            // Get the aggregated task and add a new comment from Alice
            var taskToUpdate = _dataProvider.GetRunningTaskById("UpdateTest");
            Assert.NotNull(taskToUpdate);
            taskToUpdate.Comments.Add(new TaskComment { Created = DateTime.Now, Developer = alice, Comment = "New Alice comment" });

            // Act - Update the task (Alice is current developer)
            _dataProvider.UpdateTask(taskToUpdate);

            // Assert - Check Alice's file should have both Alice comments, but not Bob's
            var aliceFilePath = Path.Combine(_testDirectory, "taskStartTimeData.alice.json");
            var aliceTasks = LoadTasksFromFile(aliceFilePath);
            var updatedAliceTask = aliceTasks["updatetest"];
            
            Assert.Equal(2, updatedAliceTask.Comments.Count);
            Assert.All(updatedAliceTask.Comments, c => Assert.Equal(alice, c.Developer));
            Assert.Contains(updatedAliceTask.Comments, c => c.Comment == "Original Alice comment");
            Assert.Contains(updatedAliceTask.Comments, c => c.Comment == "New Alice comment");
            
            // Check Bob's file should remain unchanged
            var bobFilePath = Path.Combine(_testDirectory, "taskStartTimeData.bob.json");
            var bobTasks = LoadTasksFromFile(bobFilePath);
            var originalBobTask = bobTasks["updatetest"];
            
            Assert.Single(originalBobTask.Comments);
            Assert.Equal(bob, originalBobTask.Comments[0].Developer);
            Assert.Equal("Original Bob comment", originalBobTask.Comments[0].Comment);
        }

        [Fact]
        public void CommentDeduplication_AcrossDeveloperFiles_WorksCorrectly()
        {
            // Arrange - Create scenario where same comment might appear in multiple files
            var alice = "Alice";
            var bob = "Bob";
            
            var commonTimestamp = DateTime.Now.AddHours(-1);
            
            // Alice's task
            var aliceTask = CreateTestTask("DedupeTest", alice, 30);
            aliceTask.Comments.Add(new TaskComment { Created = commonTimestamp, Developer = alice, Comment = "Unique Alice comment" });
            aliceTask.Comments.Add(new TaskComment { Created = commonTimestamp.AddMinutes(1), Developer = bob, Comment = "Duplicate comment" });
            
            // Bob's task (with the same "Duplicate comment" - this could happen if files got out of sync)
            var bobTask = CreateTestTask("DedupeTest", bob, 25);
            bobTask.Comments.Add(new TaskComment { Created = commonTimestamp.AddMinutes(1), Developer = bob, Comment = "Duplicate comment" });
            bobTask.Comments.Add(new TaskComment { Created = commonTimestamp.AddMinutes(2), Developer = bob, Comment = "Unique Bob comment" });
            
            SaveTaskToFile("taskStartTimeData.alice.json", new Dictionary<string, TaskData> { { "dedupetest", aliceTask } });
            SaveTaskToFile("taskStartTimeData.bob.json", new Dictionary<string, TaskData> { { "dedupetest", bobTask } });
            
            SetupCurrentDeveloper(alice);

            // Act
            var aggregatedTasks = _dataProvider.RunningTasks;

            // Assert - Should deduplicate the comment that appears in both files
            var mergedTask = aggregatedTasks["dedupetest"];
            Assert.Equal(3, mergedTask.Comments.Count); // Not 4, because one is deduplicated
            
            Assert.Single(mergedTask.Comments.Where(c => c.Comment == "Duplicate comment"));
            Assert.Single(mergedTask.Comments.Where(c => c.Comment == "Unique Alice comment"));
            Assert.Single(mergedTask.Comments.Where(c => c.Comment == "Unique Bob comment"));
        }

        [Fact]
        public void SaveTask_EmptyCommentsList_HandledCorrectly()
        {
            // Arrange
            var alice = "Alice";
            SetupCurrentDeveloper(alice);
            
            var task = CreateTestTask("NoCommentsTask", alice, 45);
            // Task has no comments

            // Act
            _dataProvider.AddTask(task);

            // Assert
            var aliceFilePath = Path.Combine(_testDirectory, "taskStartTimeData.alice.json");
            var aliceTasks = LoadTasksFromFile(aliceFilePath);
            var savedTask = aliceTasks["nocommentstask"];
            
            Assert.Empty(savedTask.Comments);
        }

        // Helper methods

        private void SetupCurrentDeveloper(string developerName)
        {
            var config = new DeveloperConfig { Name = developerName };
            _mockDeveloperController.Setup(x => x.CurrentDeveloperConfig).Returns(config);
        }

        private static TaskData CreateTestTask(string name, string developer, int workMinutes)
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
