using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ChangeLanguageVersionExtension.ProjectVersionServices;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace ChangeLanguageVersionExtension
{
   
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CSharpProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidChangeLanguageVersionCommandPackageString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class ChangeLanguageVersionCommandPackage : AsyncPackage
    {

        private IProjectVersionService projectVersionService;
        private List<ChangeLanguageVersionCommand> commands;
        private readonly List<(string version, int commandId)> versionCommands = new List<(string version, int commandId)> {
                                                                                                                        (LanguageVersions.Default,PackageIds.cmdSetToDefault),
                                                                                                                        (LanguageVersions.Latest,PackageIds.cmdSetToLatest),
                                                                                                                        (LanguageVersions.CSharp5, PackageIds.cmdSetToCSharp5),
                                                                                                                        (LanguageVersions.CSharp6, PackageIds.cmdSetToCSharp6),
                                                                                                                        (LanguageVersions.CSharp7, PackageIds.cmdSetToCSharp7),
                                                                                                                        (LanguageVersions.CSharp71, PackageIds.cmdSetToCSharp71),
                                                                                                                        (LanguageVersions.CSharp72, PackageIds.cmdSetToCSharp72),
                                                                                                                        (LanguageVersions.CSharp73, PackageIds.cmdSetToCSharp73),
                                                                                                                      };        

        private void HandleChecked(object sender, EventArgs e)
        {
            foreach (var command in commands)
            {
                if (command != (ChangeLanguageVersionCommand)sender)
                {
                    command.Checked = false;
                }
            }
        }
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            projectVersionService = new ProjectVersionService((DTE2)GetGlobalService(typeof(SDTE)));
            var commandService = (OleMenuCommandService)(await GetServiceAsync((typeof(IMenuCommandService))));
            commands = versionCommands.Select(c => new ChangeLanguageVersionCommand(this, projectVersionService, c.version, new CommandID(PackageGuids.guidChangeLanguageVersionCommandPackageCmdSet, c.commandId))).ToList();
            var currentLanguageVersion = projectVersionService.GetLanguageVersion();
            var availableVersions = projectVersionService.GetAvailableLanguageVersions();
            foreach (var command in commands)
            {
                if (command.LanguageVersion == currentLanguageVersion)
                {
                    command.Checked = true;
                }
                command.OnChecked += HandleChecked;
                command.Visible = availableVersions.Contains(command.LanguageVersion);
                commandService.AddCommand(command);
            }
        }
    }
}
