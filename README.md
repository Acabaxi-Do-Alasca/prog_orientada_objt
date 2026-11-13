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
| `Presentation` | Interação com o usuário (Windows Forms) |

A persistência em arquivo texto é intencional: a disciplina de banco de dados ainda não foi cursada pela turma. A camada de Persistência foi isolada por trás de repositórios para permitir migração futura para um banco relacional sem impacto nas demais camadas.

## Documentação

- [Backlog inicial](docs/backlog.md)
- [Diagrama de classes (esboço)](docs/diagrama-classes.md)

## Estrutura do código

- `src/ControleFinanceiro.Model` — entidades de domínio (`LancamentoFinanceiro`, `Receita`, `Despesa`, `Categoria`, `Orcamento`, `ItemOrcamento`, `ComparacaoOrcamento`).
- `src/ControleFinanceiro.Persistence` — interface `IRepositorioFinanceiro`; `RepositorioMemoria` (coleções genéricas `List`, `Dictionary`, `HashSet`) e `RepositorioArquivoTexto`, que herda dele e grava os dados em arquivos texto.
- `src/ControleFinanceiro.Service` — `ServicoFinanceiro`: regras de negócio (saldo, filtros, orçamento planejado x realizado, alertas de vencimento).
- `src/ControleFinanceiro.Presentation.WinForms` — tela em Windows Forms; monta as camadas em `Program.cs`.
- `tests/` — testes xUnit de cada camada (Model, Persistence e Service).

### Rodando o projeto

```bash
dotnet test
dotnet run --project src/ControleFinanceiro.Presentation.WinForms
```

Os dados ficam em `%LOCALAPPDATA%\ControleFinanceiroCeliaDoces` (`categorias.txt`, `lancamentos.txt`, `orcamentos.txt` e `configuracoes.txt`). Para usar outra pasta, por exemplo numa demonstração, passe o caminho como argumento:

```bash
dotnet run --project src/ControleFinanceiro.Presentation.WinForms -- "C:\caminho\da\pasta"
```

## Papéis do grupo

Os papéis são rotativos a cada sprint: Líder/Coordenador, Desenvolvedor, Revisor/QA, Documentador. A cada sprint, cada pessoa avança uma posição para a direita (Líder → Desenvolvedor → Revisor/QA → Documentador → Líder).

| Sprint | Líder/Coordenador | Desenvolvedor | Revisor/QA | Documentador |
|---|---|---|---|---|
| Sprint 0 | Kelvin | Leonardo Mendonça | Deivid | Bruno |
| Sprint 1 | Bruno | Kelvin | Leonardo Mendonça | Deivid |
| Sprint 2 | Deivid | Bruno | Kelvin | Leonardo Mendonça |
| Sprint 3 | Leonardo Mendonça | Deivid | Bruno | Kelvin |

## Roadmap de Sprints

- ✅ **Sprint 0 — Setup e Modelagem** (semanas 1-2): formação do grupo, escolha do projeto, setup do Git, interpretação das dores do cliente, esboço do diagrama de classes. Entrega: [README](README.md), [backlog inicial](docs/backlog.md) e [diagrama de classes](docs/diagrama-classes.md).
- ✅ **Sprint 1 — Domínio e Associações** (semanas 3-5): implementação das classes base, composição e agregação, gestão de identidade. Entrega: [`src/ControleFinanceiro.Model`](src/ControleFinanceiro.Model) e [testes](tests/ControleFinanceiro.Model.Tests) validando as associações.
- ✅ **Sprint 2 — Coleções e Windows Forms**: armazenamento das entidades em coleções genéricas (`List`, `Dictionary`, `HashSet`), filtros com `foreach`/`for`, introdução ao Windows Forms. Entrega: [`src/ControleFinanceiro.Persistence`](src/ControleFinanceiro.Persistence), [`src/ControleFinanceiro.Presentation.WinForms`](src/ControleFinanceiro.Presentation.WinForms) (tela de cadastro e listagem em `DataGridView`) e [testes](tests/ControleFinanceiro.Persistence.Tests). Peer review da sprint pendente.
- ✅ **Sprint 3 — Conclusão e entrega final**: sprint reservada pelo professor para finalizar o projeto. Foi entregue de uma só vez, por decisão do líder da sprint (ver abaixo). Entrega: [`src/ControleFinanceiro.Service`](src/ControleFinanceiro.Service), persistência em arquivo texto, os três pedidos da cliente completos, editar e excluir, prazo de alerta configurável, saldo realizado e previsto, 97 testes automatizados, documentação do projeto, slides, roteiro de instalação e executável pronto.

### Sprint 3 — decisão do líder e escopo entregue

**Líder/Coordenador da sprint:** Leonardo Mendonça.

Como o professor informou que esta sprint serviria apenas para terminar o projeto, sem conteúdo novo, o líder decidiu **consolidar numa única entrega** tudo o que o plano de ensino ainda previa (herança, polimorfismo e interfaces, arquitetura multicamadas, tratamento de exceções e persistência em arquivo texto), em vez de fatiar o trabalho em vários ciclos curtos. A decisão evita retrabalho entre ciclos, deixa a arquitetura final definida desde o início e permite que a documentação e os testes descrevam o sistema como ele realmente ficou.

O que a sprint entregou:

- **Camada Service** (`ServicoFinanceiro`) concentrando as regras de negócio, e a interface `IRepositorioFinanceiro` isolando a persistência.
- **Persistência em arquivo texto** (`RepositorioArquivoTexto`, que herda de `RepositorioMemoria` e sobrescreve `Salvar()`), com exceção própria (`ArquivoDeDadosException`) que informa o arquivo e a linha com problema.
- **Os três pedidos da cliente completos:** fluxo de caixa categorizado, comparação planejado x realizado por categoria e alertas de vencimento.
- **Acabamento para uso real:** editar e excluir categorias e lançamentos com regras de integridade, remover valor planejado, prazo do alerta configurável e saldo realizado separado do previsto.
- **Qualidade:** 97 testes automatizados (32 no Model, 27 na Persistence e 38 no Service), todos aprovados.
- **Entrega ao professor:** documentação do projeto com a evidência dos testes, slides da apresentação, roteiro de instalação e executável pronto para Windows.

Por ter sido uma entrega única, a implementação ficou concentrada no líder, e os demais integrantes participam da revisão, da documentação e da apresentação.

## Regras de funcionamento

- Reuniões semanais obrigatórias.
- Repositório Git compartilhado — pelo menos 3 commits individuais por sprint por membro.
- Documentação de decisões atualizada a cada sprint.
- Peer review ao final de cada sprint.
