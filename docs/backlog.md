# Backlog Inicial — Controle Financeiro (Célia Doces)

Levantado a partir da interpretação das dores do cliente descritas no plano de ensino: fluxo de caixa categorizado, comparação com orçamento planejado e alertas de vencimento.

## Dores do cliente (interpretação)

- Célia não consegue enxergar para onde vai o dinheiro do negócio — precisa que receitas e despesas sejam categorizadas.
- Não há comparação entre o planejado e o realizado — Célia não sabe se está estourando o orçamento do mês.
- Contas a pagar/receber vencem sem aviso prévio.

## Product Backlog

### Sprint 0 — Setup e Modelagem
- [x] Escolher o projeto (Projeto 9 — Controle Financeiro).
- [x] Configurar repositório Git compartilhado.
- [x] Interpretar as dores do cliente e levantar requisitos iniciais.
- [x] Esboçar o diagrama de classes do domínio.

### Sprint 1 — Domínio e Associações
- [x] Modelar `LancamentoFinanceiro` (base) e as especializações `Receita` e `Despesa`.
- [x] Modelar `Categoria` e associá-la aos lançamentos (agregação — categoria existe independente do lançamento).
- [x] Modelar `Orcamento` composto por `ItemOrcamento` (composição — item de orçamento não existe sem o orçamento).
- [x] Definir gestão de identidade das entidades (ex.: Id único por lançamento/orçamento).
- [x] Validar as associações com testes automatizados (xUnit).

### Backlog futuro (a detalhar nos próximos sprints, conforme plano de ensino)
- [ ] Consultas com coleções genéricas e LINQ (ex.: total de despesas por categoria, saldo do período).
- [ ] Comparação entre orçamento planejado e realizado, com cálculo de desvio.
- [ ] Alertas de vencimento de lançamentos.
- [ ] Herança/polimorfismo para tipos de lançamento e classes abstratas/interfaces onde fizer sentido.
- [ ] Camada de persistência em arquivo texto (repositórios).
- [ ] Estrutura multicamadas (Model, Service, Persistence, Presentation).
- [ ] Tratamento de exceções, sobrecarga e sobreposição de membros.
- [ ] Relatório técnico final.

## Definição de Pronto (Sprint 0)

- Repositório criado com README, backlog inicial e diagrama de classes versionados no Git.
