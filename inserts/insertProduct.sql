BEGIN;

-- =========================
-- 21 inserts: public.categories
-- =========================
INSERT INTO public.categories (id, "name", description, created_at, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 'Electrónica', 'Dispositivos electrónicos y accesorios.', '2025-12-18T10:00:00-04:00', true),
('22222222-2222-2222-2222-222222222222', 'Hogar', 'Productos para el hogar y la cocina.', '2025-12-18T10:01:00-04:00', true),
('33333333-3333-3333-3333-333333333333', 'Oficina', 'Papelería y suministros de oficina.', '2025-12-18T10:02:00-04:00', true),
('44444444-4444-4444-4444-444444444444', 'Deportes', 'Equipamiento y accesorios deportivos.', '2025-12-18T10:03:00-04:00', true),
('55555555-5555-5555-5555-555555555555', 'Moda', 'Ropa y accesorios de moda.', '2025-12-18T10:04:00-04:00', true),
('66666666-6666-6666-6666-666666666666', 'Juguetes', 'Juguetes y entretenimiento infantil.', '2025-12-18T10:05:00-04:00', true),
('77777777-7777-7777-7777-777777777777', 'Salud y Belleza', 'Cuidado personal, higiene y belleza.', '2025-12-18T10:06:00-04:00', true),
('88888888-8888-8888-8888-888888888888', 'Automotriz', 'Accesorios y productos para vehículos.', '2025-12-18T10:07:00-04:00', true),
('99999999-9999-9999-9999-999999999999', 'Mascotas', 'Alimentos y accesorios para mascotas.', '2025-12-18T10:08:00-04:00', true),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Libros', 'Libros físicos y material de lectura.', '2025-12-18T10:09:00-04:00', true),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Videojuegos', 'Consolas, juegos y accesorios gamer.', '2025-12-18T10:10:00-04:00', true),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Ferretería', 'Herramientas y materiales de ferretería.', '2025-12-18T10:11:00-04:00', true),
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Jardín', 'Productos para jardín y exteriores.', '2025-12-18T10:12:00-04:00', true),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Alimentos', 'Alimentos no perecederos y snacks.', '2025-12-18T10:13:00-04:00', true),
('ffffffff-ffff-ffff-ffff-ffffffffffff', 'Bebés', 'Productos y cuidado para bebés.', '2025-12-18T10:14:00-04:00', true),
('12121212-1212-1212-1212-121212121212', 'Música', 'Instrumentos y accesorios musicales.', '2025-12-18T10:15:00-04:00', true),
('13131313-1313-1313-1313-131313131313', 'Fotografía', 'Cámaras y accesorios de fotografía.', '2025-12-18T10:16:00-04:00', true),
('14141414-1414-1414-1414-141414141414', 'Computación', 'Laptops, periféricos y componentes.', '2025-12-18T10:17:00-04:00', true),
('15151515-1515-1515-1515-151515151515', 'Limpieza', 'Productos de limpieza y desinfección.', '2025-12-18T10:18:00-04:00', true),
('16161616-1616-1616-1616-161616161616', 'Papelería', 'Cuadernos, lápices y materiales escolares.', '2025-12-18T10:19:00-04:00', true),
('17171717-1717-1717-1717-171717171717', 'Viajes', 'Accesorios y artículos de viaje.', '2025-12-18T10:20:00-04:00', true);

-- =========================
-- 21 inserts: public.products
-- =========================
INSERT INTO public.products
(id, "name", category_id, description, price, stock, category_name, created_at, is_active)
VALUES
('20000000-0000-0000-0000-000000000001', 'Auriculares Bluetooth X1', '11111111-1111-1111-1111-111111111111', 'Auriculares inalámbricos con micrófono y estuche de carga.', 179.90, 45, 'Electrónica', '2025-12-18T11:00:00-04:00', true),
('20000000-0000-0000-0000-000000000002', 'Lámpara LED de Escritorio', '22222222-2222-2222-2222-222222222222', 'Lámpara LED con brillo regulable y puerto USB.', 89.50, 30, 'Hogar', '2025-12-18T11:01:00-04:00', true),
('20000000-0000-0000-0000-000000000003', 'Resma Papel A4 500 hojas', '33333333-3333-3333-3333-333333333333', 'Papel tamaño A4, 75 g/m², ideal para impresiones diarias.', 38.00, 120, 'Oficina', '2025-12-18T11:02:00-04:00', true),
('20000000-0000-0000-0000-000000000004', 'Pelota de Fútbol N°5', '44444444-4444-4444-4444-444444444444', 'Pelota tamaño oficial con cubierta resistente.', 75.00, 60, 'Deportes', '2025-12-18T11:03:00-04:00', true),
('20000000-0000-0000-0000-000000000005', 'Cinturón de Cuero Clásico', '55555555-5555-5555-5555-555555555555', 'Cinturón unisex de cuero con hebilla metálica.', 65.90, 25, 'Moda', '2025-12-18T11:04:00-04:00', true),
('20000000-0000-0000-0000-000000000006', 'Set Bloques de Construcción', '66666666-6666-6666-6666-666666666666', 'Bloques compatibles, incluye 120 piezas y caja.', 110.00, 40, 'Juguetes', '2025-12-18T11:05:00-04:00', true),
('20000000-0000-0000-0000-000000000007', 'Protector Solar SPF 50', '77777777-7777-7777-7777-777777777777', 'Protector solar facial y corporal, acabado no graso.', 59.90, 80, 'Salud y Belleza', '2025-12-18T11:06:00-04:00', true),
('20000000-0000-0000-0000-000000000008', 'Cargador para Auto 2 Puertos', '88888888-8888-8888-8888-888888888888', 'Cargador rápido para auto con doble salida USB.', 29.90, 100, 'Automotriz', '2025-12-18T11:07:00-04:00', true),
('20000000-0000-0000-0000-000000000009', 'Alimento Seco Premium 2kg', '99999999-9999-9999-9999-999999999999', 'Alimento seco balanceado para perros adultos.', 98.00, 55, 'Mascotas', '2025-12-18T11:08:00-04:00', true),
('20000000-0000-0000-0000-000000000010', 'Novela: Ciencia Ficción Vol. 1', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Novela de ciencia ficción, edición de bolsillo.', 42.00, 70, 'Libros', '2025-12-18T11:09:00-04:00', true),
('20000000-0000-0000-0000-000000000011', 'Control Inalámbrico Pro', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Control inalámbrico compatible con PC y consola.', 249.00, 20, 'Videojuegos', '2025-12-18T11:10:00-04:00', true),
('20000000-0000-0000-0000-000000000012', 'Juego de Destornilladores 6pzs', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Set de destornilladores con mango antideslizante.', 55.00, 90, 'Ferretería', '2025-12-18T11:11:00-04:00', true),
('20000000-0000-0000-0000-000000000013', 'Manguera de Jardín 15m', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Manguera reforzada con conectores universales.', 120.00, 35, 'Jardín', '2025-12-18T11:12:00-04:00', true),
('20000000-0000-0000-0000-000000000014', 'Pack Snacks Mixtos', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Selección de snacks salados y dulces (pack surtido).', 35.50, 150, 'Alimentos', '2025-12-18T11:13:00-04:00', true),
('20000000-0000-0000-0000-000000000015', 'Pañales Talla M 40u', 'ffffffff-ffff-ffff-ffff-ffffffffffff', 'Pañales talla M, alta absorción, paquete de 40 unidades.', 115.00, 65, 'Bebés', '2025-12-18T11:14:00-04:00', true),
('20000000-0000-0000-0000-000000000016', 'Ukelele Soprano', '12121212-1212-1212-1212-121212121212', 'Ukelele soprano con funda y afinador.', 210.00, 18, 'Música', '2025-12-18T11:15:00-04:00', true),
('20000000-0000-0000-0000-000000000017', 'Trípode para Cámara 160cm', '13131313-1313-1313-1313-131313131313', 'Trípode liviano con cabeza ajustable y nivel.', 135.00, 22, 'Fotografía', '2025-12-18T11:16:00-04:00', true),
('20000000-0000-0000-0000-000000000018', 'Teclado Mecánico 87 teclas', '14141414-1414-1414-1414-141414141414', 'Teclado compacto, switches mecánicos y anti-ghosting.', 299.90, 15, 'Computación', '2025-12-18T11:17:00-04:00', true),
('20000000-0000-0000-0000-000000000019', 'Desinfectante Multiuso 1L', '15151515-1515-1515-1515-151515151515', 'Desinfectante multiuso para superficies, 1 litro.', 22.50, 200, 'Limpieza', '2025-12-18T11:18:00-04:00', true),
('20000000-0000-0000-0000-000000000020', 'Cuaderno Universitario 100h', '16161616-1616-1616-1616-161616161616', 'Cuaderno de 100 hojas, rayado, tapa dura.', 18.00, 300, 'Papelería', '2025-12-18T11:19:00-04:00', true),
('20000000-0000-0000-0000-000000000021', 'Maleta de Cabina 20"', '17171717-1717-1717-1717-171717171717', 'Maleta rígida de cabina, 4 ruedas y candado.', 399.00, 12, 'Viajes', '2025-12-18T11:20:00-04:00', true);

COMMIT;
