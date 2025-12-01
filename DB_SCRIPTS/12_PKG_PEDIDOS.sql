-- ========================================
-- PAQUETE: PKG_PEDIDOS
-- Gestión completa del ciclo de vida de pedidos
-- ========================================

CREATE OR REPLACE PACKAGE pkg_pedidos AS
    
    -- Crea un nuevo pedido en estado Pendiente
    PROCEDURE sp_crear_pedido(
        p_id_usuario IN NUMBER,
        p_id_direccion_envio IN NUMBER,
        p_id_metodo_pago IN NUMBER,
        p_id_pedido_generado OUT NUMBER
    );

    -- Agrega línea de detalle y actualiza totales del pedido
    PROCEDURE sp_agregar_detalle_pedido(
        p_id_pedido IN NUMBER,
        p_id_variante IN NUMBER,
        p_cantidad IN NUMBER
    );

    -- Actualiza el estado de un pedido
    PROCEDURE sp_actualizar_estado_pedido(
        p_id_pedido IN NUMBER,
        p_nuevo_estado IN VARCHAR2
    );

    -- Cancela pedido y restaura inventario
    PROCEDURE sp_cancelar_pedido(
        p_id_pedido IN NUMBER
    );

    -- Lista pedidos de un usuario específico
    PROCEDURE sp_obtener_pedidos_usuario(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Obtiene detalle completo de un pedido (info + productos)
    PROCEDURE sp_obtener_detalle_pedido(
        p_id_pedido IN NUMBER,
        p_cursor_pedido OUT SYS_REFCURSOR,
        p_cursor_detalles OUT SYS_REFCURSOR
    );

    -- Lista todos los pedidos del sistema
    PROCEDURE sp_obtener_todos_pedidos(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Obtiene métodos de pago activos
    PROCEDURE sp_obtener_metodos_pago(
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_pedidos;
/

CREATE OR REPLACE PACKAGE BODY pkg_pedidos AS

    PROCEDURE sp_crear_pedido(
        p_id_usuario IN NUMBER,
        p_id_direccion_envio IN NUMBER,
        p_id_metodo_pago IN NUMBER,
        p_id_pedido_generado OUT NUMBER
    ) IS
        v_numero_pedido VARCHAR2(14);
        v_existe NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM usuarios
        WHERE id_usuario = p_id_usuario AND estado = 'A';

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Usuario no encontrado o inactivo');
        END IF;

        SELECT COUNT(*) INTO v_existe
        FROM direcciones
        WHERE id_direccion = p_id_direccion_envio;

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Dirección de envío no encontrada');
        END IF;

        SELECT COUNT(*) INTO v_existe
        FROM metodos_pago
        WHERE id_metodo = p_id_metodo_pago AND estado = 'A';

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'Método de pago no encontrado o inactivo');
        END IF;

        SELECT seq_pedidos.NEXTVAL INTO p_id_pedido_generado FROM dual;

        v_numero_pedido := 'PED-' || TO_CHAR(SYSDATE, 'YYYY') || '-' || LPAD(p_id_pedido_generado, 5, '0');

        INSERT INTO pedidos (
            id_pedido,
            id_usuario,
            id_direccion_envio,
            id_metodo_pago,
            numero_pedido,
            fecha_pedido,
            estado,
            subtotal,
            costo_envio,
            impuesto,
            total
        ) VALUES (
            p_id_pedido_generado,
            p_id_usuario,
            p_id_direccion_envio,
            p_id_metodo_pago,
            v_numero_pedido,
            SYSDATE,
            'Pendiente',
            0,
            0,
            0,
            0
        );

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al crear pedido: ' || SQLERRM);
    END sp_crear_pedido;

    PROCEDURE sp_agregar_detalle_pedido(
        p_id_pedido IN NUMBER,
        p_id_variante IN NUMBER,
        p_cantidad IN NUMBER
    ) IS
        v_precio_con_iva NUMBER;
        v_precio_base NUMBER;
        v_iva_linea NUMBER;
        v_total_linea NUMBER;
        v_stock_actual NUMBER;
        v_pedido_existe NUMBER;
        v_estado_pedido VARCHAR2(15);
    BEGIN
        SELECT COUNT(*), MAX(estado) INTO v_pedido_existe, v_estado_pedido
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        IF v_pedido_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20010, 'Pedido no encontrado');
        END IF;

        IF v_estado_pedido != 'Pendiente' THEN
            RAISE_APPLICATION_ERROR(-20011, 'No se pueden agregar productos a un pedido que no está Pendiente');
        END IF;

        IF p_cantidad <= 0 THEN
            RAISE_APPLICATION_ERROR(-20012, 'La cantidad debe ser mayor a 0');
        END IF;

        SELECT a.precio_base, v.stock
        INTO v_precio_con_iva, v_stock_actual
        FROM variantes_articulo v
        INNER JOIN articulos a ON v.id_articulo = a.id_articulo
        WHERE v.id_variante = p_id_variante AND v.estado = 'A' AND a.estado = 'A';

        IF v_stock_actual < p_cantidad THEN
            RAISE_APPLICATION_ERROR(-20013, 'Stock insuficiente. Disponible: ' || v_stock_actual);
        END IF;

        -- Calcular totales con IVA incluido
        v_total_linea := v_precio_con_iva * p_cantidad;
        v_precio_base := ROUND(v_total_linea / 1.19, 2);
        v_iva_linea := v_total_linea - v_precio_base;

        INSERT INTO detalles_pedido (
            id_detalle,
            id_pedido,
            id_variante,
            cantidad,
            precio_unitario,
            subtotal_linea
        ) VALUES (
            seq_detalles_pedido.NEXTVAL,
            p_id_pedido,
            p_id_variante,
            p_cantidad,
            v_precio_con_iva,
            v_total_linea
        );

        UPDATE variantes_articulo
        SET stock = stock - p_cantidad
        WHERE id_variante = p_id_variante;

        UPDATE pedidos p
        SET 
            subtotal = (
                SELECT ROUND(SUM(subtotal_linea / 1.19), 2)
                FROM detalles_pedido 
                WHERE id_pedido = p_id_pedido
            ),
            impuesto = (
                SELECT ROUND(SUM(subtotal_linea) - SUM(subtotal_linea / 1.19), 2)
                FROM detalles_pedido 
                WHERE id_pedido = p_id_pedido
            ),
            total = (
                SELECT SUM(subtotal_linea)
                FROM detalles_pedido 
                WHERE id_pedido = p_id_pedido
            ) + p.costo_envio
        WHERE id_pedido = p_id_pedido;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20014, 'Variante de producto no encontrada o inactiva');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al agregar detalle: ' || SQLERRM);
    END sp_agregar_detalle_pedido;

    PROCEDURE sp_actualizar_estado_pedido(
        p_id_pedido IN NUMBER,
        p_nuevo_estado IN VARCHAR2
    ) IS
        v_existe NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20020, 'Pedido no encontrado');
        END IF;

        IF p_nuevo_estado NOT IN ('Pendiente', 'Confirmado', 'Enviado', 'Entregado', 'Cancelado') THEN
            RAISE_APPLICATION_ERROR(-20021, 'Estado inválido');
        END IF;

        UPDATE pedidos
        SET estado = p_nuevo_estado
        WHERE id_pedido = p_id_pedido;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al actualizar estado: ' || SQLERRM);
    END sp_actualizar_estado_pedido;

    PROCEDURE sp_cancelar_pedido(
        p_id_pedido IN NUMBER
    ) IS
        v_estado_actual VARCHAR2(15);
    BEGIN
        SELECT estado INTO v_estado_actual
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        IF v_estado_actual NOT IN ('Pendiente', 'Confirmado') THEN
            RAISE_APPLICATION_ERROR(-20030, 'Solo se pueden cancelar pedidos Pendientes o Confirmados');
        END IF;

        FOR detalle IN (
            SELECT id_variante, cantidad
            FROM detalles_pedido
            WHERE id_pedido = p_id_pedido
        ) LOOP
            UPDATE variantes_articulo
            SET stock = stock + detalle.cantidad
            WHERE id_variante = detalle.id_variante;
        END LOOP;

        UPDATE pedidos
        SET estado = 'Cancelado'
        WHERE id_pedido = p_id_pedido;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20031, 'Pedido no encontrado');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al cancelar pedido: ' || SQLERRM);
    END sp_cancelar_pedido;

    PROCEDURE sp_obtener_pedidos_usuario(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                p.id_pedido,
                p.numero_pedido,
                p.fecha_pedido,
                p.estado,
                p.subtotal,
                p.costo_envio,
                p.impuesto,
                p.total,
                mp.nombre AS metodo_pago,
                d.direccion_completa,
                c.nombre AS ciudad
            FROM pedidos p
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            INNER JOIN direcciones d ON p.id_direccion_envio = d.id_direccion
            INNER JOIN ciudades c ON d.id_ciudad = c.id_ciudad
            WHERE p.id_usuario = p_id_usuario
            ORDER BY p.fecha_pedido DESC;
    END sp_obtener_pedidos_usuario;

    PROCEDURE sp_obtener_detalle_pedido(
        p_id_pedido IN NUMBER,
        p_cursor_pedido OUT SYS_REFCURSOR,
        p_cursor_detalles OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor_pedido FOR
            SELECT 
                p.id_pedido,
                p.id_usuario,
                p.numero_pedido,
                p.fecha_pedido,
                p.estado,
                p.subtotal,
                p.costo_envio,
                p.impuesto,
                p.total,
                u.cedula,
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                u.correo AS correo_cliente,
                u.telefono_principal,
                mp.nombre AS metodo_pago,
                d.direccion_completa,
                d.barrio,
                c.nombre AS ciudad,
                c.departamento
            FROM pedidos p
            INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            INNER JOIN direcciones d ON p.id_direccion_envio = d.id_direccion
            INNER JOIN ciudades c ON d.id_ciudad = c.id_ciudad
            WHERE p.id_pedido = p_id_pedido;

        OPEN p_cursor_detalles FOR
            SELECT 
                dp.id_detalle,
                dp.cantidad,
                dp.precio_unitario,
                dp.subtotal_linea,
                a.nombre AS nombre_producto,
                a.marca,
                v.talla,
                v.color,
                v.codigo_sku,
                img.url_imagen
            FROM detalles_pedido dp
            INNER JOIN variantes_articulo v ON dp.id_variante = v.id_variante
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            LEFT JOIN (
                SELECT id_articulo, url_imagen
                FROM imagenes_articulo
                WHERE es_principal = 'S'
                AND ROWNUM = 1
            ) img ON a.id_articulo = img.id_articulo
            WHERE dp.id_pedido = p_id_pedido
            ORDER BY dp.id_detalle;
    END sp_obtener_detalle_pedido;

    PROCEDURE sp_obtener_todos_pedidos(
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
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                u.correo AS correo_cliente,
                mp.nombre AS metodo_pago,
                (SELECT COUNT(*) FROM detalles_pedido WHERE id_pedido = p.id_pedido) AS total_productos
            FROM pedidos p
            INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            ORDER BY p.fecha_pedido DESC;
    END sp_obtener_todos_pedidos;

    PROCEDURE sp_obtener_metodos_pago(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                id_metodo,
                nombre
            FROM metodos_pago
            WHERE estado = 'A'
            ORDER BY nombre;
    END sp_obtener_metodos_pago;

END pkg_pedidos;
/