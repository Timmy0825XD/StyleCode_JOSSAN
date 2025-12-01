-- ========================================
-- CLOTHIX E-COMMERCE
-- Triggers del Sistema
-- Ejecutar como: clothix_admin
-- ========================================

-- ========================================
-- AUDITORÍA Y CONTROL
-- ========================================

-- Registra cambios en precios de artículos
CREATE OR REPLACE TRIGGER trg_auditar_cambio_precio
AFTER UPDATE OF precio_base ON articulos
FOR EACH ROW
WHEN (NEW.precio_base != OLD.precio_base)
BEGIN
    INSERT INTO historial_precios (
        id_historial,
        id_articulo,
        precio_anterior,
        precio_nuevo,
        fecha_cambio
    ) VALUES (
        seq_historial_precios.NEXTVAL,
        :NEW.id_articulo,
        :OLD.precio_base,
        :NEW.precio_base,
        SYSDATE
    );
END;
/

-- ========================================
-- GESTIÓN DE INVENTARIO
-- ========================================

-- Crea o actualiza alertas cuando el stock es bajo
CREATE OR REPLACE TRIGGER trg_gestionar_alertas_stock
AFTER UPDATE OF stock ON variantes_articulo
FOR EACH ROW
WHEN (NEW.estado = 'A')
DECLARE
    v_existe_pendiente NUMBER;
BEGIN
    IF :NEW.stock <= 5 AND :NEW.stock > 0 THEN
        
        SELECT COUNT(*) INTO v_existe_pendiente
        FROM alertas_stock
        WHERE id_variante = :NEW.id_variante
          AND estado = 'Pendiente';
        
        IF v_existe_pendiente = 0 THEN
            INSERT INTO alertas_stock (
                id_alerta,
                id_variante,
                stock_actual,
                fecha_alerta,
                estado
            ) VALUES (
                seq_alertas_stock.NEXTVAL,
                :NEW.id_variante,
                :NEW.stock,
                SYSDATE,
                'Pendiente'
            );
        ELSE
            UPDATE alertas_stock
            SET stock_actual = :NEW.stock
            WHERE id_variante = :NEW.id_variante
              AND estado = 'Pendiente';
        END IF;
        
    ELSIF :NEW.stock > 5 AND :OLD.stock <= 5 THEN
        
        UPDATE alertas_stock
        SET estado = 'Resuelta',
            fecha_resolucion = SYSDATE,
            resuelto_por = 'SISTEMA_AUTO'
        WHERE id_variante = :NEW.id_variante
          AND estado = 'Pendiente';
          
    END IF;
    
END;
/

-- Valida stock disponible antes de agregar al pedido
CREATE OR REPLACE TRIGGER trg_validar_stock_detalle
BEFORE INSERT ON detalles_pedido
FOR EACH ROW
DECLARE
    v_stock_disponible NUMBER;
    v_nombre_producto VARCHAR2(30);
    v_talla VARCHAR2(3);
    v_color VARCHAR2(15);
BEGIN
    SELECT v.stock, a.nombre, v.talla, v.color
    INTO v_stock_disponible, v_nombre_producto, v_talla, v_color
    FROM variantes_articulo v
    INNER JOIN articulos a ON v.id_articulo = a.id_articulo
    WHERE v.id_variante = :NEW.id_variante;
    
    IF v_stock_disponible < :NEW.cantidad THEN
        RAISE_APPLICATION_ERROR(-20104, 
            'Stock insuficiente para ' || v_nombre_producto || 
            ' (Talla: ' || v_talla || ', Color: ' || v_color || 
            '). Disponible: ' || v_stock_disponible || 
            ', Solicitado: ' || :NEW.cantidad);
    END IF;
END;
/

-- ========================================
-- PROTECCIÓN DE DATOS
-- ========================================

-- Previene inactivación de variantes en pedidos activos
CREATE OR REPLACE TRIGGER trg_proteger_variantes_en_uso
BEFORE UPDATE OF estado ON variantes_articulo
FOR EACH ROW
WHEN (NEW.estado = 'I' AND OLD.estado = 'A')
DECLARE
    v_pedidos_activos NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_pedidos_activos
    FROM detalles_pedido dp
    INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
    WHERE dp.id_variante = :NEW.id_variante
      AND p.estado IN ('Pendiente', 'Confirmado', 'Enviado');
    
    IF v_pedidos_activos > 0 THEN
        RAISE_APPLICATION_ERROR(-20103, 
            'No se puede inactivar esta variante porque está en ' || 
            v_pedidos_activos || ' pedido(s) activo(s).');
    END IF;
END;
/

-- Bloquea modificación de detalles en pedidos no pendientes
CREATE OR REPLACE TRIGGER trg_bloquear_edicion_pedido
BEFORE UPDATE ON detalles_pedido
FOR EACH ROW
DECLARE
    v_estado_pedido VARCHAR2(15);
BEGIN
    SELECT estado INTO v_estado_pedido
    FROM pedidos
    WHERE id_pedido = :NEW.id_pedido;
    
    IF v_estado_pedido NOT IN ('Pendiente') THEN
        RAISE_APPLICATION_ERROR(-20105, 
            'No se pueden modificar los detalles de un pedido en estado "' || 
            v_estado_pedido || '". Solo se permiten cambios en pedidos Pendientes.');
    END IF;
END;
/

-- ========================================
-- NOTIFICACIONES POR EMAIL
-- ========================================

-- Email de confirmación al crear pedido
CREATE OR REPLACE TRIGGER trg_email_pedido_creado
AFTER INSERT ON pedidos
FOR EACH ROW
WHEN (NEW.estado = 'Pendiente')
DECLARE
    v_email_cliente VARCHAR2(100);
BEGIN
    SELECT correo INTO v_email_cliente
    FROM usuarios
    WHERE id_usuario = :NEW.id_usuario;
    
    INSERT INTO cola_emails (
        id_email, tipo_email, id_pedido, destinatario, asunto, estado
    ) VALUES (
        seq_cola_emails.NEXTVAL,
        'PEDIDO_CREADO',
        :NEW.id_pedido,
        v_email_cliente,
        '¡Gracias por tu pedido ' || :NEW.numero_pedido || '! 🎉',
        'Pendiente'
    );
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Email cuando se confirma el pedido
CREATE OR REPLACE TRIGGER trg_email_pedido_confirmado
AFTER UPDATE OF estado ON pedidos
FOR EACH ROW
WHEN (NEW.estado = 'Confirmado' AND OLD.estado = 'Pendiente')
DECLARE
    v_email_cliente VARCHAR2(100);
BEGIN
    SELECT correo INTO v_email_cliente
    FROM usuarios WHERE id_usuario = :NEW.id_usuario;
    
    INSERT INTO cola_emails (
        id_email, tipo_email, id_pedido, destinatario, asunto, estado
    ) VALUES (
        seq_cola_emails.NEXTVAL, 'PEDIDO_CONFIRMADO', :NEW.id_pedido,
        v_email_cliente, 'Tu pedido ' || :NEW.numero_pedido || ' ha sido confirmado ✓', 'Pendiente'
    );
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Email cuando se envía el pedido
CREATE OR REPLACE TRIGGER trg_email_pedido_enviado
AFTER UPDATE OF estado ON pedidos
FOR EACH ROW
WHEN (NEW.estado = 'Enviado' AND OLD.estado = 'Confirmado')
DECLARE
    v_email_cliente VARCHAR2(100);
BEGIN
    SELECT correo INTO v_email_cliente
    FROM usuarios WHERE id_usuario = :NEW.id_usuario;
    
    INSERT INTO cola_emails (
        id_email, tipo_email, id_pedido, destinatario, asunto, estado
    ) VALUES (
        seq_cola_emails.NEXTVAL, 'PEDIDO_ENVIADO', :NEW.id_pedido,
        v_email_cliente, 'Tu pedido ' || :NEW.numero_pedido || ' está en camino 🚚', 'Pendiente'
    );
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Email cuando se entrega el pedido
CREATE OR REPLACE TRIGGER trg_email_pedido_entregado
AFTER UPDATE OF estado ON pedidos
FOR EACH ROW
WHEN (NEW.estado = 'Entregado' AND OLD.estado = 'Enviado')
DECLARE
    v_email_cliente VARCHAR2(100);
BEGIN
    SELECT correo INTO v_email_cliente
    FROM usuarios WHERE id_usuario = :NEW.id_usuario;
    
    INSERT INTO cola_emails (
        id_email, tipo_email, id_pedido, destinatario, asunto, estado
    ) VALUES (
        seq_cola_emails.NEXTVAL, 'PEDIDO_ENTREGADO', :NEW.id_pedido,
        v_email_cliente, '¡Tu pedido ' || :NEW.numero_pedido || ' ha sido entregado! 📦', 'Pendiente'
    );
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Email cuando se cancela el pedido
CREATE OR REPLACE TRIGGER trg_email_pedido_cancelado
AFTER UPDATE OF estado ON pedidos
FOR EACH ROW
WHEN (NEW.estado = 'Cancelado' AND OLD.estado IN ('Pendiente', 'Confirmado'))
DECLARE
    v_email_cliente VARCHAR2(100);
BEGIN
    SELECT correo INTO v_email_cliente
    FROM usuarios WHERE id_usuario = :NEW.id_usuario;
    
    INSERT INTO cola_emails (
        id_email, tipo_email, id_pedido, destinatario, asunto, estado
    ) VALUES (
        seq_cola_emails.NEXTVAL, 'PEDIDO_CANCELADO', :NEW.id_pedido,
        v_email_cliente, 'Tu pedido ' || :NEW.numero_pedido || ' ha sido cancelado', 'Pendiente'
    );
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Email de bienvenida con cupón para nuevos usuarios
CREATE OR REPLACE TRIGGER trg_cupon_bienvenida
AFTER INSERT ON usuarios
FOR EACH ROW
DECLARE
    v_codigo_bienvenida VARCHAR2(20);
    v_existe_cola NUMBER;
BEGIN
    IF :NEW.id_rol = 2 THEN
        
        BEGIN
            SELECT codigo INTO v_codigo_bienvenida
            FROM cupones
            WHERE es_bienvenida = 'S'
            AND estado = 'A'
            AND ROWNUM = 1;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RETURN;
        END;
        
        SELECT COUNT(*) INTO v_existe_cola
        FROM cola_emails
        WHERE tipo_email = 'CUPON_BIENVENIDA'
        AND destinatario = :NEW.correo
        AND estado = 'Pendiente';
        
        IF v_existe_cola = 0 THEN
            INSERT INTO cola_emails (
                id_email,
                tipo_email,
                id_pedido,
                destinatario,
                asunto,
                estado
            ) VALUES (
                seq_cola_emails.NEXTVAL,
                'CUPON_BIENVENIDA',
                0,
                :NEW.correo,
                '¡Bienvenido a Clothix! Tu cupón de descuento te espera 🎉',
                'Pendiente'
            );
        END IF;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        NULL;
END;
/