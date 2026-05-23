using FutureVet.Domain.Common;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Domain.Entities;

public class Consultas : BaseEntity
{
    public string TipoConsulta
    { get; private set; }

    public DateTime Data
    { get; private set; }

    public TimeSpan Hora
    { get; private set; }

    public string Local
    { get; private set; }


    public Guid PetId
    { get; private set; }

    public Pet Pet
    { get; private set; }


    private Consultas() { }


    public Consultas(
        string tipoConsulta,
        DateTime data,
        TimeSpan hora,
        string local,
        Guid petId)
    {
        AtualizarTipoConsulta(
            tipoConsulta);

        DefinirData(
            data);

        DefinirHora(
            hora);

        AtualizarLocal(
            local);

        PetId = petId;
    }


    public void AtualizarTipoConsulta(
        string tipo)
    {
        if (string.IsNullOrWhiteSpace(
                tipo))
            throw new DomainException(
                "Consulta inválida.");

        TipoConsulta = tipo;

        AtualizarData();
    }


    public void DefinirData(
        DateTime data)
    {
        Data = data;

        AtualizarData();
    }


    public void DefinirHora(
        TimeSpan hora)
    {
        Hora = hora;

        AtualizarData();
    }


    public void AtualizarLocal(
        string local)
    {
        if (string.IsNullOrWhiteSpace(
                local))
            throw new DomainException(
                "Local inválido.");

        Local = local;

        AtualizarData();
    }
}