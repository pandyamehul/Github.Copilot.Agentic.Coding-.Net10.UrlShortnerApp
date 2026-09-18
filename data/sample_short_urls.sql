-- Sample insert statements for short_urls (clerk_user_id = 'test-application')
-- Generated: 10 example rows

INSERT INTO public.short_urls (code, original_url, clerk_user_id, created_at, updated_at) VALUES
  ('tstA001', 'https://example.com/long/path/1', 'test-application', '2026-09-18T10:00:00+00'::timestamptz, '2026-09-18T10:00:00+00'::timestamptz),
  ('tstA002', 'https://example.com/long/path/2', 'test-application', '2026-09-18T10:10:00+00'::timestamptz, '2026-09-18T10:10:00+00'::timestamptz),
  ('tstA003', 'https://example.com/long/path/3', 'test-application', '2026-09-18T10:20:00+00'::timestamptz, '2026-09-18T10:20:00+00'::timestamptz),
  ('tstA004', 'https://example.com/long/path/4', 'test-application', '2026-09-18T10:30:00+00'::timestamptz, '2026-09-18T10:30:00+00'::timestamptz),
  ('tstA005', 'https://example.com/long/path/5', 'test-application', '2026-09-18T10:40:00+00'::timestamptz, '2026-09-18T10:40:00+00'::timestamptz),
  ('tstA006', 'https://example.com/long/path/6', 'test-application', '2026-09-18T10:50:00+00'::timestamptz, '2026-09-18T10:50:00+00'::timestamptz),
  ('tstA007', 'https://example.com/long/path/7', 'test-application', '2026-09-18T11:00:00+00'::timestamptz, '2026-09-18T11:00:00+00'::timestamptz),
  ('tstA008', 'https://example.com/long/path/8', 'test-application', '2026-09-18T11:10:00+00'::timestamptz, '2026-09-18T11:10:00+00'::timestamptz),
  ('tstA009', 'https://example.com/long/path/9', 'test-application', '2026-09-18T11:20:00+00'::timestamptz, '2026-09-18T11:20:00+00'::timestamptz),
  ('tstA010', 'https://example.com/long/path/10', 'test-application', '2026-09-18T11:30:00+00'::timestamptz, '2026-09-18T11:30:00+00'::timestamptz)
;

-- Notes:
-- - These statements assume the `public.short_urls` table already exists with columns
--   (code, original_url, clerk_user_id, created_at, updated_at).
-- - To load locally: `psql -d yourdb -f data/sample_short_urls.sql`
