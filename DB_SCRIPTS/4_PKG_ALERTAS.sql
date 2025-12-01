-- ========================================
-- PAQUETE: PKG_ALERTAS
-- Gestión de alertas de bajo stock
-- ========================================

CREATE OR REPLACE PACKAGE pkg_alertas AS

    -- Obtiene alertas pendientes de resolver
    PROCEDURE obtener_alertas_pendientes(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Obtiene todas las alertas con filtro opcional por estado
    PROCEDURE obtener_todas_alertas(
        p_estado IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    );  

    -- Obtiene el detalle completo de una alerta específica
    PROCEDURE obtener_detalle_alerta(
        p_id_alerta IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_alertas;
/

CREATE OR REPLACE PACKAGE BODY pkg_alertas AS

    PROCEDURE obtener_alertas_pendientes(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_alerta,
                a.id_variante,
                a.stock_actual,
                a.fecha_alerta,
                art.id_articulo,
                art.nombre AS nombre_producto,
                art.marca,
                v.talla,
                v.color,
                v.codigo_sku,
                v.stock AS stock_actual_bd,
                (SELECT url_imagen 
                 FROM imagenes_articulo 
                 WHERE id_articulo = art.id_articulo 
                   AND es_principal = 'S' 
                   AND ROWNUM = 1) AS imagen_producto,
                TRUNC(SYSDATE - a.fecha_alerta) AS dias_pendiente
            FROM alertas_stock a
            INNER JOIN variantes_articulo v ON a.id_variante = v.id_variante
            INNER JOIN articulos art ON v.id_articulo = art.id_articulo
            WHERE a.estado = 'Pendiente'
            ORDER BY a.fecha_alerta ASC;
    END obtener_alertas_pendientes;

    PROCEDURE obtener_todas_alertas(
        p_estado IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_alerta,
                a.id_variante,
                a.stock_actual,
                a.fecha_alerta,
                a.fecha_resolucion,
                a.estado,
                a.resuelto_por,
                art.nombre AS nombre_producto,
                art.marca,
                v.talla,
                v.color,
                v.codigo_sku,
                v.stock AS stock_actual_bd
            FROM alertas_stock a
            INNER JOIN variantes_articulo v ON a.id_variante = v.id_variante
            INNER JOIN articulos art ON v.id_articulo = art.id_articulo
            WHERE (p_estado IS NULL OR a.estado = p_estado)
            ORDER BY 
                CASE WHEN a.estado = 'Pendiente' THEN 0 ELSE 1 END,
                a.fecha_alerta DESC;
    END obtener_todas_alertas;

    PROCEDURE obtener_detalle_alerta(
        p_id_alerta IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                a.id_alerta,
                a.stock_actual AS stock_al_crear_alerta,
                a.fecha_alerta,
                a.fecha_resolucion,
                a.estado,
                a.resuelto_por,
                art.id_articulo,
                art.nombre AS nombre_producto,
                art.marca,
                art.precio_base,
                v.id_variante,
                v.talla,
                v.color,
                v.codigo_sku,
                v.stock AS stock_actual,
                (SELECT url_imagen 
                 FROM imagenes_articulo 
                 WHERE id_articulo = art.id_articulo 
                   AND es_principal = 'S' 
                   AND ROWNUM = 1) AS imagen_producto,
                CASE 
                    WHEN a.estado = 'Resuelta' THEN 
                        TRUNC(a.fecha_resolucion - a.fecha_alerta)
                    ELSE 
                        TRUNC(SYSDATE - a.fecha_alerta)
                END AS dias_resolucion
            FROM alertas_stock a
            INNER JOIN variantes_articulo v ON a.id_variante = v.id_variante
            INNER JOIN articulos art ON v.id_articulo = art.id_articulo
            WHERE a.id_alerta = p_id_alerta;
    END obtener_detalle_alerta;

END pkg_alertas;
/