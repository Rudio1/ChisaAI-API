using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChisaApi.Application.Expenses;
using ChisaApi.Application.Expenses.DataTransfer.Requests;
using ChisaApi.Application.Expenses.DataTransfer.Responses;
using ChisaApi.Web.Http;

namespace ChisaApi.Web.Controllers.Expenses;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseAppService _expenses;

    public ExpensesController(ExpenseAppService expenses)
    {
        _expenses = expenses;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> List(CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        IReadOnlyList<ExpenseDto> list = await _expenses.ListAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        ExpenseDto? e = await _expenses.GetAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            ExpenseDto e = await _expenses.CreateAsync(userId.Value, dto, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { id = e.Id }, e);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(Guid id, [FromBody] UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            ExpenseDto? e = await _expenses.UpdateAsync(userId.Value, id, dto, cancellationToken).ConfigureAwait(false);
            return e is null ? NotFound() : Ok(e);
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

        bool ok = await _expenses.SoftDeleteAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);
        return ok ? NoContent() : NotFound();
    }
}
