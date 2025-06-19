using System;
using System.Collections.Generic;
using System.IO;
using DotTimeWork.DataProvider;
using DotTimeWork.TimeTracker;
using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using Xunit;
using Moq;

namespace UnitTests.DotTimeWork
{
    public class TaskTimeTrackerDataJsonTest
    {
        private static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void StoresAndLoadsTasksPerDeveloper()
        {
            var ioMock = new Mock<IInputAndOutputService>();
            var devMock = new Mock<IDeveloperConfigController>();
            devMock.Setup(d => d.CurrentDeveloperConfig).Returns(new DeveloperConfig { Name = "Alice" });
            var provider = new TaskTimeTrackerDataJson(ioMock.Object, devMock.Object);
            string tempDir = CreateTempDir();
            provider.SetStoragePath(tempDir);

            var task = new TaskData { Name = "Task1", Started = DateTime.Now };
            task.AddOrUpdateWorkTime("Alice", 10);
            provider.AddTask(task);
            Assert.True(provider.RunningTasks.ContainsKey("task1"));
            Assert.Equal(10, provider.RunningTasks["task1"].GetWorkTimeForDeveloper("Alice"));

            // Simulate new instance, should load from Alice's file
            var provider2 = new TaskTimeTrackerDataJson(ioMock.Object, devMock.Object);
            provider2.SetStoragePath(tempDir);
            Assert.True(provider2.RunningTasks.ContainsKey("task1"));
            Assert.Equal(10, provider2.RunningTasks["task1"].GetWorkTimeForDeveloper("Alice"));
        }

        [Fact]
        public void AggregatesTasksFromAllDevelopers()
        {
            var ioMock = new Mock<IInputAndOutputService>();
            var devMockA = new Mock<IDeveloperConfigController>();
            devMockA.Setup(d => d.CurrentDeveloperConfig).Returns(new DeveloperConfig { Name = "Alice" });
            var devMockB = new Mock<IDeveloperConfigController>();
            devMockB.Setup(d => d.CurrentDeveloperConfig).Returns(new DeveloperConfig { Name = "Bob" });
            string tempDir = CreateTempDir();

            var providerA = new TaskTimeTrackerDataJson(ioMock.Object, devMockA.Object);
            providerA.SetStoragePath(tempDir);
            var taskA = new TaskData { Name = "TaskA", Started = DateTime.Now };
            taskA.AddOrUpdateWorkTime("Alice", 15);
            providerA.AddTask(taskA);

            var providerB = new TaskTimeTrackerDataJson(ioMock.Object, devMockB.Object);
            providerB.SetStoragePath(tempDir);
            var taskB = new TaskData { Name = "TaskB", Started = DateTime.Now };
            taskB.AddOrUpdateWorkTime("Bob", 20);
            providerB.AddTask(taskB);

            // Use either provider to aggregate
            var allTasks = providerA.GetAllRunningTasksForAllDevelopers();
            Assert.Contains(allTasks, t => t.Name == "TaskA" && t.GetWorkTimeForDeveloper("Alice") == 15);
            Assert.Contains(allTasks, t => t.Name == "TaskB" && t.GetWorkTimeForDeveloper("Bob") == 20);
        }

        [Fact]
        public void MultipleDevelopersCanWorkOnSameTask()
        {
            var ioMock = new Mock<IInputAndOutputService>();
            var devMock = new Mock<IDeveloperConfigController>();
            devMock.Setup(d => d.CurrentDeveloperConfig).Returns(new DeveloperConfig { Name = "Alice" });
            var provider = new TaskTimeTrackerDataJson(ioMock.Object, devMock.Object);
            string tempDir = CreateTempDir();
            provider.SetStoragePath(tempDir);

            var task = new TaskData { Name = "TaskX", Started = DateTime.Now };
            task.AddOrUpdateWorkTime("Alice", 10);
            task.AddOrUpdateWorkTime("Bob", 5);
            provider.AddTask(task);

            Assert.Equal(10, provider.RunningTasks["taskx"].GetWorkTimeForDeveloper("Alice"));
            Assert.Equal(5, provider.RunningTasks["taskx"].GetWorkTimeForDeveloper("Bob"));
        }
    }
}
