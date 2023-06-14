using System;


namespace R5T.E0070
{
    public class MSBuildLocatorExplorations : IMsBuildLocatorExplorations
    {
        #region Infrastructure

        public static IMsBuildLocatorExplorations Instance { get; } = new MSBuildLocatorExplorations();


        private MSBuildLocatorExplorations()
        {
        }

        #endregion
    }
}
