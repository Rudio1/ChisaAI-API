using System.Globalization;
using System.Text;
using ChisaApi.Application.Abstractions;
using ChisaApi.Application.ExpenseCategories.DataTransfers.Requests;
using ChisaApi.Application.ExpenseCategories.DataTransfers.Responses;
using ChisaApi.Application.ExpenseCategories.Services;
using ChisaApi.Application.Expenses.DataTransfers.Requests;
using ChisaApi.Application.Expenses.Services;
using ChisaApi.Application.Utilities;
using ChisaApi.Domain.Expenses.Entities;
using ChisaApi.Domain.Expenses.Interfaces;
using ChisaApi.Domain.Expenses.ServiceDomain;
using ChisaApi.Domain.Users;
using ChisaApi.Domain.WhatsApp;
using ChisaApi.Domain.WhatsApp.Entities;

namespace ChisaApi.Application.Integrations.WhatsApp;

public sealed class WhatsAppExpenseAppService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly TimeSpan PendingTtl = TimeSpan.FromHours(24);

    private readonly IUserRepository _users;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IWhatsAppPendingConversationRepository _pending;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExpenseCategoryAppService _categoryApp;
    private readonly ExpenseAppService _expenses;
    private readonly ExpenseCategoryDomainService _categoryDomain;

    public WhatsAppExpenseAppService(
        IUserRepository users,
        IExpenseCategoryRepository categoryRepository,
        IWhatsAppPendingConversationRepository pending,
        IUnitOfWork unitOfWork,
        ExpenseCategoryAppService categoryApp,
        ExpenseAppService expenses,
        ExpenseCategoryDomainService categoryDomain)
    {
        _users = users;
        _categoryRepository = categoryRepository;
        _pending = pending;
        _unitOfWork = unitOfWork;
        _categoryApp = categoryApp;
        _expenses = expenses;
        _categoryDomain = categoryDomain;
    }

    public async Task<WhatsAppInboundFlowResponse> ProcessMessageAsync(
        string waIdDigits,
        string messageText,
        CancellationToken cancellationToken = default)
    {
        string phone = PhoneNumberNormalizer.ToBrazilE164(waIdDigits);
        var user = await _users.GetByPhoneAsync(phone, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "user_not_found",
                ErrorCode = "USER_NOT_FOUND",
                WhatsAppReplyText = "Não encontrei cadastro com este número. Entre no app Chisa e registre-se com o mesmo WhatsApp."
            };
        }

        WhatsAppPendingConversation? pendingRead = await _pending
            .GetLatestActiveByPhoneAsync(phone, cancellationToken)
            .ConfigureAwait(false);

        bool parsedShorthand = ExpenseShorthandParser.TryParse(messageText, out decimal parsedAmount, out string parsedCategory);

        if (parsedShorthand)
        {
            await _pending.DeleteAllForPhoneAsync(phone, cancellationToken).ConfigureAwait(false);
            return await StartShorthandFlowAsync(user.Id, phone, parsedAmount, parsedCategory, cancellationToken)
                .ConfigureAwait(false);
        }

        if (pendingRead is null)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "invalid_format",
                ErrorCode = "INVALID_FORMAT",
                WhatsAppReplyText =
                    "Envie no formato: VALOR e nome da categoria.\nExemplo: 150 mercado\n(Use ponto ou vírgula nos centavos, ex: 20,50 uber)"
            };
        }

        WhatsAppPendingConversation pendingTracked = (await _pending
            .GetLatestActiveTrackedByPhoneAsync(phone, cancellationToken)
            .ConfigureAwait(false))!;

        return pendingTracked.Step switch
        {
            WhatsAppPendingStep.AwaitingMenuChoice => await HandleMenuChoiceAsync(
                user.Id,
                phone,
                pendingTracked,
                messageText,
                cancellationToken).ConfigureAwait(false),
            WhatsAppPendingStep.AwaitingCategoryListPick => await HandleCategoryListPickAsync(
                user.Id,
                phone,
                pendingTracked,
                messageText,
                cancellationToken).ConfigureAwait(false),
            _ => new WhatsAppInboundFlowResponse
            {
                Outcome = "error",
                ErrorCode = "UNKNOWN_STEP",
                WhatsAppReplyText = "Algo deu errado. Tente de novo com o formato: 150 mercado"
            }
        };
    }

    private async Task<WhatsAppInboundFlowResponse> StartShorthandFlowAsync(
        Guid userId,
        string phoneE164,
        decimal amount,
        string categoryHint,
        CancellationToken cancellationToken)
    {
        string normalizedName = _categoryDomain.NormalizeName(categoryHint);
        try
        {
            _categoryDomain.ValidateName(normalizedName);
        }
        catch (ArgumentException ex)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "validation_error",
                ErrorCode = "INVALID_CATEGORY_NAME",
                WhatsAppReplyText = ex.Message
            };
        }

        ExpenseCategory? existing = await _categoryRepository
            .FindActiveByNameForUserAsync(userId, normalizedName, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
            return await CreateExpenseAndReplyAsync(userId, amount, existing.Id, note: null, cancellationToken)
                .ConfigureAwait(false);

        var row = new WhatsAppPendingConversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PhoneE164 = phoneE164,
            Step = WhatsAppPendingStep.AwaitingMenuChoice,
            Amount = amount,
            CategoryNameUnderReview = normalizedName,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(PendingTtl)
        };

        await _pending.AddAsync(row, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new WhatsAppInboundFlowResponse
        {
            Outcome = "awaiting_choice",
            WhatsAppReplyText = BuildCategoryMissingMenu(amount, normalizedName)
        };
    }

    private static string BuildCategoryMissingMenu(decimal amount, string categoryName) =>
        $"A categoria *{categoryName.ToUpperInvariant()}* não existe.\n\n"
        + $"Valor: {FormatMoney(amount)}\n\n"
        + "O que deseja fazer?\n"
        + "1 - Sim, criar a categoria e registrar o gasto\n"
        + "2 - Não, cancelar\n"
        + "3 - Usar outra categoria já cadastrada";

    private static string FormatMoney(decimal amount) =>
        amount.ToString("C", PtBr);

    private async Task<WhatsAppInboundFlowResponse> HandleMenuChoiceAsync(
        Guid userId,
        string phoneE164,
        WhatsAppPendingConversation pending,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (!TryParseMenuChoice(messageText, out int choice))
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "awaiting_choice",
                WhatsAppReplyText =
                    "Responda apenas com *1*, *2* ou *3*.\n\n"
                    + BuildCategoryMissingMenu(pending.Amount, pending.CategoryNameUnderReview)
            };
        }

        if (choice == 2)
        {
            await _pending.DeleteAsync(pending.Id, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "cancelled",
                WhatsAppReplyText = "Ok, cancelado. Quando quiser, envie de novo no formato: 150 mercado"
            };
        }

        if (choice == 1)
        {
            try
            {
                var dto = new CreateExpenseCategoryDto(pending.CategoryNameUnderReview);
                ExpenseCategoryDto createdCat = await _categoryApp
                    .CreateAsync(userId, dto, cancellationToken)
                    .ConfigureAwait(false);

                await _pending.DeleteAsync(pending.Id, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return await CreateExpenseAndReplyAsync(userId, pending.Amount, createdCat.Id, note: null, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return new WhatsAppInboundFlowResponse
                {
                    Outcome = "validation_error",
                    ErrorCode = "CATEGORY_CREATE_FAILED",
                    WhatsAppReplyText = ex.Message
                };
            }
        }

        IReadOnlyList<ExpenseCategory> categories = await _categoryRepository
            .ListByUserAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (categories.Count == 0)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "awaiting_choice",
                WhatsAppReplyText =
                    "Você ainda não tem nenhuma categoria cadastrada no app.\n\n"
                    + $"Valor: {FormatMoney(pending.Amount)}\n"
                    + $"Categoria desejada: *{pending.CategoryNameUnderReview.ToUpperInvariant()}*\n\n"
                    + "Responda:\n"
                    + "1 - Sim, criar essa categoria e registrar o gasto\n"
                    + "2 - Não, cancelar"
            };
        }

        pending.Step = WhatsAppPendingStep.AwaitingCategoryListPick;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new WhatsAppInboundFlowResponse
        {
            Outcome = "awaiting_category_pick",
            WhatsAppReplyText = BuildNumberedCategoryList(pending.Amount, categories)
        };
    }

    private static string BuildNumberedCategoryList(decimal amount, IReadOnlyList<ExpenseCategory> categories)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Valor do gasto: {FormatMoney(amount)}");
        sb.AppendLine();
        sb.AppendLine("Escolha a categoria pelo *número*:");
        for (int i = 0; i < categories.Count; i++)
            sb.AppendLine($"{i + 1} - {categories[i].Name}");
        sb.AppendLine();
        sb.AppendLine("Envie *0* para cancelar.");
        return sb.ToString().TrimEnd();
    }

    private async Task<WhatsAppInboundFlowResponse> HandleCategoryListPickAsync(
        Guid userId,
        string phoneE164,
        WhatsAppPendingConversation pending,
        string messageText,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ExpenseCategory> categories = await _categoryRepository
            .ListByUserAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        string trimmed = messageText.Trim();
        if (trimmed.Equals("cancelar", StringComparison.OrdinalIgnoreCase)
            || trimmed == "0")
        {
            await _pending.DeleteAsync(pending.Id, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "cancelled",
                WhatsAppReplyText = "Ok, cancelado."
            };
        }

        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pick)
            || pick < 1
            || pick > categories.Count)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "awaiting_category_pick",
                WhatsAppReplyText =
                    $"Número inválido. Envie um de *1* a *{categories.Count}*, ou *0* para cancelar.\n\n"
                    + BuildNumberedCategoryList(pending.Amount, categories)
            };
        }

        ExpenseCategory chosen = categories[pick - 1];
        await _pending.DeleteAsync(pending.Id, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await CreateExpenseAndReplyAsync(userId, pending.Amount, chosen.Id, note: null, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<WhatsAppInboundFlowResponse> CreateExpenseAndReplyAsync(
        Guid userId,
        decimal amount,
        Guid categoryId,
        string? note,
        CancellationToken cancellationToken)
    {
        var dto = new CreateExpenseDto(amount, categoryId, note, DateTimeOffset.UtcNow);
        try
        {
            var expense = await _expenses.CreateAsync(userId, dto, cancellationToken).ConfigureAwait(false);
            string catName = expense.CategoryName;
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "expense_created",
                Expense = expense,
                WhatsAppReplyText =
                    $"Gasto *{FormatMoney(amount)}* registrado para a categoria *{catName}*."
            };
        }
        catch (ArgumentException ex)
        {
            return new WhatsAppInboundFlowResponse
            {
                Outcome = "validation_error",
                ErrorCode = "EXPENSE_REJECTED",
                WhatsAppReplyText = ex.Message
            };
        }
    }

    private static bool TryParseMenuChoice(string text, out int choice)
    {
        choice = 0;
        string t = text.Trim();
        if (t.Length == 0)
            return false;
        char c = t[0];
        if (c is >= '1' and <= '3' && (t.Length == 1 || !char.IsDigit(t[1])))
        {
            choice = c - '0';
            return true;
        }

        return false;
    }
}
