using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLanguageVersionExtension.ProjectVersionServices
{
    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Member is only called from the Main thread", Scope = "member", Target = "~M:ChangeLanguageVersionExtension.ProjectVersionService.SetLanguageVersion(System.String)")]
    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Member is only called from the Main thread", Scope = "member", Target = "~M:ChangeLanguageVersionExtension.ProjectVersionService.GetLanguageVersion()")]
    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Member is only called from the Main thread", Scope = "member", Target = "~M:ChangeLanguageVersionExtension.ProjectVersionService.GetActiveProject()")]
    public class ProjectVersionService : IProjectVersionService
    {
        private const string devenvExe = "devenv.exe";
        private const string langVersionIndexer = "LanguageVersion";
        private readonly DTE2 dte;
        public ProjectVersionService(DTE2 dte)
        {
            this.dte = dte;
        }
        private Project GetActiveProject()
        {
            if (dte.ActiveSolutionProjects is Array activeSolutionProjects && activeSolutionProjects.Length > 0)
            {
                return activeSolutionProjects.GetValue(0) as Project;
            }

            Document doc = dte.ActiveDocument;
            if (doc != null && !string.IsNullOrEmpty(doc.FullName))
            {
                return dte.Solution?.FindProjectItem(doc.FullName)?.ContainingProject ?? throw new Exception("Cannot get current project");
            }

            throw new Exception("Cannot get current project");
        }
        private Version GetVisualStudioVersion()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, devenvExe);
            var fvi = FileVersionInfo.GetVersionInfo(path);

            string verName = fvi.ProductVersion;
            for (int i = 0; i < verName.Length; i++)
            {
                if (!char.IsDigit(verName, i) && verName[i] != '.')
                {
                    verName = verName.Substring(0, i);
                    break;
                }
            }
            return new Version(verName);
        }

        // see https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/langversion-compiler-option
        public HashSet<string> GetAvailableLanguageVersions()
        {
            var version = GetVisualStudioVersion();
            var availableVersions = new HashSet<string> { LanguageVersions.Default, LanguageVersions.Latest, LanguageVersions.CSharp5, LanguageVersions.CSharp6 };
            if (version.Major>=16)
            {
                availableVersions.Add(LanguageVersions.LatestMajor);
                availableVersions.Add(LanguageVersions.CSharp8);
                availableVersions.Add(LanguageVersions.Preview);
            }
            else if (version.Major >= 15)
            {
                availableVersions.Add(LanguageVersions.CSharp7);
                if (version.Minor >= 3)
                {
                    availableVersions.Add(LanguageVersions.CSharp71);
                }
                if (version.Minor >= 5)
                {
                    availableVersions.Add(LanguageVersions.CSharp72);
                }
                if (version.Minor >= 7)
                {
                    availableVersions.Add(LanguageVersions.CSharp73);
                }
            }
            return availableVersions;
        }

        public void SetLanguageVersion(string version)
        {
            var project = GetActiveProject();
            project.ConfigurationManager.ActiveConfiguration.Properties.Item(langVersionIndexer).Value = version;
            project.Save();
        }

        public string GetLanguageVersion() =>
            GetActiveProject()?.ConfigurationManager?.ActiveConfiguration
                              ?.Properties?.Item(langVersionIndexer)?.Value as string ?? LanguageVersions.Default;
        

    }
}
