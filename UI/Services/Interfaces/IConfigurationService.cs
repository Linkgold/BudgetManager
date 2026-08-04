using UI.Configuration;

namespace UI.Services.Interfaces
{
    public interface IConfigurationService
    {
        ApiConfiguration Api { get; }
    }
}