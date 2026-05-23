namespace FutureVet.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; private set; }
        = Guid.NewGuid();

    public bool Disponivel { get; private set; }
        = true;

    public DateTime DataCriacao { get; private set; }
        = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; private set; }

    public void Activate()
    {
        Disponivel = true;
        AtualizarData();
    }

    public void Deactivate()
    {
        Disponivel = false;
        AtualizarData();
    }

    protected void AtualizarData()
    {
        DataAtualizacao = DateTime.UtcNow;
    }
}