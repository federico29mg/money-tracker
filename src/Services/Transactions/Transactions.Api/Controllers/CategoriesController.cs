using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Transactions.Api.Authentication;
using Transactions.Api.Contracts;
using Transactions.Application.Features.Categories;
using Transactions.Application.Features.Categories.ArchiveCategory;
using Transactions.Application.Features.Categories.CreateCategory;
using Transactions.Application.Features.Categories.GetCategories;
using Transactions.Application.Features.Categories.GetCategory;
using Transactions.Application.Features.Categories.RenameCategory;
using Transactions.Domain.Common;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CategoriesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(
        [FromServices] GetCategoriesHandler handler,
        TransactionType? type = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        Ok(await handler.HandleAsync(
            new GetCategoriesQuery(User.GetUserId(), type, includeArchived), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetCategory(
        Guid id,
        [FromServices] GetCategoryHandler handler,
        CancellationToken cancellationToken) =>
        await handler.HandleAsync(new GetCategoryQuery(User.GetUserId(), id), cancellationToken);

    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedResponse>> CreateCategory(
        CreateCategoryRequest request,
        [FromServices] CreateCategoryHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new CreateCategoryCommand(User.GetUserId(), request.Name, request.Type), cancellationToken);

        return CreatedAtAction(nameof(GetCategory), new { id }, new CreatedResponse(id));
    }

    [HttpPut("{id:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RenameCategory(
        Guid id,
        RenameRequest request,
        [FromServices] RenameCategoryHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RenameCategoryCommand(User.GetUserId(), id, request.Name), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveCategory(
        Guid id,
        [FromServices] ArchiveCategoryHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ArchiveCategoryCommand(User.GetUserId(), id), cancellationToken);

        return NoContent();
    }
}