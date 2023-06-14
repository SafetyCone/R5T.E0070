using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using R5T.T0141;


namespace R5T.E0070
{
    [ExperimentsMarker]
    public partial interface IExperiments : IExperimentsMarker
    {
        /// <summary>
        /// Select a method within a project, then use the semantic information for method invocations within that method to get a list of types used within the method.
        /// Then, foreach type, get the project file path (or NuGet package information, or project file path containing the NuGet package reference).
        /// Collect all of these project references, and compute which are the top-level project references.
        /// Then, modify a target project to include those top-level project references (if not included already).
        /// </summary>
        public async Task Get_ProjectsUsedByMethod02()
        {
            /// Inputs.
            var projectFilePath =
                //Instances.ExampleProjectPaths.Example_SimpleWithProjectReference
                Instances.ExampleProjectPaths.Example_SimpleWithNuGetPackageReference
                ;


            /// Run.
            

            await Instances.SemanticsOperator.In_ProjectContext(
                projectFilePath,
                ProjectOperation);

            static async Task ProjectOperation(Project project)
            {
                var solution = project.Solution;

                var compilation = await project.GetCompilationAsync();

                // Select the method.
                var programClass = await Instances.ProjectOperator.Get_AllSyntaxNodesOfType<ClassDeclarationSyntax>(project)
                    .Where(@class => @class.Identifier.ValueText == "Program")
                    .FirstAsync();

                var method = programClass.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Identifier.ValueText == "Main")
                    .First();

                var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);

                var memberAccesses = method.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    ;

                foreach (var memberAccess in memberAccesses)
                {
                    var accessName = memberAccess.Name;

                    var symbolInfo = semanticModel.GetSymbolInfo(accessName);

                    var methodNameSymbol = symbolInfo.Symbol;

                    var containingAssembly = methodNameSymbol.ContainingAssembly;

                    var methodLocation = methodNameSymbol.Locations.First();

                    var isSource = methodLocation.IsInSource;
                    if (isSource)
                    {
                        // This is it! Get the project for an assembly symbol!
                        var containingAssemblyProject = solution.GetProject(
                            containingAssembly);

                        var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                        Console.WriteLine($"{accessName}:\n\t{containingAssemblyProjectFilePath}");
                    }
                    else
                    {
                        var assemblyIdentity = containingAssembly.Identity;

                        Console.WriteLine($"{accessName}:\n\t{assemblyIdentity.Name} (assembly)");

                        // Since the location is not in source, it is not a project reference, and is thus an assembly reference.
                        // (Which is usually a NuGet package reference, but could just be an assembly.)
                        // (What about COM references?)
                        var metadataReference = compilation.GetMetadataReference(containingAssembly);

                        Console.WriteLine(metadataReference.Display);
                        Console.WriteLine(metadataReference.Properties.Kind);

                        if (metadataReference is PortableExecutableReference portableExecutableReference)
                        {
                            Console.WriteLine(portableExecutableReference.FilePath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Within a method in a class in project, find all types used (whether those are types whose methods are called, an InvocationExpressionSyntax,
        /// or types whose properties are accessed, a SimpleMemberAccessExpressionSyntax,
        /// but *not* declared types).
        /// Then get the set of projects which contain those types.
        /// </summary>
        public async Task Get_ProjectsUsedByMethod()
        {
            /// Inputs.
            var projectFilePath =
                //Instances.ExampleProjectPaths.Example_SimpleWithProjectReference
                Instances.ExampleProjectPaths.Example_SimpleWithNuGetPackageReference
                ;


            /// Run.
            await Instances.SemanticsOperator.In_ProjectContext(
                projectFilePath,
                ProjectOperation);

            static async Task ProjectOperation(Project project)
            {
                // Determine NuGet packages.
                foreach (var metadataReference in project.MetadataReferences)
                {
                    Console.WriteLine(metadataReference.Display);
                    Console.WriteLine(metadataReference.Properties.Kind);
                    
                    if(metadataReference is PortableExecutableReference portableExecutableReference)
                    {
                        Console.WriteLine(portableExecutableReference.FilePath);
                    }

                    // There is *no* explicit way to tell if the metadata reference comes from a NuGet package or not.
                    // They are all just assemblies.
                    // However, it seems like NuGet package reference file paths take the form:
                    //  C:\Users\David\.nuget\packages\cliwrap\3.6.3\lib\netcoreapp3.0\CliWrap.dll
                    // While .NET libraries take the form:
                    //  C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.16\ref\net6.0\Microsoft.CSharp.dll
                }

                // Now get method symbols.
                var solution = project.Solution;

                var compilation = await project.GetCompilationAsync();

                // Get the program class.
                var programClass = await Instances.ProjectOperator.Get_AllSyntaxNodesOfType<ClassDeclarationSyntax>(project)
                    .Where(@class => @class.Identifier.ValueText == "Program")
                    .FirstAsync();

                var mainMethod = programClass.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Identifier.ValueText == "Main")
                    .First();

                var semanticModel = compilation.GetSemanticModel(mainMethod.SyntaxTree);

                // Get method invocations.
                //// Pretty easy: invocation expressions whose expression is itself a member access.
                //var invocationExpressions = mainMethod.DescendantNodes()
                //    .OfType<InvocationExpressionSyntax>()
                //    .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax)
                //    ;

                //foreach (var invocation in invocationExpressions)
                //{
                //    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

                //    var accessName = memberAccess.Name;

                //    var symbolInfo = semanticModel.GetSymbolInfo(accessName);

                //    var methodNameSymbol = symbolInfo.Symbol;

                //    var containingAssembly = methodNameSymbol.ContainingAssembly;

                //    // This is it! Get the project for an assembly symbol!
                //    var containingAssemblyProject = solution.GetProject(
                //        containingAssembly);

                //    var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                //    Console.WriteLine(containingAssemblyProjectFilePath);
                //}

                // Or maybe just get all member access expressions?
                var memberAccesses = mainMethod.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    ;

                foreach (var memberAccess in memberAccesses)
                {
                    var accessName = memberAccess.Name;

                    var symbolInfo = semanticModel.GetSymbolInfo(accessName);

                    var methodNameSymbol = symbolInfo.Symbol;

                    var containingAssembly = methodNameSymbol.ContainingAssembly;

                    // We only want methods that are in source.
                    var methodLocation = methodNameSymbol.Locations.First();

                    var isSource = methodLocation.IsInSource;
                    if(isSource)
                    {
                        // This is it! Get the project for an assembly symbol!
                        var containingAssemblyProject = solution.GetProject(
                            containingAssembly);

                        var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                        Console.WriteLine($"{accessName}:\n\t{containingAssemblyProjectFilePath}");
                    }
                    else
                    {
                        var assemblyIdentity = containingAssembly.Identity;

                        // Should be null, since the location is not in source.
                        var containingAssemblyProject = solution.GetProject(containingAssembly);
                        if(containingAssemblyProject is not null)
                        {
                            var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                            Console.WriteLine($"{accessName}:\n\t{containingAssemblyProjectFilePath}");
                        }
                        else
                        {
                            // Since the location is not in source, it is not a project reference, and is thus an assembly reference.
                            // (Which is usually a NuGet package reference, but could just be an assembly.)
                            // (What about COM references?)
                            var metadataReference = compilation.GetMetadataReference(containingAssembly);

                            Console.WriteLine(metadataReference.Display);
                            Console.WriteLine(metadataReference.Properties.Kind);

                            if (metadataReference is PortableExecutableReference portableExecutableReference)
                            {
                                Console.WriteLine(portableExecutableReference.FilePath);
                            }
                        }

                        Console.WriteLine($"{accessName}:\n\t{assemblyIdentity.Name} (assembly)");
                    }
                }
            }
        }

        public async Task Get_ProjectFilePathForMethod02()
        {
            /// Inputs.
            var projectFilePath =
                Instances.ExampleProjectPaths.Example_SimpleWithProjectReference
                ;


            /// Run.
            await Instances.SemanticsOperator.In_ProjectContext(
                projectFilePath,
                ProjectOperation);

            static async Task ProjectOperation(Project project)
            {
                var solution = project.Solution;

                var compilation = await project.GetCompilationAsync();

                // Get the program class.
                var programClass = await Instances.ProjectOperator.Get_AllSyntaxNodesOfType<ClassDeclarationSyntax>(project)
                    .Where(@class => @class.Identifier.ValueText == "Program")
                    .FirstAsync();

                var mainMethod = programClass.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Identifier.ValueText == "Main")
                    .First();

                var semanticModel = compilation.GetSemanticModel(mainMethod.SyntaxTree);

                var mainMethodSymbol = semanticModel.GetDeclaredSymbol(mainMethod);

                var containingAssembly = mainMethodSymbol.ContainingAssembly;

                // This is it! Get the project for an assembly symbol!
                var containingAssemblyProject = solution.GetProject(
                    containingAssembly);

                var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                Console.WriteLine(containingAssemblyProjectFilePath);
            }
        }

        /// <summary>
        /// How do I get the project file path for a method symbol?
        /// Answer: Get the <see cref="ISymbol.ContainingAssembly"/> <see cref="IAssemblySymbol"/>,
        /// then use the <see cref="Solution.GetProject(IAssemblySymbol, System.Threading.CancellationToken)"/> method on a solution instance to get the project.
        /// The ask for the <see cref="Project.FilePath"/> value.
        /// </summary>
        public async Task Get_ProjectFilePathForMethod()
        {
            /// Inputs.
            var projectFilePath =
                Instances.ExampleProjectPaths.Example_SimpleWithProjectReference
                ;


            /// Run.
            await Instances.SemanticsOperator.In_ProjectContext(
                projectFilePath,
                ProjectOperation);

            static async Task ProjectOperation(Project project)
            {
                var solution = project.Solution;

                // Build a dictionary of project file paths by document file path.
                var projectFilePathsByDocumentFilePath = new Dictionary<string, string>();
                
                //foreach (var projectReference in project.AllProjectReferences)
                //{
                //    var currentProject = solution.GetProject()
                //}


                var compilation = await project.GetCompilationAsync();

                var mainMethodSymbol = compilation.GetEntryPoint(
                    Instances.CancellationTokens.None);

                var location = mainMethodSymbol.Locations.First();
                
                // Need to make sure the location is in source (not in metadata, i.e. a package).
                //location.IsInSource

                var locationFilePath = location.SourceTree.FilePath;

                var metadataModule = location.MetadataModule;

                //var moduleMetadata = metadataModule.GetMetadata();

                //var moduleReference = moduleMetadata.GetReference();

                //var moduleReferenceFilePath = moduleReference.FilePath;

                var syntaxTree = location.SourceTree;

                var syntaxTreeRoot = syntaxTree.GetRoot();

                var containingAssembly = mainMethodSymbol.ContainingAssembly;

                // This is it! Get the project for an assembly symbol!
                var containingAssemblyProject = solution.GetProject(
                    containingAssembly);

                var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                // Null.
                var containingAssemblyMetadata = containingAssembly.GetMetadata();

                var containingModule = mainMethodSymbol.ContainingModule;

                var programClassType = mainMethodSymbol.ContainingType;

                var allProjectReferences = project.AllProjectReferences;


                // Get the main method syntax node.
                // Search through documents.
                var firstDocument = project.Documents.First();
                // For method declarations.
                // That are static, with the name "Main".

                var semanticModel = await firstDocument.GetSemanticModelAsync();

                var syntaxRoot = await firstDocument.GetSyntaxRootAsync();
            }
        }
    }
}
