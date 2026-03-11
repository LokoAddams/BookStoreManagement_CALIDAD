-- Script para crear la base de datos y tabla para MicroServiceReports
CREATE DATABASE microservice_reports;

\c microservice_reports;

CREATE TABLE IF NOT EXISTS sale_event_records (
    "Id" UUID PRIMARY KEY,
    "SaleId" BIGINT NOT NULL,
    "Payload" TEXT NOT NULL,
    "Exchange" VARCHAR(200) NOT NULL,
    "RoutingKey" VARCHAR(200) NOT NULL,
    "ReceivedAt" TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_sale_event_records_sale_id ON sale_event_records("SaleId");
CREATE INDEX IF NOT EXISTS idx_sale_event_records_received_at ON sale_event_records("ReceivedAt");

-- Verificar la tabla
SELECT * FROM sale_event_records LIMIT 10;



CREATE TABLE public.sales (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    client_id UUID NOT NULL,
    user_id UUID NOT NULL,

    sale_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    subtotal NUMERIC(10,2) NOT NULL CHECK (subtotal >= 0),
    total NUMERIC(10,2) NOT NULL CHECK (total >= 0),

    status VARCHAR(20) NOT NULL DEFAULT 'COMPLETED',

    cancelled_at TIMESTAMPTZ,
    cancelled_by UUID,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    

    CONSTRAINT chk_sales_status
        CHECK (status IN ('COMPLETED', 'CANCELLED', 'PENDING', 'REFUNDED')),

    CONSTRAINT chk_sales_total_equals_subtotal
        CHECK (total = subtotal)
);


ALTER TABLE clients ADD COLUMN IF NOT EXISTS ci VARCHAR(20) NOT NULL DEFAULT '';
UPDATE clients SET ci = '' WHERE ci IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_ci ON clients (ci) WHERE is_active = TRUE;