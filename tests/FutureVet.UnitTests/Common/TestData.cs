using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;

namespace FutureVet.UnitTests.Common;

/// <summary>
/// Fábricas de dados válidos para a seção Arrange dos testes.
/// Cada método devolve um objeto que passa em todas as regras de domínio,
/// permitindo que cada teste altere apenas o campo que pretende exercitar.
/// </summary>
public static class TestData
{
    public static Usuario UsuarioValido(
        string nome = "Ryan Vetoriano",
        string email = "ryan@futurevet.com",
        string senha = "SenhaSegura123",
        string cpf = "12345678901",
        string telefone = "11999998888")
        => new(nome, email, senha, cpf, telefone);

    public static Pet PetValido(
        string nome = "Thor",
        EspeciePet especie = EspeciePet.Cao,
        string? raca = "Labrador",
        int idade = 3,
        PortePet tamanho = PortePet.Grande,
        decimal peso = 32.5m,
        Guid? usuarioId = null)
        => new(nome, especie, raca, idade, tamanho, peso, usuarioId ?? Guid.NewGuid());

    public static Vacina VacinaValida(
        string nome = "V10",
        DateTime? dataAplicacao = null,
        DateTime? proximaDose = null,
        string local = "Clínica Central",
        Guid? petId = null)
    {
        var aplicacao = dataAplicacao ?? new DateTime(2026, 1, 10);

        return new Vacina(
            nome,
            aplicacao,
            proximaDose ?? aplicacao.AddYears(1),
            local,
            petId ?? Guid.NewGuid());
    }

    public static Consulta ConsultaValida(
        string tipo = "Rotina",
        DateTime? data = null,
        string hora = "14:30",
        string local = "Clínica Central",
        Guid? petId = null)
        => new(
            tipo,
            data ?? new DateTime(2026, 3, 15),
            hora,
            local,
            petId ?? Guid.NewGuid());

    public static CreateUsuarioRequest CreateUsuarioRequestValido()
        => new("Ryan Vetoriano", "ryan@futurevet.com", "SenhaSegura123", "12345678901", "11999998888");

    public static UpdateUsuarioRequest UpdateUsuarioRequestValido()
        => new("Ryan V. Atualizado", "11977776666");

    public static CreatePetRequest CreatePetRequestValido(Guid? usuarioId = null)
        => new("Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 32.5m, usuarioId ?? Guid.NewGuid());

    public static UpdatePetRequest UpdatePetRequestValido()
        => new("Thor Atualizado", "Golden Retriever", 4, PortePet.Grande, 34.0m);

    public static CreateVacinaRequest CreateVacinaRequestValido(Guid? petId = null)
        => new("V10", new DateTime(2026, 1, 10), new DateTime(2027, 1, 10), "Clínica Central", petId ?? Guid.NewGuid());

    public static UpdateVacinaRequest UpdateVacinaRequestValido()
        => new(new DateTime(2027, 6, 10), "Clínica Norte");

    public static CreateConsultaRequest CreateConsultaRequestValido(Guid? petId = null)
        => new("Rotina", new DateTime(2026, 3, 15), "14:30", "Clínica Central", petId ?? Guid.NewGuid());

    public static UpdateConsultaRequest UpdateConsultaRequestValido()
        => new(new DateTime(2026, 4, 20), "09:00", "Clínica Norte");
}
