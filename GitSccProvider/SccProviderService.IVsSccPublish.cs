using System;
using System.Threading;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;
namespace GitScc
{
    public partial class SccProviderService : IVsSccPublish
    {
        public async Task BeginPublishWorkflowAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IOleCommandTarget oleCommandTarget = ServiceProvider.GlobalProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;

            // Execute the add to source control command. In an actual Source Control Provider, query the status before executing the command.
            oleCommandTarget.Exec(GuidList.guidSccProviderCmdSet, CommandId.icmdSccCommandInit, 0, IntPtr.Zero, IntPtr.Zero);

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
