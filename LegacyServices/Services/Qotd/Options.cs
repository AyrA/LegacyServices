using LegacyServices.Validation;

namespace LegacyServices.Services.Qotd;

internal class Options : IEnable, IValidateable
{
    private static readonly string[] defaultQuotes =
    [
        "Connection reset by peer.",
        "Boss makes a dollar, I make a dime, that's why I poop on company time",
        "Being aware of what toilet paper is, is knowledge. Looking for paper BEFORE taking a dump is wisdom",
        "Sometimes the most important thing a person can ever say is \"I was here\"",
        "The difference between an oral thermometer and an anal thermometer is the taste",
        "If hard work makes you rich then show me a rich donkey - Russian Proverb",
        "Be liberal in what you accept, and conservative in what you send - Postel's law"
    ];

    public bool Enabled { get; set; }

    public string[]? Quotes { get; set; }

    public bool AllowExcessiveLength { get; set; }

    public string GetQuote()
    {
        Validate();
        var list = Quotes ?? defaultQuotes;
        return list[Random.Shared.Next(list.Length)];
    }

    public void Validate()
    {
        if (Quotes == null)
        {
            return;
        }
        if (Quotes.Length == 0)
        {
            throw new ValidationException("Quote list is empty");
        }
        if (Quotes.Any(string.IsNullOrWhiteSpace))
        {
            throw new ValidationException("Quote list contains blank quotes");
        }
        if (!AllowExcessiveLength && Quotes.Any(m => m.Length >= 512))
        {
            throw new ValidationException("Quote list contains qote qith excessive length");
        }
    }
}
