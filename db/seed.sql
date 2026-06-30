-- =============================================================================
-- SVDSystem seed data
-- Run this manually against the database after schema migrations have been applied.
-- Safe to re-run: every statement uses ON CONFLICT DO NOTHING.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- System user (fixed GUID because other rows FK to it)
-- -----------------------------------------------------------------------------
INSERT INTO users (id, object_id, display_name, email) VALUES 
	('ffffffff-ffff-ffff-ffff-ffffffffffff', 'system', 'System', ''),
	(gen_random_uuid(), '8db7ab7b-0974-47fd-9cfb-2a2f798dc4e6', 'Marc Colomer', 'marc.colomer@aqaia.com')
ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- Default repository
-- -----------------------------------------------------------------------------
INSERT INTO repository_configurations
    (id, repository_id, repository_name, project_name, remote_url,
     enabled, custom_prompt, severity_threshold, vulnerability_categories,
     ignore_paths, file_type_filters,
     include_added_files, include_deleted_files, include_modified_files, use_categories)
VALUES
    ('9f4d97a5-ffb5-40c3-9e6d-e0770c56eab0',
     '9f4d97a5-ffb5-40c3-9e6d-e0770c56eab0',
     'SFQ', 'SFQ',
     'https://Securitas-SSIA@dev.azure.com/Securitas-SSIA/SFQ/_git/SFQ',
     true, '', 'Medium', '', '', '',
     true, true, true, false)
ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- System prompt templates
-- -----------------------------------------------------------------------------
INSERT INTO prompt_templates (id, name, content, is_system, created_by_id)
VALUES
    ('00000000-0000-0000-0000-000000000001',
     'General Vulnerability Analysis',
     'You are a senior Application Security (AppSec) engineer specialized in secure code review and vulnerability detection.

Your task is to determine whether the provided Git diff introduces one or more security vulnerabilities.

The code may or may not contain vulnerabilities.

Your primary objective is NOT to find vulnerabilities.
Your primary objective is to determine whether there is sufficient evidence that a vulnerability exists.

You must ONLY analyze the provided diff.
Do not assume code that is not shown.
Do not speculate.
Do not infer vulnerabilities without direct evidence.
Do not report theoretical or best-practice issues unless they represent an actual exploitable vulnerability.

Before reporting any vulnerability, ask yourself:

1. Is there direct evidence in the provided code?
2. Is the code actually vulnerable?
3. Would a security engineer confidently report this finding during a code review?

If the answer to ANY of these questions is NO,
return exactly:

[]

When uncertain, prefer returning [] instead of reporting a possible vulnerability.

Only report vulnerabilities that are clearly demonstrated by the provided code.

For each confirmed vulnerability:
- classify severity as LOW, MEDIUM, or HIGH
- provide the vulnerability type
- explain why it is vulnerable
- explain the security impact
- provide a concrete remediation
- indicate the affected lines if possible

IMPORTANT

- Never guess.
- Never speculate.
- Never reinterpret secure code as vulnerable.
- Return ONLY valid JSON.
- If there is no confirmed vulnerability, return exactly: []

{CUSTOM_REPOSITORY_INSTRUCTIONS}

Git Diff:

{GIT_DIFF}',
     true,
     'ffffffff-ffff-ffff-ffff-ffffffffffff'),

    ('00000000-0000-0000-0000-000000000002',
     'Category-Focused Analysis',
     'You are a senior Application Security (AppSec) engineer specialized in secure code review and vulnerability detection.

Your task is to determine whether the provided Git diff introduces one or more security vulnerabilities.

Search ONLY for the following vulnerability categories:

{VULNERABILITY_CATEGORIES}

IMPORTANT RESTRICTIONS

- Ignore every other type of vulnerability, even if it is present.
- Report vulnerabilities ONLY if they belong to one of the specified categories.
- Do NOT reinterpret or relabel another vulnerability as one of the requested categories.
- Do NOT infer vulnerabilities that are not directly supported by the code.
- If the code contains vulnerabilities outside the specified categories, ignore them completely.
- If none of the specified vulnerability categories are present, return an empty JSON array ([]).

The code may or may not contain vulnerabilities.

Your primary objective is NOT to find vulnerabilities.
Your primary objective is to determine whether there is sufficient evidence that a vulnerability exists.

You must ONLY analyze the provided diff.
Do not assume code that is not shown.
Do not speculate.
Do not infer vulnerabilities without direct evidence.

[...]

(Returns the same instructions as the general prompt.)

{CUSTOM_REPOSITORY_INSTRUCTIONS}

Git Diff:

{GIT_DIFF}',
     true,
     'ffffffff-ffff-ffff-ffff-ffffffffffff')
ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- Vulnerability categories
-- -----------------------------------------------------------------------------
INSERT INTO vulnerability_categories (id, name, created_by_id) VALUES
    (gen_random_uuid(), 'Injection Attacks',                         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Cross-Site Scripting (XSS)',                'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Broken Access Control',                     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Sensitive Data Exposure',                   'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Security Misconfiguration',                 'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Vulnerable & Outdated Components',          'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Broken Authentication & Session Management','ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Insecure Cryptography',                     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Insecure Deserialization',                  'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Improper Input Validation',                 'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Path & File Vulnerabilities',               'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Request Forgery (SSRF / CSRF)',             'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Denial of Service (DoS)',                   'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Race Conditions & Concurrency',             'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Insufficient Logging & Monitoring',         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Supply Chain Attacks',                      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Insecure Cloud & Infrastructure',           'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Hardcoded Secrets',                         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Open Redirect',                             'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Business Logic Vulnerabilities',            'ffffffff-ffff-ffff-ffff-ffffffffffff')
ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- File type filters
-- -----------------------------------------------------------------------------
INSERT INTO file_type_filters (id, name, extension, created_by_id) VALUES
    (gen_random_uuid(), 'C#',              '.cs',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Visual Basic',    '.vb',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'F#',              '.fs',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Java',            '.java',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Kotlin',          '.kt',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Scala',           '.scala',     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Python',          '.py',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Ruby',            '.rb',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'PHP',             '.php',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Go',              '.go',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Rust',            '.rs',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Swift',           '.swift',     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Objective-C',     '.m',         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'C',               '.c',         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'C++',             '.cpp',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'C++ Header',      '.h',         'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'C++ Header',      '.hpp',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'JavaScript',      '.js',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'TypeScript',      '.ts',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'JSX',             '.jsx',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'TSX',             '.tsx',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'SQL',             '.sql',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'PL/SQL',          '.pls',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'HTML',            '.html',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'CSS',             '.css',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'SCSS',            '.scss',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'XML',             '.xml',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'JSON',            '.json',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'YAML',            '.yaml',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'YAML',            '.yml',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'TOML',            '.toml',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Shell Script',    '.sh',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'PowerShell',      '.ps1',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Batch',           '.bat',       'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Dockerfile',      'dockerfile', 'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Terraform',       '.tf',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Bicep',           '.bicep',     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Makefile',        'makefile',   'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Markdown',        '.md',        'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'GraphQL',         '.graphql',   'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Protobuf',        '.proto',     'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'ASPX',            '.aspx',      'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Razor',           '.cshtml',    'ffffffff-ffff-ffff-ffff-ffffffffffff'),
    (gen_random_uuid(), 'Vue',             '.vue',       'ffffffff-ffff-ffff-ffff-ffffffffffff')
ON CONFLICT DO NOTHING;
