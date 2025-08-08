using LegacyServices.Validation;
using System.Globalization;

namespace LegacyServices.Services.Daytime;

internal class Options : IEnable, IValidateable
{
    public bool Enabled { get; set; }

    public string? Format { get; set; }

    public bool UseUtc { get; set; }

    public string GetDate()
    {
        Validate();
        var dt = UseUtc ? DateTime.UtcNow : DateTime.Now;
        return dt.ToString(Format ?? "f", CultureInfo.InvariantCulture);
    }

    public void Validate()
    {
        if (Format != null)
        {
            try
            {
                DateTime.UtcNow.ToString(Format);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Date format validation failed. {ex.Message}", ex);
            }
        }
    }
}
