# R5T.E0070
Roslyn semantic information experiment.

NOTE: To do a basic Roslyn semantics operation, you need three packages:

1. Microsoft.CodeAnalysis.Workspaces.MSBuild (R5T.NG0020) - The actual Roslyn workspaces packages that allows you to open a .NET project file.
2. Microsoft.Build.Locator (R5T.NG0021) - Contains the MSBuildLocator type needed to locate MSBuild assemblies on the local machine.
	Required since the MSBuild Microsoft.CodeAnalysis.Workspaces package was used. (It would have been nice if somewhere Microsoft had made the requirement for this dependency explicit, instead of having to find out via an exception.)
3. Microsoft.CodeAnalysis (R5T.NG0019) - Contains the Microsoft.CodeAnalysis.CSharp package, needed to open a C# projet file.
	Required since the project file is a C# project file. (Again, it would have been nice if somewhere Microsoft had made the requirement for this dependency explicit, instead of having to find out via an exception.)


## Implementations

* R5T.O0022 - Semantic operations.
* R5T.F0137 - Semantic operator.
* R5T.O0021 - Workspace operations.
* R5T.F0136 - Workspace operator.
* R5T.O0020 - MSBuildLocator operations.
* R5T.F0135 - MSBuildLocator operator functionality.


## Example projects

We will need example projects for which to load semantic information.
These projects should be simple, but still contain references to other projects.

* C:\Code\DEV\Git\GitHub\SafetyCone\R5T.S0060\source\R5T.S0060.S001.sln
	Simple executable with reference to another library (that itself references other libraries).
	=> Program class has one method, Main(), that has one statement, a call into R5T.F0000.


## Prior Work

* R5T.E0030.E001 - Great examples.
* R5T.E0030 - Great MSBuildLocator work!
* R5T.E0030.T015.X001 - MSBuildLocator, IsRegistered() and RunInContext()
* R5T.E0030.T015.X002 - UsingMSBuildWorkspace()
* R5T.E0029 - First try.