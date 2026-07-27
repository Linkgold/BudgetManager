using Contracts.Enums;
using Shared.DTOs.Response;
using UI.Models;

namespace UI.Extensions
{
    public static class TransactionMappingExtensions
    {
        public static TransactionModel ToTransactionModel(this TransactionResponseDTO dto)
        {
            return TransactionModel.FromDTO(dto);
        }

        public static List<TransactionModel> ToTransactionModelList(this IEnumerable<TransactionResponseDTO> dtos)
        {
            return dtos
                .Select(dto => dto.ToTransactionModel())
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}