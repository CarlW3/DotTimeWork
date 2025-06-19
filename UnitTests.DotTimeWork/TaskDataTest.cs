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
    }
}
