using Praktik.Domain.Entities;
using Praktik.Application.DTO;
namespace Praktik.Application.Interfaces;
public interface IDocumentService
{
    Task<Document> CreateDocumentAsync(Document doc, List<DocumentItemDto> items);
}