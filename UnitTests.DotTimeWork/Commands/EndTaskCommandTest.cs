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
            endTaskCommand.Execute("Task1", false);

            // Assert
            taskController.Verify(x => x.EndTask("Task1"), Times.Once);
        }

    }
}
