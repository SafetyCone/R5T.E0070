using System;
using System.Threading.Tasks;


namespace R5T.E0070
{
    class Program
    {
        static async Task Main()
        {
            //await MSBuildLocatorExplorations.Instance.Try_CreateWorkspaceAndOpenProject();
            //MSBuildLocatorExplorations.Instance.UnregisterWithoutRegister();
            //MSBuildLocatorExplorations.Instance.UnregisterTwice();

            //await Experiments.Instance.Get_ProjectFilePathForMethod();
            //await Experiments.Instance.Get_ProjectFilePathForMethod02();
            //await Experiments.Instance.Get_ProjectsUsedByMethod();
            await Experiments.Instance.Get_ProjectsUsedByMethod03();
        }
    }
}