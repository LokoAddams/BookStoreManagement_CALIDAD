-- Migración manual para añadir CI al módulo de clientes
ALTER TABLE clients ADD COLUMN IF NOT EXISTS ci VARCHAR(20) NOT NULL DEFAULT '';

-- Opcional: normalizar a mayúsculas
-- UPDATE clients SET ci = UPPER(ci);

-- Índice único condicional para evitar duplicados entre activos
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_ci_active ON clients (ci) WHERE is_active = TRUE;
