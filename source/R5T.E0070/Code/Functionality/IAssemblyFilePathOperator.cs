using System;
using System.Linq;
using R5T.T0132;
using R5T.T0172;


namespace R5T.E0070
{
    [FunctionalityMarker]
    public partial interface IAssemblyFilePathOperator : IFunctionalityMarker
    {
        /// <summary>
        /// Dotnet assembly file paths look like:
        /// <para>C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.16\ref\net6.0\Microsoft.CSharp.dll</para>
        /// </summary>
        public bool Is_DotnetAssemblyFilePath(IAssemblyFilePath assemblyFilePath)
        {
            var output = assemblyFilePath.Value.Contains(@"Program Files\dotnet\");
            return output;
        }

        /// <summary>
        /// NuGet package assembly file paths look like:
        /// <para>C:\Users\David\.nuget\packages\cliwrap\3.6.3\lib\netcoreapp3.0\CliWrap.dll</para>
        /// </summary>
        public bool Is_NuGetPackageAssemblyFilePath(IAssemblyFilePath assemblyFilePath)
        {
            var output = assemblyFilePath.Value.Contains(@".nuget\packages\");
            return output;
        }

        public string Get_NuGetPackageIdentityName(IAssemblyFilePath assemblyFilePath)
        {
            var tokens = Instances.StringOperator.Split(
                @".nuget\packages\",
                assemblyFilePath.Value);

            var packagePackagesDirectoryRelativeFilePath = tokens.Second();

            var directoryNameTokens = Instances.StringOperator.Split(
                @"\",
                packagePackagesDirectoryRelativeFilePath);

            var nugetPackageIdentityName = directoryNameTokens.First();
            return nugetPackageIdentityName;
        }
    }
}
