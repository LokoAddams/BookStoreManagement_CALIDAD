-- public.clients definition

-- Drop table

-- DROP TABLE public.clients;

CREATE TABLE public.clients (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	first_name varchar(50) NOT NULL,
	last_name varchar(50) NOT NULL,
	middle_name varchar(50) NULL,
	email varchar(100) NULL,
	phone varchar(20) NULL,
	address varchar(150) NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	created_by varchar(100) NULL,
	updated_by varchar(100) NULL,
	updated_at timestamp NULL,
	is_active bool DEFAULT true NOT NULL,
	ci varchar(20) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT clients_pkey PRIMARY KEY (id)
);