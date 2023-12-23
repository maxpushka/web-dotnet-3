using System.Text;
using Backend.API.Data;
using Backend.API.Entities;
using Backend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    : ControllerBase
{
    [HttpPost("New")]
    [Authorize]
    public async Task<IActionResult> AnalyzeLab([FromBody] AnalysisInput input)
    {
        var user = await userManager.FindByIdAsync(input.UserId);
        if (user == null) return NotFound("User not found");

        var lab = new Lab
        {
            Id = Guid.NewGuid().ToString(),
            Name = input.Name,
            UserId = user.Id
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

        var result = new AnalysisResponse();
        return Ok(result);
    }
}