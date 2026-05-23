using FutureVet.Domain.Common;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Domain.Entities;

public class Pet : BaseEntity
{
    public string NomePet { get; private set; }

    public EspeciePet Especie
        { get; private set; }

    public string? Raca
        { get; private set; }

    public int Idade
        { get; private set; }

    public PortePet Tamanho
        { get; private set; }

    public decimal Peso
        { get; private set; }


    public Guid UsuarioId
        { get; private set; }

    public Usuario Usuario
        { get; private set; }


    public ICollection<Vacinas> Vacinas
        { get; private set; }
        = new List<Vacinas>();

    public ICollection<Consultas> Consultas
        { get; private set; }
        = new List<Consultas>();


    private Pet() { }


    public Pet(
        string nomePet,
        EspeciePet especie,
        string? raca,
        int idade,
        PortePet tamanho,
        decimal peso,
        Guid usuarioId)
    {
        AtualizarNome(nomePet);

        DefinirEspecie(especie);

        AtualizarRaca(raca);

        DefinirIdade(idade);

        DefinirPorte(tamanho);

        AtualizarPeso(peso);

        UsuarioId = usuarioId;
    }


    public void AtualizarNome(
        string nome)
    {
        if (string.IsNullOrWhiteSpace(
            nome))
            throw new DomainException(
                "Nome inválido.");

        NomePet = nome;

        AtualizarData();
    }


    public void DefinirEspecie(
        EspeciePet especie)
    {
        Especie = especie;

        AtualizarData();
    }


    public void AtualizarRaca(
        string? raca)
    {
        Raca = raca;

        AtualizarData();
    }


    public void DefinirIdade(
        int idade)
    {
        if (idade < 0)
            throw new DomainException(
                "Idade inválida.");

        Idade = idade;

        AtualizarData();
    }


    public void DefinirPorte(
        PortePet porte)
    {
        Tamanho = porte;

        AtualizarData();
    }


    public void AtualizarPeso(
        decimal peso)
    {
        if (peso <= 0)
            throw new DomainException(
                "Peso inválido.");

        Peso = peso;

        AtualizarData();
    }
}