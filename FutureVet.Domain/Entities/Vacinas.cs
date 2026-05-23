using FutureVet.Domain.Common;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Domain.Entities;

public class Vacinas : BaseEntity
{
    public string NomeVacina
    { get; private set; }

    public DateTime DataAplicacao
    { get; private set; }

    public DateTime ProximaDose
    { get; private set; }

    public string LocalAplicacao
    { get; private set; }


    public Guid PetId
    { get; private set; }

    public Pet Pet
    { get; private set; }


    private Vacinas() { }


    public Vacinas(
        string nomeVacina,
        DateTime dataAplicacao,
        DateTime proximaDose,
        string localAplicacao,
        Guid petId)
    {
        AtualizarNomeVacina(
            nomeVacina);

        DefinirAplicacao(
            dataAplicacao);

        AtualizarProximaDose(
            proximaDose);

        AtualizarLocal(
            localAplicacao);

        PetId = petId;
    }


    public void AtualizarNomeVacina(
        string nome)
    {
        if (string.IsNullOrWhiteSpace(
                nome))
            throw new DomainException(
                "Vacina inválida.");

        NomeVacina = nome;

        AtualizarData();
    }


    public void DefinirAplicacao(
        DateTime data)
    {
        DataAplicacao = data;

        AtualizarData();
    }


    public void AtualizarProximaDose(
        DateTime data)
    {
        if (data < DataAplicacao)
            throw new DomainException(
                "Próxima dose inválida.");

        ProximaDose = data;

        AtualizarData();
    }


    public void AtualizarLocal(
        string local)
    {
        if (string.IsNullOrWhiteSpace(
                local))
            throw new DomainException(
                "Local inválido.");

        LocalAplicacao = local;

        AtualizarData();
    }
}