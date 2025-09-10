using IdleApi.Scripts;
using IdleApi.Wrap;
using Quartz;

namespace IdleApi.Job
{
    public class IdleJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await FlowScript.VisitHomePage();
        }
    }
}
