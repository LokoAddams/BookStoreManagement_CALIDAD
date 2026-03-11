-- public.distributors definition

-- Drop table

-- DROP TABLE public.distributors;

CREATE TABLE public.distributors (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	"name" varchar(80) NOT NULL,
	contact_email varchar(100) NULL,
	phone varchar(20) NULL,
	address varchar(150) NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	is_active bool DEFAULT true NOT NULL,
	CONSTRAINT distributors_pkey PRIMARY KEY (id)
);