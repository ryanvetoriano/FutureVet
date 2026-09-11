using Xunit;

// Os testes de integracao sobem hosts ASP.NET Core completos neste mesmo processo, e mais de
// um deles ao mesmo tempo quando as collections rodam em paralelo (a compartilhada, a de banco
// indisponivel e as de servico externo).
//
// O MeterProvider do OpenTelemetry escuta os meters pelo NOME ("Microsoft.AspNetCore.Hosting"),
// que e igual em todos os hosts. Com varios hosts vivos simultaneamente, qual provider
// reivindica cada instrumento passa a ser nao deterministico, e o /metrics de um host pode
// deixar de publicar as metricas do trafego que ele mesmo atendeu.
//
// Em producao existe um unico host por processo, entao isso nao afeta a aplicacao: e um
// artefato de rodar varios hosts em um processo de teste. Serializar as collections elimina a
// sobreposicao e mantem as assercoes de metricas verificando o comportamento real.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
