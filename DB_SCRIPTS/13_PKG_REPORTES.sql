-- ========================================
-- PAQUETE: PKG_REPORTES
-- Reportes financieros y análisis de ventas
-- ========================================

CREATE OR REPLACE PACKAGE pkg_reportes AS

    -- Resumen financiero del período (ventas, ticket promedio, IVA)
    PROCEDURE sp_obtener_resumen_financiero(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Productos más vendidos en el período
    PROCEDURE sp_obtener_top_productos(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Lista detallada de pedidos en el período
    PROCEDURE sp_obtener_detalle_ventas(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Análisis de uso e impacto de cupones
    PROCEDURE sp_obtener_resumen_cupones(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_reportes;
/

CREATE OR REPLACE PACKAGE BODY pkg_reportes AS

    PROCEDURE sp_obtener_resumen_financiero(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            WITH resumen_pedidos AS (
                SELECT 
                    NVL(SUM(p.total), 0) AS total_ventas,
                    COUNT(DISTINCT p.id_pedido) AS total_pedidos,
                    CASE 
                        WHEN COUNT(DISTINCT p.id_pedido) > 0 
                        THEN ROUND(NVL(SUM(p.total), 0) / COUNT(DISTINCT p.id_pedido), 2)
                        ELSE 0 
                    END AS ticket_promedio,
                    NVL(SUM(p.descuento_cupon), 0) AS total_descuentos,
                    NVL(SUM(p.total - NVL(p.descuento_cupon, 0)), 0) AS ingresos_netos,
                    NVL(SUM(p.impuesto), 0) AS iva_total,
                    NVL(SUM(p.subtotal), 0) AS subtotal_total,
                    COUNT(DISTINCT p.id_usuario) AS clientes_activos
                FROM pedidos p
                WHERE p.fecha_pedido BETWEEN p_fecha_inicio AND p_fecha_fin
                  AND p.estado IN ('Completado', 'Entregado', 'Pendiente')
            ),
            clientes_nuevos_count AS (
                SELECT COUNT(*) AS clientes_nuevos
                FROM usuarios u
                WHERE u.fecha_registro BETWEEN p_fecha_inicio AND p_fecha_fin
                  AND u.estado = 'A'
            )
            SELECT 
                rp.total_ventas,
                rp.total_pedidos,
                rp.ticket_promedio,
                rp.total_descuentos,
                rp.ingresos_netos,
                rp.iva_total,
                rp.subtotal_total,
                rp.clientes_activos,
                cn.clientes_nuevos
            FROM resumen_pedidos rp
            CROSS JOIN clientes_nuevos_count cn;

    END sp_obtener_resumen_financiero;

    PROCEDURE sp_obtener_top_productos(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_articulo,
                a.nombre AS nombre_producto,
                a.marca,
                ct.nombre AS categoria_tipo,
                co.nombre AS categoria_ocasion,
                SUM(dp.cantidad) AS cantidad_vendida,
                ROUND(SUM(dp.subtotal_linea), 2) AS ingresos_generados,
                ROUND(AVG(dp.precio_unitario), 2) AS precio_promedio
            FROM detalles_pedido dp
            INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
            INNER JOIN variantes_articulo va ON dp.id_variante = va.id_variante
            INNER JOIN articulos a ON va.id_articulo = a.id_articulo
            INNER JOIN categorias_tipo ct ON a.id_categoria_tipo = ct.id_categoria_tipo
            INNER JOIN categorias_ocasion co ON a.id_categoria_ocasion = co.id_categoria_ocasion
            WHERE p.fecha_pedido BETWEEN p_fecha_inicio AND p_fecha_fin
              AND p.estado IN ('Completado', 'Entregado', 'Pendiente')
            GROUP BY 
                a.id_articulo, 
                a.nombre, 
                a.marca, 
                ct.nombre, 
                co.nombre
            ORDER BY cantidad_vendida DESC, ingresos_generados DESC
            FETCH FIRST p_limite ROWS ONLY;

    END sp_obtener_top_productos;

    PROCEDURE sp_obtener_detalle_ventas(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                p.id_pedido,
                p.numero_pedido,
                p.fecha_pedido,
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                u.correo AS correo_cliente,
                mp.nombre AS metodo_pago,
                p.subtotal,
                p.impuesto,
                NVL(p.descuento_cupon, 0) AS descuento,
                p.total,
                p.estado,
                CASE 
                    WHEN EXISTS (SELECT 1 FROM facturas f WHERE f.id_pedido = p.id_pedido) 
                    THEN 'SI' 
                    ELSE 'NO' 
                END AS tiene_factura
            FROM pedidos p
            INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            WHERE p.fecha_pedido BETWEEN p_fecha_inicio AND p_fecha_fin
              AND p.estado IN ('Completado', 'Entregado', 'Pendiente')
            ORDER BY p.fecha_pedido DESC;

    END sp_obtener_detalle_ventas;

    PROCEDURE sp_obtener_resumen_cupones(
        p_fecha_inicio IN DATE DEFAULT SYSDATE - 30,
        p_fecha_fin IN DATE DEFAULT SYSDATE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                c.codigo AS codigo_cupon,
                c.descripcion,
                c.tipo_descuento,
                c.valor_descuento,
                COUNT(cu.id_uso) AS veces_usado,
                ROUND(SUM(cu.descuento_aplicado), 2) AS total_descontado,
                ROUND(AVG(cu.descuento_aplicado), 2) AS descuento_promedio
            FROM cupones c
            INNER JOIN cupones_usados cu ON c.id_cupon = cu.id_cupon
            INNER JOIN pedidos p ON cu.id_pedido = p.id_pedido
            WHERE p.fecha_pedido BETWEEN p_fecha_inicio AND p_fecha_fin
              AND p.estado IN ('Completado', 'Entregado', 'Pendiente')
              AND c.estado = 'A'
            GROUP BY 
                c.codigo, 
                c.descripcion, 
                c.tipo_descuento, 
                c.valor_descuento
            HAVING COUNT(cu.id_uso) > 0
            ORDER BY total_descontado DESC, veces_usado DESC;

    END sp_obtener_resumen_cupones;

END pkg_reportes;
/