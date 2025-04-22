# DotTimeWork
DotTimeWork is a simple time tracking tool that helps you keep track of your work hours and tasks.

## Basic Idea
Like the DotNet CLI this tool is designed to run different commands one after another -> Single Steps

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