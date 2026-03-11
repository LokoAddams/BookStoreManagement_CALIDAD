-- public.sales definition

-- Drop table

-- DROP TABLE public.sales;

CREATE TABLE public.sales (
	id uuid DEFAULT gen_random_uuid() NOT NULL,
	client_id uuid NOT NULL,
	client_name varchar(255) NULL,
	client_ci varchar(50) NULL,
	user_id uuid NOT NULL,
	user_name varchar(255) NULL,
	sale_date timestamptz DEFAULT now() NOT NULL,
	subtotal numeric(12, 2) NOT NULL,
	total numeric(12, 2) NOT NULL,
	status varchar(20) DEFAULT 'COMPLETED'::character varying NOT NULL,
	cancelled_at timestamptz NULL,
	cancelled_by uuid NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT sales_pkey PRIMARY KEY (id),
	CONSTRAINT sales_status_check CHECK (((status)::text = ANY ((ARRAY['PENDING'::character varying, 'COMPLETED'::character varying, 'CANCELLED'::character varying, 'REFUNDED'::character varying])::text[]))),
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
	"Id" uuid DEFAULT gen_random_uuid() NOT NULL,
	"SaleId" varchar(50) NOT NULL,
	"ProductId" varchar(50) NOT NULL,
	"ProductName" varchar(200) NULL,
	"Quantity" int4 NOT NULL,
	"UnitPrice" numeric(18, 2) NOT NULL,
	"Subtotal" numeric(18, 2) NOT NULL,
	"CreatedAt" timestamp NOT NULL,
	CONSTRAINT sale_details_pkey PRIMARY KEY ("Id")
);
CREATE INDEX "IX_sale_details_SaleId" ON public.sale_details USING btree ("SaleId");