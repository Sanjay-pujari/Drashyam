using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class MerchandiseStoreDto
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public StorePlatform Platform { get; set; }
    public string StoreUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MerchandiseStoreCreateDto
{
    public string StoreName { get; set; } = string.Empty;
    public StorePlatform Platform { get; set; }
    public string StoreUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}

public class MerchandiseStoreUpdateDto
{
    public string? StoreName { get; set; }
    public StorePlatform? Platform { get; set; }
    public string? StoreUrl { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
}

public class MerchandiseStoreListDto
{
    public List<MerchandiseStoreDto> Stores { get; set; } = new List<MerchandiseStoreDto>();
    public int TotalCount { get; set; }
}

