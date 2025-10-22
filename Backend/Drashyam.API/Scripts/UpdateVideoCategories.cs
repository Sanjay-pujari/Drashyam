using Drashyam.API.Data;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Scripts;

public static class UpdateVideoCategories
{
    public static async Task UpdateExistingVideos(DrashyamDbContext context)
    {
        var videos = await context.Videos.ToListAsync();
        
        foreach (var video in videos)
        {
            if (string.IsNullOrEmpty(video.Category))
            {
                // Assign categories based on video title or description
                if (video.Title.ToLower().Contains("music") || video.Title.ToLower().Contains("song"))
                {
                    video.Category = "Music";
                    video.Tags = "music,song,performance";
                }
                else if (video.Title.ToLower().Contains("game") || video.Title.ToLower().Contains("gaming"))
                {
                    video.Category = "Gaming";
                    video.Tags = "gaming,tutorial,strategy";
                }
                else if (video.Title.ToLower().Contains("tech") || video.Title.ToLower().Contains("technology"))
                {
                    video.Category = "Technology";
                    video.Tags = "technology,review,gadgets";
                }
                else if (video.Title.ToLower().Contains("learn") || video.Title.ToLower().Contains("tutorial") || video.Title.ToLower().Contains("guide"))
                {
                    video.Category = "Education";
                    video.Tags = "tutorial,beginner,guide";
                }
                else
                {
                    video.Category = "Entertainment";
                    video.Tags = "fun,comedy,entertainment";
                }
            }
        }
        
        await context.SaveChangesAsync();
    }
}
