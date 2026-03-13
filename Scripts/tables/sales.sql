-- public.sales definition

-- Drop table

-- DROP TABLE public.sales;

CREATE TABLE public.sales (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	client_id uuid NOT NULL,
	user_id uuid NOT NULL,
	sale_date timestamptz DEFAULT now() NOT NULL,
	subtotal numeric(10, 2) NOT NULL,
	total numeric(10, 2) NOT NULL,
	status varchar(20) DEFAULT 'COMPLETED'::character varying NOT NULL,
	cancelled_at timestamptz NULL,
	cancelled_by uuid NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	cancellation_reason varchar(500) NULL,
	CONSTRAINT chk_sales_status CHECK (((status)::text = ANY ((ARRAY['COMPLETED'::character varying, 'CANCELLED'::character varying, 'PENDING'::character varying, 'REFUNDED'::character varying])::text[]))),
	CONSTRAINT chk_sales_total_equals_subtotal CHECK ((total = subtotal)),
	CONSTRAINT sales_pkey PRIMARY KEY (id),
	CONSTRAINT sales_subtotal_check CHECK ((subtotal >= (0)::numeric)),
	CONSTRAINT sales_total_check CHECK ((total >= (0)::numeric))
);
CREATE INDEX idx_sales_client_id ON public.sales USING btree (client_id);
CREATE INDEX idx_sales_sale_date ON public.sales USING btree (sale_date);
CREATE INDEX idx_sales_status ON public.sales USING btree (status);
CREATE INDEX idx_sales_user_id ON public.sales USING btree (user_id);



-- public.sale_details definition

-- Drop table

-- DROP TABLE public.sale_details;

CREATE TABLE public.sale_details (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	sale_id uuid NOT NULL,
	product_id uuid NOT NULL,
	product_name varchar(255) NULL,
	quantity int4 NOT NULL,
	unit_price numeric(12, 2) NOT NULL,
	subtotal numeric(12, 2) NOT NULL,
	created_at timestamptz DEFAULT now() NULL,
	CONSTRAINT sale_details_pkey PRIMARY KEY (id),
	CONSTRAINT sale_details_quantity_check CHECK ((quantity > 0)),
	CONSTRAINT sale_details_subtotal_check CHECK ((subtotal >= (0)::numeric)),
	CONSTRAINT sale_details_unit_price_check CHECK ((unit_price >= (0)::numeric))
);
CREATE INDEX idx_sale_details_product_id ON public.sale_details USING btree (product_id);
CREATE INDEX idx_sale_details_sale_id ON public.sale_details USING btree (sale_id);


-- public.sale_details foreign keys

ALTER TABLE public.sale_details ADD CONSTRAINT sale_details_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sales(id) ON DELETE CASCADE;