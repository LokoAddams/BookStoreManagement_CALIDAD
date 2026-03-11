-- public.categories definition

-- Drop table

-- DROP TABLE public.categories;

CREATE TABLE public.categories (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	"name" varchar(100) NOT NULL,
	description text NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	is_active bool DEFAULT true NOT NULL,
	CONSTRAINT categories_pkey PRIMARY KEY (id)
);
CREATE UNIQUE INDEX idx_categories_name_unique ON public.categories USING btree (lower((name)::text));

-- public.products definition

-- Drop table

-- DROP TABLE public.products;

CREATE TABLE public.products (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	"name" varchar(100) NOT NULL,
	category_id uuid NOT NULL,
	description text NOT NULL,
	price numeric(12, 2) NOT NULL,
	stock int4 NOT NULL,
	category_name varchar(100) NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	is_active bool DEFAULT true NOT NULL,
	CONSTRAINT products_description_length CHECK ((char_length(description) <= 500)),
	CONSTRAINT products_name_length CHECK ((char_length((name)::text) <= 100)),
	CONSTRAINT products_pkey PRIMARY KEY (id),
	CONSTRAINT products_price_check CHECK ((price > (0)::numeric)),
	CONSTRAINT products_stock_check CHECK ((stock >= 0))
);
CREATE INDEX idx_products_category_id ON public.products USING btree (category_id);
CREATE INDEX idx_products_is_active ON public.products USING btree (is_active);
CREATE INDEX idx_products_name ON public.products USING btree (lower((name)::text));


-- public.products foreign keys

ALTER TABLE public.products ADD CONSTRAINT fk_products_category FOREIGN KEY (category_id) REFERENCES public.categories(id) ON DELETE RESTRICT;