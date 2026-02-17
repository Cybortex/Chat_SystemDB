-- SQL schema for Chat_SystemDB Supabase project
-- Run these statements in the Supabase SQL editor.

-- 1) users table
create table if not exists public.users (
  email text primary key,
  display_name text,
  created_at timestamptz default now()
);

-- 2) messages table
create table if not exists public.messages (
  id bigserial primary key,
  from_email text not null,
  to_email text not null,
  text text not null,
  created_at timestamptz default now()
);

-- 3) devices table (for push tokens)
create table if not exists public.devices (
  id bigserial primary key,
  user_email text not null,
  device_token text not null,
  platform text,
  created_at timestamptz default now()
);

-- Row Level Security (RLS) guidance
-- Supabase service_role key bypasses RLS, so server-side operations using the
-- service_role key do not need policies. For client-side access you will want
-- to enable RLS and add policies that expose only correct rows.

-- Example RLS policies (templates) — review and adapt before enabling RLS.
-- These examples use JWT claims provided by Supabase. Adjust claim checks
-- to match your project's JWT claim structure.

-- Enable RLS (do this after creating appropriate policies):
-- ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE public.messages ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE public.devices ENABLE ROW LEVEL SECURITY;

-- Activate Row Level Security (RLS) for these tables.
-- CAUTION: Enabling RLS without policies may block access for clients.
-- Verify policies below before running these statements in production.
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.messages ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.devices ENABLE ROW LEVEL SECURITY;

-- Example: allow authenticated users to select their own user row
-- CREATE POLICY "Users can select own row" ON public.users
-- FOR SELECT USING (
--   (
--     -- This checks the email claim produced by Supabase in the JWT
--     (current_setting('request.jwt.claims', true) ->> 'email') = email
--   )
-- );

-- Example policies to allow authenticated users to operate on their own data.
-- These policies assume your JWT contains an 'email' claim. Adapt if different.

-- Allow authenticated users to select their own user row
CREATE POLICY "Users can select own row" ON public.users
FOR SELECT USING (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = email
);

-- Allow anyone (authenticated) to insert their own user row via auth
CREATE POLICY "Users can insert own row" ON public.users
FOR INSERT WITH CHECK (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = email
);

-- Example: allow authenticated inserts into devices only for the claimed user
-- CREATE POLICY "Insert device for own user" ON public.devices
-- FOR INSERT WITH CHECK (
--   (current_setting('request.jwt.claims', true) ->> 'email') = user_email
-- );

-- Allow authenticated users to insert devices for themselves
CREATE POLICY "Insert device for own user" ON public.devices
FOR INSERT WITH CHECK (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = user_email
);

-- Allow authenticated users to select their own devices
CREATE POLICY "Select device for own user" ON public.devices
FOR SELECT USING (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = user_email
);

-- Example: allow authenticated users to insert messages when 'from_email' matches
-- CREATE POLICY "Insert message from self" ON public.messages
-- FOR INSERT WITH CHECK (
--   (current_setting('request.jwt.claims', true) ->> 'email') = from_email
-- );

-- Allow authenticated users to insert messages when from_email matches their JWT
CREATE POLICY "Insert message from self" ON public.messages
FOR INSERT WITH CHECK (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = from_email
);

-- Allow authenticated users to select messages where they are either sender or recipient
CREATE POLICY "Select messages for user" ON public.messages
FOR SELECT USING (
  (current_setting('request.jwt.claims', true)::json ->> 'email') = from_email
  OR (current_setting('request.jwt.claims', true)::json ->> 'email') = to_email
);

-- Note: The functions/current_setting usage above assume your Supabase
-- JWT includes an 'email' claim. Test and adapt these policies in the
-- Supabase SQL editor. When in doubt, keep RLS disabled until policies
-- are verified to avoid locking out access.

-- Quick grants for development (NOT for production):
-- -- Allow anon role full access (development ONLY)
-- -- grant all on public.users to anon;
-- -- grant all on public.messages to anon;
-- -- grant all on public.devices to anon;

-- End of SQL file
