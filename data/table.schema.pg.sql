
-- PostgreSQL schema for shortened URLs
-- Note: deletes are permanent (no soft deletes); no click tracking column included.

CREATE TABLE IF NOT EXISTS public.short_urls (
	id bigserial PRIMARY KEY,
	code varchar(32) NOT NULL,
	original_url varchar(2048) NOT NULL,
	clerk_user_id varchar(128) NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	updated_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT uq_short_urls_code UNIQUE (code)
);

-- Ensure an index on code for fast lookups (unique constraint also creates one,
-- but this makes intent explicit and is safe to run idempotently).
CREATE UNIQUE INDEX IF NOT EXISTS idx_short_urls_code ON public.short_urls (code);

-- If you prefer DB-side automatic updated_at maintenance, add a trigger like:
--
-- CREATE FUNCTION set_updated_at()
-- RETURNS trigger AS $$
-- BEGIN
--   NEW.updated_at = now();
--   RETURN NEW;
-- END;
-- $$ LANGUAGE plpgsql;
--
-- CREATE TRIGGER trg_set_updated_at
-- BEFORE UPDATE ON public.short_urls
-- FOR EACH ROW EXECUTE FUNCTION set_updated_at();
