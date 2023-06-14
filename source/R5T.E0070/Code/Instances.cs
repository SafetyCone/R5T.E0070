using System;


namespace R5T.E0070
{
    public static class Instances
    {
        public static F0000.ICancellationTokens CancellationTokens => F0000.CancellationTokens.Instance;
        public static IExampleProjectPaths ExampleProjectPaths => E0070.ExampleProjectPaths.Instance;
        public static L0048.IProjectOperator ProjectOperator => L0048.ProjectOperator.Instance;
        public static F0137.ISemanticsOperator SemanticsOperator => F0137.SemanticsOperator.Instance;
    }
}