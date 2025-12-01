-- ========================================
-- PAQUETE: PKG_CUPONES
-- Gestión de cupones de descuento y promociones
-- ========================================

CREATE OR REPLACE PACKAGE pkg_cupones AS
    
    -- Crea un nuevo cupón de descuento
    PROCEDURE sp_crear_cupon(
        p_codigo IN VARCHAR2,
        p_descripcion IN VARCHAR2,
        p_tipo_descuento IN VARCHAR2,
        p_valor_descuento IN NUMBER,
        p_usos_maximos IN NUMBER,
        p_fecha_inicio IN DATE,
        p_fecha_expiracion IN DATE,
        p_id_cupon_out OUT NUMBER
    );

    -- Valida si un cupón es aplicable para un usuario y pedido
    PROCEDURE sp_validar_cupon(
        p_codigo IN VARCHAR2,
        p_id_usuario IN NUMBER,
        p_subtotal IN NUMBER,
        p_es_valido OUT NUMBER,
        p_mensaje OUT VARCHAR2,
        p_descuento OUT NUMBER,
        p_id_cupon OUT NUMBER
    );

    -- Aplica un cupón válido a un pedido
    PROCEDURE sp_aplicar_cupon(
        p_id_pedido IN NUMBER,
        p_codigo_cupon IN VARCHAR2,
        p_id_usuario IN NUMBER
    );

    -- Lista todos los cupones (Admin)
    PROCEDURE sp_obtener_todos_cupones(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Lista cupones disponibles para un usuario
    PROCEDURE sp_obtener_cupones_disponibles(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Desactiva un cupón existente
    PROCEDURE sp_desactivar_cupon(
        p_id_cupon IN NUMBER
    );

    -- Obtiene el historial de uso de un cupón
    PROCEDURE sp_obtener_historial_uso(
        p_id_cupon IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_cupones;
/

CREATE OR REPLACE PACKAGE BODY pkg_cupones AS

    PROCEDURE sp_crear_cupon(
        p_codigo IN VARCHAR2,
        p_descripcion IN VARCHAR2,
        p_tipo_descuento IN VARCHAR2,
        p_valor_descuento IN NUMBER,
        p_usos_maximos IN NUMBER,
        p_fecha_inicio IN DATE,
        p_fecha_expiracion IN DATE,
        p_id_cupon_out OUT NUMBER
    ) IS
        v_existe NUMBER;
        v_fecha_inicio DATE;
        v_fecha_expiracion DATE;
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM cupones
        WHERE UPPER(codigo) = UPPER(p_codigo);
        
        IF v_existe > 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'El código del cupón ya existe');
        END IF;
        
        IF p_tipo_descuento NOT IN ('PORCENTAJE', 'MONTO') THEN
            RAISE_APPLICATION_ERROR(-20002, 'Tipo de descuento inválido');
        END IF;
        
        IF p_tipo_descuento = 'PORCENTAJE' AND (p_valor_descuento <= 0 OR p_valor_descuento > 100) THEN
            RAISE_APPLICATION_ERROR(-20003, 'El porcentaje debe estar entre 1 y 100');
        END IF;
        
        IF p_valor_descuento <= 0 THEN
            RAISE_APPLICATION_ERROR(-20004, 'El valor del descuento debe ser mayor a 0');
        END IF;
        
        -- Truncar fechas para normalizar zona horaria
        v_fecha_inicio := TRUNC(NVL(p_fecha_inicio, SYSDATE));
        
        IF p_fecha_expiracion IS NOT NULL THEN
            v_fecha_expiracion := TRUNC(p_fecha_expiracion);
        ELSE
            v_fecha_expiracion := NULL;
        END IF;
        
        SELECT seq_cupones.NEXTVAL INTO p_id_cupon_out FROM dual;
        
        INSERT INTO cupones (
            id_cupon,
            codigo,
            descripcion,
            tipo_descuento,
            valor_descuento,
            usos_maximos,
            fecha_inicio,
            fecha_expiracion,
            estado,
            es_bienvenida
        ) VALUES (
            p_id_cupon_out,
            UPPER(p_codigo),
            p_descripcion,
            p_tipo_descuento,
            p_valor_descuento,
            p_usos_maximos,
            v_fecha_inicio,
            v_fecha_expiracion,
            'A',
            'N'
        );
        
    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al crear cupón: ' || SQLERRM);
    END sp_crear_cupon;

    PROCEDURE sp_validar_cupon(
        p_codigo IN VARCHAR2,
        p_id_usuario IN NUMBER,
        p_subtotal IN NUMBER,
        p_es_valido OUT NUMBER,
        p_mensaje OUT VARCHAR2,
        p_descuento OUT NUMBER,
        p_id_cupon OUT NUMBER
    ) IS
        v_cupon cupones%ROWTYPE;
        v_ya_usado NUMBER;
    BEGIN
        p_es_valido := 0;
        p_descuento := 0;

        BEGIN
            SELECT * INTO v_cupon
            FROM cupones
            WHERE UPPER(codigo) = UPPER(p_codigo)
            AND estado = 'A';
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                p_mensaje := 'Cupón no válido o inactivo';
                RETURN;
        END;

        p_id_cupon := v_cupon.id_cupon;

        SELECT COUNT(*) INTO v_ya_usado
        FROM cupones_usados
        WHERE id_cupon = v_cupon.id_cupon
        AND id_usuario = p_id_usuario;

        IF v_ya_usado > 0 THEN
            p_mensaje := 'Ya has usado este cupón anteriormente';
            RETURN;
        END IF;

        IF v_cupon.fecha_expiracion IS NOT NULL AND v_cupon.fecha_expiracion < SYSDATE THEN
            p_mensaje := 'Este cupón ha expirado';
            RETURN;
        END IF;

        IF v_cupon.usos_maximos IS NOT NULL AND v_cupon.usos_actuales >= v_cupon.usos_maximos THEN
            p_mensaje := 'Este cupón ha alcanzado el límite de usos';
            RETURN;
        END IF;

        IF v_cupon.tipo_descuento = 'PORCENTAJE' THEN
            p_descuento := ROUND((p_subtotal * v_cupon.valor_descuento) / 100, 2);
        ELSE
            p_descuento := v_cupon.valor_descuento;
        END IF;

        IF p_descuento > p_subtotal THEN
            p_descuento := p_subtotal;
        END IF;

        p_es_valido := 1;
        p_mensaje := 'Cupón válido';

    EXCEPTION
        WHEN OTHERS THEN
            p_es_valido := 0;
            p_mensaje := 'Error al validar cupón: ' || SQLERRM;
    END sp_validar_cupon;

    PROCEDURE sp_aplicar_cupon(
        p_id_pedido IN NUMBER,
        p_codigo_cupon IN VARCHAR2,
        p_id_usuario IN NUMBER
    ) IS
        v_es_valido NUMBER;
        v_mensaje VARCHAR2(200);
        v_descuento NUMBER;
        v_id_cupon NUMBER;
        v_subtotal NUMBER;
        v_nuevo_total NUMBER;
    BEGIN
        SELECT subtotal INTO v_subtotal
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        sp_validar_cupon(
            p_codigo_cupon,
            p_id_usuario,
            v_subtotal,
            v_es_valido,
            v_mensaje,
            v_descuento,
            v_id_cupon
        );

        IF v_es_valido = 0 THEN
            RAISE_APPLICATION_ERROR(-20010, v_mensaje);
        END IF;

        v_nuevo_total := v_subtotal - v_descuento;

        SELECT 
            v_nuevo_total * 1.19 + costo_envio
        INTO v_nuevo_total
        FROM pedidos
        WHERE id_pedido = p_id_pedido;

        UPDATE pedidos
        SET 
            id_cupon = v_id_cupon,
            codigo_cupon = UPPER(p_codigo_cupon),
            descuento_cupon = v_descuento,
            total = v_nuevo_total
        WHERE id_pedido = p_id_pedido;

        INSERT INTO cupones_usados (
            id_uso,
            id_cupon,
            id_usuario,
            id_pedido,
            descuento_aplicado
        ) VALUES (
            seq_cupones_usados.NEXTVAL,
            v_id_cupon,
            p_id_usuario,
            p_id_pedido,
            v_descuento
        );

        UPDATE cupones
        SET usos_actuales = usos_actuales + 1
        WHERE id_cupon = v_id_cupon;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al aplicar cupón: ' || SQLERRM);
    END sp_aplicar_cupon;

    PROCEDURE sp_obtener_todos_cupones(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                id_cupon,
                codigo,
                descripcion,
                tipo_descuento,
                valor_descuento,
                usos_maximos,
                usos_actuales,
                fecha_inicio,
                fecha_expiracion,
                estado,
                es_bienvenida,
                fecha_creacion
            FROM cupones
            ORDER BY fecha_creacion DESC;
    END sp_obtener_todos_cupones;

    PROCEDURE sp_obtener_cupones_disponibles(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                c.id_cupon,
                c.codigo,
                c.descripcion,
                c.tipo_descuento,
                c.valor_descuento,
                c.fecha_expiracion,
                c.es_bienvenida
            FROM cupones c
            WHERE c.estado = 'A'
            AND (c.fecha_expiracion IS NULL OR c.fecha_expiracion >= SYSDATE)
            AND (c.usos_maximos IS NULL OR c.usos_actuales < c.usos_maximos)
            AND NOT EXISTS (
                SELECT 1 FROM cupones_usados cu
                WHERE cu.id_cupon = c.id_cupon
                AND cu.id_usuario = p_id_usuario
            )
            ORDER BY c.fecha_creacion DESC;
    END sp_obtener_cupones_disponibles;

    PROCEDURE sp_desactivar_cupon(
        p_id_cupon IN NUMBER
    ) IS
    BEGIN
        UPDATE cupones
        SET estado = 'I'
        WHERE id_cupon = p_id_cupon;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20020, 'Cupón no encontrado');
        END IF;
    END sp_desactivar_cupon;

    PROCEDURE sp_obtener_historial_uso(
        p_id_cupon IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                cu.id_uso,
                cu.id_pedido,
                p.numero_pedido,
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_usuario,
                u.correo AS correo_usuario,
                cu.descuento_aplicado,
                cu.fecha_uso
            FROM cupones_usados cu
            INNER JOIN usuarios u ON cu.id_usuario = u.id_usuario
            INNER JOIN pedidos p ON cu.id_pedido = p.id_pedido
            WHERE cu.id_cupon = p_id_cupon
            ORDER BY cu.fecha_uso DESC;
    END sp_obtener_historial_uso;

END pkg_cupones;
/