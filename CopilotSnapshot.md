# Copilot Development Snapshot

- Generated On: 2026-08-05 13:56:04
- Solution Root: C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI

## 0. Snapshot Inputs

- Interface Path: `src\Tessera.Nexus.AI.Application\Contracts\IBusinessRuleRepository.cs`
- Entity Path: `src\Tessera.Nexus.AI.Domain\Entities\BusinessRule.cs`
- Additional Files:
  - `src\Tessera.Nexus.AI.Infrastructure\Repositories\BusinessRuleRepository.cs`
  - `src\Tessera.Nexus.AI.Web\Components\Pages\BusinessRules.razor`
  - `src\Tessera.Nexus.AI.Infrastructure\DependencyInjection\ServiceCollectionExtensions.cs`

## 1. Current Solution Tree

```text
+-- database
    +-- Sprint01
        |-- 001_Create_NexusAI_Database.sql
+-- docs
    |-- Architecture.md
    |-- Roadmap.md
+-- scripts
    |-- Generate-CopilotSnapshot.ps1
+-- src
    +-- Tessera.Nexus.AI.Application
        +-- Contracts
            |-- .gitkeep
            |-- ApplicationSetting.cs
            |-- IApplicationSettingRepository.cs
            |-- IAuditRepository.cs
            |-- IBusinessKnowledgeRepository.cs
            |-- IBusinessRuleRepository.cs
            |-- IDatabaseHealthCheckService.cs
            |-- IDbConnectionFactory.cs
            |-- IMetadataRefreshService.cs
            |-- IMetadataRepository.cs
            |-- IPromptTemplateRepository.cs
            |-- IQueryFilterRepository.cs
        +-- DTOs
            |-- .gitkeep
        +-- Services
            |-- .gitkeep
        |-- Tessera.Nexus.AI.Application.csproj
    +-- Tessera.Nexus.AI.Audit
        +-- DependencyInjection
            |-- .gitkeep
        +-- Entities
            |-- .gitkeep
        +-- Repositories
            |-- .gitkeep
        +-- Services
            |-- .gitkeep
        |-- Class1.cs
        |-- Tessera.Nexus.AI.Audit.csproj
    +-- Tessera.Nexus.AI.Domain
        +-- Entities
            |-- .gitkeep
            |-- ApplicationEventLog.cs
            |-- BusinessKnowledge.cs
            |-- BusinessRule.cs
            |-- EpicorDataField.cs
            |-- EpicorDataSet.cs
            |-- EpicorDataTable.cs
            |-- EpicorRelation.cs
            |-- MetadataRefreshLog.cs
            |-- PromptTemplate.cs
            |-- QueryFilter.cs
        +-- Enums
            |-- .gitkeep
        +-- ValueObjects
            |-- .gitkeep
        |-- Tessera.Nexus.AI.Domain.csproj
    +-- Tessera.Nexus.AI.Infrastructure
        +-- Database
            |-- .gitkeep
            |-- DatabaseHealthCheckService.cs
            |-- SqlConnectionFactory.cs
        +-- DependencyInjection
            |-- .gitkeep
            |-- ServiceCollectionExtensions.cs
        +-- Repositories
            |-- .gitkeep
            |-- ApplicationSettingRepository.cs
            |-- BusinessKnowledgeRepository.cs
            |-- BusinessRuleRepository.cs
            |-- PromptTemplateRepository.cs
        |-- Tessera.Nexus.AI.Infrastructure.csproj
    +-- Tessera.Nexus.AI.Metadata
        +-- DependencyInjection
            |-- .gitkeep
        +-- Entities
            |-- .gitkeep
        +-- Repositories
            |-- .gitkeep
        +-- Services
            |-- .gitkeep
        |-- Class1.cs
        |-- Tessera.Nexus.AI.Metadata.csproj
    +-- Tessera.Nexus.AI.Security
        +-- DependencyInjection
            |-- .gitkeep
        +-- Entities
            |-- .gitkeep
        +-- Repositories
            |-- .gitkeep
        +-- Services
            |-- .gitkeep
        |-- Class1.cs
        |-- Tessera.Nexus.AI.Security.csproj
    +-- Tessera.Nexus.AI.Shared
        +-- Constants
            |-- .gitkeep
        +-- Models
            |-- .gitkeep
        +-- Results
            |-- .gitkeep
            |-- OperationResult.cs
            |-- OperationResultGeneric.cs
        |-- Class1.cs
        |-- Tessera.Nexus.AI.Shared.csproj
    +-- Tessera.Nexus.AI.Web
        +-- Components
            +-- Layout
                |-- MainLayout.razor
                |-- MainLayout.razor.css
                |-- NavMenu.razor
                |-- NavMenu.razor.css
                |-- ReconnectModal.razor
                |-- ReconnectModal.razor.css
                |-- ReconnectModal.razor.js
            +-- Pages
                |-- BusinessKnowledgePage.razor
                |-- BusinessRules.razor
                |-- Counter.razor
                |-- Error.razor
                |-- Health.razor
                |-- Home.razor
                |-- NotFound.razor
                |-- PromptTemplates.razor
                |-- Settings.razor
                |-- Weather.razor
            |-- _Imports.razor
            |-- App.razor
            |-- Routes.razor
        +-- Properties
            |-- launchSettings.json
        +-- Services
            |-- .gitkeep
        +-- ViewModels
            |-- .gitkeep
        +-- wwwroot
            +-- lib
                +-- bootstrap
                    +-- dist
                        +-- css
                            |-- bootstrap.css
                            |-- bootstrap.css.map
                            |-- bootstrap.min.css
                            |-- bootstrap.min.css.map
                            |-- bootstrap.rtl.css
                            |-- bootstrap.rtl.css.map
                            |-- bootstrap.rtl.min.css
                            |-- bootstrap.rtl.min.css.map
                            |-- bootstrap-grid.css
                            |-- bootstrap-grid.css.map
                            |-- bootstrap-grid.min.css
                            |-- bootstrap-grid.min.css.map
                            |-- bootstrap-grid.rtl.css
                            |-- bootstrap-grid.rtl.css.map
                            |-- bootstrap-grid.rtl.min.css
                            |-- bootstrap-grid.rtl.min.css.map
                            |-- bootstrap-reboot.css
                            |-- bootstrap-reboot.css.map
                            |-- bootstrap-reboot.min.css
                            |-- bootstrap-reboot.min.css.map
                            |-- bootstrap-reboot.rtl.css
                            |-- bootstrap-reboot.rtl.css.map
                            |-- bootstrap-reboot.rtl.min.css
                            |-- bootstrap-reboot.rtl.min.css.map
                            |-- bootstrap-utilities.css
                            |-- bootstrap-utilities.css.map
                            |-- bootstrap-utilities.min.css
                            |-- bootstrap-utilities.min.css.map
                            |-- bootstrap-utilities.rtl.css
                            |-- bootstrap-utilities.rtl.css.map
                            |-- bootstrap-utilities.rtl.min.css
                            |-- bootstrap-utilities.rtl.min.css.map
                        +-- js
                            |-- bootstrap.bundle.js
                            |-- bootstrap.bundle.js.map
                            |-- bootstrap.bundle.min.js
                            |-- bootstrap.bundle.min.js.map
                            |-- bootstrap.esm.js
                            |-- bootstrap.esm.js.map
                            |-- bootstrap.esm.min.js
                            |-- bootstrap.esm.min.js.map
                            |-- bootstrap.js
                            |-- bootstrap.js.map
                            |-- bootstrap.min.js
                            |-- bootstrap.min.js.map
            |-- app.css
            |-- favicon.png
        |-- appsettings.Development.json
        |-- appsettings.json
        |-- Program.cs
        |-- Tessera.Nexus.AI.Web.csproj
        |-- Tessera.Nexus.AI.Web.csproj.user
|-- .gitignore
|-- README.md
|-- Tessera.Nexus.AI.slnx
```

## 2. Current Build Status

```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  Tessera.Nexus.AI.Shared -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Shared\bin\Debug\net10.0\Tessera.Nexus.AI.Shared.dll
  Tessera.Nexus.AI.Domain -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Domain\bin\Debug\net10.0\Tessera.Nexus.AI.Domain.dll
  Tessera.Nexus.AI.Application -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Application\bin\Debug\net10.0\Tessera.Nexus.AI.Application.dll
  Tessera.Nexus.AI.Security -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Security\bin\Debug\net10.0\Tessera.Nexus.AI.Security.dll
  Tessera.Nexus.AI.Infrastructure -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Infrastructure\bin\Debug\net10.0\Tessera.Nexus.AI.Infrastructure.dll
  Tessera.Nexus.AI.Metadata -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Metadata\bin\Debug\net10.0\Tessera.Nexus.AI.Metadata.dll
  Tessera.Nexus.AI.Audit -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Audit\bin\Debug\net10.0\Tessera.Nexus.AI.Audit.dll
  Tessera.Nexus.AI.Web -> C:\Users\brian.hall.SKO\source\repos\Tessera.Nexus.AI\src\Tessera.Nexus.AI.Web\bin\Debug\net10.0\Tessera.Nexus.AI.Web.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.21
```

## 3. Git Status

```text
 M src/Tessera.Nexus.AI.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
?? CopilotSnapshot.md
?? SolutionTree.txt
?? scripts/
?? src/Tessera.Nexus.AI.Infrastructure/Repositories/BusinessRuleRepository.cs
?? src/Tessera.Nexus.AI.Web/Components/Pages/BusinessRules.razor
```

## 4. Recent Git History

```text
ab5e14c feat: add business knowledge management
4cb1010 feat: add prompt template administration
41f82b2 feat: add ApplicationSetting repository and settings page
e7e83a2 feat: add connection factory and database health check
6184f6d feat: add application contracts
f0afd44 feat: add Sprint 1 NexusAI database foundation
d05ad92 chore: create Tessera Nexus AI solution scaffold
```

## 5. Git Changed Files

### Unstaged Changes

```text
src/Tessera.Nexus.AI.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
```

### Staged Changes

```text
```

## 6. Git Diff Summary

```text
 .../DependencyInjection/ServiceCollectionExtensions.cs                   | 1 +
 1 file changed, 1 insertion(+)
```

## 7. Interface Being Implemented

**File:** `src\Tessera.Nexus.AI.Application\Contracts\IBusinessRuleRepository.cs`

```csharp
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IBusinessRuleRepository
{
    Task<BusinessRule?> GetByIdAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessRule>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessRule>> GetByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default);
}
```

## 8. Entity Used By That Interface

**File:** `src\Tessera.Nexus.AI.Domain\Entities\BusinessRule.cs`

```csharp
namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class BusinessRule
{
    public int BusinessRuleId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public string RuleType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
```

## 9. Additional Source Files


## 9.1 Additional File

**File:** `src\Tessera.Nexus.AI.Infrastructure\Repositories\BusinessRuleRepository.cs`

```csharp
using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class BusinessRuleRepository : IBusinessRuleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BusinessRuleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<BusinessRule?> GetByIdAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessRule
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<BusinessRule>(
            sql,
            new
            {
                BusinessRuleId = businessRuleId
            });
    }

    public async Task<IReadOnlyList<BusinessRule>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessRule
            WHERE IsActive = 1
              AND (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CAST(SYSUTCDATETIME() AS date)
                  )
              AND (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CAST(SYSUTCDATETIME() AS date)
                  )
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(sql);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessRule>> GetByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessRule
            WHERE IsActive = 1
              AND SemanticDomain = @SemanticDomain
              AND (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CAST(SYSUTCDATETIME() AS date)
                  )
              AND (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CAST(SYSUTCDATETIME() AS date)
                  )
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(
            sql,
            new
            {
                SemanticDomain = semanticDomain
            });

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessRule>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessRule
            WHERE
                RuleName LIKE @Search
                OR RuleType LIKE @Search
                OR RuleCategory LIKE @Search
                OR RuleDescription LIKE @Search
                OR RuleLogicDescription LIKE @Search
                OR PromptInstruction LIKE @Search
                OR SemanticDomain LIKE @Search
                OR AppliesToTerm LIKE @Search
                OR AppliesToObjectType LIKE @Search
                OR AppliesToObjectName LIKE @Search
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(
            sql,
            new
            {
                Search = $"%{searchText}%"
            });

        return results.ToList();
    }

    public async Task<int> CreateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO ai.BusinessRule
            (
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            )
            VALUES
            (
                @RuleName,
                @RuleType,
                @RuleCategory,
                @RuleDescription,
                @RuleLogicDescription,
                @SqlPredicate,
                @PromptInstruction,
                @SemanticDomain,
                @Company,
                @Plant,
                @AppliesToTerm,
                @AppliesToObjectType,
                @AppliesToObjectName,
                @OverridesBusinessRuleId,
                @Priority,
                @IsSystemRule,
                @IsActive,
                @EffectiveDate,
                @ExpirationDate,
                @VersionNumber,
                @ApprovedBy,
                @ApprovedDateUtc,
                @CreatedBy,
                SYSUTCDATETIME(),
                @ModifiedBy,
                @ModifiedDateUtc
            );

            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        return await connection.ExecuteScalarAsync<int>(
            sql,
            rule);
    }

    public async Task UpdateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessRule
            SET
                RuleName = @RuleName,
                RuleType = @RuleType,
                RuleCategory = @RuleCategory,
                RuleDescription = @RuleDescription,
                RuleLogicDescription = @RuleLogicDescription,
                SqlPredicate = @SqlPredicate,
                PromptInstruction = @PromptInstruction,
                SemanticDomain = @SemanticDomain,
                Company = @Company,
                Plant = @Plant,
                AppliesToTerm = @AppliesToTerm,
                AppliesToObjectType = @AppliesToObjectType,
                AppliesToObjectName = @AppliesToObjectName,
                OverridesBusinessRuleId = @OverridesBusinessRuleId,
                Priority = @Priority,
                IsSystemRule = @IsSystemRule,
                IsActive = @IsActive,
                EffectiveDate = @EffectiveDate,
                ExpirationDate = @ExpirationDate,
                VersionNumber = @VersionNumber,
                ApprovedBy = @ApprovedBy,
                ApprovedDateUtc = @ApprovedDateUtc,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            rule);
    }

    public async Task DeleteAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessRule
            SET
                IsActive = 0,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                BusinessRuleId = businessRuleId
            });
    }
}
```

## 9.2 Additional File

**File:** `src\Tessera.Nexus.AI.Web\Components\Pages\BusinessRules.razor`

```razor
<h3>BusinessRules</h3>

@code {

}
```

## 9.3 Additional File

**File:** `src\Tessera.Nexus.AI.Infrastructure\DependencyInjection\ServiceCollectionExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

using Tessera.Nexus.AI.Application.Contracts;

using Tessera.Nexus.AI.Infrastructure.Database;
using Tessera.Nexus.AI.Infrastructure.Repositories;

namespace Tessera.Nexus.AI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // Database

        services.AddScoped<IDbConnectionFactory,
                           SqlConnectionFactory>();

        services.AddScoped<IDatabaseHealthCheckService,
                           DatabaseHealthCheckService>();

        // Repositories

        services.AddScoped<IApplicationSettingRepository,
                           ApplicationSettingRepository>();

        services.AddScoped<IPromptTemplateRepository,
                           PromptTemplateRepository>();

        services.AddScoped <IBusinessKnowledgeRepository,
            BusinessKnowledgeRepository > ();

        services.AddScoped<IBusinessRuleRepository, BusinessRuleRepository>();

        return services;
    }
}
```

## 10. Suggested PCC Prompt Context

Use this snapshot when asking for PCC changes.

Recommended prompt format:

```text
PCC <FileName>

Use the attached CopilotSnapshot.md as the source of truth.
Generate the complete file only.
Do not assume missing entity, interface, repository, or Razor properties.
```
