using LegacyServices.Validation;

namespace LegacyServices.Services.Message;

public class Options : IValidateable, IEnable
{
    public bool Enabled { get; set; }

    public bool Discard { get; set; }

    public bool VersionA { get; set; }

    public bool VersionB { get; set; }

    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }
        if (!VersionA && !VersionB)
        {
            throw new ValidationException("At least version A or B must be enabled");
        }
    }
}
