# DotTimeWork
DotTimeWork is a simple time tracking commandline tool that helps you keep track of your working time and tasks.
It works by executing single commands one after another, similar to the DotNet CLI.

## Latest Update
**Under development: Verion 1.1**

- HTML Report with Project Info
- Project Config with Description Text


## How to install - Release
1. Run the commman `dotnet tool install --global dottimework` to install the tool globally.
2. Run the command `dottimework -?` to see the available commands.

## How to install - Development
1. Clone the repository to your local machine.
2. Switch to the cloned Project Directory in PowerShell
3. Install the required dependencies using `dotnet restore`.
4. Run the commman `dotnet pack` to build and publish*
5. Run the commman `dotnet tool install --global --add-source ./nupkg dottimework` to install the tool globally.
6. Run the command `dottimework -?` to see the available commands.

*in the future the tool will be published to NuGet.org and you can install it directly from there.


## How to use
Switch to the Project Directory where you want to track time.

`cd myproject`

Run command Developer to set the developer name and other settings.

`dottimework Developer`

Run command CreateProject to create a new project configuration file.

`dottimework CreateProject`

Run command Start to define the first Task you want to work on.

`dottimework Start`

Run command Work to start tracking time for the task -> Focus Time.

`dottimework Work`

## Commands
- `CreateProject`: Creates Project Configuration file for the current folder.
- `Developer`: Allows to configure the current Developer name and other settings - to be used for all Projects.
- `Start`: Creates new Task to work on.
- `End`: Finished tracking of the selected Task - marks the task as completed.
- `list`: List all active working tasks and how long they have been running.
- `Work`: Starts tracking time for the selected task.
- `Report`: Creates a CSV or HTML report of the tracked time for the current project.

# Future
- Add more commands to manage tasks and projects.
- Add connection to Microsoft Planner and/or ToDo
- Repository Support: JSON serialized Task Status needs to be merged if different developers are tracking there work with this tool
- Allow persisting on SQL-Server instead of local JSON file.
- Add a Watch Command what will scan a directory for changes and automatically start a task if a file is created. Like working on File Task
- Add Developer Statistics: How much worked a day, week, month and year.