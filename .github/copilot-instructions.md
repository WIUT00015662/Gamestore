# Copilot Instructions

## Project Guidelines
- User prefers middleware-based user context extraction over controllers doing it. Controllers should not be responsible for parsing JWT and extracting user ID - that should be centralized in middleware or a service. This is cleaner architecture.
- Separate interface from implementation into different files.
- Do not modify migration files or EF snapshot files; config files are acceptable, but migration/snapshot changes should be avoided.