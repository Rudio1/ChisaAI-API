using ChisaApi.Application.ExpenseCategories;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Requests;
using ChisaApi.Application.ExpenseCategories.DataTransfer.Responses;
using ChisaApi.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChisaApi.Web.Controllers.ExpenseCategories;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly ExpenseCategoryAppService _categories;

    public ExpenseCategoriesController(ExpenseCategoryAppService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryDto>>> List(CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        IReadOnlyList<ExpenseCategoryDto> list = await _categories.ListAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        ExpenseCategoryDto? c = await _categories.GetAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryDto>> Create([FromBody] CreateExpenseCategoryDto dto, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            ExpenseCategoryDto c = await _categories.CreateAsync(userId.Value, dto, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryDto>> Update(Guid id, [FromBody] UpdateExpenseCategoryDto dto, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            ExpenseCategoryDto? c = await _categories.UpdateAsync(userId.Value, id, dto, cancellationToken).ConfigureAwait(false);
            return c is null ? NotFound() : Ok(c);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            bool ok = await _categories.SoftDeleteAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
