using LegacyServices.Validation;

namespace LegacyServices.Services.Chargen;

internal class Options : IEnable, IValidateable
{
    public bool Enabled { get; set; }

    public int LineDelay { get; set; }

    public int LineLimit { get; set; }

    public bool GlobalDelay { get; set; }

    public void Validate()
    {
        if (LineDelay < 0)
        {
            throw new ValidationException("LineDelay cannot be negative");
        }
        if (LineLimit < 0)
        {
            throw new ValidationException("LineLimit cannot be negative");
        }
        if (LineLimit == 0 && GlobalDelay)
        {
            throw new ValidationException("GlobalDelay requires LineLimit to not be zero");
        }
    }
}
