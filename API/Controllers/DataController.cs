using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using Shared.DTOs.Response.MonthDetail;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    private readonly IDataService _dataService;

    public DataController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // ✅ Dashboard
    [HttpGet("dashboard/{year}")]
    public async Task<IActionResult> GetDashboardData(int year)
    {
        DashboardResponseDTO result = await _dataService.GetDashboardDataAsync(year);
        return Ok(result);
    }

    // ✅ MonthDetail
    [HttpGet("annualdetail/{year}")]
    public async Task<IActionResult> GetAnnualDetail(int year)
    {
        AnnualDetailResponseDTO result = await _dataService.GetAnnualDetailAsync(year);
        return Ok(result);
    }

    // ✅ HasData
    [HttpGet("has-data")]
    public async Task<IActionResult> GetHasData()
    {
        HasDataResponseDTO result = await _dataService.GetHasDataAsync();
        return Ok(result);
    }
}