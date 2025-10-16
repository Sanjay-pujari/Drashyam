namespace Drashyam.API.DTOs;

public enum VideoStatus
{
    Draft,
    Processing,
    Published,
    Private,
    Unlisted,
    Deleted
}

public enum VideoType
{
    Regular,
    Short,
    Live,
    Premier
}

public enum VideoVisibility
{
    Public,
    Private,
    Unlisted
}

public enum ChannelType
{
    Personal,
    Business,
    Creator,
    Brand
}

public enum SubscriptionType
{
    Free,
    Basic,
    Premium,
    Pro
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Yearly
}
