using Microsoft.EntityFrameworkCore;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using AutoMapper;

namespace Drashyam.API.Services;

public interface IMerchandiseService
{
    Task<MerchandiseStoreDto> CreateStoreAsync(int channelId, MerchandiseStoreCreateDto createDto);
    Task<MerchandiseStoreDto?> GetStoreAsync(int storeId);
    Task<List<MerchandiseStoreDto>> GetChannelStoresAsync(int channelId);
    Task<MerchandiseStoreDto> UpdateStoreAsync(int storeId, MerchandiseStoreUpdateDto updateDto);
    Task<bool> DeleteStoreAsync(int storeId);
    Task<bool> ReorderStoresAsync(int channelId, List<int> storeIds);
    Task<bool> ToggleStoreStatusAsync(int storeId, bool isActive);
    Task<bool> ToggleStoreFeaturedAsync(int storeId, bool isFeatured);
}

public class MerchandiseService : IMerchandiseService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MerchandiseService> _logger;

    public MerchandiseService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<MerchandiseService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MerchandiseStoreDto> CreateStoreAsync(int channelId, MerchandiseStoreCreateDto createDto)
    {
        try
        {
            // Verify channel exists
            var channel = await _context.Channels.FindAsync(channelId);
            if (channel == null)
            {
                throw new ArgumentException("Channel not found");
            }

            var store = new MerchandiseStore
            {
                ChannelId = channelId,
                StoreName = createDto.StoreName,
                Platform = createDto.Platform,
                StoreUrl = createDto.StoreUrl,
                Description = createDto.Description,
                LogoUrl = createDto.LogoUrl,
                IsActive = createDto.IsActive,
                IsFeatured = createDto.IsFeatured,
                DisplayOrder = createDto.DisplayOrder
            };

            _context.MerchandiseStores.Add(store);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created merchandise store {StoreId} for channel {ChannelId}", store.Id, channelId);
            return _mapper.Map<MerchandiseStoreDto>(store);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchandise store for channel {ChannelId}", channelId);
            throw;
        }
    }

    public async Task<MerchandiseStoreDto?> GetStoreAsync(int storeId)
    {
        try
        {
            var store = await _context.MerchandiseStores
                .Include(s => s.Channel)
                .FirstOrDefaultAsync(s => s.Id == storeId);

            return store != null ? _mapper.Map<MerchandiseStoreDto>(store) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise store {StoreId}", storeId);
            throw;
        }
    }

    public async Task<List<MerchandiseStoreDto>> GetChannelStoresAsync(int channelId)
    {
        try
        {
            var stores = await _context.MerchandiseStores
                .Where(s => s.ChannelId == channelId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<MerchandiseStoreDto>>(stores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise stores for channel {ChannelId}", channelId);
            throw;
        }
    }

    public async Task<MerchandiseStoreDto> UpdateStoreAsync(int storeId, MerchandiseStoreUpdateDto updateDto)
    {
        try
        {
            var store = await _context.MerchandiseStores.FindAsync(storeId);
            if (store == null)
            {
                throw new ArgumentException("Store not found");
            }

            // Update fields if provided
            if (updateDto.StoreName != null)
                store.StoreName = updateDto.StoreName;
            if (updateDto.Platform.HasValue)
                store.Platform = updateDto.Platform.Value;
            if (updateDto.StoreUrl != null)
                store.StoreUrl = updateDto.StoreUrl;
            if (updateDto.Description != null)
                store.Description = updateDto.Description;
            if (updateDto.LogoUrl != null)
                store.LogoUrl = updateDto.LogoUrl;
            if (updateDto.IsActive.HasValue)
                store.IsActive = updateDto.IsActive.Value;
            if (updateDto.IsFeatured.HasValue)
                store.IsFeatured = updateDto.IsFeatured.Value;
            if (updateDto.DisplayOrder.HasValue)
                store.DisplayOrder = updateDto.DisplayOrder.Value;

            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated merchandise store {StoreId}", storeId);
            return _mapper.Map<MerchandiseStoreDto>(store);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise store {StoreId}", storeId);
            throw;
        }
    }

    public async Task<bool> DeleteStoreAsync(int storeId)
    {
        try
        {
            var store = await _context.MerchandiseStores.FindAsync(storeId);
            if (store == null)
            {
                return false;
            }

            _context.MerchandiseStores.Remove(store);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted merchandise store {StoreId}", storeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting merchandise store {StoreId}", storeId);
            throw;
        }
    }

    public async Task<bool> ReorderStoresAsync(int channelId, List<int> storeIds)
    {
        try
        {
            var stores = await _context.MerchandiseStores
                .Where(s => s.ChannelId == channelId)
                .ToListAsync();

            for (int i = 0; i < storeIds.Count; i++)
            {
                var store = stores.FirstOrDefault(s => s.Id == storeIds[i]);
                if (store != null)
                {
                    store.DisplayOrder = i;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reordered merchandise stores for channel {ChannelId}", channelId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering merchandise stores for channel {ChannelId}", channelId);
            throw;
        }
    }

    public async Task<bool> ToggleStoreStatusAsync(int storeId, bool isActive)
    {
        try
        {
            var store = await _context.MerchandiseStores.FindAsync(storeId);
            if (store == null)
            {
                return false;
            }

            store.IsActive = isActive;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled merchandise store {StoreId} status to {IsActive}", storeId, isActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling merchandise store {StoreId} status", storeId);
            throw;
        }
    }

    public async Task<bool> ToggleStoreFeaturedAsync(int storeId, bool isFeatured)
    {
        try
        {
            var store = await _context.MerchandiseStores.FindAsync(storeId);
            if (store == null)
            {
                return false;
            }

            store.IsFeatured = isFeatured;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled merchandise store {StoreId} featured status to {IsFeatured}", storeId, isFeatured);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling merchandise store {StoreId} featured status", storeId);
            throw;
        }
    }
}
