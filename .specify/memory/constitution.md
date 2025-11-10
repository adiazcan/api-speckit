<!--
  Sync Impact Report
  ==================
  Version: 0.0.0 → 1.0.0
  Change Type: MAJOR (initial constitution establishment)
  Date: 2025-11-10

  Principles Established:
  - NEW: Code Quality First
  - NEW: Test-Driven Development (TDD)
  - NEW: Development Experience Consistency
  - NEW: Performance by Design

  Templates Status:
  - ✅ plan-template.md: Aligned (Constitution Check section exists)
  - ✅ spec-template.md: Aligned (Success Criteria section supports performance requirements)
  - ✅ tasks-template.md: Aligned (Test-first workflow documented)
  - ⚠️  All prompt commands: Manual review recommended for principle references

  Follow-up Actions:
  - Review all .github/prompts/*.md files for consistency with new principles
  - Ensure all commands enforce constitution checks appropriately
-->

# API-SpecKit Constitution

## Core Principles

### I. Code Quality First

**All code MUST meet these non-negotiable quality standards:**

- Code MUST be self-documenting through clear naming conventions (functions, variables, classes)
- Complex logic MUST include inline comments explaining the "why", not the "what"
- Code MUST pass linting and formatting checks before commit (no warnings tolerated)
- Code MUST follow language-specific idioms and best practices
- Functions MUST have a single, well-defined responsibility (Single Responsibility Principle)
- Magic numbers and strings MUST be replaced with named constants
- Dead code MUST be removed; commented-out code is not permitted

**Rationale:** Quality prevents technical debt accumulation and reduces cognitive load during
maintenance. Self-documenting code with clear intent reduces onboarding time and debugging
effort. Strict linting enforcement prevents style debates and maintains consistency across
contributors.

### II. Test-Driven Development (TDD)

**Testing discipline is NON-NEGOTIABLE and MUST follow this workflow:**

- Tests MUST be written BEFORE implementation (Red-Green-Refactor cycle strictly enforced)
- All tests MUST fail initially to validate they are testing the right behavior
- Implementation MUST make tests pass with minimal code (no premature optimization)
- Code coverage MUST be at least 80% for new code (measured automatically)
- Test types required:
  - **Unit tests**: All business logic functions and classes
  - **Integration tests**: API endpoints, database operations, external service interactions
  - **Contract tests**: All public API interfaces and library boundaries
- Tests MUST be independent (no shared state, deterministic execution order)
- Test names MUST clearly describe the scenario being tested (Given-When-Then pattern)
- Flaky tests MUST be fixed immediately or disabled with a tracking issue

**Rationale:** TDD ensures requirements are testable before implementation begins, prevents
scope creep, and provides executable documentation. High coverage prevents regressions and
gives confidence during refactoring. Test independence enables parallel execution and
reliable CI/CD pipelines.

### III. Development Experience Consistency

**Developer workflow MUST be consistent and frictionless:**

- Project setup MUST be achievable with a single command (documented in README)
- All projects MUST use identical tooling configurations (linters, formatters, test runners)
- Environment-specific configuration MUST use `.env` files or environment variables
- Local development MUST NOT require cloud services or paid dependencies
- Build and test commands MUST be identical across all projects
- Error messages MUST be actionable (include what went wrong and how to fix it)
- Documentation MUST include quickstart guides with copy-paste examples
- Development dependencies MUST be pinned to specific versions (lock files required)
- Onboarding time for new contributors MUST NOT exceed 30 minutes

**Rationale:** Consistency reduces context switching and cognitive load when working across
multiple features or projects. Standardized tooling prevents "it works on my machine"
scenarios. Clear error messages reduce debugging time and support self-service problem
resolution. Fast onboarding accelerates team velocity and reduces mentorship burden.

## Quality Gates

**All code changes MUST pass these gates before merge:**

### Pre-Commit Gates
- Linting and formatting checks MUST pass (enforced via pre-commit hooks)
- Local test suite MUST pass (unit tests minimum)
- No compiler/interpreter warnings MUST exist

### Pull Request Gates
- All automated tests MUST pass (unit, integration, contract)
- Code coverage MUST meet or exceed 80% for changed files
- At least one peer review approval MUST be obtained
- Constitution compliance MUST be verified (checklist in PR template)
- Performance benchmarks MUST pass if performance-sensitive code changed
- Breaking changes MUST be documented with migration guide

### Release Gates
- All pull request gates MUST pass
- End-to-end tests MUST pass in staging environment
- Performance targets MUST be validated in production-like environment
- Security scanning MUST show no high or critical vulnerabilities
- Documentation MUST be updated to reflect changes

## Development Workflow

**Standard workflow for all feature development:**

1. **Specification Phase**
   - Write feature specification using `/speckit.specify`
   - Define measurable success criteria including performance targets
   - Identify test scenarios (Given-When-Then format)
   - Get specification approval before proceeding

2. **Planning Phase**
   - Generate implementation plan using `/speckit.plan`
   - Perform constitution compliance check
   - Document technical decisions and constraints
   - Define data models and API contracts

3. **Task Generation Phase**
   - Generate task list using `/speckit.tasks`
   - Organize tasks by user story priority
   - Ensure each task includes exact file paths
   - Validate tasks are independently testable

4. **Implementation Phase**
   - Write tests first (must fail initially)
   - Implement minimal code to make tests pass
   - Refactor while keeping tests green
   - Commit after each task or logical group

5. **Validation Phase**
   - Run full test suite (unit, integration, contract)
   - Verify performance targets met
   - Check code coverage meets threshold
   - Perform manual testing of user scenarios

## Governance

**This constitution governs all development activities:**

- This constitution MUST be reviewed during specification phase (Constitution Check section)
- All pull requests MUST include constitution compliance verification
- Principle violations MUST be justified in writing (Complexity Tracking table)
- Unjustified violations MUST block pull request approval
- Constitution amendments MUST follow semantic versioning:
  - **MAJOR**: Backward incompatible changes (principle removal/redefinition)
  - **MINOR**: New principles added or material expansions
  - **PATCH**: Clarifications, wording improvements, typo fixes
- Constitution updates MUST include impact analysis on dependent templates
- All team members MUST be notified of constitution changes
- Runtime development guidance is provided in command-specific prompt files

**Version**: 1.0.0 | **Ratified**: 2025-11-10 | **Last Amended**: 2025-11-10
