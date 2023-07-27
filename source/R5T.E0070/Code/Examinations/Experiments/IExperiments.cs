using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using R5T.F0000.Extensions;
using R5T.T0141;
using R5T.T0161.Extensions;
using R5T.T0172;
using R5T.T0172.Extensions;
using R5T.T0179.Extensions;
using R5T.T0208;


namespace R5T.E0070
{
    [ExperimentsMarker]
    public partial interface IExperiments : IExperimentsMarker
    {
        /// <summary>
        /// Select a method in a target project, then find all project references required for that method.
        /// </summary>
        /// <remarks>
        /// Note that methods always live in a type, and that type exists within an assembly.
        /// NuGet packages:
        ///     Assume that NuGet packages are always supplied via a project reference (i.e. that the target project contains no NuGet packages relevant to the method).
        ///     Use the least dependent project reference for a NuGet package on the assumption that all NuGet packages are supplied via specific package selector projects.
        /// </remarks>
        public async Task Get_ProjectsUsedByMethod03()
        {
            /// Inputs.
            var projectFilePath =
                //Instances.ExampleProjectPaths.Example_SimpleWithProjectReference
                //Instances.ExampleProjectPaths.Example_SimpleWithNuGetPackageReference
                //Instances.ExampleProjectPaths.Example_SimpleWithNuGetPackageProviderProjectReference
                @"C:\Code\DEV\Git\GitHub\SafetyCone\R5T.S0073\source\R5T.S0073\R5T.S0073.csproj".ToProjectFilePath()
                ;
            var outputTextFilePath =
                Instances.Paths.OutputTextFilePath
                ;

            Task<MethodDeclarationSyntax> methodDeclarationSelector(Project project) =>
                Instances.SyntaxOperations.Get_MethodOnType(
                    project,
                    //"Program"
                    "IRepositoryScripts"
                    .ToTypeName_N1(),
                    //"Main"
                    "Create_Repository"
                    .ToMethodName_N1()
                );


            /// Run.
            // Get a mapping of NuGet packages-to-containing project for all NuGet packages referenced by all recursive project references.
            // If there are multiple NuGet package references for the same NuGet package name, make sure we evalulate projects in dependency order (from least dependent to most dependent) so we can use the least dependent.
            var recursiveProjectReferencesInDependencyOrder = await Instances.ProjectFileOperations.Get_RecursiveProjectReferences_InDependencyOrder(projectFilePath);

            var containingProjectsByPackageReference = new Dictionary<PackageReference, IProjectFilePath>(
                // Only compare on name, not including version.
                PackageReferenceNameEqualityComparer.Instance);

            foreach (var projectReference in recursiveProjectReferencesInDependencyOrder)
            {
                var packageReferences = await Instances.ProjectFileOperations.Get_PackageReferences(projectReference);
                foreach (var packageReference in packageReferences)
                {
                    // Since recursive project references are evaluated in ascending depdendency order, later projects will have more depenencies that earlier projects.
                    // Use the least dependent dependency since on the assumption that it is a specific package selector project.
                    containingProjectsByPackageReference.Add_IfKeyNotFound(packageReference, projectReference);
                }
            }

            var containingProjectsByPackageIdentityName = containingProjectsByPackageReference
                .ToDictionary(
                    // The package identity name is the lowered packge name.
                    x => x.Key.Identity.ToLowerInvariant(),
                    x => x.Value);

            var methodProjectReferences = new HashSet<IProjectFilePath>();

            await Instances.SemanticsOperator.In_ProjectContext(
                projectFilePath,
                ProjectOperation);

            async Task ProjectOperation(Project project)
            {
                var solution = project.Solution;

                var compilation = await project.GetCompilationAsync();

                var method = await methodDeclarationSelector(project);

                var methodSemanticModel = compilation.GetSemanticModel(method.SyntaxTree);

                var symbolInfos = Get_SymbolInfos(
                    method,
                    methodSemanticModel);

                foreach (var symbolInfo in symbolInfos)
                {
                    ProcessSymbolInfo(
                        symbolInfo,
                        solution,
                        compilation,
                        containingProjectsByPackageIdentityName,
                        methodProjectReferences);
                }

                //// Or maybe just get all member access expressions?
                //var memberAccesses = method.DescendantNodes()
                //    .OfType<MemberAccessExpressionSyntax>()
                //    ;

                //    foreach (var memberAccess in memberAccesses)
                //    {
                //        var accessName = memberAccess.Name;

                //        var accessSymbolInfo = methodSemanticModel.GetSymbolInfo(accessName);

                //        var accessNameSymbol = accessSymbolInfo.Symbol;

                //        var containingAssembly = accessNameSymbol.ContainingAssembly;

                //        var accessLocation = accessNameSymbol.Locations.First();

                //        var isSource = accessLocation.IsInSource;
                //        if (isSource)
                //        {
                //            // If the accessed method declaration is in source, then the method was supplied by a project reference.
                //            var containingAssemblyProject = solution.GetProject(
                //                containingAssembly);

                //            var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                //            methodProjectReferences.Add(
                //                containingAssemblyProjectFilePath.ToProjectFilePath());
                //        }
                //        else
                //        {
                //            // Else if the accessed method declaration is in metadata, then the method is assumed to be supplied by a NuGet package.
                //            // (Note: it could have been a direct assembly reference, this is currently unhandled.)
                //            var assemblyIdentity = containingAssembly.Identity;

                //            var metadataReference = compilation.GetMetadataReference(containingAssembly);

                //            if (metadataReference is PortableExecutableReference portableExecutableReference)
                //            {
                //                var metadataReferenceAssemblyFilePath = portableExecutableReference.FilePath.ToAssemblyFilePath();

                //                // NuGet package reference file paths take the form:
                //                //  C:\Users\David\.nuget\packages\cliwrap\3.6.3\lib\netcoreapp3.0\CliWrap.dll
                //                // While .NET libraries take the form:
                //                //  C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.16\ref\net6.0\Microsoft.CSharp.dll

                //                var isDotnetAssembly = Instances.AssemblyFilePathOperator.Is_DotnetAssemblyFilePath(metadataReferenceAssemblyFilePath);
                //                if(isDotnetAssembly)
                //                {
                //                    // Do nothing.
                //                    continue;
                //                }

                //                var isNugetPackageAssembly = Instances.AssemblyFilePathOperator.Is_NuGetPackageAssemblyFilePath(metadataReferenceAssemblyFilePath);
                //                if(isNugetPackageAssembly)
                //                {
                //                    // Get the package identity name.
                //                    var packageIdentityName = Instances.AssemblyFilePathOperator.Get_NuGetPackageIdentityName(metadataReferenceAssemblyFilePath);

                //                    // Lookup the containging project.
                //                    if(containingProjectsByPackageIdentityName.ContainsKey(packageIdentityName))
                //                    {
                //                        var containingProjectFilePath = containingProjectsByPackageIdentityName[packageIdentityName];

                //                        methodProjectReferences.Add(containingProjectFilePath);
                //                    }
                //                    else
                //                    {
                //                        var message = $"{packageIdentityName}: no containing project found for package identity name.";
                //                        Console.WriteLine(message);
                //                    }

                //                    continue;
                //                }

                //                // Else, throw an exception since we have no idea what to do!
                //                {
                //                    var message = $"Unhandled metadata reference assembly:\n\t{metadataReferenceAssemblyFilePath}";
                //                    Console.WriteLine(message);
                //                    //throw new Exception();
                //                }
                //            }
                //            else
                //            {
                //                // No idea what to do with the metadata reference assembly.
                //                // This link: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.metadatareference?view=roslyn-dotnet-4.3.0
                //                // Suggests there are three possibilities:
                //                //  1. Microsoft.CodeAnalysis.CompilationReference
                //                //  2. Microsoft.CodeAnalysis.PortableExecutableReference
                //                //  3. Microsoft.CodeAnalysis.UnresolvedMetadataReference
                //                var message = $"{metadataReference.Display}: metadata reference assembly was not a {nameof(PortableExecutableReference)}";
                //                Console.WriteLine(message);
                //                //throw new Exception(message);
                //            }
                //        }
                //    }
            }

            static IEnumerable<SymbolInfo> Get_SymbolInfos(
                MethodDeclarationSyntax method,
                SemanticModel semanticModel)
            {
                var memberAccessSymbolInfos = method.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Select(memberAccess =>
                    {
                        var accessName = memberAccess.Name;

                        var symbolInfo = semanticModel.GetSymbolInfo(accessName);
                        return symbolInfo;
                    })
                    ;

                var parameterSymbolInfos = method.DescendantNodes()
                    .OfType<ParameterSyntax>()
                    // Need for some reason, maybe foreach() invocations have null parameter types? (var parameters?)
                    .Where(parameter => parameter.Type is not null)
                    .Select(parameter =>
                    {
                        var typeName = parameter.Type;

                        var symbolInfo = semanticModel.GetSymbolInfo(typeName);
                        return symbolInfo;
                    })
                    ;

                var symbolInfos = Instances.EnumerableOperator.From(
                    memberAccessSymbolInfos,
                    parameterSymbolInfos);

                return symbolInfos;
            }

            static void ProcessSymbolInfo(
                SymbolInfo symbolInfo,
                Solution solution,
                Compilation compilation,
                IDictionary<string, IProjectFilePath> containingProjectsByPackageIdentityName,
                HashSet<IProjectFilePath> methodProjectReferences)
            {
                var accessNameSymbol = symbolInfo.Symbol;

                var containingAssembly = accessNameSymbol.ContainingAssembly;

                var accessLocation = accessNameSymbol.Locations.First();

                var isSource = accessLocation.IsInSource;
                if (isSource)
                {
                    // If the accessed method declaration is in source, then the method was supplied by a project reference.
                    var containingAssemblyProject = solution.GetProject(
                        containingAssembly);

                    var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                    methodProjectReferences.Add(
                        containingAssemblyProjectFilePath.ToProjectFilePath());
                }
                else
                {
                    // Else if the accessed method declaration is in metadata, then the method is assumed to be supplied by a NuGet package.
                    // (Note: it could have been a direct assembly reference, this is currently unhandled.)
                    var assemblyIdentity = containingAssembly.Identity;

                    var metadataReference = compilation.GetMetadataReference(containingAssembly);

                    if (metadataReference is PortableExecutableReference portableExecutableReference)
                    {
                        var metadataReferenceAssemblyFilePath = portableExecutableReference.FilePath.ToAssemblyFilePath();

                        // NuGet package reference file paths take the form:
                        //  C:\Users\David\.nuget\packages\cliwrap\3.6.3\lib\netcoreapp3.0\CliWrap.dll
                        // While .NET libraries take the form:
                        //  C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.16\ref\net6.0\Microsoft.CSharp.dll

                        var isDotnetAssembly = Instances.AssemblyFilePathOperator.Is_DotnetAssemblyFilePath(metadataReferenceAssemblyFilePath);
                        if (isDotnetAssembly)
                        {
                            // Do nothing.
                            return;
                        }

                        var isNugetPackageAssembly = Instances.AssemblyFilePathOperator.Is_NuGetPackageAssemblyFilePath(metadataReferenceAssemblyFilePath);
                        if (isNugetPackageAssembly)
                        {
                            // Get the package identity name.
                            var packageIdentityName = Instances.AssemblyFilePathOperator.Get_NuGetPackageIdentityName(metadataReferenceAssemblyFilePath);

                            // Lookup the containging project.
                            if (containingProjectsByPackageIdentityName.ContainsKey(packageIdentityName))
                            {
                                var containingProjectFilePath = containingProjectsByPackageIdentityName[packageIdentityName];

                                methodProjectReferences.Add(containingProjectFilePath);
                            }
                            else
                            {
                                var message = $"{packageIdentityName}: no containing project found for package identity name.";
                                Console.WriteLine(message);
                            }

                            return;
                        }

                        // Else, throw an exception since we have no idea what to do!
                        {
                            var message = $"Unhandled metadata reference assembly:\n\t{metadataReferenceAssemblyFilePath}";
                            Console.WriteLine(message);
                            //throw new Exception();
                        }
                    }
                    else
                    {
                        // No idea what to do with the metadata reference assembly.
                        // This link: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.metadatareference?view=roslyn-dotnet-4.3.0
                        // Suggests there are three possibilities:
                        //  1. Microsoft.CodeAnalysis.CompilationReference
                        //  2. Microsoft.CodeAnalysis.PortableExecutableReference
                        //  3. Microsoft.CodeAnalysis.UnresolvedMetadataReference
                        var message = $"{metadataReference.Display}: metadata reference assembly was not a {nameof(PortableExecutableReference)}";
                        Console.WriteLine(message);
                        //throw new Exception(message);
                    }
                }
            }

            // Note: do *not* remove the target project itself!
            // It is important to know that the selected method relies on methods within the target project
            // (i.e. that the method cannot be severed from the target project without other methods first being severed).
            // You will need to extricate the method as opposed to severing the method.

            var lines = methodProjectReferences
                .Get_Values()
                .OrderAlphabetically()
                ;

            await Instances.FileOperator.WriteLines(
                outputTextFilePath,
                lines);

            Instances.NotepadPlusPlusOperator.Open(outputTextFilePath);
        }

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

                var memberAccessSymbolInfos = method.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Select(memberAccess =>
                    {
                        var accessName = memberAccess.Name;

                        var symbolInfo = semanticModel.GetSymbolInfo(accessName);
                        return symbolInfo;
                    })
                    ;

                var parameterSymbolInfos = method.DescendantNodes()
                    .OfType<ParameterSyntax>()
                    .Select(parameter =>
                    {
                        var typeName = parameter.Type;

                        var symbolInfo = semanticModel.GetSymbolInfo(typeName);
                        return symbolInfo;
                    })
                    ;

                var symbolInfos = Instances.EnumerableOperator.From(
                    memberAccessSymbolInfos,
                    parameterSymbolInfos);

                foreach (var symbolInfo in symbolInfos)
                {
                    ProcessSymbolInfo(symbolInfo);
                }

                void ProcessSymbolInfo(SymbolInfo symbolInfo)
                {
                    var symbol = symbolInfo.Symbol;

                    var accessName = symbolInfo.Symbol.Name;

                    var containingAssembly = symbol.ContainingAssembly;

                    var methodLocation = symbol.Locations.First();

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

                //foreach (var memberAccess in memberAccessSymbols)
                //{
                //    var accessName = memberAccess.Name;

                //    var symbolInfo = semanticModel.GetSymbolInfo(accessName);

                //    var methodNameSymbol = symbolInfo.Symbol;

                //    var containingAssembly = methodNameSymbol.ContainingAssembly;

                //    var methodLocation = methodNameSymbol.Locations.First();

                //    var isSource = methodLocation.IsInSource;
                //    if (isSource)
                //    {
                //        // This is it! Get the project for an assembly symbol!
                //        var containingAssemblyProject = solution.GetProject(
                //            containingAssembly);

                //        var containingAssemblyProjectFilePath = containingAssemblyProject.FilePath;

                //        Console.WriteLine($"{accessName}:\n\t{containingAssemblyProjectFilePath}");
                //    }
                //    else
                //    {
                //        var assemblyIdentity = containingAssembly.Identity;

                //        Console.WriteLine($"{accessName}:\n\t{assemblyIdentity.Name} (assembly)");

                //        // Since the location is not in source, it is not a project reference, and is thus an assembly reference.
                //        // (Which is usually a NuGet package reference, but could just be an assembly.)
                //        // (What about COM references?)
                //        var metadataReference = compilation.GetMetadataReference(containingAssembly);

                //        Console.WriteLine(metadataReference.Display);
                //        Console.WriteLine(metadataReference.Properties.Kind);

                //        if (metadataReference is PortableExecutableReference portableExecutableReference)
                //        {
                //            Console.WriteLine(portableExecutableReference.FilePath);
                //        }
                //    }
                //}
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
