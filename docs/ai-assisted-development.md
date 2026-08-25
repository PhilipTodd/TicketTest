---
layout: default
title: AI-Assisted Development
permalink: /ai-assisted-development/
---

# AI-Assisted Development

AI tooling was used throughout the project as a development aid.

The objective was to use AI to accelerate implementation, investigation and documentation while retaining normal engineering review and validation practices.

## Areas of Use

AI assistance was used for activities including:

- Reviewing the technical assessment requirements
- Discussing application architecture
- Designing API contracts
- Implementing and reviewing validation rules
- Developing integration-test scenarios
- Troubleshooting ASP.NET Core and test-hosting issues
- Reviewing SQL solutions
- Developing the Angular client
- Migrating persistence from SQLite to Azure SQL
- Designing Bicep infrastructure
- Developing Azure DevOps YAML pipelines
- Troubleshooting deployment issues
- Producing project documentation

## Validation Approach

AI-generated suggestions were treated as proposed solutions rather than authoritative output.

Changes were validated through the normal development process.

### Code

Application changes were:

- Reviewed before incorporation
- Compiled locally
- Exercised through the running application
- Committed incrementally to source control

### Automated Tests

Integration tests were used to verify API behaviour.

Tests run against an isolated in-memory SQLite database and are also executed automatically by the Azure DevOps API pipeline.

### User Interface

Angular functionality was manually exercised against the API, including:

- Retrieving tickets
- Creating tickets
- Editing tickets
- Deleting tickets
- Validation behaviour

Both local and Azure-hosted configurations were tested.

### SQL

SQL answers were reviewed against the assessment requirements, including:

- Filtering and ordering
- Aggregation
- Window functions
- Percentage calculations
- Indexing
- Sargability
- Safe updates

### Azure Infrastructure

Infrastructure changes were validated using:

```text
az bicep build
```

followed by Azure deployment validation and Bicep What-If before deployment.

The deployed resources were then verified directly in Azure.

### CI/CD

Azure DevOps pipelines were validated through actual pipeline executions.

Issues encountered during pipeline development were investigated using build logs and corrected before the pipelines were considered complete.

## Human Review Remains Required

The project encountered several cases where an initial implementation or AI-generated suggestion required correction after compilation, testing or deployment.

This reinforces an important principle of AI-assisted software development:

> AI can accelerate engineering work, but generated output still requires technical review and empirical validation.

The final implementation therefore represents an iterative engineering process combining AI assistance with source review, automated testing, manual testing and deployment verification.