namespace ChisaApi.Domain.WhatsApp;

public enum WhatsAppPendingStep
{
    AwaitingMenuChoice = 0,
    /// <summary>Usuário escolheu opção 3: aguarda número da lista de categorias.</summary>
    AwaitingCategoryListPick = 1
}
