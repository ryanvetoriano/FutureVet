namespace FutureVet.Application.DTOs.Consulta;

public record UpdateConsultaRequest(
    DateTime Data,
    string Hora,     
    string Local
);