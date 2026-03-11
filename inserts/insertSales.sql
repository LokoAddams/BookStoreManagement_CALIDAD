WITH data_generator AS (
    -- 1. Generamos 21 filas de datos simulados en memoria
    SELECT 
        gen_random_uuid() AS new_sale_id,
        gen_random_uuid() AS fake_client_id,
        gen_random_uuid() AS fake_user_id,
        gen_random_uuid() AS fake_product_id,
        -- Seleccionamos un estado aleatorio
        (ARRAY['COMPLETED', 'PENDING', 'CANCELLED', 'REFUNDED'])[floor(random() * 4 + 1)] AS selected_status,
        -- Generamos precios y cantidades aleatorias
        (random() * 100 + 10)::numeric(10, 2) AS unit_price,
        floor(random() * 5 + 1)::int AS quantity,
        now() - (random() * interval '30 days') AS random_date
    FROM generate_series(1, 21)
),
sales_insert AS (
    -- 2. Insertamos en la tabla SALES usando los datos generados
    INSERT INTO public.sales (
        id, 
        client_id, 
        user_id, 
        sale_date, 
        subtotal, 
        total, 
        status, 
        cancelled_at, 
        cancelled_by, 
        cancellation_reason
    )
    SELECT 
        new_sale_id,
        fake_client_id,
        fake_user_id,
        random_date,
        (unit_price * quantity), -- Subtotal calculado
        (unit_price * quantity), -- Total (debe ser igual al subtotal por tu constraint)
        selected_status,
        -- Lógica para llenar datos de cancelación solo si el estado es CANCELLED
        CASE WHEN selected_status = 'CANCELLED' THEN random_date + interval '1 hour' ELSE NULL END,
        CASE WHEN selected_status = 'CANCELLED' THEN fake_user_id ELSE NULL END,
        CASE WHEN selected_status = 'CANCELLED' THEN 'Cancelado por el usuario o sistema automáticamente' ELSE NULL END
    FROM data_generator
    RETURNING id
)
-- 3. Insertamos en la tabla SALE_DETAILS vinculando con el ID generado arriba
INSERT INTO public.sale_details (
    sale_id, 
    product_id, 
    product_name, 
    quantity, 
    unit_price, 
    subtotal
)
SELECT 
    new_sale_id,
    fake_product_id,
    'Producto Genérico ' || substr(md5(fake_product_id::text), 1, 6), -- Nombre aleatorio
    quantity,
    unit_price,
    (unit_price * quantity)
FROM data_generator;