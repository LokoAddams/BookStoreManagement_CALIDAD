-- public.users definition

-- Drop table

-- DROP TABLE public.users;

CREATE TABLE public.users (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	username text NOT NULL,
	email text NOT NULL,
	first_name text NULL,
	last_name text NULL,
	middle_name text NULL,
	password_hash text NOT NULL,
	is_active bool DEFAULT true NOT NULL,
	must_change_password bool DEFAULT true NOT NULL,
	created_at timestamp NULL,
	created_by varchar(100) NULL
);

-- public.roles definition

-- Drop table

-- DROP TABLE public.roles;

CREATE TABLE public.roles (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	"name" text NOT NULL
);
-- public.user_roles definition

-- Drop table

-- DROP TABLE public.user_roles;

CREATE TABLE public.user_roles (
	user_id uuid NOT NULL,
	role_id uuid NOT NULL
);