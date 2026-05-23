namespace FutureVet.Application.DTOs.Vacina;

public record UpdateVacinaRequest(
    DateTime ProximaDose,
    string LocalAplicacao
);