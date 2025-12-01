-- ========================================
-- PAQUETE: PKG_ESTADISTICAS
-- Métricas y reportes del dashboard administrativo
-- ========================================

CREATE OR REPLACE PACKAGE pkg_estadisticas AS

    -- Métricas consolidadas del dashboard principal
    PROCEDURE obtener_metricas_dashboard(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Ventas mensuales por año (gráfico de líneas)
    PROCEDURE obtener_ventas_mensuales(
        p_anio IN NUMBER DEFAULT EXTRACT(YEAR FROM SYSDATE),
        p_cursor OUT SYS_REFCURSOR
    );

    -- Distribución de ventas por categoría (gráfico circular)
    PROCEDURE obtener_ventas_por_categoria(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Productos más vendidos (últimos 30 días)
    PROCEDURE obtener_top_productos(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Lista de pedidos recientes
    PROCEDURE obtener_pedidos_recientes(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Cantidad de pedidos por estado (mes actual)
    PROCEDURE obtener_pedidos_por_estado(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Clientes con mayor volumen de compras
    PROCEDURE obtener_top_clientes(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Variantes con stock bajo (alertas de inventario)
    PROCEDURE obtener_productos_stock_bajo(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Análisis de ventas por género de producto
    PROCEDURE obtener_ventas_por_genero(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Indicadores de crecimiento de ventas (mes y año)
    PROCEDURE obtener_crecimiento_ventas(
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_estadisticas;
/

CREATE OR REPLACE PACKAGE BODY pkg_estadisticas AS

    PROCEDURE obtener_metricas_dashboard(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')
                   AND estado NOT IN ('Cancelado')) AS ventas_mes_actual,

                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1)
                   AND estado NOT IN ('Cancelado')) AS ventas_mes_anterior,

                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido) = TRUNC(SYSDATE)
                   AND estado NOT IN ('Cancelado')) AS ventas_hoy,

                (SELECT COUNT(*)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')) AS total_pedidos_mes,

                (SELECT COUNT(*)
                 FROM pedidos
                 WHERE estado = 'Pendiente') AS pedidos_pendientes,

                (SELECT COUNT(*)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido) = TRUNC(SYSDATE)) AS pedidos_hoy,

                (SELECT COUNT(*)
                 FROM usuarios
                 WHERE id_rol = 2 AND estado = 'A') AS total_clientes,

                (SELECT COUNT(*)
                 FROM usuarios
                 WHERE id_rol = 2 
                   AND TRUNC(fecha_registro, 'MM') = TRUNC(SYSDATE, 'MM')) AS clientes_nuevos_mes,

                (SELECT COUNT(DISTINCT id_articulo)
                 FROM articulos
                 WHERE estado = 'A') AS total_productos_activos,

                (SELECT COUNT(*)
                 FROM variantes_articulo
                 WHERE stock <= 5 AND stock > 0 AND estado = 'A') AS productos_stock_bajo,

                (SELECT NVL(ROUND(AVG(total), 2), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')
                   AND estado NOT IN ('Cancelado')) AS ticket_promedio,

                (SELECT NVL(SUM(impuesto), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')
                   AND estado NOT IN ('Cancelado')) AS iva_mes,

                (SELECT ROUND(
                    (COUNT(CASE WHEN estado = 'Entregado' THEN 1 END) * 100.0) / 
                    NULLIF(COUNT(*), 0), 2)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')) AS tasa_entrega_porcentaje

            FROM DUAL;
    END obtener_metricas_dashboard;

    PROCEDURE obtener_ventas_mensuales(
        p_anio IN NUMBER DEFAULT EXTRACT(YEAR FROM SYSDATE),
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                TO_CHAR(mes, 'Month', 'NLS_DATE_LANGUAGE=SPANISH') AS mes_nombre,
                EXTRACT(MONTH FROM mes) AS mes_numero,
                NVL(SUM(total), 0) AS total_ventas,
                COUNT(id_pedido) AS cantidad_pedidos
            FROM (
                SELECT ADD_MONTHS(TO_DATE('01-01-' || p_anio, 'DD-MM-YYYY'), LEVEL - 1) AS mes
                FROM DUAL
                CONNECT BY LEVEL <= 12
            ) meses
            LEFT JOIN pedidos p ON TRUNC(p.fecha_pedido, 'MM') = meses.mes
                AND p.estado NOT IN ('Cancelado')
            GROUP BY mes
            ORDER BY mes;
    END obtener_ventas_mensuales;

    PROCEDURE obtener_ventas_por_categoria(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                ct.nombre AS categoria,
                COUNT(DISTINCT p.id_pedido) AS cantidad_pedidos,
                SUM(dp.cantidad) AS unidades_vendidas,
                SUM(dp.subtotal_linea) AS total_ventas,
                ROUND(
                    (SUM(dp.subtotal_linea) * 100.0) / 
                    NULLIF((SELECT SUM(subtotal_linea) 
                            FROM detalles_pedido dp2 
                            JOIN pedidos p2 ON dp2.id_pedido = p2.id_pedido
                            WHERE p2.fecha_pedido >= ADD_MONTHS(SYSDATE, -1)
                              AND p2.estado NOT IN ('Cancelado')), 0),
                    2
                ) AS porcentaje
            FROM detalles_pedido dp
            INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
            INNER JOIN variantes_articulo v ON dp.id_variante = v.id_variante
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            INNER JOIN categorias_tipo ct ON a.id_categoria_tipo = ct.id_categoria_tipo
            WHERE p.fecha_pedido >= ADD_MONTHS(SYSDATE, -1)
              AND p.estado NOT IN ('Cancelado')
            GROUP BY ct.nombre
            ORDER BY total_ventas DESC;
    END obtener_ventas_por_categoria;

    PROCEDURE obtener_top_productos(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_articulo,
                a.nombre,
                a.marca,
                a.precio_base,
                SUM(dp.cantidad) AS unidades_vendidas,
                COUNT(DISTINCT dp.id_pedido) AS cantidad_pedidos,
                SUM(dp.subtotal_linea) AS ingresos_generados,
                (SELECT url_imagen 
                 FROM imagenes_articulo 
                 WHERE id_articulo = a.id_articulo 
                   AND es_principal = 'S' 
                   AND ROWNUM = 1) AS imagen
            FROM detalles_pedido dp
            INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
            INNER JOIN variantes_articulo v ON dp.id_variante = v.id_variante
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            WHERE p.fecha_pedido >= ADD_MONTHS(SYSDATE, -1)
              AND p.estado NOT IN ('Cancelado')
            GROUP BY a.id_articulo, a.nombre, a.marca, a.precio_base
            ORDER BY unidades_vendidas DESC
            FETCH FIRST p_limite ROWS ONLY;
    END obtener_top_productos;

    PROCEDURE obtener_pedidos_recientes(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                p.id_pedido,
                p.numero_pedido,
                p.fecha_pedido,
                p.estado,
                p.total,
                u.primer_nombre || ' ' || u.apellido_paterno AS cliente,
                u.correo AS email_cliente,
                mp.nombre AS metodo_pago,
                (SELECT COUNT(*) 
                 FROM detalles_pedido 
                 WHERE id_pedido = p.id_pedido) AS cantidad_productos
            FROM pedidos p
            INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            ORDER BY p.fecha_pedido DESC
            FETCH FIRST p_limite ROWS ONLY;
    END obtener_pedidos_recientes;

    PROCEDURE obtener_pedidos_por_estado(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                estado,
                COUNT(*) AS cantidad,
                NVL(SUM(total), 0) AS total_ventas,
                ROUND((COUNT(*) * 100.0) / SUM(COUNT(*)) OVER (), 2) AS porcentaje
            FROM pedidos
            WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')
            GROUP BY estado
            ORDER BY cantidad DESC;
    END obtener_pedidos_por_estado;

    PROCEDURE obtener_top_clientes(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                u.id_usuario,
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_completo,
                u.correo,
                u.telefono_principal,
                COUNT(p.id_pedido) AS total_pedidos,
                NVL(SUM(p.total), 0) AS total_gastado,
                ROUND(AVG(p.total), 2) AS ticket_promedio,
                MAX(p.fecha_pedido) AS ultima_compra
            FROM usuarios u
            INNER JOIN pedidos p ON u.id_usuario = p.id_usuario
            WHERE p.estado NOT IN ('Cancelado')
              AND u.id_rol = 2
            GROUP BY u.id_usuario, u.primer_nombre, u.apellido_paterno, u.correo, u.telefono_principal
            ORDER BY total_gastado DESC
            FETCH FIRST p_limite ROWS ONLY;
    END obtener_top_clientes;

    PROCEDURE obtener_productos_stock_bajo(
        p_limite IN NUMBER DEFAULT 10,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_articulo,
                a.nombre,
                a.marca,
                v.talla,
                v.color,
                v.codigo_sku,
                v.stock,
                (SELECT url_imagen 
                 FROM imagenes_articulo 
                 WHERE id_articulo = a.id_articulo 
                   AND es_principal = 'S' 
                   AND ROWNUM = 1) AS imagen
            FROM variantes_articulo v
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            WHERE v.stock <= 5 
              AND v.stock > 0 
              AND v.estado = 'A'
            ORDER BY v.stock ASC
            FETCH FIRST p_limite ROWS ONLY;
    END obtener_productos_stock_bajo;

    PROCEDURE obtener_ventas_por_genero(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.genero,
                COUNT(DISTINCT p.id_pedido) AS cantidad_pedidos,
                SUM(dp.cantidad) AS unidades_vendidas,
                NVL(SUM(dp.subtotal_linea), 0) AS total_ventas,
                ROUND(
                    (SUM(dp.subtotal_linea) * 100.0) / 
                    NULLIF((SELECT SUM(subtotal_linea) 
                            FROM detalles_pedido dp2 
                            JOIN pedidos p2 ON dp2.id_pedido = p2.id_pedido
                            WHERE p2.fecha_pedido >= ADD_MONTHS(SYSDATE, -1)
                              AND p2.estado NOT IN ('Cancelado')), 0),
                    2
                ) AS porcentaje
            FROM detalles_pedido dp
            INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
            INNER JOIN variantes_articulo v ON dp.id_variante = v.id_variante
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            WHERE p.fecha_pedido >= ADD_MONTHS(SYSDATE, -1)
              AND p.estado NOT IN ('Cancelado')
            GROUP BY a.genero
            ORDER BY total_ventas DESC;
    END obtener_ventas_por_genero;

    PROCEDURE obtener_crecimiento_ventas(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM')
                   AND estado NOT IN ('Cancelado')) AS ventas_mes_actual,

                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE TRUNC(fecha_pedido, 'MM') = ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1)
                   AND estado NOT IN ('Cancelado')) AS ventas_mes_anterior,

                ROUND(
                    ((SELECT NVL(SUM(total), 0) FROM pedidos 
                      WHERE TRUNC(fecha_pedido, 'MM') = TRUNC(SYSDATE, 'MM') 
                        AND estado NOT IN ('Cancelado')) -
                     (SELECT NVL(SUM(total), 0) FROM pedidos 
                      WHERE TRUNC(fecha_pedido, 'MM') = ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1) 
                        AND estado NOT IN ('Cancelado'))) * 100.0 /
                    NULLIF((SELECT SUM(total) FROM pedidos 
                            WHERE TRUNC(fecha_pedido, 'MM') = ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1)
                              AND estado NOT IN ('Cancelado')), 0),
                    2
                ) AS crecimiento_porcentaje_mes,

                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE EXTRACT(YEAR FROM fecha_pedido) = EXTRACT(YEAR FROM SYSDATE)
                   AND estado NOT IN ('Cancelado')) AS ventas_anio_actual,

                (SELECT NVL(SUM(total), 0)
                 FROM pedidos
                 WHERE EXTRACT(YEAR FROM fecha_pedido) = EXTRACT(YEAR FROM SYSDATE) - 1
                   AND estado NOT IN ('Cancelado')) AS ventas_anio_anterior,

                ROUND(
                    ((SELECT NVL(SUM(total), 0) FROM pedidos 
                      WHERE EXTRACT(YEAR FROM fecha_pedido) = EXTRACT(YEAR FROM SYSDATE)
                        AND estado NOT IN ('Cancelado')) -
                     (SELECT NVL(SUM(total), 0) FROM pedidos 
                      WHERE EXTRACT(YEAR FROM fecha_pedido) = EXTRACT(YEAR FROM SYSDATE) - 1
                        AND estado NOT IN ('Cancelado'))) * 100.0 /
                    NULLIF((SELECT SUM(total) FROM pedidos 
                            WHERE EXTRACT(YEAR FROM fecha_pedido) = EXTRACT(YEAR FROM SYSDATE) - 1
                              AND estado NOT IN ('Cancelado')), 0),
                    2
                ) AS crecimiento_porcentaje_anio

            FROM DUAL;
    END obtener_crecimiento_ventas;

END pkg_estadisticas;
/