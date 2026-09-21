# Sistema de Controle Financeiro — Célia Doces

Projeto da disciplina **Programação Orientada a Objetos II** (Engenharia de Software — 2026.2, Prof. Evandro Cesar Estevam), desenvolvido segundo a metodologia **PBL híbrido com scaffolding**.

## Projeto 9 — Controle Financeiro

**Cliente:** Célia Doces

**Problema a solucionar:** a cliente precisa de um sistema para registrar seu fluxo de caixa de forma categorizada, comparar os gastos/receitas com um orçamento planejado e ser avisada sobre lançamentos próximos do vencimento.

**Objetivo do sistema:** categorizar despesas e receitas automaticamente e monitorar o desvio em relação ao orçamento planejado.

## Arquitetura

Aplicação em **C# / .NET**, estruturada em camadas:

| Camada | Responsabilidade |
|---|---|
| `Model` | Entidades de domínio (lançamentos, categorias, orçamentos) |
| `Service` | Regras de negócio (cálculo de saldo, comparação com orçamento, alertas) |
| `Persistence` | Repositórios em **arquivo texto** (sem banco de dados nesta etapa do curso) |
| `Presentation` | Interação com o usuário (console) |

A persistência em arquivo texto é intencional: a disciplina de banco de dados ainda não foi cursada pela turma. A camada de Persistência foi isolada por trás de repositórios para permitir migração futura para um banco relacional sem impacto nas demais camadas.

## Documentação

- [Backlog inicial](docs/backlog.md)
- [Diagrama de classes (esboço)](docs/diagrama-classes.md)

## Estrutura do código

- `src/ControleFinanceiro.Model` — entidades de domínio (`LancamentoFinanceiro`, `Receita`, `Despesa`, `Categoria`, `Orcamento`, `ItemOrcamento`).
- `src/ControleFinanceiro.Persistence` — armazenamento das entidades em coleções genéricas (`List`, `Dictionary`, `HashSet`), etapa intermediária antes da persistência em arquivo texto.
- `src/ControleFinanceiro.Presentation.WinForms` — tela (Windows Forms) de cadastro de categorias/lançamentos e listagem em `DataGridView`.
- `tests/ControleFinanceiro.Model.Tests` — testes xUnit que validam as associações (composição, agregação) e a gestão de identidade.
- `tests/ControleFinanceiro.Persistence.Tests` — testes xUnit do armazenamento em coleções (cadastro, busca por Id, filtros e totais).

### Rodando o projeto

```bash
dotnet test
dotnet run --project src/ControleFinanceiro.Presentation.WinForms
```

## Papéis do grupo

Os papéis são rotativos a cada sprint: Líder/Coordenador, Desenvolvedor, Revisor/QA, Documentador. A cada sprint, cada pessoa avança uma posição para a direita (Líder → Desenvolvedor → Revisor/QA → Documentador → Líder).

| Sprint | Líder/Coordenador | Desenvolvedor | Revisor/QA | Documentador |
|---|---|---|---|---|
| Sprint 0 | Kelvin | Leonardo Mendonça | Daivid | Bruno |
| Sprint 1 | Bruno | Kelvin | Leonardo Mendonça | Daivid |
| Sprint 2 | Daivid | Bruno | Kelvin | Leonardo Mendonça |

## Roadmap de Sprints

- ✅ **Sprint 0 — Setup e Modelagem** (semanas 1-2): formação do grupo, escolha do projeto, setup do Git, interpretação das dores do cliente, esboço do diagrama de classes. Entrega: [README](README.md), [backlog inicial](docs/backlog.md) e [diagrama de classes](docs/diagrama-classes.md).
- ✅ **Sprint 1 — Domínio e Associações** (semanas 3-5): implementação das classes base, composição e agregação, gestão de identidade. Entrega: [`src/ControleFinanceiro.Model`](src/ControleFinanceiro.Model) e [testes](tests/ControleFinanceiro.Model.Tests) validando as associações.
- ✅ **Sprint 2 — Coleções e Windows Forms**: armazenamento das entidades em coleções genéricas (`List`, `Dictionary`, `HashSet`), filtros com `foreach`/`for`, introdução ao Windows Forms. Entrega: [`src/ControleFinanceiro.Persistence`](src/ControleFinanceiro.Persistence), [`src/ControleFinanceiro.Presentation.WinForms`](src/ControleFinanceiro.Presentation.WinForms) (tela de cadastro e listagem em `DataGridView`) e [testes](tests/ControleFinanceiro.Persistence.Tests). Peer review da sprint pendente.
- ⏳ Próximos sprints seguem o plano de ensino da disciplina (herança/polimorfismo/interfaces, arquitetura multicamadas, tratamento de exceções, persistência em arquivo texto).

## Regras de funcionamento

- Reuniões semanais obrigatórias.
- Repositório Git compartilhado — pelo menos 3 commits individuais por sprint por membro.
- Documentação de decisões atualizada a cada sprint.
- Peer review ao final de cada sprint.
