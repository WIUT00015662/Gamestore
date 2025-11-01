# Gamestore React UI

Simple React + TypeScript UI for the Gamestore metadata aggregator.

## Setup

1. Copy `.env.example` to `.env`.
2. Set `VITE_API_BASE_URL` to your API URL.
3. Install dependencies:
   - `npm install`
4. Run development server:
   - `npm run dev`

## Notes

- Use `admin/admin` to login (default seeded account).
- Home page shows top discounted games from latest polling run.
- Role-aware pages are available:
  - `/guest`
  - `/user`
  - `/moderator`
  - `/manager`
  - `/admin`
- UI menus and route access are controlled by JWT permissions.
