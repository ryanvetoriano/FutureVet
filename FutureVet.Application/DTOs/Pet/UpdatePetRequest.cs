using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.Pet;

public record UpdatePetRequest(
    string NomePet,
    string? Raca,
    int Idade,
    PortePet Tamanho,
    decimal Peso
);