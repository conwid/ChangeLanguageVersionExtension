using System.Collections.Generic;

namespace ChangeLanguageVersionExtension.ProjectVersionServices
{
    public interface IProjectVersionService
    {
        HashSet<string> GetAvailableLanguageVersions();
        string GetLanguageVersion();
        void SetLanguageVersion(string version);
    }
}