using System.Text;
using Backend.API.Data;
using Backend.API.Entities;
using Backend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    : ControllerBase
{
    [HttpPost("New")]
    [Authorize]
    public async Task<ActionResult<BaseApiResponse<AnalysisResponse>>> AnalyzeLab([FromBody] AnalysisInput input)
    {
        var user = await userManager.FindByIdAsync(input.UserId);
        if (user == null) return NotFound("User not found");

        var labFiles = await PersistLab(input, user.Id);
        var result = await PerformDuplicationAnalysis(labFiles, context);
        return Ok(new BaseApiResponse<AnalysisResponse>(result));
    }

    private async Task<List<LabFile>> PersistLab(AnalysisInput input, string userId)
    {
        var lab = new Lab
        {
            Id = Guid.NewGuid().ToString(),
            Name = input.Name,
            UserId = userId
        };

        var labFiles = input.FilesContent.Select(file => new LabFile
        {
            Id = Guid.NewGuid().ToString(),
            FileContent = Encoding.UTF8.GetString(Convert.FromBase64String(file.Value)),
            Name = file.Key,
            Lab = lab // tie file to created lab
        }).ToList();

        await context.Labs.AddAsync(lab);
        await context.LabFiles.AddRangeAsync(labFiles);

        await context.SaveChangesAsync();

        return labFiles;
    }

    private async Task<AnalysisResponse> PerformDuplicationAnalysis(List<LabFile> otherLabFiles,
        ApplicationDbContext context)
    {
        // Extract user IDs from otherLabFiles
        var userIds = otherLabFiles.Select(f => f.Lab.UserId).Distinct().ToList();

        // Load in memory all files that do not belong to the same user
        var allLabFiles = await context.LabFiles
            .Include(lf => lf.Lab) // Include the Lab to access the UserId
            .Where(lf => !userIds.Contains(lf.Lab.UserId) && !otherLabFiles.Select(f => f.Id).Contains(lf.Id))
            .ToListAsync();

        var allFileContents = PreprocessFiles(allLabFiles);
        var otherFileContents = PreprocessFiles(otherLabFiles);

        // Compare files for duplications

        var result = new AnalysisResponse();
        foreach (var file in allLabFiles)
        {
            foreach (var otherFile in otherLabFiles)
            {
                if (file.Id == otherFile.Id) continue; // impossible since IDs are filtered above

                var otherFileContent = otherFileContents[otherFile.Id];
                var duplicates = allFileContents[file.Id].Intersect(otherFileContent);
                if (!duplicates.Any()) continue;

                var duplicatedLines = duplicates.ToList();
                result.Matches.Add(new LabFileMatch
                {
                    FileId = file.Id,
                    DuplicateWith = otherFile.Id,
                    DuplicatedLines = duplicatedLines,
                    DuplicatePercentage = duplicatedLines.Count / (float)otherFileContent.Count * 100
                });
            }
        }

        return result;
    }

    private Dictionary<string, HashSet<string>> PreprocessFiles(List<LabFile> files)
    {
        var fileContents = new Dictionary<string, HashSet<string>>();
        foreach (var file in files)
        {
            fileContents[file.Id] = new HashSet<string>(
                file.FileContent
                    .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
            );
        }

        return fileContents;
    }

    [HttpGet("All")]
    [Authorize]
    public async Task<ActionResult<BaseApiResponse<LabsResponse>>> GetAllLabs()
    {
        try
        {
            var labs = await context.Labs.Select(l => new LabVM
            {
                Id = l.Id,
                UserId = l.UserId,
                Name = l.Name,
                NumberOfFiles = (uint)context.LabFiles.Count(lf => lf.LabId == l.Id)
            }).ToListAsync();

            return Ok(new BaseApiResponse<LabsResponse>(new LabsResponse { Labs = labs }));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}