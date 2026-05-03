namespace Shared;

public enum Country
{
    Russia,
    France,
    Germany,
    Uae,
    India
}

public enum Region
{
    Ru,
    Eu,
    Asia
}

public static class CountryRegionMapping
{
    public static Region GetRegion(Country country)
    {
        return country switch
        {
            Country.Russia => Region.Ru,
            Country.France => Region.Eu,
            Country.Germany => Region.Eu,
            Country.Uae => Region.Asia,
            Country.India => Region.Asia,
            _ => Region.Eu
        };
    }

    public static string GetRegionCode(Region region)
    {
        return region switch
        {
            Region.Ru => "RU",
            Region.Eu => "EU",
            Region.Asia => "ASIA",
            _ => "EU"
        };
    }
}