# ContosoDashboard Constitution

<!--
Sync Impact Report
- Version change: 0.1.0 -> 1.0.0
- Modified principles: N/A -> Security by Default, Training-Grade Simplicity, Test-First and Behavior-Guided Change, Data Integrity and Access Boundaries, Offline-First Learning with Explicit Migration Paths
- Added sections: Security Requirements, Development Workflow
- Removed sections: none
- Deferred items: none
-->

## Core Principles

### I. Security by Default
The project MUST treat security as a governing requirement, not as an optional layer. All protected pages and service operations MUST enforce authentication and authorization before exposing data or allowing mutation. User identity MUST be checked against the requested resource, and request parameters, route values, and claims MUST never be trusted as authorization evidence on their own.

This rule exists because the application is intentionally teaching secure design patterns. A user-visible feature is not complete unless it preserves the principle of least privilege and prevents unauthorized access between users, projects, and task records.

### II. Training-Grade Simplicity
The application MUST favor clear, understandable architecture over unnecessary abstraction. Models, services, pages, and data access logic MUST be readable, purpose-bound, and easy to trace from feature to implementation. Complexity MUST be justified in writing, and shortcuts that obscure security decisions or business ownership are prohibited.

This principle keeps the training repository approachable while preserving disciplined engineering habits. The codebase is educational, so clarity is a production-quality behavior in its own right.

### III. Test-First and Behavior-Guided Change
Each significant behavior change MUST be specified and validated before implementation. New or amended flows involving authentication, authorization, project access, task updates, notifications, or user isolation MUST have a clear failing check or explicit verification path before being accepted as complete.

The project MUST prefer tests that validate real user-visible behavior over tests that only assert mock wiring. A feature is not considered complete when it merely compiles; it must operate under the relevant scenario and constraints.

### IV. Data Integrity and Access Boundaries
All data operations MUST respect the repository's ownership and permission model. Users MUST only see and modify data they are explicitly authorized to access, and service methods MUST enforce project membership, task ownership, and role constraints even if a UI layer is bypassed or a request is tampered with.

This rule is non-negotiable because contamination across user or project boundaries is a core risk in dashboard applications. Validation of IDs, statuses, and ownership claims is mandatory before any write or read operation proceeds.

### V. Offline-First Learning with Explicit Migration Paths
The application MUST remain runnable offline without cloud dependencies. Local data storage, mock authentication, and seed data are allowed for training purposes, but infrastructure boundaries MUST be explicit so the project can later migrate to Azure or other production systems without rewriting the business logic.

This architecture supports the training goal of demonstrating safe abstraction while keeping the app usable in restricted or isolated environments.

## Security Requirements
The project MUST keep security requirements visible in both code and documentation. All protected pages MUST use the authorization model, and service-layer checks MUST be present wherever a user can access or mutate operational data. Authorization decisions MUST not rely on a single UI gate alone.

The project MUST also maintain explicit guidance around training-safe security design. Mock authentication is acceptable only for educational contexts, and production patterns such as password hashing, MFA, OAuth, TLS enforcement, and audit logging MUST be documented as required follow-on work outside this repository's scope.

## Development Workflow
All repository changes MUST be evaluated against this constitution before merge. New work MUST begin with a clear requirement, a defined verification path, and a reminder of the application's security, user-isolation, and offline-training constraints. In code review, maintainers MUST confirm that the change preserves the repository's access boundaries and does not weaken the training-safe security model.

The workflow MUST also preserve a simple progression: understand the requirement, validate the behavior, implement the smallest correct change, and confirm the relevant outputs still satisfy the project's governance rules.

## Governance
This constitution supersedes informal practices for this repository. Amendments require a documented rationale, a sync-impact summary, and a review of affected security, access-control, and verification requirements before approval. Any change that modifies principle intent, access rules, or required validation behavior MUST be reflected in the constitution and versioned according to the policy below.

Versioning policy:
- MAJOR: backward-incompatible governance changes or principle removals or redefinitions
- MINOR: a new principle or section, or materially expanded guidance
- PATCH: clarifying wording, typo fixes, and non-semantic refinements

Compliance review expectations:
- Security and permission checks must be verified before merging changes that affect read/write flows
- User-isolation and role-based access requirements must remain intact for all dashboard features
- Training constraints and offline-first requirements must be preserved unless the project explicitly changes scope

**Version**: 1.0.0 | **Ratified**: 2026-09-15 | **Last Amended**: 2026-09-15
