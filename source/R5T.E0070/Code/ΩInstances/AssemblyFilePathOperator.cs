using System;


namespace R5T.E0070
{
    public class AssemblyFilePathOperator : IAssemblyFilePathOperator
    {
        #region Infrastructure

        public static IAssemblyFilePathOperator Instance { get; } = new AssemblyFilePathOperator();


        private AssemblyFilePathOperator()
        {
        }

        #endregion
    }
}
