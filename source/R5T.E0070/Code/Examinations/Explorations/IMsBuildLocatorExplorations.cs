using System;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

using R5T.T0141;


namespace R5T.E0070
{
    [ExplorationsMarker]
    public partial interface IMsBuildLocatorExplorations : IExplorationsMarker
    {
        /// <summary>
        /// Unregister twice succeeds.
        /// </summary>
        public void UnregisterTwice()
        {
            MSBuildLocator.RegisterDefaults();

            MSBuildLocator.Unregister();
            
            // No exception.
            MSBuildLocator.Unregister();
        }

        /// <summary>
        /// Unregistering without first registering is an exception.
        /// </summary>
        public void UnregisterWithoutRegister()
        {
            // Results in an exception:
            //  System.InvalidOperationException: 'Microsoft.Build.Locator.MSBuildLocator.Unregister was called, but no MSBuild instance is registered.
            //  Ensure that RegisterInstance, RegisterMSBuildPath, or RegisterDefaults is called before calling this method.
            //  IsRegistered should be used to determine whether calling Unregister is a valid operation.'

            MSBuildLocator.Unregister();
        }

        /// <summary>
        /// Try to create a workspace and open a project.
        /// This might require 
        /// </summary>
        public async Task Try_CreateWorkspaceAndOpenProject()
        {
            MSBuildLocator.RegisterDefaults();
            // If called twice, this exception occurs:
            //  System.InvalidOperationException: 'Microsoft.Build.Locator.MSBuildLocator.RegisterInstance was called, but MSBuild assemblies were already loaded.
            //  Ensure that RegisterInstance is called before any method that directly references types in the Microsoft.Build namespace has been called.
            //  This dependency arises from when a method is just-in-time compiled, so if it breaks even if the reference to a Microsoft.Build type has not been executed.
            //  For more details, see aka.ms/RegisterMSBuildLocator
            //  Loaded MSBuild assemblies: '
            //MSBuildLocator.RegisterDefaults();

            //static async Task Internal()
            //{
            //    // If MSBuildLocator is not used, this exception occurs:
            //    //  System.Reflection.ReflectionTypeLoadException: 'Unable to load one or more of the requested types.
            //    //  Could not load file or assembly 'Microsoft.Build.Framework, Version=15.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'. The system cannot find the file specified.
            //    using var workspace = MSBuildWorkspace.Create();

            //    // Somehow this incredibly obnoxious error appears:
            //    //  System.InvalidOperationException:
            //    //  'Cannot open project 'C:\Code\DEV\Git\GitHub\SafetyCone\R5T.S0060\source\R5T.S0060.S001\R5T.S0060.S001.csproj' because the language 'C#' is not supported.'
            //    // To fix, you need to make sure the Microsoft.CodeAnalysis.CSharp package is also available (which is part of Microsoft.CodeAnalysis package).
            //    var project = await workspace.OpenProjectAsync(
            //        Instances.ExampleProjectPaths.Example_SimpleWithProjectReference.Value);

            //    Console.WriteLine($"{project.Language}: language");
            //}

            // Unlike what is stated here: https://learn.microsoft.com/en-us/visualstudio/msbuild/find-and-use-msbuild-versions?view=vs-2019#register-instance-before-calling-msbuild
            // It seems like you do not need a sub-method.
            // (Perhaps the discussion in that link is talking about types that are directly from the Microsoft.Build.* packages.)
            using var workspace = MSBuildWorkspace.Create();

            var project = await workspace.OpenProjectAsync(
                Instances.ExampleProjectPaths.Example_SimpleWithProjectReference.Value);

            Console.WriteLine($"{project.Language}: language");

            //await Internal();
            //await this.Try_CreateWorkspaceAndOpenProject_Internal();
        }

        //    private async Task Try_CreateWorkspaceAndOpenProject_Internal()
        //    {
        //        using var workspace = MSBuildWorkspace.Create();

        //        var project = await workspace.OpenProjectAsync(
        //            Instances.ExampleProjectPaths.Example_SimpleWithProjectReference.Value);

        //        Console.WriteLine($"{project.Language}: language");
        //    }
    }
}
