using LegacyServices.Validation;

namespace LegacyServices.Services.Pwdgen;

internal class Options : IEnable, IValidateable
{
    public bool Enabled { get; set; }

    public int Count { get; set; }

    public int Length { get; set; }

    public void Validate()
    {
        if (Length < 0)
        {
            throw new ValidationException("Password length cannot be negative");
        }
        if (Length > 72)
        {
            throw new ValidationException("Password length cannot exceed 78");
        }
        if (Count < 0)
        {
            throw new ValidationException("Password count cannot be negative");
        }
    }
}
