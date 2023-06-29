using System;


namespace R5T.E0070
{
    public static class Instances
    {
        public static IAssemblyFilePathOperator AssemblyFilePathOperator => E0070.AssemblyFilePathOperator.Instance;
        public static F0000.ICancellationTokens CancellationTokens => F0000.CancellationTokens.Instance;
        public static F0000.IEnumerableOperator EnumerableOperator => F0000.EnumerableOperator.Instance;
        public static IExampleProjectPaths ExampleProjectPaths => E0070.ExampleProjectPaths.Instance;
        public static F0000.IFileOperator FileOperator => F0000.FileOperator.Instance;
        public static F0033.INotepadPlusPlusOperator NotepadPlusPlusOperator => F0033.NotepadPlusPlusOperator.Instance;
        public static IPaths Paths => E0070.Paths.Instance;
        public static O0006.IProjectFileOperations ProjectFileOperations => O0006.ProjectFileOperations.Instance;
        public static L0048.IProjectOperator ProjectOperator => L0048.ProjectOperator.Instance;
        public static F0137.ISemanticsOperator SemanticsOperator => F0137.SemanticsOperator.Instance;
        public static F0000.IStringOperator StringOperator => F0000.StringOperator.Instance;
        public static O0024.ISyntaxOperations SyntaxOperations => O0024.SyntaxOperations.Instance;
    }
}