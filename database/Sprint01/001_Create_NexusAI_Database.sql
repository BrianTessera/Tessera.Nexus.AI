/*
    Tessera Nexus AI
    Sprint 1 Database Foundation
    File: 001_Create_NexusAI_Database.sql
    Target: SQL Server

    Sprint 1 Scope:
        - Database and schemas
        - Configuration tables
        - Prompt template table
        - Epicor metadata cache tables
        - Business knowledge tables
        - Security foundation tables
        - Audit event table
        - Seed records for roles, settings, CMP, FAI, and PRG rules

    Notes:
        - Safe to re-run for existing objects.
        - Existing tables are not dropped.
        - Epicor Kinetic remains the source system.
        - Epicor ICE Z tables remain the source for schema metadata.
*/

USE master;
GO

IF DB_ID(N'NexusAI') IS NULL
BEGIN
    PRINT 'Creating database NexusAI...';
    CREATE DATABASE NexusAI;
END
ELSE
BEGIN
    PRINT 'Database NexusAI already exists.';
END
GO

ALTER DATABASE NexusAI SET RECOVERY SIMPLE;
GO

USE NexusAI;
GO

/* ============================================================
   Schemas
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'cfg') EXEC(N'CREATE SCHEMA cfg');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'meta') EXEC(N'CREATE SCHEMA meta');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'ai') EXEC(N'CREATE SCHEMA ai');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'sec') EXEC(N'CREATE SCHEMA sec');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'audit') EXEC(N'CREATE SCHEMA audit');
GO

/* ============================================================
   cfg.ApplicationSetting
   ============================================================ */

IF OBJECT_ID(N'cfg.ApplicationSetting', N'U') IS NULL
BEGIN
    CREATE TABLE cfg.ApplicationSetting
    (
        ApplicationSettingId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_cfg_ApplicationSetting PRIMARY KEY,
        SettingKey nvarchar(200) NOT NULL,
        SettingValue nvarchar(max) NULL,
        Description nvarchar(max) NULL,
        IsSensitive bit NOT NULL CONSTRAINT DF_cfg_ApplicationSetting_IsSensitive DEFAULT (0),
        IsActive bit NOT NULL CONSTRAINT DF_cfg_ApplicationSetting_IsActive DEFAULT (1),
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_cfg_ApplicationSetting_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_cfg_ApplicationSetting_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_cfg_ApplicationSetting_SettingKey UNIQUE (SettingKey)
    );
END
GO

/* ============================================================
   cfg.PromptTemplate
   ============================================================ */

IF OBJECT_ID(N'cfg.PromptTemplate', N'U') IS NULL
BEGIN
    CREATE TABLE cfg.PromptTemplate
    (
        PromptTemplateId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_cfg_PromptTemplate PRIMARY KEY,
        TemplateName nvarchar(200) NOT NULL,
        TemplateType nvarchar(100) NOT NULL,
        TemplateText nvarchar(max) NOT NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_cfg_PromptTemplate_VersionNumber DEFAULT (1),
        IsActive bit NOT NULL CONSTRAINT DF_cfg_PromptTemplate_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_cfg_PromptTemplate_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_cfg_PromptTemplate_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_cfg_PromptTemplate_Name_Version UNIQUE (TemplateName, VersionNumber)
    );

    CREATE INDEX IX_cfg_PromptTemplate_Active_Type
        ON cfg.PromptTemplate(IsActive, TemplateType, TemplateName);
END
GO

/* ============================================================
   meta.MetadataRefreshLog
   ============================================================ */

IF OBJECT_ID(N'meta.MetadataRefreshLog', N'U') IS NULL
BEGIN
    CREATE TABLE meta.MetadataRefreshLog
    (
        MetadataRefreshLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_MetadataRefreshLog PRIMARY KEY,
        RefreshType nvarchar(100) NOT NULL,
        SourceDatabaseName sysname NULL,
        StartedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_MetadataRefreshLog_StartedDateUtc DEFAULT (SYSUTCDATETIME()),
        CompletedDateUtc datetime2(3) NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_meta_MetadataRefreshLog_Status DEFAULT (N'Started'),
        DatasetRows int NULL,
        TableRows int NULL,
        FieldRows int NULL,
        RelationRows int NULL,
        RelationFieldRows int NULL,
        ErrorMessage nvarchar(max) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_meta_MetadataRefreshLog_CreatedBy DEFAULT (SUSER_SNAME()),
        CONSTRAINT CK_meta_MetadataRefreshLog_Status CHECK (Status IN (N'Started', N'Succeeded', N'Failed', N'Cancelled'))
    );
END
GO

/* ============================================================
   meta.EpicorDataSet - Source: Ice.ZDataSet
   ============================================================ */

IF OBJECT_ID(N'meta.EpicorDataSet', N'U') IS NULL
BEGIN
    CREATE TABLE meta.EpicorDataSet
    (
        EpicorDataSetId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_EpicorDataSet PRIMARY KEY,
        SystemCode nvarchar(50) NOT NULL,
        DataSetID nvarchar(200) NOT NULL,
        DataSetName nvarchar(300) NULL,
        Description nvarchar(max) NULL,
        TargetNamespace nvarchar(300) NULL,
        Version nvarchar(100) NULL,
        DSType nvarchar(50) NULL,
        SystemFlag bit NULL,
        EpicorSysRowID uniqueidentifier NULL,
        SourceSysRevID varbinary(8) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_meta_EpicorDataSet_IsActive DEFAULT (1),
        ImportedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_EpicorDataSet_ImportedDateUtc DEFAULT (SYSUTCDATETIME()),
        LastSeenDateUtc datetime2(3) NULL,
        CONSTRAINT UQ_meta_EpicorDataSet_SystemCode_DataSetID UNIQUE (SystemCode, DataSetID)
    );
END
GO

/* ============================================================
   meta.EpicorDataTable - Source: Ice.ZDataTable
   ============================================================ */

IF OBJECT_ID(N'meta.EpicorDataTable', N'U') IS NULL
BEGIN
    CREATE TABLE meta.EpicorDataTable
    (
        EpicorDataTableId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_EpicorDataTable PRIMARY KEY,
        SystemCode nvarchar(50) NOT NULL,
        DataTableID nvarchar(300) NOT NULL,
        Description nvarchar(max) NULL,
        SchemaName sysname NULL,
        DBTableName sysname NULL,
        WhereClause nvarchar(max) NULL,
        RestrictedByTer bit NULL,
        RestrictedByPlant bit NULL,
        FullSync bit NULL,
        TableType nvarchar(50) NULL,
        TableLabel nvarchar(300) NULL,
        BOUpdate bit NULL,
        UpdateMethod nvarchar(300) NULL,
        EpicorSysRowID uniqueidentifier NULL,
        SourceSysRevID varbinary(8) NULL,
        IsQueryable bit NOT NULL CONSTRAINT DF_meta_EpicorDataTable_IsQueryable DEFAULT (1),
        IsRestricted bit NOT NULL CONSTRAINT DF_meta_EpicorDataTable_IsRestricted DEFAULT (0),
        DataClassification nvarchar(100) NOT NULL CONSTRAINT DF_meta_EpicorDataTable_DataClassification DEFAULT (N'Internal'),
        RestrictionReason nvarchar(max) NULL,
        ImportedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_EpicorDataTable_ImportedDateUtc DEFAULT (SYSUTCDATETIME()),
        LastSeenDateUtc datetime2(3) NULL,
        CONSTRAINT UQ_meta_EpicorDataTable_SystemCode_DataTableID UNIQUE (SystemCode, DataTableID)
    );

    CREATE INDEX IX_meta_EpicorDataTable_DBObject
        ON meta.EpicorDataTable(SchemaName, DBTableName, IsQueryable, IsRestricted);
END
GO

/* ============================================================
   meta.EpicorDataField - Source: Ice.ZDataField
   ============================================================ */

IF OBJECT_ID(N'meta.EpicorDataField', N'U') IS NULL
BEGIN
    CREATE TABLE meta.EpicorDataField
    (
        EpicorDataFieldId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_EpicorDataField PRIMARY KEY,
        SystemCode nvarchar(50) NOT NULL,
        DataTableID nvarchar(300) NOT NULL,
        FieldName sysname NOT NULL,
        Seq int NULL,
        DBTableName sysname NULL,
        DBFieldName sysname NULL,
        DataType nvarchar(100) NULL,
        UseDBDefault bit NULL,
        Required bit NULL,
        ReadOnly bit NULL,
        Description nvarchar(max) NULL,
        Included bit NULL,
        FieldFormat nvarchar(300) NULL,
        FieldScale int NULL,
        FieldLabel nvarchar(300) NULL,
        ToolTipText nvarchar(max) NULL,
        IsDescriptionField bit NULL,
        LikeDataFieldSystemCode nvarchar(50) NULL,
        LikeDataFieldTableID nvarchar(300) NULL,
        LikeDataFieldName sysname NULL,
        BizType nvarchar(100) NULL,
        EpicorSysRowID uniqueidentifier NULL,
        SourceSysRevID varbinary(8) NULL,
        IsQueryable bit NOT NULL CONSTRAINT DF_meta_EpicorDataField_IsQueryable DEFAULT (1),
        IsRestricted bit NOT NULL CONSTRAINT DF_meta_EpicorDataField_IsRestricted DEFAULT (0),
        DataClassification nvarchar(100) NOT NULL CONSTRAINT DF_meta_EpicorDataField_DataClassification DEFAULT (N'Internal'),
        SensitivityCategory nvarchar(100) NULL,
        RestrictionReason nvarchar(max) NULL,
        ImportedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_EpicorDataField_ImportedDateUtc DEFAULT (SYSUTCDATETIME()),
        LastSeenDateUtc datetime2(3) NULL,
        CONSTRAINT UQ_meta_EpicorDataField_System_Table_Field UNIQUE (SystemCode, DataTableID, FieldName)
    );

    CREATE INDEX IX_meta_EpicorDataField_Table_Queryable
        ON meta.EpicorDataField(SystemCode, DataTableID, IsQueryable, IsRestricted);
END
GO

/* ============================================================
   meta.EpicorRelation - Source: Ice.ZRelation
   ============================================================ */

IF OBJECT_ID(N'meta.EpicorRelation', N'U') IS NULL
BEGIN
    CREATE TABLE meta.EpicorRelation
    (
        EpicorRelationId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_EpicorRelation PRIMARY KEY,
        SystemCode nvarchar(50) NOT NULL,
        DataSetID nvarchar(200) NOT NULL,
        RelationID nvarchar(300) NOT NULL,
        ParentSystemCode nvarchar(50) NULL,
        ParentDataTableID nvarchar(300) NULL,
        ChildSystemCode nvarchar(50) NULL,
        ChildDataTableID nvarchar(300) NULL,
        KeyID nvarchar(300) NULL,
        Description nvarchar(max) NULL,
        RelationType int NULL,
        EpicorSysRowID uniqueidentifier NULL,
        SourceSysRevID varbinary(8) NULL,
        IsApprovedForAI bit NOT NULL CONSTRAINT DF_meta_EpicorRelation_IsApprovedForAI DEFAULT (1),
        IsRestricted bit NOT NULL CONSTRAINT DF_meta_EpicorRelation_IsRestricted DEFAULT (0),
        RestrictionReason nvarchar(max) NULL,
        ImportedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_EpicorRelation_ImportedDateUtc DEFAULT (SYSUTCDATETIME()),
        LastSeenDateUtc datetime2(3) NULL,
        CONSTRAINT UQ_meta_EpicorRelation_System_DataSet_Relation UNIQUE (SystemCode, DataSetID, RelationID)
    );

    CREATE INDEX IX_meta_EpicorRelation_ParentChild
        ON meta.EpicorRelation(ParentDataTableID, ChildDataTableID, IsApprovedForAI);
END
GO

/* ============================================================
   meta.EpicorRelationField - Source: Ice.ZRelationField
   ============================================================ */

IF OBJECT_ID(N'meta.EpicorRelationField', N'U') IS NULL
BEGIN
    CREATE TABLE meta.EpicorRelationField
    (
        EpicorRelationFieldId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_meta_EpicorRelationField PRIMARY KEY,
        SystemCode nvarchar(50) NOT NULL,
        DataSetID nvarchar(200) NOT NULL,
        RelationID nvarchar(300) NOT NULL,
        Seq int NOT NULL,
        ParentFieldName sysname NOT NULL,
        ChildFieldName sysname NOT NULL,
        CompOp nvarchar(20) NULL,
        IsConst bit NULL,
        EpicorSysRowID uniqueidentifier NULL,
        SourceSysRevID varbinary(8) NULL,
        ImportedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_meta_EpicorRelationField_ImportedDateUtc DEFAULT (SYSUTCDATETIME()),
        LastSeenDateUtc datetime2(3) NULL,
        CONSTRAINT UQ_meta_EpicorRelationField_System_DataSet_Relation_Seq UNIQUE (SystemCode, DataSetID, RelationID, Seq)
    );
END
GO

/* ============================================================
   ai.BusinessKnowledge
   ============================================================ */

IF OBJECT_ID(N'ai.BusinessKnowledge', N'U') IS NULL
BEGIN
    CREATE TABLE ai.BusinessKnowledge
    (
        BusinessKnowledgeId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ai_BusinessKnowledge PRIMARY KEY,
        KnowledgeType nvarchar(100) NOT NULL,
        KnowledgeTitle nvarchar(300) NOT NULL,
        KnowledgeText nvarchar(max) NOT NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        AppliesToTerm nvarchar(200) NULL,
        AppliesToObjectType nvarchar(100) NULL,
        AppliesToObjectName nvarchar(300) NULL,
        PromptInstruction nvarchar(max) NULL,
        Priority int NOT NULL CONSTRAINT DF_ai_BusinessKnowledge_Priority DEFAULT (100),
        IsActive bit NOT NULL CONSTRAINT DF_ai_BusinessKnowledge_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_ai_BusinessKnowledge_VersionNumber DEFAULT (1),
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_ai_BusinessKnowledge_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_ai_BusinessKnowledge_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL
    );

    CREATE INDEX IX_ai_BusinessKnowledge_Active_Domain_Term
        ON ai.BusinessKnowledge(IsActive, SemanticDomain, AppliesToTerm, Priority);
END
GO

/* ============================================================
   ai.BusinessGlossary
   ============================================================ */

IF OBJECT_ID(N'ai.BusinessGlossary', N'U') IS NULL
BEGIN
    CREATE TABLE ai.BusinessGlossary
    (
        BusinessGlossaryId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ai_BusinessGlossary PRIMARY KEY,
        Term nvarchar(200) NOT NULL,
        TermType nvarchar(50) NOT NULL CONSTRAINT DF_ai_BusinessGlossary_TermType DEFAULT (N'BusinessTerm'),
        Definition nvarchar(max) NOT NULL,
        ExampleUsage nvarchar(max) NULL,
        PreferredSchemaName sysname NULL,
        PreferredTableName sysname NULL,
        PreferredColumnName sysname NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        Notes nvarchar(max) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_ai_BusinessGlossary_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_ai_BusinessGlossary_VersionNumber DEFAULT (1),
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_ai_BusinessGlossary_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_ai_BusinessGlossary_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL
    );

    CREATE UNIQUE INDEX UX_ai_BusinessGlossary_Term_Active
        ON ai.BusinessGlossary(Term)
        WHERE IsActive = 1;
END
GO

/* ============================================================
   ai.BusinessRule
   ============================================================ */

IF OBJECT_ID(N'ai.BusinessRule', N'U') IS NULL
BEGIN
    CREATE TABLE ai.BusinessRule
    (
        BusinessRuleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ai_BusinessRule PRIMARY KEY,
        RuleName nvarchar(200) NOT NULL,
        RuleType nvarchar(100) NOT NULL,
        RuleCategory nvarchar(100) NOT NULL,
        RuleDescription nvarchar(max) NOT NULL,
        RuleLogicDescription nvarchar(max) NULL,
        SqlPredicate nvarchar(max) NULL,
        PromptInstruction nvarchar(max) NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        AppliesToTerm nvarchar(200) NULL,
        AppliesToObjectType nvarchar(100) NULL,
        AppliesToObjectName nvarchar(300) NULL,
        OverridesBusinessRuleId int NULL,
        Priority int NOT NULL CONSTRAINT DF_ai_BusinessRule_Priority DEFAULT (100),
        IsSystemRule bit NOT NULL CONSTRAINT DF_ai_BusinessRule_IsSystemRule DEFAULT (0),
        IsActive bit NOT NULL CONSTRAINT DF_ai_BusinessRule_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_ai_BusinessRule_VersionNumber DEFAULT (1),
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_ai_BusinessRule_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_ai_BusinessRule_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_ai_BusinessRule_OverridesBusinessRule FOREIGN KEY (OverridesBusinessRuleId) REFERENCES ai.BusinessRule(BusinessRuleId)
    );

    CREATE INDEX IX_ai_BusinessRule_Active_Domain_Term
        ON ai.BusinessRule(IsActive, SemanticDomain, AppliesToTerm, Priority);
END
GO

/* ============================================================
   ai.QueryFilter
   ============================================================ */

IF OBJECT_ID(N'ai.QueryFilter', N'U') IS NULL
BEGIN
    CREATE TABLE ai.QueryFilter
    (
        QueryFilterId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ai_QueryFilter PRIMARY KEY,
        FilterName nvarchar(200) NOT NULL,
        FilterCategory nvarchar(100) NOT NULL,
        Description nvarchar(max) NOT NULL,
        SqlPredicate nvarchar(max) NOT NULL,
        PromptInstruction nvarchar(max) NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        AppliesToSchemaName sysname NULL,
        AppliesToTableName sysname NULL,
        AppliesToColumnName sysname NULL,
        AutoApply bit NOT NULL CONSTRAINT DF_ai_QueryFilter_AutoApply DEFAULT (0),
        RequiresUserConfirmation bit NOT NULL CONSTRAINT DF_ai_QueryFilter_RequiresUserConfirmation DEFAULT (0),
        Priority int NOT NULL CONSTRAINT DF_ai_QueryFilter_Priority DEFAULT (100),
        IsActive bit NOT NULL CONSTRAINT DF_ai_QueryFilter_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_ai_QueryFilter_VersionNumber DEFAULT (1),
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_ai_QueryFilter_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_ai_QueryFilter_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_ai_QueryFilter_FilterName UNIQUE (FilterName)
    );

    CREATE INDEX IX_ai_QueryFilter_Active_Category_AutoApply
        ON ai.QueryFilter(IsActive, FilterCategory, AutoApply, Priority);
END
GO

/* ============================================================
   ai.MetricDefinition
   ============================================================ */

IF OBJECT_ID(N'ai.MetricDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE ai.MetricDefinition
    (
        MetricDefinitionId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ai_MetricDefinition PRIMARY KEY,
        MetricName nvarchar(200) NOT NULL,
        MetricCategory nvarchar(100) NOT NULL,
        Description nvarchar(max) NOT NULL,
        CalculationDescription nvarchar(max) NOT NULL,
        CalculationSqlExpression nvarchar(max) NULL,
        PreferredSchemaName sysname NULL,
        PreferredTableName sysname NULL,
        PreferredColumnName sysname NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        DefaultGroupBy nvarchar(300) NULL,
        DefaultFilters nvarchar(max) NULL,
        PromptInstruction nvarchar(max) NULL,
        IsApproved bit NOT NULL CONSTRAINT DF_ai_MetricDefinition_IsApproved DEFAULT (1),
        IsActive bit NOT NULL CONSTRAINT DF_ai_MetricDefinition_IsActive DEFAULT (1),
        EffectiveDate date NULL,
        ExpirationDate date NULL,
        VersionNumber int NOT NULL CONSTRAINT DF_ai_MetricDefinition_VersionNumber DEFAULT (1),
        ApprovedBy nvarchar(256) NULL,
        ApprovedDateUtc datetime2(3) NULL,
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_ai_MetricDefinition_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_ai_MetricDefinition_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL
    );

    CREATE UNIQUE INDEX UX_ai_MetricDefinition_MetricName_Active
        ON ai.MetricDefinition(MetricName)
        WHERE IsActive = 1;
END
GO

/* ============================================================
   sec.UserIdentityMap
   ============================================================ */

IF OBJECT_ID(N'sec.UserIdentityMap', N'U') IS NULL
BEGIN
    CREATE TABLE sec.UserIdentityMap
    (
        UserIdentityMapId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_sec_UserIdentityMap PRIMARY KEY,
        OktaUserName nvarchar(256) NOT NULL,
        OktaEmail nvarchar(256) NULL,
        EpicorUserId nvarchar(100) NULL,
        DisplayName nvarchar(256) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_sec_UserIdentityMap_IsActive DEFAULT (1),
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_sec_UserIdentityMap_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_sec_UserIdentityMap_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_sec_UserIdentityMap_OktaUserName UNIQUE (OktaUserName)
    );
END
GO

/* ============================================================
   sec.ApplicationRole
   ============================================================ */

IF OBJECT_ID(N'sec.ApplicationRole', N'U') IS NULL
BEGIN
    CREATE TABLE sec.ApplicationRole
    (
        ApplicationRoleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_sec_ApplicationRole PRIMARY KEY,
        RoleName nvarchar(100) NOT NULL,
        Description nvarchar(max) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_sec_ApplicationRole_IsActive DEFAULT (1),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_sec_ApplicationRole_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_sec_ApplicationRole_RoleName UNIQUE (RoleName)
    );
END
GO

/* ============================================================
   sec.UserApplicationRole
   ============================================================ */

IF OBJECT_ID(N'sec.UserApplicationRole', N'U') IS NULL
BEGIN
    CREATE TABLE sec.UserApplicationRole
    (
        UserApplicationRoleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_sec_UserApplicationRole PRIMARY KEY,
        UserIdentityMapId int NOT NULL,
        ApplicationRoleId int NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_sec_UserApplicationRole_IsActive DEFAULT (1),
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_sec_UserApplicationRole_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_sec_UserApplicationRole_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_sec_UserApplicationRole_UserIdentityMap FOREIGN KEY (UserIdentityMapId) REFERENCES sec.UserIdentityMap(UserIdentityMapId),
        CONSTRAINT FK_sec_UserApplicationRole_ApplicationRole FOREIGN KEY (ApplicationRoleId) REFERENCES sec.ApplicationRole(ApplicationRoleId),
        CONSTRAINT UQ_sec_UserApplicationRole_User_Role UNIQUE (UserIdentityMapId, ApplicationRoleId)
    );
END
GO

/* ============================================================
   sec.SensitiveDataPolicy
   ============================================================ */

IF OBJECT_ID(N'sec.SensitiveDataPolicy', N'U') IS NULL
BEGIN
    CREATE TABLE sec.SensitiveDataPolicy
    (
        SensitiveDataPolicyId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_sec_SensitiveDataPolicy PRIMARY KEY,
        PolicyName nvarchar(200) NOT NULL,
        SchemaName sysname NULL,
        TableName sysname NOT NULL,
        ColumnName sysname NULL,
        DataClassification nvarchar(100) NOT NULL CONSTRAINT DF_sec_SensitiveDataPolicy_DataClassification DEFAULT (N'Restricted'),
        SensitivityCategory nvarchar(100) NOT NULL,
        AccessPolicy nvarchar(50) NOT NULL,
        RequiredRoleName nvarchar(100) NULL,
        SemanticDomain nvarchar(100) NULL,
        Company nvarchar(20) NULL,
        Plant nvarchar(20) NULL,
        Reason nvarchar(max) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_sec_SensitiveDataPolicy_IsActive DEFAULT (1),
        CreatedBy nvarchar(256) NOT NULL CONSTRAINT DF_sec_SensitiveDataPolicy_CreatedBy DEFAULT (SUSER_SNAME()),
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_sec_SensitiveDataPolicy_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        ModifiedBy nvarchar(256) NULL,
        ModifiedDateUtc datetime2(3) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT CK_sec_SensitiveDataPolicy_AccessPolicy CHECK (AccessPolicy IN (N'Allowed', N'Restricted', N'Blocked', N'RequiresReview')),
        CONSTRAINT UQ_sec_SensitiveDataPolicy_PolicyName UNIQUE (PolicyName)
    );

    CREATE INDEX IX_sec_SensitiveDataPolicy_Object
        ON sec.SensitiveDataPolicy(IsActive, SchemaName, TableName, ColumnName, AccessPolicy);
END
GO

/* ============================================================
   audit.ApplicationEventLog
   ============================================================ */

IF OBJECT_ID(N'audit.ApplicationEventLog', N'U') IS NULL
BEGIN
    CREATE TABLE audit.ApplicationEventLog
    (
        ApplicationEventLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_audit_ApplicationEventLog PRIMARY KEY,
        EventLevel nvarchar(20) NOT NULL,
        EventSource nvarchar(200) NOT NULL,
        EventMessage nvarchar(max) NOT NULL,
        ExceptionDetail nvarchar(max) NULL,
        UserName nvarchar(256) NULL,
        CorrelationId uniqueidentifier NULL,
        CreatedDateUtc datetime2(3) NOT NULL CONSTRAINT DF_audit_ApplicationEventLog_CreatedDateUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_audit_ApplicationEventLog_EventLevel CHECK (EventLevel IN (N'Debug', N'Information', N'Warning', N'Error', N'Critical'))
    );

    CREATE INDEX IX_audit_ApplicationEventLog_Level_Created
        ON audit.ApplicationEventLog(EventLevel, CreatedDateUtc DESC);
END
GO

/* ============================================================
   Helpful Views
   ============================================================ */

CREATE OR ALTER VIEW meta.vwQueryableFields
AS
    SELECT
        f.SystemCode,
        f.DataTableID,
        t.SchemaName,
        t.DBTableName,
        f.FieldName,
        f.DBFieldName,
        f.DataType,
        f.Description,
        f.FieldLabel,
        f.IsQueryable,
        f.IsRestricted,
        f.DataClassification,
        f.SensitivityCategory,
        f.RestrictionReason
    FROM meta.EpicorDataField f
    LEFT JOIN meta.EpicorDataTable t
        ON t.SystemCode = f.SystemCode
       AND t.DataTableID = f.DataTableID
    WHERE f.IsQueryable = 1
      AND ISNULL(t.IsQueryable, 1) = 1;
GO

CREATE OR ALTER VIEW ai.vwActiveBusinessKnowledge
AS
    SELECT N'BusinessKnowledge' AS KnowledgeType, CAST(BusinessKnowledgeId AS bigint) AS KnowledgeId, KnowledgeTitle AS Name, SemanticDomain, KnowledgeText AS Description, PromptInstruction, Priority, Company, Plant, EffectiveDate, ExpirationDate, VersionNumber
    FROM ai.BusinessKnowledge
    WHERE IsActive = 1
    UNION ALL
    SELECT N'Glossary', CAST(BusinessGlossaryId AS bigint), Term, SemanticDomain, Definition, Notes, 100, Company, Plant, EffectiveDate, ExpirationDate, VersionNumber
    FROM ai.BusinessGlossary
    WHERE IsActive = 1
    UNION ALL
    SELECT N'BusinessRule', CAST(BusinessRuleId AS bigint), RuleName, SemanticDomain, RuleDescription, PromptInstruction, Priority, Company, Plant, EffectiveDate, ExpirationDate, VersionNumber
    FROM ai.BusinessRule
    WHERE IsActive = 1
    UNION ALL
    SELECT N'QueryFilter', CAST(QueryFilterId AS bigint), FilterName, SemanticDomain, Description, PromptInstruction, Priority, Company, Plant, EffectiveDate, ExpirationDate, VersionNumber
    FROM ai.QueryFilter
    WHERE IsActive = 1
    UNION ALL
    SELECT N'MetricDefinition', CAST(MetricDefinitionId AS bigint), MetricName, SemanticDomain, Description, PromptInstruction, 50, Company, Plant, EffectiveDate, ExpirationDate, VersionNumber
    FROM ai.MetricDefinition
    WHERE IsActive = 1 AND IsApproved = 1;
GO

/* ============================================================
   Seed Data
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM sec.ApplicationRole WHERE RoleName = N'StandardUser')
BEGIN
    INSERT INTO sec.ApplicationRole (RoleName, Description)
    VALUES
        (N'StandardUser', N'Can ask approved questions and view authorized result sets.'),
        (N'PowerUser', N'Can inspect generated SQL and assist with validation.'),
        (N'Administrator', N'Can manage metadata refresh, glossary, business knowledge, rules, and sensitive data policies.'),
        (N'SecurityReviewer', N'Can review denied prompts and sensitive access attempts.');
END
GO

IF NOT EXISTS (SELECT 1 FROM cfg.ApplicationSetting WHERE SettingKey = N'PreviewRowLimit')
BEGIN
    INSERT INTO cfg.ApplicationSetting (SettingKey, SettingValue, Description)
    VALUES
        (N'PreviewRowLimit', N'500', N'Maximum number of rows shown in the interactive result grid by default.'),
        (N'ExcelExportRowLimit', N'100000', N'Maximum number of rows allowed for Excel export.'),
        (N'SqlCommandTimeoutSeconds', N'120', N'Default SQL command timeout for generated queries.'),
        (N'LlmProvider', N'Local', N'AI provider type. Expected to remain local for sensitive Epicor data.'),
        (N'FailClosedSecurity', N'true', N'If user access cannot be determined, deny or require review.');
END
GO

IF NOT EXISTS (SELECT 1 FROM cfg.PromptTemplate WHERE TemplateName = N'SqlGeneration' AND VersionNumber = 1)
BEGIN
    INSERT INTO cfg.PromptTemplate (TemplateName, TemplateType, TemplateText, VersionNumber, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'SqlGeneration',
        N'SQL',
        N'Generate read-only SQL using only the supplied metadata, business knowledge, approved filters, and approved relationships. Do not invent tables, fields, or joins. Return SQL only.',
        1,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.BusinessGlossary WHERE Term = N'CMP')
BEGIN
    INSERT INTO ai.BusinessGlossary (Term, TermType, Definition, ExampleUsage, SemanticDomain, Notes, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'CMP',
        N'BusinessTerm',
        N'CMP is work for a division of Boeing where billing or value is based on labor hours used to produce parts, not the number of parts shipped.',
        N'How many CMP shipments were there last week and how many labor hours were used to produce the parts?',
        N'CustomerProgram',
        N'AI should not assume CMP value is based on shipped quantity. Labor hours are the primary measure.',
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.BusinessKnowledge WHERE KnowledgeTitle = N'CMP Labor-Based Work')
BEGIN
    INSERT INTO ai.BusinessKnowledge (KnowledgeType, KnowledgeTitle, KnowledgeText, SemanticDomain, AppliesToTerm, PromptInstruction, Priority, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'BusinessContext',
        N'CMP Labor-Based Work',
        N'CMP work is performed for a division of Boeing and is measured by labor hours used to produce parts rather than by parts shipped.',
        N'CustomerProgram',
        N'CMP',
        N'When answering CMP questions, prioritize labor hour measures over shipped quantity unless shipment count is explicitly requested.',
        10,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.BusinessRule WHERE RuleName = N'CMP Labor Billing Rule')
BEGIN
    INSERT INTO ai.BusinessRule (RuleName, RuleType, RuleCategory, RuleDescription, RuleLogicDescription, PromptInstruction, SemanticDomain, AppliesToTerm, Priority, IsSystemRule, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'CMP Labor Billing Rule',
        N'Semantic',
        N'CustomerProgram',
        N'CMP work is measured by labor hours used to produce parts rather than the number of parts shipped.',
        N'When a prompt references CMP, prioritize labor hour measures over shipment quantity measures unless the user explicitly asks for shipment counts.',
        N'For CMP questions, do not calculate value or production effort from shipped quantity alone. Use labor hours where the prompt asks for effort, earned hours, or labor used.',
        N'CustomerProgram',
        N'CMP',
        10,
        1,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.QueryFilter WHERE FilterName = N'Exclude FAI Jobs')
BEGIN
    INSERT INTO ai.QueryFilter (FilterName, FilterCategory, Description, SqlPredicate, PromptInstruction, SemanticDomain, AppliesToTableName, AppliesToColumnName, Priority, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'Exclude FAI Jobs',
        N'JobClassification',
        N'Excludes jobs used to track First Article Inspection or similar pre-production activity. These jobs end with the suffix -FAI.',
        N'JobNum NOT LIKE ''%-FAI''',
        N'When calculating standard production labor, exclude jobs where JobNum ends with -FAI unless the user explicitly asks for FAI jobs.',
        N'Manufacturing',
        N'JobHead',
        N'JobNum',
        20,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.QueryFilter WHERE FilterName = N'Exclude PRG Jobs')
BEGIN
    INSERT INTO ai.QueryFilter (FilterName, FilterCategory, Description, SqlPredicate, PromptInstruction, SemanticDomain, AppliesToTableName, AppliesToColumnName, Priority, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'Exclude PRG Jobs',
        N'JobClassification',
        N'Excludes jobs used to track programming or pre-production activity. These jobs end with the suffix -PRG.',
        N'JobNum NOT LIKE ''%-PRG''',
        N'When calculating standard production labor, exclude jobs where JobNum ends with -PRG unless the user explicitly asks for PRG jobs.',
        N'Manufacturing',
        N'JobHead',
        N'JobNum',
        21,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.QueryFilter WHERE FilterName = N'Production Jobs Only')
BEGIN
    INSERT INTO ai.QueryFilter (FilterName, FilterCategory, Description, SqlPredicate, PromptInstruction, SemanticDomain, AppliesToTableName, AppliesToColumnName, Priority, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'Production Jobs Only',
        N'JobClassification',
        N'Includes standard production jobs and excludes pre-production or tracking jobs ending in -FAI or -PRG.',
        N'JobNum NOT LIKE ''%-FAI'' AND JobNum NOT LIKE ''%-PRG''',
        N'Use this filter when the user asks for production labor, production output, or manufacturing performance and does not explicitly include FAI or PRG jobs.',
        N'Manufacturing',
        N'JobHead',
        N'JobNum',
        19,
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.MetricDefinition WHERE MetricName = N'Production Labor Hours')
BEGIN
    INSERT INTO ai.MetricDefinition (MetricName, MetricCategory, Description, CalculationDescription, CalculationSqlExpression, PreferredTableName, PreferredColumnName, SemanticDomain, DefaultFilters, PromptInstruction, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'Production Labor Hours',
        N'Manufacturing',
        N'Total labor hours used for production work.',
        N'Sum labor hours from labor transactions. Standard production calculations should exclude jobs ending in -FAI and -PRG unless the user explicitly asks to include those jobs.',
        N'SUM(LaborDtl.LaborHrs)',
        N'LaborDtl',
        N'LaborHrs',
        N'Manufacturing',
        N'JobNum NOT LIKE ''%-FAI'' AND JobNum NOT LIKE ''%-PRG''',
        N'Use SUM(LaborDtl.LaborHrs) for production labor hours and exclude -FAI and -PRG jobs unless explicitly requested.',
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM ai.MetricDefinition WHERE MetricName = N'CMP Labor Hours')
BEGIN
    INSERT INTO ai.MetricDefinition (MetricName, MetricCategory, Description, CalculationDescription, CalculationSqlExpression, PreferredTableName, PreferredColumnName, SemanticDomain, PromptInstruction, ApprovedBy, ApprovedDateUtc)
    VALUES
    (
        N'CMP Labor Hours',
        N'CustomerProgram',
        N'Labor hours associated with CMP work.',
        N'CMP work should be analyzed based on labor hours used to produce parts, not the number of parts shipped.',
        N'SUM(LaborDtl.LaborHrs)',
        N'LaborDtl',
        N'LaborHrs',
        N'CustomerProgram',
        N'When the prompt references CMP and asks for effort, earned hours, cost driver, or hours used, use labor hours rather than shipment quantity.',
        SUSER_SNAME(),
        SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sec.SensitiveDataPolicy WHERE PolicyName = N'Restrict Labor Rate Fields')
BEGIN
    INSERT INTO sec.SensitiveDataPolicy (PolicyName, SchemaName, TableName, ColumnName, DataClassification, SensitivityCategory, AccessPolicy, RequiredRoleName, SemanticDomain, Reason)
    VALUES
    (
        N'Restrict Labor Rate Fields',
        NULL,
        N'LaborDtl',
        N'LaborRate',
        N'Payroll',
        N'LaborRates',
        N'Restricted',
        N'SecurityReviewer',
        N'Manufacturing',
        N'Labor rate fields may expose employee compensation or sensitive cost information. Access must be explicitly approved.'
    );
END
GO

/* ============================================================
   Verification Output
   ============================================================ */

SELECT name AS DatabaseName
FROM sys.databases
WHERE name = N'NexusAI';

SELECT
    s.name AS SchemaName,
    t.name AS TableName
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name IN (N'cfg', N'meta', N'ai', N'sec', N'audit')
ORDER BY s.name, t.name;

SELECT *
FROM ai.vwActiveBusinessKnowledge
ORDER BY KnowledgeType, Name;
GO
