# Security

- The app reads from the current user's `.codex` directory and exports to the Desktop; keep that scope explicit.
- Do not commit secrets, personal exports, or raw local session data into the repo.
- Keep JavaScript use minimal and prefer MAUI or C# for privileged behavior.
