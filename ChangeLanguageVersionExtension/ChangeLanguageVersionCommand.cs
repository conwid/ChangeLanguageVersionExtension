using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ChangeLanguageVersionExtension.ProjectVersionServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ChangeLanguageVersionExtension
{
  
    internal sealed class ChangeLanguageVersionCommand : OleMenuCommand
    {
        private readonly IProjectVersionService projectVersionService;
        private readonly AsyncPackage package;
        public string LanguageVersion { get; }
        public event EventHandler OnChecked;

        public ChangeLanguageVersionCommand(AsyncPackage package, IProjectVersionService projectVersionService, string languageVersion, CommandID id) : base(Execute, id)
        {
            this.package = package;
            this.projectVersionService = projectVersionService;
            this.LanguageVersion = languageVersion;          
        }


        private static void Execute(object sender, EventArgs e) => ((ChangeLanguageVersionCommand)sender).ExecuteInternal();
        private void ExecuteInternal()
        {
            projectVersionService.SetLanguageVersion(LanguageVersion);
            this.Checked = true;
        }

        public override bool Checked
        {
            get => base.Checked;
            set
            {
                base.Checked = value;
                if (value)
                    OnChecked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
