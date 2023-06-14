using System;


namespace R5T.E0070
{
    public class ExampleProjectPaths : IExampleProjectPaths
    {
        #region Infrastructure

        public static IExampleProjectPaths Instance { get; } = new ExampleProjectPaths();


        private ExampleProjectPaths()
        {
        }

        #endregion
    }
}
