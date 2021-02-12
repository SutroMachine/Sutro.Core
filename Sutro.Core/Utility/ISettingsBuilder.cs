namespace Sutro.Core.Utility
{
    public interface ISettingsBuilder
    {
        void ApplyJSONFile(string settingFile);

        void ApplyJSONSnippet(string json);
    }

    public interface ISettingsBuilder<TSettings> : ISettingsBuilder
    {
        TSettings Settings { get; }
    }
}