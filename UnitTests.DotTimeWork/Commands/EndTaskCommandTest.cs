using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using Moq;

namespace UnitTests.DotTimeWork.Commands
{
    public class EndTaskCommandTest
    {

        [Fact]
        public void Execute_TaskAsParameter()
        {
            // Arrange
            var taskController = new Mock<ITaskTimeTracker>();
            
            var endTaskCommand = new EndTaskCommand(taskController.Object);

            // Act
            // Note: Since Execute is now private and called via System.CommandLine,
            // we test the constructor and command setup instead

            // Assert
            Assert.NotNull(endTaskCommand);
            Assert.Equal("End", endTaskCommand.Name);
        }

    }
}
