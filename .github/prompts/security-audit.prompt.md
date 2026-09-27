---
name: security-audit
description: "Perform a read-only security audit of this project and report actionable vulnerabilities, risks, and missing controls."
argument-hint: "Optional audit scope or security concern to prioritize"
agent: "agent"
---

Perform a read-only security audit of this repository. Do not modify files, run destructive commands, exploit live systems, or expose secrets.

Audit the complete application within the requested scope, or the whole repository when no scope is provided. Use the repository's [AGENTS.md](../../AGENTS.md) and relevant files under [.github/instructions](../instructions) as project-specific context. Respect the current architecture: `WebApp` is the Blazor UI, `WebApi` owns persistence and business rules, and communication between them uses the typed HTTP client.

Prioritize these areas:

- Authentication and authorization, including Clerk integration, session validation, route protection, token handling, and identity propagation.
- Tenant or user-data isolation, especially `clerkUserId` handling and API authorization boundaries.
- Input validation, URL handling, injection risks, unsafe redirects, SSRF, XSS, CSRF, CORS, and error-information disclosure.
- Secrets, connection strings, credentials, sensitive configuration, logging, and environment-specific settings.
- ASP.NET Core, Blazor, EF Core, SQLite, and dependency security configuration.
- API exposure, HTTP security headers, transport assumptions, rate limiting, abuse resistance, and denial-of-service risks.
- Supply-chain, dependency, and deployment configuration risks visible in the repository.

Use evidence from the code and configuration. Trace findings across call sites and trust boundaries instead of flagging isolated patterns without confirming reachability. Distinguish confirmed vulnerabilities from defense-in-depth recommendations and state assumptions when evidence is incomplete. Never print secret values; identify only the file and setting name, redacting any value.

Return the report in this format:

# Security Audit

## Scope
State what was reviewed, what was not reviewed, and the audit focus from `$ARGUMENTS`.

## Findings
List confirmed or plausible issues first, ordered by severity: Critical, High, Medium, Low, then Informational. For each finding include:

- **ID**: Unique auto incremented identifier for the finding
- **severity**: Critical, High, Medium, Low, or Informational
- **Issues**: Describe the specific security issues identified in the finding
- **Title**: Short descriptive title of the finding
- **Evidence**: Include clickable workspace-relative file links and line numbers
- **Impact**: Describe the potential consequences if the issue is exploited
- **Attack preconditions or exploit path**: Describe the conditions or steps required to exploit the issue, without weaponized payloads
- **Recommended remediation**: Suggest fixes or mitigations, consistent with the existing architecture
- **Confidence**: High, Medium, or Low, indicating the certainty of the finding

If there are no findings, say so explicitly and explain the most important checks performed.

## Positive Controls
Mention meaningful controls that are already implemented and reduce risk.

## Validation Gaps
List tests, runtime configuration, external identity-provider settings, or deployment controls that could not be verified from the repository.

## Remediation Plan
Give a short priority-ordered plan. Separate immediate fixes from follow-up hardening. Do not implement changes unless the user explicitly asks for remediation.

Keep the report concise, concrete, and suitable for an engineering issue tracker. Do not claim compliance or exploitability beyond the evidence available in the repository.

## Output format
The report should be returned in markdown tabular format, following the structure outlined above.

| ID | Severity | Issues | Title | Evidence | Impact | Attack Preconditions or Exploit Path | Recommended Remediation | Confidence |

## Next Steps to Address Findings

Ask the user to review the findings and provide feedback or approval for the proposed remediation steps. Provide option to user to fix all issue by replying all or comma-separated list of issue IDs. Ensure that the user explicitly confirms before any changes are implemented.

After user reply, run a seperate subagent (#runSubagents) to apply the approved remediation steps. Ensure that the subagent only implements changes that have been explicitly approved by the user. Document all actions taken by the subagent for transparency and traceability. Confirm completion of all approved remediation steps with the user with simple 'subAgentsSuccess: true : false'.

