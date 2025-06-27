using System;
using System.Collections.Generic;
using DotTimeWork.TimeTracker;
using Xunit;

namespace UnitTests.DotTimeWork
{
    public class TaskDataTest
    {
        [Fact]
        public void AddOrUpdateWorkTime_AddsAndUpdatesCorrectly()
        {
            var task = new TaskData { Name = "TestTask" };
            task.AddOrUpdateWorkTime("Alice", 10);
            Assert.Equal(10, task.GetWorkTimeForDeveloper("Alice"));
            task.AddOrUpdateWorkTime("Alice", 5);
            Assert.Equal(15, task.GetWorkTimeForDeveloper("Alice"));
        }

        [Fact]
        public void EnsureDeveloperEntry_AddsDeveloperWithZeroIfNotPresent()
        {
            var task = new TaskData { Name = "TestTask" };
            task.EnsureDeveloperEntry("Bob");
            Assert.True(task.DeveloperWorkTimes.ContainsKey("Bob"));
            Assert.Equal(0, task.GetWorkTimeForDeveloper("Bob"));
        }

        [Fact]
        public void GetTotalWorkTime_SumsAllDeveloperTimes()
        {
            var task = new TaskData { Name = "TestTask" };
            task.AddOrUpdateWorkTime("Alice", 10);
            task.AddOrUpdateWorkTime("Bob", 20);
            Assert.Equal(30, task.GetTotalWorkTime());
        }

        [Fact]
        public void GetWorkTimeForDeveloper_ReturnsZeroIfNotPresent()
        {
            var task = new TaskData { Name = "TestTask" };
            Assert.Equal(0, task.GetWorkTimeForDeveloper("Charlie"));
        }

        [Fact]
        public void SetDeveloperStartTime_SetsCorrectly()
        {
            var task = new TaskData { Name = "TestTask" };
            var startTime = new DateTime(2022, 1, 1);
            task.SetDeveloperStartTime("Alice", startTime);
            Assert.Equal(startTime, task.GetDeveloperStartTime("Alice"));
        }

        [Fact]
        public void GetDeveloperStartTime_ReturnsMinValueIfNotPresent()
        {
            var task = new TaskData { Name = "TestTask" };
            Assert.Equal(DateTime.MinValue, task.GetDeveloperStartTime("NonExistentDev"));
        }

        [Fact]
        public void IsDeveloperParticipating_ReturnsTrueForExistingDeveloper()
        {
            var task = new TaskData { Name = "TestTask" };
            task.SetDeveloperStartTime("Alice", DateTime.Now);
            Assert.True(task.IsDeveloperParticipating("Alice"));
            Assert.False(task.IsDeveloperParticipating("Bob"));
        }

        [Fact]
        public void Started_BackwardCompatibility_ReturnsCreatorStartTime()
        {
            var task = new TaskData { Name = "TestTask", CreatedBy = "Alice" };
            var startTime = new DateTime(2022, 1, 1);
            task.SetDeveloperStartTime("Alice", startTime);
            Assert.Equal(startTime, task.Started);
        }
    }
}
