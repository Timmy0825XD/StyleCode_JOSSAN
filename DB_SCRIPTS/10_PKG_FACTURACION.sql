-- ========================================
-- PAQUETE: PKG_FACTURACION
-- Gestión de facturas electrónicas y validación DIAN
-- ========================================

CREATE OR REPLACE PACKAGE pkg_facturacion AS

    -- Crea factura electrónica desde un pedido
    PROCEDURE sp_crear_factura(
        p_id_pedido          IN facturas.id_pedido%TYPE,
        p_id_usuario         IN facturas.id_usuario%TYPE,
        p_numero_factura     IN facturas.numero_factura%TYPE,
        p_cufe               IN facturas.cufe%TYPE,
        p_codigo_qr          IN facturas.codigo_qr%TYPE,
        p_estado_dian        IN facturas.estado_dian%TYPE,
        p_fecha_dian         IN facturas.fecha_dian%TYPE,
        p_id_factura_out     OUT facturas.id_factura%TYPE
    );

    -- Lista todas las facturas con información del cliente
    PROCEDURE sp_obtener_todas_facturas(
        p_cursor             OUT SYS_REFCURSOR
    );

    -- Verifica si existe factura para un pedido
    PROCEDURE sp_verificar_factura_pedido(
        p_id_pedido          IN facturas.id_pedido%TYPE,
        p_existe             OUT NUMBER,
        p_id_factura         OUT facturas.id_factura%TYPE
    );

END pkg_facturacion;
/

CREATE OR REPLACE PACKAGE BODY pkg_facturacion AS

    PROCEDURE sp_crear_factura(
        p_id_pedido          IN facturas.id_pedido%TYPE,
        p_id_usuario         IN facturas.id_usuario%TYPE,
        p_numero_factura     IN facturas.numero_factura%TYPE,
        p_cufe               IN facturas.cufe%TYPE,
        p_codigo_qr          IN facturas.codigo_qr%TYPE,
        p_estado_dian        IN facturas.estado_dian%TYPE,
        p_fecha_dian         IN facturas.fecha_dian%TYPE,
        p_id_factura_out     OUT facturas.id_factura%TYPE
    ) IS
        v_id_factura         facturas.id_factura%TYPE;
        v_subtotal           pedidos.subtotal%TYPE;
        v_impuesto           pedidos.impuesto%TYPE;
        v_total              pedidos.total%TYPE;
    BEGIN
        SELECT subtotal, impuesto, total
        INTO v_subtotal, v_impuesto, v_total
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        SELECT seq_facturas.NEXTVAL INTO v_id_factura FROM dual;

        INSERT INTO facturas (
            id_factura, id_pedido, id_usuario, numero_factura,
            cufe, codigo_qr, fecha_emision, subtotal, impuesto, total,
            estado, estado_dian, fecha_dian
        ) VALUES (
            v_id_factura, p_id_pedido, p_id_usuario, p_numero_factura,
            p_cufe, p_codigo_qr, SYSDATE, v_subtotal, v_impuesto, v_total,
            'Generada', p_estado_dian, p_fecha_dian
        );

        INSERT INTO detalles_factura (
            id_detalle, id_factura, id_variante, cantidad, precio_unitario, subtotal_linea
        )
        SELECT
            seq_detalles_factura.NEXTVAL,
            v_id_factura,
            id_variante,
            cantidad,
            precio_unitario,
            subtotal_linea
        FROM detalles_pedido
        WHERE id_pedido = p_id_pedido;

        COMMIT;

        p_id_factura_out := v_id_factura;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END sp_crear_factura;

    PROCEDURE sp_obtener_todas_facturas(
        p_cursor             OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                f.id_factura,
                f.id_pedido,
                f.id_usuario,
                f.numero_factura,
                f.cufe,
                f.codigo_qr,
                f.fecha_emision,
                f.subtotal,
                f.impuesto,
                f.total,
                f.estado,
                f.estado_dian,
                f.fecha_dian,
                p.numero_pedido,
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                u.correo AS correo_cliente
            FROM facturas f
            INNER JOIN pedidos p ON f.id_pedido = p.id_pedido
            INNER JOIN usuarios u ON f.id_usuario = u.id_usuario
            ORDER BY f.fecha_emision DESC;
    END sp_obtener_todas_facturas;

    PROCEDURE sp_verificar_factura_pedido(
        p_id_pedido          IN facturas.id_pedido%TYPE,
        p_existe             OUT NUMBER,
        p_id_factura         OUT facturas.id_factura%TYPE
    ) IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*), NVL(MAX(id_factura), 0)
        INTO v_count, p_id_factura
        FROM facturas
        WHERE id_pedido = p_id_pedido;

        IF v_count > 0 THEN
            p_existe := 1;
        ELSE
            p_existe := 0;
            p_id_factura := 0;
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            p_existe := 0;
            p_id_factura := 0;
            RAISE;
    END sp_verificar_factura_pedido;

END pkg_facturacion;
/