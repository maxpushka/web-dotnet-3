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

        var submissions = context.LabSubmissions
            .Where(submission => submission.UserId == user.Id)
            .Select(submission => new
            {
                submission.Id, submission.UserId,
                LabName = submission.Name, submission.SubmissionDate
            })
            .ToList();
        return Ok(submissions);
    }
}